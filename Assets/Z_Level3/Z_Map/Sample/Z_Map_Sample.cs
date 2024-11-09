using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Map;

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
        
        {
            MapManager.instance.SetPos(MapManager.instance.characterDic[2].pos);
        }
        if (MapManager.instance.characterDic[2].ins == null)
            return;
        var main = MapManager.instance.characterDic[2].ins.transform;
        if(MapManager.instance.characterDic.ContainsKey(0))
            MapManager.instance.characterDic[0].destination = main.position;
        MapManager.instance.characterDic[1].destination = main.position;
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
            MapManager.instance.RemoveCharacterUnit(0);

        if (Input.GetKeyDown(KeyCode.E))
            MapManager.instance.AddCharacterUnit(new CharacterUnit(8, 4, main.position + Vector3.forward, Vector3.zero, Vector3.one));
            //MapManager.instance.AddItemUnit(new ItemUnit(8, 1, main.position + Vector3.forward, Vector3.zero, Vector3.one,true));
    }
}
