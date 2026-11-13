# Map runtime regression

`Run-MapRuntimeRegression.ps1 -Fixture UiTreeContainerRegression` runs native
Play-mode UiHolder pooling and UGUI/TMP sizing in a disposable project. It covers
deep dynamic trees, subtree removal, flattening before reuse, stale parameter
release, ordinary visibility changes, destroyed nodes, 58 ten-argument event rows
(1218 Units), warm instance reuse, nested operator Units, pooled Line selection,
actual editor mode switches/Return, and coalesced intrinsic layout requests.
Success: `UI TREE CONTAINER REGRESSION PASS`. Pair with
`SceneEventChooseListRegression` for the separate global Grid rebuild contract.
This does not benchmark the user's live event or an IL2CPP Player.

`python Docs/Tests/test_excel_copy_generation.py` compiles and executes Copy methods emitted by the real Excel2Cs generator in an isolated .NET project. Covers direct Form fields, nested List/Dictionary containers, tuple entries, nulls, ID behavior, shared acyclic rows, two actual mutually referencing objects from different Forms, and copying successfully after a cycle error. It uses temporary test data without changing authored workbooks.

`Run-MapRuntimeRegression.ps1 -Fixture FormCopyRegression` runs native Unity copies of real Object Product/model/event/parameter/Box Forms. Checks independent nested state, inherited live-parameter cache hooks, saved-payload equivalence, dictionary comparers, instance-based self/mutual cycles and exception cleanup, Product binding without JSON, and subsequent parameter-key edits. It reports a warmed 512-copy comparison with the equivalent JSON round trip; allocation assertions require a positive explicit-allocation calibration because the Editor counter can be unavailable. Success is `FORM_COPY_REGRESSION_PASS`. Pair it with `ObjectProductRuntimeRegression` for current-schema placement preparation and save/reload; neither test loads the current game or user saves.

`Run-MapRuntimeRegression.ps1 -Fixture CanvasToastRegression` runs native Play-mode
Canvas/TMP, bounded frame timers and the real instance pool. Covers multiple
toasts, Reset/OnDisable with immediate reuse, canceled callbacks, externally
destroyed text, 20 FPS animation completion versus the former three-second
recycle, ordinary 60 FPS completion, Character Remove, scene End/reentry and
external Canvas reuse ownership. No production scene or save is loaded.
Success: `CANVAS_TOAST_REGRESSION_PASS`.

`pwsh -File Docs/Tests/Run-TeamerSelectRegression.ps1` compiles the actual
ShowTeamerSelect/SelectOneTeamer commands and TeamSelectWindow controllers against lightweight
UI/service spies, without launching Unity or reading/modifying stories/saves.
Checks void management-window metadata/close/replacement behavior, the separate
generic selector's character return, all team members including deployed members,
stable duplicate-name rows, empty/dismissed selections, deleted or UID-reused
Products, lost membership, independent concurrent choices and cancellation.
This is a controller/command logic regression, not a native UI or Player test.
Success: `TEAMER_SELECT_REGRESSION_PASS`.

`Run-MapRuntimeRegression.ps1 -Fixture InterpreterTimeoutRegression` checks the
one-second real execution-time cap per root event per frame, including shared
subprogram time, same-frame repeated Interpret slices, final instructions and
async-start yields. It exercises the native monotonic clock with a slow command,
then deterministic accumulated-tick boundary cases. New frames and Reset release
the previous time; suspended/external time and unrelated events do not count.
Checks the normal scheduler's error-popup packet (program, source line/column,
opcode, elapsed time), event removal, Pause release and task cancellation without
rendering production UI or opening actual saves. The 4096-instruction yield limit
is retained. Success: `INTERPRETER_TIMEOUT_REGRESSION_PASS`.

`Run-MapRuntimeRegression.ps1 -Fixture LenCommandRegression` runs the real Len
command/interpreter with dense, sparse, named/noncanonical, overflowing and
same-count-mutated dictionary keys, plus randomized legacy-equivalence cases.
A 1024-element loop reproduction exercises repeated condition calls. It calibrates
the native allocation counter with a retained allocation and skips allocation-only
claims if that counter is unavailable. No actual story/save is loaded. Success:
`LEN_COMMAND_REGRESSION_PASS`.

`Run-MapRuntimeRegression.ps1 -Fixture PlayAssetTextRegression` tests the real
ShowText/ShowImage commands, PlayAsset controller, generated Image view, native
TMP/Img siblings and UiContainer pooling. Covers literal multiline and empty
content, and checks the actual saved PlayAsset prefab's three Image-holder
references before constructing native test rows (catching stale Generate bindings).
Also covers
preferred size, base-font-relative scaling without compounding, shared
position/angle/opacity/removal handles, actual Graphic alpha, pure Refresh and
single-step update timers, stale image/controller cleanup and missing images.
Old image JSON (including untouched-opacity compatibility versus explicit fades)
and mixed image/text SaveUiItem/LoadUiItem round-trip through a
fixture-owned temporary directory; no real scene, UI registry or save is edited.
Checks the exact ShowText default assignment, expression-only argument selection
and statement insertion without duplicated prefixes/terminators. TMP settings,
the imported Liberation Sans font and its SDF shader are fixture-owned dependencies.
Success marker: `PLAY_ASSET_TEXT_REGRESSION_PASS`.

