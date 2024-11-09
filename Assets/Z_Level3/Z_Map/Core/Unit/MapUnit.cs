using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_ByteSerialize;

namespace Z_Map
{

    public class MapUnit : Unit
    {
        public MapUnit(JObject jo) : base(jo)
        {
            LoadJsonData(jo);
        }
        public MapUnit(int uid, int prefabId_Data, Vector3 pos, Vector3 eular, Vector3 scale, int matId_Data, Vector3Int mapPos, UpdateType updateType = UpdateType.ShowOnly) : base(uid, prefabId_Data, pos, eular, scale, updateType)
        {
            this.matId_Data = matId_Data;
            this.mapPos = mapPos;
         }
        public MapInstance ins
        {
            set { base.ins = value; }
            get { return (MapInstance)base.ins; }
        }


        public string areaName;
        public int matId_Data;
        public Vector3Int mapPos;
        public Material mat=>MapManager.instance.data.materials[matId_Data];

        public List<ItemUnit> itemLst = new List<ItemUnit>();
        public List<CharacterUnit> characterLst = new List<CharacterUnit>();



        public override void Show<T>()
        {
            base.Show<T>();

            if (ins.renderer)
                ins.renderer.material = mat;
            
            foreach (var item in itemLst)
            {
                item.Show<ItemInstance>();
            }
            foreach (var character in characterLst)
            {
                character.Show<CharacterInstance>();
            }
        }
        public override bool VisOn()
        {
            foreach (var item in itemLst)
            {
                item.VisOn();
            }
            foreach (var character in characterLst)
            {
                character.VisOn();
            }
            return base.VisOn();
        }
        public override bool VisOff()
        {
            foreach (var item in itemLst)
            {
                item.VisOff();
            }
            foreach (var character in characterLst)
            {
                character.VisOff();
            }
            return base.VisOff();
        }
        public override bool Hide()
        {
            foreach (var character in characterLst)
            {
                character.Hide();
            }
            foreach (var item in itemLst)
            {
                item.Hide();
            }
            base.Hide();
            return true;
        }
        public void Bind(Unit tar)
        {
            if (tar is ItemUnit item)
            {
                item.map = this;
                itemLst.Add(item);
            }
            else if (tar is CharacterUnit character)
            {
                character.map = this;
                characterLst.Add(character);
            }
            else
            {
                Debug.LogError("bind cant find " + tar.GetType());
            }
        }

        public void Unbind(Unit tar)
        {
            if (tar is ItemUnit item)
            {
                itemLst.Remove(item);
                item.map = null;
            }
            else if (tar is CharacterUnit character)
            {
                characterLst.Remove(character);
                character.map = null;
            }
            else
            {
                Debug.LogError("unbind cant find " + tar.GetType());
            }

        }
        public void Unbind(CharacterUnit tar)
        {
            characterLst.Remove(tar);
            tar.map = null;
        }
        public void UpdateInfo()
        {
            var itemCache = new List<ItemUnit>();
            foreach (var item in itemLst)
            {
                itemCache.Add(item);
            }
            foreach (var item in itemCache)
            {
                item.UpdateInfo();
            }

            var characterCache = new List<CharacterUnit>();
            foreach (var character in characterLst)
            {
                characterCache.Add(character);
            }
            foreach (var character in characterCache)
            {
                character.UpdateInfo();
            }
        }
        public float GetYByPoint(Vector2 selfPos)
        {
            float length = 0;
            if (scale == Vector3.zero)
                return -999;//直接坠落
            if (eular.x != 0)
            {
                length = selfPos.y* (float)Math.Tan(-eular.x * 3.14f / 180);
            } else
            {
                length = selfPos.x * (float)Math.Tan(-eular.z * 3.14f / 180);
            }
            return pos.y + length;
        }

        private void LoadJsonData(JObject jo)
        {
            matId_Data = jo.Get<int>("matId_Data");
            mapPos = jo.Get<Vector3Int>("mapPos");
        }
        public override JObject GetJsonData()
        {
            JObject jo = base.GetJsonData();
            jo.Set("matId_Data", matId_Data);
            jo.Set("mapPos", mapPos);
            return jo;
        }

    }
}
