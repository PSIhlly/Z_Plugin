using Form;
using Microsoft.Win32;
using System.Collections;
using System.Collections.Generic;
using Ui;
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
public class CameraMoveEvent:Z_Event
{

}
public class ModSceneManager : Z_MonoManager<ModSceneManager>
{

    bool enable = false;
    public string fileName;
    public MapBaseForm.Data curData;
    bool waitForActive = false;

    public int offset = 500;


    public int layer = 0;
    public int cntX=1;
    public int cntY=1;
    public int angle=0;
    public float posX=0;
    public float posZ=0;
    public float posY=0;

    public bool posing;


    public void Begin(MapDataController dataCtrl,string fileName)
    {
        this.fileName = fileName;
        GameManager.instance.RegisterInputByUgc();
        MapManager.instance.Begin(dataCtrl);
        CameraInstance.instance.Register(Vector3.zero,Z_Math.Graph.ElementwiseMultiply(MapManager.instance.sizeLimit, MapManager.instance.dataCtrl.mainData.mapUnitSize),5,15);
        CameraInstance.instance.tarTrs.position = Z_Math.Graph.ElementwiseMultiply(new Vector3(500, 500, 500),MapManager.instance.dataCtrl.mainData.mapUnitSize);
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

        if (waitForActive)
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
                        if(!MapManager.instance.dataCtrl.maps.ContainsKey((x, hitPos.y, z)))
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
                        var mapData = MapManager.instance.dataCtrl.maps[(x, hitPos.y, z)];

                        if (terrainData.step == 0)
                        {
                            mapData.prefabName = terrainData.prefabName;
                            mapData.pos = new Vector3(mapData.pos.x, mapData.mapPos.y * 3f, mapData.pos.z);

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

                                if (MapManager.instance.dataCtrl.maps.ContainsKey((stepX, mapData.mapPos.y, stepZ)))
                                {
                                    var cur = MapManager.instance.dataCtrl.maps[(stepX, mapData.mapPos.y, stepZ)];
                                    cur.prefabName = terrainData.prefabName;
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
                        if (!MapManager.instance.dataCtrl.maps.ContainsKey((x, hitPos.y, z)))
                            continue;
                        var mapData = MapManager.instance.dataCtrl.maps[(x, hitPos.y, z)];
                        mapData.texNameDic[layer] = textureData.texName;
                    }
            }
            else if (curData is MapTransitionMaskForm.Data maskData)
            {
                for (int x = hitPos.x - cntX / 2; x < hitPos.x + cntX / 2 + (cntX % 2 == 1 ? 1 : 0); x++)
                    for (int z = hitPos.z - cntY / 2; z < hitPos.z + cntY / 2 + (cntY % 2 == 1 ? 1 : 0); z++)
                    {
                        if (!MapManager.instance.dataCtrl.maps.ContainsKey((x, hitPos.y, z)))
                            continue;

                        var mapData = MapManager.instance.dataCtrl.maps[(x, hitPos.y, z)];
                        mapData.alphaTexNameDic[layer] = maskData.texName;
                    }
            }
            else if(curData is MapObstacleForm.Data obstacleData)
            {
                for (int x = hitPos.x - cntX / 2; x < hitPos.x + cntX / 2 + (cntX % 2 == 1 ? 1 : 0); x++)
                    for (int z = hitPos.z - cntY / 2; z < hitPos.z + cntY / 2 + (cntY % 2 == 1 ? 1 : 0); z++)
                    {
                        var finalX = posX + x;
                        var finalZ = posZ + z;
                        var finalY = posY + worldPosition.y;
                        var finalPos = new Vector3(finalX, finalY, finalZ);
                        var mapPos = MapManager.instance.utilCtrl.RealPos2MapPos(finalPos);
                        if (MapManager.instance.dataCtrl.maps.ContainsKey((mapPos.x, mapPos.y, mapPos.z)))
                        {
                            var mapData = MapManager.instance.dataCtrl.maps[(mapPos.x, mapPos.y, mapPos.z)];
                            bool allow = true;
                            //放置去重
                            foreach (var subUnit in mapData.unit.subUnits)
                            {
                                if (subUnit.data is ItemUnitForm.Data otherItemData)
                                {
                                    if (otherItemData.prefabName == obstacleData.prefabName&& (otherItemData.pos- finalPos).sqrMagnitude<0.001f&&Mathf.Abs(otherItemData.euler.y - angle)<1f)
                                    {
                                        allow = false;
                                        break;
                                    }
                                }
                            }
                            if(allow)
                            {
                                var itemData = MapManager.instance.AddItem(finalPos);
                                itemData.isObstacle = true;
                                itemData.euler.y = angle;
                                itemData.prefabName = obstacleData.prefabName;
                                itemData.name = obstacleData.name;
                            }
                        }
                    }
            }
            else if (curData is MapEraseForm.Data eraseData)
            {
                for (int x = hitPos.x - cntX / 2; x < hitPos.x + cntX / 2 + (cntX % 2 == 1 ? 1 : 0); x++)
                    for (int z = hitPos.z - cntY / 2; z < hitPos.z + cntY / 2 + (cntY % 2 == 1 ? 1 : 0); z++)
                    {
                        if (!MapManager.instance.dataCtrl.maps.ContainsKey((x, hitPos.y, z)))
                            continue;
                        var mapData = MapManager.instance.dataCtrl.maps[(x, hitPos.y, z)];
                        if (eraseData.terrain)
                        {
                            mapData.unit.Remove();
                        }else if(eraseData.obstacle)
                        {
                            foreach(var sub in mapData.unit.GetAllSubUnits())
                            {
                                if(sub is ItemUnit item&&item.data.isObstacle)
                                {
                                    sub.Remove();
                                }
                            }
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
        }, gameObject);
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