`Run-MapRuntimeRegression.ps1 -Fixture GenerateItemRegression` exercises the real
`GenerateItem(item,pos)` command, GameUtil prefab composition, pooled appearance,
nearest-lower Tile support and the Item pickup operations. Covers independent
Product/model/parameter/event copies, retained quantity, exact Unit handles with
GetPosition/Destroy, world coordinates on nonuniform cells, invalid input/resource
rollback and safe pool replacement after Product UID reuse. It runs actual scene
map save/load in a fixture-owned temporary folder, never a real story/save. The
production shader is imported for material/texture checks; this is not a GPU test.
Success marker: `GENERATE_ITEM_REGRESSION_PASS`.

`Run-MapRuntimeRegression.ps1 -Fixture GifDecoderRegression` checks the real GIF decoder and native Texture2D pixels for sequential/interlaced scanlines, short and odd image heights, image-descriptor offsets, canvas orientation, and ordinary animated GIF colors/delays. It also checks first-frame and animation-cache sharing. Optional `-GifReferenceDirectory <directory>` copies `.gif` files and matching `.rgba` reference files into the isolated project: each RGBA file is the independently decoded first frame, four bytes per pixel, rows ordered from bottom to top. These single-frame reference images are checked pixel for pixel through `TexAssetForm.Data`, and decoded PNG previews are written only inside the temporary project. No source artwork, production assets or saves are changed. Success is `GIF_DECODER_REGRESSION_PASS`.

`Run-MapRuntimeRegression.ps1 -Fixture PartyHealthRegression` exercises HP-aware
current-Character selection through the real party controller and commands. Covers
immediate HP-parameter changes, paused-frame and scene-entry repair, all-dead
clearing, idempotent scene-refresh notifications, dead UI/command/prototype switch
rejection, membership fallback and compatibility with unbound/missing HP. The
real `GetDeployedCharacters()` and `GetTeamCharacters()` commands retain party
order and defeated members, omit invalid/duplicate IDs, return independent Box
snapshots, compile/round-trip and support existing `Len`/index operations. Scene
refresh uses an interface spy and saved Progress uses an in-memory Form round-trip;
no current scene, camera, authored story or real save is loaded or modified.
Success marker: `PARTY_HEALTH_REGRESSION_PASS`.

`Run-MapRuntimeRegression.ps1 -Fixture IsStunCommandRegression` checks the real
`IsStun(character)` query and its compiled/interpreted use. Covers one-argument
Character/one-numeric-result metadata, explicit registration, default source and
Form round-trip, `CharacterStun` timer/reapplication, exact expiration, default-zero
time, read-only result ownership, concrete/prototype/unique Character references,
live-scene scalar selection, missing Progress and invalid/removed/wrong-kind
references. It runs only in a disposable project, not the current scene or saves.
Success marker: `IS_STUN_COMMAND_REGRESSION_PASS`.

`Run-MapRuntimeRegression.ps1 -Fixture SkillUseCommandsRegression` runs the actual
two-argument `UseSkill(character,skill)` and four equipped LightAttack/HeavyAttack/E/Q
commands against `PlayInfoController.UseSkill`. Checks metadata/default compilation,
event/clock ownership, cooldown enabled versus ignored and exact expiry, global
skill enablement, current-slot replacement, prototype fan-out/unique characters,
reentrant UID reuse, malformed/missing slots/references and unloaded controllers.
Legacy source migration covers quoted punctuation, omitted/grouped third
arguments and nested calls. Actual `LoadEvent` must recompile an old three-argument
call despite stale bytecode, keep source-map counts aligned and leave its original
temporary event file unchanged.
It never opens a production story or save. Success marker:
`SKILL_USE_COMMANDS_REGRESSION_PASS`; real HUD casting/skill visuals need a Play smoke test.

`Run-MapRuntimeRegression.ps1 -Fixture CameraCommandsRegression` checks the real
`MoveCamera(offset,seconds)`, `MoveCameraTo(position,seconds)` and `RestoreCamera()`
commands in an isolated Unity project. Covers Scene CmdLab metadata/default-source
compilation, relative versus absolute coordinates with nonuniform map scaling,
bounded linear timing, immediate/invalid values, normal-follow suppression and
current-player restoration, overlapping commands, same-frame task polling,
scene/map/cancellation ownership, registration/disable/Play-End resets and unchanged
camera-child pose/zoom. It never opens a production scene or save. Success marker:
`CAMERA_COMMANDS_REGRESSION_PASS`; actual camera/shake rendering and scene entry
still need a Mod/Play smoke test.

`Run-MapRuntimeRegression.ps1 -Fixture EmptyMediaAssetsRegression` covers shared preserved IDs, idempotent initialization, all three empty marker references, concrete Story picker selection, default deletion protection after external renaming, replacement guards, normal deletion, silent audio/instant video completion and real command calls without player/Dialog setup. Uses an isolated filesystem root to validate ordinary hashed backing files, default manifests, file-free empty audio/video, reload and Story reset.

`Run-MapRuntimeRegression.ps1 -Fixture NavigationObjectStateRegression` checks that noncolliding Objects affect neither navigation blocking nor pass requirements; disabled/moved/removed footprints clean up immediately. Native List versions verify zero link rebuilds over 100 unchanged-state moves and overlapping blockers. Covers same-cell directional threshold changes, incoming links, pass-only updates, terrain height/solid-volume invalidation, full/local equivalence, new-map state and scene/config Product collision synchronization. Runs in an isolated project without user saves.

