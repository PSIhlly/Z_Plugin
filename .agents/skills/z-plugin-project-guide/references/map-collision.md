# Map, movement, and collision constraints

Use this reference before changing movement, collision, trigger, navigation, tile association, or map resource loading.

## Source of truth

Read `<project-root>/Docs/MoveAndInteractSystem.md` with UTF-8 before algorithm changes. It documents the current call graph, SAT behavior, prior regressions, and non-obvious invariants. Confirm the document against the current implementations in:

- `Assets/Z_Level0/Z_Math/Graph.cs`.
- `Assets/Z_Level1/Z_Mesh/Mesh.cs`.
- `Assets/Z_Level3/Z_Map/Core/MapUpdateController.cs`.
- `Assets/Z_Level3/Z_Map/Core/MapUtilController.cs`.
- `Assets/Z_Level3/Z_Map/Core/MapUnit.cs`.
- `Assets/Z_Level3/Z_Map/Core/Character/CharacterUnit.cs`.
- `Assets/Z_Level3/Z_Map/Core/Object/ObjectUnit.cs`.

## Shared movement and interaction pipeline

```text
CharacterUnit.Move(dir)
→ inspect nearby tiles and units
→ MapUpdateController.CheckCollide(..., CollideOnly)
→ Mesh.MeshIntersectMesh
→ Graph Sphere/Cube intersection
→ truncate movement and possibly enqueue slide movement
```

Interaction uses the same geometry with `TriggerOnly`, then defers Enter/Exit/Cross effects through the map update lifecycle.

Preserve the semantic split:

- `CollideOnly`: physical obstacle meshes (`isTrigger=false`).
- `TriggerOnly`: event trigger meshes (`isTrigger=true`).
- `All`: explicit callers that need both.

## Non-negotiable collision invariants

- Keep the bounded nearby-tile traversal and the current lower-layer skip rule.
- Do not reject collision candidates by Tile-anchor distance. A Tile Collider may cross logical layers (notably `mapground` extending downward); the actual `CollideOnly` Mesh AABB/SAT result is authoritative.
- `CaptureCast` gathers candidates across the complete swept segment, not only endpoint cells. In each crossed X/Z column it checks every registered Tile height, because a higher Tile's physical Collider may extend down into the queried ray height; `CollideOnly` Mesh sweep remains the final hit test.
- Merge avoidance normals through `MergeAvoidDir` / `MergeAvoidDirRange`; do not replace them with blind `AddRange` or averaging.
- Compute sphere-versus-OBB separation and push direction from the OBB closest point at contact. Do not substitute an AABB closest point for rotated/sloped boxes.
- Use AABB only for broad-phase rejection where the current algorithm does so.
- Preserve axis deduplication and the nearest-point separation axis that prevents corner false positives.
- Keep `GetPushDirByFace` only as the existing degeneracy fallback; it previously chose the wrong face at corners.
- Preserve the movement queue and first-attempt guard that bound repeated slide attempts.
- Do not erase positive Y slide/climb components through unconditional tile snapping.
- For non-player character auto-facing, accumulate actual horizontal movement across `Move` calls. Update toward the latest actual movement direction only after the total is strictly greater than `abs(speed) / 3`, then reset the accumulator; blocked or vertical-only movement keeps the prior facing, and `forceEuler` resets the accumulator.
- Apply gravity only when `HasGroundContact()` is false; preserve the lower-contact threshold and tolerance unless the whole grounding model is retuned.
- Preserve `IntersectType` meanings: `None`, `In`, `Out`, `Cross`, and `Inner`.

## Trigger and ownership rules

