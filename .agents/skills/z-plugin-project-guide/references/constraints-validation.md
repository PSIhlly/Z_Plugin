# Constraints, validation, and known risks

Use this reference before final verification, package or platform work, third-party changes, release preparation, or investigation of existing technical debt.

## Contents

- [Environment snapshot](#environment-snapshot)
- [Runtime and Editor isolation](#runtime-and-editor-isolation)
- [Repository hygiene](#repository-hygiene)
- [Validation ladder](#validation-ladder)
- [Task-specific checks](#task-specific-checks)
- [Known risk register](#known-risk-register)

## Environment snapshot

Re-read the source files instead of assuming this snapshot remains current:

- Editor: Unity/Tuanjie `2022.3.61t4` from `ProjectSettings/ProjectVersion.txt`.
- Render pipeline: URP 14.1.0 from `Packages/manifest.json`.
- UI: UGUI and TextMeshPro.
- Input: Legacy Input Manager (`activeInputHandler: 0`).
- Scripting: IL2CPP configured for Android and Standalone; code/engine stripping is enabled.
- Windows BGM playback is owned by `Z_Audio.AudioManager` through Unity `AudioSource` and `UnityWebRequestMultimedia`; it intentionally bypasses AVPro Video 2.7's Windows MediaEngine path after that native path returned `0x80070651` on Windows build 26200. AVPro remains the backend for video and non-BGM media paths.
- Packages/features: Visual Scripting, AI Navigation, Timeline, Unity Test Framework package.
- Native vendors: AVProVideo and NativeGallery under `Assets/Z_Level0/Plugins`.
- Default BGM: `Assets/StreamingAssets/Bgm.mp3`.

Consequences:

- Do not introduce New Input System APIs without a deliberate project-wide input migration.
- Treat reflection, dynamic generic construction, and runtime discovery as AOT/stripping risks.
- Preserve explicit `RuntimeInitializeOnLoadMethod` registrations unless an IL2CPP-safe replacement is validated.
- Test vendor or native-library upgrades on every affected target; Editor compilation is insufficient.
- Do not upgrade Unity, URP, packages, Android SDK settings, or vendor plugins as an incidental fix.

## Runtime and Editor isolation

Place Unity Editor code under an `Editor` directory or guard it with `#if UNITY_EDITOR`.

`GameSample/MyScripts/Mod` is a Player runtime UGC editor. Never add `UnityEditor` dependencies merely because the feature is called an editor.

The repository currently contains runtime-path files mentioning `UnityEditor`, `TreeEditor`, or related editor namespaces. Treat them as technical debt:

- Do not add new unguarded references.
- Remove unused references in files already being changed when the removal is demonstrably safe.
- Run an actual Player build before claiming release compatibility.

The read-only audit script reports runtime-path files that mention `UnityEditor`; manually distinguish guarded vendor/editor-compatible code from real Player blockers.

## Repository hygiene

- Start with `git status --short`; the worktree may already contain user and generated changes.
- Preserve unrelated `.xls`, generated Forms, Unity layouts, package locks, font assets, `.vs` files, and project settings.
- Scope diff review to the requested files and generated outputs. Do not clean a dirty tree to make validation convenient.
- Do not hand-maintain root `.csproj` or `.sln`; Unity regenerates them and ignore rules are inconsistent.
- Preserve existing line endings and encodings. Some historical generated comments are garbled and generated formatting is irregular.
- Pair Unity files with `.meta`; never reuse or regenerate an existing GUID.
- Treat `hlzy.keystore`, credentials, signing data, and private asset content as sensitive. Do not inspect, print, copy, or modify them without an explicit signing task.
- Treat `Library`, `Temp`, `Logs`, `obj`, and UserSettings as disposable/generated state, not source of truth.

## Validation ladder

Use the narrowest layer that provides meaningful evidence, then escalate with risk:

1. Static search: declarations, callers, stale APIs, namespaces, Editor references.
2. Scoped diff: authoritative source plus generated output; `git diff --check` on hand-written files.
3. Generator or deterministic script validation.
4. Touched asmdef project build.
5. `Assembly-CSharp.csproj` build after Unity has refreshed project files.
6. Unity reimport and Console review.
7. `Assets/GameSample/Game.unity` Mod/Play smoke test.
8. Target Player build, especially IL2CPP Android/Standalone.

Do not overstate evidence:

- A root `.csproj` can omit a newly created source file until Unity regenerates it.
- Editor compilation can resolve `UnityEditor` references that fail Player compilation.
- A package is installed for Unity Test Framework, but the repository currently has no formal NUnit/UnityTest suite; `Assets/Scenes/Test.cs` is an ordinary script.
- `ProjectSettings/EditorBuildSettings.asset` currently has no valid enabled scene, so configure or explicitly pass scenes before release-build automation.

Common compile commands after Unity refresh:

```powershell
dotnet build Z_DataSystem.csproj --no-restore --nologo -v:minimal
dotnet build Assembly-CSharp.csproj --no-restore --nologo -v:minimal
```

Build the specific `Z_*.csproj` first when a lower asmdef changes. Do not edit a generated project to make a failure disappear; if a temporary local adjustment is unavoidable for diagnosis, do not deliver or stage it.

## Task-specific checks

### Form/schema

- Regenerate the affected target and inspect constructor/index/API changes.
- Check for orphan generated `.cs/.meta` after workbook rename/delete.
- Load an old JSON fixture and a new round-trip fixture.
- Switch between two stories to expose global Form leakage.

### Lab

- Check unclassified `0`, nullable “all”, path display, same path under different concrete Forms, and wrong-`belong` normalization.
- Validate GameCmd static Lab IDs against command metadata.
- Run `CheckController` Lab foreign-key checks in the UGC flow.

### Assets

- Import, save, unload, reload, replace content under an existing ID, and verify concrete Story subtype/Lab ownership.
- Test texture, audio, and video independently; their storage fields and platform loaders differ.

### UI/runtime

- Show, hide, reopen, and refresh the UI twice.
- Check duplicated callbacks, stale listener responses, and missing Inspector references.
- Enter/exit both Mod and Play and load two stories consecutively.

### Map/collision

- Follow `map-collision.md` and the repository Move/Interact document; replay corner, slope, trigger, and tile-boundary cases.

### Release/platform

- Verify Build Settings, scripting backend, target architectures, stripping, permissions, native plugins, and runtime-only compilation.
- Run an IL2CPP Player build rather than relying on `dotnet build`.

## Known risk register

These are audited current conditions, not desired behavior. Re-check before acting:

1. `GameSaveController.LoadLab()` calls generated `LabForm.ClearAuto()` while `LabForm.ClearRuntimeData()` exists but is unused.
2. Generated `ClearAuto()` uses a strict `< idChain.cnt`; a dynamic ID exactly at the pool maximum can survive reset.
3. `LoadProgress()` relies on surrounding lifecycle clearing rather than clearing its Form itself.
4. `VideoController.GetSupportedExtensions()` currently reports `.mp3`, which appears inconsistent with video import behavior.
5. `TimeManager.Awake()` hides the base Awake path and deserves focused lifecycle testing if touched.
6. Several runtime-path product files have unguarded or suspicious Unity Editor namespace references; Player build closure is not established.
7. Build Settings has no valid enabled scene and no formal automated test suite is present.
8. Static high-range Lab IDs are a serialized protocol; deleting or renumbering them can strand references in old stories.
9. Existing generated files and comments contain formatting/encoding noise; broad rewrites can hide functional diffs.

Use the audit script for a quick current snapshot:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File `
  "<skill-dir>/scripts/audit-project.ps1" -ProjectRoot "<project-root>"
```

Treat its findings as routing signals, not automatic proof of a defect.
