using Microsoft.Win32;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_DesignStyle;
using Z_Map;

public class ModSceneManager : Z_MonoManager<ModSceneManager>
{
    bool enable = false;
    public string fileName;
    public void Begin(MapDataController dataCtrl,string fileName)
    {
        this.fileName = fileName;
        GameManager.instance.RegisterInputByUgc();
        MapManager.instance.Begin(dataCtrl);
        CameraInstance.instance.Register(Vector3.zero,Z_Math.Graph.ElementwiseMultiply(MapManager.instance.dataCtrl.mainData.size, MapManager.instance.dataCtrl.mainData.mapUnitSize));
        enable = true;
    }
    public void End()
    {

        GameManager.instance.RegisterInputDefault();
    }
    public void Update()
    {

        MapManager.instance.UpdateInfo();
        {
            MapManager.instance.SetPos(CameraInstance.instance.tarTrs.position);
        }
        /*
        if (CharacterUnitForm.DataByUid[303].unit.ins == null)
            return;
        var main = CharacterUnitForm.DataByUid[303].unit.ins.transform;

        CharacterUnitForm.DataByUid[304].destination = main.position;
        CharacterUnitForm.DataByUid[305].destination = main.position;*/
    }
}
