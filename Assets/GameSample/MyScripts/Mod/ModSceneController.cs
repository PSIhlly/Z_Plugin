using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using Ui.ModSceneBehaviourUnit;
using Ui.ModSceneMain;
using Ui.ModSceneUnit;
using Ui.ModStory;
using UnityEngine;
using Z_DataSystem;
using Z_DesignStyle;
using Z_Input;
using Z_Map;
using Z_Map.Form;
using Z_Time;
using Z_Ui;
using Z_Ui.Notify;
using Z_UnitSystem;
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

    public int layer { get; set; }
    public int cntX { get; set; }
    public int cntY { get; set; }
    public int angle { get; set; }
    public float posX { get; set; }
    public float posZ { get; set; }
    public float posY { get; set; }
    public bool posing { get; set; }

    public MapBaseForm.Data curData { get; set; }

    public int tileLayerDisplayMode { get; set; }

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



    public int layer { get => _layer; set => _layer = value; }
    public int cntX { get => _cntX; set => _cntX = value; }
    public int cntY { get => _cntY; set => _cntY = value; }
    public int angle { get => _angle; set => _angle = value; }
    public float posX { get => _posX; set => _posX = value; }
    public float posZ { get => _posZ; set => _posZ = value; }
    public float posY { get => _posY; set => _posY = value; }
    public bool posing { get => _posing; set => _posing = value; }
    public MapBaseForm.Data curData { get => _curData; set => _curData = value; }

    private int _tileLayerDisplayMode = int.MaxValue;
    public int tileLayerDisplayMode
    {
        get => _tileLayerDisplayMode;
        set
        {
            if (_tileLayerDisplayMode == value) return;
            _tileLayerDisplayMode = value;
            RefreshTileLayerDisplay();
        }
    }
    #endregion


    public void Begin(int id)
    {
        downPos = Vector2.zero;
        _cntX = 1;
        _cntY = 1;

        this._fileName = Main2StoryManager.GetSceneFileNameById(id);
        _tileLayerDisplayMode = int.MaxValue;
        CameraInstance.instance.Register(Vector3.zero, Z_Math.Graph.ElementwiseMultiply(mapMgr.sizeLimit, mapMgr.data.mainData.mapUnitSize), 4, 6);
        CameraInstance.instance.tarTrs.position = MapManager.instance.utilCtrl.MapPos2RealPos(GameManager.PlayerPosToMapPos(Vector3.zero));
        switch (DynamicGlobalSettings.cameraMode)
        {
            case CameraMode.Overhead:
                CameraInstance.instance.cam.transform.localPosition = new Vector3(0, 8, 0);
                CameraInstance.instance.cam.transform.eulerAngles = new Vector3(90, 0, 0);
                CameraInstance.instance.globalLight.transform.eulerAngles = Vector3.right * 80;
                CameraInstance.instance.globalLight.intensity = 1f;
                break;
            case CameraMode.Isometric:
                CameraInstance.instance.cam.transform.localPosition = new Vector3(0, 8, -8);
                CameraInstance.instance.cam.transform.eulerAngles = new Vector3(45, 0, 0);
                CameraInstance.instance.globalLight.transform.eulerAngles = Vector3.right * 70;
                CameraInstance.instance.globalLight.intensity = 1.3f;
                break;
        }

        enable = true;
        waitForActive = false;
        UiManager.instance.ShowUi<UiModSceneMainCtrl>();
    }
    public void End()
    {
        enable = false;
        NotifyManager.instance.ClearAll();
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
        // 通过射线与地平面求交，确保不同相机视角下位置计算正确
        Ray ray = CameraInstance.instance.cam.ScreenPointToRay(pos);
        // 只检测trigger的collider，避免非trigger的物理碰撞体干扰射线检测
        var allHits = Physics.RaycastAll(ray, 100, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide);
        var hits = new List<RaycastHit>(allHits.Length);
        foreach (var hit in allHits)
        {
            if (hit.collider != null && hit.collider.isTrigger)
            {
                hits.Add(hit);
            }
        }
        hits.Sort((a, b) =>
        {
            return a.distance.CompareTo(b.distance);
        });
        Vector3 worldPosition;
        if (curData != null)
        {
            // 放置模式下优先找 TileInstance（地面），避免已放置物体的碰撞体干扰射线检测
            TileInstance tileHit = null;
            RaycastHit? tileRayHit = null;
            foreach (var hit in hits)
            {
                tileHit = hit.transform.GetComponentInParent<TileInstance>();
                if (tileHit != null)
                {
                    tileRayHit = hit;
                    break;
                }
            }
            // if (tileHit != null && tileRayHit != null)
            // {
            //     worldPosition = tileRayHit.Value.point;
            //     worldPosition.y += posY;
            // }
            // else
            {
                Plane groundPlane = new Plane(Vector3.up, -CameraInstance.instance.tarTrs.position.y);
                if (groundPlane.Raycast(ray, out float enter))
                {
                    worldPosition = ray.GetPoint(enter);
                }
                else
                {
                    worldPosition = ray.GetPoint(10);
                }
            }
        }
        else
        {
            if (hits.Count > 0)
            {
                worldPosition = hits[0].point;
                worldPosition.y = CameraInstance.instance.tarTrs.position.y;
            }
            else
            {
                Plane groundPlane = new Plane(Vector3.up, -CameraInstance.instance.tarTrs.position.y);
                if (groundPlane.Raycast(ray, out float enter))
                {
                    worldPosition = ray.GetPoint(enter);
                }
                else
                {
                    worldPosition = ray.GetPoint(10);
                    worldPosition.y = CameraInstance.instance.tarTrs.position.y;
                }
            }
        }
        var hitPos = mapMgr.utilCtrl.RealPos2MapPosInt(worldPosition);
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
                                    _super.assetCtrl.AddTile(newMapPos);
                                }
                                if (!mapMgr.data.maps.ContainsKey((x, hitPos.y, z)))
                                    continue;
                                var mapData = mapMgr.data.maps[(x, hitPos.y, z)];

                                if (terrainData.step == 0)
                                {
                                    mapData.prefabName = GameManager.instance.innerAssetDic[terrainData.innerPrefabName].name;
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

                                        if (!mapMgr.data.maps.ContainsKey((stepX, mapData.mapPos.y, stepZ)))
                                        {
                                            var newMapPos = new Vector3Int(stepX, mapData.mapPos.y, stepZ);
                                            _super.assetCtrl.AddTile(newMapPos);
                                        }
                                        if (!mapMgr.data.maps.ContainsKey((stepX, mapData.mapPos.y, stepZ)))
                                            continue;
                                        var cur = mapMgr.data.maps[(stepX, mapData.mapPos.y, stepZ)];
                                        cur.prefabName = GameManager.instance.innerAssetDic[terrainData.innerPrefabName].name;
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
                                        cur.pos = new Vector3(cur.pos.x, cur.mapPos.y * MapManager.instance.data.mainData.mapUnitSize.y + MapManager.instance.data.mainData.mapUnitSize.y * (i) / terrainData.step, cur.pos.z);


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
                                mapData.texDic[layer] = textureData.id;
                                mapMgr.updateCtrl.UpdateSingleOne(mapData.unit);
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
                                mapData.texDic[GlobalSettings.TERRAIN_LAYER_MAX + layer] = maskData.id;
                                mapMgr.updateCtrl.UpdateSingleOne(mapData.unit);
                            }
                    }
                    else if (curData is MapObjectForm.Data objectData)
                    {
                        ForeachPos(hitPos, worldPosition, (_, finalPos) =>
                        {
                            _super.assetCtrl.AddObject(objectData, finalPos, angle);
                        });

                    }
                    else if (curData is MapItemForm.Data itemData)
                    {
                        var data = ItemProductForm.DataByUid[itemData.itemUid];

                        ForeachPos(hitPos, worldPosition, (_, finalPos) =>
                        {
                            _super.assetCtrl.AddItem(data, finalPos, angle);
                        });
                    }
                    else if (curData is MapCharacterForm.Data characterData)
                    {
                        var data = CharacterProductForm.DataByUid[characterData.characterUid];

                        ForeachPos(hitPos, worldPosition, (_, finalPos) =>
                        {
                            _super.assetCtrl.AddCharacter(data, finalPos, angle);
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
                                if (mapData.texDic.ContainsKey(layer))
                                    mapData.texDic.Remove(layer);
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
                        var clampedDir = new Vector2(Mathf.Clamp(dir.x, -50f, 50f), Mathf.Clamp(dir.y, -50f, 50f));
                        Vector2 moveDir = -clampedDir * 0.04f;
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
                    var clampedDir = new Vector2(Mathf.Clamp(dir.x, -50f, 50f), Mathf.Clamp(dir.y, -50f, 50f));
                    Vector2 moveDir = -clampedDir * 0.04f;
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
                var mapPos = mapMgr.utilCtrl.RealPos2MapPosInt(finalPos);
                if (mapMgr.data.maps.ContainsKey((mapPos.x, mapPos.y, mapPos.z)))
                {
                    onFind?.Invoke(mapMgr.data.maps[(mapPos.x, mapPos.y, mapPos.z)], finalPos);
                }
            }
    }
    public void ForceUpdate()
    {

        mapMgr.updateCtrl.ResetView();
        if (waitForActive)
            return;

        waitForActive = true;
        TimeManager.instance.AddNextBigFrameAction(() =>
        {
            waitForActive = false;
            RefreshTileLayerDisplay();
        }, _super.gameObject);
    }

    /// <summary>
    /// 刷新所有 Tile 的显示层级
    /// </summary>
    private void RefreshTileLayerDisplay()
    {
        foreach (var mapData in mapMgr.data.maps.Values)
        {
            if (mapData.unit.ins != null)
            {
                mapData.unit.ins.displayLayer = _tileLayerDisplayMode;
                if (mapData.unit.ins.vising)
                {
                    // 重新应用 VisOn 以刷新各层 Renderer 显隐
                    mapData.unit.ins.vising = false;
                    mapData.unit.ins.VisOn();
                }
            }
        }
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
        var cur = evt.key.Where((o) => o == KeyCode.W || o == KeyCode.S || o == KeyCode.A || o == KeyCode.D);
        var step = Vector3.zero;
        foreach (var c in cur)
        {
            switch (c)
            {
                case KeyCode.W:
                    step += Vector3.forward;
                    break;
                case KeyCode.S:
                    step += Vector3.back;
                    break;

                case KeyCode.A:
                    step += Vector3.left;
                    break;
                case KeyCode.D:
                    step += Vector3.right;
                    break;
            }
        }
        CameraInstance.instance.tarTrs.position += step.normalized * 0.02f;
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
