using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Printing;
using UnityEngine;
using Z_ByteSerialize;
using Z_UnitSystem.Form;

namespace Z_UnitSystem
{
    public enum UpdateType
    {
        ShowOnly,
        Always,
    }

    public class Unit
    {
        public Unit(UnitForm.Data data)
        {
            _data = data;
        }
        protected readonly UnitForm.Data _data;
        public UnitForm.Data data => (UnitForm.Data)_data;

        public GameObject prefab => InstancePoolManager.instance.GetPrefab(_data.prefabName);

        public Instance ins;

        public List<Unit> subUnits=new List<Unit>();
        public Unit superUnit;

        private int lastUpdateFrame;
        public bool isShowing => ins != null && ins.gameObject != null && ins.gameObject.activeSelf;

        public bool isVising => isShowing&&ins.GetComponent<Renderer>().enabled;

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

            if (_data.scale==Vector3.zero)
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
            ins.transform.position = _data.pos;
            ins.transform.eulerAngles = _data.euler;
            ins.transform.localScale = _data.scale;

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
            foreach (var renderer in ins.renderers)
            {
                renderer.SetPropertyBlock(null);
            }
            ins.unit = null;
            InstancePoolManager.instance.DeleteInstance(ins.gameObject, ins.gameObject);
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
            ins.VisOff();
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
            ins.VisOn();

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
        public void SubUpdateActive()
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
        public List<Unit> GetAllSubUnits(bool recursion=false)
        {
            List<Unit> units = new List<Unit>();
            foreach(var sub in subUnits)
            {
                units.Add(sub);
                if(recursion)
                {
                    units.AddRange(sub.GetAllSubUnits());
                }
            }
            return units;
        }

        public virtual void UpdateInfo()
        {
            if (lastUpdateFrame == Time.frameCount)
                return;
            lastUpdateFrame = Time.frameCount;
            for(int i= subUnits.Count-1; i>=0;i--)
            {
                subUnits[i].UpdateInfo();
            }
        }
        public virtual void Remove()
        {
            var lst = data.unit.GetAllSubUnits();
            foreach (var unit in lst)
            {
                unit.Remove();
            }

            if (superUnit != null)
                superUnit.Unbind(this);
            VisOff();
            Hide();
            UnitForm.RemoveData(data.uid);
        }
    }
}
