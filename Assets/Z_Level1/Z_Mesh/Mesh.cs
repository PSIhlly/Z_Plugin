using System;
using UnityEngine;
using Z_Math;
using static Z_Math.Graph;
namespace Z_Mesh
{
    public enum MeshType
    {
        None,
        Cube,
        Sphere
    }
    public class MeshInfo
    {
        public MeshType type;
        public Vector3[] positions;
        public Vector3 center;
        public float GetMaxY()
        {
            float maxY = positions[0].y;
            foreach(var pos in  positions)
            {
                maxY = Mathf.Max(pos.y, maxY);
            }
            return maxY;
        }
    }
    public static class Mesh
    {
        public static MeshInfo GetMesh(BoxCollider box, Vector3 pos, Vector3 euler, Vector3 scale)
        {
            //box.center是collider局部空间偏移，需按collider世界旋转euler旋转后再叠加到pos上
            //否则当父物体有旋转且center非零时，center会按世界轴向偏移，导致盒子位置/形态错误
            Quaternion rot = Quaternion.Euler(euler);
            Vector3 center = pos + rot * Graph.ElementwiseMultiply(box.center, scale);
            return GetMesh(center, euler, Graph.ElementwiseMultiply(box.size, scale));
        }
        public static MeshInfo GetMesh(Vector3 pos, Vector3 euler, Vector3 size)
        {
            return new MeshInfo()
            {
                type = MeshType.Cube,
                center = pos,
                positions = Graph.GetCubeEightPoint(pos, size, euler)
            };
        }
        public static MeshInfo GetMesh(SphereCollider sp, Vector3 pos, Vector3 euler, Vector3 scale)
        {
            //sp.center同理，需按collider世界旋转euler旋转后再叠加到pos上
            Quaternion rot = Quaternion.Euler(euler);
            Vector3 center = pos + rot * Graph.ElementwiseMultiply(sp.center, scale);
            return GetMesh(center, sp.radius, euler, scale);
        }
        public static MeshInfo GetMesh(Vector3 pos, float radius, Vector3 euler, Vector3 scale)
        {
            return new MeshInfo()
            {
                type = MeshType.Sphere,
                center = pos,
                positions = Graph.GetSphereSixPoint(pos, radius, euler, scale)
            };
        }
        /// <summary>
        /// Mesh交叉检测入口：根据Mesh类型(Cube/Sphere)组合分发到对应的SAT检测函数
        /// 返回碰撞类型、碰撞距离dis和避障法线方向avoidDir
        /// </summary>
        public static IntersectType MeshIntersectMesh(MeshInfo o, MeshInfo tar, Vector3 step, out float dis, out Vector3 avoidDir)
        {
            avoidDir = Vector3.zero;
            float curDis = 0;
            float length = step.magnitude;
            dis = length;
            var assist = new Graph.IntersectAssisant(false);
            switch (o.type)
            {
                case MeshType.Cube:
                    switch (tar.type)
                    {
                        case Z_Mesh.MeshType.Cube:
                            {
                                assist.Add(Graph.CubeIntersectCube(o.positions, tar.positions, step, out curDis, out avoidDir));
                                dis = Math.Min(dis, curDis);
                                break;
                            }
                        case Z_Mesh.MeshType.Sphere:
                            {
                                assist.Add(Graph.SphereIntersectCube(tar.positions, o.positions, -step, out curDis, out avoidDir));
                                dis = Math.Min(dis, curDis);
                                break;
                            }
                    }
                    break;
                case MeshType.Sphere:
                    switch (tar.type)
                    {
                        case Z_Mesh.MeshType.Cube:
                            {
                                assist.Add(Graph.SphereIntersectCube(o.positions, tar.positions, step, out curDis, out avoidDir));
                                dis = Math.Min(dis, curDis);
                                break;
                            }
                        case Z_Mesh.MeshType.Sphere:
                            {
                                assist.Add(Graph.SphereIntersectSphere(o.positions, tar.positions, step, out curDis, out avoidDir));
                                dis = Math.Min(dis, curDis);
                                break;
                            }
                    }
                    break;
            }

            return assist.GetRes();
        }

    }
}
