using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

namespace Z_Math
{
    public static class Graph
    {
        public static float DELTA = 0.0005f;
        public static bool dDebug;

        private static readonly Vector3[] s_CubeCornerOffsets = new Vector3[]
        {
            new Vector3(-0.5f, -0.5f, -0.5f),
            new Vector3(0.5f, -0.5f, -0.5f),
            new Vector3(-0.5f, 0.5f, -0.5f),
            new Vector3(0.5f, 0.5f, -0.5f),
            new Vector3(-0.5f, -0.5f, 0.5f),
            new Vector3(0.5f, -0.5f, 0.5f),
            new Vector3(-0.5f, 0.5f, 0.5f),
            new Vector3(0.5f, 0.5f, 0.5f)
        };

        private static readonly Vector3[] s_SphereDirections = new Vector3[]
        {
            Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back
        };

        private static readonly int[] s_CubeClosePointOffsets = new int[]
        {
            4, -4, 1, -1, 2, -2
        };

        public class IntersectAssisant
        {
            int isOut = 0;
            int isIn = 0;
            int isCross = 0;
            bool oldIn;
            public IntersectAssisant(bool oldIn)
            {
                this.oldIn = oldIn;
            }
            public void Add(IntersectType type)
            {
                switch (type)
                {
                    case IntersectType.Inner:
                        isIn = -1;
                        isOut = -1;
                        isCross = -1;
                        break;
                    case IntersectType.In:
                        isOut = -1;
                        isCross = -1;
                        if (isIn == 0)
                            isIn = 1;
                        break;
                    case IntersectType.Out:
                        isIn = -1;
                        isCross = -1;
                        if (isOut == 0)
                            isOut = 1;
                        break;
                    case IntersectType.Cross:
                        if (isCross == 0)
                            isCross = 1;
                        break;
                }
            }
            public void Merge(IntersectAssisant assist)
            {
                if (assist.isIn == -1)
                    isIn = -1;
                if (isIn == 0 && assist.isIn == 1)
                    isIn = 1;

                if (assist.isOut == -1)
                    isOut = -1;
                if (isOut == 0 && assist.isOut == 1)
                    isOut = 1;

                if (assist.isCross == -1)
                    isCross = -1;
                if (isOut == 0 && assist.isCross == 1)
                    isCross = 1;
            }
            public IntersectType GetRes()
            {
                if (isCross == 1)
                {
                    return oldIn ? IntersectType.Out : IntersectType.Cross;
                }
                else if (isIn == 1)
                {
                    return oldIn ? IntersectType.Inner : IntersectType.In;
                }
                else if (isOut == 1)
                {
                    return oldIn ? IntersectType.Out : IntersectType.None;
                }
                else if (isIn == -1 && isOut == -1 && isCross == -1)
                {
                    return oldIn ? IntersectType.Inner : IntersectType.In;
                }
                return oldIn ? IntersectType.Out : IntersectType.None;
            }
        }
        public enum IntersectType
        {
            None,
            In,
            Out,
            Cross,
            Inner
        }
        public enum FourDir
        {
            Up,
            Right,
            Down,
            Left,
        }

        public enum CubeEightPoint
        {
            LeftDownBack,
            RightDownBack,

            LeftUpBack,
            RightUpBack,

            LeftDownForward,
            RightDownForward,

            LeftUpForward,
            RightUpForward,
        }
        public enum SphereSixPoint
        {
            Up,
            Down,
            Left,
            Right,
            Forward,
            Back
        }
        #region Color
        public static Color NewSetR(this Color c, float r)
        {
            return new Color(r, c.g, c.b, c.a);
        }
        public static Color NewSetG(this Color c, float g)
        {
            return new Color(c.r, g, c.b, c.a);
        }
        public static Color NewSetB(this Color c, float b)
        {
            return new Color(c.r, c.g, b, c.a);
        }
        public static Color NewSetA(this Color c, float a)
        {
            return new Color(c.r, c.g, c.b, a);
        }
        #endregion

        #region Vector3
        public static Vector3 NewSetX(this Vector3 v3, float x)
        {
            return new Vector3(x, v3.y, v3.z);
        }
        public static Vector3 NewSetY(this Vector3 v3, float y)
        {
            return new Vector3(v3.x, y, v3.z);
        }
        public static Vector3 NewSetZ(this Vector3 v3, float z)
        {
            return new Vector3(v3.x, v3.y, z);
        }
        public static List<Vector3Int> DeduplicateIntPos(List<List<Vector3Int>> lst)
        {
            HashSet<Vector3Int> exist = new HashSet<Vector3Int>();
            int count = lst.Count;
            for (int i = 0; i < count; i++)
            {
                var subList = lst[i];
                int subCount = subList.Count;
                for (int j = 0; j < subCount; j++)
                {
                    exist.Add(subList[j]);
                }
            }
            return new List<Vector3Int>(exist);
        }
        public static Vector3 ElementwiseMultiply(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        }
        public static Vector3 ElementwisePlus(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
        }
        public static Vector3[] ElementwisePlus(Vector3[] a, Vector3 b)
        {
            int len = a.Length;
            Vector3[] res = new Vector3[len];
            for (int i = 0; i < len; i++)
            {
                res[i] = new Vector3(a[i].x + b.x, a[i].y + b.y, a[i].z + b.z);
            }
            return res;
        }
        public static Vector3 ElementwiseDivide(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
        }
        public static Vector3Int GetVector3Int(Vector3 a)
        {
            return new Vector3Int((int)Math.Round(a.x), (int)Math.Round(a.y), (int)Math.Round(a.z));
        }
        public static bool IsClamp(Vector3 point, Vector3 a1, Vector3 a2)
        {
            return !((point.x > a1.x && point.x > a2.x) || (point.x < a1.x && point.x < a2.x)
                     || (point.y > a1.y && point.y > a2.y) || (point.y < a1.y && point.y < a2.y)
                     || (point.z > a1.z && point.z > a2.z) || (point.z < a1.z && point.z < a2.z));
        }
        #endregion