- Maintain `Unit.collidingUnitUid` so Enter/Exit is not emitted repeatedly.
- Defer managed trigger events as the current `MapUpdateController` expects; do not mutate map collections while collision enumeration is active.
- During non-teleport movement, test each moving MapUnit against candidate Tile `CollideOnly` meshes and route contact state through the same deferred `Unit.OnEnter` / `Unit.OnExit` pipeline. On Enter, the moving unit executes `onTileTouchEvent`; this is distinct from map-edge `onBoundaryTouchEvent`.
- Treat teleport movement separately: `ApplyMove(..., teleport: true)` skips movement Trigger sweep entirely.
- Move and pool entities through Map/Unit ownership. Do not permanently instantiate parallel objects outside `InstancePoolManager`.
- Update tile association through the existing `ApplyMove` flow so spatial lookups, data position, GameObject transform, and triggers remain synchronized.
- Keep `characterTileDic` as the single owner/support association used by `belongTile`, visibility, fog, and editor placement. Keep broad-phase character coverage in `characterOverlapTileDic`; only register characters that already have an owner, refresh after add/load/move/rotation/scale changes, and clear on remove/end.
- Refresh character coverage by comparing the full queried footprint with existing links: remove departed Tiles and add newly covered Tiles in both directions. Do not delete/rebuild unchanged links or use center-cell changes as the refresh condition.
- Derive character broad-phase candidates from the actual `CollideOnly`/`All` Mesh swept AABB. Movement, grounding, trigger scans, CaptureCast, and object pushing must query the overlap index rather than assuming the owner Tile contains the whole character.
- `CharacterProductForm.size` drives `CharacterUnitForm.scale = Vector3.one * max(1, size)`, so model, physical Collider, Trigger, overlap index, and navigation clearance scale together.
- Before applying non-teleport character movement, clamp the target center inside the map's horizontal outer boundary by `max(1, CharacterUnitForm.scale.x) * 0.2`, matching `CharacterProductForm.size * 0.2`. Search across contiguous walkable Tiles when the inset spans multiple cells. Use the same distance for character `BoundaryTouch`. Object movement does not use a fixed inset: emit Object `BoundaryTouch` only when the requested target center actually touches or leaves the map area. General `InArea(Vector3)` checks retain the base `0.2` inset unless the caller explicitly supplies another distance.
- Navigation remains a shared center graph, but each character query supplies its actual horizontal Collider radius. Validate every footprint offset transition and expand path smoothing by the same clearance; size `1` must preserve the base graph behavior.
- Terrain pass requirements come from the three MapTexture IDs already persisted in `TileUnitForm.texDic` slots `0..TERRAIN_LAYER_MAX-1`. On scene entry, resolve each texture's nonzero `MapTextureForm.passType` into `TileUnit.passTypes`; `TileUnitForm.passType` is only the first-ID compatibility/cache field. Copy the complete set into `NavUnit.passTypes`. A character may enter or route through a Tile only when its runtime set, rebuilt from `CharacterProductForm.passType`, contains every required ID. Pass requirements use only the Tile containing the character/navigation center; character size, Collider/Trigger footprint, and navigation-clearance offsets do not add pass requirements from neighbouring Tiles. When a manual movement target belongs to a disallowed Tile, use the pass-aware closest-existing-Tile search to place the center at the nearest legal position. `0` never adds a requirement.

## Map resource contracts

Built-in map content loads with `Resources.LoadAll("Z_Map/")` from `Assets/Z_Level3/Z_Map/Sample/Resources/Z_Map`.

Preserve identifiers used as parsing and lookup protocols:

