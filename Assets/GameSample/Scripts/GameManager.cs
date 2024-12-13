using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Input;
using Z_Map;

public class GameManager : MonoBehaviour
{
    public List<GameObject> prefabs;
    public List<Material> materials;
    public void Start()
    {
        RegisterInput();
        MapManager.instance.Begin(MapData.GetDefault(prefabs, materials));

    }
    public void Update()
    {
        
           }
    public void RegisterInput()
    {
        var ins = InputManager.instance;
        var config = new InputConfig();

        config.onButonW = () =>
        {
            var main = MapManager.instance.unitDic[4].ins.transform;
            main.position += Time.deltaTime * Vector3.forward * 2;
        }; 
        config.onButonS = () =>
        {
            var main = MapManager.instance.unitDic[4].ins.transform;
            main.position += Time.deltaTime * Vector3.back * 2;
        }; 
        config.onButonA = () =>
        {
            var main = MapManager.instance.unitDic[4].ins.transform;
            main.position += Time.deltaTime * Vector3.left * 2;
        }; 
        config.onButonD = () =>
        {
            var main = MapManager.instance.unitDic[4].ins.transform;
            main.position += Time.deltaTime * Vector3.right * 2;
        };

        ins.Register(config);

       /* if (Input.GetKey(KeyCode.O))
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
*/
    }
}
