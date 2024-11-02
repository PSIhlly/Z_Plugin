using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Z_Map
{

    public abstract class Instance: MonoBehaviour
    {
        public Unit unit;

        private Renderer _renderer;
        public Renderer renderer
        {
            get
            {
                if (_renderer == null)
                    _renderer = GetComponent<Renderer>();
                return _renderer;
            }
        }
    }
}
