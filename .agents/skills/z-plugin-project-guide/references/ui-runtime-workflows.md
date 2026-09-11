# UI and runtime workflows

Use this reference for Managers, Controllers, product modes, UiHolder-generated UI, global events, and game commands.

## Contents

- [Manager and Controller model](#manager-and-controller-model)
- [Product runtime flow](#product-runtime-flow)
- [UiHolder generation model](#uiholder-generation-model)
- [Global event bus](#global-event-bus)
- [Event command system](#event-command-system)
- [Smoke-test checklist](#smoke-test-checklist)

## Manager and Controller model

Framework types live under `Assets/Z_Level0/Z_DesignStyle/Core`:

- `Z_Singleton<T>`: lazy plain C# singleton.
- `Z_MonoSingleton<T>`: finds an existing component or creates a GameObject/component.
- `Z_Manager<T>` / `Z_MonoManager<T>`: system lifecycle owner.
- `Z_Controller<T>`: plain C# child owned by a Manager.
- `Z_EventHelper`: synchronous static event bus.

Follow these rules:

- Call `base.Init()` when overriding `Z_MonoManager.Init()` and keep initialization idempotent.
- Construct plain Controllers from their owning Manager and pass the Manager to the controller base.
- Preserve the established `InternalXxxController` / `ExternalXxxController` split: Managers call lifecycle-facing internal APIs; UI and commands consume the narrower external API.
- Do not access a scene-configured singleton before its serialized instance completes `Awake`. Auto-creation can yield an unconfigured object.
- Inspect Inspector dependencies before moving initialization. `UiManager`, `MapManager`, and `InstancePoolManager` rely on serialized fields.
- Treat `TimeManager.Awake()` hiding its base method as an existing lifecycle risk when changing time initialization.

## Product runtime flow

Primary files:

- `GameSample/MyScripts/Main/GameManager.cs`: global services and product entry.
- `GameSample/MyScripts/Main2Scene/Main2StoryManager.cs`: story and logical-scene lifecycle.
- `GameSample/MyScripts/Mod/ModManager.cs`: runtime UGC editor.
- `GameSample/MyScripts/Play/PlayManager.cs`: play mode.

The runtime has two product modes:

```text
Mod:  authored Core data → runtime UGC editing → save Core
Play: Core/Save data → Cache logical scene → runtime progress/save
```

`Mod` is not a Unity Editor folder. Keep it Player-compatible.

Story and scene transitions must coordinate:

1. End the active Mod or Play controller lifecycle.
2. Unload Map/Unit/event state.
3. Reset process-global Forms.
4. Load the next story tables in dependency order.
5. Load or copy logical map data from Core/Save/Cache.
6. Begin Map and the target mode.

`ModStory` Test/Play must show `UiLoading` and allow it to render for one frame before synchronous story save/unload work begins. Keep a transition-owned loading item active until `PlayManager`/scene loading has registered its own loading items, so the loading UI cannot flicker closed between phases.

Test consecutive stories and repeated mode entry; many failures are stale singleton, listener, Form, or cache state rather than local logic.

## UiHolder generation model

Core files:

- `Assets/Z_Level3/Z_UI/Core/UiManager.cs`.
- `Assets/Z_Level3/Z_UI/Core/UiPreloadConfig.cs`.
- Inspector-assigned `UiPreloadConfig` ScriptableObject assets.
- `Assets/Z_Level3/Z_UI/Core/Basic/Frame/UiHolder.cs`.
- `Assets/Z_Level3/Z_UI/Core/Basic/Frame/UiCtrl.cs`.
- `Assets/Z_Level3/Z_UI/Core/Editor/UiHolderEditor.cs`.
- Product output: `Assets/GameSample/UiBase`.

Flow:

```text
Prefab + UiHolder hierarchy
→ UiHolderEditor parses names
→ Ui{Name}Base.cs partial View/Ctrl/Model/Param
→ hand-written partial Controller under GameSample/MyScripts
→ UiManager.ShowUi<T>() / CloseUi<T>()
```

Top-level generated Panels use a persistent Prefab or Prefab Variant as their
visual source. `UiManager` and each generated top-level `UiHolder` select a
`UiPreloadConfig` asset in the Inspector. The Manager reads its selected registry;
a successful Generate upserts the saved prefab source by `uiName` into the
Holder's selected registry. Keep those references aligned for each scene/product.
Pure scene holders must be prefabized first; apply relevant instance changes or
Generate in Prefab Mode so code and runtime bindings use the same source.

Lifecycle details that matter in this project:

- `UiManager.ShowUi<T>()` normally reuses the first controller instance.
- `UiHolder` guards `OnCreate()` with the controller's `inited` state, but runs
  `OnShow()` every time the UI is shown.
- `UiContainer` pools and reuses child controllers.
- Story switches do not guarantee that Mod UI objects or their Model state are
  destroyed. Reset story-scoped filters and selections explicitly when required.

Supported hierarchy prefixes include `btn_`, `txt_`, `ipt_`, `go_`, `img_`, `rimg_`, `as_`, `scr_`, `sld_`, `sta_`, `rtf_`, and `dp_`.

Shared Lab-list contract:

- `scr_labs.sta_state` uses `0 = All`, `1 = Unclassified (labId=0)`, `2 = concrete Lab`, and `3 = New`; `sta_` remains the independent selection state.
- ModStoryEvent second-level type selectors use `0 = All`, `1 = empty/unclassified level`, `2 = non-empty level`, `3 = New`; the first-level category selector omits All and uses `1 = unclassified`, `2 = non-empty category`, `3 = New`. All/empty filters must remain selected rather than auto-falling through to another type.
- New Event categories/types create an empty `LabForm` path with `belong = nameof(EventProgramDataForm)` and must be listed even before an event uses them.
- Build Lab rows from `LabForm.DatasByBelong[nameof(ConcreteForm)]`, so empty labels remain visible and Lab identity keeps its concrete `belong`.
- In UI filters, `null` means All; `LabForm.NoneId == 0` remains unclassified and is not interchangeable with `null`.
- Render the unclassified row only when the current screen has actual `labId=0` data; when it disappears, reset an active unclassified filter to the screen's valid fallback instead of leaving an invisible selection.
- `UiPlayDataBackpack` is a read-only Label filter: show All, optional Unclassified, and existing Item labels, but never render the New row or create labels from Play mode.
- `UiPlayDataCharacter` resets to its Data module each time the panel is shown; Equip and Skill remain user-selected secondary tabs for that opening.
- For New, prompt through `NotifyManager`, resolve with `LabForm.GetOrCreateDisplayName(input, nameof(ConcreteForm))`, then select a nonzero result and refresh.
- Mod list screens with `ipt_lab` and `btn_deleteLab` expose those controls only for a concrete selected Lab. Renaming reuses a matching Lab and moves every reference in that concrete Form; deletion moves references to unclassified before removing the Lab. Map Texture, Mask, and Object lists apply this within the currently selected map type.
- After renaming a prefixed object in a shared prefab, regenerate every owning Panel and update all partial Controller references to the generated field.

Rules:

- Do not hand-edit `UiBase`; the generator deletes and rewrites its target file.
- Do not restore a scene or `UiManager` preload list, and do not hand-write object references in the config asset.
- `UiHolderEditor.Generate` saves dirty Prefab Mode contents or applies current Prefab-instance overrides before reading the persistent Prefab source.
- Do not add an implicit Resources/default-config fallback; missing Manager or Holder references are configuration errors.
- Keep the registry free of null entries and duplicate `uiName` values; only top-level Panels are registered.
- Put state in the partial Model, input in Param, rendering references in generated View, and behavior in the partial Controller.
- Bind callbacks and create scroll containers once in `OnCreate()`.
- Apply incoming Param and call `Refresh()` in `OnShow()`.
- Never add the same listener in repeated `OnShow()` or pooled-item refresh paths;
  one click must still cause one action after hide/show and consecutive stories.
- Render Model → View in a dedicated `Refresh()` method; avoid hidden writes while rendering.
- Check `active` before refreshing a hidden UI from a global event.
- Use `UiManager.ShowUi<T>`, `CloseUi<T>`, and existing containers instead of manually instantiating UI prefabs.
- `Txt.languageTranslatable` treats `oriText` and serialized/overridden `m_text` as exact translation keys; include prefab-variant overrides in translation audits. Assigning `Txt.text` creates a raw runtime override that survives `OnEnable` and pooling, while assigning `oriText` clears that override and returns to the translatable source-text path. Use `oriText` for translation keys and `text` for runtime-authored literal content.
- `TextBaseForm.DataByKey` is shared by CommonText and ModText. Keep keys globally unique, put runtime/shared UI keys in CommonText, and reserve ModText for Mod-only UI.
- Translate chooser titles exactly once. Render user-authored code/text raw, and translate only the stable prefix of composite trigger keys before `$`.
- After changing dynamic TMP text when its RectTransform size is needed immediately, call `UiManager.Rebuild` on the active layout root. It synchronizes descendant `TMP_InputField` labels and `TMP_Text` meshes before rebuilding the layout; pass `recursion: true` for nested ContentSizeFitter/LayoutGroup chains.
- `UiPopupCtrl` is pooled: after changing `txt_content`, rebuild the text and ScrollRect content in the current LateUpdate before reading `content.rect.height`, then resize the ScrollRect and rebuild the Popup root. Measuring synchronously in `OnShow` can reuse the previous Popup's content height.
- Prefer synchronous rebuilds after refreshing pooled containers. `UiMultipleChooseCtrl` is a verified exception: refresh both virtualized lists first, then schedule `LayoutRebuilder.ForceRebuildLayoutImmediate(rect)` through `TimeManager.AddCurLateUpdateAction`; its pooled ScrollRects and nested ContentSizeFitters have not settled when the synchronous controller code finishes.
- `Txt.OnPreRenderText` runs inside the Canvas graphic-rebuild loop. Inline-image callbacks may only capture TMP character geometry there; create, resize, enable, disable, or change `Img` sprites after the loop has finished.
- `NotifyManager.AddTip` wraps Tip text at 30 characters per line while preserving explicit line breaks; Popup and input-area text are not wrapped by this rule.
- `UiModAssetSelectWindow` exposes `ipt_labelName` and `btn_lableDelete` for the active Label filter; renaming reuses matching labels and deletion moves affected assets to unclassified before removing the Label record.
- `UiModAssetSelectWindow`'s `btn_replace` opens the single-asset picker for the selected item, updates the existing asset in place (keeping its ID), and clears texture runtime caches before refreshing.
- `UiModAssetSelectWindow` closes its AVPro audio preview when hidden or shown again; preview playback must not resume from a previous selection.
- `UiModAssetSelectWindowCtrl.OnShow` keeps the last Label filter when reopening within the same asset scope; if that label is no longer visible, fall back to unclassified when available, otherwise All.
- Map Object Config edits `MapObjectForm.faceType`; Object Appearance displays `Fixed` for Fixed/Flexible objects and `Up/Down/Left/Right` for FourDirection objects, with a separate animation-frame list per direction. Runtime direction thresholds match Character (`Up` around 0°, then Right/Down/Left by 90° quadrants); Flexible uses the Fixed clip while allowing the perspective holder to follow Object rotation.
- Map Object Type opens `ModStoryMapObjectPassType` as its fourth mode. That page owns PassType add/delete/rename; MapTexture Config chooses one type or `0 = unrestricted`, while Character Unit Config renders all registered types as a multi-select backed by `CharacterProductForm.passType`.
- MapTexture Config owns both `isWangTile` and `enableFrontPart`. MapTexture Appearance always edits the normal `texs` animation list and shows the mirrored `frontPartTexs` list only while `enableFrontPart` is on; disabling the option hides, but does not delete, the authored front frames.
- `UiPlayMap` uses `PlayMapController.heightMap` and `unlockTextureMap`, which are generated for every Tile height when the scene begins. Its area-map ScrollRect content keeps 200 pixels of padding on every edge while map images, marks, and missions share the inner map bounds. Area-map height controls traverse only sorted heights that contain Tiles, display player-facing height coordinates, and show marks and the active mission only when their Tile/target height matches the browsed height.
- `UiPlaySceneMission` is visible only when missions are enabled and the selected mission is shown, received, unfinished, and not failed. Its active parent listens for `MissionEvent` so a hidden mission widget can reappear when a mission is added.

## Global event bus

- `ObjectUnit.Remove()` emits `ObjectEvent` with `MapEventType.Remove` after Form deregistration and instance cleanup. `GameEventSceneTriggerController` immediately forwards it as `SceneActionEvent.Remove` to clear interaction options without waiting for TriggerExit. Deferred collision callbacks reject removed Object instances, including removal during a touch event; the UI keeps one option per unit UID.

`Z_EventHelper` stores listeners in a static dictionary keyed by exact event type.

- Register once at construction/Begin/OnCreate according to the owner's lifetime.
- Unregister at End/destruction when the listener can leave the runtime permanently.
- Avoid duplicate registration and cross-story listener retention.
- Do not expect base-event or interface polymorphism; dispatch only matches the event's exact runtime type.
- Remember that the static dictionary strongly references listeners.
- Scene lifecycle listeners route Tile/Item/Character/Object through `QueueUnitEvent`: Tile/Item handle Create only; Character/Object also handle BoundaryTouch. Filter before the separate closure-allocating enqueue method. Keep Character Create self as a product reference and BoundaryTouch self as a scene-unit reference; preserve deferred execution. Story lifecycle processing here only accepts EverySecond.

## Event command system

Core command infrastructure is under `Assets/Z_Level2/Z_Code/Core`; product commands are under `Assets/GameSample/MyScripts/Main/Event/Cmd`.

`ModCmd` UI entry points use explicit execution scopes: ModStory accepts Form/story-data operations, while ModScene accepts only `*Scene*` operations. Enforce the scope inside `ModCmd` before parsing or mutating data; UI-only filtering is insufficient.

Both entry points open the shared top-level `UiCmdInputAreaCtrl`. Its Tips button resolves the `storyCmdTips` or `sceneCmdTips` ModText key for the active scope and shows the translated command reference in a scrollable Popup. Keep those two table entries synchronized with supported command names and editable `SetStory*` JSON fields.

When adding a command:

1. Inherit `CmdBase` and implement `GetName`, `GetNew`, and `ExecuteInternal`.
2. Register through `RuntimeInitializeOnLoadMethod(AfterAssembliesLoaded)`.
3. Keep runtime name, generated command metadata, Lab category, parameter count/types, and return metadata synchronized.
4. Complete asynchronous work through `InterpretAsyncTask`; return the correct synchronous/asynchronous state.
5. Consider IL2CPP/AOT and stripping; do not replace explicit registration with unverified reflection discovery.
6. Verify the command appears in the editor, compiles, serializes, loads, and executes in Play mode.

## Smoke-test checklist

- Open `Assets/GameSample/Game.unity` after Unity reimport.
- Enter and exit Mod mode twice.
- Enter and exit Play mode twice.
- Load two different stories consecutively.
- Open, refresh, hide, and reopen the changed UI.
- Generate the same Panel twice and confirm the persisted registry still has one entry; a rejected scene-only Generate must not change it.
- Confirm missing or mismatched Manager/Holder config references fail clearly and do not mutate another registry.
- Confirm the first Show from a registered prefab invokes `OnCreate()` and `OnShow()` once each.
- Confirm no duplicated callbacks or stale listener reactions.
- For command changes, execute both success and failure/async paths.
- For lifecycle changes, verify scene-configured Managers retain their Inspector references.

Story ModCmd form mutations use explicit `*Story*` names: AddStoryX, DelStoryX, CopyStoryX, and SetStoryX (for example AddStoryCharacter). Scene mutations use `*Scene*` names such as AddSceneTile, AddSceneItem, AddSceneObject, and AddSceneCharacter.

ModCmd implementation files: ModCmd.cs is the facade/shared parser; ModCmdStory.cs contains Form mutations; ModCmdScene.cs contains AddScene mutations. Keep them in the same Assembly-CSharp boundary.
