using System;
using System.Collections.Generic;
using UnityEngine;
using Z_DesignStyle;
using Z_Map.Form;

namespace Z_Map.Analysis
{
    public enum Dir
    {
        Right,
        Left,
        Forward,
        Back
    }
    public class NavUnit
    {
        public bool isNull;
        public Vector3Int pos;
        public Vector3 realPos;
        public HashSet<Dir> cantPassParts;
        public List<NavUnit> links;
    }
    public class NavigationController : Z_Controller<MapManager>
    {
        Bfs bfs;
        public NavUnit[,,] navUnits;
        public float step;
        public void Build()
        {
            step = _super.dataCtrl.mainData.mapUnitSize.y*1/10;
            InitMap(); 
            bfs = new Bfs(this);
        }
        
        public void InitMap()
        {
            Vector3Int[] tryDir = new Vector3Int[] {Vector3Int.right,Vector3Int.left,Vector3Int.forward,Vector3Int.back };
            Vector2[] offset = new Vector2[] { Vector2.right * 0.25f, Vector2.left * 0.25f, Vector2.up * 0.25f, Vector2.down * 0.25f };
                
                Vector3Int size = _super.dataCtrl.mainData.size;
                navUnits = new NavUnit[size.x,size.y,size.z];
            for (int i = 0; i < size.x; i++)
                for (int j = 0; j < size.y; j++)
                    for (int k = 0; k < size.z; k++)
                    {
                        var navUnit = new NavUnit();
                        navUnits[i, j, k] = navUnit;
                        navUnit.cantPassParts = new HashSet<Dir>();
                        navUnit.links = new List<NavUnit>();
                        navUnit.realPos = _super.dataCtrl.maps[i, j, k].pos;
                        navUnit.pos = new Vector3Int(i,j,k);
                        navUnit.isNull = _super.dataCtrl.maps[i, j, k].scale == Vector3.zero;
                    }

            for (int i = 0; i < size.x; i++)
                for (int j = 0; j < size.y; j++)
                    for (int k = 0; k < size.z; k++)
                    {
                        var navUnit = navUnits[i, j, k];
                        var map = _super.dataCtrl.maps[i, j, k];

                        for(int m=-1;m<=1;m++)
                        for (int l=0;l<4;l++)
                        {
                            Vector3Int linkPos = Z_Math.Graph.GetVector3Int( Z_Math.Graph.ElementwisePlus( new Vector3Int(i, m+j, k) , tryDir[l]));
                                if (!InArea(linkPos))
                                    continue;
                                var link = _super.dataCtrl.maps[linkPos.x, linkPos.y, linkPos.z];

                            Vector2 p = new Vector2(tryDir[l].x*0.5f, tryDir[l].z * 0.5f);
                                
                               
                                if (Math.Abs(link.unit.GetYByPoint(-p)- map.unit.GetYByPoint(p)) <=step)
                            {
                                    
                                    navUnit.links.Add(navUnits[linkPos.x, linkPos.y, linkPos.z]);
                            }
                        }
                    }

            foreach (var obs in ItemUnitForm.DataByUid.Values)
            {
                if (obs != null && obs.isObstacle)
                {
                    
                    foreach (var bc in obs.unit.prefab.GetComponentsInChildren<BoxCollider>())
                    {
                        Vector3[] points= Z_Math.Graph.GetCubeEightPoint(bc.center,bc.size, obs.euler, bc.transform.lossyScale, obs.pos);
                        var overlapMaps=Z_Math.Graph.GetRoughOverlapIntPos(points);
                        //simple
                        var quad = new Vector2[] { new Vector2(points[(int)Z_Math.Graph.CubeEightPoint.LeftDownForward].x, points[(int)Z_Math.Graph.CubeEightPoint.LeftDownForward].z),
                            new Vector2(points[(int)Z_Math.Graph.CubeEightPoint.RightDownForward].x, points[(int)Z_Math.Graph.CubeEightPoint.RightDownForward].z),
                            new Vector2(points[(int)Z_Math.Graph.CubeEightPoint.RightDownBack].x, points[(int)Z_Math.Graph.CubeEightPoint.RightDownBack].z),
                            new Vector2(points[(int)Z_Math.Graph.CubeEightPoint.LeftDownBack].x, points[(int)Z_Math.Graph.CubeEightPoint.LeftDownBack].z) };
                       foreach (var map in overlapMaps)
                        {
                            for(int i=0,icnt=offset.Length;i<icnt;i++)
                            {
                                if (InArea(map) && Z_Math.Graph.IsPointInQuad(quad, new Vector2(map.x, map.z)+offset[i]))
                                {
                                    navUnits[map.x, map.y, map.z].cantPassParts.Add((Dir)i);
                                }
                            }
                        }
                    }
                }
            }
        }
        public Vector3 GetNormalWithoutY(Vector3 tar)
        {
            tar.y = 0;
            return tar.normalized;
        }
        public Vector3 GetNextDir(Vector3 cur,Vector3 tar,int maxStep)
        {
            var res=bfs.GetNextDir(cur, tar, maxStep);
            res.y = 0;
            return res;
        }
        public Vector3Int RealPos2MapPos(Vector3 pos)
        {
            return _super.mapUtilCtrl.RealPos2MapPos(pos);
        }
        public Vector3Int GetClosestInArea(Vector3Int pos)
        {
            return _super.mapUtilCtrl.GetClosestInArea(pos);
        }
        public bool InArea(Vector3Int pos)
        {
            return _super.mapUtilCtrl.InArea(pos);
        }
        public Vector3 MapPos2RealPos(Vector3Int pos)
        {
            return _super.mapUtilCtrl.MapPos2RealPos(pos);
        }
        public Dir GetDir(Vector3 self,Vector3 tar)
        {
            if (self.x < tar.x-0.01f)
                return Dir.Right;
            if (self.x > tar.x+0.01f)
                return Dir.Left;
            if (self.z < tar.z-0.01f)
                return Dir.Forward;
            return Dir.Back;
        }
    }
    
}
