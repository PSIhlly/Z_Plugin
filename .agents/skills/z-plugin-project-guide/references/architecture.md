# Architecture and module boundaries

Use this reference to place code, understand ownership, and avoid invalid dependencies. Re-check `.asmdef` and scene files when the repository changes.

## Contents

- [Layer map](#layer-map)
- [Core modules](#core-modules)
- [Actual assembly boundaries](#actual-assembly-boundaries)
- [Product entry and modes](#product-entry-and-modes)
- [Runtime object and command flows](#runtime-object-and-command-flows)

## Layer map

| Area | Responsibility |
|---|---|
| `Assets/Z_Level0` | Foundations: design patterns, math, serialization, strings, textures, shaders, vendor plugins |
| `Assets/Z_Level1` | Focused services: audio, debug, mesh, time |
| `Assets/Z_Level2` | Reusable systems: client, code interpreter, data/assets, input, object animation, text, utilities, units |
| `Assets/Z_Level3` | Composed gameplay: fight, map, UI, login, inactive ILRuntime prototype |
| `Assets/Z_Level4` | Code visualization prototype and Unity Editor tooling |
| `Assets/GameSample` | Product layer: main runtime, UGC editor, play mode, product Forms, UI, assets |
| `Z_OtherProjects` | Separate server, Android, Protobuf, Excel2Cs, and utility projects; keep out of Unity runtime scope unless explicitly requested |
| `ExtraAssets` | Sample/import content and art; it is not the default runtime save root |

There is currently no `Z_Level5`.

## Core modules

- `Z_DesignStyle`: `Z_Singleton`, `Z_MonoSingleton`, `Z_Manager`, `Z_MonoManager`, `Z_Controller`, `Z_EventHelper`, pools and collections.
- `Z_Math` / `Z_Mesh`: algorithms, geometry, SAT collision support.
- `Z_Serialize`: JSON hooks, byte serialization, compression.
- `Z_Audio` / `Z_Texture`: media services; AVProVideo and NativeGallery are vendor dependencies.
- `Z_Code`: compiler, interpreter, command registry, command metadata Forms.
- `Z_DataSystem`: assets, Form roots, file persistence helpers.
- `Z_UnitSystem`: Unit, Instance, pooling, collider events.
- `Z_Map`: map state, tiles, objects, items, characters, navigation, movement and interaction.
- `Z_UI`: UiHolder framework, generated bindings, dialog/loading/notification modules.
- `Z_Text`: localization and text Forms; despite its Level2 location, it has no asmdef.

## Actual assembly boundaries

Treat `.asmdef` as authoritative; do not infer an assembly from folder level or namespace.

```text
Level0/1/2 Core asmdefs
          ↓ autoReferenced
Assembly-CSharp
  ├─ Z_Level2/Z_Text
  ├─ Z_Level3 runtime
  ├─ Z_Level4/Z_CodeVisual
  ├─ module Samples
  └─ GameSample

Assembly-CSharp-Editor
  ├─ Z_Level3/Z_UI/Core/Editor
  └─ Z_Level4/Z_Editor/Editor
```

`QuickModifyComponent` is self-contained in `Assembly-CSharp-Editor`; its optional single-size adjustment is enabled from the window and uses Space-drag on the selected `RectTransform`, with Unity Undo support. It must not depend on an external `GameCore` editor namespace.

Important core dependencies:

```text
Z_Debug, Z_Time              → Z_DesignStyle
Z_Mesh                       → Z_Math
Z_Audio                      → Z_DesignStyle + AVProVideo
Z_Client                     → Z_Serialize + Z_DesignStyle
Z_Code                       → Z_Debug + Z_Serialize + Z_DesignStyle
Z_DataSystem                 → Z_Serialize + Z_DesignStyle + Z_Texture
                               + Z_Time + AVProVideo + NativeGallery
Z_Input                      → Z_DesignStyle + Z_Debug
Z_ObjectAnimator             → Z_Time + Z_Debug
Z_Trick                      → Z_Serialize + Z_DesignStyle
Z_UnitSystem                 → Z_Serialize + Z_DesignStyle + Z_Mesh
```

Rules:

- Put reusable core code inside the intended asmdef's physical subtree.
- Never make a lower asmdef depend on `Assembly-CSharp` types.
- Remember that partial types cannot span assemblies.
- Avoid introducing new Level3/4 asmdefs without first resolving existing cross-module cycles.
- Do not trust namespace ownership alone. For example, `Z_DataSystem/Core/SaveAndLoad.cs` declares a `Z_UnitSystem` namespace.

## Product entry and modes

- Main Unity scene: `Assets/GameSample/Game.unity`.
- Global entry: `Assets/GameSample/MyScripts/Main/GameManager.cs`.
- Story/logical-scene lifecycle: `Assets/GameSample/MyScripts/Main2Scene/Main2StoryManager.cs`.
- Runtime UGC mode: `Assets/GameSample/MyScripts/Mod/ModManager.cs`.
- Play mode: `Assets/GameSample/MyScripts/Play/PlayManager.cs`.
- Map runtime: `Assets/Z_Level3/Z_Map/Core/MapManager.cs`.
- UI runtime: `Assets/Z_Level3/Z_UI/Core/UiManager.cs`.

`GameManager.Init()` creates its plain C# controllers, initializes language/BGM, loads built-in `Resources/Z_Map` content, and loads story overview data. `GameManager.Start()` opens the entry UI.

The project uses one principal Unity scene. A story “scene” is a serialized logical map:

```text
unload Map/Unit state
→ read Core/Cache/Save data
→ MapManager.Begin(MapInfo)
→ start Mod or Play controllers
```

Do not replace that flow with Unity scene loading.

## Runtime object and command flows

Create map entities through the existing chain:

```text
Form/Data → Unit → Instance → InstancePoolManager → Unity GameObject
```

Do not create parallel long-lived GameObjects that bypass Unit/Map/pool ownership.

Game commands live under `Z_Code` and `GameSample/MyScripts/Main/Event/Cmd`. A command normally:

1. Inherits `CmdBase`.
2. Registers with `RuntimeInitializeOnLoadMethod(AfterAssembliesLoaded)`.
3. Keeps `GetName()` aligned with command metadata.
4. Returns synchronous completion from `ExecuteInternal`, or completes through `InterpretAsyncTask`.

Preserve these registrations for IL2CPP/AOT and stripping behavior.
