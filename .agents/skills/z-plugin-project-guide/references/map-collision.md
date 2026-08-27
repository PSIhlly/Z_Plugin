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
- Derive character broad-phase candidates from the actual `CollideOnly`/`All` Mesh swept AABB. Movement, grounding, trigger scans, CaptureCast, and object pushing must query the overlap index rather than assuming the owner Tile contains the whole character.
- `CharacterProductForm.size` drives `CharacterUnitForm.scale = Vector3.one * max(1, size)`, so model, physical Collider, Trigger, overlap index, and navigation clearance scale together.
- Before applying non-teleport character movement, clamp the target center inside the map's horizontal outer boundary by `max(1, CharacterUnitForm.scale.x) * 0.2`, matching `CharacterProductForm.size * 0.2`. Search across contiguous walkable Tiles when the inset spans multiple cells. Use the same distance for character `BoundaryTouch`. Object movement does not use a fixed inset: emit Object `BoundaryTouch` only when the requested target center actually touches or leaves the map area. General `InArea(Vector3)` checks retain the base `0.2` inset unless the caller explicitly supplies another distance.
- Navigation remains a shared center graph, but each character query supplies its actual horizontal Collider radius. Validate every footprint offset transition and expand path smoothing by the same clearance; size `1` must preserve the base graph behavior.

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
- `GameMapController.ShowFinalMat` removes missing texture IDs from animation lists and uses `GlobalDefaultHelper.DefaultTexId` (or white texture as the final fallback) instead of indexing a missing asset.
- Effects rendered with `MapPrefab$img` stay horizontal (`X=90°`) in Overhead mode and whenever `EffectForm.ground` is enabled. In Isometric side view, non-ground effects use a vertical plane (`X=0°`) and multiply local Y scale by `sqrt(2)` (about `1.414`) to compensate its projected height; per-effect rotation continues on local Z.

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
