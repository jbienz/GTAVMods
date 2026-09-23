# GTA V Mod Development Instructions

These instructions apply to all Copilot Agent work in this workspace.

Always follow the mod lifecycle and source rules below for creating,
editing, deploying, testing, or removing a mod.

## Workspace layout

This is a multi-root VS Code workspace with two workspace folders:

* `Grand Theft Auto V Enhanced` — live game runtime. Mods under development start under `Grand Theft Auto V Enhanced/scripts/<ModName>/`.
* `GTAVMods` — source repository. All finished mod code belongs under `GTAVMods/Mods/<ModName>/`.

### Source vs. deployment

`GTAVMods/Mods/` is the source tree connected to Github.

`Grand Theft Auto V Enhanced/scripts/` is the development and testing folder used by the running game.

## Creating or modifying a mod

1. Make source changes only under `Grand Theft Auto V Enhanced/scripts/<ModName>/`.
2. Backup the final scripts to `GTAVMods/Mods/<ModName>/`.
3. Create or update a README.md for the mod at `GTAVMods/Mods/<ModName>/` and ensure it stays current as the mod evolves.
4. Keep shared project requirements in README.md at the root of the repo. Do not repeat them in mod-specific README.md files.
5. Make sure README.md at the root of the repo includes a brief description of the mod and links to the mod-specific folder.

## Validation policy

After editing a mod, use focused diagnostics or other checks directly relevant to the changed scripts.

Do not routinely run `git diff`, `git diff --check`, `git status`, file parity comparisons, repository-wide whitespace checks, or similar Git/repository inspection commands. Only run those checks when the user explicitly requests them or when a specific problem requires them for diagnosis.

The required backup from `Grand Theft Auto V Enhanced/scripts/<ModName>/` to `GTAVMods/Mods/<ModName>/` does not require a subsequent byte-for-byte comparison unless requested.

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