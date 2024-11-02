using UnityEngine;
using Z_DesignStyle;

namespace Z_Map.Analysis
{
    public enum PassType
    {
        Cant,
        Can
    }
    public class NavUnit
    {
        public PassType type;
    }
    public class NavigationController : Z_Controller<MapManager>
    {
        Bfs bfs;
        public NavUnit[,,] navUnits;
        public void Build()
        {
            InitMap(); 
            bfs = new Bfs(this);
        }
        
        public void InitMap()
        {
                Vector3Int size = _super.mapUtilController.GetSize();
                navUnits = new NavUnit[size.x,size.y,size.z];
                for (int i = 0; i < size.x; i++)
                    for (int j = 0; j < size.y; j++)
                        for (int k = 0; k < size.z; k++)
                    {
                        var navUnit = new NavUnit();
                        navUnit.type = PassType.Can;
                        navUnits[i, j, k] = navUnit;
                    }

            foreach (var obs in _super.itemDic.Values)
            {
                if (obs != null && obs.isObstacle)
                {
                    foreach (var bc in obs.prefab.GetComponents<BoxCollider>())
                    {
                        Vector3[] points= Z_Math.Graph.GetCubeEightPoint(bc.center,bc.size, obs.eular, obs.prefab.transform.lossyScale, obs.pos);
                        var overlapMaps=Z_Math.Graph.GetRoughOverlapIntPos(points);
                        //simple
                        var quad = new Vector2[] { new Vector2(points[(int)Z_Math.Graph.CubeEightPoint.LeftDownForward].x, points[(int)Z_Math.Graph.CubeEightPoint.LeftDownForward].z),
                            new Vector2(points[(int)Z_Math.Graph.CubeEightPoint.RightDownForward].x, points[(int)Z_Math.Graph.CubeEightPoint.RightDownForward].z),
                            new Vector2(points[(int)Z_Math.Graph.CubeEightPoint.RightDownBack].x, points[(int)Z_Math.Graph.CubeEightPoint.RightDownBack].z),
                            new Vector2(points[(int)Z_Math.Graph.CubeEightPoint.LeftDownBack].x, points[(int)Z_Math.Graph.CubeEightPoint.LeftDownBack].z) };
                        Debug.Log(quad[0]+" "+quad[1]+" "+quad[2]+" "+quad[3]+"包的" + overlapMaps.Count);
                        foreach (var map in overlapMaps)
                        {
                            Debug.Log("test" + map+" "+ InArea(map));
                            if (InArea(map) && Z_Math.Graph.IsPointInQuad(quad, new Vector2(map.x, map.z)))
                            {
                                Debug.Log("ookk" + map);
                                navUnits[map.x, map.y, map.z].type = PassType.Cant;
                            }
                        }
                    }
                }
            }
        }
        public Vector3 GetNextDir(Vector3 cur,Vector3 tar)
        {
            return bfs.GetNextDir(cur, tar);
        }
        public Vector3Int RealPos2MapPos(Vector3 pos)
        {
            return _super.mapUtilController.RealPos2MapPos(pos);
        }
        public Vector3Int GetClosestInArea(Vector3Int pos)
        {
            return _super.mapUtilController.GetClosestInArea(pos);
        }
        public bool InArea(Vector3Int pos)
        {
            return _super.mapUtilController.InArea(pos);
        }
        public Vector3 MapPos2RealPos(Vector3Int pos)
        {
            return _super.mapUtilController.MapPos2RealPos(pos);
        }
    }
    
}
