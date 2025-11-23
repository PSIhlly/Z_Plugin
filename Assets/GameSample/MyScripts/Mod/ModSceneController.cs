using Form;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using Ui;
using Ui.ModSceneBehaviourUnit;
using Ui.ModSceneMain;
using Ui.ModSceneUnit;
using Ui.ModStory;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting.FullSerializer;
using UnityEditor;
using UnityEngine;
using Z_DataSystem;
using Z_Debug;
using Z_DesignStyle;
using Z_Input;
using Z_Map;
using Z_Map.Analysis;
using Z_Map.Form;
using Z_Time;
using Z_Ui;
using Z_UnitSystem;
using static UnityEditor.PlayerSettings;
public enum DesignType
{
    MapObject,
    Event
}
public interface InternalModSceneController
{
    public string fileName { get; }
    public void Begin(int id);
    public void End();
    public void Update();
    public void OnMouse(bool click, Vector3 pos, Vector3 dir);
}
public interface ExternalModSceneController
{
    public DesignType designType { get; set; }
    public int offset { get; set; }

    public int layer { get; set; }
    public int cntX { get; set; }
    public int cntY { get; set; }
    public int angle { get; set; }
    public float posX { get; set; }
    public float posZ { get; set; }
    public float posY { get; set; }
    public bool posing { get; set; }

    public MapBaseForm.Data curData { get; set; }

    public void SetCamera(float x, float y, float z);

    public void ForceUpdate();
}
public class ModSceneController : Z_Controller<ModManager>, InternalModSceneController, ExternalModSceneController, IZ_Listener<InputKeyEvent>, IZ_Listener<InputMouseEvent>, IZ_Listener<InputMouseDownEvent>, IZ_Listener<InputMouseUpEvent>, IZ_Listener<InputMouseScrollEvent>
{
    MapManager mapMgr => MapManager.instance;
    public ModSceneController(ModManager super) : base(super)
    {
        this.Register<InputKeyEvent>();
        this.Register<InputMouseEvent>();
        this.Register<InputMouseDownEvent>();
        this.Register<InputMouseUpEvent>();
        this.Register<InputMouseScrollEvent>();
    }

    bool enable = false;
    bool waitForActive = false;
    Vector2 downPos;
    #region internal Var
    private string _fileName;
    public string fileName { get => _fileName; }
    #endregion

    #region extern Var
    private DesignType _designType;
    private int _offset = 500;

    private int _layer = 0;
    private int _cntX = 1;
    private int _cntY = 1;
    private int _angle = 0;
    private float _posX = 0;
    private float _posZ = 0;
    private float _posY = 0;

    private bool _posing;
    private MapBaseForm.Data _curData;
    public DesignType designType { get => _designType; set => _designType = value; }

    public int offset { get => _offset; set => _offset = value; }


    public int layer { get => _layer; set => _layer = value; }
    public int cntX { get => _cntX; set => _cntX = value; }
    public int cntY { get => _cntY; set => _cntY = value; }
    public int angle { get => _angle; set => _angle = value; }
    public float posX { get => _posX; set => _posX = value; }
    public float posZ { get => _posZ; set => _posZ = value; }
    public float posY { get => _posY; set => _posY = value; }
    public bool posing { get => _posing; set => _posing = value; }
    public MapBaseForm.Data curData { get => _curData; set => _curData = value; }
    #endregion


