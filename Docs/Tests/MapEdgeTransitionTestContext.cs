using System.Collections.Generic;
using UnityEngine;

// Only copied into the isolated edge source fixture, never the production game.
namespace Z_Map
{
    public static class DynamicGlobalSettings { public static bool edgeTransition = true; }
    public class MapInfo
    {
        public Dictionary<(int, int), SortedSet<int>> mapXZ2Y;
        public int mapVersion;
        public MainData mainData = new MainData();
        public class MainData { public Vector3 mapUnitSize = Vector3.one; }
    }
    public class MapManager
    {
        public static MapManager instance;
        public bool enable;
        public GameObject mainGo;
        public MapInfo data;
        public UpdateController updateCtrl;
        public class UpdateController { public Vector3 curCenterPos; }
    }
}
