# VehicleEffects

VehicleEffects applies optional visual and handling effects to the vehicle the player is currently driving.

## Installation

Copy the `VehicleEffects` folder into the game's `scripts` directory so the files are located at:

```text
Grand Theft Auto V Enhanced/
└── scripts/
    └── VehicleEffects/
        ├── VehicleEffects.cs
        └── VehicleEffects.ini
```

## Controls

The default direct keys are disabled. When `ExntrcMenu` is installed, use either action in the `VehicleEffects` submenu:

- `Toggle Transparency`
- `Toggle Vehicle Flight`

Set either key in `VehicleEffects.ini` to a `System.Windows.Forms.Keys` name to enable its direct shortcut. VehicleEffects remains fully usable without the menu when direct keys are configured.

## Configuration

| Setting | Default | Description |
| --- | --- | --- |
| `ToggleKey` | `None` | Optional direct key used to toggle special-flight handling. `None` disables the shortcut. |
| `TransparencyToggleKey` | `None` | Optional direct key used to toggle vehicle transparency. `None` disables the shortcut. |

## Behavior

- Toggle Transparency makes the current vehicle fully transparent or restores its normal opacity.
- Vehicle transparency affects only the vehicle entity; the player and other occupants remain visible.
- Pressing the configured key while driving calls `ActivateSpecialFlightMode()` when the mode is inactive.
- Pressing it again calls `DeactivateSpecialFlightMode()` when the mode is active.
- A notification is shown when the player is not in a vehicle, the current vehicle is unavailable, or the requested flight-mode change is unsupported.
- ScriptHookVDotNetEnhanced only supports activation on compatible automobiles, submarine cars, bicycles, and motorcycles.
- Deactivation may have no effect on vehicles whose built-in special-flight handling cannot be removed.

## Files

- `VehicleEffects.cs` contains the vehicle effect actions.
- `VehicleEffects.ini` contains the optional direct action keys.