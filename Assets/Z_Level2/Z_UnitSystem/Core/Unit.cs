using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_ByteSerialize;
namespace Z_UnitSystem
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
        public Unit(int uid, int prefabId_Data, Vector3 pos, Vector3 euler, Vector3 scale, UpdateType updateType)
        {
            this.uid = uid;
            this.prefabId_Data = prefabId_Data;
            this.pos = pos;
            this.euler = euler;
            this.scale = scale;
            this.updateType = updateType;
        }
        public int uid;
        public int prefabId_Data;
        public GameObject prefab => InstancePoolManager.instance.prefabs[prefabId_Data];
        public Instance ins;
        public Vector3 pos;
        public Vector3 euler;
        public Vector3 scale;
        public UpdateType updateType = UpdateType.ShowOnly;

        public List<Unit> subUnits=new List<Unit>();
        public Unit superUnit;
        private int lastUpdateFrame;
        public bool isShowing => ins != null && ins.gameObject != null && ins.gameObject.activeSelf;

        public bool isVising => isShowing&&ins.renderer.enabled;

        public virtual Type GetInsType()
        {
            return typeof(Instance);
        }

        public virtual void Show() 
        {

            if (ins == null || ins.gameObject == null)
            {
                var go = InstancePoolManager.instance.CreateInstance(prefab);
                ins = (Instance)go.GetComponent(GetInsType());
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
            ins.transform.eulerAngles = euler;
            ins.transform.localScale = scale;

            VisOn();
            foreach (var unit in subUnits)
            {
                unit.Show();
            }
        }
        public virtual bool Hide()
        {
            if (ins == null || ins.gameObject == null)
                return false;
            VisOff();
            ins.unit = null;
            InstancePoolManager.instance.DeleteInstance(ins.gameObject, prefab);
            ins = null;
            foreach (var unit in subUnits)
            {
                unit.Hide();
            }
            return true;
        }
        public virtual bool VisOff()
        {
            if (ins == null || ins.gameObject == null)
                return false;
            ins.renderer.enabled = false;
            foreach (var unit in subUnits)
            {
                unit.VisOff();
            }
            return true;
        }
        public virtual bool VisOn()
        {
            if (ins == null || ins.gameObject == null)
                return false;
            ins.renderer.enabled = true;

            foreach (var unit in subUnits)
            {
                unit.VisOn();

            }
            return true;
        }

        public virtual void Bind(Unit tar)
        {
            tar.superUnit = this;
            subUnits.Add(tar);
        }

        public virtual void Unbind(Unit tar)
        {
            if (subUnits.Contains(tar))
            {
                tar.superUnit = null;
                subUnits.Remove(tar);
            }

        }
        public void UpdateActive()
        {
           
            if (superUnit == null || !superUnit.isShowing)
            {
                if (isShowing)
                    Hide();
            }
            else if (!isShowing)
            {
                Show();
            }
        }

        public virtual void UpdateInfo()
        {
            if (lastUpdateFrame == Time.frameCount)
                return;
            lastUpdateFrame = Time.frameCount;

            var unitCache = new List<Unit>();
            foreach (var unit in subUnits)
            {
                unitCache.Add(unit);
            }
            foreach (var unit in unitCache)
            {
                unit.UpdateInfo();
            }
        }
        public virtual JObject GetJsonData()
        {
            JObject jo = new JObject();
            jo.Set("uid", uid);
            jo.Set("pos", pos);
            jo.Set("eular", euler);
            jo.Set("scale", scale);
            jo.Set("updateType", (int)updateType);
            jo.Set("prefabId_Data", prefabId_Data);

            return jo;
        }
        private void LoadJsonData(JObject jo)
        {
            uid = jo.Get<int>("uid");
            pos = jo.Get<Vector3>("pos");
            euler = jo.Get<Vector3>("eular");
            scale = jo.Get<Vector3>("scale");
            updateType = (UpdateType)jo.Get<int>("updateType");
            prefabId_Data = jo.Get<int>("prefabId_Data");
        }
    }
}
