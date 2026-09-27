# Map runtime regression

Use `-Fixture MapTextureRuntimeRegression` to check merged base/mask submissions,
independent front layers, empty/inactive layers, property-block reuse and preservation
of visibility/lighting, missing/restored/replaced assets, same-length frame edits,
single/animated WangTile selection, pool rebinding and cache reset. It measures
zero managed allocations in warmed static Tile/WangTile/filtered-frame display paths.
Animation checks directly drive the production tick helper in EditMode; real timer
scheduling, visible rendering and route performance still require Play-mode profiling.
Success is reported as `MAP_TEXTURE_RUNTIME_REGRESSION_PASS`.

Use `-Fixture MinimapRuntimeRegression` with the runner below to check real
unlock-mask texture updates: batched per-height uploads, no mipmaps, saved
unlocks, block boundaries/orientation, duplicate/unchanged reveals and scene
exit/reentry. The flush returns its completed `Apply` count for diagnostics;
tests combine that count with the changed-layer set and actual texture pixels.
`Texture.updateCount` is not an upload counter in the headless test runtime.

Run from the repository root after Unity has generated the project files:

```powershell
./Docs/Tests/Run-MapRuntimeRegression.ps1 -UnityEditor "D:/path/to/Editor/Tuanjie.exe"
```

The script builds the real runtime assemblies, creates a unique project under
`Temp/MapRuntimeRegression-*`, and runs a hidden Unity Editor there. Only the
test copy of Assembly-CSharp is renamed (using the cached Burst package's Cecil)
so Unity can attach its MonoBehaviours as precompiled plugin types.
No original scene, Form, asset, or player save is loaded or mutated.

Checks cover translated/rotated/scaled Box and Sphere geometry against the
existing direct Collider conversion; stable Mesh/vertex identities during
translation; complete, unique, bidirectional character coverage for sizes 1–3;
unchanged-frame visibility submissions, normal/front display layers, material
property preservation and pool re-enable; all four unit lifecycle event filters,
Character self-reference semantics, and allocation-free ignored callbacks.
Additional checks cover single-hash dictionary hits and non-mutating misses;
tile-first owner inheritance for Object/Item/Character; multi-tile visual
occlusion, offscreen/missing owners, late-shown and bound instances, immediate
group operations, End/re-entry, zero allocations in warmed overhead frames,
and Object center coverage updating/restoring Tile and Nav pass types across
overlap, movement, removal, and rotation, plus obstacle-driven Nav heights.
Local Object updates are checked against full Nav builds for every cell and
edge, including overlapping obstacles, cross-layer movement, rotation/scale,
solid-terrain blocking, and movement during the periodic rebuild coroutine.
Distant terrain/pass-type containers must remain untouched by local refreshes.
Navigation endpoint checks cover blocked/missing/zero-scale/pass-restricted
starts, fixed escape anchors, blocked destinations via nearest reachable cells,
terminal movement across blocked-cell boundaries, independent agent states,
destination changes, large-agent clearance, exhausted BFS budgets and empty
graphs. Real ApplyMove checks keep manual pass restrictions and allow only the
scoped endpoint segment to enter a restricted Tile, with scope/teleport resets.
Bridge coverage includes a numeric snapshot of the saved bridge 1 position,
model offset and thin scale, JSON round-trip/scene-entry order, manual and Nav
pass checks, rotation/movement/removal, neighbouring water and another floor.
Upward probes cover exactly 0..1 world units (both endpoints), reject bodies
strictly below/above that segment and tilted-body AABB false positives, detect
flat bodies, and find reachable Objects outside the visual/owner index without
rebuilding distant Nav cells.
Bridge navigation also reproduces Play enabling the collision flag for both
saved bridge shapes, including their offset BoxCollider and enlarged Trigger.
Both complete dry-bank-to-dry-bank routes must actually reach the destination
in both directions (not just approach an unreachable target). Actual Collider
tops at `0.25` must remain passable under the whole-cell `0.3` rule without
overwriting Tile slope heights. Offset bodies outside visual/owner coverage update
the correct upper-layer cells on creation, movement and removal; local results
must match full builds. Physical ranges cover negative positions and non-unit
map cell dimensions without duplicate world-integer enumeration.
Whole-cell Object checks cover small center/corner obstacles missed by the old
four samples; strict `0.3` equality and large-world rounding; real Box/Sphere
overlap rather than AABB, edge-only contact, zero scale and below-ground bodies;
clipped local heights, sloped ground, Trigger exclusion and zero per-cell geometry
allocation. Creation, overlapping removal, movement, prefab changes and final
removal update outgoing/incoming links locally and agree with full rebuilds.
Event-editor checks exercise the real Unit controller's queued display parts:
both operands remain editable and in source order even without command metadata;
logical/modulo, unary +/-/!, postfix ++, +=, and nested grouping are preserved.
The enemy range-and-line-of-sight condition must retain both expressions.
Rendering twice leaves the AST unchanged; empty string operands stay visible,
while empty strings and omitted slots in ordinary calls remain hidden.

Success is reported as `MAP_RUNTIME_REGRESSION_PASS` in the printed log path.
Projected view checks cover lower-layer positive-Z shifts in Isometric mode,
unchanged and forced refreshes, diagonal movement, ascent/descent, camera-mode
switches and distant jumps. Visible Tiles and entering/leaving sets are checked
against complete per-layer rectangles, including actual pooled visibility.
Erase-brush checks invoke the real ModScene erase operation for all-erase,
remain-terrain and each independent entity flag. They cover multiple units per
Tile, neighbour-owned overlap preservation, Form/view/index cleanup, Object
removal events, unchanged terrain textures and repeated empty erases.
Collision-scale checks vary nested Box/Sphere scales, offsets and rotation,
exclude enlarged Triggers, update old/new Tile/Nav pass requirements immediately,
and preserve distant Nav containers and visual bounds. Inactive pool-template
roots retain geometry while authored inactive child Colliders remain excluded.
Object-mark checks cover root-local placement/scale, authored rotation, uniqueness,
model Renderer/source-prefab isolation, disabled physics, mode-change events,
pool disable/reuse/listener lifetime, and late Object Show in Event mode.
Mission checks cover origin/cell-size coordinate conversion, zero and 3-4-5
world-distance cases, actual HUD text updates on consecutive EverySecond events,
cross-scene/missing-scene labels, hidden-widget recovery and listener cleanup.
EditMode explicitly drives the Object instance enable/disable callbacks; still
verify marker visuals and automatic lifecycle callbacks in actual Play mode.
These focused EditMode checks do not replace Play-mode movement/trigger tests,
visual shader verification, target-device profiling, or an IL2CPP Player build.

## Mission command regression

```powershell
./Docs/Tests/Run-MapRuntimeRegression.ps1 -Fixture MissionRuntimeRegression
```

This uses the same isolated runtime binary/project setup without loading a story.
It checks every mission command's registration, canonical/legacy names, shared
metadata and table default compilation/execution; the `mission` type and constant
return slot; ID/name references, rename/removal, invalid/empty/omitted arguments,
received/done queries, unaffected missions, and one Add/Done event per state change.
Constant-selection checks exercise the actual command-entry builder in English
and Chinese for `mission` and generic `var` parameters. Mission must appear once
in the basic/constant group with a localized label, while mission operations keep
their own category; the generated indexes and wrapped MissionForm ID are checked.
Success is reported as `MISSION_RUNTIME_REGRESSION_PASS` in the printed log.
Still smoke-test choosing a Mission in the actual event editor and the Play HUD.
