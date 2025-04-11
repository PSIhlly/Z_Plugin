using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_DesignStyle;

namespace Z_Map
{
    public class MapUtilController : Z_Controller<MapManager>
    {
        public MapUtilController(MapManager super) : base(super)
        {
        }

        public bool InArea((int, int, int) pos)
        {
            return _super.data.maps.ContainsKey((pos.Item1, pos.Item2, pos.Item3));
        }
        public bool InArea(Vector3Int pos)
        {
            return _super.data.maps.ContainsKey((pos.x, pos.y, pos.z));
        }
        public bool InArea(Vector3 pos)
        {
            int x = (int)Math.Round(pos.x / _super.data.mainData.mapUnitSize.x);
            int y = (int)(pos.y / _super.data.mainData.mapUnitSize.y);
            int z = (int)Math.Round(pos.z / _super.data.mainData.mapUnitSize.z);

            return _super.data.maps.ContainsKey((x, y, z));
        }
        public Vector3Int RealPos2MapPos(Vector3 pos)
        {
            pos = Z_Math.Graph.ElementwiseDivide(pos, _super.data.mainData.mapUnitSize);
            return new Vector3Int((int)Math.Round(pos.x), (int)(pos.y), (int)Math.Round(pos.z));
        }
        public Vector3 MapPos2RealPos(Vector3Int pos)
        {
            return Z_Math.Graph.ElementwiseMultiply(pos, _super.data.mainData.mapUnitSize);
        }

        public Vector3Int GetClosestInArea(Vector3Int pos)
        {
            var newPos = SearchClosedValid(pos);
            return RealPos2MapPos(newPos);
        }
        public Vector3 GetClosestInArea(Vector3 pos)
        {
            var newPos = SearchClosedValid(pos);
            return newPos;
        }
        private Vector3 SearchClosedValid(Vector3 pos)
        {
            
            Vector3Int mapPos = RealPos2MapPos(pos);
            //groundFirst
            int floor = -1;
            if (_super.data.mapXZ2Y.ContainsKey((mapPos.x, mapPos.z)))
            {
                foreach (var u in _super.data.mapXZ2Y[(mapPos.x, mapPos.z)])
                {
                    if (u <= mapPos.y && u > floor)
                    {
                        floor = u;
                    }
                }
            }

            if (floor > -1)
            {
                return new Vector3(pos.x, floor* _super.data.mainData.mapUnitSize.y, pos.z);
            }


            //search
            float disMin = float.MaxValue;
            Vector3 tar = Vector3.zero;
            for (int x = -1; x <= 1; x++)
                for (int y = -1; y <= 1; y++)
                    for (int z = -1; z <= 1; z++)
                    {
                        var cur = Z_Math.Graph.ElementwiseMultiply(new Vector3(mapPos.x + x, mapPos.y + y, mapPos.z + z), _super.data.mainData.mapUnitSize);
                        if (InArea(cur) && disMin > (cur - pos).sqrMagnitude)
                        {
                            disMin = (cur - pos).sqrMagnitude;
                            tar = cur;
                        }
                    }

            if(disMin!= float.MaxValue)
            {
                var size = _super.data.mainData.mapUnitSize;
                if (tar.x < mapPos.x)
                    pos.x = tar.x + size.x / 2;
                if (tar.x > mapPos.x)
                    pos.x = tar.x - size.x / 2;

                if (tar.y > mapPos.y)
                    pos.y = tar.y;


                if (tar.z < mapPos.z)
                    pos.z = tar.z + size.z / 2;
                if (tar.z > mapPos.z)
                    pos.z = tar.z - size.z / 2;
            }
            return pos;
        }

        public bool InLimit(Vector3Int pos)
        {
            if (pos.x < 0 || pos.x > _super.sizeLimit.x)
                return false;
            if (pos.y < 0 || pos.y > _super.sizeLimit.y)
                return false;
            if (pos.z < 0 || pos.z > _super.sizeLimit.z)
                return false;
            return true;
        }

    }
}
