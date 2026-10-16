using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_ByteSerialize;
using Z_DesignStyle;
using Z_Map.Form;
using Z_UnitSystem;

namespace Z_Map
{

    public partial class TileUnit : MapUnit
    {
        private readonly HashSet<int> _passTypes = new HashSet<int>();
        private bool _objectCoversCenter;
        private bool heightPlaneValid;
        private string heightPrefabName;
        private GameObject heightPrefab;
        private float heightYaw;
        private Vector3 heightNormal;
        private float heightPlaneDot;
        private bool heightHasPlane;
        private int heightPlaneVersion;

        /// <summary>
        /// 此地块要求角色具备的全部通行类型。0 表示无限制，不进入该集合。
        /// </summary>
        public IReadOnlyCollection<int> passTypes => _objectCoversCenter ? Array.Empty<int>() : _passTypes;

        public TileUnit(TileUnitForm.Data data) : base(data)
        {
        }
        public TileUnitForm.Data data => (TileUnitForm.Data)_data;
        public TileInstance ins
        {
            set { base.ins = value; }
            get { return (TileInstance)base.ins; }
        }
        public override Type GetInsType()
        {
            return typeof(TileInstance);
        }

        public void SetPassTypes(IEnumerable<int> values)
        {
            _passTypes.Clear();
            if (values == null)
                return;

            foreach (int value in values)
            {
                if (value != 0)
                    _passTypes.Add(value);
            }
        }

        public void SetObjectCenterCovered(bool covered)
        {
            _objectCoversCenter = covered;
        }

        internal void CopyPassTypesTo(HashSet<int> target)
        {
            target.Clear();
            if (!_objectCoversCenter)
                foreach (int value in _passTypes)
                    target.Add(value);
        }

        public override void Show()
        {
            if (isShowing)
            {
                return;
            }
            base.Show();
            if(DynamicGlobalSettings.playing)
                data.unlock = true;

            Z_EventHelper.Invoke(new TileEvent()
            {
                type = MapEventType.Show,
                unit = this
            });
        }




        public float GetYByPoint(Vector2 selfPos)
        {
            if (data.scale == Vector3.zero)
                return -999;//直接坠落
            if (!heightPlaneValid || heightPrefabName != data.prefabName || heightYaw != data.euler.y)
                GetHeightPlaneVersion(prefab);
            // Keep the original plane equation/order, but do no prefab lookup,
            // Collider traversal, Euler conversion or trigonometry on warm reads.
            return heightHasPlane
                ? data.pos.y + (heightPlaneDot - (heightNormal.x * selfPos.x + heightNormal.z * selfPos.y)) / heightNormal.y
                : data.pos.y;
        }

        internal int GetHeightPlaneVersion(GameObject currentPrefab)
        {
            if (heightPlaneValid && heightPrefabName == data.prefabName
                && ReferenceEquals(heightPrefab, currentPrefab) && heightYaw == data.euler.y)
                return heightPlaneVersion;

            heightPlaneValid = true;
            heightPrefabName = data.prefabName;
            heightPrefab = currentPrefab;
            heightYaw = data.euler.y;
            unchecked { heightPlaneVersion++; }
            heightHasPlane = false;
            var collider = currentPrefab != null ? currentPrefab.GetComponentInChildren<BoxCollider>() : null;
            if (collider == null)
                return heightPlaneVersion;

            var euler = collider.transform.eulerAngles;
            float pitch = euler.x;
            euler.y += heightYaw;
            var normal = Quaternion.Euler(euler) * Vector3.back;
            if (Mathf.Abs(normal.y) < 1e-6f)
                return heightPlaneVersion;
            float tan = Mathf.Tan(pitch * Mathf.PI / 180f);
            if (Mathf.Abs(tan) < 1e-6f)
                return heightPlaneVersion;

            heightNormal = normal.normalized;
            if (Mathf.Abs(heightNormal.y) < 1e-6f)
                return heightPlaneVersion;
            heightPlaneDot = Vector3.Dot(heightNormal, new Vector3(0, 1f / tan / 2f, 0));
            heightHasPlane = true;
            return heightPlaneVersion;
        }

        public override void InvalidateCollisionGeometry()
        {
            base.InvalidateCollisionGeometry();
            // UpdateSingleOne also covers same-name/in-place prefab edits.
            InvalidateHeightPlane();
        }

        internal void InvalidateHeightPlane()
        {
            heightPlaneValid = false;
        }


        public override void Remove()
        {
            MapManager.instance.data.UnRegisterMap(data);
            TileUnitForm.RemoveData(data.uid);
            base.Remove();
        }

        public override void UpdateInfo()
        {

            base.UpdateInfo();

            Z_EventHelper.Invoke(new TileEvent()
            {
                type = MapEventType.AfterUpdate,
                unit = this
            });

        }


    }
}
