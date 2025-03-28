using Form;
using Microsoft.Win32;
using System.Collections;
using System.Collections.Generic;
using Ui;
using Ui.ModSceneUnit;
using Ui.ModStory;
using UnityEditor;
using UnityEngine;
using Z_Debug;
using Z_DesignStyle;
using Z_Input;
using Z_Map;
using Z_Map.Form;
using Z_Time;
using Z_Ui;
using Z_UnitSystem;

public interface InternalModSceneController
{
    public string fileName { get;  }
    public void Begin(MapData dataCtrl, string fileName);
    public void End();
    public void Update();
    public void OnMouse(bool click, Vector3 pos, Vector3 dir);
}
public interface ExternalModSceneController
{
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
public class ModSceneController : Z_Controller<ModManager>, InternalModSceneController, ExternalModSceneController
{
    public ModSceneController(ModManager super) :base(super)
    {

    }

    bool enable = false;
    bool waitForActive = false;
    #region internal Var
    private string _fileName;
    public string fileName { get => _fileName; }
    #endregion

    #region extern Var

    private int _offset = 500;

    private int _layer = 0;
    private int _cntX=1;
    private int _cntY=1;
    private int _angle=0;
    private float _posX=0;
    private float _posZ=0;
    private float _posY=0;

    private bool _posing;
    private MapBaseForm.Data _curData;

    public int offset { get => _offset; set => _offset = value; }
   

    public int layer { get => _layer; set => _layer = value; }
    public int cntX { get => _cntX; set => _cntX=value; }
    public int cntY { get => _cntY; set => _cntY = value; }
    public int angle { get => _angle; set => _angle = value; }
    public float posX { get => _posX; set => _posX = value; }
    public float posZ { get => _posZ; set => _posZ = value; }
    public float posY { get => _posY; set => _posY = value; }
    public bool posing { get => _posing; set => _posing = value; }
    public MapBaseForm.Data curData { get => _curData; set => _curData = value; }
    #endregion


