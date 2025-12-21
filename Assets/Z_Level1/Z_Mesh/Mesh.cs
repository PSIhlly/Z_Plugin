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

    }
    public static class Mesh
    {
        public static MeshInfo GetMesh(BoxCollider box, Vector3 pos, Vector3 euler, Vector3 scale)
        {
            return GetMesh(Graph.ElementwiseMultiply(box.center, scale) + pos, euler, Graph.ElementwiseMultiply(box.size, scale));
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
            return GetMesh(Graph.ElementwiseMultiply(sp.center, scale) + pos, sp.radius, euler, scale);
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
