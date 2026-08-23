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
using Z_UnitSystem;
using Z_UnitSystem.Form;

namespace Z_Map
{
    public class MapInfo
    {
        public MapMainForm.Data mainData;
        public Dictionary<(int, int, int), TileUnitForm.Data> maps;
        public Dictionary<(int, int), SortedSet<int>> mapXZ2Y;

        public Dictionary<string,AssetForm.Data> innerPrefabDic = new Dictionary<string, AssetForm.Data>();
        public static string GetPrefabName(string name="") => "MapPrefab$" + name;

        public void Init(GameObjectAssetForm.Data mapgroundPrefab, GameObjectAssetForm.Data mapslopePrefab, GameObjectAssetForm.Data mapfloorPrefab, GameObjectAssetForm.Data imgPrefab, GameObjectAssetForm.Data canvasPrefab,TexAssetForm.Data defaultTileTexture)
        {
            innerPrefabDic["mapground"] = mapgroundPrefab;
            innerPrefabDic["mapslope"] = mapslopePrefab;
            innerPrefabDic["mapfloor"] = mapfloorPrefab;
            innerPrefabDic["img"] = imgPrefab;
            innerPrefabDic["canvas"] = canvasPrefab;
            innerPrefabDic["defaultTileTexture"] = defaultTileTexture;
            TileUnitForm.Clear();
            ObjectUnitForm.Clear();
            CharacterUnitForm.Clear();
            var unitSize = new Vector3(1, 1.5f, 1);
            mainData = new MapMainForm.Data(
                1,
                unitSize,
                new Vector3Int(30, 5, 15),
                new Vector3Int(30, 5, 15),
               "",
               "",
               "",
               ""
           );
            maps = new Dictionary<(int, int, int), TileUnitForm.Data>();
            mapXZ2Y = new Dictionary<(int, int), SortedSet<int>>();
            for (int i = 495; i < 505; i++)
            {
                for (int j = 500; j < 501; j++)
                {
                    for (int k = 495; k < 505; k++)
                    {
                        AddTile(new Vector3Int(i, j, k));
                    }
                }
            }

        }
        public void Init(string formData)
        {
            TileUnitForm.Clear();
            ObjectUnitForm.Clear();
            CharacterUnitForm.Clear();
            mainData = MapMainForm.GetDataByJo(JObject.Parse(formData));
            maps = new Dictionary<(int, int, int), TileUnitForm.Data>();
            mapXZ2Y = new Dictionary<(int, int), SortedSet<int>>();
            var mapDatas = GetTileDatasByJa(mainData.mapJa);


            for (int i = 0; i < mapDatas.Count; i++)
            {
                RegisterNewTile(mapDatas[i]);
                RegisterMap(mapDatas[i]);
            }
            var itemDatas = GetItemDatasByJa(mainData.itemJa);
            for (int i = 0; i < itemDatas.Count; i++)
            {
                RegisterNewItem(itemDatas[i]);
            }

            var objectDatas = GetObjectDatasByJa(mainData.objectJa);
            for (int i = 0; i < objectDatas.Count; i++)
            {
                RegisterNewObject(objectDatas[i]);
            }

            var charactersDatas = GetCharacterDatasByJa(mainData.characterJa);
            for (int i = 0; i < charactersDatas.Count; i++)
            {
                RegisterNewCharacter(charactersDatas[i]);
            }
        }
        public void Unload()
        {
            foreach (var data in UnitForm.DataByUid.Values)
            {
                data.unit.Hide();
            }
        }
        public void RegisterMap(TileUnitForm.Data data)
        {
            maps[(data.mapPos.x, data.mapPos.y, data.mapPos.z)] = data;

            if (!mapXZ2Y.ContainsKey((data.mapPos.x, data.mapPos.z)))
                mapXZ2Y[(data.mapPos.x, data.mapPos.z)] = new SortedSet<int>();
            mapXZ2Y[(data.mapPos.x, data.mapPos.z)].Add(data.mapPos.y);


        }
        public void UnRegisterMap(TileUnitForm.Data data)
        {
            if (maps.ContainsKey((data.mapPos.x, data.mapPos.y, data.mapPos.z)))
                maps.Remove((data.mapPos.x, data.mapPos.y, data.mapPos.z));

            if (mapXZ2Y.ContainsKey((data.mapPos.x, data.mapPos.z)))
                mapXZ2Y[(data.mapPos.x, data.mapPos.z)].Remove(data.mapPos.y);
        }
        #region unit
        public TileUnitForm.Data AddTile(Vector3Int mapPos, object[] prms = null)
        {
            var data = GetNewTile(mapPos, prms);
            RegisterNewTile(data);
            RegisterMap(data);

            return data;
        }
        public ItemUnitForm.Data AddItem(string prefabName = "", object[] prms = null)
        {
            var data = GetNewItem(prefabName, prms);
            RegisterNewItem(data);
            return data;
        }
        public ObjectUnitForm.Data AddObject(string prefabName = "", object[] prms = null)
        {
            var data = GetNewObject(prefabName, prms);
            RegisterNewObject(data);
            return data;
        }
        public CharacterUnitForm.Data AddCharacter(string prefabName = "", bool isMine = false, string extra = "")
        {
            var data = GetNewCharacter(prefabName, isMine, extra);
            RegisterNewCharacter(data);
            return data;
        }
        public void RemoveTile(TileUnitForm.Data data)
        {
            UnregisterTile(data);
        }
        public void RemoveItem(ItemUnitForm.Data data)
        {
            UnregisterItem(data);
        }
        public void RemoveObject(ObjectUnitForm.Data data)
        {
            UnregisterObject(data);
        }
        public void RemoveCharacter(CharacterUnitForm.Data data)
        {
            UnregisterCharacter(data);
        }
        #endregion

