using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Z_DesignStyle
{
    public class Z_DoubleDictionary
    {
        public class DoubleDictionary<T1, T2>
        {
            Dictionary<T1, List<T2>> d1 = new Dictionary<T1, List<T2>>();
            Dictionary<T2, List<T1>> d2 = new Dictionary<T2, List<T1>>();

            public Dictionary<T2, List<T1>> GetDicT2()
            {
                return d2;
            }
            public Dictionary<T1, List<T2>> GetDicT1()
            {
                return d1;
            }
            public List<T1> Get(T2 key)
            {
                if (!d2.ContainsKey(key))
                    d2[key] = new List<T1>();

                return d2[key];
            }
            public List<T2> Get(T1 key)
            {
                if (!d1.ContainsKey(key))
                    d1[key] = new List<T2>();

                return d1[key];
            }
            public void Move(T1 t1, T2 cur, T2 old)
            {
                Del(t1,old);
                Add(t1, cur);
            }
            public void Move(T1 t1, T2 cur)
            {
                if (d1.ContainsKey(t1) && d1[t1].Contains(cur))
                    return;
                Del(t1);
                Add(t1, cur);
            }
            public void Move(T2 t2, T1 cur, T1 old)
            {
                Del(old,t2);
                Add(cur,t2);
            }
            public void Move(T2 t2, T1 cur)
            {
                if (d2.ContainsKey(t2) && d2[t2].Contains(cur))
                    return;
                Del(t2);
                Add(cur, t2);
            }
            public void Add(T1 t1, T2 t2)
            {
                if (!d1.ContainsKey(t1))
                {
                    d1[t1] = new List<T2>();
                }
                d1[t1].Add(t2);
                if (!d2.ContainsKey(t2))
                {
                    d2[t2] = new List<T1>();
                }
                d2[t2].Add(t1);
            }
            public void Del(T1 t1)
            {
                if (d1.ContainsKey(t1))
                {
                    foreach (var t2 in d1[t1])
                    {
                        d2[t2].Remove(t1);
                    }
                    d1.Remove(t1);
                }
            }
            public void Del(T1 t1,T2 t2)
            {
                if (d1.ContainsKey(t1))
                {
                    if (d1[t1].Contains(t2))
                        d1[t1].Remove(t2);
                }
                if (d2.ContainsKey(t2))
                {
                    if (d2[t2].Contains(t1))
                        d2[t2].Remove(t1);
                }
            }
            public void Del(T2 t2)
            {
                if (d2.ContainsKey(t2))
                {
                    foreach (var t1 in d2[t2])
                    {
                        d1[t1].Remove(t2);
                    }
                    d2.Remove(t2);
                }
            }
          
            public bool Contains(T1 t1, T2 t2)
            {
                return d1.ContainsKey(t1) && d1[t1].Contains(t2);
            }

            public bool Contains(T2 t2, T1 t1)
            {
                return d2.ContainsKey(t2) && d2[t2].Contains(t1);
            }
            public void Clear()
            {

                d1.Clear(); d2.Clear();
            }

        }

    }
}