- `MapPrefab$...` for built-in map prefabs.
- `MapTexture$...` for named built-in textures.
- `runtime$...` for runtime prefab names.
- Hashed/high-range built-in asset IDs initialized by `GameManager`. The hash is derived from the full resource name, so a `MapPrefab$...` rename changes its runtime asset ID; migrate persisted `prefabName` values and audit any persisted numeric references.
- Built-in terrain choices come from `MapTerrainForm` and resolve through matching `MapPrefab$...` resources. `mapground` and `mapfloor` use `step=0` and ModScene applies them to one tile without direction logic; `mapslope` uses `step=1` and follows the directional slope branch.
- Treat `mapground` as solid volume for navigation: mark other navigation cells whose walkable cell volume overlaps its `CollideOnly` mesh as blocked, while keeping the owning `mapground` cell walkable on top. Cache each `mapground` Tile's covered NavUnit keys across periodic navigation rebuilds; rebuild that entry only when its prefab reference/name, map position, world position, rotation, scale, or map cell size changes. Adding/removing a navigation Tile or moving a navigation-cell center invalidates the coverage cache because it can change another `mapground` Tile's result. Do not apply this solid-volume rule to `mapfloor` implicitly.
- Navigation treats obstacle geometry rising less than `0.2` world units above the affected Tile's walkable ground as passable: `mapground` coverage uses the lowest sampled ground height (or the cell center when no sample exists), while ordinary Object height updates compare against the sampled directional ground. Physical collision and the separate step/slope link threshold are unchanged; equality at `0.2` is not exempt.
- After ModScene changes a terrain tile's prefab or transform, call `MapUpdateController.UpdateSingleOne` in the same placement operation so the pooled instance refreshes immediately. Collision mesh caching must also invalidate on prefab, position, rotation, or scale changes.
- `MapUnit.GetMeshes` bakes origin-relative geometry for the current prefab/rotation/scale and reuses world Mesh/vertex arrays during translation. World points are recomputed from cached offsets, not accumulated deltas. `UpdateSingleOne` invalidates the geometry explicitly for same-name prefab edits; consumers must treat returned meshes as live, read-only cache data rather than snapshots.
- Tile base layers resolve a configured but missing `MapTextureForm` ID to the first registered Map Texture by ascending ID (the default story's first entry is `grass`). An absent `texDic` key means an intentionally empty layer: disable its base/front Renderers and cancel their animation timers, without falling back or letting a retained mask enable the base Renderer. `TextureOnly` removes only the selected logical layer's base-texture key and refreshes the Tile immediately; other layers and mask selections remain unchanged. `GameMapController.ShowFinalMat` removes missing texture-asset IDs from animation lists and uses `GlobalDefaultHelper.DefaultTexId` (or white texture as the final fallback) instead of indexing a missing asset.
- Tile rendering keeps logical terrain layers separate from Renderer slots: `texDic[0..2]` drives base Renderers `0..2`, and `texDic[3..5]` remains the mask selection for those base layers. `TileInstance.renderers` and `InstancePool` include inactive child Renderers so an authored inactive layer cannot shift slot indices; pool refresh restores each Renderer GameObject's prefab active state, and applying a valid texture activates that layer GameObject. When a selected `MapTextureForm` enables its front part and the prefab exposes Renderers `3..5`, each front Renderer uses the corresponding layer's `frontPartTexs`; otherwise that front Renderer is disabled. `ModSceneMain` display states remain logical layers `0/1/2` and control the normal/front pairs `0+3`, `1+4`, and `2+5` cumulatively. For a finite Mod display ceiling `n`, each visible pair below `n` writes half of its Renderer-specific initial `_LightSensitivity` through the existing MaterialPropertyBlock, while pair `n` keeps its initial value; cache the initial value once so repeated layer switches never compound the reduction. Event and Play use `int.MaxValue` to restore all initial values. `MapInstance.animTimer` must cover all six Renderer slots.
- `isWangTile` and `frontIsWangTile` apply the same 4-by-6-quarter RPG Maker autotile splitting independently. `Main2StoryManager` splits every authored animation source frame, groups the generated textures by the same 256 neighbour masks, and registers each mask's ordered frame list with `GameMapController`; `WangTileDic`/`FrontWangTileDic` retain the first valid frame for compatibility and static sampling. Base and front select their variants from the same logical layer's eight-neighbour MapTexture-ID mask, never from Renderer `3..5`/mask keys, and play multi-frame variants with `MapTextureForm.animTimeInterval`. Generated GameTex frames and animation caches share the existing scene unload/reload cleanup lifecycle. In Play, every non-character map-texture animation (ordinary/Wang Tile base and front layers, Objects, and Items) derives its frame from `ProgressForm.seconds`, so newly shown or pooled instances join the existing global phase and pause with game time. `Z_Time.TimeManager.animationTimeGetter` is the assembly-safe clock boundary and `StartAnimationTimer` schedules against it; UGC editing falls back to `Time.time` so animation previews continue. Character animation deliberately keeps its appearance/action-local timer.
- Adding, editing, or removing a Tile refreshes the same-height 3-by-3 Tile neighbourhood after the change is applied, so base and front WangTile masks immediately reflect both gained and lost neighbours.
- MapObject WangTile uses the same 8-neighbour bit order and `TileHelper` 4-by-6 quarter-sheet variants as Tile WangTile. A neighbour must have the same MapObject product ID and touch the Object's X/Z visual-footprint edge or corner (including root/model scale), with overlapping Y volume; gaps and other heights do not connect. Object add, remove, move, boundary-touch move, and explicit `UpdateSingleOne` refresh affected visible neighbours and the moved Object. `MapEventType.Refresh` is an appearance/footprint refresh only, not a story lifecycle trigger.
- `GameMapController.GetTileLayerTextures` is the shared Tile-data resolver for scene rendering and minimap sampling: neighbour masks use the same height and logical layer, with explicit matching texture keys. `GetTileLayerTexture` samples the selected WangTile variant or first registered ordinary frame without requiring a visible/pool-backed Tile instance; configured missing MapTexture IDs retain the first-MapTexture fallback, while absent keys stay transparent. The generated minimap still samples only base layer `0` (not front Renderers, upper overlay layers, or masks). Pack texture rows by increasing map Z without a whole-image Y flip, so autotile edges keep their original UV orientation; initial and incremental unlock pixels use those same bounds independently of texture availability.
- Effects rendered with `MapPrefab$img` stay horizontal (`X=90°`) in Overhead mode and whenever `EffectForm.ground` is enabled. In Isometric side view, non-ground effects use a vertical plane (`X=0°`) and multiply local Y scale by `sqrt(2)` (about `1.414`) to compensate its projected height; per-effect rotation continues on local Z.
- In Isometric mode, `PerspectiveKeeper` aligns cube-backed image planes to the scaled root cube's Y-Z diagonal: pitch is `atan2(abs(root.lossyScale.z), abs(root.lossyScale.y))`, and the image height follows that same diagonal length. Root scale changes must refresh both values. Sphere-backed images remain diameter-sized squares and therefore keep a 45-degree diagonal pitch.
- Map occlusion reaches `DisFadeCode.shader` through `Unit.VisDegree`; `_Show` multiplies `_Alpha`, while `_Cutoff` only clips texture pixels. In Play, ordinary occluding high Tiles and high-only Objects use `_Show=0.5`; a higher map layer upgrades to `_Show=0` only when all four cardinal X/Z neighbours around the player contain Tiles on that same layer. The Tile directly above the player is not required and may be empty. Eligible current-layer Objects use `_Show=0.5`. An Object whose visual footprint covers both a fully occluding high Tile and any current-layer Tile stays half transparent, irrespective of its owner/association order. Non-occluders retain `_Show=1` and departed occlusion restores normally.
- `GlobalSettings.MOD_HIGH_LAYER_HALF_TRANSPARENT` is enabled by default. While `DynamicGlobalSettings.playing` is false (Mod mode), every visible Tile above the camera target's current map layer uses `_Show=0.5`, and the normal Play high-layer occlusion BFS is skipped. Items and Characters inherit that half-opacity from their owner Tile. Objects also use visual-footprint candidates, so a higher-layer Object whose owner is outside the visible Tile set still becomes half transparent. Current/lower Tiles remain opaque, current-layer Object rules remain in force, and switching to Play restores the normal occlusion path.
- High-layer occlusion BFS still traverses the connected high-Tile component inside the current configured view once a valid seed is found. Before applying either half or full transparency to a high Tile `(x, highY, z)`, project it to `(x, currentY, z + highY - currentY)` and query `NavigationController.IsBaseWalkable`; a missing/null `NavUnit` or one without links means that high Tile stays fully opaque and contributes no high-layer Object candidate. This projection gate deliberately ignores `passTypes`. Tile front parts use Renderer slots `3..5` and have an additional independent projection at `(x, currentY, z + highY - currentY - 1)`; when that cell is not base-walkable, only the front Renderers stay opaque while slots `0..2` and attached units keep the ordinary Tile result. In Isometric mode, a seed is valid when `forwardDistance <= highTile.mapPos.y - character.mapPos.y + 1`; the extra cell is a fixed Z-direction tolerance, not a fade band. X remains aligned with the character unless `OVERLAY_HIDE` supplies its existing tolerance. A normal valid component is half transparent (`_Show=0.5`) only for Tiles whose horizontal X/Z Euclidean distance from the player's current Tile is at most `3`; farther Tiles remain opaque and do not contribute half-transparent Object candidates. If all four cardinal neighbour positions contain Tiles on the inspected higher layer, that layer uses full transparency and BFS starts from all four neighbours before the normal seeds; full transparency continues across eligible Tiles in the whole connected component and deliberately treats an empty center-above-player position as enclosed. Isolated components beyond the Z tolerance remain opaque. Overhead keeps same-X/Z seeding (plus `OVERLAY_HIDE` tolerance), and the current-layer Object scan retains its full configured view range.
- `UpdateVision` first finishes Tile degrees (baseline + full-view BFS). Only occluding high Tiles query the Object reverse index to collect deduplicated visual-footprint candidates and preserve the lowest degree when an Object spans half- and fully-transparent components; current-layer half-opacity overrides are applied last. These candidates work outside `curObjectLst`. Afterwards traverse `curObjectLst/curItemLst/curCharacterLst` once to inherit the owner's final degree (or `1` when absent), without overwriting Object overrides. Keep these lists synchronized through `UpdateSingleOne`/view refresh/remove. Never expand all three attachment indexes per Tile during baseline/BFS. Candidate/state buffers are reused and cleared at End. The public `SetGroupVision` remains immediate.
- `DisFadeCode` represents `_Alpha * _Show` with stable screen-space dither coverage and writes depth, rather than accumulating conventional alpha blending. This keeps overlapping instances at the maximum visible coverage while allowing the nearest surface to win. Its `ShadowCaster` pass ignores `_Show`; therefore fully hidden Tiles and fully/half-transparent Objects remain active shadow casters. Shadow pixels still clip against both `_AlphaTex` and the base texture alpha, so masks and transparent texture pixels do not cast solid rectangular shadows. This requires the authored Renderer to keep shadow casting enabled.
- Visibility reads use `DoubleDictionary.TryGet/TryGetFirst` without creating empty keys; legacy `Get/GetFirst` still create missing lists but use one lookup on hits. `MapInstance.ApplyVision` compares the last applied degree/visibility/display layer/owner and reuses one MaterialPropertyBlock; always read the renderer block before changing `_Show` to preserve textures and masks. Bound units still refresh independently, pool enable/disable invalidates the state, and map End clears collection buffers. Do not restore every unit to 1 before applying occlusion each frame.
- Tile removal keeps `MapInfo.maps` and `mapXZ2Y` synchronized: removing the final height at an X/Z coordinate also removes that empty height-set key. `ShowAndAddLst` enumerates the actual sorted heights rather than treating `Min..Max` as dense, and skips any stale height whose exact Tile data is absent. This keeps Mod erase-all and sparse vertical columns safe during the next forced or camera-driven view refresh.
- Visible Tile/Object/Item/Character collections are unordered `HashSet`s. `FreshMap` clears and records the entering/leaving sets directly: identical view bounds return before scanning visible Tiles, ordinary movement queries only the non-overlapping entering slabs, and `ShowAndAddLst` calls `Show` only when a Tile is newly inserted. First, forced, or wholly non-overlapping refreshes scan the complete current view once. Related-unit visibility is derived from whether any associated Tile is showing, so entering/leaving set enumeration order cannot hide a unit that still overlaps the view. Do not reintroduce full-list copies followed by repeated `List.Remove` difference calculations.
- `objectTileDic` is the Object visual-footprint index. For combined runtime prefabs, derive it from direct-child transforms that preserve `MapModelForm.subPrefabUnitPos/subPrefabUnitScale`, then apply the Object's saved `position/euler/scale`; use Renderer local bounds only as the non-combined-prefab fallback, never Collider bounds or `colliderScale`. Refresh full multi-tile coverage on add/load/move/rotation/remove. The index also supplies collision broad-phase candidates because `colliderScale` never exceeds `1`; narrow-phase still uses Collider meshes. Use it for display and the Isometric “n rows toward smaller Z and rendered height > n” occlusion lookup.

Do not rename these resources as a cosmetic cleanup.

## Required validation

For collision or movement changes, add focused numeric checks where practical and manually test at least:

- axis-aligned wall collision;
- rotated/sloped box collision;
- sphere near a box corner without contact;
- wall sliding and corner blocking;
- ground contact, falling, and upward slope movement;
- overlapping multiple obstacles with conflicting normals;
- Trigger Enter, Exit, Cross, Inner, and teleport behavior;
- tile-boundary movement and map-edge clamping;
- repeated movement across two logical story scenes;
- character sizes `1`, `2`, and `3` against walls, other characters, Object/Trigger ranges, map edges, one-cell gaps, and sufficiently wide navigation routes.

Record before/after positions, `touchTime`, avoidance normals, and `IntersectType` when diagnosing a regression. Do not “fix” one geometry case without replaying the established corner and slope cases.
