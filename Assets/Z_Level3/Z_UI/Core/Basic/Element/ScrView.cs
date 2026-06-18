using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace Z_Ui.Base
{
    public class ScrView : ScrollRect
    {
        public enum Direction
        {
            Horizon,
            Vertical
        }
        public enum FillType
        {
            None,
            Average,
            Fill
        }
        public Direction dir;

        public Func<int, GameObject> ContainerAdd;
        public Action<GameObject> ContainerDel;
        public float xSpacing;
        public float ySpacing;

        public float top;
        public float left;

        private int cnt;
        private bool inited;
        private List<Vector3> offsets;
        private int visitId;

        public RectTransform cell;

        public FillType fiilType;
        float width => viewport.rect.width;
        float height => viewport.rect.height;
        int rowCnt
        {
            get
            {
                int v = (int)((height - top) / (cell.rect.height + ySpacing));
                if (v == 0)
                    return 1;
                return v;
            }
        }
        int columnCnt
        {
            get
            {
                int v = (int)((width - left) / (cell.rect.width + xSpacing));
                if (v == 0)
                    return 1;
                return v;
            }
        }
        Dictionary<int, GameObject> id2Go = new Dictionary<int, GameObject>();


        HashSet<int> needs = new HashSet<int>();
        List<int> lastShows = new List<int>();
        const bool DEBUG = false;

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
        public void RefreshView(int cnt, List<Vector3> offsets)
        {
            inited = true;
            float offsetMax = 0;
            this.offsets = offsets;
            Clear();
            this.cnt = cnt;
            Vector2 cellSize = new Vector2(cell.rect.width, cell.rect.height);
            Vector2 averageCellSize = new Vector2(width / columnCnt, height / rowCnt);
            if (dir == Direction.Vertical)
            {
                foreach (var offset in offsets)
                {
                    offsetMax = Math.Max(offsetMax, offset.x);
                }
                int totRow = (cnt / columnCnt) + (cnt % columnCnt != 0 ? 1 : 0);
                if (DEBUG)
                    Debug.Log("[sv]������" + totRow);

                content.sizeDelta += new Vector2(width - content.rect.width, top + Mathf.Max(totRow, rowCnt) * (cell.rect.height + ySpacing) - (content.rect.height));


                content.sizeDelta += Vector2.right * offsetMax * 2.5f;

            }
            else
            {
                foreach (var offset in offsets)
                {
                    offsetMax = Math.Min(offsetMax, offset.y);
                }
                int totColumn = (cnt / rowCnt) + (cnt % rowCnt != 0 ? 1 : 0);


                content.sizeDelta += new Vector2(left + Mathf.Max(totColumn, columnCnt) * (cell.rect.width + xSpacing) - content.rect.width, height - content.rect.height);


                content.sizeDelta += Vector2.up * offsetMax * 2.5f;
            }


            if (fiilType == FillType.Fill)
            {
                content.sizeDelta = new Vector2(content.rect.width - width / columnCnt, content.rect.height - height / rowCnt);
            }
            //stretch back
            cell.sizeDelta += cellSize - new Vector2(cell.rect.width, cell.rect.height);

            if (DEBUG)
            {
                Debug.Log("[sv]ԭʼ��ߴ磺" + cell.rect.width + "*" + cell.rect.height + "  ������" + columnCnt + "��" + $",({height} - {top}) / ({cell.rect.height} + {ySpacing})" + "=" + rowCnt + "�У����ߴ磺" + content.rect.width + "*" + content.rect.height);
            }
            UpdateInfo(normalizedPosition);
        }

        void UpdateInfo(Vector2 pos)
        {
            if (!inited)
                return;
            Vector3[] viewPortCorners = new Vector3[4];
            viewport.GetWorldCorners(viewPortCorners);

            Vector3[] contentCorners = new Vector3[4];
            content.GetWorldCorners(contentCorners);

            var relaPos = new Vector3(0, 0, 0);

            var unitSize = Vector2.zero;


            if (fiilType == FillType.Average)
            {
                if (dir == Direction.Vertical)
                {
                    unitSize = new Vector2(width / columnCnt * content.lossyScale.x, (cell.rect.height) * content.lossyScale.y);

                }
                else
                {
                    unitSize = new Vector2((cell.rect.width) * content.lossyScale.x, height / rowCnt * content.lossyScale.y);
                }

            }
            else
            {
                //not main dir use average
                if (dir == Direction.Vertical)
                {
                    unitSize = new Vector2((cell.rect.width) * content.lossyScale.x, (cell.rect.height) * content.lossyScale.y);
                }
                else
                {
                    unitSize = new Vector2((cell.rect.width) * content.lossyScale.x, (cell.rect.height) * content.lossyScale.y);
                }

            }

            if (unitSize.y<=0 ||unitSize.x<=0)
            {
                Debug.LogError("cell size error :"+ unitSize);
                return;
            }
                if (dir == Direction.Vertical)
            {

                int curRowId = (int)((contentCorners[1].y - viewPortCorners[1].y) / (unitSize.y + ySpacing));
                needs.Clear();
                while (contentCorners[1].y - curRowId * (unitSize.y + ySpacing) - top > viewPortCorners[0].y)
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
                visitId++;
                var cur = visitId;
                foreach (var id in needs)
                {
                    int row = id / columnCnt;
                    int column = id % columnCnt;
                    relaPos = new Vector3((column + 0.5f) * unitSize.x, -(row + 0.5f) * unitSize.y, 0);


                    relaPos.x += (xSpacing * (column) + left) * content.lossyScale.x;
                    relaPos.y -= (ySpacing * (row) + top) * content.lossyScale.y;
                    Add(id, contentCorners[1] + relaPos);
                    if (cur != visitId)
                    {
                        break;
                    }
                }
            }
            else
            {
                int curColumnId = (int)((viewPortCorners[1].x - contentCorners[1].x) / (unitSize.x + xSpacing));
                needs.Clear();
                while (contentCorners[1].x + curColumnId * (unitSize.x + xSpacing) + left < viewPortCorners[2].x)
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
                visitId++;
                var cur = visitId;
                foreach (var id in needs)
                {

                    int column = id / rowCnt;
                    int row = id % rowCnt;
                    relaPos = new Vector3((column + 0.5f) * unitSize.x, -(row + 0.5f) * unitSize.y, 0);

                    relaPos.x += (xSpacing * (column) + left) * content.lossyScale.x;

                    relaPos.y -= (ySpacing * (row) + top) * content.lossyScale.y;

                    Add(id, contentCorners[1] + relaPos);
                    if (cur != visitId)
                    {
                        break;
                    }
                }
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
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
            //reset shape
            obj.GetComponent<RectTransform>().sizeDelta = cell.sizeDelta;
            obj.name = id.ToString();
            obj.transform.position = pos + (offsets.Count > id ? offsets[id] : Vector3.zero);

            id2Go[id] = obj;
        }
        private void Clear()
        {
            id2Go.Clear();
            lastShows.Clear();
        }
    }
}
