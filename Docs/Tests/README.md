# Map runtime regression

Use `-Fixture MapFadeShaderRegression` to import and GPU-compile the actual
`DisFadeCode` forward/shadow passes in an isolated D3D11 Editor. It draws their
real pixels and checks a twenty-percent-opacity disk through 1.5 world units,
a quadratic fade ring from 1.5 to 3.5 (opacity 0.4 at distance 2.5), and full
opacity outside 3.5. CPU discovery/globals remain unchanged; fade distances do
not scale with map cells. Checks include fixed X/Z/diagonal boundaries,
standalone materials, non-unit/sub-unit cells, Overhead/Isometric,
camera yaw/zoom/aspect and perspective reference depth. It also checks ordinary
visibility, explicit fixed opacity, complete hiding and unchanged shadow texture/mask
cutouts. Per-pixel checks at low/mid opacity reject all opaque dither speckles;
overlapping quarter-opacity surfaces must blend to `0.4375` without writing depth.
Only cached HLSL headers are copied into temporary embedded packages;
no production package manifest or shader source is rewritten for the test.
Success is `MAP_FADE_SHADER_REGRESSION_PASS`. This is not a full Play/Player test.

The main map fixture checks the MPB protocol (`_Show=1, _FadeCenter=1` for logical
semi-occlusion, never fixed `_Show=0.5`), front/body independence, flag clearing,
unchanged-state caching, pool reuse and shared three-cell X/Z shader globals.
Its geometry assertions retain logical `0.5` as a classification only.

Use `-Fixture MapTextureRuntimeRegression` to check merged base/mask submissions,
independent front layers, empty/inactive layers, property-block reuse and preservation
of visibility/lighting, missing/restored/replaced assets, same-length frame edits,
single/animated WangTile selection, pool rebinding and cache reset. It measures
zero managed allocations in warmed static Tile/WangTile/filtered-frame display paths.
Animation checks directly drive the production tick helper in EditMode; real timer
scheduling, visible rendering and route performance still require Play-mode profiling.
Success is reported as `MAP_TEXTURE_RUNTIME_REGRESSION_PASS`.

The texture fixture also verifies `PerspectiveKeeper`'s actual transformed image
axes: cube/root nonuniform scaling must not tilt the fixed 45-degree side-view
plane or shear the image. Width and Y-Z diagonal height scale on img's rotated
local axes inside a unit-scale `rotateHolder`; Overhead switches to 90 degrees
and X/Z dimensions. The instance-owned outer scale compensator is created once
and reused. Tall/deep models,
nonuniform instance scale, yaw/pitch/roll, repeated refresh/camera switches and
authored image size, pool re-enable, refreshed-hierarchy cloning and the existing
sphere/character diameter-square behavior are covered without modifying physical Colliders.

Use `-Fixture MinimapRuntimeRegression` with the runner below to check real
unlock-mask texture updates: batched per-height uploads, no mipmaps, saved
unlocks, block boundaries/orientation, duplicate/unchanged reveals and scene
exit/reentry. The flush returns its completed `Apply` count for diagnostics;
tests combine that count with the changed-layer set and actual texture pixels.
`Texture.updateCount` is not an upload counter in the headless test runtime.

