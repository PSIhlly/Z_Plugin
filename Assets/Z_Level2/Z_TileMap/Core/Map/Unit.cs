using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Z_TileMap.Map
{

    public class Unit : MonoBehaviour
    {
        public Unit[] connectedUnitMaps;
        public (int, int) coordinates;
        public string areaName;
        public int uniqueId;
    }
}
