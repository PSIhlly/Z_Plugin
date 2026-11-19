using System.Collections.Generic;
using UnityEngine;
using Z_Math;
using Z_Mesh;
using Z_UnitSystem;
using Z_UnitSystem.Form;

namespace Z_Map
{
    public partial class MapUnit : Unit
    {
        public MapManager manager => MapManager.instance;
        public MapUnit(UnitForm.Data data) : base(data)
        {
        }

        public TileUnit belongTile
        {
            get
            {
                if (this is TileUnit tile)
                {
                    return tile;
                }
                else
                if (this is ObjectUnit obj)
                {
                    return MapManager.instance.updateCtrl.objectTileDic.GetFirst(obj);
                }
                else if (this is ItemUnit item)
                {
                    return MapManager.instance.updateCtrl.itemTileDic.GetFirst(item);
                }
                else if (this is CharacterUnit character)
                {
                    return MapManager.instance.updateCtrl.characterTileDic.GetFirst(character);
                }
                return null;
            }

        }
        private string lastPrefabName;
        private Vector3 lastPos;
        private Vector3 lastEuler;
        private Vector3 lastScale;
        // Bake geometry around the origin; translation reuses the world arrays.
        private readonly List<MeshInfo> collisionOffsets = new List<MeshInfo>();
        private readonly List<MeshInfo> canonicalCollisionOffsets = new List<MeshInfo>();
        private Rect? firstPartCollisionBounds;
        private float boundsColliderScale = 1f;
        private bool centerCollider = true;
        private bool lastHasFirstPartTransform;
        private Vector3 lastFirstPartPosition;
        private Vector3 lastFirstPartScale;

        // Product-specific geometry is resolved from current data, including for
        // off-screen units. The pool template itself always remains immutable.
        public virtual bool TryGetFirstPartTransform(out Vector3 position, out Vector3 scale)
        {
            position = Vector3.zero;
            scale = Vector3.one;
            return false;
        }

        // Appearance-dependent geometry must also work without a shown Instance.
        public bool SetFirstPartCollisionBounds(Rect? bounds, float colliderScale, bool centered = true)
        {
            if (firstPartCollisionBounds == bounds && boundsColliderScale == colliderScale && centerCollider == centered)
                return false;
            firstPartCollisionBounds = bounds;
            boundsColliderScale = colliderScale;
            centerCollider = centered;
            InvalidateCollisionGeometry();
            return true;
        }

        public Dictionary<CollideType,List<MeshInfo>> _zMeshes;
        public virtual void InvalidateCollisionGeometry()
        {
            _zMeshes = null;
            collisionOffsets.Clear();
            canonicalCollisionOffsets.Clear();
        }

        public List<MeshInfo> GetMeshes(CollideType type)
        {
            bool hasTransform = TryGetFirstPartTransform(out var partPosition, out var partScale);
            bool rebuild = _zMeshes == null
                || lastPrefabName != data.prefabName
                || lastScale != data.scale
                || lastHasFirstPartTransform != hasTransform
                || hasTransform && (lastFirstPartPosition != partPosition || lastFirstPartScale != partScale);
            bool rotated = rebuild || lastEuler != data.euler;
            if (rebuild)
            {
                lastPrefabName = data.prefabName;
                lastScale = data.scale;
                lastHasFirstPartTransform = hasTransform;
                lastFirstPartPosition = partPosition;
                lastFirstPartScale = partScale;
                var sourcePrefab = prefab;
                collisionOffsets.Clear();
                canonicalCollisionOffsets.Clear();
                _zMeshes = new Dictionary<CollideType, List<MeshInfo>>();
                CacheCollisionGeometry(sourcePrefab, CollideType.CollideOnly);
                CacheCollisionGeometry(sourcePrefab, CollideType.TriggerOnly);
                _zMeshes[CollideType.All] = new List<MeshInfo>();
                _zMeshes[CollideType.All].AddRange(_zMeshes[CollideType.CollideOnly]);
                _zMeshes[CollideType.All].AddRange(_zMeshes[CollideType.TriggerOnly]);
            }
            if (rotated)
            {
                lastEuler = data.euler;
                var rotation = Quaternion.Euler(lastEuler);
                for (int i = 0; i < collisionOffsets.Count; i++)
                {
                    var canonical = canonicalCollisionOffsets[i];
                    var offset = collisionOffsets[i];
                    offset.center = rotation * canonical.center;
                    for (int j = 0; j < offset.positions.Length; j++)
                        offset.positions[j] = rotation * canonical.positions[j];
                }
            }
            if (rotated || lastPos != data.pos)
            {
                lastPos = data.pos;
                var meshes = _zMeshes[CollideType.All];
                for (int i = 0; i < meshes.Count; i++)
                {
                    var offset = collisionOffsets[i];
                    var mesh = meshes[i];
                    mesh.center = offset.center + lastPos;
                    for (int j = 0; j < mesh.positions.Length; j++)
                        mesh.positions[j] = offset.positions[j] + lastPos;
                }
            }
            return _zMeshes[type];
        }

        private void CacheCollisionGeometry(GameObject sourcePrefab, CollideType type)
        {
            var offsets = manager.utilCtrl.GetCollidersMesh(
                sourcePrefab, Vector3.zero, Vector3.zero, lastScale, type,
                firstPartCollisionBounds, boundsColliderScale, centerCollider,
                lastHasFirstPartTransform ? lastFirstPartPosition : (Vector3?)null,
                lastHasFirstPartTransform ? lastFirstPartScale : (Vector3?)null);
            canonicalCollisionOffsets.AddRange(offsets);
            var meshes = new List<MeshInfo>(offsets.Count);
            foreach (var offset in offsets)
            {
                collisionOffsets.Add(new MeshInfo { type = offset.type, positions = new Vector3[offset.positions.Length] });
                meshes.Add(new MeshInfo { type = offset.type, positions = new Vector3[offset.positions.Length] });
            }
            _zMeshes[type] = meshes;
        }
        
        public void Create()
        {
            if (this is TileUnit tile)
            {
                if (!tile.data.enteredScene)
                {
                    tile.data.enteredScene = true;
                    Z_EventHelper.Invoke(new TileEvent()
                    {
                        type = MapEventType.Create,
                        unit = tile
                    });
                }
            }
            else
            if (this is ObjectUnit obj)
            {
                if (!obj.data.enteredScene)
                {
                    obj.data.enteredScene = true;
                    Z_EventHelper.Invoke(new ObjectEvent()
                    {
                        type = MapEventType.Create,
                        unit = obj
                    });
                }
            }
            else if (this is ItemUnit item)
            {
                if (!item.data.enteredScene)
                {
                    item.data.enteredScene = true;
                    Z_EventHelper.Invoke(new ItemEvent()
                    {
                        type = MapEventType.Create,
                        unit = item
                    });
                }
            }
            else if (this is CharacterUnit character)
            {
                if (!character.data.enteredScene)
                {
                    character.data.enteredScene = true;
                    Z_EventHelper.Invoke(new CharacterEvent()
                    {
                        type = MapEventType.Create,
                        unit = character
                    });
                }
            }

        }
        public MapInstance ins
        {
            set { base.ins = value; }
            get { return (MapInstance)base.ins; }
        }

    }
}
