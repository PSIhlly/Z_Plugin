using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_ByteSerialize;

namespace Z_Map
{
    public class MapData
    {
        public MapUnit[,,] maps;
        public List<ItemUnit> items;
        public List<CharacterUnit> characters;
        public List<GameObject> prefabs;
        public List<Material> materials;

        public Vector3 mapUnitSize;
        public Vector3Int viewSize;
        public Vector3Int size;
        public MapData(Vector3 mapUnitSize, Vector3Int viewSize, Vector3Int size, MapUnit[,,] maps, List<ItemUnit> items, List<CharacterUnit> characters, List<GameObject> prefabs, List<Material> materials)
        {
            this.mapUnitSize = mapUnitSize;
            this.viewSize = viewSize;
            this.size = size;
            this.maps = maps;
            this.items = items;
            this.characters = characters;
            this.prefabs = prefabs;
            this.materials = materials;
        }

        public MapData(JObject jo, List<GameObject> prefabs, List<Material> materials)
        {
            size = jo.Get<Vector3Int>("size");
            viewSize = jo.Get<Vector3Int>("viewSize");
            mapUnitSize = jo.Get<Vector3>("mapUnitSize");
            this.prefabs = prefabs;
            this.materials = materials;

            maps = new MapUnit[size.x, size.y, size.z];
            for (int i = 0; i < size.x; i++)
                for (int j = 0; j < size.y; j++)
                    for (int k = 0; k < size.z; k++)
                    {
                        maps[i, j, k] = new MapUnit(jo.Get<JObject>($"mapUnit_{i}_{j}_{k}"));
                    }

            var itemsJA = jo.Get<JArray>("items");
            items = new List<ItemUnit>(itemsJA.Count);
            for (int i = 0; i < itemsJA.Count; i++)
            {
                items.Add(new ItemUnit((JObject)itemsJA[i]));
            }

            var charactersJA = jo.Get<JArray>("characters");
            characters = new List<CharacterUnit>(charactersJA.Count);
            for (int i = 0; i < charactersJA.Count; i++)
            {
                characters.Add(new CharacterUnit((JObject)charactersJA[i]));
            }
        }
        public JObject GetJsonData()
        {
            var jo = new JObject();
            jo.Set("size", size);
            jo.Set("viewSize", viewSize);
            jo.Set("mapUnitSize", mapUnitSize);


            for (int i = 0; i < size.x; i++)
                for (int j = 0; j < size.y; j++)
                    for (int k = 0; k < size.z; k++)
                    {
                        jo.Set($"mapUnit_{i}_{j}_{k}", maps[i, j, k].GetJsonData());
                    }

            var itemsJA = new JArray();
            for (int i = 0; i < items.Count; i++)
            {
                itemsJA.Add(items[i].GetJsonData());
            }
            jo.Set("items", itemsJA);
            var charactersJA = new JArray();
            for (int i = 0; i < characters.Count; i++)
            {
                charactersJA.Add(characters[i].GetJsonData());
            }
            jo.Set("characters", charactersJA);
            return jo;
        }
        public static MapData GetDefault(List<GameObject> prefabs, List<Material> materials)
        {


            Vector3[] itemPoss = new Vector3[2] { new Vector3(2, 0.2f, 2), new Vector3(1, 0, 1) };
            Vector3[] itemEulars = new Vector3[2] { new Vector3(0, 0, 0), new Vector3(1, 45, 1) };

            Vector3[] characterPoss = new Vector3[3] { new Vector3(3, 0.2f, 3), new Vector3(7, 0, 7), new Vector3(0, 0, 0) };
            Vector3[] characterEulars = new Vector3[3] { new Vector3(0, 0, 0), new Vector3(1, 45, 1), new Vector3(0, 0, 0) };


            MapUnit[,,] maps = new MapUnit[10, 3, 10];

            for (int i = 0; i < maps.GetLength(0); i++)
            {
                for (int j = 0; j < maps.GetLength(1); j++)
                {
                    for (int k = 0; k < maps.GetLength(2); k++)
                    {
                        if (i == 0 && j == 0 && k == 3)
                        {
                            maps[i, j, k] = new MapUnit(k, 0, new Vector3(i, 2.5f, k), new Vector3(-45, 0, 0), new Vector3(1, 1, 1.414f), (i + j + k) % 3, new Vector3Int(i, j, k));

                        }
                        else
                            if (i == 0 && j == 0 && k == 2)
                        {
                            maps[i, j, k] = new MapUnit(k, 0, new Vector3(i, 1.5f, k), new Vector3(-45, 0, 0), new Vector3(1, 1, 1.414f), (i + j + k) % 3, new Vector3Int(i, j, k));

                        }
                        else
                            if (i == 0 && j == 0 && k == 1)
                        {
                            maps[i, j, k] = new MapUnit(k, 0, new Vector3(i, 0.5f, k), new Vector3(-45, 0, 0), new Vector3(1, 1, 1.414f), (i + j + k) % 3, new Vector3Int(i, j, k));

                        }
                        else
                            maps[i, j, k] = new MapUnit(k, 0, Z_Math.Graph.ElementwiseMultiply(new Vector3(i, j, k), new Vector3(1, 3, 1)), new Vector3(0, 0, 0), Vector3.one, (i + j + k) % 3, new Vector3Int(i, j, k));
                    }
                }
            }

            maps[0, 1, 3].scale = Vector3.zero;
            maps[0, 1, 2].scale = Vector3.zero;

            maps[0, 1, 1].scale = Vector3.zero;
            maps[0, 1, 0].scale = Vector3.zero;

            var items = new List<ItemUnit>(itemPoss.Length);
            for (int i = 0; i < itemPoss.Length; i++)
            {
                
                items.Add(new ItemUnit(i, i + 1, itemPoss[i], itemEulars[i], Vector3.one, true));
            }

            var characters = new List<CharacterUnit>(characterPoss.Length);
            for (int i = 0; i < characterPoss.Length; i++)
            {
                characters.Add(new CharacterUnit(i, i + 3, characterPoss[i], characterEulars[i], Vector3.one, (i == characterPoss.Length - 1), 5, 10));
            }
            
            return new MapData(new Vector3(1, 3, 1), new Vector3Int(5, 2, 5), new Vector3Int(10, 3, 10), maps, items, characters, prefabs,materials);
            //new Vector3(1, 3, 1),new Vector3Int(5, 2, 5),maps,items,characters
        }
    }

}

