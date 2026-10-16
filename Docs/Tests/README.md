# Map runtime regression

`Run-MapRuntimeRegression.ps1 -Fixture TileFrontVisibilityRegression` checks Play's
Show-time front suppression against the exact same-height -Z neighbour by prefab
name. It uses the real DisFade shader and texture/animation entry points, covering
offscreen neighbours, other prefab names/case, lower/forward/side neighbours, Mod,
body/front separation, subsequent vision changes, animation cancellation and a
new owner on the same instance hierarchy. This is an isolated Editor check;
native pool callbacks and final scene rendering still need Play-mode verification.

`Run-MapRuntimeRegression.ps1 -Fixture CodeInterpreterRuntimeRegression` runs the
event-language NUnit assertions on isolated Unity Mono, including compilation of
the stored ef sources. Scalar allocation checks cover lazy dictionaries, member
access/deep-copy isolation and the command API's writable root dictionaries.
Single-instruction budgets verify actual temporary Box identity reuse across
hundreds of loop operations while operands remain suspended on the stack. Wait,
retained numeric/string returns, async arguments/results, subprogram dictionary
copy-out, cancellation, Reset and instruction errors check ownership boundaries.
The main-thread free Box pool is global (128 cached entries); cross-interpreter
identity reuse, interleaved suspended operands, clearing free entries and releasing
the previous Interpreter's owner reference are checked independently.
Hot-path checks cover text/reference/number field cleanup across reuse, registered
command-argument change notifications, atomic binary-pop null/underflow diagnostics,
noncommutative operand order, string/numeric comparisons and nested dictionary
arithmetic. Temporary backing-field initialization/cleanup stays in hand-written
partials; generated Forms and ordinary value notifications remain unchanged.
Identity and functional assertions remain valid when Mono's allocation counter
fails calibration; actual per-frame GC reduction still requires Unity Profiler.

`MapRuntimeRegression.RunViewRefresh` and the full map fixture check synchronous
view-refresh pool transactions. Explicit opt-in probes observe real native
OnEnable/OnDisable counts: a returned active instance is preferred over an older
inactive FIFO entry from the same prefab, owner references detach immediately,
and its Colliders are suspended until checkout or final deactivation. Nested
scopes, repeated handoffs, different prefabs, exceptions, Clear/Destroy, and
non-opted-in fallback are covered. An unmodified real TileInstance verifies the
FreshMap integration and owner handoff without attaching a probe that would
invalidate the audited component whitelist. Sparse one-layer/multilayer queries
and empty attachment lookups also check that visibility does not create index keys.

The texture fixture checks separate typed binding/keeper lists, repeated collection
without duplicates, inactive children, runtime native components and nested Tile
instances, preserving conservative reuse eligibility without a mixed Component list.
Model slots now use only `viewRefreshBindings`: tests cover all six body/front
slots, inactive parts, null/alternate DisFade materials before routing, stable target order,
MPB cleanup and cached-list identity without a second Renderer cache.
Source-material snapshots are shared across textures. Tests verify no recapture
on warm visibility/texture/configuration/Release/camera work, stable cached defaults
for new variants, explicit Refresh/RefreshSource propagation, authored pass switches
and source-generation retirement on Clear.
Routing callers guarantee DisFade shaders: there is no shader lookup/type check
or non-DisFade compatibility path. Null/destroyed sources, alternate DisFade source
replacement and multi-material MPB fallback remain covered.

The texture fixture additionally checks real TileInstance transactions with the
production DisFade shader/binding and texture-animation timer: cancellation of
the old owner, hidden ShadowsOnly baseline restoration, empty/new textures,
visibility/fade/lighting reset over repeated handoffs, and fallback after adding
an unknown MonoBehaviour. These tests assert lifecycle and appearance state,
not unreliable batch-mode allocation counters or an in-scene frame-time gain.
`MapRuntimeRegression.RunViewRefreshPlayLifecycle` repeats the view subset inside
an isolated native Play session and adds a throwing Fresh callback, manager Clear
with an outstanding scope, stale-generation disposal and copied-scope idempotence.
Use that entry for clean native destruction/lifecycle evidence; EditMode's full
fixture can log Unity's warning when production deliberately calls deferred Destroy.

