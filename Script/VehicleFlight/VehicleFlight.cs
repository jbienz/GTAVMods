using System;
using System.ComponentModel;
using System.Windows.Forms;
using GTA;
using GTA.UI;

[Description("Vehicle Flight")]
public sealed class VehicleFlight : Script
{
    private readonly Keys toggleKey;

    public VehicleFlight()
    {
        ScriptSettings settings = ScriptSettings.Load(@"scripts\VehicleFlight\VehicleFlight.ini");
        toggleKey = ReadKey(settings, "ToggleKey", Keys.None);

        KeyDown += OnKeyDown;
    }

    private static Keys ReadKey(ScriptSettings settings, string settingName, Keys defaultKey)
    {
        string configuredKey = settings.GetValue("VehicleFlight", settingName, defaultKey.ToString());
        Keys parsedKey;

        if (Enum.TryParse(configuredKey, true, out parsedKey))
        {
            return parsedKey;
        }

        return defaultKey;
    }

    private void OnKeyDown(object sender, KeyEventArgs eventArgs)
    {
        if (toggleKey == Keys.None || eventArgs.KeyCode != toggleKey)
        {
            return;
        }

        ToggleVehicleFlight();
    }

    [Browsable(true)]
    [Description("Toggle Vehicle Flight")]
    private void ToggleVehicleFlight()
    {
        Ped player = Game.LocalPlayerPed;
        if (player == null || !player.Exists() || !player.IsInVehicle())
        {
            ShowMessage("VehicleFlight: Enter a vehicle first.");
            return;
        }

        Vehicle vehicle = player.CurrentVehicle;
        if (vehicle == null || !vehicle.Exists())
        {
            ShowMessage("VehicleFlight: Current vehicle is unavailable.");
            return;
        }

        bool wasActive = vehicle.IsSpecialFlightModeActivated();
        bool changed = wasActive
            ? vehicle.DeactivateSpecialFlightMode()
            : vehicle.ActivateSpecialFlightMode();

        if (!changed)
        {
            ShowMessage("VehicleFlight: This vehicle does not support the requested flight-mode change.");
            return;
        }

        ShowMessage(wasActive
            ? "VehicleFlight: Special flight mode disabled."
            : "VehicleFlight: Special flight mode enabled.");
    }

    private static void ShowMessage(string message)
    {
        Notification.PostTicker(message, false, false);
    }
}