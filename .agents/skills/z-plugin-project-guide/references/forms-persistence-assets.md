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
Excels/*.xls
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

Treat `.xls` as a binary source file:

- Do not edit it with text patches.
- Preserve format, sheet structure, styles, formula caches, and its existing `.meta` GUID.
- Close Excel and remove no files while a `~$*.xls` lock file exists; the generator may try to parse the lock file.
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
- `<Child>_<Parent>.xls` generates `ChildForm.Data : ParentForm.Data`.
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
- `Copy()` copies List/Dictionary containers but not every nested object; `Reset()` can reuse references. Inspect nested mutability before treating either as deep copy.
- `GetDatasByJa()` only creates Data objects. Register every loaded object with `AddData()`.
- Lazy `Init()` and `RuntimeInitializeOnLoadMethod` JSON registration are part of the generated runtime contract.

## JSON and story persistence

Generated JSON contains the primary key plus fields marked `write` and not marked `unsave`.

Compatibility behavior:

- Adding a field uses the key-0 default value for an old save.
- `CharacterProductForm.size` is a positive integer uniform model/collider scale. Keep its key-0 default at `1`; normalize it after load and before save, and never trust a scene-level `CharacterUnitForm.scale` over the owning Product.
- `PassTypeForm` is the story-owned registry for terrain traversal types. Persist it as `ptf`, load it before `MapTextureForm` and `CharacterProductForm`, and clear those consumers before clearing PassType. Every nonzero `int passType` and every element of `CharacterProductForm.passType` is a `PassTypeForm.id`; `0` means unrestricted and is not a registry row. Deleting a PassType must first reset matching MapTexture references to `0` and remove it from Character Product lists.
- `MapTextureForm.enableFrontPart` gates its optional `frontPartTexs` animation list. Preserve the list while disabled so re-enabling restores the authored front animation; old stories inherit both fields from the key-0 MapTexture default.
- `MapObjectForm.faceType` and `MapObjectForm.animClip` own Object-facing persistence. `animClip` maps each `AnimDirecton` to its animation texture IDs; `MapModelForm.subUnitTexsName[0]` remains a compatibility mirror for old stories and shared model consumers. On load, seed missing directional clips from that legacy list; before save, mirror `Fixed` or `Up` according to `faceType`.
- Renaming, deleting, or changing a field type is not migrated. The old key is silently ignored.
- Perform migration in this order: parse raw `JArray/JObject` → rewrite legacy keys/values → call generated deserialization → `AddData()`.
- Load referenced tables first. Reset dependents before their referenced roots.
- Verify two consecutive story loads because Forms are process-global registries, not fields owned by one Story object.

Story folders are relative to `Application.persistentDataPath`. On Windows, Unity uses `%USERPROFILE%\AppData\LocalLow\<CompanyName>\<ProductName>`; this project currently resolves to `C:\Users\<user>\AppData\LocalLow\HlZy\Z_Plugin`. Treat that as the standard local save root for diagnostics and manual migrations, but keep runtime code based on `Application.persistentDataPath` rather than a hard-coded user path:

```text
<story-id>/Core/    authored story data and assets
<story-id>/Save/    player/progress state
<story-id>/Cache/   logical scene working copies
```

`GameSaveController` is a manual allow-list. For a new persistent Form, handle all of:

1. Stable short filename.
2. Core and/or Save serialization.
3. Load order.
4. Reset and story unload.
5. Legacy JSON migration.
6. Referenced-table ordering.
7. Package/import behavior if external files are involved.

Do not assume all registered Forms are persisted automatically.

## LabForm protocol

- Represent every semantic label as `int labId`.
- Reserve `LabForm.NoneId == 0` for unclassified. Do not create an empty Lab row.
- Treat nullable UI selection as “all”; do not conflate it with `labId=0`.
- Define identity as `(lv1Lab, lv2Lab, lv3Lab, belong)`.
- Set `belong` to `nameof(ConcreteForm)`, never a shared base Form name.
- Use `GetOrCreate`, `GetOrCreateDisplayName`, and `GetOrCreateForBelong`; do not duplicate construction logic.
- Display through `GetDisplayName`. When editing an existing display path, pass the current ID to avoid reparsing an unchanged path containing `/`.
- Prefer creating a new Lab and repointing consumers over mutating an identity path in place. There is no automatic orphan collection or cascade.
- Load Lab before consumers, save Lab before consumers, and clear consumers before Lab.
- Treat static GameCmd Lab IDs `10001..10025` as a stable serialized protocol.
- Map `GameCmdDataForm.labId` to Lab `lv1Lab/lv2Lab`. Update `Lab.xls` and `GameCmdData_CmdData.xls` together when adding command categories.
- Validate both ID existence and exact concrete `belong` for Asset, Product, MapBase, Effect, Mission, EventProgramData, and GameCmdData trees.

Legacy label migration remains:

- Flat string → `lv1Lab`; leave levels 2 and 3 empty.
- EventProgram `category/type` → `lv1Lab/lv2Lab`.
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
- Serialized reference markers include `$i$ID$i$`, `$a$ID$a$`, `$v$ID$v$`, and `$g$ID$g$`.
- Story asset manifests are `iaff`, `aaff`, and `vaff`; entity bytes/files live under `Core/ast/`.
- Many `bytes/path/asset` fields are `unsave`; rebind loaded data to the absolute `ast/` path.
- Import through `GameSaveController.AddStoryTex/AddStoryAudio/AddStoryVideo` so data is converted to the concrete Story subtype, IDs and Lab ownership are normalized, and asset events are emitted.
- Batch media selection goes through `TexController/AudioController/VideoController.SelectMultiple`. Windows Editor uses `AssetFilePicker`'s native multi-file dialog; supported Player platforms delegate to NativeGallery. Import every returned item through the matching `GameSaveController.AddStory*` method rather than registering the batch directly.
- After a story's asset manifests load, repair missing character-animation `partTex` references through `GameSaveController.RepairMissingCharacterTextureReferences`. It creates one transparent Story texture per missing legacy ID and rewrites every affected `CharacterProductForm` entry so the repair persists on the next save.
- Do not register base `TexAssetForm.Data` directly as Story data or reuse a base Form's Lab ID under a Story subtype.
- Built-in map assets load through `Resources.LoadAll("Z_Map/")`; preserve their reserved names.

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
