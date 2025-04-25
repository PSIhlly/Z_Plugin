using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_DataSystem;
using Z_DataSystem.Form;
using Z_Map;
using Z_Map.Form;
using Z_UnitSystem;

public class Z_Map_Sample : MonoBehaviour
{
    public void Start()
    {

        var res = AssetManager.instance.LoadAssetsByFolder(Application.dataPath + "/Z_Level3/Z_Map/Sample/Imgs",false);
        foreach (var tex in res.texs)
        {
            TexAssetForm.AddData(new TexAssetForm.Data(-1, tex.Item1, tex.Item2));
        }
        var data = new MapData();
        data.Init();
        MapManager.instance.Begin(data);
    }
    public void Update()
    {
        MapManager.instance.UpdateInfo();
        {
            MapManager.instance.SetPos(Camera.main.transform.position);
        }/*
        if (CharacterUnitForm.DataByUid[303].unit.ins == null)
            return;
        var main = CharacterUnitForm.DataByUid[303].unit.ins.transform;
        
        CharacterUnitForm.DataByUid[304].destination = main.position;
        CharacterUnitForm.DataByUid[305].destination = main.position;*/

        if (Input.GetKey(KeyCode.W))
            Camera.main.transform.position += Time.deltaTime * Vector3.forward*2;
        if (Input.GetKey(KeyCode.S))
            Camera.main.transform.position += Time.deltaTime * Vector3.back*2;
        if (Input.GetKey(KeyCode.A))
            Camera.main.transform.position += Time.deltaTime * Vector3.left*2;
        if (Input.GetKey(KeyCode.D))
            Camera.main.transform.position += Time.deltaTime * Vector3.right*2;

         /* if (Input.GetKey(KeyCode.O))
            SaveAndLoad.Save("data1", JsonConvert.SerializeObject(MapManager.instance.dataCtrl.GetJsonData()));
       if (Input.GetKey(KeyCode.P))
         {
             JObject jo = JObject.Parse(SaveAndLoad.Load("data1"));
             MapManager.instance.Begin(new MapDataController(jo, prefabs, materials));
    }*/
        /* if (Input.GetKeyDown(KeyCode.Q))
             MapManager.instance.RemoveUnit(0);

         if (Input.GetKeyDown(KeyCode.E))
             MapManager.instance.AddUnit(new CharacterUnit(8, 4, main.position + Vector3.forward, Vector3.zero, Vector3.one));*/
        //MapManager.instance.AddItemUnit(new ItemUnit(8, 1, main.position + Vector3.forward, Vector3.zero, Vector3.one,true));
    }
}
