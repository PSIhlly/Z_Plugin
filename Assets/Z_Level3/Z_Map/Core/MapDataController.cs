using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_ByteSerialize;
using Z_Debug;
using Z_DesignStyle;
using Z_Map.Form;
using Z_UnitSystem.Form;

namespace Z_Map
{
    public class MapDataController : Z_Controller<MapManager>
    {
        public MapMainForm.Data mainData;
        public MapUnitForm.Data[,,] maps;

        public MapDataController(string formData)
        {

            MapUnitForm.Clear();
            ItemUnitForm.Clear();
            CharacterUnitForm.Clear();
            mainData = MapMainForm.GetDataByJo(JObject.Parse(formData));
            maps = new MapUnitForm.Data[mainData.size.x, mainData.size.y, mainData.size.z];

            var mapDatas = MapUnitForm.GetDatasByJa(JArray.Parse(mainData.mapJa));
            for (int i = 0; i < mapDatas.Count; i++)
            {
                MapUnitForm.AddData(mapDatas[i]);
                maps[mapDatas[i].mapPos.x, mapDatas[i].mapPos.y, mapDatas[i].mapPos.z]= mapDatas[i];
            }

            var itemDatas = ItemUnitForm.GetDatasByJa(JArray.Parse(mainData.ItemJa));
            for (int i = 0; i < itemDatas.Count; i++)
            {
                ItemUnitForm.AddData(itemDatas[i]);
            }

            var charactersDatas = CharacterUnitForm.GetDatasByJa(JArray.Parse(mainData.CharacterJa));
            for (int i = 0; i < charactersDatas.Count; i++)
            {
                CharacterUnitForm.AddData(charactersDatas[i]);
            }

           
        }
        public JObject GetJsonData()
        {
            mainData.mapJa = JsonConvert.SerializeObject(MapUnitForm.GetJaByDatas());
            mainData.ItemJa = JsonConvert.SerializeObject(ItemUnitForm.GetJaByDatas());

            mainData.CharacterJa = JsonConvert.SerializeObject(CharacterUnitForm.GetJaByDatas());
            return MapMainForm.GetJoByData(mainData);
        }
        public MapDataController()
        {

            MapUnitForm.Clear();
            ItemUnitForm.Clear();
            CharacterUnitForm.Clear();
            int uidCnt = 0;

            maps = new MapUnitForm.Data[300, 1, 300];

            Vector3 realPos = Vector3.one; 
            Vector3Int mapPos= Vector3Int.one;
            string mapName = "map";
            for (int i = 0; i < 300; i++)
            {
                realPos.x = i*1;
                mapPos.x = i;
                for (int j = 0; j < 1; j++)
                {
                    realPos.y = j * 3;
                    mapPos.y= j;

                    for (int k = 0; k < 300; k++)
                    {
                        realPos.z = k * 1;
                        mapPos.z = k;

                        MapUnitForm.AddData(new MapUnitForm.Data(++uidCnt, "", 0, mapPos, mapName, realPos, Vector3.zero, Vector3.one, 0));
                        maps[i, j, k] = MapUnitForm.DataByUid[uidCnt];
                    }
                }
            }
            mainData = new MapMainForm.Data(
                1, 
                uidCnt,
                new Vector3(1, 1, 1),
                new Vector3Int(300, 1, 300),
                new Vector3Int(30, 1, 20),
               "",
               "",
               ""
           );
        }
        public void Unload()
        {
            foreach (var data in UnitForm.DataByUid.Values)
            {
                data.unit.Hide();
            }
        }
    }

}

