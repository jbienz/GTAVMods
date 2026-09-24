using System;
using System.ComponentModel;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using GTA.Native;
using GTA.UI;

[Description("Special Weapons")]
public sealed class SpecialWeapons : Script
{
    private const int DefaultFireworkIntervalMilliseconds = 200;
    private const float FireworkRangeMeters = 365.0f;

    private readonly Keys americaKey;
    private readonly int fireworkIntervalMilliseconds;
    private readonly WeaponAsset fireworkAsset;

    private bool americaEnabled;
    private int nextFireworkTime;

    public SpecialWeapons()
    {
        ScriptSettings settings = ScriptSettings.Load(@"scripts\SpecialWeapons\SpecialWeapons.ini");
        americaKey = ReadKey(settings, "AmericaKey", Keys.None);
        fireworkIntervalMilliseconds = Math.Max(100, settings.GetValue("SpecialWeapons", "FireworkIntervalMilliseconds", DefaultFireworkIntervalMilliseconds));
        fireworkAsset = new WeaponAsset(WeaponHash.Firework);

        KeyDown += OnKeyDown;
        Tick += OnTick;
        Aborted += OnAborted;
    }

    private static Keys ReadKey(ScriptSettings settings, string settingName, Keys defaultKey)
    {
        string configuredKey = settings.GetValue("SpecialWeapons", settingName, defaultKey.ToString());
        Keys parsedKey;

        if (Enum.TryParse(configuredKey, true, out parsedKey))
        {
            return parsedKey;
        }

        return defaultKey;
    }

    private void OnKeyDown(object sender, KeyEventArgs eventArgs)
    {
        if (americaKey == Keys.None || eventArgs.KeyCode != americaKey)
        {
            return;
        }

        America();
    }

    private void OnTick(object sender, EventArgs eventArgs)
    {
        if (!americaEnabled)
        {
            return;
        }

        Ped player = Game.LocalPlayerPed;
        if (player == null || !player.Exists() || player.Weapons.Current.Hash != WeaponHash.Minigun)
        {
            return;
        }

        if (Game.GameTime < nextFireworkTime)
        {
            return;
        }

        if (!Function.Call<bool>(Hash.IS_PED_SHOOTING, player.Handle))
        {
            return;
        }

        if (!fireworkAsset.IsLoaded)
        {
            fireworkAsset.Request();
            return;
        }

        Vector3 aimDirection = GameplayCamera.Direction;
        Vector3 sourcePosition = GetFireworkSourcePosition(player, aimDirection);
        Vector3 targetPosition = GameplayCamera.Position + (aimDirection * FireworkRangeMeters);
        World.ShootSingleBullet(sourcePosition, targetPosition, 250, fireworkAsset, player, true, true, true, -1.0f);

        nextFireworkTime = Game.GameTime + fireworkIntervalMilliseconds;
    }

    private static Vector3 GetFireworkSourcePosition(Ped player, Vector3 aimDirection)
    {
        Prop weaponObject = player.Weapons.CurrentWeaponObject;
        if (weaponObject != null && weaponObject.Exists())
        {
            EntityBone muzzle = weaponObject.Bones["gun_muzzle"];
            if (muzzle.IsValid)
            {
                // Move just beyond the muzzle to keep the projectile clear of the weapon model.
                return muzzle.Position + (aimDirection * 0.15f);
            }

            return weaponObject.GetOffsetPosition(new Vector3(0.0f, 0.8f, 0.0f));
        }

        return player.GetOffsetPosition(new Vector3(0.2f, 1.2f, 0.6f));
    }

    [Browsable(true)]
    [Description("America!")]
    private void America()
    {
        Ped player = Game.LocalPlayerPed;
        if (player == null || !player.Exists())
        {
            ShowMessage("SpecialWeapons: Player is unavailable.");
            return;
        }

        player.Weapons.Give(WeaponHash.Minigun, 1, true, true);
        Weapon minigun = player.Weapons[WeaponHash.Minigun];
        minigun.Ammo = minigun.MaxAmmo;
        minigun.InfiniteAmmo = true;
        fireworkAsset.Request();
        americaEnabled = true;
        nextFireworkTime = Game.GameTime;

        ShowMessage("SpecialWeapons: America equipped.");
    }

    private void OnAborted(object sender, EventArgs eventArgs)
    {
        Ped player = Game.LocalPlayerPed;
        if (player != null && player.Exists() && player.Weapons.HasWeapon(WeaponHash.Minigun))
        {
            player.Weapons[WeaponHash.Minigun].InfiniteAmmo = false;
        }

        fireworkAsset.MarkAsNoLongerNeeded();
    }

    private static void ShowMessage(string message)
    {
        Notification.PostTicker(message, false, false);
    }
}