using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Z_Map
{

    public abstract class Instance: MonoBehaviour
    {
        public Unit unit;
        private BoxCollider[] _boxColliders;
        public BoxCollider[] boxColliders
        {
            get
            {
                if (_boxColliders == null)
                    _boxColliders = GetComponentsInChildren<BoxCollider>();
                return _boxColliders;
            }
        }
        private Renderer _renderer;
        public Renderer renderer
        {
            get
            {
                if (_renderer == null)
                    _renderer = GetComponentInChildren<Renderer>();
                return _renderer;
            }
        }
    }
}