`Run-MapRuntimeRegression.ps1 -Fixture ObjectCollisionTilesRegression` covers Object Collider/Trigger sweeps, root size changes, translated cached geometry, rotated center offsets, long and reverse movement, diagonal corner rejection, separated parts, disabled/no meshes, rotated thin Boxes, Sphere capsule footprints, duplicate Tile/Unit indices, real delayed Trigger Enter/Exit and nonuniform cell sizes. It runs without a game scene or user save.

`Run-MapRuntimeRegression.ps1 -Fixture CubeCollisionRegression` compares array and cached Mesh SAT results with the pre-optimization algorithm across 12,000 deterministic random pair directions plus touching, embedded, thin/zero-sized, large-coordinate and mutated-array cases. It validates type/distance/avoidance and prints a warmed timing comparison for identical pair batches. Timings describe the isolated Editor process, not a Player FPS improvement.

`Run-MapRuntimeRegression.ps1 -Fixture SceneObjectReferenceRegression` covers only canonical numeric Object Product references and explicit scene-Unit handles, rejects old alias spellings, and checks overlapping UID pools, prototype/exact targeting and real parameter/destruction commands. Reverse-index cases cover 2,000 warm scalar rounds, duplicate placements/minimum-UID selection, incremental add/remove/productInfo rebind/raw-extra replacement, stable snapshots, late Product registration/replacement and reset. The retired runtime migration/version-cache assertions are removed.

`Run-MapRuntimeRegression.ps1 -Fixture SceneBehaviourTriggerRegression` checks the scene-unit trigger list and pool reuse, protected inherited prefixes, editable/deletable local suffixes, duplicate-index selection, row-local history capture, Form round-trip, live prototype changes, cache invalidation, stale callback rejection and normal Story-window editing after pool reuse. The shared TriggerEventChoose Model renders inherited/local variable groups plus New, chooses Any or a stable global UID, reuses Scene parameter rename validation and serializes local create/rename/event edits/deletes without mutating inherited groups. It rejects renaming onto an inherited binding, preserves variable confirmation across same-owner refresh, rejects choices after owner replacement/hide/deletion, verifies immediate manager activation and releases its pooled rows and owned window. The parameter fixture matches the actual label-inside-edit-Button hierarchy, checking readable variable names, editable local parameters and disabled inherited parameters. Native Canvas/GraphicRaycaster checks cover TouchPass parent hits, underlying blocking UI, nested normal Buttons, disabled markers and preserved UGUI close clicks. Uses real production controllers in an isolated Unity project; actual prefab layout still needs an Editor smoke check.

`Run-MapRuntimeRegression.ps1 -Fixture SceneEventChooseListRegression` exercises the real scene-event Model controllers and UiContainer pooling: all twelve trigger rows, refresh/reopen instance reuse, minimap filtering without saved-data loss, current event-window binding, single click registration, and rejection of stale trigger-type callbacks after switching Product dictionaries. Native GridLayoutGroup/ContentSizeFitter/ScrollRect fixtures verify correct dimensions as soon as UiManager.Rebuild returns for active components, including a Grid behind a plain RectTransform wrapper, narrow/wide parent transitions, final row spacing and content height, pool reuse, clear and reopen. A native early-OnEnable fixture reproduces enabled layout components that are not yet active, then verifies that the first Canvas submission corrects both Grid height and the enclosing fixed-child-height layout, remains stable on subsequent submissions, skips hidden roots and preserves explicitly disabled components. Activation handling and Grid correction live inside UiManager.Rebuild; event-list controllers own no delayed rebuild. Actual prefab presentation and mouse/touch raycasts still need an Editor smoke test.

Product reference checks run in independent temporary Unity projects:

