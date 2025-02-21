using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_DesignStyle;

namespace Z_Map
{
    public class MapUtilController : Z_Controller<MapManager>
    {

        public bool InArea(Vector3Int pos)
        {
            return _super.dataCtrl.maps.ContainsKey((pos.x, pos.y, pos.z));
        }
        public bool InArea(Vector3 pos)
        {
            int x = (int)Math.Round(pos.x / _super.dataCtrl.mainData.mapUnitSize.x);
            int y = (int)(pos.y /_super.dataCtrl.mainData.mapUnitSize.y);
            int z = (int)Math.Round(pos.z / _super.dataCtrl.mainData.mapUnitSize.z);

            return _super.dataCtrl.maps.ContainsKey((x, y, z));
        }
        public Vector3Int RealPos2MapPos(Vector3 pos)
        {
            pos = Z_Math.Graph.ElementwiseDivide(pos, _super.dataCtrl.mainData.mapUnitSize);
            return new Vector3Int((int)Math.Round(pos.x), (int)(pos.y), (int)Math.Round(pos.z));
        }
        public Vector3 MapPos2RealPos(Vector3Int pos)
        {
            return Z_Math.Graph.ElementwiseMultiply(pos, _super.dataCtrl.mainData.mapUnitSize);
        }

        public Vector3Int GetClosestInArea(Vector3Int pos)
        {
            //groundFirst
            int floor = -1;
            if(_super.dataCtrl.mapXZ2Y.ContainsKey((pos.x, pos.z)))
            {
                foreach (var u in _super.dataCtrl.mapXZ2Y[(pos.x, pos.z)])
                {
                    if (u <= pos.y && u > floor)
                    {
                        floor = u;
                    }
                }
            }
            
            if(floor>-1)
            {
                return new Vector3Int(pos.x, floor, pos.z);
            }
            
            
            //search
            
                for (int x = -1; x <= 1; x ++)
                    for (int y = -1; y <= 1; y ++)
                        for (int z = -1; z <= 1; z ++)
                        {
                            var cur = new Vector3Int(pos.x + x , pos.y + y, pos.z + z);
                            if (InArea(cur))
                            {
                                return cur;
                            }
                        }

            return pos;
        }
        public Vector3 GetClosestInArea(Vector3 pos)
        {
            int x = (int)(pos.x + _super.dataCtrl.mainData.mapUnitSize.x / 2);
            int y = (int)(pos.y + _super.dataCtrl.mainData.mapUnitSize.y / 2);
            int z = (int)(pos.z + _super.dataCtrl.mainData.mapUnitSize.z / 2);

            return GetClosestInArea(new Vector3Int(x,y,z));
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
