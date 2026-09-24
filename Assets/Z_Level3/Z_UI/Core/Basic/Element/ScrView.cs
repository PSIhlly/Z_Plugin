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
        private readonly Vector3[] viewportCorners = new Vector3[4];
        private readonly Vector3[] cellCorners = new Vector3[4];

        public RectTransform cell;

        public FillType fiilType;
        private Rect GetRectInContentSpace(RectTransform rectTransform, Vector3[] corners)
        {
            rectTransform.GetWorldCorners(corners);
            Vector3 point = content.InverseTransformPoint(corners[0]);
            float xMin = point.x;
            float xMax = point.x;
            float yMin = point.y;
            float yMax = point.y;
            for (int i = 1; i < corners.Length; i++)
            {
                point = content.InverseTransformPoint(corners[i]);
                xMin = Mathf.Min(xMin, point.x);
                xMax = Mathf.Max(xMax, point.x);
                yMin = Mathf.Min(yMin, point.y);
                yMax = Mathf.Max(yMax, point.y);
            }

            return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
        }

        private Rect GetViewportRectInContentSpace()
        {
            return GetRectInContentSpace(viewport, viewportCorners);
        }

        private Vector2 GetCellSizeInContentSpace()
        {
            return GetRectInContentSpace(cell, cellCorners).size;
        }

        private int GetRowCount(float viewportHeight, float cellHeight)
        {
            return Mathf.Max(1, (int)((viewportHeight - top) / (cellHeight + ySpacing)));
        }

        private int GetColumnCount(float viewportWidth, float cellWidth)
        {
            return Mathf.Max(1, (int)((viewportWidth - left) / (cellWidth + xSpacing)));
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
            Rect viewportRect = GetViewportRectInContentSpace();
            Vector2 layoutCellSize = GetCellSizeInContentSpace();
            float width = viewportRect.width;
            float height = viewportRect.height;
            int rowCnt = GetRowCount(height, layoutCellSize.y);
            int columnCnt = GetColumnCount(width, layoutCellSize.x);
            if (dir == Direction.Vertical)
            {
                foreach (var offset in offsets)
                {
                    offsetMax = Math.Max(offsetMax, offset.x);
                }
                int totRow = (cnt / columnCnt) + (cnt % columnCnt != 0 ? 1 : 0);
                if (DEBUG)
                    Debug.Log("[sv]������" + totRow);

                content.sizeDelta += new Vector2(width - content.rect.width, top + Mathf.Max(totRow, rowCnt) * (layoutCellSize.y + ySpacing) - content.rect.height);


                content.sizeDelta += Vector2.right * offsetMax * 2.5f;

            }
            else
            {
                foreach (var offset in offsets)
                {
                    offsetMax = Math.Min(offsetMax, offset.y);
                }
                int totColumn = (cnt / rowCnt) + (cnt % rowCnt != 0 ? 1 : 0);


                content.sizeDelta += new Vector2(left + Mathf.Max(totColumn, columnCnt) * (layoutCellSize.x + xSpacing) - content.rect.width, height - content.rect.height);


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
            Rect viewportRect = GetViewportRectInContentSpace();
            Rect contentRect = content.rect;
            Vector2 layoutCellSize = GetCellSizeInContentSpace();
            float width = viewportRect.width;
            float height = viewportRect.height;
            int rowCnt = GetRowCount(height, layoutCellSize.y);
            int columnCnt = GetColumnCount(width, layoutCellSize.x);
            Vector3 contentTopLeft = new Vector3(contentRect.xMin, contentRect.yMax, 0);

            var relaPos = new Vector3(0, 0, 0);

            var unitSize = Vector2.zero;


            if (fiilType == FillType.Average)
            {
                if (dir == Direction.Vertical)
                {
                    unitSize = new Vector2(width / columnCnt, layoutCellSize.y);

                }
                else
                {
                    unitSize = new Vector2(layoutCellSize.x, height / rowCnt);
                }

            }
            else
            {
                //not main dir use average
                if (dir == Direction.Vertical)
                {
                    unitSize = layoutCellSize;
                }
                else
                {
                    unitSize = layoutCellSize;
                }

            }

            if (unitSize.y<=0 ||unitSize.x<=0)
            {
                Debug.LogError("cell size error :"+ unitSize);
                return;
            }
                if (dir == Direction.Vertical)
            {

                int curRowId = (int)((contentRect.yMax - viewportRect.yMax) / (unitSize.y + ySpacing));
                needs.Clear();
                while (contentRect.yMax - curRowId * (unitSize.y + ySpacing) - top > viewportRect.yMin)
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


                    relaPos.x += xSpacing * column + left;
                    relaPos.y -= ySpacing * row + top;
                    Add(id, contentTopLeft + relaPos);
                    if (cur != visitId)
                    {
                        break;
                    }
                }
            }
            else
            {
                int curColumnId = (int)((viewportRect.xMin - contentRect.xMin) / (unitSize.x + xSpacing));
                needs.Clear();
                while (contentRect.xMin + curColumnId * (unitSize.x + xSpacing) + left < viewportRect.xMax)
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

                    relaPos.x += xSpacing * column + left;

                    relaPos.y -= ySpacing * row + top;

                    Add(id, contentTopLeft + relaPos);
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
            var rectTransform = obj.GetComponent<RectTransform>();
            rectTransform.sizeDelta = cell.sizeDelta;
            obj.name = id.ToString();
            rectTransform.localPosition = pos + (offsets.Count > id ? offsets[id] : Vector3.zero);

            id2Go[id] = obj;
        }
        private void Clear()
        {
            id2Go.Clear();
            lastShows.Clear();
        }
    }
}
