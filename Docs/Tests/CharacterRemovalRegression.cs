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

// Run only in the isolated runner project. No story, scene or save is loaded.
public static class CharacterRemovalRegression
{
    private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
    private static readonly List<GameObject> owned = new List<GameObject>();
    private static readonly Probe probe = new Probe();
    private static MapManager map;
    private static InstancePoolManager pool;
    private static PlaySceneController scene;
    private static GameEventController events;
    private static Dictionary<CharacterProductForm.Data, CharacterUnitForm.Data> characters;
    private static ProgressForm.Data progress;
    private static TileUnit owner;
    private static string prefabName;
    private static int checks, nextUnit = 62001;

    public static void Run()
    {
        try
        {
            SetUp();
            foreach (bool sceneReference in new[] { false, true })
            foreach (bool unique in new[] { false, true })
                Removal(sceneReference, unique);
            StaleDataCannotRemoveReplacement();
            Debug.Log("CHARACTER_REMOVAL_REGRESSION_PASS checks=" + checks);
            EditorApplication.Exit(0);
        }
        catch (Exception error)
        {
            Debug.LogException(error);
            EditorApplication.Exit(1);
        }
        finally
        {
            probe.Unregister<CharacterEvent>();
            probe.Unregister<SceneActionEvent>();
            if (events != null)
            {
                events.Reset();
                events.sceneTriggerCtrl.Unregister<CollideEvent>();
                events.sceneTriggerCtrl.Unregister<TileEvent>();
                events.sceneTriggerCtrl.Unregister<ItemEvent>();
                events.sceneTriggerCtrl.Unregister<ObjectEvent>();
                events.sceneTriggerCtrl.Unregister<CharacterEvent>();
                events.sceneTriggerCtrl.Unregister<StoryLifeEvent>();
                events.storyTriggerCtrl.Unregister<StoryLifeEvent>();
                events.storyTriggerCtrl.Unregister<StoryItemEvent>();
                events.storyTriggerCtrl.Unregister<StoryCharacterEvent>();
                events.storyTriggerCtrl.Unregister<CharacterSkillEvent>();
            }
            if (pool?.pools != null) pool.pools.Clear();
            for (int i = owned.Count - 1; i >= 0; i--)
                if (owned[i] != null) Object.DestroyImmediate(owned[i]);
        }
    }

    private static void SetUp()
    {
        foreach (var type in new[] { typeof(ProductForm), typeof(CharacterProductForm), typeof(ProgressForm),
            typeof(EventInterpretDataForm), typeof(CharacterParamForm), typeof(UnitForm),
            typeof(CharacterUnitForm), typeof(TileUnitForm) })
            type.GetMethod("Register", BindingFlags.Static | BindingFlags.NonPublic)?.Invoke(null, null);
        ProductForm.InitInternal(); CharacterProductForm.InitInternal(); ProgressForm.InitInternal();
        EventInterpretDataForm.InitInternal(); UnitForm.InitInternal(); CharacterUnitForm.InitInternal();
        TileUnitForm.InitInternal();
        var game = Manager<GameManager>();
        var play = Manager<PlayManager>();
        events = new GameEventController(game);
        game.evtCtrl = events;
        scene = new PlaySceneController(play);
        play.sceneCtrl = scene;
        characters = new Dictionary<CharacterProductForm.Data, CharacterUnitForm.Data>();
        typeof(PlaySceneController).GetField("_characterDic", PrivateInstance).SetValue(scene, characters);
        progress = ProgressForm.defaultData;
        progress.uid = 1;
        progress.team = new List<int>();
        progress.teamActive = new List<int>();
        ProgressForm.AddData(progress);

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
        pool.Init();
        pool.pools = new Dictionary<string, InstancePool>(StringComparer.Ordinal);
        pool.defaultRoot = pool.transform;
        var floor = Go("removal-floor");
        floor.SetActive(false);
        floor.AddComponent<TileInstance>();
        var floorBody = floor.AddComponent<BoxCollider>();
        floorBody.center = Vector3.down * .5f;
        pool.AddPool(floor);
        var tile = new TileUnitForm.Data(57001, "", new Dictionary<int, int>(), new Vector3Int(5, 0, 5),
            floor.name, new Vector3(5, 0, 5), Vector3.zero, Vector3.one, UpdateType.ShowOnly,
            new List<int>(), "", false, false, new List<int>());
        map.data.RegisterMap(tile);
        owner = tile.unit;
        var prefab = Go("removal-character");
        prefab.SetActive(false);
        prefab.AddComponent<CharacterInstance>();
        prefab.AddComponent<MeshRenderer>();
        prefab.AddComponent<BoxCollider>();
        prefabName = prefab.name;
        pool.AddPool(prefab);
        probe.Register<CharacterEvent>();
        probe.Register<SceneActionEvent>();
    }

