using System;
using System.ComponentModel;
using System.Windows.Forms;
using GTA;
using GTA.Math;

[Description("Air Strike")]
public sealed class AirStrike : Script
{
    private const int DefaultExplosionCount = 6;
    private const float DefaultFirstExplosionDistanceMeters = 9.144f;
    private const float DefaultTotalDistanceMeters = 27.432f;
    private const int DefaultExplosionDelayMilliseconds = 1000;

    private readonly Keys activationKey;
    private readonly int explosionCount;
    private readonly float firstExplosionDistanceMeters;
    private readonly float totalDistanceMeters;
    private readonly int explosionDelayMilliseconds;

    private bool strikeActive;
    private int nextExplosionIndex;
    private int nextExplosionTime;
    private Vector3 strikeOrigin;
    private Vector3 strikeForward;
    private Ped strikeOwner;

    public AirStrike()
    {
        ScriptSettings settings = ScriptSettings.Load(@"scripts\AirStrike\AirStrike.ini");

        activationKey = ReadKey(settings, "ActivationKey", Keys.None);
        explosionCount = Math.Max(1, settings.GetValue("AirStrike", "ExplosionCount", DefaultExplosionCount));
        firstExplosionDistanceMeters = Math.Max(0.0f, settings.GetValue("AirStrike", "FirstExplosionDistanceMeters", DefaultFirstExplosionDistanceMeters));
        totalDistanceMeters = Math.Max(1.0f, settings.GetValue("AirStrike", "TotalDistanceMeters", DefaultTotalDistanceMeters));
        totalDistanceMeters = Math.Max(firstExplosionDistanceMeters, totalDistanceMeters);
        explosionDelayMilliseconds = Math.Max(0, settings.GetValue("AirStrike", "ExplosionDelayMilliseconds", DefaultExplosionDelayMilliseconds));

        KeyDown += OnKeyDown;
        Tick += OnTick;
    }

    private static Keys ReadKey(ScriptSettings settings, string settingName, Keys defaultKey)
    {
        string configuredKey = settings.GetValue("AirStrike", settingName, defaultKey.ToString());
        Keys parsedKey;

        if (Enum.TryParse(configuredKey, true, out parsedKey))
        {
            return parsedKey;
        }

        return defaultKey;
    }

    private void OnKeyDown(object sender, KeyEventArgs eventArgs)
    {
        if (activationKey == Keys.None || eventArgs.KeyCode != activationKey)
        {
            return;
        }

        StartAirStrike();
    }

    private void OnTick(object sender, EventArgs eventArgs)
    {
        if (!strikeActive || Game.GameTime < nextExplosionTime)
        {
            return;
        }

        CreateExplosion(nextExplosionIndex);
        nextExplosionIndex++;

        if (nextExplosionIndex >= explosionCount)
        {
            strikeActive = false;
            return;
        }

        nextExplosionTime = Game.GameTime + explosionDelayMilliseconds;
    }

    [Browsable(true)]
    [Description("Call Air Strike")]
    private void StartAirStrike()
    {
        Ped player = Game.LocalPlayerPed;
        if (player == null || !player.Exists())
        {
            return;
        }

        strikeOrigin = player.Position;
        strikeForward = player.ForwardVector;
        strikeForward.Z = 0.0f;
        strikeForward.Normalize();
        strikeOwner = player;
        nextExplosionIndex = 0;
        nextExplosionTime = Game.GameTime;
        strikeActive = true;
    }

    private void CreateExplosion(int index)
    {
        float distance = firstExplosionDistanceMeters;
        if (explosionCount > 1)
        {
            distance += (totalDistanceMeters - firstExplosionDistanceMeters) * index / (explosionCount - 1);
        }

        Vector3 target = strikeOrigin + (strikeForward * distance);
        Vector3 groundProbe = new Vector3(target.X, target.Y, strikeOrigin.Z + 100.0f);
        float groundHeight;

        if (World.GetGroundHeight(groundProbe, out groundHeight, GetGroundHeightMode.Normal))
        {
            target.Z = groundHeight;
        }

        Ped owner = strikeOwner;
        if (owner != null && !owner.Exists())
        {
            owner = null;
        }

        World.AddExplosion(target, ExplosionType.Grenade, 1.0f, 0.3f, owner, true, false);
    }
}