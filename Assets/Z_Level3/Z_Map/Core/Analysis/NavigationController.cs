//#define DEBUG
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
        public float[] dirMaxY;
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
            step = _super.data.mainData.mapUnitSize.y * 1 / 3;
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
                    var realPos = MapPos2RealPos(new Vector3Int(pos.Item1, pos.Item2, pos.Item2));


                    newUnit.dirMaxY = new float[4] { map.unit.GetYByPoint(offset[0]), map.unit.GetYByPoint(offset[1]), map.unit.GetYByPoint(offset[2]), map.unit.GetYByPoint(offset[3]) };
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

                    //prefab根的lossyScale，用于把collider的lossyScale换算成“相对root的局部scale”，
                    //避免与obs.scale相乘时重复计入prefab根的缩放（prefab根scale非1时会出错）
                    Vector3 rootLossyScale = obs.unit.prefab.transform.lossyScale;
                    foreach (var bc in obs.unit.prefab.GetComponentsInChildren<BoxCollider>())
                    {
                        Vector3 finalScale = Z_Math.Graph.ElementwiseMultiply(Z_Math.Graph.ElementwiseDivide(bc.transform.lossyScale, rootLossyScale), obs.scale);
                        var mesh = Mesh.GetMesh(bc, obs.pos + Vector3.up * obs.scale.y / 2, obs.euler, finalScale);
                        Vector3[] points = mesh.positions;
                        var maxY = mesh.GetMaxY();
                        var overlapPoses = Z_Math.Graph.GetRoughOverlapIntPos(points);
                        //simple
                        var quad = new Vector2[] { new Vector2(points[(int)Z_Math.Graph.CubeEightPoint.LeftDownForward].x, points[(int)Z_Math.Graph.CubeEightPoint.LeftDownForward].z),
                            new Vector2(points[(int)Z_Math.Graph.CubeEightPoint.RightDownForward].x, points[(int)Z_Math.Graph.CubeEightPoint.RightDownForward].z),
                            new Vector2(points[(int)Z_Math.Graph.CubeEightPoint.RightDownBack].x, points[(int)Z_Math.Graph.CubeEightPoint.RightDownBack].z),
                            new Vector2(points[(int)Z_Math.Graph.CubeEightPoint.LeftDownBack].x, points[(int)Z_Math.Graph.CubeEightPoint.LeftDownBack].z) };
                        foreach (var pos in overlapPoses)
                        {
                            if (!navUnits.TryGetValue((pos.x, pos.y, pos.z), out var unit))
                            {
                                continue;
                            }
                            for (int dir = 0; dir < offset.Length; dir++)
                            {
                                var dir2D = new Vector2(unit.pos.x + offset[dir].x, unit.pos.z + offset[dir].y);
                                if (Graph.IsPointInQuad(quad, dir2D))
                                {
                                    unit.dirMaxY[dir] = Mathf.Max(maxY, unit.dirMaxY[dir]);
                                }
                            }
                        }
                    }
                }
            }

            /*         curUpdateCount = 0;
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
                         if (navUnits.TryGetValue(pos, out var unit))
                         {
                             float max = float.MinValue;
                             float min = float.MaxValue;
                             for (int dir = 0; dir < offset.Length; dir++)
                             {
                                 max = Mathf.Max(unit.dirMaxY[dir], max);
                                 min = Mathf.Max(unit.dirMaxY[dir], min);
                             }
                             if (max - min > this.step)
                             {
                                 blocked.Add(pos);
                             }
                         }


                     }*/
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

                        if (!InArea(linkPos) || blocked.Contains((linkPos.x, linkPos.y, linkPos.z)))
                            continue;
                        var link = _super.utilCtrl.GetTileData(linkPos.x, linkPos.y, linkPos.z);


                        //can move
                        if (link != null)
                        {

                            var linkNavUnit = navUnits[(linkPos.x, linkPos.y, linkPos.z)];

                            if (linkNavUnit.dirMaxY[l ^ 1] - navUnit.dirMaxY[l] < this.step)
                                navUnit.links.Add(linkNavUnit);
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
        public Vector3Int GetClosestExistInArea(Vector3Int pos)
        {
            return _super.utilCtrl.GetClosestExistInArea(pos);
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