    public void Begin(int id)
    {
        downPos = Vector2.zero;

        this._fileName = Main2StoryManager.GetSceneFileNameById(id);
        CameraInstance.instance.Register(Vector3.zero, Z_Math.Graph.ElementwiseMultiply(mapMgr.sizeLimit, mapMgr.data.mainData.mapUnitSize), 5, 15);
        CameraInstance.instance.tarTrs.position = Z_Math.Graph.ElementwiseMultiply(new Vector3(500, 500, 500), mapMgr.data.mainData.mapUnitSize);
        switch (DynamicGlobalSettings.cameraMode)
        {
            case CameraMode.Overhead:
                CameraInstance.instance.cam.transform.localPosition = new Vector3(0, 8, 0);
                CameraInstance.instance.cam.transform.eulerAngles = new Vector3(90, 0, 0);
                break;
            case CameraMode.Isometric:
                CameraInstance.instance.cam.transform.localPosition = new Vector3(0, 8, -8);
                CameraInstance.instance.cam.transform.eulerAngles = new Vector3(45, 0, 0);
                break;
        }

        enable = true;
        waitForActive = false;
        UiManager.instance.ShowUi<UiModSceneMainCtrl>();
    }
    public void End()
    {
        enable = false;
        mapMgr.End();
        UiManager.instance.CloseAll();
        GameManager.instance.RegisterInputDefault();
        UiManager.instance.ShowUi<UiModStoryCtrl>();

    }
    public T TryGetUnit<T>(List<RaycastHit> hits)
    {
        foreach (var hit in hits)
        {
            var tmp = hit.transform.parent.GetComponent<Instance>();
            if (tmp != null && tmp.unit.isVising)
            {
                return (T)(object)tmp.unit;
            }
        }
        return default(T);
    }
    public void OnMouse(bool click, Vector3 pos, Vector3 dir)
    {

        if (waitForActive || !enable)
            return;
        // set z
        pos.z = CameraInstance.instance.cam.nearClipPlane;
        // to world
        Ray ray = CameraInstance.instance.cam.ScreenPointToRay(pos);
        Vector3 worldPosition = CameraInstance.instance.cam.ScreenToWorldPoint(pos);
        /*;
        Debug.Log(worldPosition + "  " + CameraInstance.instance.cam.transform.up);
        worldPosition.y = CameraInstance.instance.tarTrs.position.y;
        var hits = new List<RaycastHit>(Physics.RaycastAll(worldPosition + CameraInstance.instance.cam.transform.up * 100, -CameraInstance.instance.cam.transform.up));
    */
        var hits = new List<RaycastHit>(Physics.RaycastAll(ray , 100));
        hits.Sort((a, b) =>
        {
            return a.distance.CompareTo(b.distance);
        });
        if(hits.Count>0)
        {
            worldPosition = new Vector3(hits[0].transform.position.x, CameraInstance.instance.tarTrs.position.y, hits[0].transform.position.z);
        }
        var hitPos = mapMgr.utilCtrl.RealPos2MapPos(worldPosition);
        //manage
        switch (designType)
        {
            case DesignType.MapObject:
                if (curData != null)
                {
                    if (curData is MapTerrainForm.Data terrainData)
                    {

                        for (int x = hitPos.x - cntX / 2; x < hitPos.x + cntX / 2 + (cntX % 2 == 1 ? 1 : 0); x++)
                            for (int z = hitPos.z - cntY / 2; z < hitPos.z + cntY / 2 + (cntY % 2 == 1 ? 1 : 0); z++)
                            {
                                if (!mapMgr.data.maps.ContainsKey((x, hitPos.y, z)))
                                {
                                    var newMapPos = new Vector3Int(x, hitPos.y, z);
                                    if (mapMgr.utilCtrl.InLimit(newMapPos))
                                    {
                                        mapMgr.AddTile(newMapPos);
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }
                                var mapData = mapMgr.data.maps[(x, hitPos.y, z)];

                                if (terrainData.step == 0)
                                {
                                    mapData.prefabName = GlobalNameHelper.GetInternalPrefabName(terrainData.prefabName);
                                    mapData.pos = Z_Math.Graph.ElementwiseMultiply(new Vector3(mapData.pos.x, mapData.mapPos.y, mapData.pos.z), mapMgr.data.mainData.mapUnitSize);

                                }
                                else
                                {
                                    for (int i = 0; i < terrainData.step; i++)
                                    {
                                        int stepX = mapData.mapPos.x;
                                        int stepZ = mapData.mapPos.z;
                                        switch (Z_Math.Graph.GetFourDirByEuler(angle))
                                        {
                                            case Z_Math.Graph.FourDir.Up:
                                                stepZ += i;
                                                break;
                                            case Z_Math.Graph.FourDir.Right:
                                                stepX += i;
                                                break;
                                            case Z_Math.Graph.FourDir.Down:
                                                stepZ -= i;
                                                break;
                                            case Z_Math.Graph.FourDir.Left:
                                                stepX -= i;
                                                break;
                                        }

                                        if (mapMgr.data.maps.ContainsKey((stepX, mapData.mapPos.y, stepZ)))
                                        {
                                            var cur = mapMgr.data.maps[(stepX, mapData.mapPos.y, stepZ)];
                                            cur.prefabName = GlobalNameHelper.GetInternalPrefabName(terrainData.prefabName);
                                            switch (Z_Math.Graph.GetFourDirByEuler(angle))
                                            {
                                                case Z_Math.Graph.FourDir.Up:
                                                    cur.euler = Vector3.zero;
                                                    break;
                                                case Z_Math.Graph.FourDir.Right:
                                                    cur.euler = new Vector3(0, 90, 0);
                                                    break;
                                                case Z_Math.Graph.FourDir.Down:
                                                    cur.euler = new Vector3(0, 180, 0);
                                                    break;
                                                case Z_Math.Graph.FourDir.Left:
                                                    cur.euler = new Vector3(0, 270, 0);
                                                    break;
                                            }
                                            cur.pos = new Vector3(cur.pos.x, cur.mapPos.y * 3f + 3f * (i + 0.5f) / terrainData.step, cur.pos.z);
                                        }
                                    }
                                }
                            }


                    }
                    else if (curData is MapTextureForm.Data textureData)
                    {
                        for (int x = hitPos.x - cntX / 2; x < hitPos.x + cntX / 2 + (cntX % 2 == 1 ? 1 : 0); x++)
                            for (int z = hitPos.z - cntY / 2; z < hitPos.z + cntY / 2 + (cntY % 2 == 1 ? 1 : 0); z++)
                            {
                                if (!mapMgr.data.maps.ContainsKey((x, hitPos.y, z)))
                                    continue;
                                var mapData = mapMgr.data.maps[(x, hitPos.y, z)];
                                mapData.texNameDic[layer] = textureData.name;

                            }
                    }
                    else if (curData is MapMaskForm.Data maskData)
                    {
                        for (int x = hitPos.x - cntX / 2; x < hitPos.x + cntX / 2 + (cntX % 2 == 1 ? 1 : 0); x++)
                            for (int z = hitPos.z - cntY / 2; z < hitPos.z + cntY / 2 + (cntY % 2 == 1 ? 1 : 0); z++)
                            {
                                if (!mapMgr.data.maps.ContainsKey((x, hitPos.y, z)))
                                    continue;

                                var mapData = mapMgr.data.maps[(x, hitPos.y, z)];
                                mapData.texNameDic[GlobalSettings.TERRAIN_LAYER_MAX + layer] = maskData.name;
                            }
                    }
                    else if (curData is MapObjectForm.Data objectData)
                    {
                        ForeachPos(hitPos, worldPosition, (mapData, finalPos) =>
                        {
                            bool allow = true;
                            var key = AssetManager.GetIdNameKey(objectData.id, objectData.name);
                            //放置去重
                            foreach (var cur in mapMgr.updateCtrl.objectTileDic.Get(mapData.unit))
                            {
                                var curData = cur.data;
                                if (curData.name == key && (curData.pos - finalPos).sqrMagnitude < 0.001f && Mathf.Abs(curData.euler.y - angle) < 1f)
                                {
                                    allow = false;
                                    break;
                                }
                            }
                            if (allow)
                            {
                                object[] prms = null;
                                var newObjectData = mapMgr.AddObject(key, finalPos, objectData.name, prms);
                                newObjectData.unit.evtDic = objectData.events;
                                newObjectData.euler = new Vector3(newObjectData.euler.x, angle, newObjectData.euler.z);
                            }

                        });

                    }
                    else if (curData is MapItemForm.Data itemData)
                    {
                        var data = ItemProductForm.DataByUid[itemData.itemUid];

                        var key = AssetManager.GetIdNameKey(data.uid, data.name);
                        ForeachPos(hitPos, worldPosition, (mapData, finalPos) =>
                        {
                            bool allow = true;

                            //放置去重
                            foreach (var cur in mapMgr.updateCtrl.itemTileDic.Get(mapData.unit))
                            {
                                var curData = cur.data;
                                if (curData.name == key && (curData.pos - finalPos).sqrMagnitude < 0.001f && Mathf.Abs(curData.euler.y - angle) < 1f)
                                {
                                    allow = false;
                                    break;
                                }
                            }
                            if (allow)
                            {
                                object[] prms = null;
                                var newItemData = mapMgr.AddItem(key, finalPos, data.name, prms);
                                newItemData.unit.productInfo = (data.uid, -1);
                                newItemData.unit.evtDic = data.events;
                                newItemData.euler = new Vector3(newItemData.euler.x, angle, newItemData.euler.z);
                            }
                        });
                    }
                    else if (curData is MapCharacterForm.Data characterData)
                    {
                        var data = CharacterProductForm.DataByUid[characterData.characterUid];

                        var key = AssetManager.GetIdNameKey(data.uid, data.name);
                        ForeachPos(hitPos, worldPosition, (mapData, finalPos) =>
                        {
                            bool allow = true;
                            //放置去重
                            foreach (var cur in mapMgr.updateCtrl.characterTileDic.Get(mapData.unit))
                            {
                                var curData = cur.data;
                                if (curData.name == key && (curData.pos - finalPos).sqrMagnitude < 0.001f && Mathf.Abs(curData.euler.y - angle) < 1f)
                                {
                                    allow = false;
                                    break;
                                }
                            }
                            if (allow)
                            {
                                object[] prms = null;
                                var newCharacerData = mapMgr.AddCharacter(key, finalPos, GlobalNameHelper.GetRuntimePrefabName("character"), false);
                                newCharacerData.unit.productInfo = (data.uid, -1);
                                newCharacerData.unit.evtDic = data.events;
                                newCharacerData.euler = new Vector3(newCharacerData.euler.x, angle, newCharacerData.euler.z);
                            }
                        });
                    }
                    else if (curData is MapEraseForm.Data eraseData)
                    {
                        ForeachPos(hitPos, worldPosition, (mapData, finalPos) =>
                        {
                            if (eraseData.terrain)
                            {
                                mapMgr.RemoveTile(mapData);
                            }
                            else if (eraseData.item)
                            {
                                foreach (var sub in mapData.unit.GetAllSubUnits())
                                {
                                    if (sub is ItemUnit item)
                                    {
                                        mapMgr.RemoveItem(item.data);
                                    }
                                }
                            }
                            else if (eraseData.mObject)
                            {
                                foreach (var sub in mapData.unit.GetAllSubUnits())
                                {
                                    if (sub is ObjectUnit item)
                                    {
                                        mapMgr.RemoveObject(item.data);
                                    }
                                }
                            }

                            if (eraseData.texture)
                            {
                                if (mapData.texNameDic.ContainsKey(layer))
                                    mapData.texNameDic.Remove(layer);
                            }
                        });
                    }
                    ForceUpdate();
                }
                else
                {
                    //click
                    if (click)
                    {
                        foreach (var hit in hits)
                        {
                            
                            var ins = hit.transform.GetComponentInParent<MapInstance>();
                            if (ins != null && (ins is ObjectInstance || ins is ItemInstance || ins is CharacterInstance))
                            {
                                UiManager.instance.ShowUi<UiModSceneUnitCtrl>(new UiModSceneUnitParam()
                                {
                                    data = ins.unit.data
                                });
                                break;
                            }
                        }


                    }//move
                    else
                    {
                        Vector2 moveDir = -Time.deltaTime * dir * 4;
                        CameraInstance.instance.tarTrs.position += new Vector3(moveDir.x, 0, moveDir.y);
                        Z_EventHelper.Invoke(new CameraMoveEvent());
                    }

                }
                break;
            case DesignType.Event:
                //click
                if (click)
                {
                    foreach (var hit in hits)
                    {
                        var ins = hit.transform.GetComponentInParent<Instance>();
                        if (ins != null && ins is MapInstance mapIns)
                        {
                            UiManager.instance.ShowUi<UiModSceneBehaviourUnitCtrl>(new UiModSceneBehaviourUnitParam()
                            {
                                data = mapIns.unit.data
                            });
                            break;
                        }
                    }
                }//move
                else
                {
                    Vector2 moveDir = -Time.deltaTime * dir * 4;
                    CameraInstance.instance.tarTrs.position += new Vector3(moveDir.x, 0, moveDir.y);
                    Z_EventHelper.Invoke(new CameraMoveEvent());
                }
                break;

        }

    }
    private void ForeachPos(Vector3Int mapCenterPos, Vector3 realPos, Action<TileUnitForm.Data, Vector3> onFind)
    {
        for (int x = mapCenterPos.x - cntX / 2; x < mapCenterPos.x + cntX / 2 + (cntX % 2 == 1 ? 1 : 0); x++)
            for (int z = mapCenterPos.z - cntY / 2; z < mapCenterPos.z + cntY / 2 + (cntY % 2 == 1 ? 1 : 0); z++)
            {
                var finalX = posX + x;
                var finalZ = posZ + z;
                var finalY = posY + realPos.y;
                var finalPos = new Vector3(finalX, finalY, finalZ);
                var mapPos = mapMgr.utilCtrl.RealPos2MapPos(finalPos);
                if (mapMgr.data.maps.ContainsKey((mapPos.x, mapPos.y, mapPos.z)))
                {
                    onFind?.Invoke(mapMgr.data.maps[(mapPos.x, mapPos.y, mapPos.z)], finalPos);
                }
            }
    }
    public void ForceUpdate()
    {

        mapMgr.updateCtrl.ResetInfo();
        if (waitForActive)
            return;

        waitForActive = true;
        TimeManager.instance.AddNextBigFrameAction(() =>
        {
            waitForActive = false;
        }, _super.gameObject);
    }
    public void Update()
    {
        if (!enable)
            return;

        {
            mapMgr.updateCtrl.UpdateInfo();
            {
                mapMgr.SetPos(CameraInstance.instance.tarTrs.position);
            }
        }


    }
    public void SetCamera(float x, float y, float z)
    {
        CameraInstance.instance.tarTrs.position = new Vector3(x, y, z);
        Z_EventHelper.Invoke(new CameraMoveEvent());
    }
    #region op

    public void OnEvent(InputKeyEvent evt)
    {
        if (!enable)
            return;
        switch (evt.key)
        {
            case KeyCode.W:
                CameraInstance.instance.tarTrs.position += Time.deltaTime * Vector3.forward * 4;
                break;
            case KeyCode.S:
                CameraInstance.instance.tarTrs.position += Time.deltaTime * Vector3.back * 4;
                break;

            case KeyCode.A:
                CameraInstance.instance.tarTrs.position += Time.deltaTime * Vector3.left * 4;
                break;
            case KeyCode.D:
                CameraInstance.instance.tarTrs.position += Time.deltaTime * Vector3.right * 4;
                break;
        }
    }

    public void OnEvent(InputMouseEvent evt)
    {
        if (!enable)
            return;
        if (evt.id == 0 && evt.ui == null && downPos != Vector2.zero)//&& (downPos - new Vector2(pos.x, pos.y)).sqrMagnitude > dragDis2
        {
            ModManager.instance.OnMouse(false, evt.pos, evt.delta);
        }
    }

    public void OnEvent(InputMouseDownEvent evt)
    {
        if (!enable)
            return;
        if (evt.ui == null)
        {
            downPos = evt.pos;
        }
        else
        {
            downPos = Vector2.zero;
        }
    }

    public void OnEvent(InputMouseUpEvent evt)
    {
        if (!enable)
            return;
        if (evt.id == 0 && evt.ui == null && downPos != Vector2.zero && (downPos - new Vector2(evt.pos.x, evt.pos.y)).sqrMagnitude < GlobalSettings.DRAG_DIS2)
        {
            OnMouse(true, evt.pos, Vector3.zero);
        }
        downPos = Vector2.zero;
    }

    public void OnEvent(InputMouseScrollEvent evt)
    {
        if (!enable)
            return;

        CameraInstance.instance.cam.orthographicSize += evt.delta * -2f;
    }
    #endregion

}
