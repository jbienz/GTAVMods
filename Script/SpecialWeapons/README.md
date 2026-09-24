# SpecialWeapons

SpecialWeapons adds unusual weapon combinations for Story Mode. Its `America` action gives and equips a minigun with unlimited ammunition, then launches firework-projectile rounds along the player's aim while that minigun is firing.

GTA V fixes projectile behavior to each weapon definition, so the script cannot directly load firework ammunition into a minigun. America keeps the minigun's normal firing behavior and adds rate-limited Firework Launcher projectiles while it fires.

## Installation

Copy the `SpecialWeapons` folder into the game's `scripts` directory so the files are located at:

```text
Grand Theft Auto V Enhanced/
└── scripts/
    └── SpecialWeapons/
        ├── SpecialWeapons.cs
        └── SpecialWeapons.ini
```

## Controls

`AmericaKey` defaults to `None`. Set it to a name recognized by `System.Windows.Forms.Keys` to activate America without a menu. An invalid value falls back to `None`.

When `ExntrcMenu` is installed, `America` appears in the `SpecialWeapons` submenu. SpecialWeapons remains usable without the menu when `AmericaKey` is configured.

## Configuration

| Setting | Default | Description |
| --- | ---: | --- |
| `AmericaKey` | `None` | Optional direct key that gives and equips America. |
| `FireworkIntervalMilliseconds` | `200` | Delay between scripted firework projectiles while the minigun fires. The minimum effective value is `100`. |

## Files

- `SpecialWeapons.cs` contains the weapon grant and firework-projectile behavior.
- `SpecialWeapons.ini` contains the optional direct key and firework cadence.