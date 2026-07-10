using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;
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
        public List<NavUnit> links;
    }
    public interface NaviComponent
    {
        public Vector3 GetNextDir(Vector3 cur, Vector3 tar, int maxStep);
    }
    public class NavigationController : Z_Controller<MapManager>
    {
        public NavigationController(MapManager super) : base(super)
        { }
        NaviComponent bfs;
        public Dictionary<(int, int, int), NavUnit> navUnits;
        public float step;
        public void Build()
        {
            step = _super.data.mainData.mapUnitSize.y * 1 / 10;
            navUnits = new Dictionary<(int, int, int), NavUnit>(_super.data.maps.Count);
            UpdateMap(int.MaxValue);
            bfs = new Bfs(this);
        }
        Vector3Int[] tryDir = new Vector3Int[] { Vector3Int.right, Vector3Int.left, Vector3Int.forward, Vector3Int.back };
        Vector2[] offset = new Vector2[] { Vector2.right * 0.25f, Vector2.left * 0.25f, Vector2.up * 0.25f, Vector2.down * 0.25f };
        List<TileUnitForm.Data> curUpdateTileList = new List<TileUnitForm.Data>();
        List<ObjectUnitForm.Data> curUpdateObjList = new List<ObjectUnitForm.Data>();
        int curUpdateCount = 0;
        public void UpdateMap(int step)
        {
            _super.StartCoroutine(UpdateInternal(step));
        }
        IEnumerator UpdateInternal(int step)
        {
            int times = step;
            curUpdateCount = 0;
            //build single unit
            curUpdateTileList.Clear();
            curUpdateTileList.AddRange(_super.data.maps.Values);
            for (; curUpdateCount < curUpdateTileList.Count; curUpdateCount++)
            {
                times++;
                if (times >= step)
                {
                    times = 0;
                    yield return null;
                }
                if (!_super.enable)
                {
                    yield break;
                }
                var map = curUpdateTileList[curUpdateCount];
                if (!TileUnitForm.DataByUid.ContainsKey(map.uid))
                    continue;
                (int, int, int) pos = (map.mapPos.x, map.mapPos.y, map.mapPos.z);
                if (!navUnits.ContainsKey(pos))
                {
                    var newUnit = new NavUnit();
                    navUnits[pos] = newUnit;
                    newUnit.links = new List<NavUnit>();
                    newUnit.realPos = _super.utilCtrl.GetTileData(pos.Item1, pos.Item2, pos.Item3).pos;
                    newUnit.pos = new Vector3Int(pos.Item1, pos.Item2, pos.Item3);
                    newUnit.isNull = _super.utilCtrl.GetTileData(pos.Item1, pos.Item2, pos.Item3).scale == Vector3.zero;
                }
            }
            //先扫描障碍物，记录blocked位置
            curUpdateObjList.Clear();
            curUpdateObjList.AddRange(ObjectUnitForm.DataByUid.Values);
            curUpdateCount = 0;
            var blocked = new HashSet<(int, int, int)>();
            for (; curUpdateCount < curUpdateObjList.Count; curUpdateCount++)
            {
                var obs = curUpdateObjList[curUpdateCount];
                times++;
                if (times >= step)
                {
                    times = 0;
                    yield return null;
                }
                if (!_super.enable)
                {
                    yield break;
                }
                if (!ObjectUnitForm.DataByUid.ContainsKey(obs.uid))
                    continue;
                if (obs != null && obs.isObstacle)
                {

                    foreach (var bc in obs.unit.prefab.GetComponentsInChildren<BoxCollider>())
                    {
                        Vector3[] points = Mesh.GetMesh(bc, obs.pos + Vector3.up * obs.scale.y / 2, obs.euler, Graph.ElementwiseMultiply(bc.transform.lossyScale, obs.scale)).positions;
                        var overlapPoses = Z_Math.Graph.GetRoughOverlapIntPos(points);
                        //simple
                        var quad = new Vector2[] { new Vector2(points[(int)Z_Math.Graph.CubeEightPoint.LeftDownForward].x, points[(int)Z_Math.Graph.CubeEightPoint.LeftDownForward].z),
                            new Vector2(points[(int)Z_Math.Graph.CubeEightPoint.RightDownForward].x, points[(int)Z_Math.Graph.CubeEightPoint.RightDownForward].z),
                            new Vector2(points[(int)Z_Math.Graph.CubeEightPoint.RightDownBack].x, points[(int)Z_Math.Graph.CubeEightPoint.RightDownBack].z),
                            new Vector2(points[(int)Z_Math.Graph.CubeEightPoint.LeftDownBack].x, points[(int)Z_Math.Graph.CubeEightPoint.LeftDownBack].z) };
                        foreach (var pos in overlapPoses)
                        {
                            var mapPos = RealPos2MapPosInt(pos);
                            for (int i = 0, icnt = offset.Length; i < icnt; i++)
                            {
                                if (InArea(mapPos) && Z_Math.Graph.IsPointInQuad(quad, new Vector2(pos.x, pos.z) + offset[i]))
                                {
                                    //检测是否能达到y+1的tile（参考高度差判断）
                                    bool canReachY1 = false;
                                    Vector3Int linkPos = new Vector3Int(mapPos.x + tryDir[i].x, mapPos.y + 1, mapPos.z + tryDir[i].z);
                                    if (InArea(linkPos))
                                    {
                                        var link = _super.utilCtrl.GetTileData(linkPos.x, linkPos.y, linkPos.z);
                                        var curMap = _super.utilCtrl.GetTileData(mapPos.x, mapPos.y, mapPos.z);
                                        Vector2 p = new Vector2(tryDir[i].x * 0.5f, tryDir[i].z * 0.5f);
                                        if (link != null && curMap != null && Math.Abs(link.unit.GetYByPoint(-p) - curMap.unit.GetYByPoint(p)) <= this.step)
                                        {
                                            canReachY1 = true;
                                        }
                                    }
                                    if (!canReachY1)
                                    {
                                        blocked.Add((mapPos.x, mapPos.y, mapPos.z));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            //构建links，跳过blocked的tile
            curUpdateCount = 0;
            for (; curUpdateCount < curUpdateTileList.Count; curUpdateCount++)
            {
                times++;
                if (times >= step)
                {
                    times = 0;
                    yield return null;
                }
                if (!_super.enable)
                {
                    yield break;
                }
                var map = curUpdateTileList[curUpdateCount];
                if (!TileUnitForm.DataByUid.ContainsKey(map.uid))
                    continue;
                (int, int, int) pos = (map.mapPos.x, map.mapPos.y, map.mapPos.z);

                var navUnit = navUnits[pos];
                navUnit.links.Clear();
                if (blocked.Contains(pos))
                    continue;
                for (int m = -1; m <= 1; m++)
                    for (int l = 0; l < 4; l++)
                    {
                        Vector3Int linkPos = Z_Math.Graph.GetVector3Int(Z_Math.Graph.ElementwisePlus(new Vector3Int(pos.Item1, m + pos.Item2, pos.Item3), tryDir[l]));
                        if (!InArea(linkPos))
                            continue;
                        var link = _super.utilCtrl.GetTileData(linkPos.x, linkPos.y, linkPos.z);

                        Vector2 p = new Vector2(tryDir[l].x * 0.5f, tryDir[l].z * 0.5f);

                        //can move
                        if (link!=null&&Math.Abs(link.unit.GetYByPoint(-p) - map.unit.GetYByPoint(p)) <= step)
                        {
                            var linkNavUnit = navUnits[(linkPos.x, linkPos.y, linkPos.z)];
                            if (!blocked.Contains((linkPos.x, linkPos.y, linkPos.z)))
                            {
                                navUnit.links.Add(linkNavUnit);
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
        public Vector3Int RealPos2MapPosInt(Vector3 pos)
        {
            return _super.utilCtrl.RealPos2MapPosInt(pos);
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
