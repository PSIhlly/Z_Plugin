using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace Z_Ui.Base
{
    public class ScrView : ScrollRect
    {
        public RectTransform viewPort;

        public Func<int, GameObject> ContainerAdd;
        public Action<GameObject> ContainerDel;

        private int cnt;
        private bool inited;

        public Vector2 cellSize;
        float width => viewPort.rect.width;
        float height => viewPort.rect.height;
        int rowCnt => (int)((height) / cellSize.y);
        int columnCnt => (int)((width) / cellSize.x);

        Dictionary<int, GameObject> id2Go = new Dictionary<int, GameObject>();


        HashSet<int> needs = new HashSet<int>();
        List<int> lastShows = new List<int>();

        protected override void Start()
        {
            onValueChanged.AddListener(UpdateInfo);
        }
        /*public void ResetView(UiContainer<T> container) 
        {
            container= container;
            RefreshView();
            UpdateInfo(Vector2.zero);
        }*/
        public void RefreshView(int cnt)
        {
            inited = true;

            Clear();
            this.cnt = cnt;

            if (vertical)
            {
                int totRow = (cnt / columnCnt) + (cnt % columnCnt != 0 ? 1 : 0);
                content.sizeDelta = new Vector2(width, Mathf.Max(totRow, rowCnt) * cellSize.y);
            }
            else
            {
                int totColumn = (cnt / rowCnt) + (cnt % rowCnt != 0 ? 1 : 0);
                content.sizeDelta = new Vector2(Mathf.Max(totColumn, columnCnt) * cellSize.x, height);
            }
            UpdateInfo(normalizedPosition);
        }

        void UpdateInfo(Vector2 pos)
        {
            if (!inited)
                return;

            Vector3[] viewPortCorners = new Vector3[4];
            viewPort.GetWorldCorners(viewPortCorners);

            Vector3[] contentCorners = new Vector3[4];
            content.GetWorldCorners(contentCorners);
            if (vertical)
            {
                int curRowId = (int)((contentCorners[1].y - viewPortCorners[1].y) / cellSize.y);
                needs.Clear();
                while (contentCorners[1].y - curRowId * cellSize.y > viewPortCorners[0].y)
                {
                    for (int i = 0; i < columnCnt; i++)
                    {
                        int id = curRowId * columnCnt + i;
                        if (id >= cnt || id < 0)
                            continue;
                        needs.Add(curRowId * columnCnt + i);
                    }
                    curRowId++;
                }
                UpdateDic();
                foreach (var id in needs)
                {
                    int row = id / columnCnt;
                    int column = id % columnCnt;
                    Add(id, contentCorners[1] + new Vector3((column + 0.5f) * cellSize.x, -(row + 0.5f) * cellSize.y, 0));
                }
            }
            else
            {
                int curColumnId = (int)((viewPortCorners[1].x - contentCorners[1].x) / cellSize.x);
                needs.Clear();
                while (contentCorners[1].x + curColumnId * cellSize.x < viewPortCorners[2].x)
                {
                    for (int i = 0; i < rowCnt; i++)
                    {
                        int id = curColumnId * rowCnt + i;
                        if (id >= cnt || id < 0)
                            continue;
                        needs.Add(curColumnId * rowCnt + i);
                    }
                    curColumnId++;
                }
                UpdateDic();
                foreach (var id in needs)
                {
                    int column = id / columnCnt;
                    int row = id % rowCnt;
                    Add(id, contentCorners[1] + new Vector3((column + 0.5f) * cellSize.x, -(row + 0.5f) * cellSize.y, 0));
                }
            }
        }
        private void UpdateDic()
        {
            lastShows.Clear();
            foreach (var k in id2Go.Keys)
            {
                lastShows.Add(k);
            }
            for (int i = 0; i < lastShows.Count; i++)
            {
                int k = lastShows[i];
                if (needs.Contains(k))
                    continue;

                var go = id2Go[k];
                ContainerDel(go);
                id2Go.Remove(k);
            }
        }

        private void Add(int id, Vector3 pos)
        {
            if (id2Go.ContainsKey(id))
                return;
            var obj = ContainerAdd(id);
            obj.GetComponent<RectTransform>().sizeDelta = new Vector2(cellSize.x, cellSize.y);
            obj.name = id.ToString();
            obj.transform.position = pos;
            id2Go[id] = obj;
        }
        private void Clear()
        {
            id2Go.Clear();
            lastShows.Clear();
        }
    }
}