The map fixture also checks Object `centerCollider` defaults and copy/save/load,
fixed local -Z rear anchoring for Box/Sphere Collider scales 0, 0.25, 0.5, 1 and 1.5,
optional texture fitting, live/off-screen Collider/Trigger equivalence under root
rotation/nonuniform scale, repeated pool restore and switching back to centered.

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
tops at `0.25` must remain passable under the central/edge `0.3` rule without
overwriting Tile slope heights. Offset bodies outside visual/owner coverage update
the correct upper-layer cells on creation, movement and removal; local results
must match full builds. Physical ranges cover negative positions and non-unit
map cell dimensions without duplicate world-integer enumeration.
Central/edge Object checks cover small center/corner obstacles missed by the old
four samples; strict `0.3` equality and large-world rounding; real Box/Sphere
overlap rather than AABB, edge-only contact, zero scale and below-ground bodies;
clipped local heights, sloped ground, Trigger exclusion and zero per-cell geometry
allocation. Creation, overlapping removal, movement, prefab changes and final
removal update outgoing/incoming links locally and agree with full rebuilds.
Directional checks separate central `0.8 x 0.8` standability from four full-edge
`1 x 0.4` inward strips on unit Tiles. They cover all four sides, corner coverage,
both source/target edge vetoes, standing in an enclosed center without endpoint
escape, BFS detours and smoothing across a barrier. Moving/rotating/lowering an
obstacle and overlapping removal refresh only local edges, preserve distant
containers and match full rebuilds including the directional bitmask.
Event-editor checks exercise the real Unit controller's queued display parts:
both operands remain editable and in source order even without command metadata;
logical/modulo, unary +/-/!, postfix ++, +=, and nested grouping are preserved.
The enemy range-and-line-of-sight condition must retain both expressions.
Rendering twice leaves the AST unchanged; empty string operands stay visible,
while empty strings and omitted slots in ordinary calls remain hidden.
Form-list checks cover pooled row sibling order after filtering/count changes,
including a trailing New row; Lab ID sorting and lowest-ID path aliases;
Program category order by Lab ID and merged category entries by Data UID.
Names and insertion order deliberately conflict with the expected ID order.
Empty Object thumbnail checks exercise the real list, appearance-frame and Mod Tool
controllers: empty/default/missing IDs and explicitly empty facings cannot retain a
pooled Sprite, override, GIF/WebP binding or icon/legacy fallback. Valid rebinding
restores the Image, and new Objects/frames start with empty clips/unassigned ID 0.
Collision-bound checks include alpha `1/76/77/255`: the normalized `0.3`
threshold excludes detached faint pixels in ordinary/GIF/WebP frames and assembled
WangTile variants, includes alpha 77 edges, and preserves zero XYZ geometry when
an imported image has no pixels meeting the threshold.
Diagonal-bound checks cover tall/deep Objects, nonuniform model/root scale, Y/Z
size and center mapping, preserved Trigger padding and converted Sphere Y-scale
compensation. WangTile/facing changes, hidden root resizing, live Colliders,
pass/navigation refresh and pool reuse all consume the same fitted geometry.
Object-height checks exercise the appearance Model's bottom-relative input mapping:
unit/thin/tall heights, repeated changes, positive/negative elevation, X/Z dragging,
Reset and JSON reload. Real preview/runtime Collider meshes retain the requested
bottom with bounds fitting both off and on; merely reading legacy saved offsets
must not move existing Objects or change their serialized data.