`pwsh -File Docs/Tests/Run-MobileInputRegression.ps1 -Platform Android` (repeat with
`-Platform Desktop`) compiles actual InputManager, event bus, Stick and player-touch
controllers, plus ModScene pointer/commit methods, against lightweight Unity/service
spies without launching the project or reading stories/saves. It checks stable
finger IDs/crossings, phase and cancel routing, focus/pause/disable cleanup, independent
stick ownership, blocked-gameplay continuation, LightAttack self-continuation,
scene UI-origin filtering, brush termination over UI/cancel, and desktop mouse paths.
The authored Stick source and Q/E prefab overrides are checked statically for
sibling center/area layout, a raycast-enabled center and disabled Q/E area Graphic.
Actual `UiPlaySceneMainCtrl` Q/E callback/setup/refresh helpers are extracted too:
center press captures zero offset, edge drag maps screen XY to world XZ, Q/E bind
their own skills, Refresh preserves an owned aim, and blocking/skill-disable/cancel
still ends it without casting or clearing another skill's option. Original raycast
authoring remains unchanged; these spies do not run native Unity GraphicRaycaster.
Move/LightAttack floating-mode checks cover an idle-hidden knob with a transparent
active hit region, off-center press origins, scaled relative drags, independent
crossing fingers, immediate release/cancel/block hiding, and reuse in fixed mode.
Android move-facing checks cover initial movement, neutral/long attack holds,
the one-second release/cancel boundary, renewed input, Q/E priority and desktop exclusion.
Mod Begin/End reset calls are checked statically; real map painting/history and
Android touch/UI hit testing still require on-device Play/Mod smoke tests.

Mod package file selection has a separate non-interactive PowerShell 7 regression:
`pwsh -File Docs/Tests/Run-ModPackageFileRegression.ps1`. It compiles the actual
`AssetFilePicker` source for Windows Editor (including an Android active build
target), Windows Standalone and Android Player, checks safe Unicode/default/
reserved/long filenames, Windows native API guards, OPENFILENAME layout and
overwrite-confirmation flag, and resolves the native DLL exports without opening
a dialog. Gallery callback stubs also verify shared image import, PNG filtering,
unchanged selected paths, cancel/error handling and exact byte-array gallery export.
Manually check `ModStory.btn_outPut` in Editor/Windows Player: choose a
directory, export/import a `.png` with its embedded story intact, cancel, confirm
or reject overwriting, and try a read-only destination. Android package import
shares `AssetFilePicker.GetImage` and NativeGallery with ordinary image import,
using PNG MIME; export saves the original PNG bytes through NativeGallery's
byte-array overload. No custom phone file-management Activity/JNI bridge remains.
Check gallery permission denial, cancel, non-PNG files, failed saves and repeated
clicks on-device. These tests do not prove interactive gallery delivery or IL2CPP builds.

Import/export now show Loading before opening platform selection/saving and keep
their own loading item through async completion. The picker regression also
executes the production async UI and Loading methods: frame-before-work,
duplicate-click guards, permission/error/cancel paths, hide/reopen, late and
duplicate callbacks, and independent loading-item ownership. File processing and
PNG compression run on workers; Form/UI/cover/platform APIs remain on Unity's
thread. Native dialogs and NativeGallery's final save are not background APIs.

`Run-MapRuntimeRegression.ps1 -Fixture StoryPackageImportRegression` checks the
actual package decoder and import transaction in an isolated Unity project and
temporary persistence root: round-trip bytes/nested assets, fresh IDs, duplicate
imports, occupied roots, rewritten persisted overview, unrelated Form identity,
invalid/plain/truncated PNGs, bad Base64/overview, traversal paths and rollback.
New `.png` exports store the GZip-compressed package in lossless RGB image pixels,
with a versioned header, payload length/checksum and optional small cover; they
write no `tEXt`/ZEXTRA metadata. Legacy ZEXTRA PNGs remain readable through a
read-only compatibility path. Old packages previously named `.txt` or `.mod` can
be renamed to `.png` without altering their original bytes (legacy cover schema
still requires migration). Android's shared image picker filters `image/png`
and retains the provider's original selected/cache path, including `.tmp` names.
Windows checks the `.png` suffix; Android identifies packages by PNG byte signature
and protocol during import, not a cache filename. Import reads original file bytes
without loading or re-encoding them as a texture. Do not resize or edit package
pixels or convert them to JPEG: lossy changes destroy the embedded data.

The story package fixture also checks async import/export payload preservation,
Form registration/main-thread continuation, canceled/failed import cleanup,
registration rollback, concurrent ID/directory races and worker encoding after
the cover texture is destroyed. It runs only against its temporary persistence
root, never player stories.

