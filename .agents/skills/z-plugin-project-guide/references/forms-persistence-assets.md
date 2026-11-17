# Forms, persistence, Lab, and assets

Use this reference before changing any Excel-backed Form, generated API, serialized story data, label, or imported asset.

## Contents

- [Data pipeline](#data-pipeline)
- [Excel contract](#excel-contract)
- [Generation and inheritance](#generation-and-inheritance)
- [Runtime Form behavior](#runtime-form-behavior)
- [JSON and story persistence](#json-and-story-persistence)
- [LabForm protocol](#labform-protocol)
- [Asset protocol](#asset-protocol)
- [Change workflow](#change-workflow)

## Data pipeline

```text
Excels/*.xlsx
  → Z_OtherProjects/Z_Tool/Excel2Cs/Excel2Cs.py
  → ExcelCs/*Form.cs
  → hand-written partials/controllers
  → global Form dictionaries, indexes, ID chain, JSON
  → GameSaveController orchestration
  → SaveAndLoad filesystem
```

Key roots:

- Common Forms: `Assets/Z_Level2/Z_DataSystem/Core/Form/Excels`.
- Product Forms: `Assets/GameSample/Forms/Excels`.
- Generated output: the sibling `ExcelCs` directory.
- Save orchestration: `Assets/GameSample/MyScripts/Main/GameSaveController.cs`.
- Filesystem helper: `Assets/Z_Level2/Z_DataSystem/Core/SaveAndLoad.cs`.
- Runtime Lab helpers: `Assets/Z_Level2/Z_DataSystem/Core/Form/LabForm.Runtime.cs`.

## Excel contract

Every workbook's first sheet uses this physical layout:

1. Row 1: field names; the first column is the primary key (`id` or `uid`).
2. Row 2: semicolon-separated generation flags.
3. Row 3: comments.
4. Row 4: C# types.
5. Row 5 onward: default and static data.

Require a primary-key `0` default row. Generated deserialization reads `_defaultData`; key `0` is not inserted into `DataById/DataByUid` and is not emitted by `GetJaByDatas()`.

Treat `.xlsx` as the workbook source file:

- Do not edit it with text patches.
- Preserve format, sheet structure, styles, formula caches, and its existing `.meta` GUID.
- Close Excel before generation; the generator skips `~$` lock files. A process holding a workbook open may still prevent moving or writing either format.
- Keep authoritative data on the first sheet because the generator uses the default sheet read.

Generation flags are case-sensitive:

| Flag | Meaning |
|---|---|
| `write` | Public setter and change hooks; serialize unless `unsave` |
| `unsave` | Exclude the field from JSON and use the default row on load |
| `index` | `Dictionary<Key,List<Data>>` index |
| `uniqueIndex` | `Dictionary<Key,Data>` index |
| `indexN` / `uniqueIndexN` | Composite tuple index for fields sharing N, where N is 0–9 |
| `override` | Reuse a parent property and pass it to the parent constructor |
| `medium` / `mass` | Primary-key pool maximum 10,000 / 1,000,000; default is 100 |
| `enum:T` | Convert an Excel token to `T.Value` |
| `data:...` | Resolve an ID through another Form and copy its data |
| `auto` | Reuse the previous static value when a cell is empty |
| `custom` | Emit cell content as raw C# expression |
| `sub` | Create a runtime child object with `new Type(this)` rather than a normal saved field |

## Generation and inheritance

- Never hand-edit `ExcelCs/*Form.cs`; use a sibling partial file for runtime behavior.
- `<Child>_<Parent>.xlsx` generates `ChildForm.Data : ParentForm.Data`.
- The filename parser uses only the first two underscore-separated components. Avoid extra underscores in Form names.
- Output is flat within one `ExcelCs`; duplicate child names in different source subfolders overwrite one another.
- Repeat every required parent-constructor field in a child sheet. Mark inherited fields `override` and preserve constructor order and type exactly.
- An inheritance tree shares the root Form's ID chain. Explicit IDs must be unique across the entire tree.
- Parent dictionaries and indexes contain derived instances. Filter with `data.GetType() == typeof(ExpectedForm.Data)` when exact ownership matters.
- Deleting or renaming a workbook does not delete its old generated `.cs/.meta`; audit and remove true orphans explicitly.
- Add a unique Unity `.meta` for every new workbook and hand-written partial. Never regenerate an existing GUID.

Generated index names use Python `capitalize()`, which lowercases the remaining characters:

```text
labId               → DatasByLabid
protoUid            → DatasByProtouid
lv1Lab              → DatasByLv1lab
name + protoUid     → DataByNameProtouid
```

Inspect the generated result instead of inventing an expected PascalCase API.

## Runtime Form behavior

- Construct new dynamic data with primary key `-1`, then call `AddData()` to allocate an ID.
- Do not mutate `id/uid` after registration. The generated primary setter does not repair the main dictionary or ID chain.
- `AddData()` silently returns for an existing ID; remove before deliberate replacement.
- Generated setters maintain secondary indexes. Do not bypass them or mutate indexed collection contents in ways the index cannot observe.
- `uniqueIndex` has weak conflict diagnostics; duplicate static keys may fail initialization and runtime duplicates can corrupt the unique lookup.
- `Clear()` removes static and dynamic rows and does not repopulate static Excel data in the same process.
- Generated `ClearAuto()` uses a strict pool-bound comparison. Avoid allocating or manually assigning the exact pool maximum; use a Form-specific helper where one exists.
- `Z_Chain.Chain` must preserve IDs returned through `PushId()` when it advances into previously uncreated sequential IDs. Repeated `ClearAuto()` and reload cycles must not reduce the remaining automatic-ID capacity.
- Excel2Cs `Copy()` calls `Copy()` on fields typed as `...Form.Data` and recursively clones List/Dictionary contents, including nested containers and Form rows inside tuples. Nested row IDs stay unchanged; only the outer `Copy(false)` requests a new ID. Null top-level collections retain the historical empty-collection behavior; nested nulls and null Form references are preserved. Value types and non-Form references (for example Unity assets) retain their existing value/reference behavior. Collection helpers use loops and pre-sized containers rather than JSON or LINQ. `Reset()` still reuses references.
- Every generated `Copy()` enters `Z_DesignStyle.DataCopyGuard` on the actual source instance. A reference back to an object on the current copying path throws `InvalidOperationException` with a type/instance path before recursive stack overflow. Same-type distinct instances and shared acyclic references are valid; shared rows are independently copied for each owning field. Per-thread active references are removed in `finally`/Dispose, including after exceptions. `BoxDataForm.Data.DeepCopy` uses the same guard.
- Generated copies call `CopyRuntimeStateTo` through the parent chain and an optional partial `OnCopy(Data copy)` hook. Keep unsaved mutable caches in hand-written partials: `GameParamForm.Data.OnCopy` clones an already materialized live Box without serializing it or eagerly parsing cold parameter strings. This preserves script-modified values even when `v` is stale. Do not add JSON-based deep copies to Object Product `ToProduct`: callers already use the generated owned `Copy(false)` before registration. `test_excel_copy_generation.py` compiles/executes real emitted methods across two mutually referencing Form types; `FormCopyRegression` covers native runtime copies, cycles, inherited caches, binding and calibrated performance comparison.
- `CopyStory*` commands keep their saved-payload cloning through `GetJoByData`/`GetDataByJo`, replace the copied name, reset its primary key to `-1`, and register with `AddData()`. This serialization boundary deliberately differs from generated Copy, which also copies unsaved constructor fields and partial runtime state.
- Scene `MapUnit.productInfo` intentionally keeps an ID link to its Product, while each unit's mutable `paramInfo` values must be copied from Product defaults. Never put the Product's parameter `Data` instances directly into a scene unit; otherwise a scene event can mutate the Product or another unit through a shared reference.
- `ObjectProduct_Product.xlsx` owns the `ObjectProductForm.Data.isWangTile` bool. Regenerate `ObjectProductForm.cs` after schema changes; old story JSON omitting the field loads with `false`. Object WangTile source frames come from each enabled `animClip` direction; generated GameTex mask variants are Story-scoped and reused with Tile WangTile assets across scene switches; Story reset releases them before clearing Forms. Mod changes to the flag, face type, or a real source frame rebuild only that Object product's generated variants and refresh its visible instances immediately; adding an empty/default frame waits for import rather than trying to parse the placeholder as an autotile sheet.
- `GetDatasByJa()` only creates Data objects. Register every loaded object with `AddData()`.
- Lazy `Init()` and `RuntimeInitializeOnLoadMethod` JSON registration are part of the generated runtime contract.

## JSON and story persistence

Generated JSON contains the primary key plus fields marked `write` and not marked `unsave`.

Story packages use `.png`. `TextureHelper.GetPNGWithExtraInfo` now delegates to `Z_Texture.PngPixelPackage`: GZip the existing UTF-8 filesystem JSON and store the compressed bytes directly as RGB8 pixels below a small cover preview. Export only standard IHDR/IDAT/IEND chunks, never tEXt/zTXt/iTXt. The first image scanline carries a 64-byte ZPKGPNG1 header: big-endian version=1 at 8, compression=1 at 12, raw length at 16, compressed length at 20, cover row count at 24, reserved zero at 28, and raw SHA-256 at 32. Data starts at row 1+coverRows. Decode rows on the CPU without creating/uploading a giant Unity texture; validate PNG framing/CRC/zlib, capacity/size limits, exact decompressed length and digest before JSON or filesystem writes. RGB8/RGBA8 noninterlaced lossless re-encoding may preserve data; resizing, palette reduction, screenshot or JPEG conversion does not. Legacy ZEXTRA tEXt packages remain read-only compatible through the bounded managed extractor (including the historical pre-IHDR ordering), never a native image decoder. Old .mod/.txt exports can be renamed to .png for import; old cover schema still needs migration.

`ImportPackage` still validates all relative file paths/Base64 and `sf` before writing. It normalizes Windows/Android path separators, stages Core under a unique nonnumeric temporary directory, and moves it to an unused positive local Story ID below `StoryForm.autoIdCnt`, skipping registered IDs and all existing story roots. Rewrite the persisted overview ID before publication, preserve other metadata/assets, and register only that row. Failed imports remove only their newly owned directory; never overwrite/delete another Core/Save/Cache or call LoadOverview from an asynchronous file-picker callback. The underlying Form/filesystem JSON protocol is unchanged.

`ImportPackageAsync(path, token)` keeps the same transaction: capture an absolute persistence root on the Unity thread, decode/validate/write a unique stage on a worker, parse the overview and allocate its ID on the Unity thread, write the new staged `sf` on a worker, then recheck cancellation/ID and synchronously move/register on the Unity thread. Wait for writers to finish before deleting only the request-owned stage/failed publication. Concurrent ID/target races fail without overwriting and can be retried. `PackageAsync(id, token)` captures `PngPixelPackage.CoverSnapshot` on the Unity thread and runs `SaveAndLoad.WritePackage` through `EncodeStreamWithCover` in Task.Run; snapshots contain immutable managed RGB pixels, never Texture/RenderTexture objects. WritePackage preserves the existing indented filesystem JSON/Base64 exactly, emits BOM-free UTF-8, leaves its output open, and retains only the current file rather than all asset bytes plus whole UTF-16/UTF-8 copies. The encoder hashes and GZips incrementally, enforces the same 512 MiB raw limit, and checks cancellation between files, writes and image rows. Windows Editor/Standalone use bcrypt CNG for standard SHA-256 (native handles/pinned buffers are released); unavailable native providers and non-Windows use SHA256.Create. Decode uses the same digest provider without weakening validation. Packed payloads >=1 MiB use standard stored-DEFLATE IDAT blocks to avoid recompressing GZip bytes, with bounded up-front output capacity; smaller images keep compressed IDAT to avoid blank-row overhead. Large images may be slightly larger due to uncompressed cover padding. Header version, checksums, synchronous APIs and import compatibility are unchanged.

Compatibility behavior:

- `SaveAndLoad.GetRealPath` distinguishes rooted paths with `Path.IsPathRooted` and resolves only relative keys under `perPath`. Resolution is idempotent for absolute Core/asset paths, including overview discovery/reload and Unpackage writes. Never use the presence of the product/company string `HlZy` to decide whether a path is absolute; installations and relative filenames may contain different names.

- `StoryForm.icon` is a Base64 `string` containing the cover image bytes (empty string means no cover). `Story.xlsx` is the schema authority. Encode once at cover selection and decode for display/package output; `SaveOverview` saves the string directly. Runtime loading accepts only this new schema, by design. Existing local `Core/sf` files were migrated once from the legacy `[{"v": byte}, ...]` representation with byte-for-byte image verification and backups; legacy external stories/packages require offline migration before use.
- `StoryForm.randomSeed` is the persistent string seed for deterministic story randomness. New stories receive a GUID seed; old stories missing the field use the key-0 empty default and `Random` falls back to the stable Story ID.
- Adding a field uses the key-0 default value for an old save.
- `CharacterProductForm.size` is a positive integer uniform model/collider scale. Keep its key-0 default at `1`; normalize it after load and before save, and never trust a scene-level `CharacterUnitForm.scale` over the owning Product.
- `PassTypeForm` is the story-owned registry for terrain traversal types. Persist it as `ptf`, load it before `MapTextureForm` and `CharacterProductForm`, and clear those consumers before clearing PassType. Every nonzero `int passType` and every element of `CharacterProductForm.passType` is a `PassTypeForm.id`; `0` means unrestricted and is not a registry row. Deleting a PassType must first reset matching MapTexture references to `0` and remove it from Character Product lists.
- `CharacterUnit_Unit.xlsx` and `TileUnit_Unit.xlsx` now save `passType` as `List<int>`. The scene unit list mirrors the Product's available types or the Tile's terrain-texture requirements; the runtime `passTypes` set is refreshed from that list. Never reduce a Tile's requirements to one ID. Before generated Tile deserialization, migrate legacy scalar `passType` JSON (`0` to an empty list, nonzero to a one-element list) because the generated list reader cannot read an integer.
- `MapTextureForm.enableFrontPart` gates its optional `frontPartTexs` animation list. Preserve the list while disabled so re-enabling restores the authored front animation; old stories inherit both fields from the key-0 MapTexture default.
- `MapTextureForm.frontIsWangTile` is a saved, independently selectable front-part autotile flag (default `false` for old stories). `FrontWangTileDic`, like `WangTileDic`, is `write;unsave`: first scene entry generates its texture IDs, later entries reuse unchanged sources, and Story reset clears them; never persist the generated IDs or replace the authored `frontPartTexs` list with split tiles.
- `ObjectProductForm.faceType` and `ObjectProductForm.animClip` own Object-facing persistence. `animClip` maps each `AnimDirecton` to its animation texture IDs; `MapModelForm.subUnitTexsName[0]` remains a compatibility mirror for old stories and shared model consumers. On load, seed missing directional clips from that legacy list; before save, mirror `Fixed` or `Up` according to `faceType`.
- `ObjectProduct_Product.xlsx` also saves `modelSizes` and `modelPositions` as `Dictionary<AnimDirecton,Vector3>` for the first model part's local dimensions and center offset. Fixed/Flexible use `Fixed`; FourDirection selects Up/Right/Down/Left from the current root Euler Y, without a saved/cached facing. `EnsureDirectionData` seeds every missing direction from legacy `model.subPrefabUnitScale[0]` / `subPrefabUnitPos[0]` before any direction is edited or mirrored; preserve explicit zero values and already configured directions. Normalize/load and save mirror only the default facing (`Fixed`, or `Up` for FourDirection) back to those legacy model fields for compatibility. Copy, Product conversion and JSON round trips must retain independent direction dictionaries; changing a facing must not mutate another facing or its authored prototype. These are model-local values, not scene Unit root position/scale, and do not belong in shared MapModel/Item data.
- `ObjectProduct_Product.xlsx` also owns the saved `boundsCollision` bool, with key-0 default `false`. Old stories missing this field remain unchanged; copying/resetting/JSON round-tripping must preserve it. `ModText_TextBase.xlsx` supplies its bilingual label. Do not put this Object-only option in the shared MapModel/Item schema.
- Renaming, deleting, or changing a field type is not migrated. The old key is silently ignored.
- `ObjectProduct_Product.xlsx` owns the saved Object-only `centerCollider` bool, with key-0/static defaults `true`. Missing old-save fields remain centered; new Objects explicitly pass `true` to the constructor. Copy/reset/JSON preserve an authored `false` rear anchor. Normal next story save writes the field; no offline rewrite is required. Keep it out of shared MapModel/Item data.
- Perform migration in this order: parse raw `JArray/JObject` → rewrite legacy keys/values → call generated deserialization → `AddData()`.
- Load referenced tables first. Reset dependents before their referenced roots.
- Verify two consecutive story loads because Forms are process-global registries, not fields owned by one Story object.

Story folders are relative to `Application.persistentDataPath`. On Windows, Unity uses `%USERPROFILE%\AppData\LocalLow\<CompanyName>\<ProductName>`; this project currently resolves to `C:\Users\<user>\AppData\LocalLow\HlZy\Z_Plugin`. Treat that as the standard local save root for diagnostics and manual migrations, but keep runtime code based on `Application.persistentDataPath` rather than a hard-coded user path:

```text
<story-id>/Core/    authored story data and assets
<story-id>/Save/    player/progress state
<story-id>/Cache/   logical scene working copies
```

Tile Y spacing changed from `1.5` to `1` world unit. For old story folders, run `.agents/skills/z-plugin-project-guide/scripts/migrate-tile-height.ps1 -StoryDirectory <story-folder>` while the game is closed. It converts numeric scene files in `Core/Cache/Save` plus world-height positions in `pf` and `msf`, backs up affected originals under `Backups/`, and is a no-op after conversion. Never divide logical `mapPos.y`, `targetScene.item2`, rotations, or local model geometry by `1.5`.

Play scene entry must keep `PlayManager.enable` false until the map-complete position callback and player creation finish. Capture first-entry status (`ProgressForm.sceneId == 0`) before asynchronous loading; first entry converts `targetScene.Item2` to world position, while resume retains saved `pos`. Reused player units must also be placed through `ApplyMove(..., teleport: true)` so data, transform and Tile indexes match the progress position before Enter events run.

When loading a Play `Save/pf`, preserve `characterUid=0` with an explicitly empty `teamActive`: party commands may deliberately undeploy every member while retaining reserves. Otherwise, if the current UID is `0` or refers to a missing Character Product, choose the first valid active-team UID, otherwise the first valid team UID. A missing/null active list retains this legacy repair behavior. Ensure the replacement belongs to both lists and rewrite only the repaired Save progress. Leave `Core/pf` and saves with no valid candidate unchanged; the Play scene can run without a current player character.

`GameSaveController` is a manual allow-list. For a new persistent Form, handle all of:

1. Stable short filename.
2. Core and/or Save serialization.
3. Load order.
4. Reset and story unload.
5. Legacy JSON migration.
6. Referenced-table ordering.
7. Package/import behavior if external files are involved.

Do not assume all registered Forms are persisted automatically.

`EventGlobalVariableForm` stores story-wide named Boxes as `egvf` in Core/Save. `GameSaveController.SaveGlobalVariables` participates in both save paths; `LoadGlobalVariables` participates in Core/Save loading and ResetStory. Clear the whole global Form before loading, including on null/missing files, so old stories start empty and consecutive stories never share state. Scene changes and event-controller Reset do not clear globals. Boxes are nested inline through existing generated JSON and need no separate Box registry or schema migration.

`ImageUiItem.xlsx` owns the shared PlayAsset image/text record. Its appended saved fields are `isText` (bool, default false), `text` (string, default empty) and `textScale` (float, default 1). Legacy JSON lacking them remains an image; an empty string can still be an explicit text record because kind is not inferred from content. Keep the existing `iuif` save/load/reset allow-list and UID/control-handle space for both kinds. Text uses the same saved position, rotation, opacity, progress and removal fields; `textScale` is relative to the current authored text prefab font size and text bounds are recomputed on binding. `Data.ctrl` stays a transient partial member and must not be serialized.

Before generated `iuif` deserialization, legacy rows without an `isText` token whose `oldOpacity`, `tarOpacity` and `opacityTime` are all zero normalize old/target opacity to one in memory. The former image Add used that zero/zero/zero initial state but never assigned its computed color, so it represented an opaque untouched image. Old explicit opacity commands stored `oldOpacity=1` and must not match; positive-time fades and every new-schema row (including deliberately transparent images/text) remain unchanged. Loading does not rewrite the source file; the next normal save persists the normalized record.

### Object Products

- `Story/Scene/ObjectProduct_Product.xlsx` owns Object configuration and prototype/runtime Product identities. `MapObject_MapBase.xlsx` is only a transient chooser row pointing at `objectUid`; no `mof` table is loaded or migrated. Load/save `oprf` through the generated Form APIs, with authored rows in Core and all rows in Save.
- Object Units use only `productInfo=(ObjectProduct UID,-1)` in `extra.pdt`. There are no `opv`/`sorv` runtime version branches, legacy MapObject-ID tuples, wrapper Product imports, template aliases or automatic Object-reference migration. Canonical handles are `$so$ProductUid$so$` and explicit `$so$u:UnitUid$so$`; old `p:`/`t:`/`$sop$` spellings are rejected. Repair invalid stored data directly rather than restoring runtime compatibility paths.
- `ObjectProductForm.Runtime.cs` and `unitBindings` are removed. `ObjectUnit.product` reads `ObjectProductForm.DataByUid` directly using `productInfo.Item1`; do not add another Unit-to-Product cache. `ObjectProductForm.Reference.cs` contains only prototype/concrete reference selection. Source prototypes and runtime Products keep independent nested configuration through generated `Copy(false)`.
- `ObjectUnit.BindProduct` creates a placement-owned copy in Play when needed, assigns the canonical link and prefab, and binds Product-owned parameter values. `GameSaveController.PrepareObjectProducts` prepares every current Cache scene before initial Save, including unloaded scenes, merging authored per-placement parameter overrides once. Reentry retains existing runtime Products and their changed values. This is current Play initialization, not old-format migration; Core is unchanged.
- `MapUnit.productInfo` caches only its parsed tuple, recognizes replacement `extra` content and writes explicit assignments back to `extra`. Register one `UnitForm.changeExtraAction` handler: when content changes, synchronously discard Product tuple state, parameter projections, local event dictionaries and combined-event caches. Do not clear actual Product parameter data. Getter/setting-controller initialization and subsystem-registration cleanup must keep the static handler registered exactly once. Event rendering/editor/serialization can replace `extra`; it is not immutable.
- A Play Object's `paramInfo` values point to its Product-owned parameters; additions are published through the setter. Cold Play reads go directly to Product values without decoding `extra.prm` again. Mod keeps independent per-placement parameter copies and saved overrides. Cache invalidation releases references, then getters rebuild from the current binding and mode.
- `ObjectProductForm.SceneUnits` provides the separate Product-to-Unit index. Concrete Product UIDs index Unit-UID-ordered placements; prototype queries visit the Product and its `DatasByProtouid` rows. `GetSceneUnit` picks the lowest live UID without creating a list; plural resolution retains sorted snapshots. Build once, then update only added/rebound/extra-edited Units and remove entries on Form removal; defer pre-write Extra notifications until the announced value is installed. Registry/reset/subsystem changes clear the index, and returned Units validate typed/root Form Data identity. Scalar Product resolution, scene-object commands and Product-wide WangTile refresh use it.
- Unit `extra.events` stores local additions. Resolve Product events before the local suffix, preserving inherited trigger type and saved IDs. Keep combined event caches runtime-only and rebuild them after Extra changes. Ordinary scene hiding/unload retains Products; explicit removal deletes only runtime Products. WangTile assets group by prototype UID; physical bounds cache by concrete UID.
- `ObjectProductRuntimeRegression` covers current-schema Cache preparation, parameter ownership, persistence, direct Product resolution and immediate Form-action invalidation of Product/parameter/event caches. `SceneObjectReferenceRegression` covers canonical references, old-spelling rejection, warm reverse-index reuse, duplicate placements, replacement and reset.

## LabForm protocol

- Represent every semantic label as `int labId`.
- Reserve `LabForm.NoneId == 0` for unclassified. Do not create an empty Lab row.
- Treat nullable UI selection as “all”; do not conflate it with `labId=0`.
- Define identity as `(lv1Lab, lv2Lab, lv3Lab, belong)` for new runtime labels. Static GameCmd Lab IDs are legacy aliases after flattening: several IDs may intentionally share one first-level identity.
- Set `belong` to `nameof(ConcreteForm)`, never a shared base Form name.
- Use `GetOrCreate`, `GetOrCreateDisplayName`, and `GetOrCreateForBelong`; do not duplicate construction logic.
- Display through `GetDisplayName`. When editing an existing display path, pass the current ID to avoid reparsing an unchanged path containing `/`.
- Prefer creating a new Lab and repointing consumers over mutating an identity path in place. There is no automatic orphan collection or cascade.
- Load Lab before consumers, save Lab before consumers, and clear consumers before Lab.
- Command categories belong to `Assets/GameSample/Forms/Excels/Story/Event/CmdLab_Lab.xlsx`, generating `Form.CmdLabForm.Data : Z_DataSystem.Form.LabForm.Data`. The common `Lab.xlsx` owns only generic label schema/defaults and contains no command rows. Inherited rows remain visible in the parent registry; command choosers and validation use the concrete CmdLab registry. Initialize CmdLab before loading/saving Labs; story `lf` excludes built-in CmdLab rows and skips legacy command rows on load, without rewriting authored event/asset labels.
- Preserve static CmdLab IDs `10001..10025` as a compatibility protocol. They have only `lv1Lab`; legacy aliases share paths, so neither sheet adds a unique path index. Canonical categories are `basic=10001`, `character=10007`, `effect=10013`, `item=10014`, `mission=10015`, `sceneObject=10016`, `skill=10020`, `ui=10022`; `scene=10026` owns the shared position/movement/rotation commands. `SetRotation` keeps the former Object rotation UID `100065` with a generic `var` unit argument; retired Character rotation metadata is replaced by a runtime alias. Add command categories in `CmdLab_Lab.xlsx`, never the common Lab workbook, and keep `GameCmdDataForm.labId`/concrete `belong=GameCmdDataForm` synchronized.
- Validate both ID existence and exact concrete `belong` for Asset, Product, MapBase, Effect, Mission, EventProgramData, and GameCmdData trees.

Legacy label migration remains:

- Flat string → `lv1Lab`; leave levels 2 and 3 empty.
- EventProgram labels are flat: legacy `category/type` is collapsed during `LoadEvent` to the original `category` (`lv1Lab`), discarding `type` (`lv2Lab`). Programs sharing a category appear together. The repository sample `ExtraAssets/2/Core/lf` uses the original first-level category names and retains the referenced IDs in `ef`.
- Existing `labId` with the wrong `belong` → clone the same path under the correct concrete Form.
- Historical Mission bodies that were never saved cannot be reconstructed from a label migration.

## Asset protocol

```text
AssetForm
├─ TexAssetForm
│  ├─ GameTexAssetForm
│  └─ StoryTexAssetForm
├─ AudioAssetForm
│  └─ StoryAudioAssetForm
├─ VideoAssetForm
│  └─ StoryVideoAssetForm
└─ GameObjectAssetForm
```

- The entire tree shares `AssetForm.idChain`; media types do not have independent ID spaces.
- `GameManager.Init` calls `GameSaveController.EnsureEmptyAssets` to create one concrete Story image/audio/video row, named empty and unclassified, at preserved IDs `AssetForm.autoIdCnt + 200000/200001/200002`. Repeated calls reuse existing rows. Configure each controller's `emptyAssetId` so `$i$$i$`, `$a$$a$` and `$v$$v$` resolve to those rows; reserved audio and video IDs serialize as `$a$$a$` and `$v$$v$`, while legacy numeric references remain readable. Expose the three IDs through `GlobalDefaultHelper` and identify protected defaults by ID/type, never just their mutable names. Story resets keep them. Empty audio/video have no payload, clip or backing file: keep their manifest rows, skip entity-file saving and leave their paths empty after loading. Ordinary media still uses ID-and-hash files. `EmptyMediaAssetsRegression` verifies creation, resolution, deletion/replacement protection, playback, reset and save/reload without user saves.
- Serialized reference markers include `$i$ID$i$`, `$a$ID$a$`, `$v$ID$v$`, and `$g$ID$g$`.
- Story asset manifests are `iaff`, `aaff`, and `vaff`; entity bytes/files live under `Core/ast/`.
- Story entity filenames include both the stable asset ID and the content hash: `$i$ID$HASH$i$`, `$a$ID$HASH$a$`, and `$v$ID$HASH$v$`. Serialized references keep the shorter `$i$ID$i$` / `$a$ID$a$` / `$v$ID$v$` markers. Before saving manifests, recompute each non-built-in asset hash from its current bytes or backing file so stale non-empty metadata cannot preserve an obsolete filename. Save a file only when its full ID-and-hash filename is absent, remove stale hash versions after saving, and fall back to the legacy ID-only filename while loading old stories.
- Many `bytes/path/asset` fields are `unsave`; rebind loaded data to the absolute `ast/` path.
- Import through `GameSaveController.AddStoryTex/AddStoryAudio/AddStoryVideo` so data is converted to the concrete Story subtype, IDs and Lab ownership are normalized, and asset events are emitted.
- Batch media selection goes through `TexController/AudioController/VideoController.SelectMultiple`. Windows Editor uses `AssetFilePicker`'s native multi-file dialog; supported Player platforms delegate to NativeGallery. Import every returned item through the matching `GameSaveController.AddStory*` method rather than registering the batch directly.
- Texture import accepts PNG/JPG/BMP/GIF/WebP. WebP is detected by its RIFF/WEBP signature as saved story asset filenames have no original extension. `TexAssetForm.Data.GetTex()` returns the first decoded GIF frame (or the first composited WebP frame) for static consumers and correct image dimensions; `GetAnimationFrames/GetAnimationSprites` provides all frames to `Img` for timed playback. Keep the original animated bytes in story asset storage so reloading can rebuild frames and sprites. The single-image and batch Windows Editor pickers both include `.webp`.
- `GifDecoder` supports both sequential and interlaced image descriptors, including single-frame GIFs. After LZW decoding, interlaced scanlines are placed in four passes (starts 0/4/2/1, strides 8/8/4/2), relative to the descriptor's own height; apply its top/left offset and then Unity's vertical flip. Short images can have empty passes. Keep source bytes unchanged: cached previews and animation frames share this decoder. `GifDecoderRegression` covers ordinary/interlaced rows, short/odd heights, offsets and existing animation timing; optional independent RGBA references validate real artwork pixel for pixel.
- Material animation uses `TextureAnimation.GetSample` and the asset's cached `GetAnimationDuration/GetAnimationTexture`: each GIF/WebP entry plays one internal cycle at its authored frame delays, holds its final frame for the clip interval, then advances. Cache static/animated detection and cumulative internal frame ends; `ClearRuntimeCache` must clear both when replacing bytes under the same ID. `SetRuntimeAnimationFrames` attaches Story-only generated WangTile frames after `AddGameTex` wraps the concrete row; these frames are not serialized and are rebuilt from original source bytes on cache invalidation or a new Story lifetime.
- Audio asset import recognizes `.mp3` and `.wav` files when scanning folders and in both the Windows Editor single-file and multi-file pickers; Player platforms continue to use NativeGallery's audio MIME filter. The single-file picker is used when replacing an existing audio asset.
- After a story's asset manifests load, repair missing character-animation `partTex` references through `GameSaveController.RepairMissingCharacterTextureReferences`. It creates one transparent Story texture per missing legacy ID and rewrites every affected `CharacterProductForm` entry so the repair persists on the next save.
- Do not register base `TexAssetForm.Data` directly as Story data or reuse a base Form's Lab ID under a Story subtype.
- Built-in map assets load through `Resources.LoadAll("Z_Map/")`; preserve their reserved names.

## Offline texture split and join

- For character action PNGs using the 托利亚 `newVersion` naming convention, use `前`, `后`, `左`, or `右` for facing direction. A standing frame has no suffix; suffix `1` denotes the left-hit reaction, `2` the left punch, and `3` and `4` the two walking frames (for example, `左2.png` faces left and punches). Identify direction and action from the image before renaming; preserve both walking frames.

### 整理图片

For each facing direction, use its standing PNG as the reference. Measure fully transparent rows above and below its nontransparent content. Scale each other frame's nontransparent content proportionally to match the standing frame's content height, and place it on the same-size transparent canvas so both empty-row counts match. Preserve the original horizontal center where possible, shifting only as needed to fit without clipping; if the scaled width cannot fit, stop and resolve the canvas constraint rather than distort or crop the image. For pixel art use nearest-neighbor resizing. Leave the standing reference unchanged and verify each resulting alpha bounding box against it.

- `Z_OtherProjects/Z_Tool/ImageSplit/切图.bat` launches `split_png.py` to grid-split all direct sibling PNGs. Prompt for positive row/column counts; optional positional CLI counts and `--directory` support automation. Boundaries use `i*size//count`, preserving every pixel and transparency without resizing; non-divisible cells differ by at most one pixel. Reject zero-area grids per image and continue the batch. Output is `output/<stem>_<rows>x<columns>/r01_c01.png` with versioned directories on repeats; never overwrite sources or scan output recursively. APNG uses its first frame.

- `ExtraAssets/使用中/贴图/test/texture_tool.py` owns the numbered PNG workflow invoked by `拆分.bat` and `拼合.bat`; the adjacent `PNG用途说明.txt` documents each input/output. Split overwrites 1–29 and 50–51; avoid a full split when refreshing only one preview would preserve authored edits.
- Inner-corner previews 6–9 use fixed `INNER_CORNER_LAYOUTS` matching `Assets/Z_Level3/Z_Map/Core/TileHelper.cs`'s actual part table and eight-neighbor masks. Keep the missing diagonal transparent. 22–25 reuse those same previews with only the central cell marked purple. Never choose corners by water color/seam similarity or a separate historical overwrite for 9's top-right.
- Expanded previews 18–19 are exactly 3×2 source cells and 20–21 exactly 2×3; start closed-interval crops at the boundary plus one to avoid a duplicate row/column. Join classifies 2–5 by aspect ratio, not exact pixel size, including scaled normal strips and older off-by-one expanded files.
- Split previews 14–17 crop the former full marked images into directional half-width/half-height regions: X/Y fractions are (1/6–2/3, 1/6–2/3), (1/3–5/6, 1/6–2/3), (1/6–2/3, 1/3–5/6), and (1/3–5/6, 1/3–5/6). Each preview's purple region becomes its middle third in both axes.
- Join chooses the central base from 101's middle 3×3 tile, else 100's whole image, else 50's whole image, else 1's middle 3×3 tile. Read 1 only for that final fallback. Resize the selected tile to the source sheet's central region.
- Optional authored 104–107 correspond to repaired 14–17. Crop their middle third proportionally, then overwrite the central tile's top-left, top-right, bottom-left, and bottom-right quadrants. These patches take precedence over 101 and affect only present quadrants. Using 101 or any of these patches saves the final central tile as 50 and places the same pixels into 0. Split must not generate or overwrite these optional inputs.

## Change workflow

1. Locate the workbook, generated Form, parent workbook, callers, and hand-written partial.
2. Verify the default row, types, flags, indexes, inheritance signature, and ID range.
3. Edit the binary workbook with a format-preserving spreadsheet workflow.
4. Run the narrow generator directly with Python; avoid interactive `run.bat` files containing `pause`.
5. Inspect every generated diff, API name, constructor, index, and orphan output.
6. Update business/UI callers and raw-JSON migrations.
7. Validate save, reload, story switch, old save, unclassified Lab, concrete Lab ownership, and imported media.

Common regeneration commands:

```powershell
python Z_OtherProjects/Z_Tool/Excel2Cs/Excel2Cs.py `
  "Assets/Z_Level2/Z_DataSystem/Core/Form/" `
  "Assets/Z_Level2/Z_DataSystem/Core/Form/" `
  namespace:Z_DataSystem

python Z_OtherProjects/Z_Tool/Excel2Cs/Excel2Cs.py `
  "Assets/GameSample/Forms/" `
  "Assets/GameSample/Forms/" `
  using:Z_UnitSystem.Form using:Z_Text.Form using:Z_DataSystem.Form `
  using:Z_Map.Form using:Z_Map using:Z_Ui.Form using:Z_Code.Form
```