        #region 3D

        public static Vector3[] GetCubeEightPoint(Vector3 center, Vector3 size, Vector3 euler)
        {
            Vector3[] ans = new Vector3[8];
            Quaternion rotation = Quaternion.Euler(euler);
            for (int i = 0; i < 8; i++)
            {
                ans[i] = center + rotation * ElementwiseMultiply(s_CubeCornerOffsets[i], size);
            }
            return ans;
        }
        public static Vector3[] GetSphereSixPoint(Vector3 center, float radius, Vector3 euler, Vector3 scale)
        {
            Vector3[] ans = new Vector3[6];
            Quaternion rotation = Quaternion.Euler(euler);
            for (int i = 0; i < 6; i++)
            {
                ans[i] = center + rotation * ElementwiseMultiply(s_SphereDirections[i] * radius, scale);
            }
            return ans;
        }


        public static List<Vector3Int> GetRoughOverlapIntPos(Vector3[] points)
        {
            int len = points.Length;
            int minX = int.MaxValue;
            int minY = int.MaxValue;
            int minZ = int.MaxValue;
            int maxX = int.MinValue;
            int maxY = int.MinValue;
            int maxZ = int.MinValue;
            for (int i = 0; i < len; i++)
            {
                Vector3 p = points[i];
                int px = (int)p.x;
                int py = (int)p.y;
                int pz = (int)p.z;
                if (px < minX) minX = px;
                if (py < minY) minY = py;
                if (pz < minZ) minZ = pz;
                int px1 = px + 1;
                int py1 = py + 1;
                int pz1 = pz + 1;
                if (px1 > maxX) maxX = px1;
                if (py1 > maxY) maxY = py1;
                if (pz1 > maxZ) maxZ = pz1;
            }
            int sizeX = maxX - minX + 1;
            int sizeY = maxY - minY + 1;
            int sizeZ = maxZ - minZ + 1;
            List<Vector3Int> res = new List<Vector3Int>(sizeX * sizeY * sizeZ);
            for (int i = minX; i <= maxX; i++)
                for (int j = minY; j <= maxY; j++)
                    for (int k = minZ; k <= maxZ; k++)
                    {
                        res.Add(new Vector3Int(i, j, k));
                    }
            return res;
        }

        public static bool GetPointInSphere(Vector3 point, float a2, float b2, float c2)
        {
            return point.x * point.x / a2 + point.y * point.y / b2 + point.z * point.z / c2 <= 1e-9;
        }