`AreaMapSelfMarker` (full/scene-command fixture) verifies PlayMap's live self
position against the inner map rectangle, excluding the minimum 200-pixel margins. It covers
non-unit map cells, rotated/scaled UI, movement without advancing game seconds,
height/world-mode filtering, absent player/support, unready/single-cell maps, and
disable/reopen/first-show cleanup. Opening centers the current player once, clears
scroll inertia, and retains manual panning on later updates. The fixture checks
rotated/scaled UI and noncentral viewport pivots, single-cell/edge-map padding
against ScrollRect's actual elastic correction, and pending-center cancellation.
Prefab and generated bindings remain authored
UI assets; verify the marker's actual artwork and scrolling in Play mode too.

The scene-command fixture also checks absolute/relative Character navigation with
an empty/null/missing speed binding, missing/null parameter dictionaries or values,
and valid custom/zero speed parameters. Invalid bindings preserve the scene speed
without rewriting Product configuration; destinations and navigation activation
still update, including the reported `NewVector(0, 4, 0)` on non-unit map cells.

Mission-marker checks use the real `DoneMission`, `PlayInfoController` selection
and area-map/minimap pending-refresh/filter paths with lightweight test containers.
They cover completion/failure clearing the last selected ID, switching to another
active mission independent of listener order, stale completed IDs, frozen game
seconds, guide-arrow cleanup, hidden/unreceived/disabled/cross-scene missions,
area-map height filtering, unchanged-frame caching and hide/reopen subscription
cleanup. Authored marker artwork and actual Play-mode UI timing remain manual checks.

Scene rotation checks cover `SetRotation` and both legacy aliases, Character
Product/scene and Object scene references, same-call Data/Instance updates,
Character force-facing synchronization, preserved X/Z, shortest-path wraparound,
bounded timed interpolation, exact completion, invalid values/references and
removed-instance task cleanup. Canonical metadata/default-code compilation and
alias lookup are checked in the scene-command Lab fixture without duplicate
rotation chooser entries. Real directional artwork still needs Play-mode review.

`MapRuntimeRegression.RunDestroyCharacter` checks Character removal through both
`$so$` interaction/BoundaryTouch and `$ch$` Product references: immediate pooled
instance hiding, Form/spatial/view/lookup cleanup, unique Product preservation,
non-unique Product deletion, interaction-option removal, duplicate/missing/wrong-type
references, recycled UID safety, current-player reference cleanup and self-removal
followed by Continue in the same interpreter pass. The full map fixture runs these
checks too. Run only in an isolated test project; no real story/save is changed.

`MapRuntimeRegression.RunInterpreterSchedulingOptimization` exercises the real
event/task scheduler in isolation and also runs in the full map fixture. It checks
frame-snapshot reuse, zero allocations for one thousand warmed empty passes when
the runtime's allocation counter passes an explicit-allocation calibration,
next-pass execution of newly added events, reentrant `LateUpdate`, recycled event
and scene-owner UID identity, normal root-event completion canceling subprogram
tasks, and self-removal allowing only the current event pass to finish. Task
checks retain immediate polling and the original same-pass order for callback-
appended tasks, stable survivor compaction, and callback-driven `ClearScene` or
`Reset` without erasing or polling the replacement lifetime's queue. Repeated
cancellation and a throwing task callback are checked too: finished work stays
removed while the throwing and not-yet-polled tasks remain queued. These are
lifecycle/allocation regressions, not a claim about whole-frame interpreter time
or command business-logic cost in Play mode.

`CodeLanguageRegressionTests` covers the accompanying interpreter allocation
changes: suspended commands are created/prepared only once, their parameters stay
captured while waiting, and completion/Discard still balances the stack. Program
runtime snapshots preserve concrete metadata and share only a private compiled
code/map revision; replacing either source/map list (even with equal contents)
starts a new revision without changing an active execution. Published compiled
lists are immutable; updates always publish new lists. Parse caches are shared across
interpreters without sharing stacks/heaps/cancellation, survive Reset, and reject
stale numeric/opcode results after list replacement. Return-info checks retain independent
public default values while avoiding an immediately discarded default Box when a
return value is already supplied. Dictionary `+=` checks require fresh result
fields without modifying aliases. Shared parent/child instruction budgets, depth
limits and a waiting subprogram whose source signature/registry changes remain
part of the language regressions; existing 4,096-instruction and timing rules are
not replaced by a new global scheduler budget.

