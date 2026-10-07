using System.Collections.Generic;
using Form;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Z_Map;
using Z_Map.Form;
using Z_UnitSystem.Form;

/// <summary>Apply changed Form rows without reloading the map or its unaffected units.</summary>
public static class ModSceneHistoryRestore
{
    public static void Apply(MapManager map, IReadOnlyList<ModSceneHistory.RowChange> rows, bool forward)
    {
        // Deserialize all targets before removing anything. Only changed rows are
        // parsed; fresh Unit instances also discard old lazy extra/event caches.
        var targets = new Dictionary<(ModSceneHistory.Table table, int uid), object>();
        var tilePositions = new HashSet<Vector3Int>();
        foreach (var row in rows)
        {
            var json = forward ? row.after : row.before;
            object target = json == null ? null : Deserialize(row.key.table, (JObject)json.DeepClone());
            targets.Add(row.key, target);
            if (row.key.table == ModSceneHistory.Table.Tile)
            {
                if (TileUnitForm.DataByUid.TryGetValue(row.key.uid, out var current))
                    tilePositions.Add(current.mapPos);
                if (target is TileUnitForm.Data tile)
                    tilePositions.Add(tile.mapPos);
            }
        }

        var affectedUnits = new HashSet<MapUnit>();
        foreach (var pos in tilePositions)
            map.updateCtrl.CollectHistoryColumn(pos, affectedUnits);

        // Entities first; their old footprint cleanup still has valid old Tiles.
        foreach (var row in rows)
            if (row.key.table != ModSceneHistory.Table.Tile)
                RemoveUnit(map, row.key.table, row.key.uid);
        foreach (var row in rows)
            if (row.key.table == ModSceneHistory.Table.Tile
                && TileUnitForm.DataByUid.TryGetValue(row.key.uid, out var tile))
            {
                GameManager.instance.mapCtrl?.ForgetSceneUnitCache(tile);
                map.data.RemoveTile(tile);
                map.updateCtrl.DetachHistoryTile(tile.unit);
            }

        foreach (var row in rows)
        {
            var value = targets[row.key];
            if (value is MapMainForm.Data main)
            {
                main.mapJa = map.data.mainData.mapJa;
                main.objectJa = map.data.mainData.objectJa;
                main.characterJa = map.data.mainData.characterJa;
                main.itemJa = map.data.mainData.itemJa;
                map.data.mainData.Reset(main);
            }
            else if (value is SceneForm.Data scene)
            {
                if (SceneForm.DataByUid.TryGetValue(scene.uid, out var current)) current.Reset(scene);
                else SceneForm.AddData(scene);
            }
            else if (row.key.table == ModSceneHistory.Table.Scene && value == null)
                SceneForm.RemoveData(row.key.uid);
            else if (value is TileUnitForm.Data tile)
            {
                TileUnitForm.AddData(tile);
                map.data.RegisterMap(tile);
                GameMapData.ApplyTilePassTypes(tile);
            }
        }

        // Terrain is installed before entities, retaining the NavigationController
        // and every unaffected NavUnit. Ground resampling is local too.
        map.navigationCtrl?.RefreshTerrainTiles(tilePositions);
        foreach (var row in rows)
        {
            var value = targets[row.key];
            if (value is ObjectUnitForm.Data obj)
            {
                ObjectUnitForm.AddData(obj);
                affectedUnits.Add(obj.unit);
            }
            else if (value is CharacterUnitForm.Data character)
            {
                CharacterUnitForm.AddData(character);
                GameMapData.ApplyCharacterProductSize(character);
                GameMapData.ApplyCharacterProductPassTypes(character);
                affectedUnits.Add(character.unit);
            }
            else if (value is ItemUnitForm.Data item)
            {
                ItemUnitForm.AddData(item);
                affectedUnits.Add(item.unit);
            }
        }
        map.updateCtrl.RefreshHistoryTiles(tilePositions);
        foreach (var pos in tilePositions)
            map.updateCtrl.CollectHistoryColumn(pos, affectedUnits);
        foreach (var unit in affectedUnits)
            if (UnitForm.DataByUid.TryGetValue(unit.data.uid, out var current)
                && ReferenceEquals(current, unit.data))
                map.updateCtrl.RefreshHistoryUnit(unit);
    }

    private static object Deserialize(ModSceneHistory.Table table, JObject json)
    {
        switch (table)
        {
            case ModSceneHistory.Table.Main: return MapMainForm.GetDataByJo(json);
            case ModSceneHistory.Table.Scene: return SceneForm.GetDataByJo(json);
            case ModSceneHistory.Table.Tile: return TileUnitForm.GetDataByJo(json);
            case ModSceneHistory.Table.Object: return ObjectUnitForm.GetDataByJo(json);
            case ModSceneHistory.Table.Character: return CharacterUnitForm.GetDataByJo(json);
            case ModSceneHistory.Table.Item: return ItemUnitForm.GetDataByJo(json);
            default: return null;
        }
    }

    private static void RemoveUnit(MapManager map, ModSceneHistory.Table table, int uid)
    {
        UnitForm.Data data = null;
        if (table == ModSceneHistory.Table.Object && ObjectUnitForm.DataByUid.TryGetValue(uid, out var obj))
            data = obj;
        else if (table == ModSceneHistory.Table.Character && CharacterUnitForm.DataByUid.TryGetValue(uid, out var character))
            data = character;
        else if (table == ModSceneHistory.Table.Item && ItemUnitForm.DataByUid.TryGetValue(uid, out var item))
            data = item;
        if (data == null) return;
        if (data is ObjectUnitForm.Data objectData) map.RemoveObject(objectData);
        else if (data is CharacterUnitForm.Data characterData)
        {
            GameManager.instance.characterCtrl?.UnregisterAnim(characterData);
            GameManager.instance.characterCtrl?.animControllerDic.Remove(characterData);
            map.RemoveCharacter(characterData);
        }
        else if (data is ItemUnitForm.Data itemData) map.RemoveItem(itemData);
        // Object removal listeners need the cached old WangTile footprint to
        // refresh surviving neighbours before this row's cache is discarded.
        GameManager.instance.mapCtrl?.ForgetSceneUnitCache(data);
    }
}
