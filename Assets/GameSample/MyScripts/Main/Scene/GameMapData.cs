using Form;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_ByteSerialize;
using Z_DataSystem;
using Z_DataSystem.Form;
using Z_Debug;
using Z_DesignStyle;
using Z_Map;
using Z_Map.Form;
using Z_UnitSystem.Form;

    public class GameMapData:MapData
    {
      
    public override CharacterUnitForm.Data GetNewCharacter(string prefabName = "", bool isMine = false, object[] prms = null)
    {
        var form = new CharacterUnitForm.Data(-1, !isMine, Vector3.zero, 4, 4, 4, isMine, "", prefabName, Vector3.zero, Vector3.zero, Vector3.one, 0, "");
        //init
        var dic = form.unit.evtDic;
        var pdt = form.unit.productInfo;
        return form;
    }
   
    public override List<CharacterUnitForm.Data> GetCharacterDatasByJa(string ja)
    {
        return CharacterUnitForm.GetDatasByJa(JArray.Parse(mainData.characterJa));
        
    }

    public override ObjectUnitForm.Data GetNewObject(string prefabName = "", object[] prms = null)
    {
        var form= new ObjectUnitForm.Data(-1, false, "", prefabName, Vector3.zero, Vector3.zero, Vector3.one, 0, "");
        //init
        var dic=form.unit.evtDic;
        if (prms != null&&prms[0] is (string,int))
        {
            form.unit.productInfo= ((string, int))prms[0];
        }
        return form;
    }
   
    public override List<ObjectUnitForm.Data> GetObjectDatasByJa(string ja)
    {
        return ObjectUnitForm.GetDatasByJa(JArray.Parse(mainData.objectJa));
    }

    public override TileUnitForm.Data GetNewTile(Vector3Int mapPos, object[] prms = null)
    {
        var form = new TileUnitForm.Data(-1, "", new Dictionary<int, string>() { { 0, defaultTextureName } }, mapPos, mapName, Z_Math.Graph.ElementwiseMultiply(mapPos, mainData.mapUnitSize), Vector3.zero, Vector3.one, 0, "");
        //init
        var dic = form.unit.evtDic;
        var pdt = form.unit.productInfo;

        return form;
    }
 
    public override List<TileUnitForm.Data> GetTileDatasByJa(string ja)
    {
        return TileUnitForm.GetDatasByJa(JArray.Parse(mainData.mapJa));
    }

}