- `Run-MapRuntimeRegression.ps1 -Fixture CharacterReferenceRegression`: Character constant registration/typed chooser metadata and literal compilation, exact/runtime versus prototype Character resolution, current-scene snapshots, unique singleton, invalid references, recycled Data identity, team/current remapping and independent skill copies. Cache checks verify reuse of `_characterDic` and its Unit groups across 2,000 warm exact/prototype/absent-unit query rounds, cached GetOrNewCharacter, duplicate-Product placements with minimum-UID selection, stable plural snapshots, add/remove and productInfo-rebinding invalidation, and same-UID Product replacement. In Mod's event editor, manually select a `character` argument, choose the basic `Character` constant, select a Story Character, then Apply/reopen to confirm the quoted `$ch$ProductUid$ch$` persists; canceling must preserve the prior argument.
- `Run-MapRuntimeRegression.ps1 -Fixture CharacterPartyRegression`: five Character party commands, command metadata/default compilation, prototype recruitment and fan-out, unique Characters, team/active/current invariants, idempotence, current-character fallback, deliberate reserve-only save reload versus legacy invalid-UID repair, invalid/omitted references, HUD/trigger null safety and existing team-slot UI entry points. Test actual HUD refresh/player switching in Play mode too.
- `Run-MapRuntimeRegression.ps1 -Fixture CharacterRemovalRegression`: real legacy `DestroyCharacter` execution through Product and scene-Unit references for unique/non-unique Characters. Checks synchronous instance/registry/index/interaction cleanup, repeated deletion, preserved unique Product/parameter identities and party references, recreating a scene instance from that Product, and rejection of stale Data after Unit UID reuse. Also runs the real current-Character refresh: full rotation/position handoff to existing/new Characters, immediate Instance synchronization, clearing stale pending rotation, unchanged-current idempotence and first/empty-current selection compatibility. This focused fixture has no old Renderer API dependencies and never loads a story or save. Success is `CHARACTER_REMOVAL_REGRESSION_PASS`; actual scene UI and Player behavior still need a smoke check.
- `Run-MapRuntimeRegression.ps1 -Fixture UnifiedDestroyRegression`: checks canonical `Destroy` metadata/scene Lab and both legacy aliases; Character/Object prototype fan-out and exact references; Item Product/scene references; unique Character preservation; registry, instance, interaction and owned-event/task cleanup; malformed/mismatched/Tile no-ops and reentrant UID reuse. Pre-destruction hooks cover still-live `param1`/GetPosition, prototype-before-local order, asynchronous completion before removal/caller continuation, nested Pause ownership, recursive self/mutual destruction, cancellation/reset cleanup and reused-UID protection. Item destruction removes matching scene drops, never backpack entries or Item Products. Uses the real command and removal owners without loading a story/save. Success is `UNIFIED_DESTROY_REGRESSION_PASS`; real scene/Player smoke checks remain separate.
- `Run-MapRuntimeRegression.ps1 -Fixture ObjectProductRuntimeRegression`: current-schema Cache placement preparation, independent nested Product state and parameter overrides, idempotence, no Core rewrite or live Unit registration, direct Unit-to-Product lookup, parameter-key publishing, Core/Save filtering, save/reload across unloaded scenes and reset. Extra replacement checks immediate Form-action invalidation of Product, parameter and local/combined event caches, rebuilding from the new Extra, preserved real Product values and write-through Product bindings.
- `Run-MapRuntimeRegression.ps1 -Fixture SceneObjectReferenceRegression`: canonical numeric Product and explicit Unit IDs, rejection of retired aliases, `IsSameObject` prototype comparisons and interpreter calls, parameter fan-out and saved cached values, unloaded Product reads/writes, malformed refs and actual multi-instance removal. Prototype comparison covers exact prototypes, sibling/self Products, different/zero prototypes, both invalid argument positions, unloaded Products and recycled Unit IDs.
- `Run-MapRuntimeRegression.ps1 -Fixture SceneProductCommandRegression`: Character/Object template versus exact position/move/rotation, independent relative starts, bounded time, legacy duration aliases, original Data identity and all-target completion. It does not compile the unrelated renderer probes in the broad map fixture.
- `Run-MapRuntimeRegression.ps1 -Fixture StoryTargetSelectionRegression`: registers and compiles all Story 1 programs, compiling the `vector`-returning `朝向敌人` first, then executes the real compiler/interpreter with deterministic command probes. Covers nearest visible selection across self-first/unsorted/reversed candidates, friendly/occluded exclusion, higher/lower targets, forward fallback, raw query/distance coordinates and explicitly raised LOS endpoints. Executes player shot events, the enemy ranged/Sword chain, the beam's projectile routing, straight movement and homing; checks returned 3D directions, Sword's doubled speed, nonzero height displacement and unchanged movement durations. A real `IsUnobstructed`/`CaptureCast` low-wall case requires unraised endpoints to hit and caller-raised endpoints to pass. Exports the seven changed events (`5`, `6`, `7`, `19`, `20`, `23`, `28`) to `compiled-bullet-events.json` inside the isolated project; actual Play aiming/projectile rendering still requires a smoke check.

These Editor checks do not replace Mod/Play scene-switch or target Player smoke tests.

`Run-MapRuntimeRegression.ps1 -Fixture ModSceneHistoryRuntimeRegression -ItemPicking`
generates real Item prefabs with zero/small gameplay collider scales and nested
Box/Sphere triggers, then checks mouse down/up selection in normal/Event mode,
overhead/side view and after pool reuse. Play/preview collision scale, authored
Item/MapModel data and source prefabs must retain their original values.

`Run-LoadingProgressRegression.ps1` executes the production LoadingManager and
UiLoading text controller with in-memory UI/frame dependencies. It covers work
counts, visibility guards, retained completed children, discovered work, monotonic
text, session reset, duplicate/stale reports, async failure/cancellation, parallel
worker increments, detailed substage counts, render budgets, generated bilingual
stage titles (including unchanged percentages and language changes). Loading does
not write runtime diagnostic logs.

`Run-ObjectWangTileNeighbourRegression.ps1` executes the actual WangTile mask,
footprint and neighbour invalidation methods with geometry/column/appearance spies.
It checks add/move/remove, old/new templates, hidden/fitted neighbours, ordinary
instances sharing a WangTile template, first-notification ordering, contact tolerance
and repeated warm calls. The 10,000-call unchanged workload must make zero column
queries; this is a deterministic call-count check, not a Player FPS benchmark.

`Run-MapRuntimeRegression.ps1 -Fixture FadeCommandsRegression` checks the actual
FadeIn/FadeOut commands, numeric-duration metadata, old omitted-argument defaults,
black-image start/mid/end alpha at custom unscaled durations and immediate closing
for nonpositive/nonfinite input. Covers independent FadeOut from stale/hidden graphic
state, first-Canvas timing despite a simulated loading stall, replacement/reopening/
Hide callback cleanup, and immediate continuation by the real interpreter.
Visually check the registered PlayEffect prefab in Play too.

