using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Form;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using Z_Code;
using Z_Code.Form;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Map;
using Z_Map.Form;
using Z_UnitSystem;
using Z_UnitSystem.Form;
using Object = UnityEngine.Object;

// Isolated command/ownership fixture. Never run against a loaded story or save.
public static class UnifiedDestroyRegression
{
    private const BindingFlags Hidden = BindingFlags.Instance | BindingFlags.NonPublic;
    private static readonly List<GameObject> owned = new List<GameObject>();
    private static readonly Probe probe = new Probe();
    private static MapManager map;
    private static GameEventController events;
    private static GameMapController mapEvents;
    private static PlaySceneController scene;
    private static InstancePoolManager pool;
    private static ProgressForm.Data progress;
    private static TileUnit owner;
    private static Dictionary<CharacterProductForm.Data, CharacterUnitForm.Data> characters;
    private static int checks, nextProduct = 73001, nextUnit = 75001;
    private static readonly Dictionary<Type, string> prefabs = new Dictionary<Type, string>();
    private static readonly List<int> destroyTrace = new List<int>();
    private static bool destroyGate;
    private static int programSequence, gatePolls, gateCancels;

    public static void Run()
    {
        try
        {
            SetUp();
            MetadataAndAliases();
            UniqueAndOrdinaryCharacters();
            CharacterPrototypeFanOut();
            ObjectProductAndSceneReferences();
            ItemProductAndSceneReferences();
            InvalidReferencesAndKindIsolation();
            SnapshotRejectsReplacementIdentity();
            BeforeDestroyTriggers();
            Debug.Log("UNIFIED_DESTROY_REGRESSION_PASS checks=" + checks);
            EditorApplication.Exit(0);
        }
        catch (Exception error)
        {
            Debug.LogException(error);
            EditorApplication.Exit(1);
        }
        finally
        {
            probe.Unregister<CharacterEvent>(); probe.Unregister<ObjectEvent>();
            probe.Unregister<ItemEvent>(); probe.Unregister<SceneActionEvent>();
            if (events != null)
            {
                events.Reset();
                events.sceneTriggerCtrl.Unregister<CollideEvent>(); events.sceneTriggerCtrl.Unregister<TileEvent>();
                events.sceneTriggerCtrl.Unregister<ItemEvent>(); events.sceneTriggerCtrl.Unregister<ObjectEvent>();
                events.sceneTriggerCtrl.Unregister<CharacterEvent>(); events.sceneTriggerCtrl.Unregister<StoryLifeEvent>();
                events.storyTriggerCtrl.Unregister<StoryLifeEvent>(); events.storyTriggerCtrl.Unregister<StoryItemEvent>();
                events.storyTriggerCtrl.Unregister<StoryCharacterEvent>(); events.storyTriggerCtrl.Unregister<CharacterSkillEvent>();
            }
            if (mapEvents != null)
            { mapEvents.Unregister<TileEvent>(); mapEvents.Unregister<ObjectEvent>(); mapEvents.Unregister<ItemEvent>(); }
            if (pool?.pools != null) pool.pools.Clear();
            for (int i = owned.Count - 1; i >= 0; i--)
                if (owned[i] != null) Object.DestroyImmediate(owned[i]);
        }
    }

