using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using Z_Math;
using Z_Mesh;
using static Z_Math.Graph;
using Debug = UnityEngine.Debug;
using Mesh = Z_Mesh.Mesh;

// Native Editor checks against the pre-optimization SAT implementation below.
public static class CubeCollisionRegression
{
    private static int checks;
    private static float sink;
    private sealed class Pair { public MeshInfo a, b; public Vector3 step; }

    public static void Run()
    {
        try
        {
            var random = new System.Random(18403);
            var benchmark = new List<Pair>();
            for (int i = 0; i < 6000; i++)
            {
                float origin = i % 2 == 0 ? 0 : 500;
                var aCenter = RandomVector(random, 4) + Vector3.one * origin;
                var bCenter = aCenter + RandomVector(random, 5);
                var aSize = new Vector3(Next(random, .02f, 4), Next(random, .02f, 4), Next(random, .02f, 4));
                var bSize = new Vector3(Next(random, .02f, 4), Next(random, .02f, 4), Next(random, .02f, 4));
                if (i % 19 == 0) aSize.y = 0;
                if (i % 31 == 0) bSize.x = 0;
                var aEuler = i % 3 == 0 ? Vector3.zero : RandomVector(random, 180);
                var bEuler = i % 5 == 0 ? aEuler : RandomVector(random, 180);
                var pair = new Pair
                {
                    a = Mesh.GetMesh(aCenter, aEuler, aSize),
                    b = Mesh.GetMesh(bCenter, bEuler, bSize),
                    step = i % 11 == 0 ? Vector3.zero : RandomVector(random, 8)
                };
                Compare(pair);
                Compare(new Pair { a = pair.b, b = pair.a, step = -pair.step });
                if (i < 128) benchmark.Add(pair);
            }
            foreach (Vector3 step in new[] { Vector3.zero, Vector3.right, Vector3.left, Vector3.up,
                         new Vector3(10, 0, 0), new Vector3(.001f, 0, 0) })
            foreach (float gap in new[] { -1f, 0f, .00001f, .01f, 2f })
                Compare(new Pair { a = Mesh.GetMesh(Vector3.zero, Vector3.zero, Vector3.one),
                    b = Mesh.GetMesh(new Vector3(1 + gap, 0, 0), Vector3.zero, Vector3.one), step = step });

            var changed = benchmark[0];
            changed.a.positions = Graph.GetCubeEightPoint(new Vector3(7, 2, 3), new Vector3(2, 1, 3), new Vector3(0, 39, 0));
            Compare(changed);
            changed.a.positions[7] += new Vector3(.2f, .3f, -.1f);
            Compare(changed);
            for (int i = 0; i < changed.a.positions.Length; i++) changed.a.positions[i] += Vector3.one * 500;
            Compare(changed);

            // Warm both paths before comparing identical batches of pairs.
            for (int i = 0; i < 4; i++) Measure(benchmark, 4, i % 2 == 0);
            double reference = double.MaxValue, optimized = double.MaxValue;
            for (int i = 0; i < 3; i++)
            {
                reference = Math.Min(reference, Measure(benchmark, 300, false));
                optimized = Math.Min(optimized, Measure(benchmark, 300, true));
            }
            Debug.Log($"CUBE_COLLISION_BENCHMARK pairs={benchmark.Count * 300} referenceMs={reference:F2} cachedMs={optimized:F2} sink={sink}");
            Debug.Log("CUBE_COLLISION_REGRESSION_PASS checks=" + checks);
            EditorApplication.Exit(0);
        }
        catch (Exception error) { Debug.LogException(error); EditorApplication.Exit(1); }
    }