`Run-SceneTransitionRegression.ps1` runs the production Play transition/unload
methods and LoadingManager with in-memory frame/save/map/UI spies. It checks
Loading render before save, a single old-map save, stopped Play updates, reentry
guards, asynchronous completion, failure cleanup and first-event loading ownership.
`Run-MapRuntimeRegression.ps1 -Fixture WangTileStoryCacheRegression` checks
WangTile generated-ID/Texture reuse across scene resets, configuration/same-ID
asset/animation invalidation, and full Story release followed by fresh generation.
Run two scene switches in Play to verify Loading presentation and end-to-end
latency in the actual game.

`Run-MapRuntimeRegression.ps1 -Fixture MapRelatedUnitRefreshRegression` exercises
the actual Object Show -> GameMapController appearance -> RefreshObjectOverlap
chain with a facing different from the pooled template, proving that reverse
Tile lists really change while every initial Object still becomes visible.
Character/Item/Object probes also cover self/peer unlink, reorder and insertion
from Show/Hide, latest multi-Tile visibility, cross-type snapshot capture,
synchronous nested refresh, exception-safe buffer return and warm list reuse,
non-mutating absent-key reads and single-unit refresh. It uses no scene/save or
shader rendering. Success is `MAP_RELATED_UNIT_REFRESH_REGRESSION_PASS`;
real Mod/Play entry and view movement still need a smoke check.

`Run-MapRuntimeRegression.ps1 -Fixture ObjectProductEditorRegression` checks Product-backed creation/deletion, concrete label ownership, transient MapObject list projection and stable row identity across refreshes. After one successful production build, use `-SkipBuild` for additional isolated fixtures to avoid competing writes to the same compiler output; it deliberately assumes the existing binaries match the current source.

The Object editor fixture also checks New Collider scale `1` through the editor
and `AddStorySceneObject`, preserving the shared MapModel default and existing/
copied authored scales. `Run-MapRuntimeRegression.ps1 -Fixture AssetPickerSelectionRegression`
checks current-image typed ID lookup, its own Label instead of All, missing/empty/
wrong-kind fallback, inaccessible-label All fallback that retains selection/focus,
pending-focus lifetime and dual-list completion, and absence of completion/events or
automatic media playback. Actual Object frame/minimap calls pass their stored
image through UiManager; Image command literal, empty and variable inputs keep
the typed current ID and quoted callback marker contract. Real `ScrView.ScrollToIndex` tests cover offscreen,
first/last and invalid indices, horizontal/vertical lists, nonuniform Canvas/
content scaling, stopped inertia and partial virtualization. This tests data and
native layout geometry without opening real story files; visual highlight and
UGUI input still need a manual smoke check. Success is
`ASSET_PICKER_SELECTION_REGRESSION_PASS`.

`Run-MapRuntimeRegression.ps1 -Fixture MapEdgeTransitionRegression` builds the
production runtime, then tests the actual edge component source in an isolated
Unity project with only a minimal column-map context. It checks the Euclidean
transform against brute-force distances, reduced convex-corner gradient creases,
mirrored concave/convex outlines, unchanged straight-edge profiles, black borders,
zero/sub-cell fade widths, interior holes, mask budget and work-buffer reuse.
The log includes a warmed rebuild duration and an `edge-corners-preview.png`
showing unsoftened Euclidean edges (left) and default corner smoothing (right).
The preview samples the actual Alpha8 mask with GPU-equivalent texel centers and
the unchanged shader profile; it is not a GPU screenshot or a full-map smoke test.
Check the real Mod/Play view after adjusting `cornerSmoothing` (default 0.5 cell,
zero disables), including large-map adaptive mask resolution. Per-frame drawing
still uses one texture sample and one quad; smoothing only runs during rebuilds.

`Run-MapRuntimeRegression.ps1 -Fixture TileFrontVisibilityRegression` checks that
configured Tile fronts stay visible beside same-name neighbours in Play and Mod.
It uses the real DisFade shader and texture/animation entry points, covering
offscreen neighbours, other prefab names/case, lower/forward/side neighbours,
body/front separation, subsequent vision changes, continuing front animation,
disabled/empty/restored front configuration and a new owner on the same instance
hierarchy. This is an isolated Editor check;
native pool callbacks and final scene rendering still need Play-mode verification.

`Run-MapRuntimeRegression.ps1 -Fixture WangTileSlopeNeighbourRegression` checks
the production Tile WangTile mask without opening a scene or save. It covers all
eight directions and 256 lower-slope combinations, exact next-layer lookup,
logical texture layers/IDs, missing/null texture dictionaries, same-layer
priority, non-slope rejection, arbitrary lower-slope rotation and negative/zero
coordinates. A slope itself also sees the uphill cardinal and two diagonals one
layer above for all four facings, including wrapped yaws; this additional path
cannot suppress a matching same-height Tile. Base, front and minimap samplers
must select the same mixed upper/lower mask. Local
edit refresh probes cover the neighbouring 3-by-3 on the same/adjacent heights,
overlapping-batch deduplication, removal, unchanged distant Tiles and view bounds.
Success is `WANG_TILE_SLOPE_NEIGHBOUR_REGRESSION_PASS`; actual Mod/Play slope-edge
rendering still needs a visual smoke check.

