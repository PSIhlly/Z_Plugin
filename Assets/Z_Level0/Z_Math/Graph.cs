using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Z_Math
{
    public static class Graph
    {
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
        public static Vector3 SetX(this Vector3 v3, float x)
        {
            return new Vector3(x, v3.y, v3.z);
        }
        public static Vector3 SetY(this Vector3 v3, float y)
        {
            return new Vector3(v3.x, y, v3.z);
        }
        public static Vector3 SetZ(this Vector3 v3,float z)
        {
            return new Vector3(v3.x, v3.y, z);
        }
        public static Vector3[] RotatePointAroundOrigin(Vector3[] points, Vector3 euler)
        {
            Vector3[] newPos = new Vector3[points.Length];
            Quaternion rotation = Quaternion.Euler(euler);
            for (int i=0,icnt=points.Length;i<icnt;i++)
            {
                // 将欧拉角转换为四元数
                // 使用四元数旋转点
                newPos[i]= rotation * points[i];
            }
            return newPos;
        }
        public static Vector3[] GetCubeEightPoint(Vector3 center,Vector3 size,Vector3 euler,Vector3 scale,Vector3 offset)
        {
            
            Vector3[] ans = new Vector3[8];
            Quaternion rotation = Quaternion.Euler(euler);
            //ref CubeEightPoint
            Vector3[] choose = new[] { new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0.5f, -0.5f, -0.5f), new Vector3(-0.5f, 0.5f, -0.5f), new Vector3(0.5f, 0.5f, -0.5f),
                                        new Vector3(-0.5f, -0.5f, 0.5f), new Vector3(0.5f, -0.5f, 0.5f), new Vector3(-0.5f, 0.5f, 0.5f), new Vector3(0.5f, 0.5f, 0.5f)};
            for(int i=0;i<8;i++)
            {
                ans[i] = offset+rotation * ElementwiseMultiply(center + ElementwiseMultiply(choose[i],size), scale);
            }
            return ans;
        }
        public static List<Vector3Int> GetRoughOverlapIntPos(Vector3[] cubeEightPoint)
        {
            int minX = int.MaxValue;
            int minY= int.MaxValue;
            int minZ = int.MaxValue;
            int maxX = int.MinValue;
            int maxY = int.MinValue;
            int maxZ = int.MinValue;
            for (int i = 0; i < 8; i++)
            {
                minX = Mathf.Min((int)cubeEightPoint[i].x,minX);
                minY = Mathf.Min((int)cubeEightPoint[i].y, minY);
                minZ = Mathf.Min((int)cubeEightPoint[i].z, minZ);
                maxX = Mathf.Max((int)(cubeEightPoint[i].x+1), maxX);
                maxY = Mathf.Max((int)(cubeEightPoint[i].y+1), maxY);
                maxZ = Mathf.Max((int)(cubeEightPoint[i].z+1), maxZ);
            }
            List<Vector3Int> res = new List<Vector3Int>((maxX-minX+1)*( maxY - minY + 1)*(maxZ - minZ + 1));
            for (int i = minX; i <= maxX; i++)
                for (int j = minY; j <= maxY; j++)
                    for (int k = minZ; k <= maxZ; k++)
                    {
                        res.Add(new Vector3Int(i,j,k));
                    }
            return res;
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
        public static float GetLineYByX(Vector2 p1,Vector2 p2,float x)
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
            else if(eular >= 45 && eular <= 135)
            {
                return  FourDir.Right;
            }
            else  if (eular >= 135 && eular <= 225)
            {
                return  FourDir.Down;

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
        public static Vector2 GetRelativePos(Vector2 relativePos,RectTransform area)
        {
            Vector3[] cor = new Vector3[4];
            area.GetWorldCorners(cor);
            return new Vector2(cor[0].x + (cor[3].x - cor[0].x) * relativePos.x, cor[0].y + (cor[1].y - cor[0].y) * relativePos.y);
        }
    }
}