    private static CharacterProductForm.Data Product(int id, bool unique)
    {
        var data = CharacterProductForm.defaultData;
        data.uid = id;
        data.name = "removal-product-" + id;
        data.unique = unique;
        data.paramDic = new Dictionary<string, CharacterParamForm.Data>
        {
            ["RemovalHp"] = new CharacterParamForm.Data(-1, "RemovalHp", ValType.Float,
                "0", "17", "100", ParamShowType.Hide)
        };
        CharacterProductForm.AddData(data);
        return data;
    }

    private static CharacterUnitForm.Data Unit(CharacterProductForm.Data product, int uid = -1)
    {
        var data = new CharacterUnitForm.Data(uid > 0 ? uid : nextUnit++, false, Vector3.zero,
            1, 1, 1, false, "", prefabName, owner.data.pos, Vector3.zero, Vector3.one, UpdateType.ShowOnly,
            new List<int>(), MapUnit.GetProductInfoString(new JObject(), (product.uid, -1)),
            false, 0, new List<int>());
        CharacterUnitForm.AddData(data);
        characters[product] = data;
        map.updateCtrl.characterTileDic.Add(data.unit, owner);
        map.updateCtrl.RefreshCharacterOverlap(data.unit);
        map.updateCtrl.curCharacterLst.Add(data);
        data.unit.Show();
        return data;
    }

    private static void Destroy(string handle)
    {
        var interpreter = new InterpretDataForm.Data(-1, new List<BoxDataForm.Data>(),
            new Dictionary<string, BoxDataForm.Data>(), null, 0, -1, 0, null, new List<BoxDataForm.Data>(), 0);
        var task = new InterpretAsyncTask(new Interpreter(interpreter));
        new DestroyCharacterCmd().Execute(new[] { CodeHelper.CreateBoxByStr(handle) }, interpreter.heap, task);
        Check(task.IsComplete() && string.IsNullOrEmpty(task.error), "DestroyCharacter finishes synchronously");
    }

