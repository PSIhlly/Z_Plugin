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
            mainData.mapJa = MapUnitForm.GetJaByDatas().ToString();
            mainData.ItemJa = ItemUnitForm.GetJaByDatas().ToString();

            mainData.CharacterJa = CharacterUnitForm.GetJaByDatas().ToString();
            return MapMainForm.GetJoByData(mainData);
        }
        public MapDataController()
        {

            MapUnitForm.Clear();
            ItemUnitForm.Clear();
            CharacterUnitForm.Clear();
            int uidCnt = 0;

            maps = new MapUnitForm.Data[10, 3, 10];
            Vector3[] itemPoss = new Vector3[2] { new Vector3(2, 0.2f, 2), new Vector3(1, 0, 1) };
            Vector3[] itemEulers = new Vector3[2] { new Vector3(0, 0, 0), new Vector3(1, 45, 1) };

            Vector3[] characterPoss = new Vector3[3] { new Vector3(3, 0.2f, 3), new Vector3(7, 0, 7), new Vector3(0, 0, 0) };
            Vector3[] characterEulers = new Vector3[3] { new Vector3(0, 0, 0), new Vector3(1, 45, 1), new Vector3(0, 0, 0) };


            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    for (int k = 0; k < 10; k++)
                    {
                        if (i == 0 && j == 0 && k == 3)
                        {
                            MapUnitForm.AddData(new MapUnitForm.Data(++uidCnt,"_",0, new Vector3Int(i, j, k),"map", new Vector3(i, 2.5f, k), new Vector3(-45, 0, 0), new Vector3(1, 1, 1.414f),0));

                        }
                        else
                            if (i == 0 && j == 0 && k == 2)
                        {
                            MapUnitForm.AddData(new MapUnitForm.Data(++uidCnt, "_", 0, new Vector3Int(i, j, k), "map", new Vector3(i, 1.5f, k), new Vector3(-45, 0, 0), new Vector3(1, 1, 1.414f), 0));

                        }
                        else
                            if (i == 0 && j == 0 && k == 1)
                        {
                            MapUnitForm.AddData(new MapUnitForm.Data(++uidCnt, "_", 0, new Vector3Int(i, j, k), "map", new Vector3(i, 0.5f, k), new Vector3(-45, 0, 0), new Vector3(1, 1, 1.414f), 0));

                        }
                        else
                        if ((i == 0 && j == 1 && (k == 0 || k == 1||k == 2 || k == 3)))
                        {
                            MapUnitForm.AddData(new MapUnitForm.Data(++uidCnt, "_", 0, new Vector3Int(i, j, k), "map", Z_Math.Graph.ElementwiseMultiply(new Vector3(i, j, k), new Vector3(1, 3, 1)), new Vector3(0, 0, 0), Vector3.zero, 0));

                        }
                        else
                        {
                            MapUnitForm.AddData(new MapUnitForm.Data(++uidCnt, "_", 0, new Vector3Int(i, j, k), "map", Z_Math.Graph.ElementwiseMultiply(new Vector3(i, j, k), new Vector3(1, 3, 1)), new Vector3(0, 0, 0), Vector3.one, 0));
                        }

                        maps[i, j, k] = MapUnitForm.DataByUid[uidCnt];
                    }
                }
            }

            for (int i = 0; i < itemPoss.Length; i++)
            {
                ItemUnitForm.AddData(new ItemUnitForm.Data(++uidCnt, true, "item"+(i+1), itemPoss[i], itemEulers[i], Vector3.one,0));
            }

            
            for (int i = 0; i < characterPoss.Length; i++)
            {
                CharacterUnitForm.AddData(new CharacterUnitForm.Data(++uidCnt, i!=0, Vector3.zero,1,20,20,  i != 0, "character" + (i + 1), characterPoss[i], characterEulers[i], Vector3.one, 0));
             }
            mainData = new MapMainForm.Data(
                1, 
                uidCnt,
                new Vector3(1, 3, 1),
                new Vector3Int(10, 3, 10),
                new Vector3Int(5, 3, 5),
               MapUnitForm.GetJaByDatas().ToString(),
               ItemUnitForm.GetJaByDatas().ToString(),
               CharacterUnitForm.GetJaByDatas().ToString()
           );
            Z_Log.Log("数据：" + GetJsonData());
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

