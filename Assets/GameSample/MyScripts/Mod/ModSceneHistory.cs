using System;
using System.Collections.Generic;
using System.Linq;
using Form;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Z_Map;
using Z_Map.Form;
using Z_UnitSystem.Form;

/// <summary>Explicit, row-local Form transactions; never snapshots or reloads the whole map.</summary>
public sealed class ModSceneHistory
{
    private const int Capacity = 10;
    private readonly MapManager map;
    private readonly MapInfo sceneMap;
    private readonly int sceneUid;
    private readonly List<List<RowChange>> undo = new List<List<RowChange>>();
    private readonly List<List<RowChange>> redo = new List<List<RowChange>>();
    private readonly Dictionary<(Table table, int uid), JObject> pending =
        new Dictionary<(Table, int), JObject>();
    private Func<bool> shouldRecord;
    private int depth;
    private int generation;
    private bool applying;

    public int undoCount => undo.Count;
    public int redoCount => redo.Count;
    public int pendingRowCount => pending.Count;
    public int lastCapturedRowCount { get; private set; }

    public ModSceneHistory(MapManager map, int sceneUid = 0)
    {
        this.map = map;
        sceneMap = map.data;
        this.sceneUid = sceneUid;
    }

    private bool isCurrentScene => map.data != null && ReferenceEquals(map.data, sceneMap);
    private bool canTrack => depth > 0 && !applying && isCurrentScene;

    public IDisposable BeginOperation(Func<bool> shouldRecord = null)
    {
        if (applying || !isCurrentScene)
            return new Operation(null, 0);
        if (depth == 0)
        {
            pending.Clear();
            lastCapturedRowCount = 0;
            this.shouldRecord = shouldRecord;
        }
        depth++;
        return new Operation(this, generation);
    }

    /// <summary>Call before the first actual mutation/removal, including dictionary/extra writes.</summary>
    public void Track(UnitForm.Data data)
    {
        if (!canTrack || data == null || !TryGetTable(data, out var table))
            return;
        Track((table, data.uid), false);
    }

    /// <summary>Call after a newly allocated row has been registered and initialized.</summary>
    public void TrackAdded(UnitForm.Data data)
    {
        if (!canTrack || data == null || !TryGetTable(data, out var table))
            return;
        Track((table, data.uid), true);
    }

    public void TrackMain()
    {
        if (canTrack)
            Track((Table.Main, map.data.mainData.uid), false);
    }

    public void TrackScene()
    {
        if (canTrack && sceneUid != 0)
            Track((Table.Scene, sceneUid), false);
    }

    private void Track((Table table, int uid) key, bool added)
    {
        // Repeated frames, nested operations and repeat paints of one row are O(1).
        if (!pending.ContainsKey(key))
            pending.Add(key, added ? null : CaptureRow(key));
    }

    public void Clear()
    {
        generation++;
        depth = 0;
        pending.Clear();
        shouldRecord = null;
        lastCapturedRowCount = 0;
        undo.Clear();
        redo.Clear();
    }

    private void EndOperation(int startedGeneration)
    {
        if (generation != startedGeneration || depth == 0 || --depth != 0)
            return;
        var guard = shouldRecord;
        shouldRecord = null;
        if (!isCurrentScene)
        {
            Clear();
            return;
        }
        try
        {
            if (guard != null && !guard())
                return;
            var rows = new List<RowChange>(pending.Count);
            foreach (var row in pending)
            {
                var after = CaptureRow(row.Key);
                if (!JToken.DeepEquals(Comparable(row.Value, row.Key.table), Comparable(after, row.Key.table)))
                    rows.Add(new RowChange { key = row.Key, before = row.Value, after = after });
            }
            if (rows.Count == 0)
                return;
            undo.Add(rows);
            if (undo.Count > Capacity)
                undo.RemoveAt(0);
            redo.Clear();
        }
        finally { pending.Clear(); }
    }

    public bool TryUndo() => Apply(undo, redo, false);
    public bool TryRedo() => Apply(redo, undo, true);

