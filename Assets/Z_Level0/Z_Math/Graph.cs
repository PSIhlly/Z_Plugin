using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Z_Math
{
    public static class Graph
    {
        public static bool dDebug;
        public class IntersectAssisant
        {
            int isOut = 0;
            int isIn = 0;
            int isCross = 0;
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
            public IntersectType GetRes()
            {
                if (isCross == 1)
                {
                    return IntersectType.Cross;
                }
                else if (isIn == 1)
                {
                    return IntersectType.In;
                }
                else if (isOut == 1)
                {
                    return IntersectType.Out;
                }
                return IntersectType.None;
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
            foreach (var o in lst)
            {
                foreach (var p in o)
                {
                    if (!exist.Contains(p))
                    {
                        exist.Add(p);
                    }
                }
            }
            return exist.ToList();
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
            Vector3[] res = new Vector3[a.Length];
            for (int i = 0; i < a.Length; i++)
            {
                res[i] = ElementwisePlus(a[i], b);
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
            //ref CubeEightPoint
            Vector3[] choose = new[] { new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0.5f, -0.5f, -0.5f), new Vector3(-0.5f, 0.5f, -0.5f), new Vector3(0.5f, 0.5f, -0.5f),
                                        new Vector3(-0.5f, -0.5f, 0.5f), new Vector3(0.5f, -0.5f, 0.5f), new Vector3(-0.5f, 0.5f, 0.5f), new Vector3(0.5f, 0.5f, 0.5f)};
            for (int i = 0; i < 8; i++)
            {
                ans[i] = center + rotation * ElementwiseMultiply(choose[i], size);
            }
            return ans;
        }
        public static Vector3[] GetSphereSixPoint(Vector3 center, float radius, Vector3 euler, Vector3 scale)
        {

            Vector3[] ans = new Vector3[6];
            Quaternion rotation = Quaternion.Euler(euler);
            //ref CubeEightPoint
            Vector3[] choose = new[] { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };
            for (int i = 0; i < choose.Length; i++)
            {
                ans[i] = center + rotation * ElementwiseMultiply(choose[i] * radius, scale);
            }
            return ans;
        }


        public static List<Vector3Int> GetRoughOverlapIntPos(Vector3[] points)
        {
            int minX = int.MaxValue;
            int minY = int.MaxValue;
            int minZ = int.MaxValue;
            int maxX = int.MinValue;
            int maxY = int.MinValue;
            int maxZ = int.MinValue;
            for (int i = 0; i < points.Length; i++)
            {
                minX = Mathf.Min((int)points[i].x, minX);
                minY = Mathf.Min((int)points[i].y, minY);
                minZ = Mathf.Min((int)points[i].z, minZ);
                maxX = Mathf.Max((int)(points[i].x + 1), maxX);
                maxY = Mathf.Max((int)(points[i].y + 1), maxY);
                maxZ = Mathf.Max((int)(points[i].z + 1), maxZ);
            }
            List<Vector3Int> res = new List<Vector3Int>((maxX - minX + 1) * (maxY - minY + 1) * (maxZ - minZ + 1));
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
            try
            {

                return point.x * point.x / a2 + point.y * point.y / b2 + point.z * point.z / c2 <= 1e-9;
            }
            catch (Exception e)
            {
                return false;
            }
        }



        // 计算椭球在指定轴上的有效半径
        private static float ProjectSphereRadius(Vector3[] sphereSixPoints, Vector3 axis)
        {
            var center = (sphereSixPoints[(int)SphereSixPoint.Right] + sphereSixPoints[(int)SphereSixPoint.Left]) / 2;
            Vector3 a = sphereSixPoints[(int)SphereSixPoint.Right] - center; // 第一个半轴向量
            Vector3 b = sphereSixPoints[(int)SphereSixPoint.Up] - center; // 第二个半轴向量
            Vector3 c = sphereSixPoints[(int)SphereSixPoint.Forward] - center; // 第三个半轴向量

            float dotA = Vector3.Dot(a, axis);
            float dotB = Vector3.Dot(b, axis);
            float dotC = Vector3.Dot(c, axis);

            return Mathf.Sqrt(dotA * dotA + dotB * dotB + dotC * dotC);
        }

        #region Intersect
        public static IntersectType CubeIntersectCube(Vector3[] aCubeEightPoints, Vector3[] bCubeEightPoints, Vector3 dir, out float dis)
        {
            bool fromIn = false;
            bool toIn = false;
            var mag = dir.magnitude;

            HashSet<Vector3> exist = new HashSet<Vector3>();
            dis = mag;


            List<Vector3> axesToCheck = new List<Vector3>();
            axesToCheck.AddRange(GetFaceNormals(aCubeEightPoints));
            axesToCheck.AddRange(GetFaceNormals(bCubeEightPoints));

            float minTime = 1;

            if (IsCubesOverlap(aCubeEightPoints, bCubeEightPoints))
            {
                fromIn = true;
                if (mag <= 0)
                {
                    return IntersectType.Inner;
                }
            }

            foreach (Vector3 axis in axesToCheck)
            {
                if (exist.Contains(axis))
                {
                    continue;
                }
                if (axis.sqrMagnitude <= 0) continue;

                (float minA, float maxA) = ProjectCubeOntoAxis(aCubeEightPoints, axis);
                (float minB_proj, float maxB_proj) = ProjectCubeOntoAxis(bCubeEightPoints, axis);

                float velocityProjection = Vector3.Dot(dir, axis);

                float tCandidate = 1;

                if (Mathf.Abs(velocityProjection) <= 0)
                {
                    continue;
                }
                else if (velocityProjection > 0)
                {
                    if (maxA < minB_proj)
                    {
                        tCandidate = (minB_proj - maxA) / velocityProjection;
                    }
                }
                else
                {
                    if (minA > maxB_proj)
                    {
                        tCandidate = (maxB_proj - minA) / velocityProjection;
                    }
                }

                if (tCandidate >= 0 && tCandidate <= 1.0f)
                {
                    float newMinA = minA + velocityProjection * tCandidate;
                    float newMaxA = maxA + velocityProjection * tCandidate;

                    if (IsOverlap(newMinA, newMaxA, minB_proj, maxB_proj))
                    {
                        if (tCandidate < minTime)
                        {
                            minTime = tCandidate;
                        }
                    }
                }
            }
            dis = mag * minTime;

            if (IsCubesOverlap(ElementwisePlus(aCubeEightPoints, dir), bCubeEightPoints))
            {
                toIn = true;
            }

            return GetIntersectRes(fromIn, toIn, minTime < 1);
        }
        public static IntersectType SphereIntersectCube(Vector3[] sphereSixPoints, Vector3[] cubeEightPoints, Vector3 dir, out float dis)
        {
            bool fromIn = false;
            bool toIn = false;
            var mag = dir.magnitude;
            dis = mag;
            HashSet<Vector3> exist = new HashSet<Vector3>();
            // 先检查初始是否重叠
            if (IsSphereAndCubeOverlap(sphereSixPoints, cubeEightPoints))
            {
                fromIn = true;
                if (mag == 0)
                {
                    return IntersectType.Inner;
                }
            }

            var aCenter = (sphereSixPoints[(int)SphereSixPoint.Right] + sphereSixPoints[(int)SphereSixPoint.Left]) / 2;
            var bCenter = (cubeEightPoints[(int)CubeEightPoint.LeftDownBack] + cubeEightPoints[(int)CubeEightPoint.RightUpForward]) / 2;

            float minTime = 0;
            if (fromIn)
            {
                if (Vector3.Dot((aCenter - bCenter), dir) >= 0)
                    minTime = 1;
            }
            else
            {
                // 生成所有潜在分离轴（与静态重叠判断一致）

                List<Vector3> axesToCheck = new List<Vector3>();
                axesToCheck.AddRange(GetFaceNormals(cubeEightPoints));
                // 2. 添加椭球的3个半轴方向
                axesToCheck.Add(sphereSixPoints[(int)SphereSixPoint.Right].normalized);
                axesToCheck.Add(sphereSixPoints[(int)SphereSixPoint.Up].normalized);
                axesToCheck.Add(sphereSixPoints[(int)SphereSixPoint.Forward].normalized);

                // 3. 添加椭球半轴与立方体面法线的叉乘方向（补充潜在分离轴）
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 3; j < 6; j++)
                    {
                        var cross = Vector3.Cross(axesToCheck[i], axesToCheck[j]).normalized;
                        if (cross.sqrMagnitude > 0) // 避免零向量
                            axesToCheck.Add(cross);
                    }
                }
                // 对每个轴计算碰撞时间
                foreach (var axis in axesToCheck)
                {
                    if (exist.Contains(axis))
                    {
                        continue;
                    }
                    exist.Add(axis);
                    // 1. 初始投影区间
                    float ellipsoidCenterProj0 = Vector3.Dot(aCenter, axis);
                    float ellipsoidRadius = ProjectSphereRadius(sphereSixPoints, axis);
                    float ellipsoidMin0 = ellipsoidCenterProj0 - ellipsoidRadius;
                    float ellipsoidMax0 = ellipsoidCenterProj0 + ellipsoidRadius;

                    (float cubeMin, float cubeMax) = ProjectCubeOntoAxis(cubeEightPoints, axis);

                    // 2. 移动速度在轴上的投影
                    float velocityProj = Vector3.Dot(dir, axis);

                    if (Mathf.Abs(velocityProj) <= 0)
                    {
                        // 速度与轴垂直，无相对移动，跳过
                        if (ellipsoidMax0 <= cubeMin || ellipsoidMin0 >= cubeMax)
                        {
                            minTime = 1;
                            break;
                        }
                    }
                   
                    // 椭球向轴负方向移动，检查是否追上立方体右边界
                    if (ellipsoidMin0 >= cubeMax)
                    {
                        if (velocityProj < 0)
                        {
                            minTime = Mathf.Max(minTime, (cubeMax- ellipsoidMin0) / velocityProj);
                        }
                        else
                        {
                            minTime = 1;
                            break;
                        }

                    }
                    else if (ellipsoidMax0 <= cubeMin)
                    {
                        if (velocityProj > 0)
                        {
                            minTime = Mathf.Max(minTime,(cubeMin - ellipsoidMax0) / velocityProj);
                        }
                        else
                        {
                            minTime = 1;
                            break;
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
            }
            if (IsSphereAndCubeOverlap(ElementwisePlus(sphereSixPoints, dir), cubeEightPoints))
            {
                toIn = true;
            }

            dis = minTime * mag;

            return GetIntersectRes(fromIn, toIn, minTime < 1);
        }

        public static IntersectType SphereIntersectSphere(Vector3[] aSphereSixPoints, Vector3[] bSphereSixPoints, Vector3 dir, out float dis)
        {
            var mag = dir.magnitude;
            dis = mag;
            float minTime = 1;
            bool fromIn = false;
            bool toIn = false;

            HashSet<Vector3> exist = new HashSet<Vector3>();
            var aCenter = (aSphereSixPoints[(int)SphereSixPoint.Left] + aSphereSixPoints[(int)SphereSixPoint.Right]) / 2;
            var bCenter = (bSphereSixPoints[(int)SphereSixPoint.Left] + bSphereSixPoints[(int)SphereSixPoint.Right]) / 2;

            // 先检查初始是否已重叠
            if (IsSpheresOverlap(aSphereSixPoints, bSphereSixPoints))
            {
                fromIn = true;
            }
            if (IsSpheresOverlap(ElementwisePlus(aSphereSixPoints, dir), bSphereSixPoints))
            {
                toIn = true;
            }

            // 生成15个分离轴
            List<Vector3> axesToCheck = GenerateSpheresSeparationAxes(aSphereSixPoints, bSphereSixPoints);

            foreach (var axis in axesToCheck)
            {
                if (exist.Contains(axis))
                {
                    continue;
                }
                if (axis.sqrMagnitude <= 0) continue;

                // 1. 初始投影区间（t=0时）
                float projM0 = Vector3.Dot(aCenter, axis);
                float radiusM = ProjectSphereRadius(aSphereSixPoints, axis);
                float minM0 = projM0 - radiusM;
                float maxM0 = projM0 + radiusM;

                float projS = Vector3.Dot(bCenter, axis);
                float radiusS = ProjectSphereRadius(bSphereSixPoints, axis);
                float minS = projS - radiusS;
                float maxS = projS + radiusS;

                // 2. 移动速度在轴上的投影
                float velocityProj = Vector3.Dot(dir, axis);

                // 3. 计算碰撞时间候选t
                float tCandidate = float.PositiveInfinity;
                if (Mathf.Abs(velocityProj) <= 0)
                {
                    // 速度与轴垂直，无相对移动，跳过
                    continue;
                }
                else if (velocityProj > 0)
                {
                    // 移动椭球向轴正方向移动，检查是否追上静止椭球的左边界
                    if (maxM0 < minS)
                        tCandidate = (minS - maxM0) / velocityProj;
                }
                else
                {
                    // 移动椭球向轴负方向移动，检查是否追上静止椭球的右边界
                    if (minM0 > maxS)
                        tCandidate = (maxS - minM0) / velocityProj;
                }

                // 4. 验证候选时间的有效性（0≤t≤1，且投影重叠）
                if (tCandidate >= 0 && tCandidate <= 1.0f)
                {
                    // 计算t时刻移动椭球的投影区间
                    float projMT = projM0 + velocityProj * tCandidate;
                    float minMT = projMT - radiusM;
                    float maxMT = projMT + radiusM;

                    // 检查投影是否重叠
                    if (maxMT >= minS && maxS >= minMT)
                        minTime = Mathf.Min(minTime, tCandidate);
                }
            }
            dis = minTime * mag;

            return GetIntersectRes(fromIn, toIn, minTime < 1);
        }
        /// <summary>
        /// 生成两个椭球的15个分离轴
        /// </summary>
        private static List<Vector3> GenerateSpheresSeparationAxes(Vector3[] aSphereSixPoints, Vector3[] bSphereSixPoints)
        {
            List<Vector3> axes = new List<Vector3>();
            var aCenter = (aSphereSixPoints[(int)SphereSixPoint.Left] + aSphereSixPoints[(int)SphereSixPoint.Right]) / 2;

            // 1. 添加E1的3个半轴方向

            axes.Add((aSphereSixPoints[(int)SphereSixPoint.Right] - aCenter).normalized);
            axes.Add((aSphereSixPoints[(int)SphereSixPoint.Up] - aCenter).normalized);
            axes.Add((aSphereSixPoints[(int)SphereSixPoint.Forward] - aCenter).normalized);

            var bCenter = (bSphereSixPoints[(int)SphereSixPoint.Left] + bSphereSixPoints[(int)SphereSixPoint.Right]) / 2;

            // 2. 添加E2的3个半轴方向
            axes.Add((bSphereSixPoints[(int)SphereSixPoint.Right] - bCenter).normalized);
            axes.Add((bSphereSixPoints[(int)SphereSixPoint.Up] - bCenter).normalized);
            axes.Add((bSphereSixPoints[(int)SphereSixPoint.Forward] - bCenter).normalized);

            // 3. 添加E1半轴与E2半轴的两两叉乘方向（3×3=9个）
            for (int i = 0; i < 3; i++)
            {
                for (int j = 3; j < 6; j++)
                {
                    Vector3 cross = Vector3.Cross(axes[i], axes[j]).normalized;
                    if (cross.sqrMagnitude > 0) // 避免零向量
                        axes.Add(cross);
                }
            }
            return axes;
        }
        private static List<Vector3> GetFaceNormals(Vector3[] cubeEightPoints)
        {
            List<Vector3> normals = new List<Vector3>();
            Vector3 p0 = cubeEightPoints[(int)CubeEightPoint.LeftDownBack];
            Vector3 p1 = cubeEightPoints[(int)CubeEightPoint.LeftUpBack];
            Vector3 p2 = cubeEightPoints[(int)CubeEightPoint.LeftDownForward];
            Vector3 p3 = cubeEightPoints[(int)CubeEightPoint.RightDownBack];

            Vector3 normal1 = Vector3.Cross(p1 - p0, p2 - p0).normalized;
            Vector3 normal2 = Vector3.Cross(p2 - p0, p3 - p0).normalized;
            Vector3 normal3 = Vector3.Cross(p3 - p0, p1 - p0).normalized;

            normals.Add(normal1);
            normals.Add(normal2);
            normals.Add(normal3);

            return normals;
        }

        private static (float min, float max) ProjectCubeOntoAxis(Vector3[] cubeEightPoints, Vector3 axis)
        {
            float min = float.MaxValue;
            float max = float.MinValue;
            foreach (var v in cubeEightPoints)
            {
                float proj = Vector3.Dot(v, axis);
                if (proj < min) min = proj;
                if (proj > max) max = proj;
            }
            return (min, max);
        }



        public static IntersectType LineIntersectCube(Vector3[] eightPoints, Vector3 from, Vector3 to, out float dis)
        {
            var mag = (to - from).magnitude;
            dis = mag;
            var forward = eightPoints[(int)CubeEightPoint.RightDownForward] - eightPoints[(int)CubeEightPoint.RightDownBack];
            var up = eightPoints[(int)CubeEightPoint.RightUpBack] - eightPoints[(int)CubeEightPoint.RightDownBack];
            var right = eightPoints[(int)CubeEightPoint.RightDownBack] - eightPoints[(int)CubeEightPoint.LeftDownBack];
            to = GetNewCoordinateVector(to, forward, up, right) - eightPoints[(int)CubeEightPoint.LeftDownBack];
            from = GetNewCoordinateVector(from, forward, up, right) - eightPoints[(int)CubeEightPoint.LeftDownBack];

            var x = right.magnitude;
            var y = up.magnitude;
            var z = forward.magnitude;
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

            var fromIn = false;
            var toIn = false;
            if (from.x < x && from.x > 0 && from.y > 0 && from.y < y && from.z > 0 && from.z < z)
            {
                fromIn = true;
            }

            if (to.x < x && to.x > 0 && to.y > 0 && to.y < y && to.z > 0 && to.z < z)
            {
                toIn = true;
            }

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
        public static IntersectType PointIntersectCube(Vector3[] eightPoints, Vector3 point)
        {
            var forward = eightPoints[(int)CubeEightPoint.RightDownForward] - eightPoints[(int)CubeEightPoint.RightDownBack];
            var up = eightPoints[(int)CubeEightPoint.RightUpBack] - eightPoints[(int)CubeEightPoint.RightDownBack];
            var right = eightPoints[(int)CubeEightPoint.RightDownBack] - eightPoints[(int)CubeEightPoint.LeftDownBack];
            point = GetNewCoordinateVector(point, forward, up, right) - eightPoints[(int)CubeEightPoint.LeftDownBack];

            var x = right.magnitude;
            var y = up.magnitude;
            var z = forward.magnitude;

            if (point.x >= 0 && point.x <= x && point.y >= 0 && point.y <= y && point.z >= 0 && point.z <= z)
                return IntersectType.In;
            return IntersectType.Out;

        }
        public static IntersectType LineIntersectSphere(Vector3[] sixPoint, Vector3 from, Vector3 to, out float dis)
        {
            Vector3 dir = to - from;
            var mag = dir.magnitude;
            dis = mag;
            var center = (sixPoint[(int)SphereSixPoint.Forward] + sixPoint[(int)SphereSixPoint.Back]) / 2;
            var forward = sixPoint[(int)SphereSixPoint.Forward] - center;
            var up = sixPoint[(int)SphereSixPoint.Up] - center;
            var right = sixPoint[(int)SphereSixPoint.Right] - center;
            to = GetNewCoordinateVector(to, forward, up, right) - center;
            from = GetNewCoordinateVector(from, forward, up, right) - center;

            var x = right.magnitude;
            var y = up.magnitude;
            var z = forward.magnitude;
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
                dis = Math.Min(t1 * dir.magnitude, dis);
                t1Valid = true;
            }

            bool fromIn = GetPointInSphere(from, a2, b2, c2);
            if (t2 >= 0 && t2 <= 1)
            {
                dis = Math.Min(t2 * dir.magnitude, dis);
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
            try
            {
                var dir = to - from;
                var basic = (x - from.x) / dir.x;
                res = new Vector3(x, basic * dir.y + from.y, basic * dir.z + from.z);
                return IsClamp(res, from, to);
            }
            catch (Exception e)
            {
                return false;
            }
        }
        public static bool GetPointOnLineByY(Vector3 from, Vector3 to, float y, out Vector3 res)
        {
            res = Vector3.zero;
            try
            {
                var dir = to - from;
                var basic = (y - from.y) / dir.y;
                res = new Vector3(basic * dir.x + from.x, y, basic * dir.z + from.z);
                return IsClamp(res, from, to);
            }
            catch (Exception e)
            {
                return false;
            }
        }
        public static bool GetPointOnLineByZ(Vector3 from, Vector3 to, float z, out Vector3 res)
        {
            res = Vector3.zero;
            try
            {
                var dir = to - from;
                var basic = (z - from.z) / dir.z;
                res = new Vector3(basic * dir.x + from.x, basic * dir.y + from.y, z);
                return IsClamp(res, from, to);
            }
            catch (Exception e)
            {
                return false;
            }
        }
        public static Vector3[] RotatePointAroundOrigin(Vector3[] points, Vector3 euler)
        {
            Vector3[] newPos = new Vector3[points.Length];
            Quaternion rotation = Quaternion.Euler(euler);
            for (int i = 0, icnt = points.Length; i < icnt; i++)
            {
                // 将欧拉角转换为四元数
                // 使用四元数旋转点
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

        /// <summary>
        /// 获取向量法平面上随机向量
        /// </summary>
        /// <param name="v"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static Vector3 GetRandomVectorOnPlane(Vector3 v, float length)
        {
            // 计算两个与v垂直的基向量
            Vector3 right = Vector3.Cross(v, Vector3.up);  // 与v垂直的向量
            if (right.magnitude < 0.001f)  // 如果v与Vector3.up平行，尝试与其他向量交叉
            {
                right = Vector3.Cross(v, Vector3.forward);
            }

            Vector3 forward = Vector3.Cross(v, right);  // 获取另一个与v垂直的向量

            // 随机生成平面内的两个方向
            float randomX = UnityEngine.Random.Range(-1f, 1f);
            float randomY = UnityEngine.Random.Range(-1f, 1f);

            // 通过线性组合获得随机的向量
            Vector3 randomVector = (right * randomX + forward * randomY).normalized * length;

            return randomVector;
        }

        private static bool IsCubesOverlap(Vector3[] cubeA, Vector3[] cubeB)
        {
            List<Vector3> axes = new List<Vector3>();
            axes.AddRange(GetFaceNormals(cubeA));
            axes.AddRange(GetFaceNormals(cubeB));

            foreach (var axis in axes)
            {
                if (axis.sqrMagnitude < 0) continue;

                (float minA, float maxA) = ProjectCubeOntoAxis(cubeA, axis);
                (float minB, float maxB) = ProjectCubeOntoAxis(cubeB, axis);

                if (!IsOverlap(minA, maxA, minB, maxB))
                {
                    return false;
                }
            }
            return true;
        }
        public static bool IsSphereAndCubeOverlap(Vector3[] sphereSixPoints, Vector3[] cubeEightPoints)
        {
            List<Vector3> axesToCheck = new List<Vector3>();
            // 1. 添加立方体的3个面法线
            axesToCheck.AddRange(GetFaceNormals(cubeEightPoints));

            // 2. 添加椭球的3个半轴方向
            axesToCheck.Add(sphereSixPoints[(int)SphereSixPoint.Right].normalized);
            axesToCheck.Add(sphereSixPoints[(int)SphereSixPoint.Up].normalized);
            axesToCheck.Add(sphereSixPoints[(int)SphereSixPoint.Forward].normalized);

            // 3. 添加椭球半轴与立方体面法线的叉乘方向（补充潜在分离轴）
            for (int i = 0; i < 3; i++)
            {
                for (int j = 3; j < 6; j++)
                {
                    var cross = Vector3.Cross(axesToCheck[i], axesToCheck[j]).normalized;
                    if (cross.sqrMagnitude > 0) // 避免零向量
                        axesToCheck.Add(cross);
                }
            }
            var center = (sphereSixPoints[(int)SphereSixPoint.Right] + sphereSixPoints[(int)SphereSixPoint.Left]) / 2;
            // 对每个轴检查投影重叠
            foreach (var axis in axesToCheck)
            {
                // 计算椭球在轴上的投影区间 [ellipsoidMin, ellipsoidMax]
                float ellipsoidCenterProj = Vector3.Dot(center, axis);
                float ellipsoidRadius = ProjectSphereRadius(sphereSixPoints, axis);
                float ellipsoidMin = ellipsoidCenterProj - ellipsoidRadius;
                float ellipsoidMax = ellipsoidCenterProj + ellipsoidRadius;

                // 计算立方体在轴上的投影区间 [cubeMin, cubeMax]
                (float cubeMin, float cubeMax) = ProjectCubeOntoAxis(cubeEightPoints, axis);

                // 检查是否分离
                if (ellipsoidMax <= cubeMin || cubeMax <= ellipsoidMin)
                    return false;
            }

            return true;
        }
        public static bool IsSpheresOverlap(Vector3[] aSphereSixPoints, Vector3[] bSphereSixPoints)
        {
            List<Vector3> axesToCheck = GenerateSpheresSeparationAxes(aSphereSixPoints, bSphereSixPoints);
            var aCenter = (aSphereSixPoints[(int)SphereSixPoint.Left] + aSphereSixPoints[(int)SphereSixPoint.Right]) / 2;
            var bCenter = (bSphereSixPoints[(int)SphereSixPoint.Left] + bSphereSixPoints[(int)SphereSixPoint.Right]) / 2;
            foreach (var axis in axesToCheck)
            {
                if (axis.sqrMagnitude < 0) continue; // 跳过零向量

                // 计算E1在轴上的投影区间 [min1, max1]
                float projCenter1 = Vector3.Dot(aCenter, axis);
                float radius1 = ProjectSphereRadius(aSphereSixPoints, axis);
                float min1 = projCenter1 - radius1;
                float max1 = projCenter1 + radius1;

                // 计算E2在轴上的投影区间 [min2, max2]
                float projCenter2 = Vector3.Dot(bCenter, axis);
                float radius2 = ProjectSphereRadius(bSphereSixPoints, axis);
                float min2 = projCenter2 - radius2;
                float max2 = projCenter2 + radius2;

                // 检查投影是否分离（分离则两椭球不重叠）
                if (max1 < min2 || max2 < min1)
                    return false;
            }

            return true; // 所有轴都重叠，判定碰撞
        }



        public static Vector3 GetNewCoordinateVector(Vector3 old, Vector3 forward, Vector3 up, Vector3 right)
        {
            float x = Vector3.Dot(old, right.normalized);
            float y = Vector3.Dot(old, up.normalized);
            float z = Vector3.Dot(old, forward.normalized);
            return new Vector3(x, y, z);
        }

        #endregion

        #region 2D
        public static bool IsPointInQuad(Vector2[] quadFourPoint, Vector2 point)
        {

            bool isPositive = false;
            bool isNegative = false;

            for (int i = 0; i < quadFourPoint.Length; i++)
            {
                Vector2 p1 = quadFourPoint[i];
                Vector2 p2 = quadFourPoint[(i + 1) % quadFourPoint.Length];

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
        /// <summary>
        /// 左下后角 0 0 0
        /// </summary>
        /// <param name="relativePos"></param>
        /// <param name="area"></param>
        public static Vector2 GetNormalizedRelativePos(Vector2 realPos, RectTransform area)
        {
            Vector3[] cor = new Vector3[4];
            area.GetWorldCorners(cor);
            return new Vector2((realPos.x - cor[0].x) / (cor[3].x - cor[0].x), (realPos.y - cor[0].y) / (cor[1].y - cor[0].y));
        }
        /// <summary>
        /// 左下后角 0 0 0
        /// </summary>
        /// <param name="relativeNormalizedPos"></param>
        /// <param name="area"></param>
        public static Vector2 GetRealPos(Vector2 relativeNormalizedPos, RectTransform area)
        {
            Vector3[] cor = new Vector3[4];
            area.GetWorldCorners(cor);
            return new Vector2(cor[0].x + (cor[3].x - cor[0].x) * relativeNormalizedPos.x, cor[0].y + (cor[1].y - cor[0].y) * relativeNormalizedPos.y);
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
            return new Vector2((cor[3].x + cor[0].x) / 2, (cor[1].y + cor[0].y) / 2);
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


        /// <summary>
        /// 计算直角三角形的斜边长度和指定夹角
        /// </summary>
        /// <param name="a">第一条直角边长度</param>
        /// <param name="b">第二条直角边长度</param>
        /// <param name="hypotenuse">输出：斜边长度</param>
        /// <param name="angleDegrees">输出：斜边与第一条直角边的夹角（度）</param>
        public static void CalculateTriangle(float a, float b, out float hypotenuse, out float angleDegrees)
        {
            // 验证输入（边长必须为正数）
            if (a <= 0 || b <= 0)
            {
                throw new ArgumentException("直角边长度必须为正数");
            }

            // 1. 计算斜边长度（勾股定理：c = √(a² + b²)）
            hypotenuse = Mathf.Sqrt(a * a + b * b);

            // 2. 计算夹角（与第一条直角边a的夹角θ）
            // 原理：tanθ = 对边/邻边 = b/a → θ = arctan(b/a)
            float angleRadians = Mathf.Atan(b / a); // 结果为弧度
            angleDegrees = Mathf.Round(RadiansToDegrees(angleRadians)); // 转换为度并保留2位小数
        }

        /// <summary>
        /// 弧度转角度
        /// </summary>
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
