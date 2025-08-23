using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_DataSystem.Form;
using Z_Debug;
using Z_DesignStyle;
using Z_Map;
using Z_Map.Analysis;
using Z_Map.Form;
using Z_UnitSystem;
using Z_UnitSystem.Form;

namespace Z_Map
{
    public static class GlobalSettings
    {
        public const int TEX_ANIM_MAX = 10;
        public const int ITEM_UNIT_MAX = 10;
        public const bool NAV_DEBUG = true;
        public const bool MAP_SHOW_DEBUG = false;
        public const bool OVERLAY_HIDE = true;
        public const bool UPDATE_TILE_ALWAYS = true;
    }

    public static class GlobalHelper
    {

        public static string GetInternalPrefabName(string name="")
        {
            return "z_map$" + name;
        }
    }
    public enum MapEventType
    {
        Show,
        AfterUpdate,
    }
    public class ItemEvent : Z_Event
    {
        public ItemUnit unit;
        public MapEventType type;
    }
    public class ObjectEvent : Z_Event
    {
        public ObjectUnit unit;
        public MapEventType type;
    }
    public class TileEvent : Z_Event
    {

        public TileUnit unit;
        public MapEventType type;
    }

    public class CharacterEvent : Z_Event
    {
        public CharacterUnit unit;
        public MapEventType type;
    }

    public partial class MapUnit : Unit
    {
        public MapUnit(UnitForm.Data data) : base(data)
        {
        }
        public MapInstance ins
        {
            set { base.ins = value; }
            get { return (MapInstance)base.ins; }
        }
    }
    public partial class MapInstance : Instance
    {
        public MapUnit unit
        {
            set { base.unit = value; }
            get { return (MapUnit)base.unit; }
        }
        private float degree=0;
        public override void VisOn()
        {
            if (vising)
                return;
            vising = true;
            foreach (var render in renderers)
            {
                MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
                render.GetPropertyBlock(propBlock);
                propBlock.SetFloat("_Show", 1);
                degree = 1;
                render.SetPropertyBlock(propBlock);
            }
        }
        public override void VisDegree(float degree)
        {
            if (degree==this.degree)
                return;
            foreach (var render in renderers)
            {
                MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
                render.GetPropertyBlock(propBlock);
                propBlock.SetFloat("_Show", degree);
                this.degree = degree;
                render.SetPropertyBlock(propBlock);
            }

        }
        public override void VisOff()
        {
            if (!vising)
                return;
            vising = false;
            foreach (var render in renderers)
            {
                MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
                render.GetPropertyBlock(propBlock);
                propBlock.SetFloat("_Show", 0);
                degree = 0;
                render.SetPropertyBlock(propBlock);
            }
        }
    }
}

public class MapManager : Z_MonoManager<MapManager>
{

    public Vector3 sizeLimit = new Vector3(1000, 1000, 1000);
    public MapData data;
    public GameObject mainGo;


    public NavigationController navigationCtrl;
    public MapUtilController utilCtrl;

    private Vector3 curCenterPos;
    private Vector3 lastCenterPos;
    private Vector3Int viewCenter;

    public List<TileUnitForm.Data> curMapLst
    {
        get;
        private set;
    }
    = new List<TileUnitForm.Data>();

    public override void Init()
    {
        base.Init();

        navigationCtrl = new NavigationController(this);

        utilCtrl = new MapUtilController(this);
    }

    #region external