Run those NUnit assertions with
`Run-MapRuntimeRegression.ps1 -Fixture CodeInterpreterRuntimeRegression` in the
same isolated Unity workflow. The wrapper invokes every Test/TestCase on Unity
Mono using the already cached NUnit dependency, compiles the test assembly first,
and copies only the saved ef fixture for source compilation checks. It temporarily
enables the isolated Editor profiler for allocation counters; no production
scene, program, save or profiler setting is modified. Some Tuanjie batch runtimes
report zero even for explicit allocations. That environment explicitly skips the
relative RetInfo allocation assertion (and scheduler allocation measurement),
instead of treating zero as evidence of allocation-free execution; functional,
call-count, cache-identity and lifecycle assertions still run.

`MapRuntimeRegression.RunNavigationOptimization` is the isolated navigation
optimization entry. It checks central/four-edge Box/Sphere rules, overlap removal,
local/full equivalence, warmed zero allocations, 3,600 randomized Box cases against
the original clipping path (flat/slope, rotated/axis-aligned, large world coordinates
and unit/non-unit cells), small-batch completion and active-rebuild request suppression.
An axis-aligned microbenchmark logs legacy/optimized time without asserting a
machine-dependent speed ratio. Play periodic batches are `/100, min 5` instead of
`/50, min 10`; actual frame-time improvement needs in-scene profiling.

The same entry now runs incremental path queries against controlled logical
frames: every Character receives at most ten cell work items per frame, including
blocked-start nearest-cell search, graph traversal, backtracking and smoothing.
The first unfinished search waits; later unfinished refreshes keep the last
completed waypoint and recompute its direction from the latest Character position.
Checks cover repeated same-frame calls and Reset without replenishing that
frame's budget, two independent agents sharing one BFS,
continued progress while the start or ordinary target crosses cells every frame,
the optional-state scratch query, radius/capability changes and Reset. A target
cell change keeps the current ordinary search on its captured target and retains
the previous ordinary waypoint until a new round completes; the next round uses
the latest destination. Do not redirect an old target-following waypoint across
cells, which could cut through obstacles. Existing direct-endpoint phases still
reset when their destination changes cell. Existing endpoint, bridge,
directional-wall and unreachable-route assertions explicitly wait for a finished
query in EditMode, whose native `Time.frameCount` otherwise stays fixed. Reapplying
an equal pass-capability set must not cancel an active search. Ten thousand warm
public `CanPass` calls with a nonzero clearance footprint must allocate zero bytes
while preserving adjacency, standability and center-only pass requirements.
A grounded real Character also executes `UpdateInfo` while its first result is
pending: position/facing remain unchanged, a preexisting sidestep escape is
canceled, and waiting refreshes the collision-stall timer instead of starting
another escape. This uses state/movement assertions, not whole-frame GC counts.
This limits search work per Character rather than imposing one global ten-cell
budget for all Characters; overall costs still require a real Play/phone profile.

`TerrainHeightCaching` in the main map fixture compares cached Tile heights with
the original slope formula, including flat/vertical/missing-Collider fallbacks,
yaw, world-height changes and zero-scale transitions. It measures 10,000 warm
height reads with zero managed allocations and verifies periodic Nav rebuilds
skip unchanged samples while reusing both height arrays and pass-type sets.
Explicit Collider edits, terrain-local refreshes and same-name pool replacement
must update samples without replacing arrays or touching distant Tile caches.
The fixture logs legacy/cached sampling allocations and warm full-rebuild bytes;
these isolated results do not substitute for real Play/Player frame-time profiling.

