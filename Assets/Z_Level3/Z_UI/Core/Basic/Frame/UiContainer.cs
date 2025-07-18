using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_DesignStyle;

namespace Z_Ui.Base
{
    public class UiContainer<T> where T : UiCtrl,new()
    {
        public GameObject ori;
        
        private bool cycle;
        public List<UiParam> paramLst = new List<UiParam>();
        UiPool uiPool;
        
        protected class UiPool : Z_Pool<GameObject>
        {
            public GameObject ori;
            public override void Clear(GameObject obj)
            {
                obj.SetActive(false);
            }
            public override void Destroy()
            {
            }
            public override void Fresh(GameObject obj)
            {
            }
            public override GameObject New()
            {
                var newGo= GameObject.Instantiate(ori, ori.transform.parent);
                newGo.SetActive(false);
                return newGo;
            }
        }
        List<GameObject> curUis;
        public UiContainer(GameObject ori,bool cycle=true)
        {
            ori.SetActive(false);
            this.ori = ori;
            if(cycle)
            {
                uiPool = new UiPool()
                {
                    ori = ori
                };
            }
            
            curUis = new List<GameObject>();
        }

        public virtual void Clear()
        {
            foreach(var ui in curUis)
            {
                if(cycle)
                {
                    uiPool.Push(ui);
                }else
                {
                    GameObject.Destroy(ui);
                }
            }
            curUis.Clear();
            paramLst.Clear();
        }
        public virtual void DelReal(GameObject go)
        {
            if (cycle)
            {
                uiPool.Push(go);
            }else
            {
                GameObject.Destroy(go);
            }
            curUis.Remove(go);
        }
        public GameObject AddReal(UiParam param = null)
        {
            UiHolder holder = null;
            if (cycle)
            {
                holder = uiPool.Get().GetComponent<UiHolder>();
            }
            else
            {
                holder = GameObject.Instantiate(ori,ori.transform.parent).GetComponent<UiHolder>();
            }
            var ctrl = holder.ctrl;

            if (!holder.binded)
            {
                ctrl = new T();
                holder.SubUiBind(ctrl);
            }
            ctrl.SetParam(param);
            holder.gameObject.SetActive(true);
            curUis.Add(holder.gameObject);
            return holder.gameObject;
        }
        public virtual void Add(UiParam param=null)
        {
            paramLst.Add(param);
        }
        public virtual void Del(int id)
        {
            paramLst.RemoveAt(id);
        }
        public virtual void Refresh(List<Vector3> offsets = null)
        {
            if (offsets == null)
                offsets = new List<Vector3>();
            for(int i=0;i< paramLst.Count;i++)
            {
                var go=AddReal(paramLst[i]);
            }
        }
    }
}