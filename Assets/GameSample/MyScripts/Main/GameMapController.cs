using Form;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using UnityEngine;
using UnityEngine.Rendering;
using Z_ByteSerialize;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Map;
using Z_Map.Form;
using Z_Texture;
using Z_Time;
using Z_UnitSystem;
using Z_UnitSystem.Form;
using static UnityEngine.Rendering.DebugUI;



namespace Z_Map
{
    public partial class MapUnit
    {
        public static string productKey = "pdt";
        private (int, int) _productInfo;
        public (int, int) productInfo
        {
            get
            {
                if (_productInfo == default)
                {
                    _productInfo = (0, 0);
                    if (!string.IsNullOrEmpty(data.extra))
                    {
                        var jo = JObject.Parse(data.extra);
                        if (jo != null && jo[productKey].Type != JTokenType.Null)
                        {
                            _productInfo.Item1 = (int)jo[productKey][0];
                            _productInfo.Item2 = (int)jo[productKey][1];
                        }
                    }
                }
                return _productInfo;
            }
            set
            {
                _productInfo = value;
            }

        }
        public static string GetProductInfoString(JObject ori, (int, int) info)
        {
            var ja = new JArray();
            ja.Add(info.Item1);
            ja.Add(info.Item2);
            ori[productKey] = ja;
            return ori.ToString();
        }

        public static string paramKey = "prm";
        private Dictionary<string, GameParamForm.Data> _paramInfo;
        public Dictionary<string, GameParamForm.Data> paramInfo
        {
            get
            {
                if (_paramInfo == default)
                {
                    _paramInfo = new Dictionary<string, GameParamForm.Data>();
                    if (!string.IsNullOrEmpty(data.extra))
                    {
                        var jo = JObject.Parse(data.extra);
                        if (jo != null && jo[paramKey] != null)
                        {
                            _paramInfo = jo.Get<Dictionary<string, GameParamForm.Data>>(paramKey);
                        }
                    }
                    if (this is CharacterUnit ch)
                    {
                        var chp = CharacterProductForm.DataByUid.GetDv(ch.productInfo.Item1, null);
                        if (chp != null)
                        {
                            foreach (var pair in chp.paramDic)
                            {
                                if (!_paramInfo.ContainsKey(pair.Key))
                                {
                                    _paramInfo[pair.Key] = pair.Value.Copy();
                                }
                            }
                        }
                    }
                    else if (this is ObjectUnit obj)
                    {
                        var objp = MapObjectForm.DataById.GetDv(obj.productInfo.Item1, null);
                        if (objp != null)
                        {
                            foreach (var pair in objp.paramDic)
                            {
                                if (!_paramInfo.ContainsKey(pair.Key))
                                {
                                    _paramInfo[pair.Key] = pair.Value.Copy();
                                }
                            }
                        }
                    }
                    else if (this is ItemUnit item)
                    {
                        var itemp = ItemProductForm.DataByUid.GetDv(item.productInfo.Item1, null);
                        if (itemp != null)
                        {
                            foreach (var pair in itemp.paramDic)
                            {
                                if (!_paramInfo.ContainsKey(pair.Key))
                                {
                                    _paramInfo[pair.Key] = pair.Value.Copy();
                                }
                            }
                        }
                    }
                }
                return _paramInfo;
            }
            set
            {
                _paramInfo = value;
            }
        }
        public static string GetParamInfoString(JObject ori, Dictionary<string, GameParamForm.Data> info)
        {
            if (info != null)
            {
                ori.Set(paramKey, info);
            }
            return ori.ToString();
        }
    }
}

public class GameMapController : Z_Controller<GameManager>, IZ_Listener<TileEvent>, IZ_Listener<ObjectEvent>, IZ_Listener<ItemEvent>
{
    public GameMapController(GameManager super) : base(super)
    {
        Z_EventHelper.Register<TileEvent>(this);
        Z_EventHelper.Register<ObjectEvent>(this);
        Z_EventHelper.Register<ItemEvent>(this);
        ProgressForm.changeCameramodeAction += (data, old, now) =>
        {
            DynamicGlobalSettings.cameraMode = now;
        };
        MapTextureForm.changePasstypeAction += (_, _, _) =>
        {
            foreach (var tileData in TileUnitForm.DataByUid.Values)
                GameMapData.ApplyTilePassTypes(tileData);
        };

        UnitForm.beforeGetAction += (data) =>
        {
            if (data.unit is MapUnit mapU)
            {
                var jo = string.IsNullOrEmpty(data.extra) ? new JObject() : JObject.Parse(data.extra);

                MapUnit.GetProductInfoString(jo, mapU.productInfo);

                MapUnit.GetParamInfoString(jo, mapU.paramInfo);
                data.extra = jo.ToString();
            }

        };
    }

