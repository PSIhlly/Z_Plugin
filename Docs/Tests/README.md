# Map runtime regression

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
group operations, End/re-entry, and zero allocations in warmed overhead frames.

Success is reported as `MAP_RUNTIME_REGRESSION_PASS` in the printed log path.
These focused EditMode checks do not replace Play-mode movement/trigger tests,
visual shader verification, target-device profiling, or an IL2CPP Player build.
