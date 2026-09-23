# SpawnDeluxo

SpawnDeluxo attempts to stream and create the GTA Online Deluxo vehicle ahead of the player in Story Mode.

## Installation

Copy the `SpawnDeluxo` folder into the game's `scripts` directory so the files are located at:

```text
Grand Theft Auto V Enhanced/
└── scripts/
    └── SpawnDeluxo/
        ├── SpawnDeluxo.cs
        └── SpawnDeluxo.ini
```

## Controls

| Key | Action |
| --- | --- |
| `F11` | Spawn a Deluxo ahead of the player. |

The key can be changed through `SpawnDeluxo.ini`.

## Configuration

| Setting | Default | Description |
| --- | ---: | --- |
| `SpawnKey` | `F11` | Key used to spawn the Deluxo. |
| `SpawnDistanceMeters` | `5` | Distance ahead of the player where the vehicle is created. Values below `2` are raised to `2`. |

## Behavior

- The script checks that the `DELUXO` model exists and is recognized as a vehicle before requesting it.
- The Deluxo is created in the direction the player is facing and placed on nearby ground.
- The streamed model is released after the creation attempt.
- A notification reports whether the vehicle spawned or why the attempt failed.

## Files

- `SpawnDeluxo.cs` contains the model validation and vehicle-spawning logic.
- `SpawnDeluxo.ini` contains the configurable spawn key and distance.