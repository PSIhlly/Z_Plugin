using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_DesignStyle;

namespace Z_Ui.Base
{
    public class UiContainer<T> where T : UiCtrl,new()
    {
        public GameObject ori;

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
        public UiContainer(GameObject ori)
        {
            ori.SetActive(false);
            this.ori = ori;
            uiPool = new UiPool()
            {
                ori= ori
            };
            curUis = new List<GameObject>();
        }

        public virtual void Clear()
        {
            foreach(var ui in curUis)
            {
                uiPool.Push(ui);
            }
            curUis.Clear();
            paramLst.Clear();
        }
        public virtual void DelReal(GameObject go)
        {
            uiPool.Push(go);
            curUis.Remove(go);
        }
        public GameObject AddReal(UiParam param = null)
        {
            var holder = uiPool.Get().GetComponent<UiHolder>();
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
        public virtual void Refresh()
        {
            foreach(var param in paramLst)
            {
                AddReal(param);
            }
        }
    }
}