Success is reported as `MAP_RUNTIME_REGRESSION_PASS` in the printed log path.
Projected view checks cover lower-layer positive-Z shifts in Isometric mode,
unchanged and forced refreshes, diagonal movement, ascent/descent, camera-mode
switches and distant jumps. Visible Tiles and entering/leaving sets are checked
against complete per-layer rectangles, including actual pooled visibility.
Erase-brush checks invoke the real ModScene erase operation for all-erase,
remain-terrain and each independent entity flag. They cover multiple units per
Tile, neighbour-owned overlap preservation, Form/view/index cleanup, Object
removal events, unchanged terrain textures and repeated empty erases.
The ModScene history fixture also drives actual Camera raycasts and pointer
Down/Up erase clicks with placement offsets retained across simulated reentry;
the clicked layer/owners must be erased, controls preserved and undo usable.
Placement checks cover unbounded Object/Item/Character height inputs, actual clicks
in empty camera layers, sparse-column nearest-lower ownership, retained world
height/rotation, immediate display, duplicate no-ops and local undo/redo/rebinding.
Above-only or empty columns must reject without Form/history/redo changes. Map-cell
dimensions and the existing logical-coordinate conversion remain authoritative.
Object-picking checks drive actual pointer clicks in normal/Event mode with a
lower owner but raised physical Collider and padded Trigger. They verify rejection
continues to the lower ray hit, equality and height switching, child centers,
rotation/scaling, multipart/disabled/inactive bodies, Trigger-only and Sphere
prefabs, and higher-owner filtering without history operations. Empty-click
fallback checks cover multiple raised Objects in overhead/side view, current-height
priority, highest-to-lowest ray hits, Trigger-only/higher-owner fallback and truly
empty clicks. Event Tile selection prevents fallback; normal mode ignores Tiles.
Full-occlusion checks require only an occludable Tile directly above the player,
including zero, one or two cardinal neighbours; an absent/kept-visible center does
not trigger full hiding. Preserve projected-height/front exceptions, keep full
hiding when moving under the same component and restore after leaving all head Tiles.
Current-layer owner-roof checks also hide elevated Objects when the player's exact
Tile owns one, extending only through cardinal Tiles that own elevated Objects.
Every physical Collider bottom must exceed `owner.pos.y + 1` (world units, with
`0.0001` tolerance). Bottoms between ground and that upper boundary, equality,
within-layer multipart bodies and within-layer bridges do not hide/connect roofs.
They cover child Collider offsets, padded Triggers, rotated physical Spheres,
multipart ground contact, Trigger-only/missing bodies, texture-fitting, view edges,
nonzero layers, omitted visible-list Objects, diagonal/foreign overlap exclusions,
missing/grounded bridge breaks, departures/restoration, Mod isolation and warmed
zero allocations without hiding current Tile/front, Item, Character or grounded
Objects. This full-hide override wins over nearby/mixed Object half-opacity.
Disconnected components keep ordinary opacity,
while the full center component can extend beyond the half-opacity radius.
Sparse upper-layer checks skip empty intermediate layers and use nonzero player
layers. The shared Isometric scan is X +/-3, then backward Z 0..-5, then height
0..+5, inclusive. Direct high-Tile hits test height >= backward depth (height > 0),
including depth 1/height 1, depth 2/height 2 and depth 5/height 5 without the old
anchor-radius cutoff. This is a fast path, not an exclusive condition: when height
is less than depth, the player-layer projection (x, z + height) inside radius 3
still qualifies. Left/right one, backward three, height two regressions verify
actual body/front fade flags, radius boundary/exclusion, nonzero player layers,
Collider exemptions, Overhead/departure restoration and warmed zero allocations.
Existing head-full/connected-component rules are retained; ordinary BFS
half-expansion still uses logical X/Z radius 3. Body/front upper-Collider
exceptions take priority over direct hits, and cached checks reset next frame.
Independent Object checks cover every rectangle column, fifth depth/height,
lower/upper/direction exclusions, gapped owners and out-of-list Objects.
Current-layer Object projection compares transformed visual corners against the
current enumerated Tile's front edge on that Tile's height plane:
max(world Z+Y) > tile.pos.z + tile.pos.y + abs(mapUnitSize.z)/2 + 0.0001.
Touching is opaque; fractional player movement does not replace that boundary.
A failed first linked Tile must not suppress a later qualifying Tile. Checks
include root/model offsets, rotation/scale, multipart extrema, owner rebinding,
non-unit Y/Z cells and nonzero layers. Eligible independent Objects remain half
transparent even over fully hidden/Collider-exempt owners.
Higher Objects retain the original player-plane projection and high-Tile/full
inheritance without the new front-edge gate. Classification follows the Object's
root logical layer, not a higher Tile it overlaps; moving between layers updates
that rule immediately. Owner half-opacity and mixed links cannot bypass the
respective projection test. Mod's explicit preview is retained.
Every automatic Object half-opacity path, including Mod/Overhead inheritance,
requires a higher root layer, or a current-layer physical Collider with final
world Y extent > Z extent (0.0001 comparison tolerance). Lower roots are excluded.
Current-layer zero-X/Z-edge Boxes are rejected before comparing extents, including
the saved grass4 colliderScale=0 shape, rotated degenerate bodies and fitted
variants. Collapse/restore checks verify that stale fade flags clear next frame.
Checks cover Y<Z/Y=Z/Y>Z, horizontal Collider scaling, texture-fitted bodies,
small-but-tall bodies, rotated/negative/nested scaling, disjoint bodies,
inactive/Trigger-only/missing geometry and physical Spheres. High Objects bypass
shape filtering. Root layer changes, next-frame size refresh/restoration and
out-of-list fade flags are verified. Neither an absolute-height threshold nor
a colliderScale<=0.8 shortcut remains. Full hiding, explicit group degrees and
Tile opacity are unchanged. Physical shape eligibility and visual projection
are cached once per Object/frame without warmed allocations.
Item/Character inheritance, Mod/Overhead/departure restoration,
non-mutating indexes and warmed-frame zero allocations are verified too.
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
