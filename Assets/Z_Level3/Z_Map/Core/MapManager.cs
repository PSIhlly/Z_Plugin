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
    public enum CameraMode
    {
        Overhead,
        Isometric,
    }

    public static class GlobalSettings
    {
        public const int TEX_ANIM_MAX = 10;
        public const int ITEM_UNIT_MAX = 10;
        public const bool NAV_DEBUG = true;
        public const bool MAP_SHOW_DEBUG = false;
        public const bool OVERLAY_HIDE = false;
        public const bool UPDATE_TILE_ALWAYS = false;
        public const bool UPDATE_ALL_CHARACTER = true;
        public const bool UPDATE_ALL_OBJECT = true;
        public const bool ENABLE_GRAVITY = true;
    }
    public static class DynamicGlobalSettings
    {
        public static CameraMode cameraMode;
        public static bool playing;
        public static bool pauseNav;
    }


    public enum MapEventType
    {
        Create,
        Show,
        AfterUpdate,
        Move,
        BoundaryTouch,
        Hide,
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





}

public class MapManager : Z_MonoManager<MapManager>
{

    public Vector3 sizeLimit = new Vector3(1000, 1000, 1000);
    public MapInfo data;
    public GameObject mainGo;


    public NavigationController navigationCtrl;
    public MapUtilController utilCtrl;
    public MapUpdateController updateCtrl;

    public bool enable;


    public override void Init()
    {
        base.Init();

        navigationCtrl = new NavigationController(this);

        utilCtrl = new MapUtilController(this);
        updateCtrl = new MapUpdateController(this);

    }

    #region external

    public void Begin(MapInfo data)
    {
        End();
        Init();
        mainGo.SetActive(true);
        this.data = data;
        enable = true;
        updateCtrl.Begin();

        navigationCtrl.Build();
        mainGo.SetActive(true);

        //try invoke create
        foreach (var tData in TileUnitForm.DataByUid.Values)
        {
            tData.unit.Create();
        }
        foreach (var iData in ItemUnitForm.DataByUid.Values)
        {
            iData.unit.Create();
        }
        foreach (var oData in ObjectUnitForm.DataByUid.Values)
        {
            oData.unit.Create();
        }
        foreach (var cData in CharacterUnitForm.DataByUid.Values)
        {
            cData.unit.Create();
        }
    }
    #region unit
    public TileUnitForm.Data AddTile(Vector3Int mapPos, object[] prms = null)
    {
        var tData = data.AddTile(mapPos, prms);
        tData.unit.Create();
        return tData;
    }
    public ObjectUnitForm.Data AddObject(string name, Vector3 realPos, string prefabName, object[] prms = null)
    {
        var mapPos = utilCtrl.RealPos2MapPosInt(realPos);

        var oData = this.data.AddObject(prefabName, prms);
        if (oData==null)
        {
            return null;
        }
        oData.name = name;
        oData.pos = realPos;
        foreach (var m in utilCtrl.GetOverlap(oData))
        {
            updateCtrl.objectTileDic.Add(oData.unit, m);

        }
        oData.unit.Create();
        updateCtrl.UpdateSingleOne(oData.unit);
        return oData;
    }
    public ItemUnitForm.Data AddItem(string name, Vector3 realPos, string prefabName, object[] prms = null)
    {
        var mapPos = utilCtrl.RealPos2MapPosInt(realPos);

        var iData = this.data.AddItem(prefabName, prms);
        if (iData == null)
        {
            return null;
        }
        iData.name = name; ;
        iData.pos = realPos;
        updateCtrl.itemTileDic.Add(iData.unit, this.data.maps[(mapPos.x, mapPos.y, mapPos.z)].unit);
        iData.unit.Create();
        updateCtrl.UpdateSingleOne(iData.unit);
        return iData;
    }
    public CharacterUnitForm.Data AddCharacter(string name, Vector3 realPos, string prefabName, bool isMine = false, string extra = "")
    {
        var mapPos = utilCtrl.RealPos2MapPosInt(realPos);

        var cData = this.data.AddCharacter(prefabName, isMine, extra);
        if (cData == null)
        {
            return null;
        }
        cData.pos = realPos;
        updateCtrl.characterTileDic.Add(cData.unit, this.data.maps[(mapPos.x, mapPos.y, mapPos.z)].unit);
        cData.name = name;
        cData.unit.Create();
        updateCtrl.UpdateSingleOne(cData.unit);
        return cData;
    }
    public void RemoveTile(TileUnitForm.Data form)
    {
        data.RemoveTile(form);

        updateCtrl.characterTileDic.Del(form.unit);
        updateCtrl.itemTileDic.Del(form.unit);
        updateCtrl.objectTileDic.Del(form.unit);
        updateCtrl.curTileLst.Remove(form);
    }
    public void RemoveCharacter(CharacterUnitForm.Data form)
    {
        data.RemoveCharacter(form);
        updateCtrl.characterTileDic.Del(form.unit);
        updateCtrl.curCharacterLst.Remove(form);

    }
    public void RemoveItem(ItemUnitForm.Data form)
    {
        data.RemoveItem(form);
        updateCtrl.itemTileDic.Del(form.unit);
        updateCtrl.curItemLst.Remove(form);

    }
    public void RemoveObject(ObjectUnitForm.Data form)
    {
        data.RemoveObject(form);
        updateCtrl.objectTileDic.Del(form.unit);
        updateCtrl.curObjectLst.Remove(form);
    }
    #endregion
    public void SetPos(Vector3 curCenterPos)
    {
        updateCtrl.curCenterPos = curCenterPos;
    }
    public void ClearEnteredScene()
    {
        foreach (var tData in TileUnitForm.DataByUid.Values)
        {
            tData.enteredScene = false;
        }
        foreach (var iData in ItemUnitForm.DataByUid.Values)
        {
            iData.enteredScene = false;
        }
        foreach (var oData in ObjectUnitForm.DataByUid.Values)
        {
            oData.enteredScene = false;
        }
        foreach (var cData in CharacterUnitForm.DataByUid.Values)
        {
            cData.enteredScene = false;
        }
    }
    public void End()
    {
        updateCtrl.End();

        enable = false;
        mainGo.SetActive(false);
        if (data != null)
        {
            data.Unload();
            data = null;
        }


    }


    #endregion






}

