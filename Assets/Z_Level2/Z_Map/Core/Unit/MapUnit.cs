using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Z_Map
{

    public class MapUnit : Unit
    {
        public MapUnit(int uid, GameObject prefab, Vector3 pos, Vector3 eular, Material mat,UpdateType updateType= UpdateType.ShowOnly) : base(uid, prefab, pos, eular, updateType)
        {
            this.mat = mat;
        }
        public MapInstance ins
        {
            set { base.ins = value; }
            get { return (MapInstance)base.ins; }
        }

        public MapUnit[] connectedMapUnits;
        public string areaName;
        public int uniqueId;
        public Material mat;

        public List<ItemUnit> itemLst = new List<ItemUnit>();
        public List<CharacterUnit> characterLst = new List<CharacterUnit>();


        public void Show()
        {
            var tar = MapManager.instance.mapUtilController.CreateInstance(prefab);
            ins = tar.GetComponent<MapInstance>();

            ins.unit = this;
            ins.transform.position = pos;
            ins.renderer.material = mat;
            foreach (var item in itemLst)
            {
                item.Show();
            }
            foreach (var character in characterLst)
            {
                character.Show();
            }
        }

        public void Hide()
        {
            foreach (var character in characterLst)
            {
                character.Hide();
            }
            foreach (var item in itemLst)
            {
                item.Hide();
            }
            MapManager.instance.pool.Push(ins.gameObject);
            ins = null;
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
    }
}
