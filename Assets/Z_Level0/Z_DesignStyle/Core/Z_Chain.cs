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
            private int localCnt;
            public Chain(int cnt)
            {
                this.cnt = cnt;
                localCnt = 1;
                id2ChainItem = new Dictionary<int, IdChainItem>(cnt);
                id2ChainItem[1] = new IdChainItem(1);
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
                GenerateNewHead();
                return v;
            }
            private void GenerateNewHead()
            {
                IdChainItem newHead = null;
                while (localCnt < cnt)
                {
                    ++localCnt;
                    if (id2ChainItem.ContainsKey(localCnt))
                        continue;

                    id2ChainItem[localCnt] = new IdChainItem(localCnt);
                    newHead = id2ChainItem[localCnt];
                    break;
                }

                if (newHead == null)
                {
                    newHead = chainHead.nxt;
                }
                newHead.pre = null;
                chainHead.pre = null;
                chainHead.nxt = null;

                chainHead = newHead;
            }
            public void PopId(int v)
            {
                if (!id2ChainItem.ContainsKey(v))
                {
                    id2ChainItem[v] = new IdChainItem(v);
                    return;
                }
                if (id2ChainItem[v] == chainHead)
                {
                    GenerateNewHead();
                }
                else
                {
                    if (id2ChainItem[v].nxt != null)
                        id2ChainItem[v].nxt.pre = id2ChainItem[v].pre;
                    if (id2ChainItem[v].pre != null)
                        id2ChainItem[v].pre.nxt = id2ChainItem[v].nxt;
                }

                id2ChainItem[v].pre = null;
                id2ChainItem[v].nxt = null;
            }
            public void PushId(int id)
            {
                if (!id2ChainItem.ContainsKey(id))
                {
                    id2ChainItem[id] = new IdChainItem(id);
                }
                if (id2ChainItem[id].nxt != null || id2ChainItem[id].pre != null || id2ChainItem[id] == chainHead)//double push
                {
                    return;
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
                var last = id2ChainItem[1];
                foreach (var chain in id2ChainItem.Values)
                {
                    if (chain == last)
                        continue;

                    chain.pre = last;
                    last.nxt = chain;
                    last = chain;
                }
                last.nxt = null;

                chainHead = id2ChainItem[1];
            }
            public void Debug(int limit = 200)
            {
                var cur = chainHead;
                int cnt = 0;
                string res = "";
                while (cur != null && cnt < limit)
                {
                    cnt++;

                    res += cnt + "  :" + cur.v + " " + (id2ChainItem[cur.v] == cur) + " pre:" + (id2ChainItem[cur.v].pre != null ? id2ChainItem[cur.v].pre.v : "") + " nxt:" + (id2ChainItem[cur.v].nxt != null ? id2ChainItem[cur.v].nxt.v : "") + " \n";
                    cur = cur.nxt;
                }

                UnityEngine.Debug.Log(res);
            }

        }

    }
}
