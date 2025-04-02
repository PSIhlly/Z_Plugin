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
    public class MapData
    {
        public MapMainForm.Data mainData;
        public Dictionary<(int,int,int), MapUnitForm.Data>maps;
        public Dictionary<(int,int),SortedSet<int>>mapXZ2Y;

        string mapName => GlobalHelper.GetInternalPrefabName("map");
        string defaultTextureName => "grass";

        public MapData(string formData)
        {

            MapUnitForm.Clear();
            ObjectUnitForm.Clear();
            CharacterUnitForm.Clear();
            mainData = MapMainForm.GetDataByJo(JObject.Parse(formData));
            maps = new Dictionary<(int, int, int), MapUnitForm.Data>();
            mapXZ2Y = new Dictionary<(int, int), SortedSet<int>>();
            var mapDatas = MapUnitForm.GetDatasByJa(JArray.Parse(mainData.mapJa));
            for (int i = 0; i < mapDatas.Count; i++)
            {
                MapUnitForm.AddData(mapDatas[i]);
                RegisterMap(mapDatas[i]);
            }

            var itemDatas = ObjectUnitForm.GetDatasByJa(JArray.Parse(mainData.objectJa));
            for (int i = 0; i < itemDatas.Count; i++)
            {
                ObjectUnitForm.AddData(itemDatas[i]);
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
            mainData.objectJa = JsonConvert.SerializeObject(ObjectUnitForm.GetJaByDatas());

            mainData.characterJa = JsonConvert.SerializeObject(CharacterUnitForm.GetJaByDatas());
            return MapMainForm.GetJoByData(mainData);
        }
        public MapData()
        {

            MapUnitForm.Clear();
            ObjectUnitForm.Clear();
            CharacterUnitForm.Clear();
            maps = new Dictionary<(int, int, int), MapUnitForm.Data>();
            mapXZ2Y = new Dictionary<(int, int), SortedSet<int>>();
            var unitSize = new Vector3(1, 2, 1);
            Vector3 realPos = Vector3.one; 
            Vector3Int mapPos= Vector3Int.one;
            for (int i = 495; i < 505; i++)
            {
                realPos.x = i * unitSize.x;
                mapPos.x = i;
                for (int j = 500; j < 501; j++)
                {
                    realPos.y = j * unitSize.y;
                    mapPos.y= j;

                    for (int k = 495; k < 505; k++)
                    {
                        realPos.z = k * unitSize.z;
                        mapPos.z = k;
                        var data = new MapUnitForm.Data(-1, "", new Dictionary<int, string>() { { 0, defaultTextureName } }, new Dictionary<int, string>(), new Dictionary<int, int>() { { 0, 0 },{1,0 },{ 2,0} }, mapPos, mapName, realPos, Vector3.zero, Vector3.one, 0);
                        MapUnitForm.AddData(data);
                        RegisterMap(data);
                    }
                }
            }
            mainData = new MapMainForm.Data(
                1,
                unitSize,
                new Vector3Int(30, 5, 15),
                new Vector3Int(30, 5, 15),
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
        public void RegisterMap(MapUnitForm.Data data)
        {
            maps[(data.mapPos.x, data.mapPos.y, data.mapPos.z)] = data;

            if (!mapXZ2Y.ContainsKey((data.mapPos.x, data.mapPos.z)))
                mapXZ2Y[(data.mapPos.x, data.mapPos.z)] = new SortedSet<int>();
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
            var data = new MapUnitForm.Data(-1, "", new Dictionary<int, string>() { { 0, defaultTextureName } }, new Dictionary<int, string>(), new Dictionary<int, int>() { { 0, 0 }, { 1, 0 },{2,0 } }, mapPos, mapName, mapPos,Vector3.zero,Vector3.one,0);
            MapUnitForm.AddData(data);
            RegisterMap(data);

            return data;
        }
        public ObjectUnitForm.Data AddItem()
        {
            var data = new ObjectUnitForm.Data(-1,false,"","", Vector3.zero, Vector3.zero,Vector3.one,0);
            ObjectUnitForm.AddData(data);
            return data;
        }
    }

}

