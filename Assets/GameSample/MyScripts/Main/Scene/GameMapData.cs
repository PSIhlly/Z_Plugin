using Form;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        JObject extra = new JObject();
        extra[MapUnit.evtKey] = GameManager.instance.evtCtrl.GetEventJa(EventType.Tile);
        return new CharacterUnitForm.Data(-1, !isMine, Vector3.zero, 4, 4, 4, isMine, "", prefabName, Vector3.zero, Vector3.zero, Vector3.one, 0, extra.ToString());
    }
   
    public override List<CharacterUnitForm.Data> GetCharacterDatasByJa(string ja)
    {
        return CharacterUnitForm.GetDatasByJa(JArray.Parse(mainData.characterJa));
        
    }

    public override ObjectUnitForm.Data GetNewObject(string prefabName = "", object[] prms = null)
    {
        JObject extra = new JObject();
        extra[MapUnit.evtKey] = GameManager.instance.evtCtrl.GetEventJa(EventType.Object);
        return new ObjectUnitForm.Data(-1, false, "", prefabName, Vector3.zero, Vector3.zero, Vector3.one, 0, extra.ToString());
    }
   
    public override List<ObjectUnitForm.Data> GetObjectDatasByJa(string ja)
    {
        return ObjectUnitForm.GetDatasByJa(JArray.Parse(mainData.objectJa));
    }

    public override TileUnitForm.Data GetNewTile(Vector3Int mapPos, object[] prms = null)
    {
        JObject extra = new JObject();
        extra[MapUnit.evtKey] = GameManager.instance.evtCtrl.GetEventJa(EventType.Tile);
        return new TileUnitForm.Data(-1, "", new Dictionary<int, string>() { { 0, defaultTextureName } }, mapPos, mapName, Z_Math.Graph.ElementwiseMultiply(mapPos, mainData.mapUnitSize), Vector3.zero, Vector3.one, 0, extra.ToString());
    }
 
    public override List<TileUnitForm.Data> GetTileDatasByJa(string ja)
    {
        return TileUnitForm.GetDatasByJa(JArray.Parse(mainData.mapJa));
    }
}


