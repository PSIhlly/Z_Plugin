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
                    return MapManager.instance.updateCtrl.objectTileDic.Get(obj)[0];
                }
                else if (this is ItemUnit item)
                {
                    return MapManager.instance.updateCtrl.itemTileDic.Get(item)[0];
                }
                else if (this is CharacterUnit character)
                {
                    return MapManager.instance.updateCtrl.characterTileDic.Get(character)[0];
                }
                return null;
            }

        }
        private Vector3 lastPos;

        public Dictionary<CollideType,List<MeshInfo>> _zMeshes;
        public List<MeshInfo> GetMeshes(CollideType type)
        {
            if (_zMeshes == null)
            {
                _zMeshes = new Dictionary<CollideType, List<MeshInfo>>();
            }
            if(lastPos != data.pos)
            {
                lastPos = data.pos;
                _zMeshes[CollideType.CollideOnly] = MapManager.instance.utilCtrl.GetCollidersMesh(prefab, data.pos, data.euler, data.scale, CollideType.CollideOnly);
                _zMeshes[CollideType.TriggerOnly] = MapManager.instance.utilCtrl.GetCollidersMesh(prefab, data.pos, data.euler, data.scale, CollideType.TriggerOnly);
            }
            return _zMeshes[type];
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