    public Dictionary<(int, int), Texture2D> alphaTextureDic = new Dictionary<(int, int), Texture2D>();
    public Dictionary<UnitForm.Data, Dictionary<int, int>> animCurCache = new Dictionary<UnitForm.Data, Dictionary<int, int>>();
    private static readonly Dictionary<(int mapTextureId, bool frontPart, int mask), List<int>>
        wangTileAnimationTextures = new Dictionary<(int, bool, int), List<int>>();
    private static readonly Dictionary<(int mapObjectId, AnimDirecton direction, int mask), List<int>>
        objectWangTileAnimationTextures = new Dictionary<(int, AnimDirecton, int), List<int>>();
    private readonly Dictionary<ObjectUnit, Bounds> objectWangTileBounds = new Dictionary<ObjectUnit, Bounds>();
    private static readonly (int x, int z, int bit)[] wangTileNeighbours =
    {
        (-1, 1, 0), (0, 1, 1), (1, 1, 2),
        (-1, 0, 3),             (1, 0, 4),
        (-1, -1, 5), (0, -1, 6), (1, -1, 7)
    };
    public void Reset()
    {
        alphaTextureDic.Clear();
        animCurCache.Clear();
        objectWangTileBounds.Clear();
        ClearWangTileAnimationTextures();
    }

    internal static void ClearWangTileAnimationTextures()
    {
        wangTileAnimationTextures.Clear();
        objectWangTileAnimationTextures.Clear();
    }

    internal static void RegisterWangTileAnimationTexture(
        int mapTextureId, bool frontPart, int mask, int textureId)
    {
        var key = (mapTextureId, frontPart, mask);
        if (!wangTileAnimationTextures.TryGetValue(key, out List<int> textures))
        {
            textures = new List<int>();
            wangTileAnimationTextures.Add(key, textures);
        }
        textures.Add(textureId);
    }

    private static bool TryGetWangTileAnimationTextures(
        int mapTextureId, bool frontPart, int mask, out List<int> textures)
    {
        return wangTileAnimationTextures.TryGetValue((mapTextureId, frontPart, mask), out textures)
            && textures.Count > 0;
    }

    internal static void RegisterObjectWangTileAnimationTexture(
        int mapObjectId, AnimDirecton direction, int mask, int textureId)
    {
        var key = (mapObjectId, direction, mask);
        if (!objectWangTileAnimationTextures.TryGetValue(key, out List<int> textures))
        {
            textures = new List<int>();
            objectWangTileAnimationTextures.Add(key, textures);
        }
        textures.Add(textureId);
    }

    internal static void ClearObjectWangTileAnimationTextures(int mapObjectId)
    {
        foreach (var key in objectWangTileAnimationTextures.Keys
                     .Where(key => key.mapObjectId == mapObjectId).ToList())
            objectWangTileAnimationTextures.Remove(key);
    }

    public void RefreshObjectWangTileAppearances(int mapObjectId)
    {
        var data = MapObjectForm.DataById.GetDv(mapObjectId, null);
        if (data == null)
            return;

        foreach (ObjectUnitForm.Data objectData in ObjectUnitForm.DataByUid.Values)
        {
            ObjectUnit unit = objectData.unit;
            if (unit.productInfo.Item1 != mapObjectId)
                continue;

            if (data.isWangTile && TryGetObjectWangTileBounds(unit, out Bounds bounds))
                objectWangTileBounds[unit] = bounds;
            else
                objectWangTileBounds.Remove(unit);

            if (unit.isShowing)
            {
                ConfigureObjectKeepers(unit, data);
                RefreshObjectAppearance(unit, data);
            }
        }
    }

    static Texture GetDefaultTexture()
    {
        if (TexAssetForm.DataById.TryGetValue(GlobalDefaultHelper.DefaultTexId, out var data))
        {
            var texture = data.GetTex();
            if (texture != null)
                return texture;
        }
        return Texture2D.whiteTexture;
    }

