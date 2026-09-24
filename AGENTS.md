# GTA V Mod Development Instructions

These instructions apply to all Copilot Agent work in this workspace.

Always follow the mod lifecycle and source rules below for creating,
editing, deploying, testing, or removing a mod.

## Workspace layout

This is a multi-root VS Code workspace with two workspace folders:

* `GTAVMods` — source repository and the only place where mod source is edited.
* `Grand Theft Auto V Enhanced` — live game runtime used only for deployed test copies under `scripts/<ModName>/`.

### Source vs. deployment

Runtime-compiled script mods belong under `GTAVMods/Script/<ModName>/`.

Compiled DLL mods belong under `GTAVMods/Dll/<ModName>/`.

`Grand Theft Auto V Enhanced/scripts/` contains deployment output only. Do not edit deployed files there because a later deployment can replace or remove them.

## Creating or modifying a mod

1. Make source changes only under `GTAVMods/Script/<ModName>/` or `GTAVMods/Dll/<ModName>/`, according to the mod type.
2. Create or update the README.md in that source folder and ensure it stays current as the mod evolves.
3. Deploy changed mods for testing by running `./Deploy-Mods.ps1` from the repository root. Pass `-GameRoot` when the game is installed somewhere other than the script's default path.
4. Keep shared project requirements in README.md at the root of the repo. Do not repeat them in mod-specific README.md files.
5. Make sure README.md at the root of the repo includes a brief description of the mod and links to the mod-specific folder.

### Deployment behavior

`Deploy-Mods.ps1` mirrors each `Script/<ModName>/` folder to the matching game `scripts/<ModName>/` folder. It compares file content, copies only changed or new files, and removes deployed files that no longer exist in source.

For each project under `Dll/`, the deploy script runs a normal incremental .NET build. MSBuild recompiles only when project inputs have changed. The project file must deploy only required runtime artifacts such as the mod DLL, its INI file, and declared runtime dependencies. Do not deploy source, README files, `obj`, `pdb`, or other build artifacts for DLL mods. Runtime copies must use `SkipUnchangedFiles` or equivalent behavior.

## ExntrcMenu integration

All gameplay mods must remain fully usable without `ExntrcMenu` and must not reference LemonUI or the menu assembly.

Expose a menu action by decorating a normal, parameterless instance method with `[Browsable(true)]`. Use `[Description("Menu Text")]` for the visible action text and optional `[Category("Section Name")]` to place the action under a separator within the mod submenu. Keep the gameplay logic in the owning mod; introduce a parameterless wrapper when an existing method requires runtime arguments such as the player ped.

Decorate each gameplay mod class with `[Description("Mod Name")]` to set its visible submenu name. Use the class name split into words by default, such as `[Description("Special Weapons")]` for `SpecialWeapons`. ExntrcMenu falls back to the raw class name when the attribute is absent or blank.

When changing menu actions:

1. Add an attributed method for every new player-triggerable action that should appear in the menu.
2. Keep mod sections alphabetical. `ExntrcMenu` performs this sorting during discovery.
3. Keep actions alphabetical within each category. `ExntrcMenu` performs this sorting during discovery.
4. Remove the attributes or method when an action is removed so its menu item disappears after the next SHVDN reset.
5. Remove all attributed actions when a mod is removed. A mod with no attributed actions does not receive a menu section.
6. Do not add registration calls, pending-action fields, string action identifiers, or other menu-owned state to gameplay mods.

`ExntrcMenu` discovers running scripts on its first open and caches the resulting menus for the lifetime of that SHVDN script domain. A SHVDN reset creates a new menu instance and rebuilds the cache.

## Validation policy

After editing a mod, use focused diagnostics or other checks directly relevant to the changed scripts.

Do not routinely run `git diff`, `git diff --check`, `git status`, file parity comparisons, repository-wide whitespace checks, or similar Git/repository inspection commands. Only run those checks when the user explicitly requests them or when a specific problem requires them for diagnosis.

## Runtime crash troubleshooting

When GTA V crashes while entering Story Mode or invoking a mod action, diagnose from runtime evidence before changing mod code:

1. Record the crash time and inspect `ScriptHookVDotNet.log`, `ScriptHookV.log`, and `asiloader.log` under the game root. Check file timestamps first so stale logs are not mistaken for the current launch.
2. Query recent Windows Application events for `.NET Runtime` event ID `1026`, `Application Error` event ID `1000`, and relevant Windows Error Reporting event ID `1001` records. The `.NET Runtime` stack is the primary source when SHVDN cannot write its log.
3. Identify the first mod method in the stack and the lowest SHVDN or GTA API call beneath it. Distinguish script compilation failures from runtime exceptions and native access violations.
4. Confirm the deployed source matches the repository source and inspect the exact installed versions and timestamps of `ScriptHookVDotNet.asi`, `ScriptHookVDotNet2.dll`, and `ScriptHookVDotNet3.dll`.
5. Reproduce compile behavior with the .NET Framework compiler against the installed `ScriptHookVDotNet3.dll`. Compile every runtime script after an SHVDN upgrade or rollback, and build DLL projects against the same game assembly.
6. Search the official SHVDN issues and discussions for the exact exception, native-memory method, GTA API member, game edition, and nightly version before introducing a compatibility workaround.
7. Form one local hypothesis from the stack, make the smallest targeted fix, rerun the focused compile or build, then deploy through `Deploy-Mods.ps1`.

Do not broadly replace supported SHVDN APIs merely because a nightly build faults inside them. Treat reproducible failures in `SHVDN.NativeMemory` or ordinary API calls as possible upstream regressions, preserve the crash evidence, and prefer upgrading, rolling back, or reporting the issue when appropriate.

## API Documentation

Develop scripts using ScriptHookVDotNetEnhanced.

Source: https://github.com/Chiheb-Bacha/ScriptHookVDotNetEnhanced
Docs: `GTAVMods/Docs/`.

## .NET language compatibility

Treat runtime-compiled `.cs` scripts as pre-C# 7 code. In particular, do not use
inline `out` variable declarations such as `out float groundHeight`; declare the
variable before the call and pass it with `out groundHeight`.

## Code Comments
Generate code comments in script code that identifies key logic and decision points. Don't comment the obvious.

Add comments to configuration ini files that describe what each setting does.