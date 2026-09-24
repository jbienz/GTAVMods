# FriendAndFoe

FriendAndFoe lets the player recruit nearby NPCs or animals as followers, turn nearby human NPCs hostile, and toggle crew hunting. Recruited crew members follow the player and defend against tracked hostiles and other nearby human threats, while enraged NPCs attack the player or the closest crew member.

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
| `F6` | Recruit eligible human NPCs within `RecruitRadius`. |
| `F7` | Make eligible human NPCs within `HostileRadius` hostile to the player and crew. |
| `F8` | Recruit eligible animals within `AnimalRadius`. |
| `F9` | Dismiss the crew and restore all NPCs altered by the mod. |
| `F10` | Toggle hunting of the animal models listed in `HuntedAnimals`. |

The keys can be changed in `FriendAndFoe.ini` using names recognized by `System.Windows.Forms.Keys`. An invalid value falls back to the default key.

When `ExntrcMenu` is installed, the same actions appear alphabetically under the `FriendAndFoe` submenu. FriendAndFoe remains fully usable without the menu.

## Configuration

Settings are read from `FriendAndFoe.ini` when the script loads.

| Setting | Default | Description |
| --- | ---: | --- |
| `RecruitKey` | `F6` | Recruits eligible nearby NPCs into the player's group. |
| `RecruitAnimalsKey` | `F8` | Recruits eligible nearby animals into the player's group. |
| `HostileKey` | `F7` | Makes eligible nearby NPCs hostile. |
| `ToggleHuntingKey` | `F10` | Toggles crew hunting of configured animal models. |
| `DismissKey` | `F9` | Restores all NPCs currently managed by the mod. |
| `RecruitRadius` | `9.144` | Radius in meters used by the human recruit command. The minimum effective value is `1`. |
| `HostileRadius` | `9.144` | Radius in meters used by the hostile command. The minimum effective value is `1`. |
| `AnimalRadius` | `185` | Radius in meters used by the animal recruit command. The minimum effective value is `1`. |
| `ThreatScanRadiusMeters` | `91.44` | Radius in meters in which crew members search for threats to the player. The minimum effective value is `1`. |
| `MaximumCrewSize` | `7` | Maximum number of recruited followers. Values are limited to the range `1` through `7`. |
| `CombatRefreshMilliseconds` | `1000` | Interval between combat-target updates. The minimum effective value is `250`. |
| `ArmUnarmedRecruits` | `true` | Gives an unarmed recruit the configured weapon with ammunition. |
| `RecruitWeapon` | `UpNAtomizer` | `WeaponHash` name given to unarmed recruits. Invalid values fall back to `UpNAtomizer`. |
| `IncludePolice` | `false` | Allows police officers to be selected by recruit and hostile commands. |
| `IncludeMissionPeds` | `false` | Allows mission-controlled or scripted NPCs to be selected. Enabling this may interfere with missions. |
| `HostilesIncludeCrew` | `false` | Allows the hostile command to remove current crew members and turn them against the player. |
| `HuntedAnimals` | Wild game and birds | Comma-separated GTA animal model names hunted while hunting is enabled. The default includes boar, birds, coyotes, deer, cougars, and rabbits, but excludes pets and farm animals. |

## NPC Selection

The normal recruit and hostile commands select living human NPCs. The animal recruit command selects only non-human NPCs. By default, police officers and mission entities are excluded, the player is never selected, and NPCs already tracked as crew or hostiles are not added again.

The police and mission NPC filters apply to both human and animal recruitment where relevant. Crew members ignore animals as ambient threats unless hunting is enabled. While hunting is enabled, they attack only animal models listed in `HuntedAnimals`; recruited animals are always excluded from target selection.

## Combat And Restoration

- Recruits join the player's group and remain with it until reset, death, removal, or script shutdown.
- Hostiles periodically retarget the player or the closest valid crew member.
- Crew members periodically target tracked hostiles or nearby human NPCs who are fighting or have a hostile relationship with the player.
- Hunting is disabled each time the script loads and can be toggled independently of recruitment.
- The mod records each affected NPC's relationship group, group membership, persistence, and behavior flags before changing them.
- Pressing the dismiss key or unloading the script restores surviving tracked NPCs to their recorded state.
- GTA notifications report how many NPCs were recruited, enraged, or reset.

## Files

- `FriendAndFoe.cs` contains the recruitment, hostility, combat, and restoration logic.
- `FriendAndFoe.ini` contains controls, ranges, timing, and eligibility settings.