# AirStrike

AirStrike calls a line of explosions along the ground in front of the player. The strike begins at the player's current position and facing direction when activated, then places each explosion at an evenly spaced point in the configured range.

## Installation

Copy the `AirStrike` folder into the game's `scripts` directory so the files are located at:

```text
Grand Theft Auto V Enhanced/
└── scripts/
    └── AirStrike/
        ├── AirStrike.cs
        └── AirStrike.ini
```

## Controls

The default direct key is disabled. When `ExntrcMenu` is installed, use `AirStrike > Call Air Strike`.

Set `ActivationKey` in `AirStrike.ini` to any `System.Windows.Forms.Keys` name to enable a direct shortcut. AirStrike remains fully usable without the menu when a direct key is configured.

Activating the mod during an active strike restarts the sequence from the player's new position and facing direction.

## Configuration

Settings are read from `AirStrike.ini` when the script loads.

| Setting | Default | Description |
| --- | ---: | --- |
| `ActivationKey` | `None` | Optional direct key used to call an air strike. `None` disables the shortcut. |
| `ExplosionCount` | `6` | Number of explosions in one strike. Values below `1` are raised to `1`. |
| `FirstExplosionDistanceMeters` | `9.144` | Distance in meters from the player to the first explosion. Values below `0` are raised to `0`. |
| `TotalDistanceMeters` | `27.432` | Distance in meters from the player to the final explosion. This cannot be less than the first-explosion distance. |
| `ExplosionDelayMilliseconds` | `1000` | Delay between explosions. Set to `0` to create them without an intentional delay. |

## Behavior

- Explosion positions are distributed evenly between the first and final configured distances.
- Distances are configured directly in game-world meters.
- The player's horizontal facing direction is captured when the strike begins, so turning afterward does not redirect an active strike.
- Each target position is adjusted to the detected ground height when ground detection succeeds.

## Files

- `AirStrike.cs` contains the mod logic.
- `AirStrike.ini` contains the configurable strike settings.