    private static void SetUp()
    {
        foreach (var type in new[] { typeof(ProductForm), typeof(CharacterProductForm), typeof(ObjectProductForm),
            typeof(ItemProductForm), typeof(MapModelForm), typeof(EventTriggerForm), typeof(MapObjectParamForm),
            typeof(CharacterParamForm), typeof(GameParamForm), typeof(GameCmdDataForm), typeof(GameCmdTypeDataForm),
            typeof(ProgressForm), typeof(EventInterpretDataForm), typeof(UnitForm), typeof(CharacterUnitForm),
            typeof(ObjectUnitForm), typeof(ItemUnitForm), typeof(TileUnitForm) })
            type.GetMethod("Register", BindingFlags.Static | BindingFlags.NonPublic)?.Invoke(null, null);
        ProductForm.InitInternal(); CharacterProductForm.InitInternal(); ObjectProductForm.InitInternal();
        ItemProductForm.InitInternal(); ProgressForm.InitInternal(); EventInterpretDataForm.InitInternal();
        UnitForm.InitInternal(); CharacterUnitForm.InitInternal(); ObjectUnitForm.InitInternal();
        ItemUnitForm.InitInternal(); TileUnitForm.InitInternal();
        foreach (var type in new[] { typeof(SceneForm), typeof(ProgramDataForm), typeof(EventProgramDataForm) })
        {
            type.GetMethod("Register", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
            type.GetMethod("InitInternal", BindingFlags.Static | BindingFlags.Public).Invoke(null, null);
        }
        DynamicGlobalSettings.playing = true;
        var game = Manager<GameManager>();
        var play = Manager<PlayManager>();
        progress = ProgressForm.defaultData;
        progress.uid = 1;
        progress.team = new List<int>(); progress.teamActive = new List<int>(); progress.bag = new List<int>();
        ProgressForm.AddData(progress);
        events = new GameEventController(game);
        game.evtCtrl = events;
        var sceneData = SceneForm.defaultData; sceneData.uid = 1;
        SceneForm.AddData(sceneData); game.curScene = sceneData;
        mapEvents = new GameMapController(game);
        game.mapCtrl = mapEvents;
        // Test real Remove cleanup, without unrelated appearance rendering on Show.
        mapEvents.Unregister<TileEvent>(); mapEvents.Unregister<ObjectEvent>(); mapEvents.Unregister<ItemEvent>();
        scene = new PlaySceneController(play);
        play.sceneCtrl = scene;
        characters = new Dictionary<CharacterProductForm.Data, CharacterUnitForm.Data>();
        typeof(PlaySceneController).GetField("_characterDic", Hidden).SetValue(scene, characters);
        map = Manager<MapManager>();
        map.enable = true;
        map.data = new MapInfo
        {
            mainData = new MapMainForm.Data(1, Vector3.one, new Vector3Int(20, 5, 20),
                new Vector3Int(8, 3, 8), "", "", "", ""),
            maps = new Dictionary<(int, int, int), TileUnitForm.Data>(),
            mapXZ2Y = new Dictionary<(int, int), SortedSet<int>>()
        };
        map.utilCtrl = new MapUtilController(map);
        map.updateCtrl = new MapUpdateController(map);
        map.navigationCtrl = new Z_Map.Analysis.NavigationController(map);
        pool = Manager<InstancePoolManager>();
        pool.Init(); pool.pools = new Dictionary<string, InstancePool>(StringComparer.Ordinal);
        pool.defaultRoot = pool.transform;
        foreach (var type in new[] { typeof(CharacterInstance), typeof(ObjectInstance), typeof(ItemInstance), typeof(TileInstance) })
        {
            var prefab = Go("unified-destroy-" + type.Name);
            prefab.SetActive(false);
            prefab.AddComponent(type);
            prefab.AddComponent<MeshRenderer>();
            prefab.AddComponent<BoxCollider>();
            prefabs[type] = prefab.name;
            pool.AddPool(prefab);
        }
        var tile = new TileUnitForm.Data(72001, "", new Dictionary<int, int>(), new Vector3Int(5, 0, 5),
            prefabs[typeof(TileInstance)], new Vector3(5, 0, 5), Vector3.zero, Vector3.one,
            UpdateType.ShowOnly, new List<int>(), "", false, false, new List<int>());
        TileUnitForm.AddData(tile);
        map.data.RegisterMap(tile);
        owner = tile.unit;
        probe.Register<CharacterEvent>(); probe.Register<ObjectEvent>();
        probe.Register<ItemEvent>(); probe.Register<SceneActionEvent>();
    }

    private static void MetadataAndAliases()
    {
        typeof(DestroyCmd).GetMethod("Init", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
        Check(CmdBase.TryGetForm("Destroy", out var canonical) && canonical is GameCmdDataForm.Data data
            && data.uid == 100036 && data.labId == 10026 && data.prmTypes.SequenceEqual(new[] { "var" }),
            "Destroy has one var parameter under the scene category and preserves UID 100036");
        foreach (string alias in new[] { "DestroyCharacter", "DestroyObject" })
        {
            Check(CmdBase.TryGetForm(alias, out var legacy) && ReferenceEquals(legacy, canonical)
                && BaseData.cmdDic.ContainsKey(alias) && !GameCmdDataForm.DataByName.ContainsKey(alias),
                "legacy alias resolves the canonical row without a second chooser entry: " + alias);
            var program = ProgramDataForm.defaultData;
            program.name = "legacy-destroy-" + alias;
            Check(program.TryApplyCode(alias + "(\"\");", out _, out var aliasErrors),
                "legacy source compiles through alias metadata: " + string.Join("; ", aliasErrors));
            var execution = new InterpretDataForm.Data(-1, new List<BoxDataForm.Data>(),
                new Dictionary<string, BoxDataForm.Data>(), program, 0, -1, 0, null,
                new List<BoxDataForm.Data>(), 0);
            var result = execution.Interpret();
            Check(result.complete && result.errors.Count == 0 && execution.stack.Count == 0,
                "legacy bytecode resolves the canonical command with balanced stack: " + alias);
        }
        Check(new Compiler().TryCompile(canonical.defaultCode, out _, out _, out _, out _, out _, out var errors),
            "canonical default compiles: " + string.Join("; ", errors));
        Check(new DestroyCharacterCmd().GetName() == "Destroy" && new DestroyObjectCmd().GetName() == "Destroy",
            "legacy command classes share canonical behavior and metadata");
    }

    private static CharacterProductForm.Data Character(bool unique, int proto = 0)
    {
        var product = CharacterProductForm.defaultData;
        product.uid = nextProduct++; product.name = "destroy-character-" + product.uid;
        product.unique = unique; product.protoUid = proto;
        product.paramDic = new Dictionary<string, CharacterParamForm.Data>
        { ["Hp"] = new CharacterParamForm.Data(-1, "Hp", ValType.Float, "0", "17", "100", ParamShowType.Hide) };
        CharacterProductForm.AddData(product);
        return product;
    }
    private static ObjectProductForm.Data ObjectProduct(int proto = 0)
    {
        var product = ObjectProductForm.GetDataByJo(new JObject());
        product.uid = nextProduct++; product.name = "destroy-object-" + product.uid; product.protoUid = proto;
        ObjectProductForm.AddData(product);
        return product;
    }
    private static ItemProductForm.Data ItemProduct(int proto = 0)
    {
        var product = ItemProductForm.defaultData;
        product.uid = nextProduct++; product.name = "destroy-item-" + product.uid; product.protoUid = proto;
        ItemProductForm.AddData(product);
        return product;
    }
    private static MapUnit Spawn(ProductForm.Data product, int uid = -1)
    {
        uid = uid > 0 ? uid : nextUnit++;
        string extra = MapUnit.GetProductInfoString(product is ObjectProductForm.Data
            ? new JObject {  } : new JObject(),
            (product.uid, -1));
        MapUnit unit;
        if (product is CharacterProductForm.Data character)
        {
            var data = new CharacterUnitForm.Data(uid, false, Vector3.zero, 1, 1, 1, false, "",
                prefabs[typeof(CharacterInstance)], owner.data.pos, Vector3.zero, Vector3.one,
                UpdateType.ShowOnly, new List<int>(), extra, false, 0, new List<int>());
            CharacterUnitForm.AddData(data); characters[character] = data; unit = data.unit;
            map.updateCtrl.characterTileDic.Add(data.unit, owner);
            map.updateCtrl.RefreshCharacterOverlap(data.unit);
            map.updateCtrl.curCharacterLst.Add(data);
        }
        else if (product is ObjectProductForm.Data)
        {
            var data = new ObjectUnitForm.Data(uid, false, "", prefabs[typeof(ObjectInstance)], owner.data.pos,
                Vector3.zero, Vector3.one, UpdateType.ShowOnly, new List<int>(), extra, false, 0);
            ObjectUnitForm.AddData(data); unit = data.unit;
            map.updateCtrl.objectTileDic.Add(data.unit, owner);
            map.updateCtrl.curObjectLst.Add(data);
        }
        else
        {
            var data = new ItemUnitForm.Data(uid, "", prefabs[typeof(ItemInstance)], owner.data.pos,
                Vector3.zero, Vector3.one, UpdateType.ShowOnly, new List<int>(), extra, false, 0);
            ItemUnitForm.AddData(data); unit = data.unit;
            map.updateCtrl.itemTileDic.Add(data.unit, owner);
            map.updateCtrl.curItemLst.Add(data);
        }
        unit.Show();
        Check(unit.ins != null && unit.isVising, "spawn creates a visible owned scene instance");
        return unit;
    }
    private static void Execute(string handle, CmdBase command = null)
    {
        // The production map listener owns runtime Object Product disposal.
        mapEvents.Register<ObjectEvent>();
        try
        {
            var task = new InterpretAsyncTask(null);
            (command ?? new DestroyCmd()).Execute(new[] { CodeHelper.CreateBoxByStr(handle) },
                new Dictionary<string, BoxDataForm.Data>(), task);
            Check(task.IsComplete() && string.IsNullOrEmpty(task.error), "Destroy completes synchronously: " + task.error);
        }
        finally { mapEvents.Unregister<ObjectEvent>(); }
    }
    private static string UnitHandle(MapUnit unit) => GlobalEventHelper.GetSceneUnitReference(unit.data.uid);
    private static string CharacterHandle(int uid) => "$ch$" + uid + "$ch$";
    private static string ObjectHandle(int uid) => GlobalEventHelper.GetSceneObjectProductReference(uid);
    private static string ItemHandle(int uid) => "$it$" + uid + "$it$";

    private static void Removed(MapUnit unit, Instance instance)
    {
        Check(!UnitForm.DataByUid.ContainsKey(unit.data.uid) && unit.ins == null
            && !instance.gameObject.activeSelf && !instance.vising, "Destroy deregisters and releases the target instance now");
        Check(probe.removed.Count(value => value == unit.data.uid) == 1
            && probe.options.Count(value => value == unit.data.uid) == 1, "one Remove event clears one interaction option");
    }
    private static EventInterpretDataForm.Data OwnedTask(MapUnit unit, out InterpretAsyncTask task)
    {
        var data = new EventInterpretDataForm.Data(-1, new List<BoxDataForm.Data>(),
            new Dictionary<string, BoxDataForm.Data>(), null, 0, -1, unit.data.uid, null,
            new List<BoxDataForm.Data>(), 0, "onPerSecondEvent", 0) { sceneOwner = unit.data };
        EventInterpretDataForm.AddData(data);
        task = new InterpretAsyncTask(new Interpreter(data)); task.Run();
        events.StartTask(task, () => false);
        return data;
    }
    private static void UniqueAndOrdinaryCharacters()
    {
        foreach (bool unique in new[] { false, true })
        {
            var product = Character(unique);
            var hp = product.paramDic["Hp"];
            var unit = (CharacterUnit)Spawn(product);
            var instance = unit.ins;
            var pending = OwnedTask(unit, out var task);
            progress.team = new List<int> { product.uid }; progress.teamActive = new List<int> { product.uid };
            progress.characterUid = product.uid;
            typeof(PlaySceneController).GetField("_playerM", Hidden).SetValue(scene, unit.data);
            typeof(PlaySceneController).GetField("_playerG", Hidden).SetValue(scene, product);
            Execute(CharacterHandle(product.uid)); Removed(unit, instance);
            Check(pending.IsCancelled && task.IsCancelled(), "Character-owned event/task is cancelled");
            Check(CharacterProductForm.DataByUid.ContainsKey(product.uid) == unique,
                "unique Character retains Product; ordinary Character releases Product");
            Check(scene.playerM == null && scene.playerG == null && !characters.ContainsKey(product), "live Character lookup/control references are cleared");
            if (unique)
                Check(ReferenceEquals(CharacterProductForm.DataByUid[product.uid], product)
                    && ReferenceEquals(product.paramDic["Hp"], hp) && hp.v == "17"
                    && progress.team.SequenceEqual(new[] { product.uid }) && progress.teamActive.SequenceEqual(new[] { product.uid })
                    && progress.characterUid == product.uid, "unique Product state and party references survive");
            Execute(CharacterHandle(product.uid));
        }
        var legacy = Character(true);
        var old = Spawn(legacy); var view = old.ins;
        Execute(UnitHandle(old), new DestroyCharacterCmd()); Removed(old, view);
        Check(CharacterProductForm.DataByUid.ContainsKey(legacy.uid), "legacy class also preserves unique Product via Unit reference");
    }
    private static void CharacterPrototypeFanOut()
    {
        var prototype = Character(false);
        var first = Character(false, prototype.uid); var second = Character(false, prototype.uid);
        var a = Spawn(first); var b = Spawn(second); var av = a.ins; var bv = b.ins;
        var bystander = Spawn(Character(true));
        Execute(CharacterHandle(prototype.uid)); Removed(a, av); Removed(b, bv);
        Check(CharacterProductForm.DataByUid.ContainsKey(prototype.uid)
            && !CharacterProductForm.DataByUid.ContainsKey(first.uid) && !CharacterProductForm.DataByUid.ContainsKey(second.uid)
            && UnitForm.DataByUid.ContainsKey(bystander.data.uid), "prototype deletion snapshots every matching Character and preserves defaults/bystanders");
    }
    private static void ObjectProductAndSceneReferences()
    {
        var prototype = ObjectProduct();
        var first = ObjectProduct(prototype.uid); var second = ObjectProduct(prototype.uid);
        var a = Spawn(first); var b = Spawn(second); var av = a.ins; var bv = b.ins;
        var pending = OwnedTask(a, out var task);
        Execute(ObjectHandle(prototype.uid)); Removed(a, av); Removed(b, bv);
        Check(ObjectProductForm.DataByUid.ContainsKey(prototype.uid)
            && !ObjectProductForm.DataByUid.ContainsKey(first.uid) && !ObjectProductForm.DataByUid.ContainsKey(second.uid),
            "Object prototype deletes every runtime instance/Product, retaining its authored prototype");
        Check(pending.IsCancelled && task.IsCancelled(), "Object-owned work is cancelled");
        var exact = ObjectProduct(prototype.uid); var sibling = ObjectProduct(prototype.uid);
        a = Spawn(exact); b = Spawn(sibling); av = a.ins; bv = b.ins;
        Execute(ObjectHandle(exact.uid)); Removed(a, av);
        Check(UnitForm.DataByUid.ContainsKey(b.data.uid), "exact Object Product reference does not delete its sibling");
        Execute(UnitHandle(b), new DestroyObjectCmd()); Removed(b, bv);
    }
    private static void ItemProductAndSceneReferences()
    {
        var product = ItemProduct();
        var sibling = ItemProduct(product.uid);
        var a = Spawn(product); var b = Spawn(product); var av = a.ins; var bv = b.ins;
        var unrelated = Spawn(sibling);
        progress.bag = new List<int> { product.uid, sibling.uid };
        var pending = OwnedTask(a, out var task);
        Execute(ItemHandle(product.uid)); Removed(a, av); Removed(b, bv);
        Check(pending.IsCancelled && task.IsCancelled(), "Item-owned event/task is cancelled immediately");
        Check(ItemProductForm.DataByUid.ContainsKey(product.uid) && ItemProductForm.DataByUid.ContainsKey(sibling.uid)
            && progress.bag.SequenceEqual(new[] { product.uid, sibling.uid }) && UnitForm.DataByUid.ContainsKey(unrelated.data.uid),
            "Item Product handle only removes its matching drops, not Products, inventory or siblings");
        Execute(ItemHandle(product.uid));
        var view = unrelated.ins;
        Execute(UnitHandle(unrelated), new DestroyObjectCmd()); Removed(unrelated, view);
    }
    private static void InvalidReferencesAndKindIsolation()
    {
        var character = Character(true); var ch = Spawn(character);
        var obj = ObjectProduct(99999); var so = Spawn(obj);
        var item = ItemProduct(); var drop = Spawn(item);
        int count = UnitForm.DataByUid.Count;
        foreach (string invalid in new[] { null, "", "$ch$$ch$", "$ch$-1$ch$", "$it$+1$it$", "$it$ 1$it$",
            "$it$1$it$tail", "$it$0$it$", "$so$p:missing$so$", "$so$p:0$so$", "$ch$999999$ch$",
            "$it$999999$it$", "$so$999999$so$", "$ch$" + obj.uid + "$ch$", "$it$" + character.uid + "$it$",
            ObjectHandle(character.uid), UnitHandle(owner) }) Execute(invalid);
        Check(UnitForm.DataByUid.Count == count && UnitForm.DataByUid.ContainsKey(ch.data.uid)
            && UnitForm.DataByUid.ContainsKey(so.data.uid) && UnitForm.DataByUid.ContainsKey(drop.data.uid),
            "malformed, missing, mismatched Product kinds and Tile references never delete bystanders");
    }
    private static void SnapshotRejectsReplacementIdentity()
    {
        var prototype = Character(false);
        var a = Spawn(Character(false, prototype.uid)); var b = (CharacterUnit)Spawn(Character(false, prototype.uid));
        ObjectUnit replacement = null;
        probe.afterCharacterRemove = unit =>
        {
            if (!ReferenceEquals(unit, a)) return;
            probe.afterCharacterRemove = null;
            scene.RemoveCharacter(b.data);
            replacement = (ObjectUnit)Spawn(ObjectProduct(99999), b.data.uid);
        };
        Execute(CharacterHandle(prototype.uid));
        Check(replacement != null && ReferenceEquals(UnitForm.DataByUid[replacement.data.uid], replacement.data)
            && replacement.ins != null, "a snapshot's stale Character cannot remove an Object created with its recycled UID during a callback");
    }

    private sealed class DestroyProbeCmd : CmdBase
    {
        public override string GetName() => "DestroyProbe";
        public override CmdBase GetNew() => new DestroyProbeCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask task)
        {
            int tag = (int)prm[0].num;
            var unit = GlobalEventHelper.ResolveSceneObjectUnit(prm[1].str);
            if (tag < 0) Check(unit == null, "caller continues only after the Unit is removed");
            else
            {
                Check(unit != null && unit.ins != null && UnitForm.DataByUid[unit.data.uid] == unit.data,
                    "destroy handler executes while its original Unit and visible instance still exist");
                Check(prm[1].str == UnitHandle(unit), "param1 names the exact destruction owner");
                Vector3 expected = GameManager.MapPosToPlayerPos(map.utilCtrl.RealPos2MapPos(unit.data.pos));
                Check(prm[2].dic["x"].num == expected.x && prm[2].dic["height"].num == expected.y
                    && prm[2].dic["y"].num == expected.z, "GetPosition(param1) reads the still-live owner's position before destruction");
            }
            destroyTrace.Add(tag);
            return true;
        }
    }

    private sealed class DestroyGateCmd : CmdBase
    {
        public override string GetName() => "DestroyGate";
        public override CmdBase GetNew() => new DestroyGateCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask task)
        {
            GameManager.instance.evtCtrl.StartTask(task, () =>
            {
                gatePolls++;
                if (!destroyGate) return false;
                task.Complete(); return true;
            }, () => gateCancels++);
            return false;
        }
    }