        public virtual JObject GetJsonData()
        {
            mainData.mapJa = JsonConvert.SerializeObject(TileUnitForm.GetJaByDatas());
            mainData.objectJa = JsonConvert.SerializeObject(ObjectUnitForm.GetJaByDatas());
            mainData.characterJa = JsonConvert.SerializeObject(CharacterUnitForm.GetJaByDatas());
            mainData.itemJa = JsonConvert.SerializeObject(ItemUnitForm.GetJaByDatas());
            return MapMainForm.GetJoByData(mainData);
        }


        public virtual CharacterUnitForm.Data GetNewCharacter(string prefabName = "", bool isMine = false, string extra = "")
        {
            return new CharacterUnitForm.Data(-1, false, Vector3.zero, 4, 4, 4, isMine, "", prefabName, Vector3.zero, Vector3.zero, Vector3.one, 0,new List<int>(), extra, false, GlobalDefaultHelper.DefaultTexId);
        }
        public virtual void RegisterNewCharacter(CharacterUnitForm.Data data)
        {
            CharacterUnitForm.AddData(data);
        }
        public virtual List<CharacterUnitForm.Data> GetCharacterDatasByJa(string ja)
        {
            return CharacterUnitForm.GetDatasByJa(JArray.Parse(mainData.characterJa));
        }

        public virtual ItemUnitForm.Data GetNewItem(string prefabName = "", object[] prms = null)
        {
            return new ItemUnitForm.Data(-1, "", prefabName, Vector3.zero, Vector3.zero, Vector3.one, 0, new List<int>(), "", false,GlobalDefaultHelper.DefaultTexId);
        }
        public virtual void RegisterNewItem(ItemUnitForm.Data data)
        {
            ItemUnitForm.AddData(data);
        }
        public virtual List<ItemUnitForm.Data> GetItemDatasByJa(string ja)
        {
            return ItemUnitForm.GetDatasByJa(JArray.Parse(mainData.itemJa));
        }


        public virtual ObjectUnitForm.Data GetNewObject(string prefabName = "", object[] prms = null)
        {
            return new ObjectUnitForm.Data(-1, false, "", prefabName, Vector3.zero, Vector3.zero, Vector3.one, 0, new List<int>(), "", false, GlobalDefaultHelper.DefaultTexId);
        }


        public virtual void RegisterNewObject(ObjectUnitForm.Data data)
        {
            ObjectUnitForm.AddData(data);

        }

        public virtual List<ObjectUnitForm.Data> GetObjectDatasByJa(string ja)
        {
            return ObjectUnitForm.GetDatasByJa(JArray.Parse(mainData.objectJa));
        }
        public virtual void UnregisterItem(ItemUnitForm.Data data)
        {
            data.unit.Remove();
        }
        public virtual void UnregisterTile(TileUnitForm.Data data)
        {
            data.unit.Remove();
        }
        public virtual void UnregisterCharacter(CharacterUnitForm.Data data)
        {
            data.unit.Remove();
        }
        public virtual void UnregisterObject(ObjectUnitForm.Data data)
        {
            data.unit.Remove();
        }

        public virtual TileUnitForm.Data GetNewTile(Vector3Int mapPos, object[] prms = null)
        {
            return new TileUnitForm.Data(-1, "", new Dictionary<int, int>() { { 0, 1 } }, mapPos, GetPrefabName("mapground"), Z_Math.Graph.ElementwiseMultiply(mapPos, mainData.mapUnitSize), Vector3.zero, Vector3.one, 0, new List<int>(), "",false,false);
        }
        public virtual void RegisterNewTile(TileUnitForm.Data data)
        {
            TileUnitForm.AddData(data);
        }
        public virtual List<TileUnitForm.Data> GetTileDatasByJa(string ja)
        {
            return TileUnitForm.GetDatasByJa(JArray.Parse(mainData.mapJa));
        }

        public virtual bool CheckItemUnit(ItemUnitForm.Data item)
        {
            return InstancePoolManager.instance.GetPrefab(item.prefabName) != null;
        }
        public virtual bool CheckObjectUnit(ObjectUnitForm.Data obj)
        {
            return InstancePoolManager.instance.GetPrefab(obj.prefabName) != null;
        }
        public virtual bool CheckCharacterUnit(CharacterUnitForm.Data ch)
        {
            return InstancePoolManager.instance.GetPrefab(ch.prefabName) != null;
        }
    }

}