    static Texture GetTextureOrDefault(int texId)
    {
        if (TexAssetForm.DataById.TryGetValue(texId, out var data))
        {
            var texture = data.GetTex();
            if (texture != null)
                return texture;
        }
        return GetDefaultTexture();
    }

    static MapTextureForm.Data GetMapTextureOrFirst(int mapTextureId)
    {
        if (MapTextureForm.DataById.TryGetValue(mapTextureId, out var data))
            return data;

        return MapTextureForm.DataById
            .OrderBy(pair => pair.Key)
            .Select(pair => pair.Value)
            .FirstOrDefault();
    }

    internal static int GetAnimationFrameIndex(float animationTime, float interval, int frameCount)
    {
        if (interval <= 0 || frameCount <= 1)
            return 0;

        long frame = (long)Math.Floor(Math.Max(0, animationTime) / interval);
        return (int)(frame % frameCount);
    }


    public void ShowFinalMat(MapInstance ins, int rendererId, List<int> animTexs, float interval, bool isMask = false)
    {
        var data = ins.unit.data;
        if (!animCurCache.ContainsKey(data))
        {
            animCurCache[data] = new Dictionary<int, int>();
        }
        Renderer renderer = ins.renderers[rendererId];


        MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(propBlock);

        var hasConfiguredTextures = animTexs != null && animTexs.Count > 0;
        var validAnimTexs = animTexs == null
            ? null
            : animTexs.Where(texId => TexAssetForm.DataById.ContainsKey(texId)).ToList();

        if (validAnimTexs != null && validAnimTexs.Count > 0)
        {
            renderer.gameObject.SetActive(true);

            if (isMask)
            {

                int linkDesc = 0;
                for (int x = -1; x <= 1; x++)
                {
                    for (int z = -1; z <= 1; z++)
                    {
                        if (x == 0 && z == 0)
                            continue;
                        var mapPos = MapManager.instance.utilCtrl.RealPos2MapPos(data.pos);
                        var pos = ((int)(x + mapPos.x), (int)(mapPos.y), (int)(z + mapPos.z));
                        if (MapManager.instance.data.maps.ContainsKey(pos)
                            && MapManager.instance.data.maps[pos].texDic.ContainsKey(rendererId)
                            && MapManager.instance.data.maps[pos].texDic[rendererId] == validAnimTexs[0])
                        {
                            linkDesc |= 1 << ((z + 1) * 3 + (x + 2));
                        }
                    }
                }
                if (alphaTextureDic.TryGetValue((validAnimTexs[0], linkDesc), out var alphaTexture)
                    && alphaTexture != null)
                    propBlock.SetTexture("_AlphaTex", alphaTexture);
                else
                    propBlock.SetTexture("_AlphaTex", Texture2D.whiteTexture);

            }
            else
            {
                propBlock.SetTexture("_AlphaTex", Texture2D.whiteTexture);
            }



            renderer.enabled = true;


            TimeManager.instance.CancelTimer(ins.animTimer[rendererId]);

            if (interval > 0 && validAnimTexs.Count > 1)
            {
                int cur = GetAnimationFrameIndex(
                    TimeManager.GetAnimationTime(), interval, validAnimTexs.Count);

                renderer.GetPropertyBlock(propBlock);
                animCurCache[data][rendererId] = cur;
                propBlock.SetTexture("_Tex", GetTextureOrDefault(validAnimTexs[cur]));
                int tempId = rendererId;
                ins.animTimer[rendererId] = TimeManager.instance.StartAnimationTimer(interval, () =>
                {
                    int next = GetAnimationFrameIndex(
                        TimeManager.GetAnimationTime(), interval, validAnimTexs.Count);
                    if (next == cur)
                        return false;

                    MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
                    renderer.GetPropertyBlock(propBlock);
                    cur = next;
                    animCurCache[data][tempId] = cur;
                    propBlock.SetTexture("_Tex", GetTextureOrDefault(validAnimTexs[cur]));
                    renderer.SetPropertyBlock(propBlock);
                    return false;
                }, ins);
            }
            else
            {
                propBlock.SetTexture("_Tex", GetTextureOrDefault(validAnimTexs[0]));
            }
        }
        else if (hasConfiguredTextures)
        {
            renderer.gameObject.SetActive(true);
            renderer.enabled = true;
            TimeManager.instance.CancelTimer(ins.animTimer[rendererId]);
            propBlock.SetTexture("_AlphaTex", Texture2D.whiteTexture);
            if (!isMask)
                propBlock.SetTexture("_Tex", GetDefaultTexture());
        }
        else if (!isMask)
        {
            TimeManager.instance.CancelTimer(ins.animTimer[rendererId]);
            renderer.enabled = false;
            propBlock.SetTexture("_AlphaTex", Texture2D.blackTexture);
        }



        renderer.SetPropertyBlock(propBlock);

    }
    public void RegisterObject(ObjectUnitForm.Data newObjectData, MapObjectForm.Data objectData)
    {
        newObjectData.unit.productInfo = (objectData.id, -1);
        newObjectData.unit.paramInfo = new Dictionary<string, GameParamForm.Data>();
        foreach (var pair in objectData.paramDic)
            newObjectData.unit.paramInfo[pair.Key] = pair.Value.Copy();

        newObjectData.isObstacle = objectData.collision;
        if (objectData.isWangTile)
            UpdateObjectWangTileNeighbours(newObjectData.unit, objectData);
    }