    private static EventProgramDataForm.Data Program(string code)
    {
        var program = new EventProgramDataForm.Data(-1, "before-destroy-" + ++programSequence,
            "", new List<string>(), new List<int>(), 0, "void", 0);
        Check(program.TryApplyCode(code, out _, out var errors), "destroy script compiles: " + string.Join(";", errors));
        EventProgramDataForm.AddData(program); return program;
    }
    private static EventTriggerForm.Data Hook(string code) => new EventTriggerForm.Data(-1, "onDestroyEvent",
        new List<int> { Program(code).uid }, TriggerType.NoLimit);
    private static string Trace(int tag, string reference = "param1") => "DestroyProbe(" + tag + "," + reference + ",GetPosition(" + reference + "));";
    private static string Quoted(MapUnit unit) => "\"" + UnitHandle(unit) + "\"";
    private static void LocalHook(MapUnit unit, string code)
    {
        var local = unit.evtDic; local["onDestroyEvent"] = Hook(code); unit.evtDic = local;
    }
    private static void Drain(Func<bool> done)
    {
        for (int i = 0; i < 24 && !done(); i++) events.LateUpdate();
        Check(done(), "destroy operation finishes without a blocking/reentrant deadlock");
    }
    private static void BeforeDestroyTriggers()
    {
        foreach (var type in new[] { typeof(PauseCmd), typeof(ContinueCmd), typeof(GetPositionCmd) })
            type.GetMethod("Init", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
        BaseData.cmdDic["DestroyProbe"] = new DestroyProbeCmd();
        BaseData.cmdDic["DestroyGate"] = new DestroyGateCmd();
        CmdDataForm.AddData(new CmdDataForm.Data(98601, "DestroyProbe", new List<string> { "tag", "owner", "position" },
            new List<string> { "num", "var", "var" }, null, new List<string> { "void" }, "", "DestroyProbe(0,param1,GetPosition(param1));"));
        CmdDataForm.AddData(new CmdDataForm.Data(98602, "DestroyGate", new List<string>(), new List<string>(),
            null, new List<string> { "void" }, "", "DestroyGate();"));
        mapEvents.Register<ObjectEvent>();
        try
        {
            events.Reset(); progress.blockProgramUid = 0; destroyTrace.Clear(); destroyGate = false;
            var product = ObjectProduct(99999); var obj = Spawn(product);
            product.events = new Dictionary<string, EventTriggerForm.Data>
            { ["onDestroyEvent"] = Hook(Trace(1) + "Destroy(param1);") };
            LocalHook(obj, "DestroyGate();" + Trace(2));
            events.Execute(Program("Pause();Destroy(" + Quoted(obj) + ");Continue();" + Trace(-3, Quoted(obj))), 0, null);
            events.LateUpdate();
            Check(UnitForm.DataByUid.ContainsKey(obj.data.uid) && destroyTrace.Count == 0 && progress.blockProgramUid != 0,
                "Destroy queues its hook through the normal next-pass scheduler before removing anything");
            events.LateUpdate(); events.LateUpdate();
            Check(destroyTrace.SequenceEqual(new[] { 1 }) && UnitForm.DataByUid.ContainsKey(obj.data.uid) && gatePolls > 0,
                "prototype executes before local appends, recursive Destroy(self) is harmless, and an async hook keeps the owner alive under Pause");
            destroyGate = true;
            Drain(() => destroyTrace.Contains(-3));
            Check(destroyTrace.SequenceEqual(new[] { 1, 2, -3 }) && progress.blockProgramUid == 0,
                "all inherited/local handlers finish before actual removal and caller continuation");
            Check(obj.evtDic["onDestroyEvent"].evt.Count == 1 && product.events["onDestroyEvent"].evt.Count == 1,
                "destroy execution never copies inherited event IDs into saved local bindings");

            foreach (bool unique in new[] { false, true })
            {
                destroyTrace.Clear(); var character = Character(unique); var unit = Spawn(character);
                character.events = new Dictionary<string, EventTriggerForm.Data> { ["onDestroyEvent"] = Hook(Trace(4)) };
                events.Execute(Program("Destroy(" + Quoted(unit) + ");"), 0, null);
                events.LateUpdate(); Check(UnitForm.DataByUid.ContainsKey(unit.data.uid), "Character survives until its pre-destroy event runs");
                Drain(() => !UnitForm.DataByUid.ContainsKey(unit.data.uid));
                Check(destroyTrace.SequenceEqual(new[] { 4 }) && CharacterProductForm.DataByUid.ContainsKey(character.uid) == unique,
                    "Character hook preserves ordinary versus unique Product cleanup");
            }
            destroyTrace.Clear(); var itemProduct = ItemProduct(); var item = Spawn(itemProduct); LocalHook(item, Trace(5));
            events.Execute(Program("Destroy(\"$it$" + itemProduct.uid + "$it$\");"), 0, null);
            Drain(() => !UnitForm.DataByUid.ContainsKey(item.data.uid));
            Check(destroyTrace.SequenceEqual(new[] { 5 }) && ItemProductForm.DataByUid.ContainsKey(itemProduct.uid),
                "Item hook runs before removal without deleting the stored Item Product");

            destroyTrace.Clear(); destroyGate = false;
            var cancelled = Spawn(ObjectProduct(99999)); LocalHook(cancelled, "DestroyGate();" + Trace(6));
            events.Execute(Program("Destroy(" + Quoted(cancelled) + ");"), 0, null);
            events.LateUpdate(); events.LateUpdate(); int cancelledBefore = gateCancels;
            events.ClearScene(); destroyGate = true; events.LateUpdate();
            Check(UnitForm.DataByUid.ContainsKey(cancelled.data.uid) && !destroyTrace.Contains(6) && gateCancels > cancelledBefore,
                "clearing a caller cancels its child hooks and pending removal together");
            events.Execute(Program("Destroy(" + Quoted(cancelled) + ");"), 0, null);
            Drain(() => !UnitForm.DataByUid.ContainsKey(cancelled.data.uid));
            Check(destroyTrace.SequenceEqual(new[] { 6 }), "cancellation releases the reservation so a later Destroy can run normally");

            destroyTrace.Clear(); destroyGate = false;
            var stale = (ObjectUnit)Spawn(ObjectProduct(99999)); LocalHook(stale, "DestroyGate();" + Trace(7));
            events.Execute(Program("Destroy(" + Quoted(stale) + ");"), 0, null);
            events.LateUpdate(); events.LateUpdate();
            map.RemoveObject(stale.data); var replacement = Spawn(ObjectProduct(99999), stale.data.uid);
            destroyGate = true; for (int i = 0; i < 4; i++) events.LateUpdate();
            Check(ReferenceEquals(UnitForm.DataByUid[replacement.data.uid], replacement.data) && !destroyTrace.Contains(7),
                "a reused UID cannot receive an old hook or its deferred removal");

            destroyTrace.Clear(); var a = Spawn(ObjectProduct(99999)); var b = Spawn(ObjectProduct(99999));
            LocalHook(a, "Destroy(" + Quoted(b) + ");" + Trace(8));
            LocalHook(b, "Destroy(" + Quoted(a) + ");" + Trace(9));
            events.Execute(Program("Pause();Destroy(" + Quoted(a) + ");Continue();"), 0, null);
            Drain(() => !UnitForm.DataByUid.ContainsKey(a.data.uid) && !UnitForm.DataByUid.ContainsKey(b.data.uid));
            Check(destroyTrace.SequenceEqual(new[] { 9, 8 }), "mutually recursive destruction hooks run once each without deadlock");
            Drain(() => progress.blockProgramUid == 0);
        }
        finally { mapEvents.Unregister<ObjectEvent>(); }
    }

    private sealed class Probe : IZ_Listener<CharacterEvent>, IZ_Listener<ObjectEvent>, IZ_Listener<ItemEvent>, IZ_Listener<SceneActionEvent>
    {
        public readonly List<int> removed = new List<int>(), options = new List<int>();
        public Action<CharacterUnit> afterCharacterRemove;
        private void Record(MapUnit unit)
        {
            Check(!UnitForm.DataByUid.ContainsKey(unit.data.uid) && unit.ins == null,
                "Remove dispatch occurs only after deregistration and instance release");
            removed.Add(unit.data.uid);
        }
        public void OnEvent(CharacterEvent value)
        { if (value.type == MapEventType.Remove) { Record(value.unit); afterCharacterRemove?.Invoke(value.unit); } }
        public void OnEvent(ObjectEvent value)
        { if (value.type == MapEventType.Remove) Record(value.unit); }
        public void OnEvent(ItemEvent value)
        { if (value.type == MapEventType.Remove) Record(value.unit); }
        public void OnEvent(SceneActionEvent value)
        { if (value.type == SceneActionEventType.Remove) options.Add(value.unitUid); }
    }
    private static T Manager<T>() where T : Z_MonoSingleton<T>
    {
        var root = Go(typeof(T).Name); root.SetActive(false);
        var manager = root.AddComponent<T>();
        typeof(Z_MonoSingleton<T>).GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, manager);
        return manager;
    }
    private static GameObject Go(string name)
    { var value = new GameObject(name); owned.Add(value); return value; }
    private static void Check(bool condition, string message)
    { if (!condition) throw new InvalidOperationException(message); checks++; }
}
