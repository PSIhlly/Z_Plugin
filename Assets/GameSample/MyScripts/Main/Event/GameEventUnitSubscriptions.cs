using System;
using System.Collections.Generic;
using Form;
using Z_Code;
using Z_Code.Form;
using Z_Map;
using Z_Map.Form;
using Z_UnitSystem.Form;

// Runtime-only subscriptions, owned by one event controller. No save schema.
internal sealed class GameEventUnitSubscriptions : IZ_Listener<EventModifyEvent>
{
    internal const string GlobalPrefix = "onGlobalVarChangeEvent$";
    private const string Second = "onPerSecondEvent";
    private sealed class Dispatch : Z_Event
    {
        internal GameEventUnitSubscriptions owner;
        internal readonly List<Subscription> targets = new List<Subscription>();
        internal readonly HashSet<Subscription> seen = new HashSet<Subscription>();
    }
    private sealed class Subscription : IZ_Listener<Dispatch>
    {
        internal MapUnit unit;
        internal GameEventUnitSubscriptions owner;
        internal long order;
        internal readonly HashSet<string> keys = new HashSet<string>(StringComparer.Ordinal);
        public void OnEvent(Dispatch evt)
        {
            if (ReferenceEquals(owner, evt.owner) && evt.seen.Add(this)) evt.targets.Add(this);
        }
        internal void Clear()
        {
            foreach (var key in keys) this.Unregister<Dispatch>(key);
            keys.Clear();
        }
    }
    private readonly GameEventController controller;
    private readonly Dictionary<MapUnit, Subscription> subscriptions = new Dictionary<MapUnit, Subscription>();
    private readonly HashSet<MapUnit> dirty = new HashSet<MapUnit>();
    private readonly Dictionary<object, object> pending = new Dictionary<object, object>();
    private readonly List<object> committed = new List<object>();
    private readonly List<MapUnit> refreshed = new List<MapUnit>();
    private readonly HashSet<(Type, int)> missingProducts = new HashSet<(Type, int)>();
    private readonly HashSet<string> keys = new HashSet<string>(StringComparer.Ordinal);
    private readonly Stack<Dispatch> dispatchPool = new Stack<Dispatch>();
    private Dictionary<int, UnitForm.Data> rows;
    private int rowCount;
    private long nextOrder;
    private bool tracking, rebuild = true;

    internal GameEventUnitSubscriptions(GameEventController controller)
    {
        this.controller = controller;
        Track();
    }

    private void Track()
    {
        if (tracking) return;
        tracking = true;
        UnitForm.addAction += Added;
        UnitForm.removeAction += Removed;
        UnitForm.changeExtraAction += ExtraChanged;
        TileUnitForm.changeTexdicAction += TileChanged;
        ObjectProductForm.changeEventsAction += ObjectChanged;
        CharacterProductForm.changeEventsAction += CharacterChanged;
        ItemProductForm.changeEventsAction += ItemChanged;
        MapTextureForm.changeEventsAction += TextureChanged;
        ObjectProductForm.addAction += ProductAdded;
        CharacterProductForm.addAction += ProductAdded;
        ItemProductForm.addAction += ProductAdded;
        MapTextureForm.addAction += TextureAdded;
        ObjectProductForm.removeAction += ProductRemoved;
        CharacterProductForm.removeAction += ProductRemoved;
        ItemProductForm.removeAction += ProductRemoved;
        MapTextureForm.removeAction += TextureRemoved;
        this.Register<EventModifyEvent>();
    }

