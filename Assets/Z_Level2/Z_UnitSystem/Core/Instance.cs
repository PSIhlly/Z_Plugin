using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Z_UnitSystem
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
        private Renderer[] _renderers;
        public Renderer[] renderers
        {
            get
            {
                if (_renderers == null)
                    _renderers = GetComponentsInChildren<Renderer>();
                return _renderers;
            }
        }
        private Rigidbody _rigidbody;
        public Rigidbody rigidbody
        {
            get
            {
                if (_rigidbody == null)
                    _rigidbody = GetComponentInChildren<Rigidbody>();
                return _rigidbody;
            }
        }
    }
}