    public void Begin(MapData dataCtrl,string fileName)
    {
        this._fileName = fileName;
        GameManager.instance.RegisterInputByUgc();
        MapManager.instance.Begin(dataCtrl);
        CameraInstance.instance.Register(Vector3.zero,Z_Math.Graph.ElementwiseMultiply(MapManager.instance.sizeLimit, MapManager.instance.data.mainData.mapUnitSize),5,15);
        CameraInstance.instance.tarTrs.position = Z_Math.Graph.ElementwiseMultiply(new Vector3(500, 500, 500),MapManager.instance.data.mainData.mapUnitSize);
        enable = true;
        waitForActive = false;
    }
    public void End()
    {
        enable = false;
        MapManager.instance.End();
        UiManager.instance.CloseAll();
        GameManager.instance.RegisterInputDefault();
        UiManager.instance.ShowUi<UiModStoryCtrl>();

    }
    public T TryGetUnit<T>(RaycastHit[] hits)
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
    public void OnMouse(bool click,Vector3 pos,Vector3 dir)
    {

        if (waitForActive||!enable)
            return;
        // set z
        pos.z = CameraInstance.instance.cam.nearClipPlane;
        // to world
        Vector3 worldPosition = CameraInstance.instance.cam.ScreenToWorldPoint(pos);
        worldPosition.y = CameraInstance.instance.tarTrs.position.y;
        var hits = Physics.RaycastAll(worldPosition + Vector3.up * 100, Vector3.down);
        var hitPos = MapManager.instance.utilCtrl.RealPos2MapPos(worldPosition);
        //manage
        if (curData!=null)
        {
            if(curData is MapTerrainForm.Data terrainData)
            {

                for (int x= hitPos.x-cntX / 2;x< hitPos.x+cntX/2+(cntX%2==1?1:0); x++)
                for (int z= hitPos.z-cntY / 2;z< hitPos.z+cntY/2+(cntY%2==1?1:0); z++)
                    {
                        if(!MapManager.instance.data.maps.ContainsKey((x, hitPos.y, z)))
                        {
                            var newMapPos = new Vector3Int(x, hitPos.y, z);
                            if (MapManager.instance.utilCtrl.InLimit(newMapPos))
                            {
                                MapManager.instance.AddMap(newMapPos);
                            }
                            else
                            {
                                continue;
                            }
                        }
                        var mapData = MapManager.instance.data.maps[(x, hitPos.y, z)];

                        if (terrainData.step == 0)
                        {
                            mapData.prefabName = GlobalNameHelper.GetInternalPrefabName(terrainData.prefabName);
                            mapData.pos = Z_Math.Graph.ElementwiseMultiply(new Vector3(mapData.pos.x, mapData.mapPos.y, mapData.pos.z), MapManager.instance.data.mainData.mapUnitSize);

                        }
                        else
                        {
                            for (int i = 0; i < terrainData.step; i++)
                            {
                                int stepX = mapData.mapPos.x;
                                int stepZ = mapData.mapPos.z;
                                switch(Z_Math.Graph.GetFourDirByEuler(angle))
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

                                if (MapManager.instance.data.maps.ContainsKey((stepX, mapData.mapPos.y, stepZ)))
                                {
                                    var cur = MapManager.instance.data.maps[(stepX, mapData.mapPos.y, stepZ)];
                                    cur.prefabName = GlobalNameHelper.GetInternalPrefabName(terrainData.prefabName);
                                    switch (Z_Math.Graph.GetFourDirByEuler(angle))
                                    {
                                        case Z_Math.Graph.FourDir.Up:
                                            cur.euler = Vector3.zero;
                                            break;
                                        case Z_Math.Graph.FourDir.Right:
                                            cur.euler = new Vector3(0,90,0);
                                            break;
                                        case Z_Math.Graph.FourDir.Down:
                                            cur.euler = new Vector3(0,180, 0);
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
            else if(curData is MapTextureForm.Data textureData)
            {
                for (int x = hitPos.x - cntX / 2; x < hitPos.x + cntX / 2 + (cntX % 2 == 1 ? 1 : 0); x++)
                    for (int z = hitPos.z - cntY / 2; z < hitPos.z + cntY / 2 + (cntY % 2 == 1 ? 1 : 0); z++)
                    {
                        if (!MapManager.instance.data.maps.ContainsKey((x, hitPos.y, z)))
                            continue;
                        var mapData = MapManager.instance.data.maps[(x, hitPos.y, z)];
                        mapData.texNameDic[layer] = textureData.name;
                        mapData.animInterval[layer] = textureData.animTimeInterval==0?0:Mathf.Max((int)(textureData.animTimeInterval*Application.targetFrameRate),1);
                        
                    }
            }
            else if (curData is MapMaskForm.Data maskData)
            {
                for (int x = hitPos.x - cntX / 2; x < hitPos.x + cntX / 2 + (cntX % 2 == 1 ? 1 : 0); x++)
                    for (int z = hitPos.z - cntY / 2; z < hitPos.z + cntY / 2 + (cntY % 2 == 1 ? 1 : 0); z++)
                    {
                        if (!MapManager.instance.data.maps.ContainsKey((x, hitPos.y, z)))
                            continue;

                        var mapData = MapManager.instance.data.maps[(x, hitPos.y, z)];
                        mapData.alphaTexNameDic[layer] = maskData.name;
                    }
            }
            else if(curData is MapObjectForm.Data itemData)
            {
                for (int x = hitPos.x - cntX / 2; x < hitPos.x + cntX / 2 + (cntX % 2 == 1 ? 1 : 0); x++)
                    for (int z = hitPos.z - cntY / 2; z < hitPos.z + cntY / 2 + (cntY % 2 == 1 ? 1 : 0); z++)
                    {
                        var finalX = posX + x;
                        var finalZ = posZ + z;
                        var finalY = posY + worldPosition.y;
                        var finalPos = new Vector3(finalX, finalY, finalZ);
                        var mapPos = MapManager.instance.utilCtrl.RealPos2MapPos(finalPos);
                        if (MapManager.instance.data.maps.ContainsKey((mapPos.x, mapPos.y, mapPos.z)))
                        {
                            var mapData = MapManager.instance.data.maps[(mapPos.x, mapPos.y, mapPos.z)];
                            bool allow = true;
                            //放置去重
                            foreach (var subUnit in mapData.unit.subUnits)
                            {
                                if (subUnit.data is ObjectUnitForm.Data otherItemData)
                                {
                                    if (otherItemData.prefabName == itemData.name&& (otherItemData.pos- finalPos).sqrMagnitude<0.001f&&Mathf.Abs(otherItemData.euler.y - angle)<1f)
                                    {
                                        allow = false;
                                        break;
                                    }
                                }
                            }
                            if(allow)
                            {
                                var newItemData = MapManager.instance.AddItem(finalPos);
                                newItemData.isObstacle = true;
                                newItemData.euler = new Vector3(newItemData.euler.x, angle, newItemData.euler.z);
                                newItemData.prefabName = itemData.name;
                                newItemData.name = itemData.name;
                            }
                        }
                    }
            }
            else if (curData is MapEraseForm.Data eraseData)
            {
                for (int x = hitPos.x - cntX / 2; x < hitPos.x + cntX / 2 + (cntX % 2 == 1 ? 1 : 0); x++)
                    for (int z = hitPos.z - cntY / 2; z < hitPos.z + cntY / 2 + (cntY % 2 == 1 ? 1 : 0); z++)
                    {
                        if (!MapManager.instance.data.maps.ContainsKey((x, hitPos.y, z)))
                            continue;
                        var mapData = MapManager.instance.data.maps[(x, hitPos.y, z)];
                        if (eraseData.terrain)
                        {
                            mapData.unit.Remove();
                        }else if(eraseData.item)
                        {
                            foreach(var sub in mapData.unit.GetAllSubUnits())
                            {
                                if(sub is ObjectUnit item)
                                {
                                    sub.Remove();
                                }
                            }
                        }

                        if(eraseData.texture)
                        {
                            if (mapData.texNameDic.ContainsKey(layer))
                                mapData.texNameDic.Remove(layer);
                            if (mapData.animInterval.ContainsKey(layer))
                                mapData.animInterval[layer] = 0;
                            if (mapData.alphaTexNameDic.ContainsKey(layer))
                                mapData.alphaTexNameDic.Remove(layer);
                        }
                    }
            }
            ForceUpdate();
        }
        else
        {
            //click
            if (click)
            {
                foreach(var hit in hits)
                {
                    var ins = hit.transform.parent.GetComponent<Instance>();
                    if(ins!=null&&ins is ItemInstance itemIns)
                    {
                        UiManager.instance.ShowUi<UiModSceneUnitCtrl>(new UiModSceneUnitParam()
                        {
                            data = itemIns.unit.data
                        }) ;
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
    }
    public void ForceUpdate()
    {
        
        MapManager.instance.ResetInfo();
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
            MapManager.instance.UpdateInfo();
            {
                MapManager.instance.SetPos(CameraInstance.instance.tarTrs.position);
            }
        }
        
       
    }
    public void SetCamera(float x, float y, float z)
    {
        CameraInstance.instance.tarTrs.position = new Vector3(x, y, z);
        Z_EventHelper.Invoke(new CameraMoveEvent());
    }


}