    private void Added(UnitForm.Data data)
    {
        if (data.unit is MapUnit unit) Refresh(unit);
        rowCount = UnitForm.DataByUid.Count;
    }
    private void Removed(UnitForm.Data data)
    {
        if (data.unit is MapUnit unit) { dirty.Remove(unit); pending.Remove(unit); pending.Remove(data); Remove(unit); }
        rowCount = UnitForm.DataByUid.Count;
    }
    private void ExtraChanged(UnitForm.Data data, string before, string after)
    {
        // Generated SetActions run before assigning the backing field.
        if (before != after && data.unit is MapUnit unit) { dirty.Add(unit); pending[unit] = after; }
    }
    private void TileChanged(TileUnitForm.Data data, Dictionary<int, int> before, Dictionary<int, int> after)
    { if (data.unit != null) { dirty.Add(data.unit); pending[data] = after; } }
    private void ObjectChanged(ObjectProductForm.Data data, Dictionary<string, EventTriggerForm.Data> before, Dictionary<string, EventTriggerForm.Data> after)
    { var rows = ObjectProductForm.DataByUid; if (rows != null && rows.TryGetValue(data.uid, out var current) && ReferenceEquals(current, data)) Changed(data, after); }
    private void CharacterChanged(CharacterProductForm.Data data, Dictionary<string, EventTriggerForm.Data> before, Dictionary<string, EventTriggerForm.Data> after)
    { var rows = CharacterProductForm.DataByUid; if (rows != null && rows.TryGetValue(data.uid, out var current) && ReferenceEquals(current, data)) Changed(data, after); }
    private void ItemChanged(ItemProductForm.Data data, Dictionary<string, EventTriggerForm.Data> before, Dictionary<string, EventTriggerForm.Data> after)
    { var rows = ItemProductForm.DataByUid; if (rows != null && rows.TryGetValue(data.uid, out var current) && ReferenceEquals(current, data)) Changed(data, after); }
    private void TextureChanged(MapTextureForm.Data data, Dictionary<string, EventTriggerForm.Data> before, Dictionary<string, EventTriggerForm.Data> after)
    { var rows = MapTextureForm.DataById; if (rows != null && rows.TryGetValue(data.id, out var current) && ReferenceEquals(current, data)) Changed(data, after); }
    private void Changed(object source, object after) { pending[source] = after; rebuild = true; }
    private void ProductAdded(Z_DataSystem.Form.ProductForm.Data data)
    {
        var type = data is ObjectProductForm.Data ? typeof(ObjectProductForm)
            : data is CharacterProductForm.Data ? typeof(CharacterProductForm) : typeof(ItemProductForm);
        if (missingProducts.Contains((type, data.uid))) rebuild = true;
    }
    private void TextureAdded(MapTextureForm.Data data)
    { if (missingProducts.Contains((typeof(MapTextureForm), data.id))) rebuild = true; }
    private void ProductRemoved(Z_DataSystem.Form.ProductForm.Data data) => rebuild = true;
    private void TextureRemoved(MapTextureForm.Data data) => rebuild = true;
    public void OnEvent(EventModifyEvent evt) => rebuild = true;

    private static bool IsLive(MapUnit unit) => unit != null
        && UnitForm.DataByUid.TryGetValue(unit.data.uid, out var current) && ReferenceEquals(current, unit.data);

    private void AddKeys(Dictionary<string, EventTriggerForm.Data> events, bool perSecond)
    {
        if (events == null) return;
        foreach (var pair in events)
            if (pair.Value != null && (pair.Key.StartsWith(GlobalPrefix, StringComparison.Ordinal)
                || perSecond && pair.Key == Second)) keys.Add(pair.Key);
    }

    private void Refresh(MapUnit unit)
    {
        if (!IsLive(unit)) { Remove(unit); return; }
        keys.Clear();
        bool perSecond = unit is ItemUnit || unit is CharacterUnit || unit is ObjectUnit;
        var prototype = unit.GetPrototypeEvents();
        if (prototype == null)
        {
            if (unit is TileUnit tile && tile.data.texDic.TryGetValue(0, out int id)) missingProducts.Add((typeof(MapTextureForm), id));
            else if (perSecond)
                missingProducts.Add((unit is ObjectUnit ? typeof(ObjectProductForm)
                    : unit is CharacterUnit ? typeof(CharacterProductForm) : typeof(ItemProductForm), unit.productInfo.Item1));
        }
        AddKeys(prototype, perSecond);
        AddKeys(unit.evtDic, perSecond);
        subscriptions.TryGetValue(unit, out var subscription);
        if (subscription != null && subscription.keys.SetEquals(keys)) return;
        subscription?.Clear();
        if (keys.Count == 0) { subscriptions.Remove(unit); return; }
        if (subscription == null)
            subscriptions.Add(unit, subscription = new Subscription { unit = unit, owner = this, order = nextOrder++ });
        foreach (var key in keys)
        {
            subscription.keys.Add(key);
            subscription.Register<Dispatch>(key);
        }
    }

    private void Remove(MapUnit unit)
    {
        if (unit == null) return;
        if (subscriptions.TryGetValue(unit, out var subscription))
        { subscription.Clear(); subscriptions.Remove(unit); }
    }

    private void Ensure()
    {
        Track();
        var current = UnitForm.DataByUid;
        if (!ReferenceEquals(rows, current) || rowCount != current.Count) rebuild = true;
        if (rebuild)
        {
            ClearBindings();
            missingProducts.Clear();
            foreach (var data in current.Values) if (data.unit is MapUnit unit) Refresh(unit);
            rows = current; rowCount = current.Count; rebuild = false;
        }
        refreshed.Clear();
        foreach (var unit in dirty)
        {
            if (pending.TryGetValue(unit, out var extra) && !Equals(unit.data.extra, extra)) continue;
            if (unit is TileUnit tile && pending.TryGetValue(tile.data, out var texture) && !ReferenceEquals(tile.data.texDic, texture)) continue;
            Refresh(unit); refreshed.Add(unit);
        }
        foreach (var unit in refreshed) dirty.Remove(unit);
        refreshed.Clear(); committed.Clear();
        foreach (var pair in pending)
        {
            object value = pair.Key is MapUnit unit ? (object)unit.data.extra
                : pair.Key is TileUnitForm.Data tile ? tile.texDic
                : pair.Key is ObjectProductForm.Data obj ? obj.events
                : pair.Key is CharacterProductForm.Data ch ? ch.events
                : pair.Key is ItemProductForm.Data item ? item.events
                : ((MapTextureForm.Data)pair.Key).events;
            if (Equals(value, pair.Value)) committed.Add(pair.Key);
            else if (!(pair.Key is MapUnit) && !(pair.Key is TileUnitForm.Data)) rebuild = true;
        }
        foreach (var source in committed) pending.Remove(source);
        committed.Clear();
    }

