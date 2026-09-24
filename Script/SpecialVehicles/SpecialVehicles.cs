using System;
using System.ComponentModel;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using GTA.UI;

[Description("Special Vehicles")]
public sealed class SpecialVehicles : Script
{
    private const float DefaultSpawnDistanceMeters = 5.0f;

    private readonly Keys spawnKey;
    private readonly float spawnDistanceMeters;

    public SpecialVehicles()
    {
        ScriptSettings settings = ScriptSettings.Load(@"scripts\SpecialVehicles\SpecialVehicles.ini");
        spawnKey = ReadKey(settings, "SpawnKey", Keys.None);
        spawnDistanceMeters = Math.Max(2.0f, settings.GetValue("SpecialVehicles", "SpawnDistanceMeters", DefaultSpawnDistanceMeters));

        KeyDown += OnKeyDown;
    }

    private static Keys ReadKey(ScriptSettings settings, string settingName, Keys defaultKey)
    {
        string configuredKey = settings.GetValue("SpecialVehicles", settingName, defaultKey.ToString());
        Keys parsedKey;

        if (Enum.TryParse(configuredKey, true, out parsedKey))
        {
            return parsedKey;
        }

        return defaultKey;
    }

    private void OnKeyDown(object sender, KeyEventArgs eventArgs)
    {
        if (spawnKey == Keys.None || eventArgs.KeyCode != spawnKey)
        {
            return;
        }

        SpawnVehicle();
    }

    [Browsable(true)]
    [Description("Spawn Deluxo")]
    private void SpawnVehicle()
    {
        Ped player = Game.LocalPlayerPed;
        if (player == null || !player.Exists())
        {
            ShowMessage("SpecialVehicles: Player is unavailable.");
            return;
        }

        Model deluxoModel = new Model(VehicleHash.Deluxo);
        if (!deluxoModel.IsInCdImage || !deluxoModel.IsVehicle)
        {
            ShowMessage("SpecialVehicles: The DELUXO model is unavailable in this game build.");
            return;
        }

        Vector3 spawnPosition = player.Position + (player.ForwardVector * spawnDistanceMeters);
        Vehicle deluxo = Vehicle.Create(deluxoModel, spawnPosition, player.Heading);
        deluxoModel.MarkAsNoLongerNeeded();

        if (deluxo == null || !deluxo.Exists())
        {
            ShowMessage("SpecialVehicles: The vehicle could not be created.");
            return;
        }

        // Let the engine settle the vehicle onto nearby terrain after it is streamed and created.
        deluxo.PlaceOnGround();
        ShowMessage("SpecialVehicles: Deluxo spawned.");
    }

    private static void ShowMessage(string message)
    {
        Notification.PostTicker(message, false, false);
    }
}