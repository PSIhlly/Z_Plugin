using System;
using System.Collections.Generic;
using UnityEngine;
using Z_DesignStyle;
using Z_Map.Form;
using Z_Math;

using Mesh = Z_Mesh.Mesh;

namespace Z_Map.Analysis
{
    public enum Dir
    {
        Right,
        Left,
        Forward,
        Back
    }
    public class NavUnit
    {
        public bool isNull;
        public Vector3Int pos;
        public Vector3 realPos;
        public HashSet<Dir> cantPassParts;
        public List<NavUnit> links;
    }
    public interface NaviComponent
    {
        public Vector3 GetNextDir(Vector3 cur, Vector3 tar, int maxStep);
    }
    public class NavigationController : Z_Controller<MapManager>
    {
        public NavigationController(MapManager super):base(super)
        { }
        NaviComponent bfs;
        public Dictionary<(int, int, int), NavUnit> navUnits;
        public float step;
        public void Build()
        {
            step = _super.data.mainData.mapUnitSize.y * 1 / 10;
            InitMap();
            bfs = new Bfs(this);
        }

        public void InitMap()
        {
            Vector3Int[] tryDir = new Vector3Int[] { Vector3Int.right, Vector3Int.left, Vector3Int.forward, Vector3Int.back };
            Vector2[] offset = new Vector2[] { Vector2.right * 0.25f, Vector2.left * 0.25f, Vector2.up * 0.25f, Vector2.down * 0.25f };

            navUnits = new Dictionary<(int, int, int), NavUnit>(_super.data.maps.Count);

            //build single unit
            foreach (var map in _super.data.maps.Values)
            {
                var navUnit = new NavUnit();
                (int, int, int) pos = (map.mapPos.x, map.mapPos.y, map.mapPos.z);
                navUnits[pos] = navUnit;
                navUnit.cantPassParts = new HashSet<Dir>();
                navUnit.links = new List<NavUnit>();
                navUnit.realPos = _super.data.maps[pos].pos;
                navUnit.pos = new Vector3Int(pos.Item1, pos.Item2, pos.Item3);
                navUnit.isNull = _super.data.maps[pos].scale == Vector3.zero;
            }

            //4 dir link
            foreach (var map in _super.data.maps.Values)
            {
                (int, int, int) pos = (map.mapPos.x, map.mapPos.y, map.mapPos.z);
                var navUnit = navUnits[pos];

                for (int m = -1; m <= 1; m++)
                    for (int l = 0; l < 4; l++)
                    {
                        Vector3Int linkPos = Z_Math.Graph.GetVector3Int(Z_Math.Graph.ElementwisePlus(new Vector3Int(pos.Item1, m + pos.Item2, pos.Item3), tryDir[l]));
                        if (!InArea(linkPos))
                            continue;
                        var link = _super.data.maps[(linkPos.x, linkPos.y, linkPos.z)];

                        Vector2 p = new Vector2(tryDir[l].x * 0.5f, tryDir[l].z * 0.5f);

                        //can move
                        if (Math.Abs(link.unit.GetYByPoint(-p) - map.unit.GetYByPoint(p)) <= step)
                        {

                            navUnit.links.Add(navUnits[(linkPos.x, linkPos.y, linkPos.z)]);
                        }
                    }
            }

            foreach (var obs in ObjectUnitForm.DataByUid.Values)
            {
                if (obs != null && obs.isObstacle)
                {

                    foreach (var bc in obs.unit.prefab.GetComponentsInChildren<BoxCollider>())
                    {
                        Vector3[] points = Mesh.GetMesh(bc, obs.pos+Vector3.up * obs.scale.y / 2, obs.euler, obs.scale).positions;
                        var overlapPoses = Z_Math.Graph.GetRoughOverlapIntPos(points);
                        //simple
                        var quad = new Vector2[] { new Vector2(points[(int)Z_Math.Graph.CubeEightPoint.LeftDownForward].x, points[(int)Z_Math.Graph.CubeEightPoint.LeftDownForward].z),
                            new Vector2(points[(int)Z_Math.Graph.CubeEightPoint.RightDownForward].x, points[(int)Z_Math.Graph.CubeEightPoint.RightDownForward].z),
                            new Vector2(points[(int)Z_Math.Graph.CubeEightPoint.RightDownBack].x, points[(int)Z_Math.Graph.CubeEightPoint.RightDownBack].z),
                            new Vector2(points[(int)Z_Math.Graph.CubeEightPoint.LeftDownBack].x, points[(int)Z_Math.Graph.CubeEightPoint.LeftDownBack].z) };
                        foreach (var pos in overlapPoses)
                        {
                            var mapPos = RealPos2MapPos(pos);
                            for (int i = 0, icnt = offset.Length; i < icnt; i++)
                            {
                                if (InArea(mapPos) && Z_Math.Graph.IsPointInQuad(quad, new Vector2(pos.x, pos.z) + offset[i]))
                                {
                                    navUnits[(mapPos.x, mapPos.y, mapPos.z)].cantPassParts.Add((Dir)i);
                                }
                            }
                        }
                    }
                }
            }
        }
        public Vector3 GetNormalWithoutY(Vector3 tar)
        {
            tar.y = 0;
            return tar.normalized;
        }
        public Vector3 GetNextDir(Vector3 cur, Vector3 tar, int maxStep)
        {
            var res = bfs.GetNextDir(cur, tar, maxStep);
            res.y = 0;
            return res;
        }
        public Vector3Int RealPos2MapPos(Vector3 pos)
        {
            return _super.utilCtrl.RealPos2MapPos(pos);
        }
        public Vector3Int GetClosestInArea(Vector3Int pos)
        {
            return _super.utilCtrl.GetClosestInArea(pos);
        }
        public bool InArea(Vector3Int pos)
        {
            return _super.utilCtrl.InArea(pos);
        }
        public Vector3 MapPos2RealPos(Vector3Int pos)
        {
            return _super.utilCtrl.MapPos2RealPos(pos);
        }
        public Dir GetDir(Vector3 self, Vector3 tar)
        {
            if (self.x < tar.x - 0.01f)
                return Dir.Right;
            if (self.x > tar.x + 0.01f)
                return Dir.Left;
            if (self.z < tar.z - 0.01f)
                return Dir.Forward;
            return Dir.Back;
        }
    }

}