Use `-Fixture MapFadeShaderRegression` to import and GPU-compile the actual
`DisFadeCode` opaque/forward/shadow passes in an isolated D3D11 Editor. It draws their
real pixels and checks a twenty-percent-opacity disk through 1.5 world units,
a quadratic fade ring from 1.5 to 3.5 (opacity 0.4 at distance 2.5), and full
opacity outside 3.5. CPU discovery/globals remain unchanged; fade distances do
not scale with map cells. Checks include fixed X/Z/diagonal boundaries,
standalone materials, non-unit/sub-unit cells, Overhead/Isometric,
camera yaw/zoom/aspect and perspective reference depth. It also checks ordinary
visibility, explicit fixed opacity, complete hiding and unchanged shadow texture-alpha
cutouts. Per-pixel checks at low/mid opacity reject all opaque dither speckles;
overlapping quarter-opacity surfaces must blend to `0.4375` without writing depth.
All opaque pixels draw color/depth first, then partial-alpha pixels blend without
writing depth. GPU checks reverse opaque draw order, cross tilted image planes,
composite translucent foreground over a later opaque background, reject opaque
depth at `0.9999`, and split the fading disk/ring from its opaque exterior on the
same Renderer. Hidden/texture-alpha holes remain depth-free. Feature checks cover
the actual URP pass's phase, layer/queue filter, unchanged MPBs, camera cleanup,
disposal and single-pass fallback for previews/Renderers without the feature.
Native `ScriptableRenderContext.DrawRenderers` checks also exercise the real
Feature's pre-cull routing hook with an actual MeshRenderer and camera target.
Opaque Forward is tested with its shader discard disabled, and transparent/hidden
variants with opaque draw-time properties, so empty pixels prove the pass was not
submitted rather than merely clipped. Cull must capture the disabled pass flags;
changing flags only in AddRenderPasses is too late. Cleanup before Submit must
still restore the next fallback camera correctly. Mixed surfaces retain both passes.
Instanced draws test all three real passes with multiple transforms and independent
Show/Alpha/FadeCenter/Cutoff/LightSensitivity values; the shader has no AlphaTex.
Native DrawRenderers tests compare ordinary and instanced submission of shared
meshes/materials with numeric-only MPBs, preserving opaque depth and transparent
sorting. Instancing must reduce actual draw calls, not merely expose a checkbox.
The batchmode Editor may publish zero rendering counters/native FrameDebugger
events despite producing real GPU pixels. In that case an isolated derivative
of the production shader retains its buffers, IDs and clip math, but encodes
the hardware instance ID in final color. Ordinary Renderer controls must all
have ID zero; both opaque and Forward automatic-instancing draws must produce
32 distinct consecutive IDs (0..31). This proves multi-instance submission
without treating a missing counter as zero work or modifying production shaders.
The runner verifies the feature is attached to the GraphicsSettings default
Renderer. Cached HLSL headers are copied into temporary embedded packages, and
the Feature source is compiled against cached production URP/runtime binaries;
no production package manifest or shader source is rewritten for the test.
Success is `MAP_FADE_SHADER_REGRESSION_PASS`. This is not a full Play/Player test.

The map visibility MPB protocol is uniform opacity: `_Show=degree` and
`_FadeCenter=0`, including fixed `_Show=0.5` for half occlusion. All nonzero
partial degrees use the cached Transparent Route and a single forward color
pass. The current routing fixture covers front/body independence, clearing old
center-fade overrides, texture updates and repeated Character pool acquisition;
the main map fixture's geometry assertions read the submitted degree directly.

Use `-Fixture InstancePoolNameRegression` to check prefab-name pool lookup,
acquisition/return, duplicate-name rejection, identity-safe unregistration,
same-refresh reuse, registration during refresh and Clear/scope invalidation.
Success is `INSTANCE_POOL_NAME_REGRESSION_PASS`.

Use `-Fixture DisFadeRoutingRegression` for the current Renderer-based routing
entry points. It checks cached caller-supplied Route selection (without MPB reclassification), uniform half-occlusion opacity/Transparent routing, explicit
Hidden/Opaque/Transparent/Mixed transitions, scalar and texture
updates with immutable material-to-texture bindings, independent Tile body/front routes, empty layers, actual Shader pass
switches, Hidden color-pass suppression across texture/camera/show transitions,
authored shadow-pass and Renderer/shadow-mode preservation, split-camera cleanup
and Character first Show/reuse from a disabled
template, including part enablement and visibility during texture replacement.
It runs the production C# methods against the
real shader in an isolated Unity project; GPU appearance and draw-call counts
still require Play-mode profiling. Success is `DIS_FADE_ROUTING_REGRESSION_PASS`.

Use `-Fixture MapTextureRuntimeRegression` to check shared texture-material binding,
independent front layers, empty/inactive layers, property-block reuse and preservation
of visibility/lighting, missing/restored/replaced assets, same-length frame edits,
single/animated WangTile selection, pool rebinding and cache reset. It measures
zero managed allocations in warmed static Tile/WangTile/filtered-frame display paths.
Animation checks directly drive the production tick helper in EditMode; real timer
scheduling, visible rendering and route performance still require Play-mode profiling.
Success is reported as `MAP_TEXTURE_RUNTIME_REGRESSION_PASS`.

