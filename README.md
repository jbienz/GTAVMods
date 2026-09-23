# GTA V Enhanced Mods

A collection of mods for Grand Theft Auto V Enhanced. Runtime-compiled mods live under `Script`, while compiled projects live under `Dll`.

## Requirements

- [Grand Theft Auto V Enhanced](https://store.steampowered.com/app/3240220/Grand_Theft_Auto_V_Enhanced/)
- [ScriptHookVDotNetEnhanced](https://github.com/Chiheb-Bacha/ScriptHookVDotNetEnhanced)

`ExntrcMenu` uses [LemonUI](https://github.com/LemonUIbyLemon/LemonUI). Its project restores LemonUI 2.2 from NuGet and deploys `LemonUI.SHVDN3.dll` to the game's `scripts` directory when built; the third-party binary is not stored in this repository.

## Mods

Deploy changed files and incrementally build DLL projects with:

```powershell
.\Deploy-Mods.ps1
```

Pass `-GameRoot` when Grand Theft Auto V Enhanced is installed somewhere other than the script's default path.

### [AirStrike](Script/AirStrike)

Calls in a configurable line of explosions along the ground in front of the player. The number of explosions, strike distance, spacing, and timing can be adjusted through the included INI file.

### [ExntrcMenu](Dll/ExntrcMenu)

Provides the LemonUI-based `eXntrc's Mods` menu. It discovers attributed actions from installed mods and supports LemonUI's standard keyboard and controller navigation.

### [FriendAndFoe](Script/FriendAndFoe)

Recruits nearby NPCs into the player's crew or turns them hostile. Configuration options control the command keys, selection radius, crew size, combat updates, equipment, and which types of NPCs can be affected.

### [SpawnDeluxo](Script/SpawnDeluxo)

Attempts to stream and spawn the Deluxo vehicle ahead of the player in Story Mode.

### [VehicleFlight](Script/VehicleFlight)

Toggles ScriptHookVDotNetEnhanced special-flight handling on the vehicle the player is currently driving.