`Run-MapRuntimeRegression.ps1 -Fixture ObjectDirectionGeometryRegression` checks
Object-facing `modelSizes`/`modelPositions`: legacy/partial-data fallback, five
independent entries, facing-mode changes, JSON round trips, independent runtime
Products, bottom-preserving height/axis edits, directional previews, offscreen
body/Trigger and visual bounds, independent 0.1-world-unit Trigger padding,
immediate live-facing changes, two simultaneous facings and pooled-owner handoff
without changing the shared prefab or Unit root. Visual, physical-navigation and
terrain-pass candidate indexes must update their old/new Tile links exactly.
Texture-fitted, rear-anchored Cube and Sphere-to-Box models are also compared
against independent directional previews for body/Trigger vertices at ordinary
and nonuniform root scales, including repeated facing changes.
Growing from a zero-height default direction must also produce finite,
ordinary-sized body/Trigger geometry identical to an independently built facing.
Success is `OBJECT_DIRECTION_GEOMETRY_REGRESSION_PASS`. This isolated data/geometry
fixture does not establish final Mod/Play rendering or target-Player behavior.

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
The authored Stick source and Q/E effective prefab flags are checked statically for
sibling center/area layout, a raycast-enabled center and disabled Q/E area Graphic,
including inherited flags without redundant instance overrides. The saved nested
`img_` binding and non-raycastable icon/mask hierarchy are checked too.
Actual `UiPlaySceneMainCtrl` Q/E callback/setup/refresh helpers are extracted too:
center press captures zero offset, edge drag maps screen XY to world XZ, Q/E bind
their own skills, Refresh preserves an owned aim, and blocking/skill-disable/cancel
still ends it without casting or clearing another skill's option. Original raycast
authoring remains unchanged; these spies do not run native Unity GraphicRaycaster.
Play Move/LightAttack checks cover visible idle knobs, fixed authored centers,
neutral center/active edge presses, independent crossing fingers, release/cancel
recentering without hiding, preserved area Graphics, and hide/reopen ownership reset.
Generic floating-mode checks retain idle-hidden knobs, transparent active hit regions,
press origins, scaled relative drags, cancellation/block hiding and fixed-mode reuse.
Android move-facing checks cover initial movement, neutral/long attack holds,
the 0.1-second release/cancel boundary, renewed input, Q/E priority and desktop exclusion.
Skill icon checks cover LightAttack/E/Q equipment, character switches, retained
aim ownership, missing Skill/texture/zero IDs, empty current Character, fresh Params
and hide/reopen. Icon refresh leaves movement visuals and the raycastable knobs intact.
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
It also compares WritePackage's streamed UTF-8 bytes with the legacy Package
JSON, checks odd-offset/incremental SHA-256 writes against the managed reference,
cancellation during compression, and multi-megabyte stored-DEFLATE PNGs through
both the managed decoder and Unity's native image decoder/re-encoder. Windows
uses CNG for the same SHA-256 digest; non-Windows retains the managed fallback.
Large exports avoid full JSON/payload copies and redundant IDAT recompression;
the on-disk version, checksums and import compatibility are unchanged. For a
large real-story performance check, read only its Core into an isolated test
process, time EncodeStreamWithCover plus writing a temporary PNG, then compare
the decoded payload byte-for-byte with the original Package output. Never
benchmark by importing over an existing user story or writing to its Core.
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

`Run-MapRuntimeRegression.ps1 -Fixture NavigationRouteCacheRegression` exercises
the completed full-route cache using the production BFS and controlled logical
frames, without loading a story or scene. A 118-edge one-cell-wide serpentine
corridor prevents smoothing beyond the first adjacent cell. At speed 6 and
60 FPS, warm routes must advance every frame through multiple turns while new
search rounds remain pending; consuming many cached waypoints in one logical
frame must not replenish the ten-cell search budget or lose a route when search
working buffers are cleared. It also checks independent Characters sharing one
BFS, ordinary target changes, removed/blocked/pass-restricted cells, removed
directional links, Reset and radius/capability changes, plus the exact final
non-center coordinate without backtracking. This verifies native vector/query
semantics and invokes the actual Character remaining-distance method, its real
Move and ApplyMove chain against a registered Sphere prefab, spending a large
single-frame distance across several waypoints and consecutive corners without
refilling the BFS budget. That synthetic graph has no physical Tiles/obstacles,
so it does not replace collision/Trigger or real Player timing checks; replay
the normal movement smoke checks and confirm frame-time in the target scene
separately.
Even-sized Character queries also carry explicit logical size. Size 2 must use
the lower-right Tile as junction anchor and return that Tile's upper-left corner
as the world waypoint. Tests cross two-cell-wide corridors in both orientations,
steer from an off-center broad room into/out of the passage while the full body
stays on supported cells, reject a one-cell corridor, and retain size-1 centers
and size-4 clearance. Pass requirements use the right-down Tile consistently;
size/parity changes invalidate cached coordinates without refilling the budget.
A real scaled Sphere Character executes the same path through Move/ApplyMove.
Sphere navigation radius must match the physical collision radius at every yaw,
including world coordinates near 500 and sizes 1 through 4; rotated six-point
samples are not world-axis sphere bounds. A real size-2 Character replays near
and distant pursuit with target updates every 30 frames and alternating command
and movement facing, retaining its cached route without backward realignment
and without refilling the search budget.
The fixture covers blocked/missing/pass-restricted starts and retained escape
centers separately from target selection. An invalid destination is replaced
BEFORE BFS by the first footprint/pass-safe point in increasing world Euclidean
distance; equal-distance choices are arbitrary. Tests distinguish Euclidean from
square rings, expand beyond three cells, compare different height layers, and
verify size=2 clearance. Pending target lookup must spend at most ten work items
per frame and leave the BFS visit table empty. Arrival stops at the replacement,
including same-cell motion of the original blocked coordinate; disconnected
chosen candidates retain normal BFS failure behavior. Start escape permissions,
manual pass restrictions and teleport reset remain covered.
A fixed 1,600-cell grid compares size=1/2 startup, edge targets, blocked starts
and fresh queries after Reset. Tests reuse only the shared map geometry, while
checking live obstacle changes. Real Tile prefabs verify Build/Rebuild index and
bounds publication, stable graph/node identity, terrain/cell-size invalidation,
and irregular-center fallback. Logged frame counts describe deterministic search
latency, not measured Player frame time.
Small open-grid checks retain center-line-only pass requirements on smoothed
diagonals, immediate cached-raw fallback when a target's within-cell offset would
cross a restricted neighbour, completion without repeated waits, and adoption of
a changed target direction during an unfinished far smoothed leg.
Success is `NAVIGATION_ROUTE_CACHE_REGRESSION_PASS`.

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
`Material.SetShaderPassEnabled` and `GetShaderPassEnabled` address the pass's
LightMode (`UniversalForward`); `Material.FindPass` addresses its ShaderLab Name
(`Universal Forward`). Native GPU submission must verify suppression, because
setting and reading the same incorrect key can agree while the pass still draws.
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