    private static void Compare(Pair pair)
    {
        var expected = CubeSatReference.CubeIntersectCube(pair.a.positions, pair.b.positions, pair.step, out float distance, out var normal);
        var actual = Graph.CubeIntersectCube(pair.a.positions, pair.b.positions, pair.step, out float rawDistance, out var rawNormal);
        Check(actual == expected && Mathf.Abs(rawDistance - distance) <= .00001f
            && (expected == IntersectType.None || (normal - rawNormal).sqrMagnitude < .0000001f), "array SAT agrees with the original algorithm");
        actual = Mesh.MeshIntersectMesh(pair.a, pair.b, pair.step, out rawDistance, out rawNormal);
        // The old one-item IntersectAssisant starts with oldIn=false.
        var classified = expected == IntersectType.Inner ? IntersectType.In : expected == IntersectType.Out ? IntersectType.None : expected;
        Check(actual == classified && Mathf.Abs(rawDistance - Mathf.Min(pair.step.magnitude, distance)) <= .00001f
            && (expected == IntersectType.None || (normal - rawNormal).sqrMagnitude < .0000001f), "cached Mesh dispatch preserves type, distance and avoidance");
    }

    private static double Measure(List<Pair> pairs, int rounds, bool cached)
    {
        var timer = Stopwatch.StartNew();
        for (int i = 0; i < rounds; i++)
        foreach (var pair in pairs)
        {
            float distance;
            if (cached) Mesh.MeshIntersectMesh(pair.a, pair.b, pair.step, out distance, out _);
            else CubeSatReference.CubeIntersectCube(pair.a.positions, pair.b.positions, pair.step, out distance, out _);
            sink += distance;
        }
        timer.Stop();
        return timer.Elapsed.TotalMilliseconds;
    }

    private static float Next(System.Random random, float min, float max) => min + (float)random.NextDouble() * (max - min);
    private static Vector3 RandomVector(System.Random random, float range) => new Vector3(Next(random, -range, range), Next(random, -range, range), Next(random, -range, range));
    private static void Check(bool condition, string message) { if (!condition) throw new InvalidOperationException(message + " at check " + checks); checks++; }
}

