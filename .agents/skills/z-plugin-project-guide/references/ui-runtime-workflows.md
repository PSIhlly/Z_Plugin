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

After the map-complete callback applies the initial/saved position, `PlayManager.BeginScene` finishes controller/player setup and immediately dispatches the pending Enter/FirstEnter event. Keep its `playSceneStart` loading item until the first normal event `LateUpdate` completes, so an immediate `ShowDialog` appears before the Play HUD can render. Do not run the interpreter an extra time during initialization or re-enable gameplay while the map is loading. Release the presentation loading item on scene/story exit and initialization failure too.

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
- `UiContainer.AddReal` restores sibling order before activation, so pooled rows retain the current render order after filtering or count changes rather than their former hierarchy order.
- Story switches do not guarantee that Mod UI objects or their Model state are
  destroyed. Reset story-scoped filters and selections explicitly when required.

Supported hierarchy prefixes include `btn_`, `txt_`, `ipt_`, `go_`, `img_`, `rimg_`, `as_`, `scr_`, `sld_`, `sta_`, `rtf_`, and `dp_`.

Shared Lab-list contract:

- Form-backed UI rows and selection entries use ascending Data primary keys (`id`, or `uid` for those Forms), never dictionary/insertion or display-name order. This includes ModStoryMapObjectList's Texture/Mask/Object rows, product/scene/parameter/asset lists, Play Form rows and Form selection windows. Build filtered rows in that order, retain special All/Unclassified/None rows in their existing slots, and append the New/Add row last. Do not reorder authored event execution lists, animation frames, or enum-defined equipment/skill slots.
- Concrete Lab rows use ascending `LabForm.id`; identical path aliases remain deduplicated to their lowest ID. Event Program category rows use the lowest Lab ID for that first-level name; command/program choosers order categories by Lab ID and the merged category's actual entries by Data UID, including legacy aliases. Sorting is UI-only; do not reorder Form registries or saved authoring sequences.
- `scr_labs.sta_state` uses `0 = All`, `1 = Unclassified (labId=0)`, `2 = concrete Lab`, and `3 = New`; `sta_` remains the independent selection state.
- ModStoryEvent Program renders one `scr_lab` list backed by first-level `EventProgramDataForm` Labs (`lv1Lab`); the second list is removed. `labId=0` is unclassified, and label changes assign a flat Lab.
- New Event labels create a first-level `LabForm` path with `belong = nameof(EventProgramDataForm)` and remain listed even before a Program uses them. Its `ipt_lab` and `btn_deleteLab` rename or delete the selected first-level label, moving all matching Program references first; the Form schema does not change.
- Build Lab rows from `LabForm.DatasByBelong[nameof(ConcreteForm)]`, so empty labels remain visible and Lab identity keeps its concrete `belong`.
- In UI filters, `null` means All; `LabForm.NoneId == 0` remains unclassified and is not interchangeable with `null`.
- Render the unclassified row only when the current screen has actual `labId=0` data; when it disappears, reset an active unclassified filter to the screen's valid fallback instead of leaving an invisible selection.
- `UiPlayDataBackpack` is a read-only Label filter: show All, optional Unclassified, and existing Item labels, but never render the New row or create labels from Play mode.
- `UiPlayDataCharacter` resets to its Data module each time the panel is shown; Equip and Skill remain user-selected secondary tabs for that opening.
- `UiPlayDataCharacterSkill` lists only equipped Skills that resolve to a `SkillProductForm` record. Each slot's `txt_` shows the translated `SkillType`; selecting one shows that type in the page's `txt_`, the Skill name in `txt_name`, and `SkillProductForm.desc` in `txt_desc`. There is no `txt_type` binding. `UiModStorySkillUnitOverview.ipt_introduction` edits that same persisted description field.
- Play scenes may have `ProgressForm.characterUid == 0` after the Mod story config removes the main character. `PlaySceneController` then keeps a null player and positions the camera from saved progress; the main HUD tolerates the pre-player `OnShow`, refreshes after player setup, omits character parameters/actions and missing team rows, and guards player-only collision/interact callbacks. Mod story config displays an empty main-character name until one is selected; when adding a team or active-team character while the current UID is `0`, it assigns the added character as main and keeps that main character active. Core editing does not directly rewrite `Save/pf`; `LoadSaveStory` repairs invalid saved current-character IDs separately.
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
- `UiModStoryEffectUnitCtrl` renders Clip rows under an inner `ContentSizeFitter`. After refreshing the Clip container, rebuild its parent layout before `go_content`, then repeat that order in the current `LateUpdate` so the outer scroll content receives the final Clip height.
- `UiModStoryEventEditWindow` owns one Unit container per Item row. The prefab's `rtf_unitRoot` and `go_unit` are inside `go_item`; render each non-empty row's syntax as Units there. Unit code must not assign any height, including `rtf_root` or `img_`; their dimensions come from the prefab/layout. Toggle `go_image` for image visibility and use `img_` only to bind the texture; keep the child image enabled in the prefab. `go_line` is a separate, one-to-one background/click target for each rendered Item: its Sta states are unselected even row (`0`), unselected odd row (`1`), and selected row (`2`). Clicking a Line selects that row's root Unit; clicking a nested Unit also selects its owning row for insert/delete operations. Selection updates only existing Unit/Line states and detail buttons; it must not rebuild the list or center/scroll the selection. Refresh Unit selection through the controllers registered when instantiated, not every `unitCon.paramLst` entry: nested Unit rendering appends params and not all planned params are guaranteed to appear in the container's `Get` lookup. Generated Item/Line/Unit template controllers have no Param and can be activated by panel state changes; keep them disabled instead of running data-dependent rendering.
- Event-edit Units omit empty argument slots and empty string arguments from their visual child list while retaining the syntax-tree positions and serialized call. Resolve custom Event Program call nodes by their source name through `ProgramDataForm.DataByName` (their syntax nodes do not contain the numeric UID); render visible stored arguments as editable Units with their original `paramN` indices and separators. Creating, inserting, or replacing a custom Program call emits `ProgramName(param1,...,paramN);` from that Program's current `paramCount`.
- Event-edit Operator Units must render editable operands even when `CmdDataForm` has no description (including `&&`, `||`, `!`, `%`, `++`, and `+=`). Binary AST children are `[right, left]`; the fallback displays left/operator/right, unary operators display before their operand, and `++` displays after it. Preserve existing nonempty binary descriptions, but never use them for unary +/-; group nested Operator operands explicitly in parentheses so prose descriptions cannot hide AST grouping. Empty strings are hidden only as call arguments, not as Operator operands (render the latter as `""`). Rendering must not mutate the syntax tree or drop operand Unit identities.
- Event-edit Item indentation is rendered by one pooled `Deepth` child per nesting level before the Item content, not by spaces in `txt_new` or manual `rtf_unitRoot` offsets. A `for` body Action is a container, not a second visible Item: render its statements and insertion row directly one level below the `for` Item, targeting the body's own statement list.
- Event-edit Item creation and Insert compile the table's command default. Independently callable `void` defaults must already include `;` in the Excel source; Item UI must not silently repair missing terminators. `if`/`for` syntax blocks remain unterminated by `;`.
- Applying raw code in `UiModStoryEventEditWindow` places the caret at the first compiler error's source index, waits for the input field to scroll it into view, then anchors a non-expiring Notify Comment to that character's screen position. Keep the code editor open on failure; other entry/unit compilation failures continue using Tips.
- `ScrView.xSpacing`, `ySpacing`, `top`, `left`, and optional item offsets are Content-local Canvas reference units, not fixed physical screen pixels. Keep viewport bounds, cell bounds, virtualization tests, content sizing, and item placement in Content-local coordinates; do not mix world-space corners or `lossyScale` into those calculations.
- `UiModStoryMapObjectObjectConfigCtrl` toggles the saved Object `isWangTile` flag through `btn_isWangTile` and reflects it in `sta_isWangTile`; the generated UiBase and prefab bindings must both expose those names. Object texture variants are built at scene load and rebuilt for just the affected product when Mod changes its flag, face type, or animation clip.
- The same Object Config binds `btn_boundsCollision` once in `OnCreate` and renders `sta_boundsCollision` from the saved Object flag. Its `boundsCollision` text key is supplied by ModText. This option fits generated Collider X/Z to texture alpha bounds (see map-collision.md), not the visual model dimensions; default/off and old stories preserve existing collision behavior.
- `UiEventCustomTriggerCtrl` sorts displayed custom triggers by their smallest referenced Event Program UID, then by trigger name; triggers without a program come after numbered triggers, and the Add row is always last. Do not reorder `EventTriggerForm.Data.evt` itself: that list is the execution sequence.
- Object Config, not Appearance, owns `ipt_colliderScale` (stored in `MapObjectForm.model.colliderScale`) and `btn|sta_centerCollider` (saved Object flag; default on). Bind once, refresh the input/state and rebuild Config layout. Toggling the anchor refreshes product instances and physical indexes. ModText supplies the bilingual `centerCollider` label.
- Event Program editing uses one first-level Lab list. Program creation and selection assign a first-level `EventProgramDataForm` Lab; the former second list and the edit window's `btn_type/txt_type` are removed. Keep partial Controllers free of those obsolete bindings after Generate. Item sizing belongs to the prefab layout, not controller code.
- `UiModStoryEventEditWindow` uses `btn_lab/txt_lab` for its Lab chooser and one shared Apply button. Clicking Apply, switching modes, or clicking Return compiles the active Item AST or Code text and synchronizes the other representation; the full Item/Line list is rebuilt while its panel is visible, including after switching back from Code, followed by a next-update layout rebuild for nested ContentSizeFitters. Do not build Item rows while the Code panel hides their hierarchy, or they can retain collapsed sizes. Return closes only after a successful apply, while a compile error keeps the current mode open. Unit replacement refreshes only its owning Item row; Item insertion and deletion stay in the draft until Apply, mode switch, or Return.
- `Z_Ui.Base.Ipt.scrollParentOnDragOutside` is opt-in (off by default). While dragging a text selection beyond its ancestor `ScrollRect` viewport, it scrolls that parent in enabled directions and asks TMP to extend the selection after the content moves. `ModStoryEventEditWindow.ipt_code` enables it in the prefab; the outer ScrollRect content must contain the input field.
- Event-edit Unit, new row, and Insert command choices open `ModStoryEventCmdChooseWindow` through `ModAssetCtrl.ChooseCmd`. Its left list groups eligible GameCmds by translated Lab and custom Programs under `#`-prefixed Lab; its right list selects an entry and closes before invoking the editor callback. The prefab lists use plain button templates without child UiHolders, so this window pools those template rows and sizes their existing ScrollRect contents through LayoutGroups.
- `Txt.OnPreRenderText` runs inside the Canvas graphic-rebuild loop. Inline-image callbacks may only capture TMP character geometry there; create, resize, enable, disable, or change `Img` sprites after the loop has finished.
- `NotifyManager.AddTip` wraps Tip text at 30 characters per line while preserving explicit line breaks; Popup and input-area text are not wrapped by this rule. A pooled Tip rebuilds its text and root immediately, then repeats that child-to-parent rebuild in the current `LateUpdate`, after activation-time `ContentSizeFitter` callbacks have settled, so the background width follows the current text rather than the previous pooled value.
- `NotifyManager.AddComment(content, screenPosition)` wraps text like a Tip and shows a non-blocking Comment at an explicit screen-pixel position. Comments have no timer, may coexist, and each closes through its own `btn_back` (or an explicit global `ClearAll`); their pooled controllers rebuild layout and restore the requested position on each show.
- `UiModAssetSelectWindow` exposes `ipt_labelName` and `btn_lableDelete` for the active Label filter; renaming reuses matching labels and deletion moves affected assets to unclassified before removing the Label record.
- `UiModAssetSelectWindow`'s `btn_replace` opens the single-asset picker for the selected item, updates the existing asset in place (keeping its ID), and clears texture runtime caches before refreshing.
- `UiModAssetSelectWindow` closes its AVPro audio preview when hidden or shown again; preview playback must not resume from a previous selection.
- `UiModAssetSelectWindowCtrl.OnShow` keeps the last Label filter when reopening within the same asset scope; if that label is no longer visible, fall back to unclassified when available, otherwise All.
- Map Object Config edits `MapObjectForm.faceType`; Object Appearance displays `Fixed` for Fixed/Flexible objects and `Up/Down/Left/Right` for FourDirection objects, with a separate animation-frame list per direction. Bounds-enabled previews pass the selected direction to `CombineNewObjectByPrefabs(previewDirection: model.dir)` so their Box fits only that clip (or its assembled isolated WangTile), never the union of four facings. Runtime direction thresholds match Character (`Up` around 0°, then Right/Down/Left by 90° quadrants); Flexible uses the Fixed clip while allowing the perspective holder to follow Object rotation.
- Object Appearance `ipt_posHeight` is bottom-relative for the unit-height model: display `subPrefabUnitPos[0].y - (abs(height) - 1) / 2`, and store the inverse. Changing `ipt_height` preserves that bottom height by adjusting the saved center offset; dragging the preview X/Z axis preserves Y. Shared model assembly still adds its legacy `0.5` pivot, so old saves, bridges, Items and Characters retain their authored geometry without migration. Height-position `0` places the bottom at the Object root for any edited model height, consistently in preview and runtime; Reset remains scale one/offset zero.
- Empty Object thumbnails stay blank: list, appearance-frame and ModTool rows always rebind through `Img.BindTexDataOrHide`, including null/New rows, so previous pooled static/animated bindings and Sprite overrides are cleared. Hide the Image when no Sprite exists and re-enable it before binding a valid image. Empty/missing/default-placeholder frames must not reuse an existing texture or fall back to Object `icon`; ModTool uses the default-facing clip, not the legacy mirror of the last edited facing. New Object texture slots contain ID `0` until import; existing explicit empty directional clips remain empty.
- Map Object Type opens `ModStoryMapObjectPassType` as its fourth mode. That page owns PassType add/delete/rename; MapTexture Config chooses one type or `0 = unrestricted`, while Character Unit Config renders all registered types as a multi-select backed by `CharacterProductForm.passType`.
- MapTexture Config owns both `isWangTile` and `enableFrontPart`. Its refresh updates every state, pass-type label, and EventChoose model before recursively rebuilding the active config root, so dynamic rows and text settle in the same refresh. MapTexture Appearance always edits the normal `texs` animation list and shows the mirrored `frontPartTexs` list only while `enableFrontPart` is on; disabling the option hides, but does not delete, the authored front frames.
- ModSceneMain's `btn|sta_layer0..2` is the only layer selection: selecting `n` enters map-edit mode, writes Texture/Mask/TextureOnly operations to logical layer `n`, and displays layers `0..n` (normal/front pairs). Each displayed layer below `n` uses half of that Renderer pair's recorded initial `_LightSensitivity`; layer `n` keeps its initial value. There is no separate MapObject button or Tool layer selector. Each scene entry defaults to layer `2`. Selecting Event clears all three layer selection states, displays `0..2`, and restores every layer's initial light sensitivity, without hiding Tool or discarding the current brush. Texture, Mask, and texture-erasing brushes are rejected on ground with the translated ModText key `cantUseInEventMod` (once per pointer stroke); non-layer brushes remain usable. With no brush selected, Event clicks open the unit behavior editor. Selecting any layer, including the previous layer, leaves Event mode. The lifetime-owned ModSceneController Tile Show listener reapplies the display ceiling and lighting override for newly shown/pooled Tiles without changing their current occlusion degree; when ModScene is disabled, it restores unrestricted display and initial light sensitivity so Play does not inherit Mod-only Renderer overrides.
- `UiPlayMap` uses `PlayMapController.heightMap` and `unlockTextureMap`, which are generated for every Tile height when the scene begins. Its area-map ScrollRect content keeps 200 pixels of padding on every edge while map images, marks, and missions share the inner map bounds. Area-map height controls traverse only sorted heights that contain Tiles, display player-facing height coordinates, and show marks and the active mission only when their Tile/target height matches the browsed height.
- Auto-generated area maps and scene minimaps sample base layer `0` through `GameMapController.GetTileLayerTexture`, sharing the scene's WangTile neighbour/variant selection rather than shrinking the source sheet. Texture rows and unlock pixels follow increasing map Z without flipping each tile's top/bottom edges. Custom scene minimap assets are unchanged.
- Minimap unlock masks are readable RGBA32 textures without mipmaps. Initialization fills saved unlocks using a reusable black Tile pixel block and uploads once per height. Runtime reveals deduplicate Tile UIDs, write each block with `SetPixels`, and mark dirty heights; `PlayManager.LateUpdate` calls `FlushUnlockTextures` after event processing so each changed height uploads once with `Apply(false, false)`, and unchanged masks do no uploads. Begin/End clear pending heights; preserve the bottom-to-top row orientation and the independent terrain-map texture resolution.
- `PlayMapController.Begin` initializes from the loaded `GameManager.curScene` while Play updates are still disabled; never gate bounds registration on `PlayManager.enable` or resolve the scene through the not-yet-committed progress scene ID. Map UI waits for `isReady`. All map positions use `TryGetRelativePosition`: world-to-map conversion, half-cell outer borders, and inclusive row/column counts keep single-row/column maps finite. Empty maps clear their bounds and stay unready.
- `UiPlaySceneMission` is visible only when missions are enabled and the selected mission is shown, received, unfinished, and not failed. Its active parent listens for `MissionEvent` so a hidden mission widget can reappear when a mission is added.
- Mission `targetPos` is authored in player-facing coordinates, not world coordinates. `MissionGuide.GetTargetWorldPosition` applies `PlayerPosToMapPos` and `MapPos2RealPos` before HUD/list distance and map/minimap placement; measure Euclidean world meters against the current player data position, not the previous camera-update snapshot. Other scenes show the translated destination prompt and missing target scenes clear stale text. HUD and mission-list owners subscribe to `StoryLifeEvent.EverySecond` while shown, update only distance text, and unregister on hide; this follows game seconds and pauses with blocked game time. Mission selection emits `MissionEventType.Select` so the HUD changes immediately. Map/minimap mission markers must match their target scene.
- ModScene entry and Object Show attach the instance-owned `MapPrefab$mapMark`; Event/layer mode switches and scene exit publish `ObjectMarkVisibilityEvent`. Only Event mode shows markers, with unit local scale; do not poll them each frame or inject decorations into runtime prefab templates/model Renderer slots. Offscreen Objects acquire their marker when their normal pooled instance is shown.
- ModSceneMain `btn_undo/btn_redo` call the scene controller's scene-local `ModSceneHistory`. Wrap every authored scene mutation in `ExternalModSceneController.BeginOperation()` and call `Track(data)` before modifying/removing each affected Unit Form row, or `TrackAdded(data)` after registering/initializing a new row. Nested scopes merge into one transaction and repeated tracking of the same row captures its original value once. Main/Scene changes use explicit `TrackMain/TrackScene`; an empty BeginOperation must not serialize any Form. The history covers MapMain, Tile/Object/Character/Item Unit Forms (including nested `extra` data), and the current SceneForm row. Brush down/up is one scope, including releases over UI; coordinate/rotation input starts at the first real edit, and the three-axis Align action is grouped. ModAssetCtrl placement APIs own their scopes/tracking, so AddScene commands participate too. Tool/mode/layer/camera selections, save-time `*Ja` caches, `enteredScene`, and collision bookkeeping are not operations. No-op edits preserve redo; a meaningful new edit clears redo; at most ten changes are retained. Empty undo/redo shows translated `noAvailableOperations`.
- Scene history is strictly row-local: capture only explicitly tracked rows before the first write and once on outer commit, with deep-cloned nested data. Never enumerate entire Form tables at Begin/commit/undo/redo. Restore only changed rows through `ModSceneHistoryRestore`, keeping unrelated Data/Units/pooled instances and the existing navigation controller/graph alive. Rebuild only affected ownership/visual/pass/physical indexes, Tile neighbours, terrain samples and incoming/outgoing navigation links; no Map.Begin/End/Init, Form.Clear, global animation reset, full navigation rebuild or forced full-view refresh. Existing Tile/Object Show listeners synchronize editor layer/marker state on refreshed units. ModScene `ForceUpdate` retains input throttling only: mutation APIs already refresh the changed unit/neighbourhood. Placement applies the final Item/Character angle through single-unit `ApplyMove`; an already shown Item refreshes its own appearance after product metadata is assigned. Do not depend on a later full-view pass to initialize placement. Close unit editors holding changed references without restarting Main/Tool or changing camera/selections. Begin/End clears scene history.
- ModScene unit coordinate/rotation editing uses teleport-style `ApplyMove` and publishes one Object `Refresh` event to update the moved Object and its old/new WangTile neighbours. Rotation and coordinates share this path; do not duplicate the appearance refresh or depend on a full-map update. This editor appearance notification is not a gameplay `Move` trigger.
- Scene-controller model-placement offsets can survive ModScene reentry. Erase targets the clicked logical Tile grid, without those X/Y/Z offsets, while preserving the Tool values for subsequent model placement. Actual pointer Down/Up erasure must remain one undoable stroke and leave neighbouring owners untouched; do not silently reset placement controls as an erase workaround.
- ModTool placement height has no upper limit for Objects, Items or Characters; retain the existing nonnegative minimum and map-unit conversion. The shared ModSceneUnit coordinate editor also removes its owner-relative `+0.9` upper cap, retaining its existing lower bound. `ForeachPos` and all three ModAsset placement APIs resolve the target layer's Tile or the nearest lower Tile in the same X/Z column through `MapUtilController.GetPlacementTile`. This also applies when the camera's current layer is empty at the clicked coordinate. Keep the requested world position/rotation rather than snapping down to the owner; if no Tile exists at/below that logical layer, reject without a Form row or history/redo mutation. Tile brushes still use exact-layer rules.
- ModScene no-brush Object clicks, in both normal and Event mode, first skip higher owner Tiles and Objects whose enabled, active physical Collider lower bounds all exceed the current editing plane (`CameraInstance.tarTrs.position.y`, tolerance `0.0001`). Use live world `Collider.bounds`, including child offsets/rotation/scale and texture fitting; padded picking Triggers cannot admit a physically elevated Object merely because its root/belongTile is lower. Any body touching or extending below the plane permits selection; disabled/inactive bodies do not. Trigger-only prefabs use the hit Trigger's lower bound. Continue through sorted ray hits to prefer any height-eligible target. Only when none is selected, use the first skipped Object in the existing near-to-far (top-down) ray order. Normal mode still ignores Tiles; an eligible Event Tile counts as a selection and prevents fallback. Item/Character selection and placement/erase are unchanged.
- Ordinary ModScene frames call `MapUpdateController.UpdateInfo(simulateCharacters: false)`, preserving incremental visibility/occlusion and Unit `AfterUpdate` appearance callbacks while skipping Character gravity/navigation/pending gameplay rotation. Authored Character coordinates must not drift between brush hold and release. Character placement rejects the same product UID at the same 3D coordinate (existing squared-distance tolerance `0.001`), irrespective of instance name or placement angle; rejected placements add no history operation. Play keeps the default Character simulation enabled.
- `PlaySceneEffectController` owns character-attached `CanvasHolder` instances for exactly one Play scene. `EndScene` invalidates queued LateUpdate bindings, resets each holder, returns it to the current canvas pool, and clears the UID cache before the next scene rebuilds prefab pools. `CanvasHolder.Reset` is idempotent and tolerates UI children already destroyed by external pool teardown; pooled Slider text is destroyed with its Slider root rather than a second time.