    // Bits match TileHelper's 3x3 mask: NW,N,NE,W,E,SW,S,SE.
    // Visual bounds include the root scale and the model's actual footprint.
    internal static int GetObjectNeighbourBits(Bounds center, Bounds other)
    {
        const float tolerance = 0.001f;
        if (Mathf.Min(center.max.y, other.max.y) - Mathf.Max(center.min.y, other.min.y) <= tolerance)
            return 0;

        bool left = Mathf.Abs(center.min.x - other.max.x) <= tolerance;
        bool right = Mathf.Abs(center.max.x - other.min.x) <= tolerance;
        bool down = Mathf.Abs(center.min.z - other.max.z) <= tolerance;
        bool up = Mathf.Abs(center.max.z - other.min.z) <= tolerance;
        bool xOverlap = Mathf.Min(center.max.x, other.max.x) - Mathf.Max(center.min.x, other.min.x) > tolerance;
        bool zOverlap = Mathf.Min(center.max.z, other.max.z) - Mathf.Max(center.min.z, other.min.z) > tolerance;

        int mask = 0;
        if (left && up) mask |= 1 << 0;
        if (xOverlap && up) mask |= 1 << 1;
        if (right && up) mask |= 1 << 2;
        if (left && zOverlap) mask |= 1 << 3;
        if (right && zOverlap) mask |= 1 << 4;
        if (left && down) mask |= 1 << 5;
        if (xOverlap && down) mask |= 1 << 6;
        if (right && down) mask |= 1 << 7;
        return mask;
    }

    private static bool TryGetObjectWangTileBounds(ObjectUnit unit, out Bounds bounds)
    {
        return MapManager.instance.utilCtrl.TryGetVisionBounds(unit.data, out bounds);
    }

    private static int GetObjectWangTileMask(ObjectUnit unit, int mapObjectId, Bounds bounds)
    {
        int mask = 0;
        foreach (ObjectUnitForm.Data otherData in ObjectUnitForm.DataByUid.Values)
        {
            ObjectUnit other = otherData.unit;
            if (other == unit || other.productInfo.Item1 != mapObjectId
                || !TryGetObjectWangTileBounds(other, out Bounds otherBounds))
                continue;
            mask |= GetObjectNeighbourBits(bounds, otherBounds);
        }
        return mask;
    }

    private void UpdateObjectWangTileNeighbours(ObjectUnit unit, MapObjectForm.Data data)
    {
        if (!data.isWangTile)
            return;

        bool hadOldBounds = objectWangTileBounds.TryGetValue(unit, out Bounds oldBounds);
        bool hasNewBounds = TryGetObjectWangTileBounds(unit, out Bounds newBounds);
        if (hasNewBounds)
            objectWangTileBounds[unit] = newBounds;
        else
            objectWangTileBounds.Remove(unit);

        foreach (ObjectUnitForm.Data otherData in ObjectUnitForm.DataByUid.Values)
        {
            ObjectUnit other = otherData.unit;
            if (other == unit || other.productInfo.Item1 != data.id || !other.isShowing
                || !TryGetObjectWangTileBounds(other, out Bounds otherBounds))
                continue;
            if ((hadOldBounds && GetObjectNeighbourBits(otherBounds, oldBounds) != 0)
                || (hasNewBounds && GetObjectNeighbourBits(otherBounds, newBounds) != 0))
                RefreshObjectAppearance(other, data);
        }

    }

