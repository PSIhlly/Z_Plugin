using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations;
using UnityEngine.UIElements;
namespace Z_Math
{
    public static class Graph
    {
        public static float DELTA = 0.05f;
        public static bool dDebug;
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





        #region Intersect
        public static IntersectType CubeIntersectCube(Vector3[] aCubeEightPoints, Vector3[] bCubeEightPoints, Vector3 dir, out float dis, out Vector3 avoidDir)
        {
            avoidDir = Vector3.zero;
            bool fromIn = false;
            bool toIn = false;
            var mag = dir.magnitude;
            dis = mag;
            HashSet<Vector3> exist = new HashSet<Vector3>();
            List<Vector3> axesToCheck = new List<Vector3>();
            var aCenter = (aCubeEightPoints[(int)CubeEightPoint.LeftDownBack] + aCubeEightPoints[(int)CubeEightPoint.RightUpForward]) / 2;
            var bCenter = (bCubeEightPoints[(int)CubeEightPoint.LeftDownBack] + bCubeEightPoints[(int)CubeEightPoint.RightUpForward]) / 2;

            if (IsCubesOverlap(aCubeEightPoints, bCubeEightPoints))
            {
                fromIn = true;
                if (mag <= 0)
                {
                    return IntersectType.Inner;
                }
            }

            float touchTime = 0;
            float avoidTime = 1;

            {
                axesToCheck.AddRange(GetFaceNormals(aCubeEightPoints));
                axesToCheck.AddRange(GetFaceNormals(bCubeEightPoints));

                foreach (Vector3 axis in axesToCheck)
                {
                    if (exist.Contains(axis))
                    {
                        continue;
                    }
                    exist.Add(axis);

                    if (axis.sqrMagnitude <= 0)
                    {
                        continue;
                    }

                    (float aMin, float aMax) = ProjectCubeOntoAxis(aCubeEightPoints, axis);
                    (float bMin, float bMax) = ProjectCubeOntoAxis(bCubeEightPoints, axis);

                    float dirProj = Vector3.Dot(dir, axis);

                    if (CalcTouchTimeAndAvoidTime(aMin, aMax, bMin, bMax, dirProj, ref touchTime, ref avoidTime, out var avoid))
                        break;
                    avoidDir += (avoid * axis).normalized;
                }
            }
            if (fromIn)
            {
                avoidTime = 1;
                touchTime = 0;
                if (Vector3.Dot((aCenter - bCenter), dir) >= 0)//todo
                    touchTime = 1;
            }
            if (touchTime > avoidTime)
            {
                touchTime = 1;
            }
            if (IsCubesOverlap(ElementwisePlus(aCubeEightPoints, dir), bCubeEightPoints))
            {
                toIn = true;
            }

            dis = mag * touchTime;
            avoidDir = avoidDir.normalized;
            return GetIntersectRes(fromIn, toIn, touchTime < 1);
        }
        public static IntersectType SphereIntersectCube(Vector3[] sphereSixPoints, Vector3[] cubeEightPoints, Vector3 dir, out float dis, out Vector3 avoidDir)
        {
            avoidDir = Vector3.zero;
            bool fromIn = false;
            bool toIn = false;
            var mag = dir.magnitude;
            dis = mag;

            var sphereCenter = (sphereSixPoints[(int)SphereSixPoint.Right] + sphereSixPoints[(int)SphereSixPoint.Left]) / 2;
            var disDir = GetPointToCube(sphereCenter, cubeEightPoints, out fromIn);

            if (fromIn)
            {
                if (mag == 0)
                {
                    return IntersectType.Inner;
                }
            }

            var newDisDir = GetPointToCube(sphereCenter + dir, cubeEightPoints, out var newInner);



            float sphereRadius = (sphereSixPoints[(int)SphereSixPoint.Right] - sphereSixPoints[(int)SphereSixPoint.Left]).magnitude / 2;
            var cubeCenter = (cubeEightPoints[(int)CubeEightPoint.LeftDownBack] + cubeEightPoints[(int)CubeEightPoint.RightUpForward]) / 2;

            float touchTime = 0;
            float avoidTime = 1;

            var axis = disDir.normalized;

            // 1. 初始投影区间
            float sphereCenterProj = Vector3.Dot(sphereCenter, axis);
            float sphereMin = sphereCenterProj - sphereRadius;
            float sphereMax = sphereCenterProj + sphereRadius;

            (float cubeMin, float cubeMax) = ProjectCubeOntoAxis(cubeEightPoints, axis);

            float dirProj = Vector3.Dot(dir, axis);

            var res = CalcTouchTimeAndAvoidTime(sphereMin, sphereMax, cubeMin, cubeMax, dirProj, ref touchTime, ref avoidTime, out var avoid);

            avoidDir += (avoid * axis).normalized;
            if (dir.y == 0)
            {
                int temp = 0;
                //Debug.Log(Time.frameCount);
            }
            //Debug.Log(Time.frameCount + " : " + axis + " " + sphereMin + " " + sphereMax + " " + cubeMin + " " + cubeMax + "  " + dir + "  " + dirProj + " " + " --- " + touchTime + " " + avoidTime);

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
            if (IsSphereAndCubeOverlap(ElementwisePlus(sphereSixPoints, dir), cubeEightPoints))
            {
                toIn = true;
            }


            dis = touchTime * mag;

            avoidDir = avoidDir.normalized;
            return GetIntersectRes(fromIn, toIn, touchTime < 1);
        }