    private static void Removal(bool sceneReference, bool unique)
    {
        int id = 61001 + (sceneReference ? 2 : 0) + (unique ? 1 : 0);
        var product = Product(id, unique);
        var parameters = product.paramDic;
        var hp = parameters["RemovalHp"];
        var unit = Unit(product);
        var instance = unit.unit.ins;
        string handle = sceneReference ? GlobalEventHelper.GetSceneUnitReference(unit.uid) : "$ch$" + product.uid + "$ch$";
        Check(scene.GetCharacterUnit(product.uid) == unit && unit.unit.isVising, "test Character is live and visible");
        if (unique)
        {
            progress.team = new List<int> { product.uid };
            progress.teamActive = new List<int> { product.uid };
            progress.characterUid = product.uid;
            typeof(PlaySceneController).GetField("_playerM", PrivateInstance).SetValue(scene, unit);
            typeof(PlaySceneController).GetField("_playerG", PrivateInstance).SetValue(scene, product);
        }
        Destroy(handle);
        Check(!instance.gameObject.activeSelf && !instance.vising && unit.unit.ins == null,
            "scene removal releases the visible instance immediately");
        Check(!CharacterUnitForm.DataByUid.ContainsKey(unit.uid) && !UnitForm.DataByUid.ContainsKey(unit.uid)
            && !map.updateCtrl.curCharacterLst.Contains(unit)
            && !map.updateCtrl.characterTileDic.TryGet(unit.unit, out _)
            && !map.updateCtrl.characterOverlapTileDic.TryGet(unit.unit, out _),
            "scene removal clears Unit registries, visible set, owner and overlap indexes");
        Check(CharacterProductForm.DataByUid.ContainsKey(product.uid) == unique
            && ProductForm.DataByUid.ContainsKey(product.uid) == unique,
            "only non-unique Products are removed from child and root registries");
        Check(scene.GetCharacterUnit(product.uid) == null && !characters.ContainsKey(product), "scene lookup cannot revive removal");
        Check(probe.removed.Count(value => value == unit.uid) == 1 && probe.options.Count(value => value == unit.uid) == 1,
            "removal and interaction cleanup are emitted exactly once");
        if (unique)
        {
            Check(ReferenceEquals(CharacterProductForm.DataByUid[product.uid], product)
                && ReferenceEquals(product.paramDic, parameters) && ReferenceEquals(product.paramDic["RemovalHp"], hp)
                && hp.v == "17", "unique Product and its mutable parameter state retain their original identities");
            Check(progress.team.SequenceEqual(new[] { product.uid }) && progress.teamActive.SequenceEqual(new[] { product.uid })
                && progress.characterUid == product.uid, "scene removal does not remove unique Product party references");
            Check(scene.playerM == null && scene.playerG == null, "only live player control references are cleared");
        }
        Destroy(handle);
        Check(probe.removed.Count(value => value == unit.uid) == 1, "repeating the same removal is harmless");
        if (unique)
        {
            int count = CharacterProductForm.DataByUid.Count;
            var recreated = Unit(product);
            Check(scene.GetCharacterUnit(product.uid) == recreated && recreated.unit.productInfo.Item1 == product.uid
                && CharacterProductForm.DataByUid.Count == count,
                "a new scene instance can reuse the exact preserved unique Product");
            Destroy("$ch$" + product.uid + "$ch$");
            Check(ReferenceEquals(CharacterProductForm.DataByUid[product.uid], product), "removing a recreated unique Character still retains Product");
        }
    }

    private static void StaleDataCannotRemoveReplacement()
    {
        var product = Product(61020, true);
        var stale = Unit(product);
        map.RemoveCharacter(stale);
        var other = Product(61021, true);
        var replacement = Unit(other, stale.uid);
        Check(!scene.RemoveCharacter(stale), "old Unit Data cannot remove a reused scene UID");
        Destroy("$ch$" + product.uid + "$ch$");
        Check(ReferenceEquals(CharacterUnitForm.DataByUid[replacement.uid], replacement)
            && ReferenceEquals(CharacterProductForm.DataByUid[product.uid], product),
            "an absent unique Character neither deletes its Product nor a replacement instance");
        Destroy("$ch$" + other.uid + "$ch$");
    }

    private sealed class Probe : IZ_Listener<CharacterEvent>, IZ_Listener<SceneActionEvent>
    {
        public readonly List<int> removed = new List<int>();
        public readonly List<int> options = new List<int>();
        public void OnEvent(CharacterEvent value)
        {
            if (value.type != MapEventType.Remove) return;
            Check(!UnitForm.DataByUid.ContainsKey(value.unit.data.uid) && value.unit.ins == null,
                "Character Remove callback runs after deregistration and instance cleanup");
            removed.Add(value.unit.data.uid);
        }
        public void OnEvent(SceneActionEvent value)
        {
            if (value.type == SceneActionEventType.Remove) options.Add(value.unitUid);
        }
    }

    private static GameObject Go(string name)
    {
        var value = new GameObject(name);
        owned.Add(value);
        return value;
    }
    private static T Manager<T>() where T : Z_MonoSingleton<T>
    {
        var root = Go(typeof(T).Name);
        root.SetActive(false);
        var value = root.AddComponent<T>();
        typeof(Z_MonoSingleton<T>).GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, value);
        return value;
    }
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
        checks++;
    }
}
