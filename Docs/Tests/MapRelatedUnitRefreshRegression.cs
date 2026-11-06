using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Form;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Map;
using Z_Map.Form;
using Z_UnitSystem;
using Z_UnitSystem.Form;
using Object = UnityEngine.Object;

// Only run in the runner's isolated Editor project; never open a scene or save.
public static class MapRelatedUnitRefreshRegression
{
    private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
    private const BindingFlags PrivateStatic = BindingFlags.Static | BindingFlags.NonPublic;
    private static readonly MethodInfo Refresh = typeof(MapUpdateController).GetMethod("UpdateRelatedUnit", PrivateInstance);
    private static readonly FieldInfo Buffers = typeof(MapUpdateController).GetField("relatedUnitBuffers", PrivateInstance);
    private static readonly List<GameObject> owned = new List<GameObject>();
    private static int checks, nextUid = 89500;
    private static InstancePoolManager pool;
    private static GameMapController appearance;

    public static void Run()
    {
        bool previousPlaying = DynamicGlobalSettings.playing;
        CameraMode previousCamera = DynamicGlobalSettings.cameraMode;
        try
        {
            RegisterForms();
            DynamicGlobalSettings.playing = false;
            DynamicGlobalSettings.cameraMode = CameraMode.Overhead;
            Check(Refresh != null && Buffers != null, "production visibility entry point and reusable buffer pool exist");
            foreach (Kind kind in Enum.GetValues(typeof(Kind)))
            {
                CheckShowMutation(kind);
                CheckHideMutation(kind);
                CheckMultipleOwners(kind);
            }
            CheckCaptureAllTypesBeforeCallbacks();
            CheckNestedRefresh();
            CheckExceptionAndReuse();
            CheckMissingReadDoesNotCreateKeys();
            CheckSingleUnitRefresh();
            CheckRealObjectShowRebindsReverseList();
            Debug.Log("MAP_RELATED_UNIT_REFRESH_REGRESSION_PASS checks=" + checks);
            EditorApplication.Exit(0);
        }
        catch (Exception error)
        {
            Debug.LogException(error);
            EditorApplication.Exit(1);
        }
        finally
        {
            if (appearance != null)
            {
                Z_EventHelper.Unregister<TileEvent>(appearance);
                Z_EventHelper.Unregister<ObjectEvent>(appearance);
                Z_EventHelper.Unregister<ItemEvent>(appearance);
            }
            DynamicGlobalSettings.playing = previousPlaying;
            DynamicGlobalSettings.cameraMode = previousCamera;
            if (pool?.pools != null) pool.pools.Clear();
            for (int i = owned.Count - 1; i >= 0; i--)
                if (owned[i] != null) Object.DestroyImmediate(owned[i]);
        }
    }

    private static void RegisterForms()
    {
        foreach (var type in new[] { typeof(UnitForm), typeof(TileUnitForm), typeof(ObjectUnitForm),
                     typeof(CharacterUnitForm), typeof(ItemUnitForm), typeof(MapMainForm), typeof(ProductForm),
                     typeof(ObjectProductForm), typeof(MapModelForm), typeof(EventTriggerForm), typeof(MapObjectParamForm),
                     typeof(GameParamForm), typeof(ParamForm) })
            type.GetMethod("Register", PrivateStatic)?.Invoke(null, null);
        UnitForm.InitInternal();
        TileUnitForm.InitInternal();
        ObjectUnitForm.InitInternal();
        CharacterUnitForm.InitInternal();
        ItemUnitForm.InitInternal();
        MapMainForm.InitInternal();
        ProductForm.InitInternal();
        ObjectProductForm.InitInternal();
        MapModelForm.InitInternal();
    }

    private static TileUnit Tile(bool visible, Vector3Int? position = null)
    {
        var data = TileUnitForm.GetDataByJo(new JObject());
        data.uid = nextUid++;
        data.mapPos = position ?? Vector3Int.zero;
        data.pos = data.mapPos;
        data.scale = Vector3.one;
        var tile = data.unit;
        var go = Go("related-refresh-tile");
        go.SetActive(false);
        tile.ins = go.AddComponent<TileInstance>();
        tile.ins.unit = tile;
        go.SetActive(visible);
        return tile;
    }

