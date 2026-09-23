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

| Key | Action |
| --- | --- |
| `F10` | Enable or disable special-flight handling on the current vehicle. |

The key can be changed through `VehicleFlight.ini`.

## Configuration

| Setting | Default | Description |
| --- | --- | --- |
| `ToggleKey` | `F10` | Key used to toggle special-flight handling. |

## Behavior

- Pressing the configured key while driving calls `ActivateSpecialFlightMode()` when the mode is inactive.
- Pressing it again calls `DeactivateSpecialFlightMode()` when the mode is active.
- A notification is shown when the player is not in a vehicle or the requested mode change is unsupported.
- ScriptHookVDotNetEnhanced only supports activation on compatible automobiles, submarine cars, bicycles, and motorcycles.
- Deactivation may have no effect on vehicles whose built-in special-flight handling cannot be removed.

## Files

- `VehicleFlight.cs` contains the flight-mode toggle logic.
- `VehicleFlight.ini` contains the configurable activation key.