    private void RemoveObjectWangTileNeighbours(ObjectUnit unit, MapObjectForm.Data data)
    {
        bool hadBounds = objectWangTileBounds.TryGetValue(unit, out Bounds oldBounds)
            || TryGetObjectWangTileBounds(unit, out oldBounds);
        objectWangTileBounds.Remove(unit);
        if (!data.isWangTile || !hadBounds)
            return;

        foreach (ObjectUnitForm.Data otherData in ObjectUnitForm.DataByUid.Values)
        {
            ObjectUnit other = otherData.unit;
            if (other.productInfo.Item1 == data.id && other.isShowing
                && TryGetObjectWangTileBounds(other, out Bounds otherBounds)
                && GetObjectNeighbourBits(otherBounds, oldBounds) != 0)
                RefreshObjectAppearance(other, data);
        }
    }
    private static int GetWangTileMask(TileUnitForm.Data tileData, int layer, int mapTextureId)
    {
        int mask = 0;
        Vector3Int mapPos = tileData.mapPos;
        foreach (var neighbour in wangTileNeighbours)
        {
            var pos = (mapPos.x + neighbour.x, mapPos.y, mapPos.z + neighbour.z);
            if (MapManager.instance.data.maps.TryGetValue(pos, out TileUnitForm.Data neighbourData)
                && neighbourData.texDic != null
                && neighbourData.texDic.TryGetValue(layer, out int neighbourTextureId)
                && neighbourTextureId == mapTextureId)
            {
                mask |= 1 << neighbour.bit;
            }
        }
        return mask;
    }

    private static (List<int> textures, float interval) GetTileLayerTextures(TileUnitForm.Data tileData,
        int layer, bool frontPart = false)
    {
        if (tileData?.texDic == null || !tileData.texDic.TryGetValue(layer, out int mapTextureId))
            return (null, 0);

        var data = GetMapTextureOrFirst(mapTextureId);
        if (data == null || (frontPart && !data.enableFrontPart))
            return (null, 0);

        bool isWangTile = frontPart ? data.frontIsWangTile : data.isWangTile;
        var variants = frontPart ? data.FrontWangTileDic : data.WangTileDic;
        if (isWangTile)
        {
            int mask = GetWangTileMask(tileData, layer, mapTextureId);
            if (TryGetWangTileAnimationTextures(mapTextureId, frontPart, mask, out List<int> animationTextures))
            {
                var textures = new List<int>(animationTextures);
                return (textures, textures.Count > 1 ? data.animTimeInterval : 0);
            }
            if (variants != null && variants.TryGetValue(mask, out int textureId))
                return (new List<int> { textureId }, 0);
        }

        return (frontPart ? data.frontPartTexs : data.texs, data.animTimeInterval);
    }

    // Minimap sampling shares the scene's tile selection, without requiring a visible TileInstance.
    internal static Texture GetTileLayerTexture(TileUnitForm.Data tileData, int layer)
    {
        var (textures, _) = GetTileLayerTextures(tileData, layer);
        if (textures == null || textures.Count == 0)
            return null;

        foreach (int textureId in textures)
        {
            if (TexAssetForm.DataById.ContainsKey(textureId))
                return GetTextureOrDefault(textureId);
        }
        return GetDefaultTexture();
    }