Use `-Fixture MapShadowBiasRegression` to GPU-test the production ShadowCaster's
directional/punctual depth bias, transformed-normal bias and near-plane clamp.
It covers origin and `(496, 500, 519)`, ordinary and actual instanced draws,
hidden-unit silhouettes and texture-alpha holes in an isolated D3D11 Editor.
Success is `MAP_SHADOW_BIAS_REGRESSION_PASS`; revisit the reported Scene view
with shadows enabled to confirm the original black line is gone.

Use `-Fixture MapShadowReceiverRegression` to draw the actual opaque and partial-alpha
color passes against URP's 1x1 empty shadow texture and its `(1, 0, 1, 0)` shadow
parameters. A retained world-to-shadow matrix produces a false strip with the
realtime-only GetMainLight overload; the world-position overload must apply URP's
distance fade and leave no dark strip. The 96 cases cover origin and
`(496, 500, 519)`, three camera yaws, 1x/2x MSAA, enabled/disabled shadow keywords,
and both color passes with real instanced Tile/front Quads. Disabling the fade
while retaining shadow strength is a negative control: the strip must remain,
proving ordinary shadows were not globally disabled. Success is
`MAP_SHADOW_RECEIVER_REGRESSION_PASS`. This does not replace confirmation in the
reported Scene view.

Use `-Fixture DisFadeBatchingRegression` for the WangTile batching regression.
It generates the real 47 variants from a uniform white sheet, proving that equal
pixels and material display names do not imply shared Texture/Material objects.
Native GPU instance IDs verify that 32 Tiles using one variant share one draw,
while two depth-interleaved variants split into 32 draws with transparent sorting.
The same two variants must use two 16-instance draws through the production
`DisFadeOpaqueFeature` pass. The fixture executes its real queue filter, opaque
sorting and camera cleanup, rather than only testing a separately configured SRP.
Overlapping variant planes also check depth after swapping near/far positions
and material groups, and after hiding the nearer Tile.
The same fixture draws only `UniversalForward` with the shader's split discard
disabled: Opaque must submit no pixels for split cameras and render for fallback
cameras, while Transparent/Mixed remain visible and Hidden remains absent. A
control disables the ShaderLab Name and still draws; disabling the LightMode
must remove that native draw. This catches incorrect pass keys even when their
setter/getter checks agree.
Success is `DIS_FADE_BATCHING_REGRESSION_PASS`; the test runs in an isolated D3D11
project and does not load the current story.

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
switches by LightMode (`UniversalForward`) and pass lookup by Name (`Universal Forward`),
Hidden color-pass suppression across texture/camera/show transitions,
canonical transparent material sharing for empty Tile base/front slots across
instances, animation cancellation, stable empty bindings through visibility/display
changes and pool preparation, and restoration of newly configured textures,
authored shadow-pass and Renderer/shadow-mode preservation, split-camera cleanup
and Character first Show/reuse from a disabled
template, including part enablement and visibility during texture replacement.
Mod layer-selection checks keep all normal/front pairs displayed, cap higher
pairs at half opacity without multiplying existing occlusion, preserve full
hiding, and verify repeat selection plus Event/Play restoration and lower-layer lighting.
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
Object-picking checks drive actual pointer Down/Up clicks through near-to-far,
Unit-deduplicated ray candidates in normal/Event mode and overhead/side view.
Repeated clicks advance from the current selected Unit and wrap to the top;
changed lists, duplicate Colliders, single candidates and empty rays retain
consistent behavior without editing-height/Collider-height prioritization.
Event mode includes Tiles in the cycle; normal mode omits them. The focused
ItemPicking entry also retains gameplay-collision-scale and pool-reuse checks.
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
`param1` its Product UID as `$so$`; `param2` remains the player Product. Scene
event ownership still uses the emitting Unit/Data identity. Missing player or empty
pooled row data must not launch an event. BoundaryTouch keeps its scene-Unit
identity for Characters through explicit `$so$u:UnitUid$so$` handles.
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

## Global variable command regression

```powershell
./Docs/Tests/Run-MapRuntimeRegression.ps1 -Fixture GlobalVariableRegression
```