`MapTextureRuntimeRegression.RunDeferredVisibilityPlayLifecycle` is a focused
isolated Unity execute-method entry using native Play-mode callbacks. It checks
that Hide retains MPBs/materials/textures/routing state and timer handles, normal
and same-refresh Show restore the next owner's appearance, VisOff suppresses
color while retaining authored shadows, VisOn restores current numeric values,
and animation/cache retirement cannot reveal a hidden Renderer. Timer checks
exercise inactive-bind termination and the real texture callback's old-owner
guard; empty texture slots and pooled external/null material replacements are
covered too. Success: `MAP_DEFERRED_VISIBILITY_LIFECYCLE_PASS`.

The same fixture imports the actual DisFade shader and checks shared routing
variants, independent normal/front slots, all four authored shadow modes,
hidden animation/ignored-legacy-mask updates, explicit MPB alpha, empty texture configuration,
display ceilings, source changes through Refresh, excluded render queues,
pool reuse and scene cleanup. Never-used inactive Instances must not create
binding components on disable; first access includes inactive model parts. Warm six-slot visibility,
routing and camera-scope transitions must allocate zero managed bytes.
DisFade MPBs must contain only instanced scalars, never Tex/AlphaTex. Same original
source/Texture/route shares one cached material; different textures and source
configurations remain distinct. Template binding recipes survive Instantiate/pool
reuse/cache-generation retirement without dead materials or lost artwork. Active
objects preserve their current texture at Clear; inactive ones reset to the template.
External/custom/null material replacements must not revive an old binding.

Character first-show checks build the real empty/disabled runtime template through
`GameUtilController`, clear the material cache twice **before** cloning, then drive
pool activation, visibility and both-part animation across three reuses. Inactive
templates must retain their authored caster/enabled baseline without a routing state;
the cloned image must not inherit a permanent `ShadowsOnly` mode. Add
`-CharacterPlayLifecycle` with `-Fixture MapTextureRuntimeRegression` to exercise
the same case inside the isolated project's Play mode using native lifecycle
callbacks instead of the EditMode callback surrogate. Its success marker is
`MAP_CHARACTER_PLAY_LIFECYCLE_PASS`; the real game scene and saves are never opened.

`pwsh -NoProfile -File Docs/Tests/Run-ObjectPreviewLifecycleRegression.ps1`
checks the Object appearance page's actual OnShow/OnHide methods and the preview
manager source with isolated lifecycle stubs. Repeated show/hide, preview replacement,
view changes, null-parameter reopen and Clear must deactivate the independent
camera and release preview objects. It does not open the real game scene or saves;
Play-mode profiling is still needed to measure the rendering benefit.

Character animation lifecycle checks execute the real `CharacterPlayAnim` command
for a never-shown Character, lazy visible registration, forced replay, model rebind,
unregister/re-register, hidden/unbound instances, removed Products, recycled Data
with the same UID, scene reset and delayed animation after removal. They verify
no runtime diagnostic, no resurrection, both-part timer cancellation and Idle
recovery after clearing Special state. Real Play-mode visibility/scheduler smoke
tests remain necessary.

The texture fixture decodes a real two-frame GIF and checks native frame delays,
one-cycle playback followed by an interval holding the final frame, mixed static/GIF
clips, zero intervals, sole GIF loops, shared map phase/pause, cache replacement/reset,
and zero-allocation warm timeline sampling. It drives Character's production tick
through GIF waits, Special/Die completion, forced replay and both body parts' facing
changes. Animated WangTile generation must preserve both internal frames and delays
for all 256 masks while sharing 47 variant textures per frame. These are deterministic
Editor checks, not a real Play-mode scheduler/rendering or IL2CPP Player test.

The texture fixture also verifies `PerspectiveKeeper`'s actual transformed image
axes: cube/root nonuniform scaling must set the side-view pitch to the Y/Z diagonal
angle `atan2(abs(Z), abs(Y))`, without additional tilt or shear. A `1 x 0.1 x 1`
bridge uses about 84.29 degrees and its actual image up vector spans `(0, 0.1, 1)`;
model aspect-ratio changes also refresh through LateUpdate. Width and Y-Z diagonal height scale on img's rotated
local axes inside a unit-scale `rotateHolder`; Overhead switches to 90 degrees
and X/Z dimensions. The instance-owned outer scale compensator is created once
and reused. Tall/deep models,
nonuniform instance scale, yaw/pitch/roll, fixed/default pitch paths, repeated refresh/camera switches and
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
The actual Object Config button is also clicked repeatedly with no active map or
scene renderer, then reopened, checking immediate state refresh and single-click toggles.