    private enum Kind { Character, Item, Object }
    private sealed class State
    {
        public int shows, hides;
        public Action show, hide;
        public void Show() { shows++; show?.Invoke(); }
        public bool Hide() { hides++; hide?.Invoke(); return true; }
    }
    private sealed class CharacterProbe : CharacterUnit
    {
        public readonly State state = new State();
        public CharacterProbe(CharacterUnitForm.Data data) : base(data) { }
        public override void Show() => state.Show();
        public override bool Hide() => state.Hide();
    }
    private sealed class ItemProbe : ItemUnit
    {
        public readonly State state = new State();
        public ItemProbe(ItemUnitForm.Data data) : base(data) { }
        public override void Show() => state.Show();
        public override bool Hide() => state.Hide();
    }
    private sealed class ObjectProbe : ObjectUnit
    {
        public readonly State state = new State();
        public ObjectProbe(ObjectUnitForm.Data data) : base(data) { }
        public override void Show() => state.Show();
        public override bool Hide() => state.Hide();
    }
    private sealed class Probe
    {
        public MapUnit unit;
        public State state;
        public Action<TileUnit> link;
        public Action unlink;
        public Func<bool> visible;
    }
    private static Probe NewProbe(Kind kind, MapUpdateController controller)
    {
        switch (kind)
        {
            case Kind.Character:
                var chData = CharacterUnitForm.GetDataByJo(new JObject());
                chData.uid = nextUid++;
                var ch = new CharacterProbe(chData);
                return new Probe { unit = ch, state = ch.state,
                    link = tile => controller.characterTileDic.Add(ch, tile),
                    unlink = () => controller.characterTileDic.Del(ch),
                    visible = () => controller.curCharacterLst.Contains(ch.data) };
            case Kind.Item:
                var itemData = ItemUnitForm.GetDataByJo(new JObject());
                itemData.uid = nextUid++;
                var item = new ItemProbe(itemData);
                return new Probe { unit = item, state = item.state,
                    link = tile => controller.itemTileDic.Add(item, tile),
                    unlink = () => controller.itemTileDic.Del(item),
                    visible = () => controller.curItemLst.Contains(item.data) };
            default:
                var objData = ObjectUnitForm.GetDataByJo(new JObject());
                objData.uid = nextUid++;
                var obj = new ObjectProbe(objData);
                return new Probe { unit = obj, state = obj.state,
                    link = tile => controller.objectTileDic.Add(obj, tile),
                    unlink = () => controller.objectTileDic.Del(obj),
                    visible = () => controller.curObjectLst.Contains(obj.data) };
        }
    }

    private static void CheckShowMutation(Kind kind)
    {
        var controller = new MapUpdateController(null);
        var shown = Tile(true);
        var hidden = Tile(false);
        var a = NewProbe(kind, controller); var b = NewProbe(kind, controller);
        var c = NewProbe(kind, controller); var d = NewProbe(kind, controller);
        var inserted = NewProbe(kind, controller);
        foreach (var p in new[] { a, b, c, d }) p.link(shown);
        a.state.show = () =>
        {
            a.state.show = null;
            a.unlink(); a.link(hidden); // Showing can remove its own visible owner.
            b.unlink();               // A captured peer can disappear before its turn.
            c.unlink(); c.link(shown); // Reordering the original live list must not skip c/d.
            inserted.link(shown);     // Newly added work must not extend this snapshot.
        };
        Update(controller, shown);
        Check(a.state.shows == 1 && a.state.hides == 1 && !a.visible(), kind + " self unlink is rechecked after Show");
        Check(b.state.shows == 0 && b.state.hides == 1 && !b.visible(), kind + " removed peer cannot be resurrected");
        Check(c.state.shows == 1 && d.state.shows == 1 && c.visible() && d.visible(), kind + " original surviving peers are not skipped");
        Check(inserted.state.shows == 0 && inserted.state.hides == 0, kind + " callbacks do not extend the active snapshot");
        Update(controller, shown);
        Check(inserted.state.shows == 1 && inserted.visible(), kind + " next refresh sees newly linked work");
        Check(Borrowed(controller).Count == 1 && Borrowed(controller).Peek().Count == 0, kind + " snapshot is cleared and returned");
    }

    private static void CheckHideMutation(Kind kind)
    {
        var controller = new MapUpdateController(null);
        var hidden = Tile(false);
        var a = NewProbe(kind, controller); var b = NewProbe(kind, controller);
        var c = NewProbe(kind, controller); var inserted = NewProbe(kind, controller);
        foreach (var p in new[] { a, b, c }) p.link(hidden);
        a.state.hide = () =>
        {
            a.state.hide = null;
            a.unlink(); b.unlink(); c.unlink(); c.link(hidden); inserted.link(hidden);
        };
        Update(controller, hidden);
        foreach (var p in new[] { a, b, c })
            Check(p.state.hides == 1 && p.state.shows == 0 && !p.visible(), kind + " Hide mutation retains original candidate coverage");
        Check(inserted.state.hides == 0, kind + " Hide insertion waits for a later snapshot");
    }

