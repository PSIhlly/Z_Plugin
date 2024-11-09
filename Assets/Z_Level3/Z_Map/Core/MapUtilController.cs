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
            var size = _super.data.size;
            return (pos.x >= 0 && pos.x < size.x) && (pos.y >= 0 && pos.y < size.y) && (pos.z >= 0 && pos.z < size.z);
        }
        public bool InArea(Vector3 pos)
        {
            var size = _super.data.size;
            return (pos.x >= -_super.data.mapUnitSize.x/2 && pos.x < size.x + _super.data.mapUnitSize.x / 2) && (pos.y >= -_super.data.mapUnitSize.y / 2 && pos.y < size.y+ _super.data.mapUnitSize.y / 2) && (pos.z >= -_super.data.mapUnitSize.z / 2 && pos.z < size.z+_super.data.mapUnitSize.z / 2);
        }
        public Vector3Int RealPos2MapPos(Vector3 pos)
        {
            pos = Z_Math.Graph.ElementwiseDivide(pos, _super.data.mapUnitSize);
            return Z_Math.Graph.GetVector3Int(pos);
        }
        public Vector3 MapPos2RealPos(Vector3Int pos)
        {
            return Z_Math.Graph.ElementwiseMultiply(pos, _super.data.mapUnitSize);
        }
        public GameObject GetPrefab(string name)
        {
            foreach (var pool in _super.pools)
                if (name == pool.prefab.name)
                {
                    return pool.prefab;
                }
            return null;
        }
        public GameObject CreateInstance(GameObject tar)
        {
            foreach(var pool in _super.pools)
            if(tar==pool.prefab)
            {
                return pool.Get();
            }
            var go = GameObject.Instantiate(tar);
            go.transform.SetParent(_super.mainGo.transform);
            return go;
        }
        public void DeleteInstance(GameObject tar,GameObject proto)
        {
            foreach (var pool in _super.pools)
                if (proto == pool.prefab)
                {
                    pool.Push(tar);
                    return;
                }

            tar.SetActive(false);
        }

        public Vector3Int GetClosestInArea(Vector3Int pos)
        {
            var size = _super.data.size;
            if (pos.x < 0) pos.x = 0;
            if (pos.x >= size.x) pos.x = size.x-1;
            if (pos.y < 0) pos.y = 0;
            if (pos.y >= size.y) pos.y = size.y-1;
            if (pos.z < 0) pos.z = 0;
            if (pos.z >= size.z) pos.z = size.z-1;
            return pos;
        }
        public Vector3 GetClosestInArea(Vector3 pos)
        {
            var size = _super.data.size;
            var uSize = _super.data.mapUnitSize;
            if (pos.x < -uSize.x / 2) pos.x = -uSize.x / 2;
            if (pos.x >= size.x + uSize.x / 2) pos.x = size.x + uSize.x / 2;
            if (pos.y < -uSize.y / 2) pos.y = -uSize.y / 2;
            if (pos.y >= size.y+ uSize.y / 2) pos.y = size.y+ uSize.y / 2;
            if (pos.z < -uSize.z / 2) pos.z = -uSize.z / 2;
            if (pos.z >= size.z+ uSize.z / 2) pos.z = size.z+ uSize.z / 2;
            return pos;
        }
       
    }
}
