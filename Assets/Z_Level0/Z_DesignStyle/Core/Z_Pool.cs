using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Z_DesignStyle
{
    public abstract class Z_Pool<Obj>
    {
        protected Queue<Obj> pool=new Queue<Obj>();

        public abstract void Clear(Obj obj);
        public abstract void Destroy();
        public abstract Obj New();
        public abstract void Fresh(Obj obj);
        public Obj Get()
        {
            if (pool.Count == 0)
                pool.Enqueue(New());
            var obj=pool.Dequeue();
            Fresh(obj);
            return obj;
        }
        public void Push(Obj obj)
        {
            pool.Enqueue(obj);
            Clear(obj);
        }

    }
}
