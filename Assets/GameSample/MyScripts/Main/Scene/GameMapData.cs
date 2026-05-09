using Form;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;
using Z_DataSystem;
using Z_Map;
using Z_Map.Form;

public class GameMapData : MapInfo
{

    public override CharacterUnitForm.Data GetNewCharacter(string prefabName = "", bool isMine = false, string extra = "")
    {
        var form = new CharacterUnitForm.Data(-1, !isMine, Vector3.zero, 4, 4, 4, isMine, "", prefabName, Vector3.zero, Vector3.zero, Vector3.one, 0, new List<int>(), extra, false,"");
        //init
        var dic = form.unit.evtDic;
        var pdt = form.unit.productInfo;
        return form;
    }

    public override List<CharacterUnitForm.Data> GetCharacterDatasByJa(string ja)
    {
        return CharacterUnitForm.GetDatasByJa(JArray.Parse(mainData.characterJa));

    }
    public override ItemUnitForm.Data GetNewItem(string prefabName = "", object[] prms = null)
    {
        var form = new ItemUnitForm.Data(-1, "", prefabName, Vector3.zero, Vector3.zero, Vector3.one, 0, new List<int>(), "",false,"");
        return form;
    }
    public override ObjectUnitForm.Data GetNewObject(string prefabName = "", object[] prms = null)
    {
        var form = new ObjectUnitForm.Data(-1, false, "", prefabName, Vector3.zero, Vector3.zero, Vector3.one, 0, new List<int>(), "", false,"");
        return form;
    }

    public override List<ObjectUnitForm.Data> GetObjectDatasByJa(string ja)
    {
        return ObjectUnitForm.GetDatasByJa(JArray.Parse(mainData.objectJa));
    }

    public override TileUnitForm.Data GetNewTile(Vector3Int mapPos, object[] prms = null)
    {
        var form = new TileUnitForm.Data(-1, "", new Dictionary<int, string>() { { 0, defaultTextureName } }, mapPos, mapName, Z_Math.Graph.ElementwiseMultiply(mapPos, mainData.mapUnitSize), Vector3.zero, Vector3.one, 0, new List<int>(), "",false,false);
        //init
        var dic = form.unit.evtDic;
        var pdt = form.unit.productInfo;

        return form;
    }

    public override List<TileUnitForm.Data> GetTileDatasByJa(string ja)
    {
        return TileUnitForm.GetDatasByJa(JArray.Parse(mainData.mapJa));
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


