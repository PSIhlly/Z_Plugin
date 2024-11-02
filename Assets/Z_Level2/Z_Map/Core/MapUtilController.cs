using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_DesignStyle;

namespace Z_Map
{
    public class MapUtilController:Z_Controller<MapManager>
    {
        public bool InArea(Vector3Int pos)
        {
            var size = GetSize();
            return (pos.x >= 0 && pos.x < size.x) && (pos.y >= 0 && pos.y < size.y) && (pos.z >= 0 && pos.z < size.z);
        }
        public Vector3Int RealPos2MapPos(Vector3 pos)
        {
            return new Vector3Int((int)Math.Round(pos.x), (int)Math.Round(pos.y), (int)Math.Round(pos.z));
        }
        public Vector3 MapPos2RealPos(Vector3Int pos)
        {
            return pos;
        }
        public GameObject CreateInstance(GameObject tar)
        {
            if(tar==_super.pool.prefab)
            {
                return _super.pool.Get();
            }
            var go = GameObject.Instantiate(tar);
            go.transform.SetParent(_super.transform);
            return go;
        }

        public Vector3Int GetClosestInArea(Vector3Int pos)
        {
            var size = GetSize();
            if (pos.x < 0) pos.x = 0;
            if (pos.x >= size.x) pos.x = size.x;
            if (pos.y < 0) pos.y = 0;
            if (pos.y >= size.y) pos.y = size.y;
            if (pos.z < 0) pos.z = 0;
            if (pos.z >= size.z) pos.z = size.z;
            return pos;
        }

        public Vector3Int GetSize()
        {
            var units = _super.maps;
            if (units == null)
                return Vector3Int.zero;
            return new Vector3Int(units.GetLength(0), units.GetLength(1), units.GetLength(2));
        }
    }
}