    public void Begin(MapData dataCtrl)
    {
        End();
        Init();
        mainGo.SetActive(true);
        this.data = dataCtrl;

        viewCenter = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);
        foreach (var itemData in ItemUnitForm.DataByUid.Values)
        {
            var mapPos = utilCtrl.RealPos2MapPos(itemData.pos);
            if (utilCtrl.InArea(mapPos))
            {
                TileUnitForm.DataByUid[dataCtrl.maps[(mapPos.x, mapPos.y, mapPos.z)].uid].unit.Bind(itemData.unit);
            }
        }
        foreach (var objectData in ObjectUnitForm.DataByUid.Values)
        {
            var mapPos = utilCtrl.RealPos2MapPos(objectData.pos);
            if (utilCtrl.InArea(mapPos))
            {
                TileUnitForm.DataByUid[dataCtrl.maps[(mapPos.x, mapPos.y, mapPos.z)].uid].unit.Bind(objectData.unit);
            }
        }
        foreach (var characterData in CharacterUnitForm.DataByUid.Values)
        {
            var mapPos = utilCtrl.RealPos2MapPos(characterData.pos);
            if (utilCtrl.InArea(mapPos))
            {
                TileUnitForm.DataByUid[dataCtrl.maps[(mapPos.x, mapPos.y, mapPos.z)].uid].unit.Bind(characterData.unit);
            }
        }

