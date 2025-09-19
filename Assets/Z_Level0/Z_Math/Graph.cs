using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static UnityEngine.Networking.UnityWebRequest;
namespace Z_Math
{
    public static class Graph
    {
        public static bool dDebug;
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
        public enum SphereFourPoint
        {
            Center,
            Forward,
            Up,
            Right
        }
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
        public static Vector3[] GetSphereFourPoint(Vector3 center, float radius, Vector3 euler, Vector3 scale)
        {

            Vector3[] ans = new Vector3[4];
            Quaternion rotation = Quaternion.Euler(euler);
            //ref CubeEightPoint
            Vector3[] choose = new[] { Vector3.forward, Vector3.up, Vector3.right };
            ans[0] = center;
            for (int i = 0; i < 3; i++)
            {
                ans[i+1] = center + rotation * ElementwiseMultiply(choose[i] * radius, scale);
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
        public static float Cross(Vector2 from, Vector2 to, Vector2 o)
        {
            return (to.x - from.x) * (o.y - from.y) - (to.y - from.y) * (o.x - from.x);
        }
        public static float GetLineYByX(Vector2 p1, Vector2 p2, float x)
        {
            float k = (p2.y - p1.y) / (p2.x - p1.x);
            return k * x + (p1.y - k * p1.x);
        }
        public static Vector3 ElementwiseMultiply(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        }
        public static Vector3 ElementwisePlus(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
        }
        public static Vector3 ElementwiseDivide(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
        }
        public static Vector3Int GetVector3Int(Vector3 a)
        {
            return new Vector3Int((int)Math.Round(a.x), (int)Math.Round(a.y), (int)Math.Round(a.z));
        }
        public static IntersectType LineIntersectCube(Vector3[] eightPoints, Vector3 from, Vector3 to, out float dis)
        {
          
            dis = float.MaxValue;
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
            var fromOut = false;
            var toOut = false;
            if (from.x <= x && from.x >= 0 && from.y >= 0 && from.y <= y && from.z >= 0 && from.z <= z)
            {
                fromIn = true;
                dis = 0;
            }
            else
            {
                fromOut = true;
            }

            if (to.x <= x && to.x >= 0 && to.y >= 0 && to.y <= y && to.z >= 0 && to.z <= z)
            {
                toIn = true;
                dis = Mathf.Min((to - from).magnitude, dis);
            }
            else
            {
                toOut = true;
            }
            if (fromIn && toIn)
            {
                return IntersectType.Inner;
            }


            bool inCube = false;
            bool outCube = false;
            Vector3 res;
            if (GetPointOnLineByX(from, to, 0, out res))
            {
                if (res.y <= y && res.y >= 0 && res.z >= 0 && res.z <= z)
                {
                    inCube = true;
                    dis = Mathf.Min((res - from).magnitude, dis);
                }
                else
                {
                    outCube = true;
                }
            }
            if (GetPointOnLineByX(from, to, x, out res))
            {
                if (res.y <= y && res.y >= 0 && res.z >= 0 && res.z <= z)
                {
                    inCube = true;
                    dis = Mathf.Min((res - from).magnitude, dis);
                }
                else
                {
                    outCube = true;
                }
            }
            if (GetPointOnLineByY(from, to, 0, out res))
            {
                if (res.x <= x && res.x >= 0 && res.z >= 0 && res.z <= z)
                {
                    inCube = true;
                    dis = Mathf.Min((res - from).magnitude, dis);
                }
                else
                {
                    outCube = true;
                }
            }
            if (GetPointOnLineByY(from, to, y, out res))
            {
                if (res.x <= x && res.x >= 0 && res.z >= 0 && res.z <= z)
                {
                    inCube = true;
                    dis = Mathf.Min((res - from).magnitude, dis);
                }
                else
                {
                    outCube = true;
                }
            }
            if (GetPointOnLineByZ(from, to, 0, out res))
            {
                if (res.x <= x && res.x >= 0 && res.y >= 0 && res.y <= y)
                {
                    inCube = true;
                    dis = Mathf.Min((res - from).magnitude, dis);
                }
                else
                {
                    outCube = true;
                }
            }
            if (GetPointOnLineByZ(from, to, z, out res))
            {
                if (res.x <= x && res.x >= 0 && res.y >= 0 && res.y <= y)
                {
                    inCube = true;
                    dis = Mathf.Min((res - from).magnitude, dis);
                }
                else
                {
                    outCube = true;
                }
            }

            if (fromOut && (toIn || inCube))
                return IntersectType.In;
            if (fromIn && (toOut || outCube))
                return IntersectType.Out;
            if (fromOut && toOut && inCube)
                return IntersectType.Cross;
            return IntersectType.None;
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
        public static IntersectType LineIntersectSphere(Vector3[] fourPoint, Vector3 from, Vector3 to, out float dis)
        {

            dis = float.MaxValue;
            var forward = fourPoint[(int)SphereFourPoint.Forward] - fourPoint[(int)SphereFourPoint.Center];
            var up = fourPoint[(int)SphereFourPoint.Up] - fourPoint[(int)SphereFourPoint.Center];
            var right = fourPoint[(int)SphereFourPoint.Right] - fourPoint[(int)SphereFourPoint.Center];
            to = GetNewCoordinateVector(to, forward, up, right) - fourPoint[(int)SphereFourPoint.Center];
            from = GetNewCoordinateVector(from, forward, up, right) - fourPoint[(int)SphereFourPoint.Center];

            var x = right.magnitude;
            var y = up.magnitude;
            var z = forward.magnitude;
            float a2 = x * x;
            float b2= y * y;
            float c2 = z * z;
            Vector3 dir = to - from;

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
            float b = 2*(from.x * dir.x / a2 + from.y * dir.y / b2 + from.z * dir.z / c2);
            float c = (from.x * from.x / a2 + from.y * from.y / b2 + from.z * from.z / c2) - 1;

            float delta = b * b - 4 * a * c;
            if(delta<-1e-9)
            {
                return IntersectType.None;
            }


            float sqrtD = Mathf.Sqrt(delta);
            float t1 = (-b - sqrtD) / (2 * a);
            float t2 = (-b + sqrtD) / (2 * a);
            bool t1Valid=false;
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
            if (!t1Valid&&!t2Valid)
            {

                return IntersectType.None;
            }
            if(fromIn)
                return IntersectType.Out;
            return IntersectType.In;
        }
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
        public static bool GetPointInSphere(Vector3 point,float a2,float b2,float c2)
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
        public static bool IsClamp(Vector3 point, Vector3 a1, Vector3 a2)
        {
            return !((point.x > a1.x && point.x > a2.x) || (point.x < a1.x && point.x < a2.x)
                     || (point.y > a1.y && point.y > a2.y) || (point.y < a1.y && point.y < a2.y)
                     || (point.z > a1.z && point.z > a2.z) || (point.z < a1.z && point.z < a2.z));
        }

        public static Vector3 GetNewCoordinateVector(Vector3 old, Vector3 forward, Vector3 up, Vector3 right)
        {
            float x = Vector3.Dot(old, right.normalized);
            float y = Vector3.Dot(old, up.normalized);
            float z = Vector3.Dot(old, forward.normalized);
            return new Vector3(x, y, z);
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
        /// 左下后角 0 0 0
        /// </summary>
        /// <param name="relativePos"></param>
        /// <param name="area"></param>
        public static Vector2 GetNormalizedRelativePos(Vector2 realPos, RectTransform area)
        {
            Vector3[] cor = new Vector3[4];
            area.GetWorldCorners(cor);
            return new Vector2((realPos.x- cor[0].x)/(cor[3].x - cor[0].x), (realPos.y - cor[0].y) / (cor[1].y - cor[0].y));
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
        public static Vector2 GetSize(RectTransform area)
        {
            Vector3[] cor = new Vector3[4];
            area.GetWorldCorners(cor);
            return new Vector2(cor[3].x - cor[0].x, cor[1].y - cor[0].y);
        }
    }
}
