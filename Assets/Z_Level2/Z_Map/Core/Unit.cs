using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Z_Map
{
    public enum UpdateType
    {
        ShowOnly,
        Always
    }
    public abstract class Unit
    {
        public Unit(int uid, GameObject prefab,Vector3 pos,Vector3 eular,UpdateType updateType)
        {
            this.uid = uid;
            this.prefab = prefab;
            this.pos = pos;
            this.eular = eular;
            this.updateType = updateType;
        }
        public int uid;
        public GameObject prefab;

        public Instance ins;
        public Vector3 pos;
        public Vector3 eular;
        public UpdateType updateType = UpdateType.ShowOnly;
        public bool isShowing => ins != null && ins.gameObject != null && ins.gameObject.activeSelf;
        
    }
}
