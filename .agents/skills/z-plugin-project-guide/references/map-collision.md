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

- Keep the bounded nearby-tile traversal. Character movement checks the local 3×3×3 neighborhood and skips lower-layer tiles according to the current code.
- Preserve the current distance prefilter (historically 1.3 units / squared 1.69) unless profiling and gameplay tests justify a coordinated change.
- Merge avoidance normals through `MergeAvoidDir` / `MergeAvoidDirRange`; do not replace them with blind `AddRange` or averaging.
- Compute sphere-versus-OBB separation and push direction from the OBB closest point at contact. Do not substitute an AABB closest point for rotated/sloped boxes.
- Use AABB only for broad-phase rejection where the current algorithm does so.
- Preserve axis deduplication and the nearest-point separation axis that prevents corner false positives.
- Keep `GetPushDirByFace` only as the existing degeneracy fallback; it previously chose the wrong face at corners.
- Preserve the movement queue and first-attempt guard that bound repeated slide attempts.
- Do not erase positive Y slide/climb components through unconditional tile snapping.
- Apply gravity only when `HasGroundContact()` is false; preserve the lower-contact threshold and tolerance unless the whole grounding model is retuned.
- Preserve `IntersectType` meanings: `None`, `In`, `Out`, `Cross`, and `Inner`.

## Trigger and ownership rules

- Maintain `Unit.collidingUnitUid` so Enter/Exit is not emitted repeatedly.
- Defer managed trigger events as the current `MapUpdateController` expects; do not mutate map collections while collision enumeration is active.
- Treat teleport movement separately: passing a zero direction intentionally avoids normal Cross semantics.
- Move and pool entities through Map/Unit ownership. Do not permanently instantiate parallel objects outside `InstancePoolManager`.
- Update tile association through the existing `ApplyMove` flow so spatial lookups, data position, GameObject transform, and triggers remain synchronized.

## Map resource contracts

Built-in map content loads with `Resources.LoadAll("Z_Map/")` from `Assets/Z_Level3/Z_Map/Sample/Resources/Z_Map`.

Preserve identifiers used as parsing and lookup protocols:

- `MapPrefab$...` for built-in map prefabs.
- `MapTexture$...` for named built-in textures.
- `runtime$...` for runtime prefab names.
- Hashed/high-range built-in asset IDs initialized by `GameManager`.
- `GameMapController.ShowFinalMat` removes missing texture IDs from animation lists and uses `GlobalDefaultHelper.DefaultTexId` (or white texture as the final fallback) instead of indexing a missing asset.

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
- repeated movement across two logical story scenes.

Record before/after positions, `touchTime`, avoidance normals, and `IntersectType` when diagnosing a regression. Do not “fix” one geometry case without replaying the established corner and slope cases.