        #region Intersect
        static HashSet<Vector3> exist = new HashSet<Vector3>(10);
        /// <summary>
        /// 立方体与立方体SAT碰撞检测
        /// 使用分离轴定理，检测两个立方体在给定移动方向上的碰撞
        /// 输出：dis=碰撞距离（可移动距离），avoidDir=避障方向法线
        /// 检测轴：A的面法线3个 + B的面法线3个，共6个轴
        /// </summary>
        public static IntersectType CubeIntersectCube(Vector3[] aCubeEightPoints, Vector3[] bCubeEightPoints, Vector3 dir, out float dis, out Vector3 avoidDir)
        {
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

        /// <summary>
        /// 单轴SAT检测：将两个立方体投影到给定轴上，计算碰触时间touchTime和脱出时间avoidTime
        /// 根据投影区间关系确定避障方向avoidDir（+1/-1，表示沿轴正/负方向避让）
        /// </summary>
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

        /// <summary>
        /// 球体与立方体SAT碰撞检测
        /// 检测轴：立方体的3个面法线 + 移动方向与立方体3条棱的叉积，共6个轴
        /// 输出：dis=碰撞距离，avoidDir=避障方向（用球心到立方体最近点方向计算）
        /// </summary>
        public static IntersectType SphereIntersectCube(Vector3[] sphereSixPoints, Vector3[] cubeEightPoints, Vector3 dir, out float dis, out Vector3 avoidDir)
        {
            avoidDir = Vector3.zero;
            bool fromIn = false;
            bool toIn = false;
            float mag = dir.magnitude;
            dis = mag;

            Vector3 sphereCenter = (sphereSixPoints[(int)SphereSixPoint.Right] + sphereSixPoints[(int)SphereSixPoint.Left]) * 0.5f;
            Vector3 disDir = GetPointToCube(sphereCenter, cubeEightPoints, out fromIn);

            if (fromIn)
            {
                if (mag == 0)
                {
                    return IntersectType.Inner;
                }
            }

            Vector3 newDisDir = GetPointToCube(sphereCenter + dir, cubeEightPoints, out var newInner);

            float sphereRadius = (sphereSixPoints[(int)SphereSixPoint.Right] - sphereSixPoints[(int)SphereSixPoint.Left]).magnitude * 0.5f;

            float touchTime = 0;
            float avoidTime = 1;
            exist.Clear();

            Vector3 normal1, normal2, normal3;
            GetFaceNormals(cubeEightPoints, out normal1, out normal2, out normal3);

            if (AddAxisCheckSphere(normal1, exist))
            {
                CheckSphereAxis(sphereCenter, sphereRadius, cubeEightPoints, dir, normal1, ref touchTime, ref avoidTime);
            }
            if (AddAxisCheckSphere(normal2, exist))
            {
                CheckSphereAxis(sphereCenter, sphereRadius, cubeEightPoints, dir, normal2, ref touchTime, ref avoidTime);
            }
            if (AddAxisCheckSphere(normal3, exist))
            {
                CheckSphereAxis(sphereCenter, sphereRadius, cubeEightPoints, dir, normal3, ref touchTime, ref avoidTime);
            }

            if (dir.sqrMagnitude > 0)
            {
                Vector3 right, up, forward;
                GetCubeEdgeDirections(cubeEightPoints, out right, out up, out forward);

                if (right.sqrMagnitude > 0)
                {
                    Vector3 crossAxis = Vector3.Cross(dir, right);
                    if (crossAxis.sqrMagnitude > 0 && AddAxisCheckSphere(crossAxis.normalized, exist))
                    {
                        CheckSphereAxis(sphereCenter, sphereRadius, cubeEightPoints, dir, crossAxis.normalized, ref touchTime, ref avoidTime);
                    }
                }
                if (up.sqrMagnitude > 0)
                {
                    Vector3 crossAxis = Vector3.Cross(dir, up);
                    if (crossAxis.sqrMagnitude > 0 && AddAxisCheckSphere(crossAxis.normalized, exist))
                    {
                        CheckSphereAxis(sphereCenter, sphereRadius, cubeEightPoints, dir, crossAxis.normalized, ref touchTime, ref avoidTime);
                    }
                }
                if (forward.sqrMagnitude > 0)
                {
                    Vector3 crossAxis = Vector3.Cross(dir, forward);
                    if (crossAxis.sqrMagnitude > 0 && AddAxisCheckSphere(crossAxis.normalized, exist))
                    {
                        CheckSphereAxis(sphereCenter, sphereRadius, cubeEightPoints, dir, crossAxis.normalized, ref touchTime, ref avoidTime);
                    }
                }
            }

            if (IsSphereAndCubeOverlap(ElementwisePlus(sphereSixPoints, dir), cubeEightPoints))
            {
                toIn = true;
            }
            if ((fromIn && !newInner) ||
                (fromIn && newInner && newDisDir.sqrMagnitude < disDir.sqrMagnitude)
                || (!fromIn && !newInner && newDisDir.sqrMagnitude > disDir.sqrMagnitude))
            {
                touchTime = 1;
            }
            else
            {
                if (touchTime > avoidTime && avoidTime > 0)
                {
                    touchTime = 1;
                }
            }


            dis = touchTime * mag;
            if (touchTime < 1)
            {
                avoidDir = GetPointToCube(sphereCenter + dir * touchTime, cubeEightPoints, out _);
            }
            return GetIntersectRes(fromIn, toIn, touchTime < 1);
        }

        private static bool AddAxisCheckSphere(Vector3 axis, HashSet<Vector3> exist)
        {
            if (axis.sqrMagnitude <= 0) return false;
            if (exist.Contains(axis) || exist.Contains(-axis)) return false;
            exist.Add(axis);
            return true;
        }

        /// <summary>
        /// 球体单轴SAT检测：将球体（投影为区间[sphereMin,sphereMax]）和立方体投影到给定轴上
        /// </summary>
        private static void CheckSphereAxis(Vector3 sphereCenter, float sphereRadius, Vector3[] cubeEightPoints,
            Vector3 dir, Vector3 axis, ref float touchTime, ref float avoidTime)
        {
            float sphereCenterProj = Vector3.Dot(sphereCenter, axis);
            float sphereMin = sphereCenterProj - sphereRadius;
            float sphereMax = sphereCenterProj + sphereRadius;

            (float cubeMin, float cubeMax) = ProjectCubeOntoAxis(cubeEightPoints, axis);

            float dirProj = Vector3.Dot(dir, axis);

            CalcTouchTimeAndAvoidTime(sphereMin, sphereMax, cubeMin, cubeMax, dirProj, ref touchTime, ref avoidTime, out _);
        }

        public static IntersectType SphereIntersectSphere(Vector3[] aSphereSixPoints, Vector3[] bSphereSixPoints, Vector3 dir, out float dis, out Vector3 avoidDir)
        {
            avoidDir = Vector3.zero;
            bool fromIn = false;
            bool toIn = false;
            float mag = dir.magnitude;
            dis = mag;
            if (IsSpheresOverlap(aSphereSixPoints, bSphereSixPoints))
            {
                fromIn = true;
                if (mag == 0)
                {
                    return IntersectType.Inner;
                }
            }
            Vector3 aCenter = (aSphereSixPoints[(int)SphereSixPoint.Left] + aSphereSixPoints[(int)SphereSixPoint.Right]) * 0.5f;
            Vector3 bCenter = (bSphereSixPoints[(int)SphereSixPoint.Left] + bSphereSixPoints[(int)SphereSixPoint.Right]) * 0.5f;
            float aRadius = (aSphereSixPoints[(int)SphereSixPoint.Left] - aSphereSixPoints[(int)SphereSixPoint.Right]).magnitude * 0.5f;
            float bRadius = (bSphereSixPoints[(int)SphereSixPoint.Left] - bSphereSixPoints[(int)SphereSixPoint.Right]).magnitude * 0.5f;
            float touchTime = 0;
            float avoidTime = 1;


            {
                Vector3 diff = aCenter - bCenter;
                float sqrMag = diff.sqrMagnitude;
                Vector3 axis = sqrMag > 0 ? diff.normalized : Vector3.up;

                if (axis.sqrMagnitude > 0)
                {
                    float aCenterProj = Vector3.Dot(aCenter, axis);
                    float aMin = aCenterProj - aRadius;
                    float aMax = aCenterProj + aRadius;

                    float bCenterProj = Vector3.Dot(bCenter, axis);
                    float bMin = bCenterProj - bRadius;
                    float bMax = bCenterProj + bRadius;

                    float dirProj = Vector3.Dot(dir, axis);
                    CalcTouchTimeAndAvoidTime(aMin, aMax, bMin, bMax, dirProj, ref touchTime, ref avoidTime, out var avoid);

                    avoidDir += (avoid * axis).normalized;
                }

            }

            if ((aCenter - bCenter).sqrMagnitude < (aCenter + dir - bCenter).sqrMagnitude)
            {
                touchTime = 1;
            }
            else
            {
                if (touchTime > avoidTime)
                {
                    touchTime = 1;
                }
                if (IsSpheresOverlap(ElementwisePlus(aSphereSixPoints, dir), bSphereSixPoints))
                {
                    toIn = true;
                }
            }

            dis = touchTime * mag;

            avoidDir = avoidDir.normalized;
            return GetIntersectRes(fromIn, toIn, touchTime < 1);
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

        private static void GetCubeEdgeDirections(Vector3[] cubeEightPoints, out Vector3 right, out Vector3 up, out Vector3 forward)
        {
            Vector3 p0 = cubeEightPoints[(int)CubeEightPoint.LeftDownBack];
            Vector3 p1 = cubeEightPoints[(int)CubeEightPoint.RightDownBack];
            Vector3 p2 = cubeEightPoints[(int)CubeEightPoint.LeftUpBack];
            Vector3 p3 = cubeEightPoints[(int)CubeEightPoint.LeftDownForward];

            right = (p1 - p0).normalized;
            up = (p2 - p0).normalized;
            forward = (p3 - p0).normalized;
        }

        /// <summary>
        /// 判断一组向量是否全部位于某个半球内
        /// 遍历每个向量作为候选法线，检查其余所有向量与它的点积是否>=0（即在同一半球）
        /// 如果是，输出该半球法线方向hemisphereNormal，用于角色移动层判断是否可以沿避障方向滑行
        /// 当避障方向在同一半球内时，说明障碍物在同一侧，可以贴墙滑行
        /// </summary>
        public static bool IsVectorsInHemisphere(List<Vector3> vectors, out Vector3 hemisphereNormal)
        {
            hemisphereNormal = Vector3.zero;
            if (vectors == null || vectors.Count == 0)
                return true;

            int count = vectors.Count;
            for (int i = 0; i < count; i++)
            {
                var candidate = vectors[i];
                if (candidate.sqrMagnitude <= 0)
                    continue;
                candidate.Normalize();
                bool allInHemisphere = true;
                for (int j = 0; j < count; j++)
                {
                    if (i == j)
                        continue;
                    var v = vectors[j];
                    if (v.sqrMagnitude <= 0)
                        continue;
                    if (Vector3.Dot(candidate, v.normalized) < 0)
                    {
                        allInHemisphere = false;
                        break;
                    }
                }
                if (allInHemisphere)
                {
                    hemisphereNormal = candidate;
                    return true;
                }
            }
            return false;
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



        public static IntersectType LineIntersectCube(Vector3[] eightPoints, Vector3 from, Vector3 to, out float dis)
        {
            float mag = (to - from).magnitude;
            dis = mag;
            Vector3 forward = eightPoints[(int)CubeEightPoint.RightDownForward] - eightPoints[(int)CubeEightPoint.RightDownBack];
            Vector3 up = eightPoints[(int)CubeEightPoint.RightUpBack] - eightPoints[(int)CubeEightPoint.RightDownBack];
            Vector3 right = eightPoints[(int)CubeEightPoint.RightDownBack] - eightPoints[(int)CubeEightPoint.LeftDownBack];
            to = GetNewCoordinateVector(to, right, up, forward) - eightPoints[(int)CubeEightPoint.LeftDownBack];
            from = GetNewCoordinateVector(from, right, up, forward) - eightPoints[(int)CubeEightPoint.LeftDownBack];

            float x = right.magnitude;
            float y = up.magnitude;
            float z = forward.magnitude;
            if (to.x < 0 && from.x < 0)
                return IntersectType.None;
            if (to.x > x && from.x > x)
                return IntersectType.None;
            if (to.y < 0 && from.y < 0)
                return IntersectType.None;
            if (to.y > y && from.y > y)
                return IntersectType.None;
            if (to.z < 0 && from.z < 0)
                return IntersectType.None;
            if (to.z > z && from.z > z)
                return IntersectType.None;

            bool fromIn = from.x < x && from.x > 0 && from.y > 0 && from.y < y && from.z > 0 && from.z < z;

            bool toIn = to.x < x && to.x > 0 && to.y > 0 && to.y < y && to.z > 0 && to.z < z;

            if (fromIn && toIn)
            {
                return IntersectType.Inner;
            }

            Vector3 res;
            if (GetPointOnLineByX(from, to, 0, out res))
            {
                if (res.y < y && res.y > 0 && res.z > 0 && res.z < z)
                {
                    dis = Mathf.Min((res - from).magnitude, dis);
                }
            }
            if (GetPointOnLineByX(from, to, x, out res))
            {
                if (res.y < y && res.y > 0 && res.z > 0 && res.z < z)
                {
                    dis = Mathf.Min((res - from).magnitude, dis);
                }
            }
            if (GetPointOnLineByY(from, to, 0, out res))
            {
                if (res.x < x && res.x > 0 && res.z > 0 && res.z < z)
                {
                    dis = Mathf.Min((res - from).magnitude, dis);
                }
            }
            if (GetPointOnLineByY(from, to, y, out res))
            {
                if (res.x < x && res.x > 0 && res.z > 0 && res.z < z)
                {
                    dis = Mathf.Min((res - from).magnitude, dis);
                }
            }
            if (GetPointOnLineByZ(from, to, 0, out res))
            {
                if (res.x < x && res.x > 0 && res.y > 0 && res.y < y)
                {
                    dis = Mathf.Min((res - from).magnitude, dis);
                }
            }
            if (GetPointOnLineByZ(from, to, z, out res))
            {
                if (res.x < x && res.x > 0 && res.y > 0 && res.y < y)
                {
                    dis = Mathf.Min((res - from).magnitude, dis);
                }

            }


            return GetIntersectRes(fromIn, toIn, dis < mag);
        }
        public static IntersectType PointIntersectCube(Vector3[] eightPoints, Vector3 point, out float dis)
        {
            dis = 0;
            Vector3 forward = eightPoints[(int)CubeEightPoint.RightDownForward] - eightPoints[(int)CubeEightPoint.RightDownBack];
            Vector3 up = eightPoints[(int)CubeEightPoint.RightUpBack] - eightPoints[(int)CubeEightPoint.RightDownBack];
            Vector3 right = eightPoints[(int)CubeEightPoint.RightDownBack] - eightPoints[(int)CubeEightPoint.LeftDownBack];
            point = GetNewCoordinateVector(point, right, up, forward) - eightPoints[(int)CubeEightPoint.LeftDownBack];

            float x = right.magnitude;
            float y = up.magnitude;
            float z = forward.magnitude;

            if (point.x >= 0 && point.x <= x && point.y >= 0 && point.y <= y && point.z >= 0 && point.z <= z)
                return IntersectType.In;
            return IntersectType.Out;

        }
        public static IntersectType LineIntersectSphere(Vector3[] sixPoint, Vector3 from, Vector3 to, out float dis)
        {
            Vector3 dir = to - from;
            float mag = dir.magnitude;
            dis = mag;
            Vector3 center = (sixPoint[(int)SphereSixPoint.Forward] + sixPoint[(int)SphereSixPoint.Back]) * 0.5f;
            Vector3 forward = sixPoint[(int)SphereSixPoint.Forward] - center;
            Vector3 up = sixPoint[(int)SphereSixPoint.Up] - center;
            Vector3 right = sixPoint[(int)SphereSixPoint.Right] - center;
            to = GetNewCoordinateVector(to, right, up, forward) - center;
            from = GetNewCoordinateVector(from, right, up, forward) - center;

            float x = right.magnitude;
            float y = up.magnitude;
            float z = forward.magnitude;
            float a2 = x * x;
            float b2 = y * y;
            float c2 = z * z;

            if (to.x < -x && from.x < -x)
                return IntersectType.None;
            if (to.x > x && from.x > x)
                return IntersectType.None;
            if (to.y < -y && from.y < -y)
                return IntersectType.None;
            if (to.y > y && from.y > y)
                return IntersectType.None;
            if (to.z < -z && from.z < -z)
                return IntersectType.None;
            if (to.z > z && from.z > z)
                return IntersectType.None;

            float a = (dir.x * dir.x / a2 + dir.y * dir.y / b2 + dir.z * dir.z / c2);
            float b = 2 * (from.x * dir.x / a2 + from.y * dir.y / b2 + from.z * dir.z / c2);
            float c = (from.x * from.x / a2 + from.y * from.y / b2 + from.z * from.z / c2) - 1;

            float delta = b * b - 4 * a * c;
            if (delta < -1e-9)
            {
                return IntersectType.None;
            }


            float sqrtD = Mathf.Sqrt(delta);
            float t1 = (-b - sqrtD) / (2 * a);
            float t2 = (-b + sqrtD) / (2 * a);
            bool t1Valid = false;
            bool t2Valid = false;
            if (t1 >= 0 && t1 <= 1)
            {
                dis = Math.Min(t1 * mag, dis);
                t1Valid = true;
            }

            bool fromIn = GetPointInSphere(from, a2, b2, c2);
            if (t2 >= 0 && t2 <= 1)
            {
                dis = Math.Min(t2 * mag, dis);
                t2Valid = true;
            }

            if (t1Valid && t2Valid)
            {
                return IntersectType.Cross;
            }
            if (!t1Valid && !t2Valid)
            {

                return IntersectType.None;
            }
            if (fromIn)
                return IntersectType.Out;
            return IntersectType.In;
        }
        #endregion

        #endregion


        #region 3DUtil
        public static bool GetPointOnLineByX(Vector3 from, Vector3 to, float x, out Vector3 res)
        {
            res = Vector3.zero;
            Vector3 dir = to - from;
            float dirX = dir.x;
            if (Mathf.Abs(dirX) < 1e-9f)
                return false;
            float basic = (x - from.x) / dirX;
            res = new Vector3(x, basic * dir.y + from.y, basic * dir.z + from.z);
            return IsClamp(res, from, to);
        }
        public static bool GetPointOnLineByY(Vector3 from, Vector3 to, float y, out Vector3 res)
        {
            res = Vector3.zero;
            Vector3 dir = to - from;
            float dirY = dir.y;
            if (Mathf.Abs(dirY) < 1e-9f)
                return false;
            float basic = (y - from.y) / dirY;
            res = new Vector3(basic * dir.x + from.x, y, basic * dir.z + from.z);
            return IsClamp(res, from, to);
        }
        public static bool GetPointOnLineByZ(Vector3 from, Vector3 to, float z, out Vector3 res)
        {
            res = Vector3.zero;
            Vector3 dir = to - from;
            float dirZ = dir.z;
            if (Mathf.Abs(dirZ) < 1e-9f)
                return false;
            float basic = (z - from.z) / dirZ;
            res = new Vector3(basic * dir.x + from.x, basic * dir.y + from.y, z);
            return IsClamp(res, from, to);
        }
        public static Vector3[] RotatePointAroundOrigin(Vector3[] points, Vector3 euler)
        {
            int len = points.Length;
            Vector3[] newPos = new Vector3[len];
            Quaternion rotation = Quaternion.Euler(euler);
            for (int i = 0; i < len; i++)
            {
                newPos[i] = rotation * points[i];
            }
            return newPos;
        }
        public static Vector3 Clamp(Vector3 v, Vector3 min, Vector3 max)
        {
            if (v.x < min.x) v.x = min.x;
            if (v.x > max.x) v.x = max.x;
            if (v.y < min.y) v.y = min.y;
            if (v.y > max.y) v.y = max.y;
            if (v.z < min.z) v.z = min.z;
            if (v.z > max.z) v.z = max.z;
            return v;
        }

        public static Vector3 GetRandomVectorOnPlane(Vector3 v, float length)
        {
            Vector3 right = Vector3.Cross(v, Vector3.up);
            if (right.sqrMagnitude < 0.001f)
            {
                right = Vector3.Cross(v, Vector3.forward);
            }

            Vector3 forward = Vector3.Cross(v, right);

            float randomX = UnityEngine.Random.Range(-1f, 1f);
            float randomY = UnityEngine.Random.Range(-1f, 1f);

            Vector3 randomVector = (right * randomX + forward * randomY).normalized * length;

            return randomVector;
        }
        public static Vector3 GetDistanceVector3D(Vector3 linePoint, Vector3 lineDir, Vector3 targetPoint)
        {
            Vector3 w = targetPoint - linePoint;
            float dotVV = Vector3.Dot(lineDir, lineDir);
            if (dotVV < 1e-6f)
                return Vector3.zero;

            float t = Vector3.Dot(w, lineDir) / dotVV;
            Vector3 projection = t * lineDir;
            return w - projection;
        }
        private static bool IsCubesOverlap(Vector3[] cubeA, Vector3[] cubeB)
        {
            Vector3 normal1, normal2, normal3;

            GetFaceNormals(cubeA, out normal1, out normal2, out normal3);
            if (!CheckAxisOverlap(cubeA, cubeB, normal1)) return false;
            if (!CheckAxisOverlap(cubeA, cubeB, normal2)) return false;
            if (!CheckAxisOverlap(cubeA, cubeB, normal3)) return false;

            GetFaceNormals(cubeB, out normal1, out normal2, out normal3);
            if (!CheckAxisOverlap(cubeA, cubeB, normal1)) return false;
            if (!CheckAxisOverlap(cubeA, cubeB, normal2)) return false;
            if (!CheckAxisOverlap(cubeA, cubeB, normal3)) return false;

            return true;
        }

        private static bool CheckAxisOverlap(Vector3[] cubeA, Vector3[] cubeB, Vector3 axis)
        {
            if (axis.sqrMagnitude <= 0) return true;

            (float minA, float maxA) = ProjectCubeOntoAxis(cubeA, axis);
            (float minB, float maxB) = ProjectCubeOntoAxis(cubeB, axis);

            return IsOverlap(minA, maxA, minB, maxB);
        }

        public static bool IsSphereAndCubeOverlap(Vector3[] sphereSixPoints, Vector3[] cubeEightPoints)
        {
            Vector3 aCenter = (sphereSixPoints[(int)SphereSixPoint.Right] + sphereSixPoints[(int)SphereSixPoint.Left]) * 0.5f;
            float aRadius = (sphereSixPoints[(int)SphereSixPoint.Right] - sphereSixPoints[(int)SphereSixPoint.Left]).magnitude * 0.5f;
            Vector3 axis = GetPointToCube(aCenter, cubeEightPoints, out var inner).normalized;
            if (inner)
                return true;
            {
                float ellipsoidCenterProj = Vector3.Dot(aCenter, axis);
                float ellipsoidMin = ellipsoidCenterProj - aRadius;
                float ellipsoidMax = ellipsoidCenterProj + aRadius;

                (float cubeMin, float cubeMax) = ProjectCubeOntoAxis(cubeEightPoints, axis);

                if (ellipsoidMax <= cubeMin || cubeMax <= ellipsoidMin)
                    return false;
            }

            return true;
        }
        public static bool IsSpheresOverlap(Vector3[] aSphereSixPoints, Vector3[] bSphereSixPoints)
        {
            Vector3 aCenter = (aSphereSixPoints[(int)SphereSixPoint.Left] + aSphereSixPoints[(int)SphereSixPoint.Right]) * 0.5f;
            Vector3 bCenter = (bSphereSixPoints[(int)SphereSixPoint.Left] + bSphereSixPoints[(int)SphereSixPoint.Right]) * 0.5f;
            float aRadius = (aSphereSixPoints[(int)SphereSixPoint.Left] - aCenter).magnitude;
            float bRadius = (bSphereSixPoints[(int)SphereSixPoint.Left] - bCenter).magnitude;

            return (bCenter - aCenter).sqrMagnitude <= (aRadius + bRadius) * (aRadius + bRadius);
        }

        public static Vector3 GetPointToCube(Vector3 point, Vector3[] cubeEightPoints, out bool inner)
        {
            inner = false;
            int id = 0;
            float min2 = float.MaxValue;
            for (int i = 0; i < 8; i++)
            {
                Vector3 diff = cubeEightPoints[i] - point;
                float dis = diff.sqrMagnitude;
                if (dis < min2)
                {
                    min2 = dis;
                    id = i;
                }
            }
            Vector3 dir = cubeEightPoints[id] - point;
            Vector3[] axis = GetCubeClosePoint(cubeEightPoints, (CubeEightPoint)id);
            axis[0] = (cubeEightPoints[id] - axis[0]).normalized;
            axis[1] = (cubeEightPoints[id] - axis[1]).normalized;
            axis[2] = (cubeEightPoints[id] - axis[2]).normalized;
            Vector3 res = GetNewCoordinateVector(point - cubeEightPoints[id], axis[0], axis[1], axis[2]);
            if (res.x <= 0 && res.y <= 0 && res.z <= 0)
            {
                inner = true;
                if (res.x > res.y && res.x > res.z)
                {
                    res.y = 0; res.z = 0;
                }
                else if (res.y > res.z && res.y > res.z)
                {
                    res.x = 0; res.z = 0;
                }
                else
                {
                    res.x = 0; res.y = 0;
                }
            }
            else
            {
                if (res.x < 0) res.x = 0;
                if (res.y < 0) res.y = 0;
                if (res.z < 0) res.z = 0;
            }
            return new Vector3(res.x * axis[0].x + res.y * axis[1].x + res.z * axis[2].x,
                                   res.x * axis[0].y + res.y * axis[1].y + res.z * axis[2].y,
                                   res.x * axis[0].z + res.y * axis[1].z + res.z * axis[2].z);

        }


        public static Vector3 GetNewCoordinateVector(Vector3 old, Vector3 right, Vector3 up, Vector3 forward)
        {
            float x = Vector3.Dot(old, right.normalized);
            float y = Vector3.Dot(old, up.normalized);
            float z = Vector3.Dot(old, forward.normalized);
            return new Vector3(x, y, z);
        }
        public static Vector3[] GetCubeClosePoint(Vector3[] cubeEightPoints, CubeEightPoint cur)
        {
            int id = (int)cur;
            Vector3[] res = new Vector3[3];
            res[0] = (cubeEightPoints[id / 4 == 0 ? id + 4 : id - 4]);
            res[1] = (cubeEightPoints[id % 2 == 0 ? id + 1 : id - 1]);
            res[2] = (cubeEightPoints[id / 2 % 2 == 0 ? id + 2 : id - 2]);
            return res;
        }

        #endregion

        #region 2D
        public static bool IsPointInQuad(Vector2[] quadFourPoint, Vector2 point)
        {

            bool isPositive = false;
            bool isNegative = false;

            int len = quadFourPoint.Length;
            for (int i = 0; i < len; i++)
            {
                Vector2 p1 = quadFourPoint[i];
                Vector2 p2 = quadFourPoint[(i + 1) % len];

                float crossProduct = Cross(p1, p2, point);

                if (crossProduct > 0)
                    isPositive = true;
                else if (crossProduct < 0)
                    isNegative = true;

                if (isPositive && isNegative)
                    return false;
            }

            return true;
        }



        #endregion
        #region 2DUtil
        /// <summary>
        /// SAT核心计算：根据两个区间[aMin,aMax]和[bMin,bMax]在轴上的投影关系，计算碰触时间和避障方向
        /// aMin/aMax: 物体A在轴上的投影区间
        /// bMin/bMax: 物体B在轴上的投影区间
        /// dir: 移动方向在轴上的投影值
        /// touchTime: A触碰B的时间（0~1，相对于移动距离的比例）
        /// avoidTime: A脱出B的时间
        /// avoidDir: 避障方向（+1=沿轴正方向避让，-1=沿轴负方向避让）
        /// 
        /// 5种区间关系：
        /// [] {} : A在B左侧，A向右移动才会碰到B
        /// [{}]  : A完全包含B
        /// {[}]  : B完全包含A
        /// [{)}] : A的右半部分与B的左半部分重叠
        /// {[]}  : A的左半部分与B的右半部分重叠
        /// {} [] : A在B右侧，A向左移动才会碰到B
        /// </summary>
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
        public static float Cross(Vector2 from, Vector2 to, Vector2 o)
        {
            return (to.x - from.x) * (o.y - from.y) - (to.y - from.y) * (o.x - from.x);
        }
        public static float GetLineYByX(Vector2 p1, Vector2 p2, float x)
        {
            float k = (p2.y - p1.y) / (p2.x - p1.x);
            return k * x + (p1.y - k * p1.x);
        }
        #endregion






        #region UI
        public static Vector2 GetNormalizedRelativePos(Vector2 realPos, RectTransform area)
        {
            Vector3[] cor = new Vector3[4];
            area.GetWorldCorners(cor);
            float width = cor[3].x - cor[0].x;
            float height = cor[1].y - cor[0].y;
            return new Vector2((realPos.x - cor[0].x) / width, (realPos.y - cor[0].y) / height);
        }
        public static Vector2 GetRealPos(Vector2 relativeNormalizedPos, RectTransform area)
        {
            Vector3[] cor = new Vector3[4];
            area.GetWorldCorners(cor);
            float width = cor[3].x - cor[0].x;
            float height = cor[1].y - cor[0].y;
            return new Vector2(cor[0].x + width * relativeNormalizedPos.x, cor[0].y + height * relativeNormalizedPos.y);
        }
        public static Vector2 GetSize(this RectTransform area)
        {
            Vector3[] cor = new Vector3[4];
            area.GetWorldCorners(cor);
            return new Vector2(cor[3].x - cor[0].x, cor[1].y - cor[0].y);
        }
        public static Vector2 GetCenterWorldPos(this RectTransform area)
        {
            Vector3[] cor = new Vector3[4];
            area.GetWorldCorners(cor);
            return new Vector2((cor[3].x + cor[0].x) * 0.5f, (cor[1].y + cor[0].y) * 0.5f);
        }
        #endregion
        #region Misc

        public static FourDir GetFourDirByEuler(float eular)
        {
            eular = (eular % 360 + 360) % 360;
            if (eular <= 45 || eular >= 315)
            {
                return FourDir.Up;
            }
            else if (eular >= 45 && eular <= 135)
            {
                return FourDir.Right;
            }
            else if (eular >= 135 && eular <= 225)
            {
                return FourDir.Down;

            }
            else if (eular >= 225 && eular <= 315)
            {
                return FourDir.Left;
            }
            return FourDir.Up;
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


        public static void CalculateTriangle(float a, float b, out float hypotenuse, out float angleDegrees)
        {
            if (a <= 0 || b <= 0)
            {
                throw new ArgumentException("直角边长度必须为正数");
            }

            hypotenuse = Mathf.Sqrt(a * a + b * b);

            float angleRadians = Mathf.Atan(b / a);
            angleDegrees = Mathf.Round(RadiansToDegrees(angleRadians));
        }

        public static float RadiansToDegrees(float radians)
        {
            return radians * (180 / Mathf.PI);
        }

        private static bool IsOverlap(float minA, float maxA, float minB, float maxB)
        {
            return !(maxA < minB || maxB < minA);
        }
        #endregion



    }
}
