using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Math;
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
        public static MeshInfo GetMesh(BoxCollider box,Vector3 pos,Vector3 euler,Vector3 scale)
        {
            return GetMesh(Graph.ElementwiseMultiply(box.center, scale) + pos, euler, Graph.ElementwiseMultiply(box.size,scale));
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
        public static MeshInfo GetMesh(Vector3 pos, float radius,Vector3 euler, Vector3 scale)
        {
            return new MeshInfo()
            {
                type = MeshType.Sphere,
                center = pos,
                positions = Graph.GetSphereFourPoint(pos, radius, euler, scale)
            };
        }
        }
}
