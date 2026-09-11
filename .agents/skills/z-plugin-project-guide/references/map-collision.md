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
- Terrain pass requirements come from the three MapTexture IDs already persisted in `TileUnitForm.texDic` slots `0..TERRAIN_LAYER_MAX-1`. On scene entry, resolve each texture's nonzero `MapTextureForm.passType` into `TileUnit.passTypes`; `TileUnitForm.passType` is only the first-ID compatibility/cache field. Copy the complete set into `NavUnit.passTypes`. A character may enter or route through a Tile only when its runtime set, rebuilt from `CharacterProductForm.passType`, contains every required ID. Manual movement applies the same rule to the character Collider footprint; `0` never adds a requirement.

## Map resource contracts

Built-in map content loads with `Resources.LoadAll("Z_Map/")` from `Assets/Z_Level3/Z_Map/Sample/Resources/Z_Map`.

Preserve identifiers used as parsing and lookup protocols:

- `MapPrefab$...` for built-in map prefabs.
- `MapTexture$...` for named built-in textures.
- `runtime$...` for runtime prefab names.
- Hashed/high-range built-in asset IDs initialized by `GameManager`. The hash is derived from the full resource name, so a `MapPrefab$...` rename changes its runtime asset ID; migrate persisted `prefabName` values and audit any persisted numeric references.
- Built-in terrain choices come from `MapTerrainForm` and resolve through matching `MapPrefab$...` resources. `mapground` and `mapfloor` use `step=0` and ModScene applies them to one tile without direction logic; `mapslope` uses `step=1` and follows the directional slope branch.
- Treat `mapground` as solid volume for navigation: mark other navigation cells whose walkable cell volume overlaps its `CollideOnly` mesh as blocked, while keeping the owning `mapground` cell walkable on top. Do not apply this solid-volume rule to `mapfloor` implicitly.
- After ModScene changes a terrain tile's prefab or transform, call `MapUpdateController.UpdateSingleOne` in the same placement operation so the pooled instance refreshes immediately. Collision mesh caching must also invalidate on prefab, position, rotation, or scale changes.
- `MapUnit.GetMeshes` bakes origin-relative geometry for the current prefab/rotation/scale and reuses world Mesh/vertex arrays during translation. World points are recomputed from cached offsets, not accumulated deltas. `UpdateSingleOne` invalidates the geometry explicitly for same-name prefab edits; consumers must treat returned meshes as live, read-only cache data rather than snapshots.
- Tile base layers resolve a missing `MapTextureForm` ID to the first registered Map Texture by ascending ID (the default story's first entry is `grass`). `GameMapController.ShowFinalMat` removes missing texture-asset IDs from animation lists and uses `GlobalDefaultHelper.DefaultTexId` (or white texture as the final fallback) instead of indexing a missing asset.
- Tile rendering keeps logical terrain layers separate from Renderer slots: `texDic[0..2]` drives base Renderers `0..2`, and `texDic[3..5]` remains the mask selection for those base layers. `TileInstance.renderers` and `InstancePool` include inactive child Renderers so an authored inactive layer cannot shift slot indices; pool refresh restores each Renderer GameObject's prefab active state, and applying a valid texture activates that layer GameObject. When a selected `MapTextureForm` enables its front part and the prefab exposes Renderers `3..5`, each front Renderer uses the corresponding layer's `frontPartTexs`; otherwise that front Renderer is disabled. `ModSceneMain` display states remain logical layers `0/1/2` and control the normal/front pairs `0+3`, `1+4`, and `2+5` cumulatively. `MapInstance.animTimer` must cover all six Renderer slots.
- Effects rendered with `MapPrefab$img` stay horizontal (`X=90°`) in Overhead mode and whenever `EffectForm.ground` is enabled. In Isometric side view, non-ground effects use a vertical plane (`X=0°`) and multiply local Y scale by `sqrt(2)` (about `1.414`) to compensate its projected height; per-effect rotation continues on local Z.
- Map occlusion reaches `DisFadeCode.shader` through `Unit.VisDegree`; `_Show` multiplies `_Alpha`, while `_Cutoff` only clips texture pixels. Obstructing high ground and eligible Objects use `_Show=0.5`.
- `UpdateVision` first finishes Tile degrees (baseline + BFS), then traverses `curObjectLst/curItemLst/curCharacterLst` once to inherit the owner's final degree (or `1` when absent). Keep these lists synchronized through `UpdateSingleOne`/view refresh/remove. Object visual-footprint overrides take precedence and also cover candidates outside those lists. Never expand all three attachment indexes per Tile during baseline/BFS. The public `SetGroupVision` remains immediate.
- Visibility reads use `DoubleDictionary.TryGet/TryGetFirst` without creating empty keys; legacy `Get/GetFirst` still create missing lists but use one lookup on hits. `MapInstance.ApplyVision` compares the last applied degree/visibility/display layer/owner and reuses one MaterialPropertyBlock; always read the renderer block before changing `_Show` to preserve textures and masks. Bound units still refresh independently, pool enable/disable invalidates the state, and map End clears collection buffers. Do not restore every unit to 1 before applying occlusion each frame.
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