    private bool Apply(List<List<RowChange>> source, List<List<RowChange>> target, bool forward)
    {
        if (depth != 0 || applying || source.Count == 0 || !isCurrentScene)
            return false;
        var rows = source[source.Count - 1];
        applying = true;
        lastCapturedRowCount = 0;
        try
        {
            ModSceneHistoryRestore.Apply(map, rows, forward);
            source.RemoveAt(source.Count - 1);
            target.Add(rows);
            return true;
        }
        finally { applying = false; }
    }

    private JObject CaptureRow((Table table, int uid) key)
    {
        JObject row;
        switch (key.table)
        {
            case Table.Main:
                row = MapMainForm.GetJoByData(map.data.mainData);
                row.Remove("mapJa");
                row.Remove("objectJa");
                row.Remove("characterJa");
                row.Remove("itemJa");
                break;
            case Table.Scene:
                if (!SceneForm.DataByUid.TryGetValue(key.uid, out var scene)) return null;
                row = SceneForm.GetJoByData(scene);
                break;
            case Table.Tile:
                if (!TileUnitForm.DataByUid.TryGetValue(key.uid, out var tile)) return null;
                row = TileUnitForm.GetJoByData(tile);
                break;
            case Table.Object:
                if (!ObjectUnitForm.DataByUid.TryGetValue(key.uid, out var obj)) return null;
                row = ObjectUnitForm.GetJoByData(obj);
                break;
            case Table.Character:
                if (!CharacterUnitForm.DataByUid.TryGetValue(key.uid, out var character)) return null;
                row = CharacterUnitForm.GetJoByData(character);
                break;
            case Table.Item:
                if (!ItemUnitForm.DataByUid.TryGetValue(key.uid, out var item)) return null;
                row = ItemUnitForm.GetJoByData(item);
                break;
            default: return null;
        }
        lastCapturedRowCount++;
        return row;
    }

    private static bool TryGetTable(UnitForm.Data data, out Table table)
    {
        if (data is TileUnitForm.Data) table = Table.Tile;
        else if (data is ObjectUnitForm.Data) table = Table.Object;
        else if (data is CharacterUnitForm.Data) table = Table.Character;
        else if (data is ItemUnitForm.Data) table = Table.Item;
        else { table = default; return false; }
        return true;
    }

    // Explicit shared types for local map restoration, not reflection dispatch.
    public enum Table { Main, Tile, Object, Character, Item, Scene }
    public sealed class RowChange
    {
        public (Table table, int uid) key;
        public JObject before;
        public JObject after;
    }

    private static JToken Comparable(JObject row, Table table)
    {
        if (row == null) return null;
        var copy = (JObject)row.DeepClone();
        if (table != Table.Main && table != Table.Scene)
        {
            copy.Remove("enteredScene");
            copy.Remove("collidingUnitUid");
            var extra = copy.Value<string>("extra");
            if (!string.IsNullOrWhiteSpace(extra))
                copy["extra"] = JObject.Parse(extra);
        }
        return Canonical(copy);
    }

    private static JToken Canonical(JToken token)
    {
        if (token is JObject obj)
            return new JObject(obj.Properties().OrderBy(p => p.Name, StringComparer.Ordinal)
                .Select(p => new JProperty(p.Name, Canonical(p.Value))));
        if (token is JArray array)
        {
            var values = array.Select(Canonical).ToList();
            if (values.Count > 0 && values.All(v => v is JObject o && o["k"] != null))
                values = values.OrderBy(v => v["k"].ToString(Formatting.None), StringComparer.Ordinal).ToList();
            return new JArray(values);
        }
        return token.DeepClone();
    }

    private sealed class Operation : IDisposable
    {
        private ModSceneHistory owner;
        private readonly int generation;
        public Operation(ModSceneHistory owner, int generation)
        {
            this.owner = owner;
            this.generation = generation;
        }
        public void Dispose()
        {
            var current = owner;
            owner = null;
            current?.EndOperation(generation);
        }
    }
}
