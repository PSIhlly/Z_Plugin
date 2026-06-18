using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Z_UnitSystem
{
    public enum CollideEventType
    {
        TriggerEnter,
        TriggerExit
    }
    public class CollideEvent : Z_Event
    {
        public CollideEventType type;
        public Unit a;
        public Unit b;
    }

    public abstract class Instance: MonoBehaviour
    {
        public Unit unit;
        private BoxCollider[] _boxColliders;
        private CapsuleCollider[] _capsuleColliders;
        public bool vising;

        /// <summary>
        /// 显示层级上限（0=仅显示第0层Renderer, 1=显示0和1层, int.MaxValue=显示全部）
        /// </summary>
        public int displayLayer = int.MaxValue;

        public BoxCollider[] boxColliders
        {
            get
            {
                if (_boxColliders == null)
                    _boxColliders = GetComponentsInChildren<BoxCollider>();
                return _boxColliders;
            }
        }
        public CapsuleCollider[] capsuleColliders
        {
            get
            {
                if (_capsuleColliders == null)
                    _capsuleColliders = GetComponentsInChildren<CapsuleCollider>();
                return _capsuleColliders;
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
        public virtual void VisOff()
        {
            if (!vising)
                return;
            vising = false;
            foreach (var render in renderers)
            {
                render.enabled = false;
            }
        }
        public virtual void VisDegree(float degree)
        {
            if (!vising)
                return;
        }
        public virtual void VisOn()
        {
            if (vising)
                return;
            vising = true;
            foreach (var render in renderers)
            {
                render.enabled = true;
            }
        }

       
    }
}