Uses an isolated runtime project to check `GetGlobalVar(name)` / `SetGlobalVar(name,value)`
registration, basic CmdLab metadata and compiler/interpreter stack balance. Covers
numeric, string and nested dictionary Boxes, overwrite and copy isolation, missing
and empty names, sharing across event heaps and subprograms, plus the actual `egvf`
file save/load path. Missing files, null loads and consecutive story folders must
clear stale globals. Filesystem checks use only a fixture-owned temporary directory;
no production story or save is loaded or changed. Success marker:
`GLOBAL_VARIABLE_REGRESSION_PASS`.

## Pause-owned event tasks and remaining Wait

```powershell
./Docs/Tests/Run-MapRuntimeRegression.ps1 -Fixture PauseTasksRegression
```

Runs real Pause/Continue, scheduler, interpreter Wait, Progress updates and Object
movement steps in an isolated map. Checks other events' Wait/countdowns and tasks
freeze without catch-up, the Pause owner and required child/subprogram tasks keep
running, immediate first polls cannot bypass blocking, and paused tasks still
cancel/reset cleanly. Progress.seconds remains frozen while the owner's Wait can
continue to Continue. No production scene/save is loaded. Success marker:
`PAUSE_TASKS_REGRESSION_PASS`.

## Story projectile Team/damage snapshots

```powershell
./Docs/Tests/Run-MapRuntimeRegression.ps1 -Fixture StoryProjectileDamageRegression
```

Runs migrated Story 1 attack, homing, Character-hit and scenery-hit source through
the actual compiler/interpreter with deterministic scene-command probes. Checks
every projectile receives numeric `所属Team` and captured `伤害`, same-Team/self
contact has no hit or Destroy side effect, defense clamps damage at zero, neutral
rules remain unchanged, scenery uses the same captured damage, and behavior does
not dereference a removed shooter or use its later attack/Team changes. An optional
`-ProjectileEventDirectory` copies only `ef` files beneath an explicit preview
directory into the disposable project, allowing repository and local-save previews
to be checked together. No live scene/save is loaded or modified. Success marker:
`STORY_PROJECTILE_DAMAGE_REGRESSION_PASS`.

## Scene global-variable change triggers

```powershell
./Docs/Tests/Run-MapRuntimeRegression.ps1 -Fixture GlobalVariableSceneTriggerRegression
```

Exercises the actual `SetGlobalVar` command, change event, Story trigger listener,
queued event interpreter and normal LateUpdate scheduling in an isolated project.
Bindings use the global UID (`onGlobalVarChangeEvent$UID`), or the empty Any suffix.
Scene-panel and Unit bindings share `param2 = variable name`,
`param3 = new value - old value` (signed numeric delta, like Character parameter changes).
`param1` is zero for Scene bindings and the exact owner reference for Unit bindings.
The full new-value snapshot remains in `param4`; text/dictionary changes still fire,
with numeric delta zero. Includes simultaneous Scene Any/exact and all four Unit
types for the `格挡1` regression, decreases and fractional deltas.
Checks include deep equality and type changes, rename-stable matching, independent
handler snapshots, same-value reassignments, scene/Mod/Leave guards and Scene JSON
round trips. The actual Scene configuration builder and chooser callbacks also
cover Any/UID selection, unused New keys, duplicate-target rejection and stale
Scene/global-record protection. No production scene, UI or save is loaded. Success marker:
`GLOBAL_VARIABLE_SCENE_TRIGGER_REGRESSION_PASS`.
The same fixture checks global bindings on Object/Character/Item Products and
Tile main textures, prototype-before-instance execution, scene-owner cancellation,
and deletion from live bindings and nested unloaded-map JSON without removing Any
or unrelated saved fields.

## ModStory typed defaults and global-variable editor

```powershell
./Docs/Tests/Run-MapRuntimeRegression.ps1 -Fixture ModStoryVariableEditorRegression
```

Runs the actual editor controllers and shared value chooser in an isolated project,
without loading the production UI or story. Covers numeric/Text selection, correct
text prefill and completion, immediate serialized-default updates before copying a
new Product parameter, template/pool safety, Event/global-variable page selection,
global creation/rename/deletion and separate Core defaults versus Save values.
Filesystem checks use fixture-owned temporary folders only. Success marker:
`MOD_STORY_VARIABLE_EDITOR_REGRESSION_PASS`. Also retain `GlobalVariableRegression`
for command execution, deep Box copies and the `egvf` persistence lifecycle.

World-unit coordinate checks: `SceneProductCommandRegression` repeats grouped Character/Object Get/Set/Move with nonuniform `(2,3,4)` cells and checks direct normalization, yaw, area radius, navigation and mission targets. `CameraCommandsRegression` verifies world offsets and absolute +500 positions on nonuniform cells. `Run-MapRuntimeRegression.ps1 -Fixture StoryTargetSelectionRegression -LineOfSight` checks the same real low-wall LOS at tall and short cell sizes without depending on editable Story 1 programs. It also replays the logged cherry-tree pass-through in both directions with radii 0.2/0.3/0.45 and yaw/slope rotations, checks first-contact normals, preserves outward/embedded escape and ground tangency, and exercises real `CaptureCast`/`IsUnobstructed` on the tree. `MapRuntimeRegression -SceneCommands` retains scene/mission coverage with world-unit expectations. Grid indices, Tile geometry and minimap projection remain cell-based internally.
