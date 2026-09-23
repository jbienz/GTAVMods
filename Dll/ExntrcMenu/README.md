# ExntrcMenu

ExntrcMenu displays `eXntrc's Mods`, a LemonUI-based menu containing actions discovered from the currently running mods.

## Requirements

Shared project requirements are listed in the repository README. ExntrcMenu additionally uses LemonUI 2.2 for SHVDN3, which is restored automatically from NuGet during the build.

## Build And Installation

The project requires the .NET SDK and the installed game's `ScriptHookVDotNet3.dll`. From the repository root, run:

```powershell
.\Deploy-Mods.ps1
```

The deployment script invokes the project through MSBuild's incremental build pipeline, so unchanged project inputs are not recompiled. The project copies only changed runtime outputs.

The build restores LemonUI from NuGet and deploys:

```text
Grand Theft Auto V Enhanced/
└── scripts/
    ├── LemonUI.SHVDN3.dll
    └── ExntrcMenu/
        ├── ExntrcMenu.dll
        └── ExntrcMenu.ini
```

No separate LemonUI download is required. The project restores it from NuGet and deploys the runtime assembly without copying source files, `obj`, or PDB files.

## Controls

| Input | Action |
| --- | --- |
| `F5` | Open or close the menu. Configurable through `ExntrcMenu.ini`. |
| GTA frontend navigation | Move through menu items using the keyboard or controller mappings configured by GTA. |
| GTA frontend accept | Open a submenu or activate an action. |
| GTA frontend cancel | Return to the previous menu or close the menu. |

LemonUI handles keyboard and controller navigation after the menu opens. ExntrcMenu does not define separate custom navigation bindings.

## Action Discovery

The first time the menu opens, ExntrcMenu inspects the live SHVDN script instances and caches the resulting menu for the lifetime of that script domain. A SHVDN reset creates a new menu instance and rebuilds the cache.

A mod exposes an action with standard component-model attributes:

```csharp
[Browsable(true)]
[Category("NPC Commands")]
[Description("Recruit Nearby NPCs")]
private void RecruitNearbyPeds()
{
    // The owning mod performs its normal action here.
}
```

- `Browsable(true)` marks a parameterless `void` instance method as a menu action.
- `Description` supplies the visible menu text. The method name is used when it is omitted.
- `Category` creates a separator within the mod submenu.
- Mod sections, categories, and actions are sorted alphabetically.
- Mods do not reference LemonUI or ExntrcMenu and remain standalone.

## Files

- `ExntrcMenu.cs` contains discovery, sorting, caching, invocation, and LemonUI menu construction.
- `ExntrcMenu.csproj` restores dependencies, builds the DLL, and deploys the menu and LemonUI.
- `ExntrcMenu.ini` contains the configurable menu-open key.