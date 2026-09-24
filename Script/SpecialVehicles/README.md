# SpecialVehicles

SpecialVehicles contains unusual or otherwise special vehicle actions for Story Mode. Its first action attempts to stream and create the GTA Online Deluxo vehicle ahead of the player.

## Installation

Copy the `SpecialVehicles` folder into the game's `scripts` directory so the files are located at:

```text
Grand Theft Auto V Enhanced/
└── scripts/
    └── SpecialVehicles/
        ├── SpecialVehicles.cs
        └── SpecialVehicles.ini
```

## Controls

The default direct key is disabled. When `ExntrcMenu` is installed, use `Special Vehicles > Spawn Deluxo`.

Set `SpawnKey` in `SpecialVehicles.ini` to any `System.Windows.Forms.Keys` name to enable a direct shortcut. SpecialVehicles remains fully usable without the menu when a direct key is configured.

## Configuration

| Setting | Default | Description |
| --- | ---: | --- |
| `SpawnKey` | `None` | Optional direct key used to spawn the Deluxo. `None` disables the shortcut. |
| `SpawnDistanceMeters` | `5` | Distance ahead of the player where the vehicle is created. Values below `2` are raised to `2`. |

## Behavior

- The script checks that the `DELUXO` model exists and is recognized as a vehicle before requesting it.
- The Deluxo is created in the direction the player is facing and placed on nearby ground.
- The streamed model is released after the creation attempt.
- A notification reports whether the vehicle spawned or why the attempt failed.

## Files

- `SpecialVehicles.cs` contains the model validation and vehicle-spawning logic.
- `SpecialVehicles.ini` contains the configurable spawn key and distance.