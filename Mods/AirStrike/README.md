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

| Key | Action |
| --- | --- |
| `F8` | Start an air strike in the direction the player is facing. |

The activation key is currently fixed in the script. Activating the mod during an active strike restarts the sequence from the player's new position and facing direction.

## Configuration

Settings are read from `AirStrike.ini` when the script loads.

| Setting | Default | Description |
| --- | ---: | --- |
| `ExplosionCount` | `6` | Number of explosions in one strike. Values below `1` are raised to `1`. |
| `FirstExplosionDistanceYards` | `10` | Distance from the player to the first explosion. Values below `0` are raised to `0`. |
| `TotalDistanceYards` | `30` | Distance from the player to the final explosion. This cannot be less than the first-explosion distance. |
| `ExplosionDelayMilliseconds` | `1000` | Delay between explosions. Set to `0` to create them without an intentional delay. |

## Behavior

- Explosion positions are distributed evenly between the first and final configured distances.
- Distances are configured in yards and converted to game-world meters by the script.
- The player's horizontal facing direction is captured when the strike begins, so turning afterward does not redirect an active strike.
- Each target position is adjusted to the detected ground height when ground detection succeeds.

## Files

- `AirStrike.cs` contains the mod logic.
- `AirStrike.ini` contains the configurable strike settings.