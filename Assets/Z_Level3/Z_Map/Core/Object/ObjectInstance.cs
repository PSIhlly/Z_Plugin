using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Time;
using Z_UnitSystem;

namespace Z_Map
{
    public class ObjectMarkVisibilityEvent : Z_Event
    {
        public bool visible;
    }
    
    public class ObjectInstance : MapInstance, IZ_Listener<ObjectMarkVisibilityEvent>
    {
        private GameObject mapMark;

        public void EnsureMapMark(GameObject prefab, bool visible)
        {
            if (mapMark == null && prefab != null)
            {
                // Freeze the model renderer slots before appending this decoration.
                // Appearance/animation/vision must never treat it as a model part.
                _ = renderers;
                mapMark = Instantiate(prefab, transform, false);
                mapMark.name = prefab.name;
                mapMark.transform.localPosition = Vector3.zero;
                mapMark.transform.localScale = Vector3.one;
                foreach (var collider in mapMark.GetComponentsInChildren<Collider>(true))
                    collider.enabled = false;
            }
            if (mapMark != null)
            {
                if (enabled && gameObject.activeInHierarchy)
                    this.Register<ObjectMarkVisibilityEvent>();
                mapMark.SetActive(visible);
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (mapMark != null)
                this.Register<ObjectMarkVisibilityEvent>();
        }

        protected override void OnDisable()
        {
            this.Unregister<ObjectMarkVisibilityEvent>();
            if (mapMark != null)
                mapMark.SetActive(false);
            base.OnDisable();
        }

        public void OnEvent(ObjectMarkVisibilityEvent evt)
        {
            if (mapMark != null)
                mapMark.SetActive(evt.visible);
        }
        public ObjectUnit unit
        {
            set { base.unit = value; }
            get { return (ObjectUnit)base.unit; }
        }


    }
}
