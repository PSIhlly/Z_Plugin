using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_ByteSerialize;
namespace Z_Map
{
    public enum UpdateType
    {
        ShowOnly,
        Always,

    }
    public abstract class Unit
    {
        public Unit(JObject jo)
        {
            LoadJsonData(jo);
        }
        public Unit(int uid, int prefabId_Data, Vector3 pos, Vector3 eular, Vector3 scale, UpdateType updateType)
        {
            this.uid = uid;
            this.prefabId_Data = prefabId_Data;
            this.pos = pos;
            this.eular = eular;
            this.scale = scale;
            this.updateType = updateType;
        }
        public int uid;
        public int prefabId_Data;
        public GameObject prefab => MapManager.instance.data.prefabs[prefabId_Data];
        public Instance ins;
        public Vector3 pos;
        public Vector3 eular;
        public Vector3 scale;
        public UpdateType updateType = UpdateType.ShowOnly;
        public bool isShowing => ins != null && ins.gameObject != null && ins.gameObject.activeSelf;
        public virtual void Show<T>() where T : Instance
        {

            if (ins == null || ins.gameObject == null)
            {
                var go = MapManager.instance.mapUtilController.CreateInstance(prefab);
                ins = go.GetComponent<T>();
            }

            if (scale == Vector3.zero)
            {
                foreach (var bc in ins.boxColliders)
                {
                    bc.enabled = false;
                }
            }
            else
            {
                foreach (var bc in ins.boxColliders)
                {
                    bc.enabled = true;
                }
            }
            ins.unit = this;
            ins.gameObject.SetActive(true);
            ins.transform.position = pos;
            ins.transform.eulerAngles = eular;
            ins.transform.localScale = scale;
        }
        public virtual bool Hide()
        {
            if (ins == null || ins.gameObject == null)
                return false;
            ins.unit = null;
            MapManager.instance.mapUtilController.DeleteInstance(ins.gameObject, prefab);
            ins = null;

            return true;
        }
        public virtual bool VisOff()
        {
            if (ins == null || ins.gameObject == null)
                return false;
            ins.renderer.gameObject.layer = 6;
            return true;
        }
        public virtual bool VisOn()
        {
            if (ins == null || ins.gameObject == null)
                return false;
            ins.renderer.gameObject.layer = 0;
            return true;
        }

        public virtual JObject GetJsonData()
        {
            JObject jo = new JObject();
            jo.Set("uid", uid);
            jo.Set("pos", pos);
            jo.Set("eular", eular);
            jo.Set("scale", scale);
            jo.Set("updateType", (int)updateType);
            jo.Set("prefabId_Data", prefabId_Data);

            return jo;
        }
        private void LoadJsonData(JObject jo)
        {
            uid = jo.Get<int>("uid");
            pos = jo.Get<Vector3>("pos");
            eular = jo.Get<Vector3>("eular");
            scale = jo.Get<Vector3>("scale");
            updateType = (UpdateType)jo.Get<int>("updateType");
            prefabId_Data = jo.Get<int>("prefabId_Data");
        }
    }
}