    private static void CheckMultipleOwners(Kind kind)
    {
        var controller = new MapUpdateController(null);
        var hidden = Tile(false); var shown = Tile(true);
        var p = NewProbe(kind, controller); p.link(hidden); p.link(shown);
        Update(controller, hidden);
        Check(p.state.shows == 1 && p.state.hides == 0 && p.visible(), kind + " any showing owner keeps a multi-Tile unit visible");
        shown.ins.gameObject.SetActive(false);
        Update(controller, shown);
        Check(p.state.hides == 1 && !p.visible(), kind + " last owner leaving view hides the unit");
    }

    private static void CheckCaptureAllTypesBeforeCallbacks()
    {
        var controller = new MapUpdateController(null);
        var tile = Tile(true);
        var character = NewProbe(Kind.Character, controller);
        var item = NewProbe(Kind.Item, controller);
        var obj = NewProbe(Kind.Object, controller);
        var inserted = NewProbe(Kind.Item, controller);
        character.link(tile); item.link(tile); obj.link(tile);
        character.state.show = () => { obj.unlink(); inserted.link(tile); };
        Update(controller, tile);
        Check(character.state.shows == 1 && item.state.shows == 1, "cross-type edits retain originally captured Character and Item");
        Check(obj.state.shows == 0 && obj.state.hides == 1, "earlier type can remove a captured later-type candidate safely");
        Check(inserted.state.shows == 0, "all three types are captured before the first callback");
    }

    private static void CheckNestedRefresh()
    {
        var controller = new MapUpdateController(null);
        var outer = Tile(true); var inner = Tile(true);
        var a = NewProbe(Kind.Object, controller); var b = NewProbe(Kind.Object, controller);
        var c = NewProbe(Kind.Item, controller); var d = NewProbe(Kind.Character, controller);
        a.link(outer); b.link(outer); c.link(inner); d.link(inner);
        a.state.show = () => { a.state.show = null; Update(controller, inner); };
        Update(controller, outer);
        foreach (var p in new[] { a, b, c, d }) Check(p.state.shows == 1 && p.visible(), "nested refresh cannot overwrite outer snapshot");
        var buffers = Borrowed(controller);
        Check(buffers.Count == 2, "nested calls rent independent reusable lists");
        foreach (var buffer in buffers) Check(buffer.Count == 0, "both nested lists release their unit references");
        var identities = buffers.ToArray();
        for (int i = 0; i < 30; i++) Update(controller, outer);
        Check(buffers.Count == 2 && Array.Exists(identities, x => ReferenceEquals(x, buffers.Peek())), "warm refresh reuses its bounded buffer set");
    }

    private sealed class ExpectedCallbackFailure : Exception { }
    private static void CheckExceptionAndReuse()
    {
        var controller = new MapUpdateController(null);
        var tile = Tile(true);
        var a = NewProbe(Kind.Object, controller); var b = NewProbe(Kind.Object, controller);
        a.link(tile); b.link(tile);
        a.state.show = () => { a.unlink(); a.link(tile); throw new ExpectedCallbackFailure(); };
        bool threw = false;
        try { Update(controller, tile); } catch (ExpectedCallbackFailure) { threw = true; }
        Check(threw, "callback failure is propagated, not swallowed as enumeration tolerance");
        Check(Borrowed(controller).Count == 1 && Borrowed(controller).Peek().Count == 0, "finally returns an empty buffer on callback failure");
        var reused = Borrowed(controller).Peek();
        a.state.show = null;
        Update(controller, tile);
        Check(a.visible() && b.visible() && b.state.shows == 1, "a later refresh succeeds after callback failure");
        Check(ReferenceEquals(reused, Borrowed(controller).Peek()), "exception recovery reuses the returned buffer");
    }

    private static void CheckMissingReadDoesNotCreateKeys()
    {
        var controller = new MapUpdateController(null);
        var tile = Tile(true);
        Update(controller, tile);
        Check(controller.characterTileDic.GetDicT1().Count == 0 && controller.characterTileDic.GetDicT2().Count == 0,
            "missing Character association reads do not create keys");
        Check(controller.itemTileDic.GetDicT1().Count == 0 && controller.itemTileDic.GetDicT2().Count == 0,
            "missing Item association reads do not create keys");
        Check(controller.objectTileDic.GetDicT1().Count == 0 && controller.objectTileDic.GetDicT2().Count == 0,
            "missing Object association reads do not create keys");
    }

    private static void CheckSingleUnitRefresh()
    {
        var controller = new MapUpdateController(null);
        var shown = Tile(true); var hidden = Tile(false);
        var p = NewProbe(Kind.Character, controller);
        p.link(hidden); p.link(shown);
        var character = (CharacterUnit)p.unit;
        p.state.show = () =>
        {
            p.state.show = null;
            controller.characterTileDic.Del(character, hidden);
            controller.characterTileDic.Del(character, shown);
            p.link(shown);
        };
        controller.UpdateSingleOne(p.unit);
        Check(p.state.shows == 1 && p.visible(), "UpdateSingleOne also tolerates forward-list rebinding during Show");
    }

