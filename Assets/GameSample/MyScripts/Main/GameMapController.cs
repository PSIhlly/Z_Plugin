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
                                    _paramInfo[pair.Key] = pair.Value;
                                }
                            }
                        }
                    }
                    else if (this is ObjectUnit obj)
                    {
                        var objp = CharacterProductForm.DataByUid.GetDv(obj.productInfo.Item1, null);
                        if (objp != null)
                        {
                            foreach (var pair in objp.paramDic)
                            {
                                if (!_paramInfo.ContainsKey(pair.Key))
                                {
                                    _paramInfo[pair.Key] = pair.Value;
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
    }

    Texture GetDefaultTexture()
    {
        if (TexAssetForm.DataById.TryGetValue(GlobalDefaultHelper.DefaultTexId, out var data))
        {
            var texture = data.GetTex();
            if (texture != null)
                return texture;
        }
        return Texture2D.whiteTexture;
    }

    Texture GetTextureOrDefault(int texId)
    {
        if (TexAssetForm.DataById.TryGetValue(texId, out var data))
        {
            var texture = data.GetTex();
            if (texture != null)
                return texture;
        }
        return GetDefaultTexture();
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
        if (animTexs != null)
            animTexs.RemoveAll(texId => !TexAssetForm.DataById.ContainsKey(texId));
        var validAnimTexs = animTexs == null ? null : new List<int>(animTexs);

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

            if (interval > 0)
            {
                float all = interval * validAnimTexs.Count;

                int cur = (int)((Time.time % all) / interval);
                float timeProgress = (Time.time % interval);

                renderer.GetPropertyBlock(propBlock);
                animCurCache[data][rendererId] = cur;
                propBlock.SetTexture("_Tex", GetTextureOrDefault(validAnimTexs[cur]));
                int tempId = rendererId;
                ins.animTimer[rendererId] = TimeManager.instance.StartTimer(timeProgress, interval, () =>
                {
                    MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
                    renderer.GetPropertyBlock(propBlock);
                    cur = (cur + 1) % validAnimTexs.Count;
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
            newObjectData.unit.paramInfo[pair.Key] = pair.Value;

        newObjectData.isObstacle = objectData.collision;
    }
    private int GetWangTileMask(TileUnit unit, int rendererId, int mapTextureId)
    {
        int mask = 0;
        Vector3Int mapPos = unit.data.mapPos;
        foreach (var neighbour in wangTileNeighbours)
        {
            var pos = (mapPos.x + neighbour.x, mapPos.y, mapPos.z + neighbour.z);
            if (MapManager.instance.data.maps.TryGetValue(pos, out TileUnitForm.Data neighbourData)
                && neighbourData.texDic.GetDv(rendererId, -1) == mapTextureId)
            {
                mask |= 1 << neighbour.bit;
            }
        }
        return mask;
    }
    public void OnEvent(TileEvent evt)
    {
        switch (evt.type)
        {
            case MapEventType.Show:
                int baseRendererCount = Mathf.Min(GlobalSettings.TERRAIN_LAYER_MAX, evt.unit.ins.renderers.Length);
                for (int layer = 0; layer < baseRendererCount; layer++)
                {
                    var texName = evt.unit.data.texDic.GetDv(layer, -1);
                    var data = MapTextureForm.DataById.GetDv(texName, null);
                    if (data != null
                        && data.isWangTile
                        && data.WangTileDic != null
                        && data.WangTileDic.TryGetValue(GetWangTileMask(evt.unit, layer, texName), out int wangTexId))
                    {
                        ShowFinalMat(evt.unit.ins, layer, new List<int>() { wangTexId }, 0, false);
                    }
                    else
                    {
                        ShowFinalMat(evt.unit.ins, layer, data != null ? data.texs : null, data != null ? data.animTimeInterval : 0, false);
                    }

                    int frontRendererId = GlobalSettings.TERRAIN_LAYER_MAX + layer;
                    if (frontRendererId < evt.unit.ins.renderers.Length)
                    {
                        var frontPartTexs = data != null && data.enableFrontPart ? data.frontPartTexs : null;
                        ShowFinalMat(evt.unit.ins, frontRendererId, frontPartTexs, data != null ? data.animTimeInterval : 0, false);
                    }
                }

                for (int layer = 0; layer < baseRendererCount; layer++)
                {
                    int maskKey = GlobalSettings.TERRAIN_LAYER_MAX + layer;
                    var texName = evt.unit.data.texDic.GetDv(maskKey, -1);
                    var data = MapMaskForm.DataById.GetDv(texName, null);

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
                        foreach (var keeper in evt.unit.ins.keepers)
                        {
                            keeper.enableFixedYRotation = data.faceType != FaceType.Flexible;
                            keeper.enableFixedZRotation0 = data.faceType != FaceType.Flexible;
                            keeper.fixedYRotation = 0;
                        }
                        RefreshObjectAppearance(evt.unit, data);
                    }
                    break;
                }
            case MapEventType.Move:
                {
                    var data = MapObjectForm.DataById.GetDv(evt.unit.productInfo.Item1, null);
                    if (data != null)
                        RefreshObjectAppearance(evt.unit, data);
                    break;
                }
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
        ShowFinalMat(unit.ins, 0, data.GetAnimClip(direction), data.model.animTimeInterval, false);
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