## Global event bus

- MapTexture Config binds `btn|sta_frontIsWangTile` independently of `btn|sta_isWangTile`; saving and refreshing must preserve both flags. The front flag only affects an enabled front part, and its `frontIsWangTile` ModText key renders the translated label.

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
- For ModScene history, run `Docs/Tests/Run-MapRuntimeRegression.ps1 -Fixture ModSceneHistoryRuntimeRegression`; include a map of at least 4,000 Tiles and assert empty transactions serialize zero rows, a single existing row is serialized only twice, and undo/redo preserves unrelated Data/Unit/Instance/navigation identities. Manually verify a multi-tile brush stroke and release over UI, coordinate/rotation input, Align, unit delete, each AddScene command, ten undo/redo steps, redo branching, empty-history Tips in both languages, retained tool/mode/layer/camera selections, and history reset on scene exit/reentry. Profile with the large map to reject any reintroduced all-Form capture or map reload.

Story ModCmd form mutations use explicit `*Story*` names: AddStoryX, DelStoryX, CopyStoryX, and SetStoryX (for example AddStoryCharacter). Scene mutations use `*Scene*` names such as AddSceneTile, AddSceneItem, AddSceneObject, and AddSceneCharacter.

`StoryEffect` maps to `EffectForm` and supports all four Story operations. `AddStoryEffect` initializes the same default clip/frame as Mod Story's New Effect action because the generated key-0 Effect row has null `clips`, which the Effect editor cannot render. `CopyStoryEffect` uses the standard JSON round-trip so nested clips are independent; `SetStoryEffect` accepts the saved `name`, `labId`, `clips`, and `ground` fields. Keep the bilingual `storyCmdTips` workbook entry synchronized and regenerate `ModTextForm.cs`.

ModCmd implementation files: ModCmd.cs is the facade/shared parser; ModCmdStory.cs contains Form mutations; ModCmdScene.cs contains AddScene mutations. Keep them in the same Assembly-CSharp boundary.
