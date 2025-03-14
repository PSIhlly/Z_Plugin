using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace Z_Ui.Base
{
    public class ScrView : ScrollRect
    {
        public enum FillType
        {
            None,
            Average,
            Fill
        }
        public RectTransform viewPort;

        public Func<int, GameObject> ContainerAdd;
        public Action<GameObject> ContainerDel;

        private int cnt;
        private bool inited;

        public RectTransform cell;

        public FillType fiilType;
        float width => viewPort.rect.width;
        float height => viewPort.rect.height;
        int rowCnt
        {
            get
            {
                int v = (int)((height) / cell.rect.height);
                if (v == 0)
                    return 1;
                return v;
            }
        }
        int columnCnt
        {
            get
            {
                int v = (int)((width) / cell.rect.width);
                if (v == 0)
                    return 1;
                return v;
            }
        }
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
            Vector2 cellSize = new Vector2(cell.rect.width, cell.rect.height);
            Vector2 fakeCellSize = new Vector2(width / columnCnt, height / rowCnt);
            if (vertical)
            {
                int totRow = (cnt / columnCnt) + (cnt % columnCnt != 0 ? 1 : 0);
                if (fiilType == FillType.Average)
                {

                    content.sizeDelta += new Vector2(width - content.rect.width, Mathf.Max(totRow, rowCnt) * fakeCellSize.y - (content.rect.height));
                }
                else
                {
                    content.sizeDelta += new Vector2(width - content.rect.width, Mathf.Max(totRow, rowCnt) * cell.rect.height - (content.rect.height));
                }
            }
            else
            {
                int totColumn = (cnt / rowCnt) + (cnt % rowCnt != 0 ? 1 : 0);
                if (fiilType == FillType.Average)
                {
                    content.sizeDelta += new Vector2(Mathf.Max(totColumn, columnCnt) * fakeCellSize.x - content.rect.width, height - content.rect.height);

                }
                else
                {
                    content.sizeDelta += new Vector2(Mathf.Max(totColumn, columnCnt) * cell.rect.width - content.rect.width, height - content.rect.height);
                }
            }


            if (fiilType == FillType.Fill)
            {
                content.sizeDelta = new Vector2(content.rect.width - width / columnCnt, content.rect.height - height / rowCnt);
            }
            //stretch back
            cell.sizeDelta += cellSize - new Vector2(cell.rect.width, cell.rect.height);

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

            var relaPos = new Vector3(0, 0, 0);


            if (vertical)
            {
                int curRowId = (int)((contentCorners[1].y - viewPortCorners[1].y) / cell.rect.height / content.lossyScale.y);
                needs.Clear();
                while (contentCorners[1].y - curRowId * cell.rect.height * content.lossyScale.y > viewPortCorners[0].y)
                {
                    for (int i = 0; i < columnCnt; i++)
                    {
                        int id = curRowId * columnCnt + i;
                        if (id >= cnt || id < 0)
                            continue;
                        needs.Add(id);
                    }
                    curRowId++;
                }
                UpdateDic();
                foreach (var id in needs)
                {
                    int row = id / columnCnt;
                    int column = id % columnCnt;

                    if (fiilType == FillType.Average)
                    {
                        relaPos = new Vector3((column + 0.5f) * width / columnCnt * content.lossyScale.x, -(row + 0.5f) * height / rowCnt * content.lossyScale.y, 0);

                    }
                    else
                    {
                        relaPos = new Vector3((column + 0.5f) * cell.rect.width * content.lossyScale.x, -(row + 0.5f) * cell.rect.height * content.lossyScale.y, 0);

                    }
                    Add(id, contentCorners[1] + relaPos);
                }
            }
            else
            {
                int curColumnId = (int)((viewPortCorners[1].x - contentCorners[1].x) / cell.rect.width / content.lossyScale.x);
                needs.Clear();
                while (contentCorners[1].x + curColumnId * cell.rect.width * content.lossyScale.x < viewPortCorners[2].x)
                {
                    for (int i = 0; i < rowCnt; i++)
                    {
                        int id = curColumnId * rowCnt + i;
                        if (id >= cnt || id < 0)
                            continue;
                        needs.Add(id);
                    }
                    curColumnId++;
                }
                UpdateDic();
                foreach (var id in needs)
                {

                    int column = id / rowCnt;
                    int row = id % rowCnt;
                    if (fiilType == FillType.Average)
                    {
                        relaPos = new Vector3((column + 0.5f) * width / columnCnt * content.lossyScale.x, -(row + 0.5f) * height / rowCnt * content.lossyScale.y, 0);

                    }
                    else
                    {
                        relaPos = new Vector3((column + 0.5f) * cell.rect.width * content.lossyScale.x, -(row + 0.5f) * cell.rect.height * content.lossyScale.y, 0);

                    }
                    Add(id, contentCorners[1] + relaPos);
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