    public void OnEvent(TileEvent evt)
    {
        switch (evt.type)
        {
            case MapEventType.Show:
                int baseRendererCount = Mathf.Min(GlobalSettings.TERRAIN_LAYER_MAX, evt.unit.ins.renderers.Length);
                for (int layer = 0; layer < baseRendererCount; layer++)
                {
                    var (textures, interval) = GetTileLayerTextures(evt.unit.data, layer);
                    ShowFinalMat(evt.unit.ins, layer, textures, interval, false);

                    int frontRendererId = GlobalSettings.TERRAIN_LAYER_MAX + layer;
                    if (frontRendererId < evt.unit.ins.renderers.Length)
                    {
                        var (frontTextures, frontInterval) = GetTileLayerTextures(evt.unit.data, layer, true);
                        ShowFinalMat(evt.unit.ins, frontRendererId, frontTextures, frontInterval, false);
                    }
                }

                for (int layer = 0; layer < baseRendererCount; layer++)
                {
                    int maskKey = GlobalSettings.TERRAIN_LAYER_MAX + layer;
                    var texName = evt.unit.data.texDic.GetDv(maskKey, -1);
                    var data = MapMaskForm.DataById.GetDv(texName, null);

                    // A retained mask must not re-enable an intentionally empty base layer.
                    if (evt.unit.data.texDic.ContainsKey(layer))
                        ShowFinalMat(evt.unit.ins, layer, data != null ? data.texsName : null, 0, true);

                }
                break;
            case MapEventType.AfterUpdate:
                GameMapData.ApplyTilePassTypes(evt.unit.data);
                break;
        }
    }
    public void OnEvent(ObjectEvent evt)
    {
        switch (evt.type)
        {
            case MapEventType.Show:
                {
                    var data = MapObjectForm.DataById.GetDv(evt.unit.productInfo.Item1, null);
                    if (data != null)
                    {
                        data.EnsureDirectionData();
                        ConfigureObjectKeepers(evt.unit, data);
                        RefreshObjectAppearance(evt.unit, data);
                        if (data.isWangTile)
                            UpdateObjectWangTileNeighbours(evt.unit, data);
                    }
                    break;
                }
            case MapEventType.Move:
            case MapEventType.BoundaryTouch:
            case MapEventType.Refresh:
                {
                    var data = MapObjectForm.DataById.GetDv(evt.unit.productInfo.Item1, null);
                    if (data != null)
                    {
                        if (data.isWangTile)
                            UpdateObjectWangTileNeighbours(evt.unit, data);
                        RefreshObjectAppearance(evt.unit, data);
                    }
                    break;
                }
            case MapEventType.Remove:
                {
                    var data = MapObjectForm.DataById.GetDv(evt.unit.productInfo.Item1, null);
                    if (data != null)
                        RemoveObjectWangTileNeighbours(evt.unit, data);
                    else
                        objectWangTileBounds.Remove(evt.unit);
                    break;
                }
        }
    }

    private static void ConfigureObjectKeepers(ObjectUnit unit, MapObjectForm.Data data)
    {
        if (unit.ins == null)
            return;
        foreach (var keeper in unit.ins.keepers)
        {
            keeper.enableFixedYRotation = data.faceType != FaceType.Flexible;
            keeper.enableFixedZRotation0 = data.faceType != FaceType.Flexible;
            keeper.fixedYRotation = 0;
        }
    }

    private void RefreshObjectAppearance(ObjectUnit unit, MapObjectForm.Data data)
    {
        if (unit.ins == null)
            return;

        // Keep this independent from renderer availability: pooled/custom
        // prefabs may start with their renderer GameObject disabled, while the
        // perspective helper still needs the new root rotation immediately.
        foreach (var keeper in unit.ins.keepers)
            keeper.RefreshNow();

        if (unit.ins.renderers == null || unit.ins.renderers.Length == 0)
            return;

        var direction = data.GetAnimDirection(unit.data.euler.y);
        if (data.isWangTile && TryGetObjectWangTileBounds(unit, out Bounds bounds)
            && objectWangTileAnimationTextures.TryGetValue(
                (data.id, direction, GetObjectWangTileMask(unit, data.id, bounds)), out List<int> textures)
            && textures.Count > 0)
        {
            ShowFinalMat(unit.ins, 0, textures, textures.Count > 1 ? data.model.animTimeInterval : 0, false);
        }
        else
        {
            ShowFinalMat(unit.ins, 0, data.GetAnimClip(direction), data.model.animTimeInterval, false);
        }
    }
    public void OnEvent(ItemEvent evt)
    {
        switch (evt.type)
        {
            case MapEventType.Show:
                var data = ItemProductForm.DataByUid.GetDv(evt.unit.productInfo.Item1, null);
                if (data != null && data.model != null && data.model.subUnitTexsName != null && data.model.subUnitTexsName.Count > 0)
                {
                    ShowFinalMat(evt.unit.ins, 0, data.model.subUnitTexsName[0], data.model.animTimeInterval, false);
                }
                break;
            case MapEventType.AfterUpdate:
                break;
        }
    }

}