The main fixture checks case-sensitive `Return` diagnostics using the reported
nested Team condition: lowercase `return` produces three errors beginning at line 3,
column 8, while changing only those keywords to `Return` compiles with two parameters
and a numeric return. Timed Notify Comments expire independently at their five-second
absolute deadline; ordinary Comments remain persistent, manually closed entries do
not expire a pooled replacement, and simultaneous expiries remove all matching entries.

Compiler checks cover registered Cmd aliases and custom methods, self/forward calls,
case-sensitive lookup, nested/unreachable missing calls with exact CRLF source locations,
and preservation of the last valid program on failed Apply. Strings and member names
must not be mistaken for calls. The Z_Code NUnit fixture explicitly registers real
command templates for EditMode and registers all ef program names before compiling;
each temporary registry entry is restored afterwards.

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
Movement time-step checks preserve fast-frame scaled delta, cap slow frames at
0.02 seconds, discard stalls without recovery catch-up, and advance timed movement
and its remaining duration together. Keyboard input checks cover normalized
diagonals, opposing keys, speed parameters and the blocking guard with the same
bounded step. Shared frame-clock checks cover the common `TimeManager` cap,
scaled/unscaled paths, invalid inputs, ObjectAnimator duration/frame waits,
actual interpreter Wait, input drag scrolling and Play progress blocking/disable
guards; progress/cooldown/UI consumers now share the same cap.
Actual canonical Move/MoveRelative and legacy MoveObject/MoveCharacter command
checks start at nonzero positions and retain asynchronous tasks. New commands use
supplied logical tiles/second independently of Character Speed; legacy aliases
retain duration. Both use bounded displacement/time without mutating vector inputs
or shared interpreter heapTemp. Deterministic frame steps verify slow-frame caps,
nonuniform map dimensions, no overshoot, invalid-speed no-ops and removed-instance
completion. Get/SetPosition absolute/relative checks cover Character Product and
scene handles, mixed relative origins, the player-coordinate offset, malformed
references and interaction scene-handle area-query centers. They retain unobstructed
sub-centimetre Character steps without changing collision contact dead zones.
`MapRuntimeRegression.RunMovementTimeStep` is the focused entry for these checks;
command timing tests temporarily exclude navigation rebuilding and restore it.
`MapRuntimeRegression.RunSceneCommands` also checks all six canonical default calls,
old-source compilation/metadata lookup, CmdLab inheritance and foreign keys, no
duplicate old chooser rows, and temporary story-label saves/loads. Built-in command
labels must survive story switching and cannot be overwritten by legacy lf rows.
The EditMode fixture explicitly registers/initializes child Forms because runtime
registration callbacks do not run there. It never edits user story/save directories.
Interaction argument checks invoke the real UiAction button listener and execute
its event program. Character `param1` must carry its Product UID as `$ch$`, Object
`param1` its scene Unit UID as `$so$`; `param2` remains the player Product. Scene
event ownership still uses the emitting Unit/Data identity. Missing player or empty
pooled row data must not launch an event. Existing BoundaryTouch references remain
unchanged.
Current-layer covered-roof checks also hide elevated Objects when one covers the
player's exact Tile, extending through cardinal Tiles with elevated Object coverage.
Foreign owners may seed/bridge the component; changing only belongTile preserves
hiding, while removing bridge/seed coverage restores disconnected roofs.
Actual footprint refresh checks use an offset/scaled roof with an out-of-view
owner and no visible-list entry, including movement, rotation and scale changes.
Every physical Collider bottom must exceed `tile.pos.y + 1` (world units, with
`0.0001` tolerance). Bottoms between ground and that upper boundary, equality,
within-layer multipart bodies and within-layer bridges do not hide/connect roofs.
They cover child Collider offsets, padded Triggers, rotated physical Spheres,
multipart ground contact, Trigger-only/missing bodies, texture-fitting, view edges,
nonzero layers, omitted visible-list Objects, diagonal exclusions/foreign coverage,
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