    private static void CheckRealObjectShowRebindsReverseList()
    {
        var map = CreateManager<MapManager>();
        map.utilCtrl = new MapUtilController(map);
        map.updateCtrl = new MapUpdateController(map);
        map.data = new MapInfo
        {
            mainData = new MapMainForm.Data(1, Vector3.one, new Vector3Int(20, 5, 20), new Vector3Int(8, 3, 8), "", "", "", ""),
            maps = new Dictionary<(int, int, int), TileUnitForm.Data>(),
            mapXZ2Y = new Dictionary<(int, int), SortedSet<int>>()
        };
        var tile = Tile(true, new Vector3Int(5, 0, 5));
        map.data.RegisterMap(tile.data);
        pool = CreateManager<InstancePoolManager>();
        pool.defaultRoot = pool.transform;
        pool.Init();
        var product = ObjectProductForm.GetDataByJo(new JObject());
        product.uid = -1;
        product.name = "related-refresh-real-object";
        product.faceType = FaceType.FourDirection;
        product.model = new MapModelForm.Data(0, new List<int> { 1 }, new List<Vector3> { Vector3.zero },
            new List<Vector3> { Vector3.one }, new List<List<int>> { new List<int>() }, .1f, true, 1f);
        product.EnsureDirectionData();
        product.SetModelSize(AnimDirecton.Right, new Vector3(2, .8f, 1));
        product.SetModelPosition(AnimDirecton.Right, new Vector3(.2f, .1f, 0));
        ObjectProductForm.AddData(product);
        var prefab = Go(GlobalDefaultHelper.GetRuntimeMapObjectPrefabName(product.uid));
        prefab.SetActive(false);
        prefab.AddComponent<ObjectInstance>();
        var part = Go("first-part");
        part.transform.SetParent(prefab.transform, false);
        part.transform.localPosition = Vector3.up * .5f;
        var collider = Go("body");
        collider.transform.SetParent(part.transform, false);
        collider.AddComponent<BoxCollider>();
        pool.AddPool(prefab);
        appearance = new GameMapController(null); // Actual synchronous Object.Show listener.
        var units = new List<ObjectUnit>();
        for (int i = 0; i < 3; i++)
        {
            var data = ObjectUnitForm.GetDataByJo(new JObject());
            data.uid = nextUid++;
            data.pos = tile.data.pos;
            data.euler = new Vector3(0, 90, 0);
            data.scale = Vector3.one;
            ObjectProductForm.AssignProduct(data, product);
            ObjectUnitForm.AddData(data);
            units.Add(data.unit);
            map.updateCtrl.RefreshObjectOverlap(data.unit);
        }
        Check(map.updateCtrl.objectTileDic.TryGet(tile, out var live) && live.Count == 3,
            "real hidden Objects initially share a Tile reverse-index list");
        var version = typeof(List<ObjectUnit>).GetField("_version", PrivateInstance);
        int beforeVersion = (int)version.GetValue(live);
        Update(map.updateCtrl, tile);
        Check((int)version.GetValue(live) > beforeVersion, "actual Show appearance callbacks rebind the list being refreshed");
        foreach (var unit in units)
        {
            Check(unit.isShowing && map.updateCtrl.curObjectLst.Contains(unit.data), "all real Objects survive the same-pass Show/rebind lifecycle");
            Check(unit.ins.transform.GetChild(0).localScale == product.GetModelSize(AnimDirecton.Right),
                "real lifecycle applies the current facing size before returning");
            Check(unit.ins.transform.GetChild(0).localPosition == product.GetModelPosition(AnimDirecton.Right) + Vector3.up * .5f,
                "real lifecycle applies the current facing offset before returning");
            Check(map.updateCtrl.objectTileDic.Contains(unit, tile), "actual Object rebind retains a complete bidirectional association");
        }
    }

    private static Stack<List<MapUnit>> Borrowed(MapUpdateController controller) =>
        (Stack<List<MapUnit>>)Buffers.GetValue(controller);
    private static void Update(MapUpdateController controller, TileUnit tile)
    {
        try { Refresh.Invoke(controller, new object[] { tile }); }
        catch (TargetInvocationException error) when (error.InnerException != null)
        { ExceptionDispatchInfo.Capture(error.InnerException).Throw(); }
    }
    private static GameObject Go(string name) { var go = new GameObject(name); owned.Add(go); return go; }
    private static T CreateManager<T>() where T : Z_MonoSingleton<T>
    {
        var go = Go(typeof(T).Name); go.SetActive(false);
        var value = go.AddComponent<T>();
        typeof(Z_MonoSingleton<T>).GetField("_instance", PrivateStatic).SetValue(null, value);
        return value;
    }
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
        checks++;
    }
}
