using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_ByteSerialize;
using Z_DataSystem;
using Z_DataSystem.Form;
using Z_Debug;
using Z_DesignStyle;
using Z_Map.Form;
using Z_UnitSystem.Form;

namespace Z_Map
{
    public class MapDataController : Z_Controller<MapManager>
    {
        public MapMainForm.Data mainData;
        public Dictionary<(int,int,int), MapUnitForm.Data>maps;
        public Dictionary<(int,int),List<int>>mapXZ2Y;

        public MapDataController(string formData)
        {

            MapUnitForm.Clear();
            ItemUnitForm.Clear();
            CharacterUnitForm.Clear();
            mainData = MapMainForm.GetDataByJo(JObject.Parse(formData));
            maps = new Dictionary<(int, int, int), MapUnitForm.Data>();
            mapXZ2Y = new Dictionary<(int, int), List<int>>();
            var mapDatas = MapUnitForm.GetDatasByJa(JArray.Parse(mainData.mapJa));
            for (int i = 0; i < mapDatas.Count; i++)
            {
                MapUnitForm.AddData(mapDatas[i]);
                RegisterMap(mapDatas[i]);
            }

            var itemDatas = ItemUnitForm.GetDatasByJa(JArray.Parse(mainData.itemJa));
            for (int i = 0; i < itemDatas.Count; i++)
            {
                ItemUnitForm.AddData(itemDatas[i]);
            }

            var charactersDatas = CharacterUnitForm.GetDatasByJa(JArray.Parse(mainData.characterJa));
            for (int i = 0; i < charactersDatas.Count; i++)
            {
                CharacterUnitForm.AddData(charactersDatas[i]);
            }

           
        }
        public JObject GetJsonData()
        {
            mainData.mapJa = JsonConvert.SerializeObject(MapUnitForm.GetJaByDatas());
            mainData.itemJa = JsonConvert.SerializeObject(ItemUnitForm.GetJaByDatas());

            mainData.characterJa = JsonConvert.SerializeObject(CharacterUnitForm.GetJaByDatas());
            return MapMainForm.GetJoByData(mainData);
        }
        public MapDataController()
        {

            MapUnitForm.Clear();
            ItemUnitForm.Clear();
            CharacterUnitForm.Clear();
            maps = new Dictionary<(int, int, int), MapUnitForm.Data>();
            mapXZ2Y = new Dictionary<(int, int), List<int>>();

            Vector3 realPos = Vector3.one; 
            Vector3Int mapPos= Vector3Int.one;
            string mapName = "map";
            for (int i = 495; i < 505; i++)
            {
                realPos.x = i*1;
                mapPos.x = i;
                for (int j = 500; j < 501; j++)
                {
                    realPos.y = j * 3;
                    mapPos.y= j;

                    for (int k = 495; k < 505; k++)
                    {
                        realPos.z = k * 1;
                        mapPos.z = k;
                        var data = new MapUnitForm.Data(-1, "", new Dictionary<int, string>() { { 0,"tile1" } }, new Dictionary<int, string>(), mapPos, mapName, realPos, Vector3.zero, Vector3.one, 0);
                        MapUnitForm.AddData(data);
                        RegisterMap(data);
                    }
                }
            }


           


            mainData = new MapMainForm.Data(
                1, 
                new Vector3(1, 3, 1),
                new Vector3Int(30, 5, 20),
               "",
               "",
               "",
               new List<string>() { "tile1", "tile2", "tile3", "tile4" },
               new List<string>() {"alpha" }
           );
        }
        public void Unload()
        {
            foreach (var data in UnitForm.DataByUid.Values)
            {
                data.unit.Hide();
            }
        }
        public void RegisterMap(MapUnitForm.Data data)
        {
            maps[(data.mapPos.x, data.mapPos.y, data.mapPos.z)] = data;

            if (!mapXZ2Y.ContainsKey((data.mapPos.x, data.mapPos.z)))
                mapXZ2Y[(data.mapPos.x, data.mapPos.z)] = new List<int>();
            mapXZ2Y[(data.mapPos.x, data.mapPos.z)].Add(data.mapPos.y);
        }
        public void UnRegisterMap(MapUnitForm.Data data)
        {
            if (maps.ContainsKey((data.mapPos.x, data.mapPos.y, data.mapPos.z)))
                maps.Remove((data.mapPos.x, data.mapPos.y, data.mapPos.z));

            if (mapXZ2Y.ContainsKey((data.mapPos.x, data.mapPos.z)))
                mapXZ2Y[(data.mapPos.x, data.mapPos.z)].Remove(data.mapPos.y);
        }

        public MapUnitForm.Data AddMap(Vector3Int mapPos)
        {
            var data = new MapUnitForm.Data(-1,"", new Dictionary<int, string>() { {0,"tile1" } }, new Dictionary<int, string>(), mapPos,"map", mapPos,Vector3.zero,Vector3.one,0);
            MapUnitForm.AddData(data);
            RegisterMap(data);
            return data;
        }
        public ItemUnitForm.Data AddItem()
        {
            var data = new ItemUnitForm.Data(-1,false,"","", Vector3.zero, Vector3.zero,Vector3.one,0);
            ItemUnitForm.AddData(data);
            return data;
        }
    }

}

