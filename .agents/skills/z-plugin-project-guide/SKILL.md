---
name: z-plugin-project-guide
description: Project-specific architecture and workflow guide for the Z_Plugin Unity repository. Use when Codex works in Z_Plugin on Unity C# code, asmdef boundaries, GameSample runtime or UGC behavior, Excel2Cs Forms, story persistence, LabForm labels, assets, generated UI, map and collision logic, event commands, or build validation. Do not use for unrelated Unity repositories.
---

# Z Plugin Project Guide

Work from the repository's actual source-of-truth files while preserving its layered architecture, generated-code boundaries, runtime protocols, and dirty worktree.

## Establish context

1. Locate the project root by finding both `ProjectSettings/ProjectVersion.txt` and `Assets/GameSample`.
2. Read `AGENTS.md` and `ProjectSettings/ProjectVersion.txt` before version-sensitive work. Treat the active GameObject in `AGENTS.md` as transient context, not a permanent project fact.
3. Run `git status --short` before editing. Preserve unrelated changes, generated churn, Unity settings, IDE files, and user assets.
4. Optionally run the read-only audit:

   ```powershell
   powershell -NoProfile -ExecutionPolicy Bypass -File `
     "<this-skill-dir>/scripts/audit-project.ps1" -ProjectRoot "<project-root>"
   ```

5. Identify the target's real assembly, source of truth, lifecycle owner, persistence path, and validation surface before changing it.

## Load only the relevant references

- Read [architecture.md](references/architecture.md) when locating a module, changing dependencies, managers, scenes, units, commands, or platform-facing code.
- Read [event-language.md](references/event-language.md) before changing the compiler, interpreter, event code generation, command names, ProgramData, or persisted ef.
- Read [forms-persistence-assets.md](references/forms-persistence-assets.md) before touching `.xls`, `ExcelCs`, Form inheritance, IDs, JSON, saves, LabForm, or assets.
- Read [ui-runtime-workflows.md](references/ui-runtime-workflows.md) before touching UiHolder-generated UI, Game/Mod/Play flows, controllers, or global events.
- Read [map-collision.md](references/map-collision.md) before changing map movement, collision, trigger, navigation, tile association, or map resource behavior.
- Read [constraints-validation.md](references/constraints-validation.md) before final verification, player-build work, package changes, or third-party plugin changes.

## Enforce the non-negotiable boundaries

- Do not hand-edit `ExcelCs/*Form.cs`; edit the matching Excel source and regenerate.
- Do not put business logic in `GameSample/UiBase`; extend generated UI with partial controllers under `GameSample/MyScripts`.
- Do not edit Unity-generated `.csproj` or `.sln` as deliverables. Use `.asmdef` files as the dependency authority.
- Do not make a lower asmdef reference `Assembly-CSharp`. Check the physical asmdef boundary rather than trusting the `Z_Level` folder or namespace.
- Do not replace logical story-scene loading with Unity `SceneManager` loading.
- Preserve every existing `.meta` GUID. Pair every new Unity asset or script with a unique `.meta`.
- Preserve binary `.xls` format, workbook layout, styles, formulas, and lock-file hygiene.
- Treat save/load as an explicit allow-list. A new Form is not persistent until save, load, reset, ordering, and migration are handled deliberately.
- Treat `labId=0` as unclassified and nullable UI state as “all”. Use concrete `belong` Form names for Lab identities.
- Use existing Manager, Controller, Unit, Map, UI, Asset, and event entry points instead of bypassing them with direct long-lived Unity objects.
- Keep runtime code free of new `UnityEditor` dependencies. `GameSample/MyScripts/Mod` is a runtime UGC editor.
- Preserve reserved resource names such as `MapPrefab$...`, `runtime$...`, `$i$...$i$`, `$a$...$a$`, and `$v$...$v$`.

## Execute changes

1. Search with `rg` for the declaration, generated API, callers, data source, and sibling implementations.
2. Inspect the smallest relevant dependency chain and reference file; avoid broad rediscovery or unrelated refactors.
3. Modify the authoritative source and any required partial/controller code.
4. If a schema or serialized key changes, migrate raw JSON before generated deserialization and verify old saves.
5. If a global table or manager changes, verify initialization, reset, unload, and consecutive-story behavior.
6. If a listener registers globally, verify its matching lifetime and unregistration strategy.
7. If a resource or prefab name changes, inspect every parser and lookup using that string contract.

## Validate proportionally

1. Review scoped diffs and run `git diff --check` on hand-written files. Do not normalize unrelated generated formatting.
2. Regenerate only the affected Form target when possible, then inspect all generated diffs and orphan outputs.
3. Let Unity refresh project files before relying on root `.csproj` builds, especially after adding files.
4. Build the touched asmdef project and `Assembly-CSharp.csproj` where available. Treat Unity reimport and a Player build as more authoritative than stale generated projects.
5. Run the task-specific manual checks listed in the relevant reference. Use `Assets/GameSample/Game.unity` for Mod/Play smoke tests.
6. Report any validation that could not be run. Editor compilation alone does not prove an IL2CPP Player build.

## Keep this guide current

When a task intentionally changes a stable module boundary, generator contract, save protocol, reserved identifier, or known limitation, update the affected repository reference in the same change. Keep unrelated Skill edits out of ordinary implementation tasks.