        public static IntersectType SphereIntersectSphere(Vector3[] aSphereSixPoints, Vector3[] bSphereSixPoints, Vector3 dir, out float dis, out Vector3 avoidDir)
        {
            avoidDir = Vector3.zero;
            bool fromIn = false;
            bool toIn = false;
            var mag = dir.magnitude;
            dis = mag;
            // 先检查初始是否已重叠
            if (IsSpheresOverlap(aSphereSixPoints, bSphereSixPoints))
            {
                fromIn = true;
                if (mag == 0)
                {
                    return IntersectType.Inner;
                }
            }
            var aCenter = (aSphereSixPoints[(int)SphereSixPoint.Left] + aSphereSixPoints[(int)SphereSixPoint.Right]) / 2;
            var bCenter = (bSphereSixPoints[(int)SphereSixPoint.Left] + bSphereSixPoints[(int)SphereSixPoint.Right]) / 2;
            var aRadius = (aSphereSixPoints[(int)SphereSixPoint.Left] - aSphereSixPoints[(int)SphereSixPoint.Right]).magnitude / 2;
            var bRadius = (bSphereSixPoints[(int)SphereSixPoint.Left] - bSphereSixPoints[(int)SphereSixPoint.Right]).magnitude / 2;
            float touchTime = 0;
            float avoidTime = 1;


            {

                var axis = (aCenter - bCenter).normalized;

                if (axis.sqrMagnitude > 0)
                {
                    // 1. 初始投影区间（t=0时）
                    float aCenterProj = Vector3.Dot(aCenter, axis);
                    float aMin = aCenterProj - aRadius;
                    float aMax = aCenterProj + aRadius;

                    float bCenterProj = Vector3.Dot(bCenter, axis);
                    float bMin = bCenterProj - bRadius;
                    float bMax = bCenterProj + bRadius;

                    // 2. 移动速度在轴上的投影
                    float dirProj = Vector3.Dot(dir, axis);
                    var res = CalcTouchTimeAndAvoidTime(aMin, aMax, bMin, bMax, dirProj, ref touchTime, ref avoidTime, out var avoid);

                    /* if(!res)
                         Debug.Log(res + " " + aMin + " " + aMax + " " + bMin + " " + bMax + "  " + axis + "  " + dirProj + " " + dir + " --- " + touchTime + " " + avoidTime);
 */
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
            to = GetNewCoordinateVector(to, right, up, forward) - eightPoints[(int)CubeEightPoint.LeftDownBack];
            from = GetNewCoordinateVector(from, right, up, forward) - eightPoints[(int)CubeEightPoint.LeftDownBack];

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
        public static IntersectType PointIntersectCube(Vector3[] eightPoints, Vector3 point, out float dis)
        {
            dis = 0;
            var forward = eightPoints[(int)CubeEightPoint.RightDownForward] - eightPoints[(int)CubeEightPoint.RightDownBack];
            var up = eightPoints[(int)CubeEightPoint.RightUpBack] - eightPoints[(int)CubeEightPoint.RightDownBack];
            var right = eightPoints[(int)CubeEightPoint.RightDownBack] - eightPoints[(int)CubeEightPoint.LeftDownBack];
            point = GetNewCoordinateVector(point, right, up, forward) - eightPoints[(int)CubeEightPoint.LeftDownBack];

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
            to = GetNewCoordinateVector(to, right, up, forward) - center;
            from = GetNewCoordinateVector(from, right, up, forward) - center;

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
        public static Vector3 GetDistanceVector3D(Vector3 linePoint, Vector3 lineDir, Vector3 targetPoint)
        {
            Vector3 w = targetPoint - linePoint;
            float dotVV = Vector3.Dot(lineDir, lineDir);
            if (dotVV < 1e-6f) // 直线方向向量不能为零
                return Vector3.zero;

            float t = Vector3.Dot(w, lineDir) / dotVV;
            Vector3 projection = t * lineDir;
            return w - projection;
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


            var aCenter = (sphereSixPoints[(int)SphereSixPoint.Right] + sphereSixPoints[(int)SphereSixPoint.Left]) / 2;
            var aRadius = (sphereSixPoints[(int)SphereSixPoint.Right] - sphereSixPoints[(int)SphereSixPoint.Left]).magnitude / 2;
            var axis = GetPointToCube(aCenter, cubeEightPoints, out var inner).normalized;
            if (inner)
                return true;
            // 对每个轴检查投影重叠
            {
                // 计算椭球在轴上的投影区间 [ellipsoidMin, ellipsoidMax]
                float ellipsoidCenterProj = Vector3.Dot(aCenter, axis);
                float ellipsoidMin = ellipsoidCenterProj - aRadius;
                float ellipsoidMax = ellipsoidCenterProj + aRadius;

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

            var aCenter = (aSphereSixPoints[(int)SphereSixPoint.Left] + aSphereSixPoints[(int)SphereSixPoint.Right]) / 2;
            var bCenter = (bSphereSixPoints[(int)SphereSixPoint.Left] + bSphereSixPoints[(int)SphereSixPoint.Right]) / 2;

            return (bCenter - aCenter).magnitude <= (aSphereSixPoints[(int)SphereSixPoint.Left] - aCenter).magnitude + (bSphereSixPoints[(int)SphereSixPoint.Left] - bCenter).magnitude; // 所有轴都重叠，判定碰撞
        }

        public static Vector3 GetPointToCube(Vector3 point, Vector3[] cubeEightPoints, out bool inner)
        {
            inner = false;
            var boxCenter = (cubeEightPoints[(int)CubeEightPoint.LeftUpBack] + cubeEightPoints[(int)CubeEightPoint.RightDownForward]) / 2;
            var min2 = float.MaxValue;
            var id = 0;
            for (int i = 0; i < cubeEightPoints.Length; i++)
            {
                var dis = (cubeEightPoints[i] - point).sqrMagnitude;
                if (dis < min2)
                {
                    min2 = dis;
                    id = i;
                }
            }
            var dir = cubeEightPoints[id] - point;
            var axis = GetCubeClosePoint(cubeEightPoints, (CubeEightPoint)id);
            axis[0] = (cubeEightPoints[id] - axis[0]).normalized;
            axis[1] = (cubeEightPoints[id] - axis[1]).normalized;
            axis[2] = (cubeEightPoints[id] - axis[2]).normalized;
            var res = GetNewCoordinateVector(point - cubeEightPoints[id], axis[0], axis[1], axis[2]);
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
            var res = new Vector3[3];
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
        private static bool CalcTouchTimeAndAvoidTime(float aMin, float aMax, float bMin, float bMax, float dir, ref float touchTime, ref float avoidTime, out int avoidDir)
        {
            avoidDir = 0;
            if (Mathf.Abs(aMax - bMin) < DELTA)
            {
                avoidDir = -1;
            }
            else if (Mathf.Abs(aMin - bMax) < DELTA)
            {
                avoidDir = 1;
            }
            if (Mathf.Abs(dir) <= 0)
            {
                if (aMax <= bMin || aMin >= bMax)
                {
                    avoidTime = 0;
                    touchTime = 1;
                    return true;
                }
            }

            if (aMin >= bMax)
            {
                if (dir < 0)
                {
                    touchTime = Mathf.Max(touchTime, (bMax - aMin) / dir);
                }
                else
                {
                    avoidTime = 0;
                    touchTime = 1;
                    return true;
                }

            }
            else if (aMax <= bMin)
            {
                if (dir > 0)
                {
                    touchTime = Mathf.Max(touchTime, (bMin - aMax) / dir);
                }
                else
                {
                    avoidTime = 0;
                    touchTime = 1;
                    return true;
                }
            }

            if (aMin <= bMin)
            {
                if (dir < 0)
                {
                    avoidTime = Mathf.Min(avoidTime, (bMin - aMax) / dir);
                }
                else
                {
                    avoidTime = Mathf.Min(avoidTime, (bMax - aMin) / dir);
                }
            }
            else
            {
                if (dir > 0)
                {
                    avoidTime = Mathf.Min(avoidTime, (bMax - aMin) / dir);
                }
                else
                {
                    avoidTime = Mathf.Min(avoidTime, (bMin - aMax) / dir);
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
