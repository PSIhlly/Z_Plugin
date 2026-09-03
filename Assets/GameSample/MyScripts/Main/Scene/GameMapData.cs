using Form;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Z_DataSystem;
using Z_DesignStyle;
using Z_Map;
using Z_Map.Form;

public class GameMapData : MapInfo
{
    public static void ApplyCharacterProductPassTypes(CharacterUnitForm.Data unitData,
        CharacterProductForm.Data productData = null)
    {
        if (unitData == null)
            return;

        if (productData == null)
            CharacterProductForm.DataByUid.TryGetValue(unitData.unit.productInfo.Item1, out productData);

        unitData.unit.SetPassTypes(productData?.passType);
    }

    public static void ApplyTilePassTypes(TileUnitForm.Data unitData)
    {
        if (unitData == null)
            return;

        var requiredTypes = new HashSet<int>();
        if (unitData.texDic != null)
        {
            foreach (var pair in unitData.texDic.OrderBy(pair => pair.Key))
            {
                // texDic 后续槽位存储 MapMask；只有前三个地表材质槽参与通行判定。
                if (pair.Key < 0 || pair.Key >= GlobalSettings.TERRAIN_LAYER_MAX)
                    continue;
                if (!MapTextureForm.DataById.TryGetValue(pair.Value, out var textureData))
                    continue;
                if (textureData.passType != 0 && PassTypeForm.DataById.ContainsKey(textureData.passType))
                    requiredTypes.Add(textureData.passType);
            }
        }

        unitData.unit.SetPassTypes(requiredTypes);
        // 场景表中的 int 字段仅保留首个类型用于兼容和检视，完整条件由 TileUnit.passTypes 保存。
        unitData.passType = requiredTypes.OrderBy(id => id).FirstOrDefault();
    }

    public static int GetCharacterProductSize(CharacterProductForm.Data productData)
    {
        if (productData == null)
            return 1;

        int size = Mathf.Max(1, productData.size);
        if (productData.size != size)
            productData.size = size;
        return size;
    }

    public static bool ApplyCharacterProductSize(CharacterUnitForm.Data unitData, CharacterProductForm.Data productData = null)
    {
        if (unitData == null)
            return false;

        if (productData == null)
            CharacterProductForm.DataByUid.TryGetValue(unitData.unit.productInfo.Item1, out productData);

        return ApplyCharacterProductSize(unitData, GetCharacterProductSize(productData));
    }

    public static bool ApplyCharacterProductSize(CharacterUnitForm.Data unitData, int size)
    {
        if (unitData == null)
            return false;

        Vector3 scale = Vector3.one * Mathf.Max(1, size);
        bool changed = unitData.scale != scale;
        unitData.scale = scale;
        if (unitData.unit.ins != null)
            unitData.unit.ins.transform.localScale = scale;
        return changed;
    }

    public override CharacterUnitForm.Data GetNewCharacter(string prefabName = "", bool isMine = false, string extra = "")
    {
        var form = new CharacterUnitForm.Data(-1, !isMine, Vector3.zero, 4, 4, 4, isMine, "", prefabName, Vector3.zero, Vector3.zero, Vector3.one, 0, new List<int>(), extra, false, GlobalDefaultHelper.DefaultTexId);
        //init
        var dic = form.unit.evtDic;
        var pdt = form.unit.productInfo;
        ApplyCharacterProductSize(form);
        ApplyCharacterProductPassTypes(form);
        return form;
    }

    public override List<CharacterUnitForm.Data> GetCharacterDatasByJa(string ja)
    {
        var result = CharacterUnitForm.GetDatasByJa(JArray.Parse(mainData.characterJa));
        foreach (var data in result)
        {
            ApplyCharacterProductSize(data);
            ApplyCharacterProductPassTypes(data);
        }
        return result;

    }
    public override ItemUnitForm.Data GetNewItem(string prefabName = "", object[] prms = null)
    {
        var form = new ItemUnitForm.Data(-1, "", prefabName, Vector3.zero, Vector3.zero, Vector3.one, 0, new List<int>(), "",false,GlobalDefaultHelper.DefaultTexId);
        return form;
    }
    public override ObjectUnitForm.Data GetNewObject(string prefabName = "", object[] prms = null)
    {
        var form = new ObjectUnitForm.Data(-1, false, "", prefabName, Vector3.zero, Vector3.zero, Vector3.one, 0, new List<int>(), "", false, GlobalDefaultHelper.DefaultTexId);
        return form;
    }

    public override List<ObjectUnitForm.Data> GetObjectDatasByJa(string ja)
    {
        return ObjectUnitForm.GetDatasByJa(JArray.Parse(mainData.objectJa));
    }

    public override TileUnitForm.Data GetNewTile(Vector3Int mapPos, object[] prms = null)
    {
        var form = new TileUnitForm.Data(-1, "", new Dictionary<int, int>() { { 0, 1 } }, mapPos, GetPrefabName("mapground"), Z_Math.Graph.ElementwiseMultiply(mapPos, mainData.mapUnitSize), Vector3.zero, Vector3.one, 0, new List<int>(), "",false,false, 0);
        //init
        var dic = form.unit.evtDic;
        var pdt = form.unit.productInfo;
        ApplyTilePassTypes(form);

        return form;
    }

    public override List<TileUnitForm.Data> GetTileDatasByJa(string ja)
    {
        var result = TileUnitForm.GetDatasByJa(JArray.Parse(mainData.mapJa));
        foreach (var data in result)
            ApplyTilePassTypes(data);
        return result;
    }
    public override bool CheckItemUnit(ItemUnitForm.Data item)
    {
        int id = item.unit.productInfo.Item1;
        if (ItemProductForm.DataByUid.ContainsKey(id))
        {
            if (ItemProductForm.DataByUid[id].name == item.name)
                return base.CheckItemUnit(item);
        }
        return false;
    }
    public override bool CheckObjectUnit(ObjectUnitForm.Data obj)
    {
        return base.CheckObjectUnit(obj);
    }
    public override bool CheckCharacterUnit(CharacterUnitForm.Data ch)
    {
        int id = ch.unit.productInfo.Item1;
        if (CharacterProductForm.DataByUid.ContainsKey(id))
        {
            if (CharacterProductForm.DataByUid[id].name == ch.name)
                return base.CheckCharacterUnit(ch);
        }
        return false;
    }

}


