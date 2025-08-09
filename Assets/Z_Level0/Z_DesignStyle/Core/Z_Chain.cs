using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Z_DesignStyle
{
    public class Z_Chain
    {
        public class Chain
        {
            public int cnt;
            public Chain(int cnt)
            {
                this.cnt = cnt;
                id2ChainItem = new Dictionary<int, IdChainItem>(cnt);
                id2ChainItem[1] = new IdChainItem(1);
                for (int i = 2; i <= cnt; i++)
                {
                    id2ChainItem[i] = new IdChainItem(i);
                    id2ChainItem[i].pre = id2ChainItem[i - 1];
                    id2ChainItem[i - 1].nxt = id2ChainItem[i];
                }
                chainHead = id2ChainItem[1];
            }
            class IdChainItem
            {
                public IdChainItem pre;
                public IdChainItem nxt;
                public int v;
                public IdChainItem(int v)
                {
                    this.v = v;
                }
            }
            private Dictionary<int, IdChainItem> id2ChainItem;
            private IdChainItem chainHead;
            public int PeekId()
            {
                if (chainHead == null)
                    return -1;
                return chainHead.v;
            }
            public int GetId()
            {
                if (chainHead == null)
                    return -1;
                int v = chainHead.v;

                var newHead = chainHead.nxt;

                chainHead.pre = null;
                chainHead.nxt = null;

                chainHead = newHead;
                
                return v;
            }
            public void PopId(int v)
            {
                if(!id2ChainItem.ContainsKey(v))
                {
                    return;
                }
                if(id2ChainItem[v] == chainHead)
                {
                    chainHead = chainHead.nxt;
                    chainHead.pre = null;
                }
                else
                {
                    if(id2ChainItem[v].nxt!=null)
                        id2ChainItem[v].nxt.pre = id2ChainItem[v].pre;
                    if (id2ChainItem[v].pre != null)
                        id2ChainItem[v].pre.nxt = id2ChainItem[v].nxt;
                }

                id2ChainItem[v].pre = null;
                id2ChainItem[v].nxt = null;
            }
            public void PushId(int id)
            {
                if(!id2ChainItem.ContainsKey(id))
                {
                    id2ChainItem[id] = new IdChainItem(id);
                }
                id2ChainItem[id].pre = null;
                id2ChainItem[id].nxt = null;
                if (chainHead == null)
                {
                    chainHead = id2ChainItem[id];
                }
                else
                {
                    if (chainHead.nxt == null)
                    {
                        chainHead.nxt = id2ChainItem[id];
                        id2ChainItem[id].pre = chainHead;
                    }
                    else
                    {
                        chainHead.nxt.pre = id2ChainItem[id];
                        id2ChainItem[id].nxt = chainHead.nxt;
                        chainHead.nxt = id2ChainItem[id];
                        id2ChainItem[id].pre = chainHead;
                    }
                }
            }
            public void Clear()
            {
                id2ChainItem[1].pre = null;
                for (int i = 2; i <= cnt; i++)
                {
                    id2ChainItem[i].pre = id2ChainItem[i - 1];
                    id2ChainItem[i - 1].nxt = id2ChainItem[i];
                }
                id2ChainItem[cnt].nxt = null;

                chainHead = id2ChainItem[1];
            }

        }
      
    }
}
