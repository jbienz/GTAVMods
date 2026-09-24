using System;
using System.ComponentModel;
using System.Windows.Forms;
using GTA;
using GTA.UI;

[Description("Vehicle Effects")]
public sealed class VehicleEffects : Script
{
    private readonly Keys toggleKey;
    private readonly Keys transparencyToggleKey;

    public VehicleEffects()
    {
        ScriptSettings settings = ScriptSettings.Load(@"scripts\VehicleEffects\VehicleEffects.ini");
        toggleKey = ReadKey(settings, "ToggleKey", Keys.None);
        transparencyToggleKey = ReadKey(settings, "TransparencyToggleKey", Keys.None);

        KeyDown += OnKeyDown;
    }

    private static Keys ReadKey(ScriptSettings settings, string settingName, Keys defaultKey)
    {
        string configuredKey = settings.GetValue("VehicleEffects", settingName, defaultKey.ToString());
        Keys parsedKey;

        if (Enum.TryParse(configuredKey, true, out parsedKey))
        {
            return parsedKey;
        }

        return defaultKey;
    }

    private void OnKeyDown(object sender, KeyEventArgs eventArgs)
    {
        if (toggleKey != Keys.None && eventArgs.KeyCode == toggleKey)
        {
            ToggleVehicleFlight();
        }

        if (transparencyToggleKey != Keys.None && eventArgs.KeyCode == transparencyToggleKey)
        {
            ToggleTransparency();
        }
    }

    [Browsable(true)]
    [Description("Toggle Transparency")]
    private void ToggleTransparency()
    {
        Ped player = Game.LocalPlayerPed;
        if (player == null || !player.Exists() || !player.IsInVehicle())
        {
            ShowMessage("VehicleEffects: Enter a vehicle first.");
            return;
        }

        Vehicle vehicle = player.CurrentVehicle;
        if (vehicle == null || !vehicle.Exists())
        {
            ShowMessage("VehicleEffects: Current vehicle is unavailable.");
            return;
        }

        bool wasTransparent = vehicle.Opacity < 255;

        // Opacity is applied to the vehicle entity only, leaving its occupants visible.
        if (wasTransparent)
        {
            vehicle.ResetOpacity();
        }
        else
        {
            vehicle.SetOpacity(0, true);
        }

        ShowMessage(wasTransparent
            ? "VehicleEffects: Vehicle transparency disabled."
            : "VehicleEffects: Vehicle transparency enabled.");
    }

    [Browsable(true)]
    [Description("Toggle Vehicle Flight")]
    private void ToggleVehicleFlight()
    {
        Ped player = Game.LocalPlayerPed;
        if (player == null || !player.Exists() || !player.IsInVehicle())
        {
            ShowMessage("VehicleEffects: Enter a vehicle first.");
            return;
        }

        Vehicle vehicle = player.CurrentVehicle;
        if (vehicle == null || !vehicle.Exists())
        {
            ShowMessage("VehicleEffects: Current vehicle is unavailable.");
            return;
        }

        bool wasActive = vehicle.IsSpecialFlightModeActivated();
        bool changed = wasActive
            ? vehicle.DeactivateSpecialFlightMode()
            : vehicle.ActivateSpecialFlightMode();

        if (!changed)
        {
            ShowMessage("VehicleEffects: This vehicle does not support the requested flight-mode change.");
            return;
        }

        ShowMessage(wasActive
            ? "VehicleEffects: Special flight mode disabled."
            : "VehicleEffects: Special flight mode enabled.");
    }

    private static void ShowMessage(string message)
    {
        Notification.PostTicker(message, false, false);
    }
}