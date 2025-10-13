using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using UnityEngine;
using Z_DataSystem.Form;
using Z_Debug;
using Z_DesignStyle;
using Z_Map;
using Z_Map.Analysis;
using Z_Map.Form;
using Z_Math;
using Z_Time;
using Z_UnitSystem;
using Z_UnitSystem.Form;
using static Z_DesignStyle.Z_DoubleDictionary;
using static Z_Math.Graph;

namespace Z_Map
{
    public static class GlobalSettings
    {
        public const int TEX_ANIM_MAX = 10;
        public const int ITEM_UNIT_MAX = 10;
        public const bool NAV_DEBUG = true;
        public const bool MAP_SHOW_DEBUG = false;
        public const bool OVERLAY_HIDE = true;
        public const bool UPDATE_TILE_ALWAYS = false;
    }

    public static class GlobalHelper
    {
        public static string GetInternalPrefabName(string name = "")
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
        public MapManager manager => MapManager.instance;
        public MapUnit(UnitForm.Data data) : base(data)
        {
        }
        public TileUnit belongTile
        {
            get
            {
                if (this is ObjectUnit obj)
                {
                    return MapManager.instance.updateCtrl.objectTileDic.Get(obj)[0];
                }
                else if (this is ItemUnit item)
                {
                    return MapManager.instance.updateCtrl.itemTileDic.Get(item)[0];
                }
                else if (this is CharacterUnit character)
                {
                    return MapManager.instance.updateCtrl.characterTileDic.Get(character)[0];
                }
                return null;
            }

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
        private float degree = 0;
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

            if (degree == this.degree)
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
    public MapUpdateController updateCtrl;




    public override void Init()
    {
        base.Init();

        navigationCtrl = new NavigationController(this);

        utilCtrl = new MapUtilController(this);
        updateCtrl = new MapUpdateController(this);
    }

    #region external

    public void Begin(MapData data)
    {
        End();
        Init();
        mainGo.SetActive(true);
        this.data = data;
        updateCtrl.Begin();



        navigationCtrl.Build();
        mainGo.SetActive(true);
    }
    #region unit
    public TileUnitForm.Data AddTile(Vector3Int mapPos, object[] prms = null)
    {
        return data.AddTile(mapPos, prms);
    }
    public ObjectUnitForm.Data AddObject(string name,Vector3 realPos, string prefabName, object[] prms = null)
    {
        var mapPos = utilCtrl.RealPos2MapPos(realPos);
        if (!this.data.maps.ContainsKey((mapPos.x, mapPos.y, mapPos.z)))
        {
            return null;
        }
        var data = this.data.AddObject(prefabName, prms);
        data.name = name;
        data.pos = realPos;
        foreach (var m in utilCtrl.GetOverlap(data))
        {
            updateCtrl.objectTileDic.Add(data.unit, m);
        }
        return data;
    }
    public ItemUnitForm.Data AddItem(string name,Vector3 realPos, string prefabName, object[] prms = null)
    {
        var mapPos = utilCtrl.RealPos2MapPos(realPos);
        if (!this.data.maps.ContainsKey((mapPos.x, mapPos.y, mapPos.z)))
        {
            return null;
        }
        var data = this.data.AddItem(prefabName, prms);
        data.name= name; ;
        data.pos = realPos;
        updateCtrl.itemTileDic.Add(data.unit, this.data.maps[(mapPos.x, mapPos.y, mapPos.z)].unit);
        return data;
    }
    public CharacterUnitForm.Data AddCharacter(string name,Vector3 realPos, string prefabName, bool isMine = false, object[] prms = null)
    {
        var mapPos = utilCtrl.RealPos2MapPos(realPos);
        if (!this.data.maps.ContainsKey((mapPos.x, mapPos.y, mapPos.z)))
        {
            return null;
        }
        var data = this.data.AddCharacter(prefabName, isMine, prms);
        data.pos = realPos;
        updateCtrl.characterTileDic.Add(data.unit, this.data.maps[(mapPos.x, mapPos.y, mapPos.z)].unit);
        data.name = name;
        return data;
    }
    public void RemoveTile(TileUnitForm.Data form)
    {
        data.RemoveTile(form);

        updateCtrl.characterTileDic.Del(form.unit);
        updateCtrl.itemTileDic.Del(form.unit);
        updateCtrl.objectTileDic.Del(form.unit);
    }
    public void RemoveCharacter(CharacterUnitForm.Data form)
    {
        data.RemoveCharacter(form);
        updateCtrl.characterTileDic.Del(form.unit);

    }
    public void RemoveItem(ItemUnitForm.Data form)
    {
        data.RemoveItem(form);
        updateCtrl.itemTileDic.Del(form.unit);

    }
    public void RemoveObject(ObjectUnitForm.Data form)
    {
        data.RemoveObject(form);
        updateCtrl.objectTileDic.Del(form.unit);
    }
    #endregion
    public void SetPos(Vector3 curCenterPos)
    {
        updateCtrl.curCenterPos = curCenterPos;
    }

    public void End()
    {
        updateCtrl.End();

        mainGo.SetActive(false);
        if (data != null)
        {
            data.Unload();
            data = null;
        }


    }


    #endregion



   

}

