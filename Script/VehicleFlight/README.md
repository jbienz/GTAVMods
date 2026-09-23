# VehicleFlight

VehicleFlight toggles ScriptHookVDotNetEnhanced special-flight handling on the vehicle the player is currently driving.

## Installation

Copy the `VehicleFlight` folder into the game's `scripts` directory so the files are located at:

```text
Grand Theft Auto V Enhanced/
└── scripts/
    └── VehicleFlight/
        ├── VehicleFlight.cs
        └── VehicleFlight.ini
```

## Controls

The default direct key is disabled. When `ExntrcMenu` is installed, use `VehicleFlight > Toggle Vehicle Flight`.

Set `ToggleKey` in `VehicleFlight.ini` to any `System.Windows.Forms.Keys` name to enable a direct shortcut. VehicleFlight remains fully usable without the menu when a direct key is configured.

## Configuration

| Setting | Default | Description |
| --- | --- | --- |
| `ToggleKey` | `None` | Optional direct key used to toggle special-flight handling. `None` disables the shortcut. |

## Behavior

- Pressing the configured key while driving calls `ActivateSpecialFlightMode()` when the mode is inactive.
- Pressing it again calls `DeactivateSpecialFlightMode()` when the mode is active.
- A notification is shown when the player is not in a vehicle or the requested mode change is unsupported.
- ScriptHookVDotNetEnhanced only supports activation on compatible automobiles, submarine cars, bicycles, and motorcycles.
- Deactivation may have no effect on vehicles whose built-in special-flight handling cannot be removed.

## Files

- `VehicleFlight.cs` contains the flight-mode toggle logic.
- `VehicleFlight.ini` contains the configurable activation key.