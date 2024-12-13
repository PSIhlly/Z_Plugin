using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Map;
using Z_UnitSystem;

public class Z_Map_Sample : MonoBehaviour
{
    public List<GameObject> prefabs;
    public List<Material> materials;
    public void Start()
    {
        MapManager.instance.Begin(MapData.GetDefault(prefabs, materials));
    }
    public void Update()
    {
        MapManager.instance.UpdateInfo();
        {
            MapManager.instance.SetPos(MapManager.instance.unitDic[4].pos);
        }
        if (MapManager.instance.unitDic[4].ins == null)
            return;
        var main = MapManager.instance.unitDic[4].ins.transform;
        if(MapManager.instance.unitDic.ContainsKey(0))
            ((CharacterUnit)(MapManager.instance.unitDic[2])).destination = main.position;
        ((CharacterUnit)(MapManager.instance.unitDic[3])).destination = main.position;
        if (Input.GetKey(KeyCode.W))
            main.position += Time.deltaTime * Vector3.forward*2;
        if (Input.GetKey(KeyCode.S))
            main.position += Time.deltaTime * Vector3.back*2;
        if (Input.GetKey(KeyCode.A))
            main.position += Time.deltaTime * Vector3.left*2;
        if (Input.GetKey(KeyCode.D))
            main.position += Time.deltaTime * Vector3.right*2;

        if (Input.GetKey(KeyCode.O))
            SaveAndLoad.Save("data1", JsonConvert.SerializeObject(MapManager.instance.data.GetJsonData()));
        if (Input.GetKey(KeyCode.P))
        {
            JObject jo = JObject.Parse(SaveAndLoad.Load("data1"));
            MapManager.instance.Begin(new MapData(jo, prefabs, materials));
        }
        if (Input.GetKeyDown(KeyCode.Q))
            MapManager.instance.RemoveUnit(0);

        if (Input.GetKeyDown(KeyCode.E))
            MapManager.instance.AddUnit(new CharacterUnit(8, 4, main.position + Vector3.forward, Vector3.zero, Vector3.one));
            //MapManager.instance.AddItemUnit(new ItemUnit(8, 1, main.position + Vector3.forward, Vector3.zero, Vector3.one,true));
    }
}