    private Dispatch Collect(string first, string second = null)
    {
        Ensure();
        var dispatch = dispatchPool.Count > 0 ? dispatchPool.Pop() : new Dispatch();
        dispatch.owner = this;
        try
        {
            Z_EventHelper.Invoke(dispatch, first);
            if (second != null) Z_EventHelper.Invoke(dispatch, second);
            dispatch.targets.Sort((a, b) => a.order.CompareTo(b.order));
            return dispatch;
        }
        catch { Release(dispatch); throw; }
    }
    private void Release(Dispatch dispatch)
    {
        dispatch.targets.Clear(); dispatch.seen.Clear(); dispatch.owner = null; dispatchPool.Push(dispatch);
    }

    internal void GlobalChanged(GlobalVariableChangedEvent evt, Dictionary<string, BoxDataForm.Data> sceneHeap)
    {
        var scene = GameManager.instance.curScene;
        string specific = GlobalPrefix + evt.uid;
        var dispatch = Collect(GlobalPrefix, specific);
        try
        {
            foreach (var target in dispatch.targets)
            {
                if (!ReferenceEquals(scene, GameManager.instance.curScene)) break;
                var unit = target.unit;
                if (!IsLive(unit)) continue;
                var any = unit.GetEvt(GlobalPrefix);
                var exact = unit.GetEvt(specific);
                if (any == null && exact == null) continue;
                var heap = new Dictionary<string, BoxDataForm.Data>(sceneHeap)
                { ["param1"] = CodeHelper.CreateBoxByStr(GlobalEventHelper.GetSceneUnitReference(unit.data.uid)) };
                controller.TriggerEventExecute(any, unit.data.uid, heap, copyDefaultHeap: true);
                if (IsLive(unit) && ReferenceEquals(scene, GameManager.instance.curScene))
                    controller.TriggerEventExecute(exact, unit.data.uid, heap, copyDefaultHeap: true);
            }
        }
        finally { Release(dispatch); }
    }

    internal void EverySecond()
    {
        var scene = GameManager.instance.curScene;
        var dispatch = Collect(Second);
        try
        {
            // Preserve the existing Item -> Character -> Object dispatch groups.
            for (int kind = 0; kind < 3; kind++)
                foreach (var target in dispatch.targets)
                {
                    if (!ReferenceEquals(scene, GameManager.instance.curScene)) return;
                    var unit = target.unit;
                    if (!IsLive(unit) || (unit is ItemUnit ? 0 : unit is CharacterUnit ? 1 : 2) != kind) continue;
                    var trigger = unit.GetEvt(Second);
                    if (trigger == null) continue;
                    var heap = new Dictionary<string, BoxDataForm.Data> {
                        ["param1"] = CodeHelper.CreateBoxByStr(unit is CharacterUnit
                            ? GlobalEventHelper.GetName(GlobalEventHelper.CHARACTER, unit.productInfo.Item1.ToString())
                            : GlobalEventHelper.GetSceneObjectReference(unit)) };
                    controller.TriggerEventExecute(trigger, unit.data.uid, heap);
                }
        }
        finally { Release(dispatch); }
    }

    private void ClearBindings()
    {
        foreach (var item in subscriptions.Values) item.Clear();
        subscriptions.Clear();
    }

    internal void Reset()
    {
        ClearBindings(); dirty.Clear(); pending.Clear(); committed.Clear(); refreshed.Clear();
        missingProducts.Clear();
        rows = null; rowCount = 0; nextOrder = 0; rebuild = true;
        if (!tracking) return;
        tracking = false;
        UnitForm.addAction -= Added; UnitForm.removeAction -= Removed; UnitForm.changeExtraAction -= ExtraChanged;
        TileUnitForm.changeTexdicAction -= TileChanged;
        ObjectProductForm.changeEventsAction -= ObjectChanged; CharacterProductForm.changeEventsAction -= CharacterChanged;
        ItemProductForm.changeEventsAction -= ItemChanged; MapTextureForm.changeEventsAction -= TextureChanged;
        ObjectProductForm.addAction -= ProductAdded; CharacterProductForm.addAction -= ProductAdded; ItemProductForm.addAction -= ProductAdded;
        MapTextureForm.addAction -= TextureAdded;
        ObjectProductForm.removeAction -= ProductRemoved; CharacterProductForm.removeAction -= ProductRemoved; ItemProductForm.removeAction -= ProductRemoved;
        MapTextureForm.removeAction -= TextureRemoved;
        this.Unregister<EventModifyEvent>();
    }
}