internal static class CubeSatReference
{
    private static readonly HashSet<Vector3> exist = new HashSet<Vector3>(10);
        public static IntersectType CubeIntersectCube(Vector3[] aCubeEightPoints, Vector3[] bCubeEightPoints, Vector3 dir, out float dis, out Vector3 avoidDir)
        {
#if DEBUG_GRAPH
            string tmp = "";
            foreach (var o in aCubeEightPoints)
                tmp += o.ToString("F10") + " ";
            Debug.Log($"[graph]{Time.frameCount}aCubeSize:" + tmp);
            tmp = "";
            foreach (var o in bCubeEightPoints)
                tmp += o.ToString("F10") + " ";
            Debug.Log($"[graph]{Time.frameCount}bCubeSize:" + tmp);
            Debug.Log($"[graph]{Time.frameCount}dir:" + dir.ToString("F10"));
#endif
            avoidDir = Vector3.zero;

            float mag = dir.magnitude;
            dis = mag;
            Vector3 aCenter = (aCubeEightPoints[0] + aCubeEightPoints[7]) * 0.5f;
            Vector3 bCenter = (bCubeEightPoints[0] + bCubeEightPoints[7]) * 0.5f;
            exist.Clear();



            float touchTime = 0;
            float avoidTime = 1;

            Vector3 normal1, normal2, normal3;
            GetFaceNormals(aCubeEightPoints, out normal1, out normal2, out normal3);

            if (AddAxisCheck(normal1, exist)) CheckAxis(aCubeEightPoints, bCubeEightPoints, dir, normal1, ref touchTime, ref avoidTime, ref avoidDir);
            if (AddAxisCheck(normal2, exist)) CheckAxis(aCubeEightPoints, bCubeEightPoints, dir, normal2, ref touchTime, ref avoidTime, ref avoidDir);
            if (AddAxisCheck(normal3, exist)) CheckAxis(aCubeEightPoints, bCubeEightPoints, dir, normal3, ref touchTime, ref avoidTime, ref avoidDir);

            GetFaceNormals(bCubeEightPoints, out normal1, out normal2, out normal3);

            if (AddAxisCheck(normal1, exist)) CheckAxis(aCubeEightPoints, bCubeEightPoints, dir, normal1, ref touchTime, ref avoidTime, ref avoidDir);
            if (AddAxisCheck(normal2, exist)) CheckAxis(aCubeEightPoints, bCubeEightPoints, dir, normal2, ref touchTime, ref avoidTime, ref avoidDir);
            if (AddAxisCheck(normal3, exist)) CheckAxis(aCubeEightPoints, bCubeEightPoints, dir, normal3, ref touchTime, ref avoidTime, ref avoidDir);


            if (avoidTime < 0)
                avoidTime = 0;
            if (touchTime > 1)
                touchTime = 1;
            if (touchTime > avoidTime)
            {
                touchTime = 1;
            }
            bool fromIn = false;
            bool toIn = false;
            if (touchTime == 0)
            {
                fromIn = true;
            }

            if (touchTime < 1)
            {
                toIn = true;
            }

            if (fromIn)//out
            {
                avoidTime = 1;
                touchTime = 0;
                if (Vector3.Dot((aCenter - bCenter), dir) >= 0)
                    touchTime = 1;
            }

            dis = mag * touchTime;
            avoidDir = avoidDir.normalized;
            return GetIntersectRes(fromIn, toIn, touchTime < 1);
        }
        private static bool AddAxisCheck(Vector3 axis, HashSet<Vector3> exist)
        {
            if (axis.sqrMagnitude <= 0) return false;
            if (exist.Contains(axis)) return false;
            exist.Add(axis);
            return true;
        }
        private static void CheckAxis(Vector3[] aCubeEightPoints, Vector3[] bCubeEightPoints, Vector3 dir, Vector3 axis,
            ref float touchTime, ref float avoidTime, ref Vector3 avoidDir)
        {
            (float aMin, float aMax) = ProjectCubeOntoAxis(aCubeEightPoints, axis);
            (float bMin, float bMax) = ProjectCubeOntoAxis(bCubeEightPoints, axis);

            float dirProj = Vector3.Dot(dir, axis);

            if (CalcTouchTimeAndAvoidTime(aMin, aMax, bMin, bMax, dirProj, ref touchTime, ref avoidTime, out var avoid))
                return;
            avoidDir += (avoid * axis).normalized;
        }
        private static (float min, float max) ProjectCubeOntoAxis(Vector3[] cubeEightPoints, Vector3 axis)
        {
            float min = float.MaxValue;
            float max = float.MinValue;
            for (int i = 0; i < 8; i++)
            {
                float proj = Vector3.Dot(cubeEightPoints[i], axis);
                if (proj < min) min = proj;
                if (proj > max) max = proj;
            }
            return (min, max);
        }
        private static bool CalcTouchTimeAndAvoidTime(float aMin, float aMax, float bMin, float bMax, float dir, ref float touchTime, ref float avoidTime, out int avoidDir)
        {
            avoidDir = 0;
            if (aMax <= bMin)// [] {} 或 恰好相切(aMax==bMin)
            {
                if (dir > 0)
                {
                    avoidDir = -1;
                    touchTime = Mathf.Max(touchTime, (bMin - aMax) / dir);
                    avoidTime = Mathf.Min(avoidTime, (bMax - aMin) / dir);
                }
                else
                {
                    avoidDir = 1;
                    touchTime = 1;
                    avoidTime = 0;
                    return true;
                }

            }
            else if (aMax > bMin && aMax < bMax && aMin < bMin)//[{]}
            {
                if (dir > 0)
                {
                    avoidDir = -1;
                    touchTime = Mathf.Max(touchTime, 0);
                    if (dir != 0) avoidTime = Mathf.Min(avoidTime, (bMax - aMin) / dir);
                }
                else if (dir < 0)
                {
                    avoidDir = 1;
                    touchTime = Mathf.Max(touchTime, 0);
                    avoidTime = Mathf.Min(avoidTime, (aMax - bMin) / -dir);
                }
                else // dir==0: 已重叠且无此轴移动，已触碰，不修改avoidTime(避免除以零)
                {
                    avoidDir = -1;
                    touchTime = Mathf.Max(touchTime, 0);
                }
            }
            else if (aMax < bMax && aMin > bMin)//{[]}
            {
                if (dir > 0)
                {
                    touchTime = Mathf.Max(touchTime, 0);
                    if (dir != 0) avoidTime = Mathf.Min(avoidTime, (bMax - aMin) / dir);
                    avoidDir = (bMax - aMin) > (aMax - bMin) ? -1 : 1;
                }
                else if (dir < 0)
                {
                    touchTime = Mathf.Max(touchTime, 0);
                    avoidTime = Mathf.Min(avoidTime, (aMax - bMin) / -dir);
                    avoidDir = (bMax - aMin) > (aMax - bMin) ? 1 : -1;
                }
                else // dir==0
                {
                    touchTime = Mathf.Max(touchTime, 0);
                    avoidDir = (bMax - aMin) > (aMax - bMin) ? -1 : 1;
                }
            }
            else if (aMax > bMax && aMin < bMin)//[{}]
            {
                if (dir > 0)
                {
                    touchTime = Mathf.Max(touchTime, 0);
                    if (dir != 0) avoidTime = Mathf.Min(avoidTime, (bMax - aMin) / dir);
                    avoidDir = (bMax - aMin) > (aMax - bMin) ? -1 : 1;
                }
                else if (dir < 0)
                {
                    touchTime = Mathf.Max(touchTime, 0);
                    avoidTime = Mathf.Min(avoidTime, (aMax - bMin) / -dir);
                    avoidDir = (bMax - aMin) > (aMax - bMin) ? 1 : -1;
                }
                else // dir==0
                {
                    touchTime = Mathf.Max(touchTime, 0);
                    avoidDir = (bMax - aMin) > (aMax - bMin) ? -1 : 1;
                }
            }
            else if (aMax > bMax && aMin > bMin && aMin < bMax)//{[}]
            {
                if (dir > 0)
                {
                    avoidDir = 1;
                    touchTime = Mathf.Max(touchTime, 0);
                    if (dir != 0) avoidTime = Mathf.Min(avoidTime, (bMax - aMin) / dir);
                }
                else if (dir < 0)
                {
                    avoidDir = -1;
                    touchTime = Mathf.Max(touchTime, 0);
                    avoidTime = Mathf.Min(avoidTime, (aMax - bMin) / -dir);
                }
                else // dir==0
                {
                    avoidDir = 1;
                    touchTime = Mathf.Max(touchTime, 0);
                }
            }
            else if (aMin >= bMax)//{}[] 或 恰好相切(aMin==bMax)
            {
                if (dir >= 0)
                {
                    avoidDir = 1;
                    touchTime = 1;
                    avoidTime = 0;
                    return true;
                }
                else
                {
                    avoidDir = -1;
                    touchTime = Mathf.Max(touchTime, (aMin - bMax) / -dir);
                    avoidTime = Mathf.Min(avoidTime, (aMax - bMin) / -dir);
                }
            }

            return false;
        }
        private static void GetFaceNormals(Vector3[] cubeEightPoints, out Vector3 normal1, out Vector3 normal2, out Vector3 normal3)
        {
            Vector3 p0 = cubeEightPoints[(int)CubeEightPoint.LeftDownBack];
            Vector3 p1 = cubeEightPoints[(int)CubeEightPoint.LeftUpBack];
            Vector3 p2 = cubeEightPoints[(int)CubeEightPoint.LeftDownForward];
            Vector3 p3 = cubeEightPoints[(int)CubeEightPoint.RightDownBack];

            normal1 = Vector3.Cross(p1 - p0, p2 - p0).normalized;
            normal2 = Vector3.Cross(p2 - p0, p3 - p0).normalized;
            normal3 = Vector3.Cross(p3 - p0, p1 - p0).normalized;
        }
        private static IntersectType GetIntersectRes(bool fromIn, bool toIn, bool intersected)
        {
            if (fromIn && !toIn)
            {
                return IntersectType.Out;
            }
            if (!fromIn && toIn)
            {
                return IntersectType.In;
            }
            if (!fromIn && !toIn && intersected)
            {
                return IntersectType.Cross;
            }
            if (fromIn && toIn)
            {
                return IntersectType.Inner;
            }
            return IntersectType.None;
        }
}
