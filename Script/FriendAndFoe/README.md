# FriendAndFoe

FriendAndFoe lets the player recruit nearby NPCs as followers or turn nearby NPCs hostile. Recruited crew members follow the player and defend against tracked hostiles and other nearby threats, while enraged NPCs attack the player or the closest crew member.

## Installation

Copy the `FriendAndFoe` folder into the game's `scripts` directory so the files are located at:

```text
Grand Theft Auto V Enhanced/
└── scripts/
    └── FriendAndFoe/
        ├── FriendAndFoe.cs
        └── FriendAndFoe.ini
```

## Controls

| Default key | Action |
| --- | --- |
| `F6` | Recruit eligible NPCs within the configured command radius. |
| `F7` | Make eligible NPCs within the configured command radius hostile to the player and crew. |
| `F9` | Dismiss the crew and restore all NPCs altered by the mod. |

The keys can be changed in `FriendAndFoe.ini` using names recognized by `System.Windows.Forms.Keys`. An invalid value falls back to the default key.

When `ExntrcMenu` is installed, the same actions appear alphabetically under the `FriendAndFoe` submenu. FriendAndFoe remains fully usable without the menu.

## Configuration

Settings are read from `FriendAndFoe.ini` when the script loads.

| Setting | Default | Description |
| --- | ---: | --- |
| `RecruitKey` | `F6` | Recruits eligible nearby NPCs into the player's group. |
| `HostileKey` | `F7` | Makes eligible nearby NPCs hostile. |
| `DismissKey` | `F9` | Restores all NPCs currently managed by the mod. |
| `RadiusMeters` | `9.144` | Radius in meters used by the recruit and hostile commands. The minimum effective value is `1`. |
| `ThreatScanRadiusMeters` | `91.44` | Radius in meters in which crew members search for threats to the player. The minimum effective value is `1`. |
| `MaximumCrewSize` | `7` | Maximum number of recruited followers. Values are limited to the range `1` through `7`. |
| `CombatRefreshMilliseconds` | `1000` | Interval between combat-target updates. The minimum effective value is `250`. |
| `ArmUnarmedRecruits` | `true` | Gives an unarmed recruit the configured weapon with ammunition. |
| `RecruitWeapon` | `UpNAtomizer` | `WeaponHash` name given to unarmed recruits. Invalid values fall back to `UpNAtomizer`. |
| `IncludePolice` | `false` | Allows police officers to be selected by recruit and hostile commands. |
| `IncludeMissionPeds` | `false` | Allows mission-controlled or scripted NPCs to be selected. Enabling this may interfere with missions. |
| `IncludeAnimals` | `false` | Allows non-human NPCs to be selected. |
| `HostilesIncludeCrew` | `false` | Allows the hostile command to remove current crew members and turn them against the player. |

## NPC Selection

By default, the mod affects living human NPCs that are not police officers or mission entities. The player is never selected. NPCs already tracked as crew or hostiles are not added to the same list again.

The police, mission NPC, and animal filters only control selection by the recruit and hostile commands. Crew members can still defend the player against an NPC who becomes a threat through normal gameplay.

## Combat And Restoration

- Recruits join the player's group and remain with it until reset, death, removal, or script shutdown.
- Hostiles periodically retarget the player or the closest valid crew member.
- Crew members periodically target tracked hostiles or nearby NPCs who are fighting or have a hostile relationship with the player.
- The mod records each affected NPC's relationship group, group membership, persistence, and behavior flags before changing them.
- Pressing the dismiss key or unloading the script restores surviving tracked NPCs to their recorded state.
- GTA notifications report how many NPCs were recruited, enraged, or reset.

## Files

- `FriendAndFoe.cs` contains the recruitment, hostility, combat, and restoration logic.
- `FriendAndFoe.ini` contains controls, ranges, timing, and eligibility settings.