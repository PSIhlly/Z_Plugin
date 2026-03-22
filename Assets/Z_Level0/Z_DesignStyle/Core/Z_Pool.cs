using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Z_DesignStyle
{
    public abstract class Z_Pool<Obj>
    {
        protected Queue<Obj> pool=new Queue<Obj>();
        protected HashSet<Obj> activeObjs=new HashSet<Obj>();

        public abstract void Clear(Obj obj);
        public virtual void Destroy()
        {

        }
        public abstract Obj New();
        public abstract void Fresh(Obj obj);
        public virtual Obj Get()
        {
            if (pool.Count == 0)
                pool.Enqueue(New());
            var obj=pool.Dequeue();
            activeObjs.Add(obj);
            Fresh(obj);
            return obj;
        }
        public virtual void Push(Obj obj)
        {
            pool.Enqueue(obj);
            activeObjs.Remove(obj);
            Clear(obj);
        }

        public virtual void Clear()
        {
            var lst = new List<Obj>(activeObjs);
            for(int i=0; i< lst.Count; i++)
            {
                if (activeObjs.Contains(lst[i]))
                    Push(lst[i]);
            }
        }
    }
}
