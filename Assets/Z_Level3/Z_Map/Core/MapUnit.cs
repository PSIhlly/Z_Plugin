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

        public Dictionary<CollideType,List<MeshInfo>> _zMeshes;
        public void InvalidateCollisionGeometry()
        {
            _zMeshes = null;
            collisionOffsets.Clear();
        }

        public List<MeshInfo> GetMeshes(CollideType type)
        {
            bool rebuild = _zMeshes == null
                || lastPrefabName != data.prefabName
                || lastEuler != data.euler
                || lastScale != data.scale;
            if (rebuild)
            {
                lastPrefabName = data.prefabName;
                lastEuler = data.euler;
                lastScale = data.scale;
                var sourcePrefab = prefab;
                collisionOffsets.Clear();
                _zMeshes = new Dictionary<CollideType, List<MeshInfo>>();
                CacheCollisionGeometry(sourcePrefab, CollideType.CollideOnly);
                CacheCollisionGeometry(sourcePrefab, CollideType.TriggerOnly);
                _zMeshes[CollideType.All] = new List<MeshInfo>();
                _zMeshes[CollideType.All].AddRange(_zMeshes[CollideType.CollideOnly]);
                _zMeshes[CollideType.All].AddRange(_zMeshes[CollideType.TriggerOnly]);
            }
            if (rebuild || lastPos != data.pos)
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
                sourcePrefab, Vector3.zero, lastEuler, lastScale, type);
            collisionOffsets.AddRange(offsets);
            var meshes = new List<MeshInfo>(offsets.Count);
            foreach (var offset in offsets)
                meshes.Add(new MeshInfo
                {
                    type = offset.type,
                    positions = new Vector3[offset.positions.Length]
                });
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