        navigationCtrl.Build();
        mainGo.SetActive(true);
    }
    #region unit
    public TileUnitForm.Data AddTile(Vector3Int mapPos, object[] prms = null)
    {
        return data.AddTile(mapPos, prms);
    }
    public ObjectUnitForm.Data AddObject(Vector3 realPos, string prefabName, object[] prms = null)
    {
        var mapPos = utilCtrl.RealPos2MapPos(realPos);
        if (!this.data.maps.ContainsKey((mapPos.x, mapPos.y, mapPos.z)))
        {
            return null;
        }
        var data = this.data.AddObject(prefabName, prms);
        data.pos = realPos;
        this.data.maps[(mapPos.x, mapPos.y, mapPos.z)].unit.Bind(data.unit);
        return data;
    }
    public ItemUnitForm.Data AddItem(Vector3 realPos, string prefabName, object[] prms = null)
    {
        var mapPos = utilCtrl.RealPos2MapPos(realPos);
        if (!this.data.maps.ContainsKey((mapPos.x, mapPos.y, mapPos.z)))
        {
            return null;
        }
        var data = this.data.AddItem(prefabName, prms);
        data.pos = realPos;
        this.data.maps[(mapPos.x, mapPos.y, mapPos.z)].unit.Bind(data.unit);
        return data;
    }
    public CharacterUnitForm.Data AddCharacter(Vector3 realPos, string prefabName, bool isMine = false,object[] prms=null)
    {
        var mapPos = utilCtrl.RealPos2MapPos(realPos);
        if (!this.data.maps.ContainsKey((mapPos.x, mapPos.y, mapPos.z)))
        {
            return null;
        }
        var data = this.data.AddCharacter(prefabName,isMine, prms);
        data.pos = realPos;
        this.data.maps[(mapPos.x, mapPos.y, mapPos.z)].unit.Bind(data.unit);
        return data;
    }
    public void RemoveTile(TileUnitForm.Data form)
    {
        data.RemoveTile(form);
    }
    public void RemoveCharacter(CharacterUnitForm.Data form)
    {
        data.RemoveCharacter(form);
    }
    public void RemoveItem(ItemUnitForm.Data form)
    {
        data.RemoveItem(form);
    }
    public void RemoveObject(ObjectUnitForm.Data form)
    {
        data.RemoveObject(form);
    }
    #endregion
    public void SetPos(Vector3 curCenterPos)
    {
        this.curCenterPos = curCenterPos;
    }

    public void End()
    {
        if (curMapLst != null)
            curMapLst.Clear();
        mainGo.SetActive(false);
        if (data != null)
        {
            data.Unload();
            data = null;
        }
        lastView = (0, 0, 0, 0, 0, 0);
        lastCenterPos = Vector3.one * -9999999;
    }


    #endregion


    (int, int, int, int, int, int) lastView;

    private void FreshMap()
    {

        List<TileUnitForm.Data> nowTmp = new List<TileUnitForm.Data>();
        var viewSize = data.mainData.viewSize;
        var curView = (viewCenter.x - viewSize.x, viewCenter.x + viewSize.x, viewCenter.y - viewSize.y, viewCenter.y + viewSize.y, viewCenter.z - viewSize.z, viewCenter.z + viewSize.z);
        (int, int, int, int, int, int) commonView = (Mathf.Max(curView.Item1, lastView.Item1), Mathf.Min(curView.Item2, lastView.Item2),
            Mathf.Max(curView.Item3, lastView.Item3), Mathf.Min(curView.Item4, lastView.Item4),
            Mathf.Max(curView.Item5, lastView.Item5), Mathf.Min(curView.Item6, lastView.Item6));

        //old:
        foreach (var map in curMapLst)
        {
            if (map.mapPos.x >= curView.Item2 || map.mapPos.x < curView.Item1
                || map.mapPos.y >= curView.Item4 || map.mapPos.y < curView.Item3
                 || map.mapPos.z >= curView.Item6 || map.mapPos.z < curView.Item5)
            {
                map.unit.Hide();
            }
            else
            {
                nowTmp.Add(map);
            }
        }
        //fill
        if (curView.Item1 < lastView.Item1)
        {
            ShowAndAddLst(nowTmp, curView.Item1, Mathf.Min(lastView.Item1, curView.Item2), curView.Item3, curView.Item4, curView.Item5, curView.Item6);
        }
        else if (curView.Item2 > lastView.Item2)
        {
            ShowAndAddLst(nowTmp, Mathf.Max(lastView.Item2, curView.Item1), curView.Item2, curView.Item3, curView.Item4, curView.Item5, curView.Item6);
        }

        if (curView.Item3 < lastView.Item3)
        {
            ShowAndAddLst(nowTmp, commonView.Item1, commonView.Item2, curView.Item3, Mathf.Min(lastView.Item3, curView.Item4), curView.Item5, curView.Item6);
        }
        else if (curView.Item4 > lastView.Item4)
        {
            ShowAndAddLst(nowTmp, commonView.Item1, commonView.Item2, Mathf.Max(lastView.Item4, curView.Item3), curView.Item4, curView.Item5, curView.Item6);
        }

        if (curView.Item5 < lastView.Item5)
        {
            ShowAndAddLst(nowTmp, commonView.Item1, commonView.Item2, commonView.Item3, commonView.Item4, curView.Item5, Mathf.Min(lastView.Item5, curView.Item6));
        }
        else if (curView.Item6 > lastView.Item6)
        {
            ShowAndAddLst(nowTmp, commonView.Item1, commonView.Item2, commonView.Item3, commonView.Item4, Mathf.Max(lastView.Item6, curView.Item5), curView.Item6);
        }


        curMapLst = nowTmp;
        lastView = curView;
    }
    private void ShowAndAddLst(List<TileUnitForm.Data> lst, int minX, int maxX, int minY, int maxY, int minZ, int maxZ)
    {
        for (int i = minX; i < maxX; i++)
        {

            for (int k = minZ; k < maxZ; k++)
            {
                if (!data.mapXZ2Y.ContainsKey((i, k)))
                    continue;
                int yMax = Mathf.Min(data.mapXZ2Y[(i, k)].Max + 1, maxY);
                int yMin = Mathf.Max(data.mapXZ2Y[(i, k)].Min, minY);
                for (int j = yMin; j < yMax; j++)
                {
                    if (!utilCtrl.InArea((i, j, k)))
                        continue;
                    var map = data.maps[(i, j, k)];
                    map.unit.Show();
                    lst.Add(map);

                }
            }
        }


    }


    private void UpdateMapInfo()
    {
        //return;
        //update
        if(GlobalSettings.UPDATE_TILE_ALWAYS)
        {
            foreach (var map in curMapLst)
            {
                map.unit.UpdateInfo();
            }
        }
       

    }
    (int, int)[] dir9 = new[] {
                    (0, 0), (1, 0), (-1, 0),
                    (0, 1), (0, -1), (1, 1),
                    (-1,-1),(1, -1),(-1, 1)
                };
    /// <summary>
    /// Manage vison
    /// </summary>
    private void UpdateVision()
    {
        if (GlobalSettings.OVERLAY_HIDE)
        {
            var viewSize = data.mainData.viewSize;
            foreach (var curMap in curMapLst)
            {
                curMap.unit.VisOn();
                curMap.unit.VisDegree(1f);
            }
            HashSet<(int, int)> visited = new HashSet<(int, int)>();
            Queue<(int, int)> queue = new Queue<(int, int)>();
            for (int i = viewCenter.y + 1; i < viewCenter.y + viewSize.y; i++)
            {
                visited.Clear();
                queue.Clear();

                List<((int,int), float)> dis = new List<((int, int), float)>();
                for (int dirId = 0; dirId < dir9.Length; dirId++)
                {
                    var dir = dir9[dirId];
                    dir.Item1 += viewCenter.x;
                    dir.Item2 += viewCenter.z;
                    dis.Add((dir, new Vector2(curCenterPos.x - dir.Item1, curCenterPos.z - dir.Item2).magnitude));
                }
                dis.Sort((a, b) => { return a.Item2.CompareTo(b.Item2); });

                foreach (var d in dis)
                {
                    var dir = d.Item1;
                    float degree = Math.Clamp(d.Item2 - 0.75f, 0, 1);
                    if (data.maps.ContainsKey((dir.Item1, i, dir.Item2)))
                    {
                        if(!visited.Contains(dir))
                        {
                            visited.Add(dir);
                            queue.Enqueue(dir);
                        }
                        while (queue.Count > 0)
                        {
                            var cur = queue.Dequeue();
                            if (d.Item2 <= 0)
                            {
                                data.maps[(cur.Item1, i, cur.Item2)].unit.VisOff();
                            }
                            else
                            {
                                data.maps[(cur.Item1, i, cur.Item2)].unit.VisDegree(degree);
                            }
                            for (int x = cur.Item1 - 1; x <= cur.Item1 + 1 && x < dir.Item1 + viewSize.x && x >= dir.Item1 - viewSize.x; x += 2)
                            {
                                if (!visited.Contains((x, cur.Item2)) && data.maps.ContainsKey((x, i, cur.Item2)))
                                {
                                    visited.Add((x, cur.Item2));
                                    queue.Enqueue((x, cur.Item2));
                                }
                            }
                            for (int z = cur.Item2 - 1; z <= cur.Item2 + 1 && z < dir.Item2 + viewSize.z && z >= dir.Item2 - viewSize.z; z += 2)
                            {
                                if (!visited.Contains((cur.Item1, z)) && data.maps.ContainsKey((cur.Item1, i, z)))
                                {
                                    visited.Add((cur.Item1, z));
                                    queue.Enqueue((cur.Item1, z));
                                }
                            }

                        }

                    }

                }

            }

        }
        else
        {
            foreach (var curMap in curMapLst)
            {
                if (curMap.mapPos.y <= viewCenter.y)
                {
                    curMap.unit.VisOn();
                }
                else
                {
                    curMap.unit.VisOff();
                }
            }
        }


    }


    public Vector3 GetNavDir(Vector3 cur, Vector3 tar, int maxStep = 99999)
    {
        return navigationCtrl.GetNextDir(cur, tar, maxStep);
    }
    public void ResetInfo(TileUnit unit = null)
    {
        lastView = (0, 0, 0, 0, 0, 0);
        foreach (var map in curMapLst)
        {
            map.unit.Hide();
        }
        curMapLst.Clear();
        UpdateInfo(true);
    }
    public void UpdateInfo(bool forceFresh = false)
    {
        UpdateMapInfo();

        viewCenter = utilCtrl.RealPos2MapPos(curCenterPos);

        if (forceFresh || (curCenterPos - lastCenterPos).sqrMagnitude >= 1f)
        {

            if (GlobalSettings.MAP_SHOW_DEBUG)
            {
                Z_Log.Log("pos:" + curCenterPos + " to now cam Pos:" + viewCenter);
            }
            FreshMap();
            lastCenterPos = curCenterPos;
        }
        UpdateVision();
    }
    public void DebugShow()
    {
        foreach (var map in curMapLst)
        {
            if ((map.unit.ins.transform.GetChild(0).position - map.pos).sqrMagnitude > 0.2)
                Debug.Log((map.unit.ins.transform.position - map.pos).sqrMagnitude + "    ->  " + map.pos + " " + map.unit.ins.transform.position);
        }
    }

}

