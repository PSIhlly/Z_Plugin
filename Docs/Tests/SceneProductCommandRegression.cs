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
using Z_DesignStyle;
using Z_Map;
using Z_Map.Form;
using Z_UnitSystem;
using Z_UnitSystem.Form;

// Standalone command fixture: no production scene/save and no renderer test subclasses.
public static class SceneProductCommandRegression
{
    private static int checks;
    private static MapManager map;
    private static GameEventController events;
    private static string prefabName;
    private static readonly FieldInfo Tasks = typeof(GameEventController).GetField("tasks", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly MethodInfo Clear = typeof(GameEventController).GetMethod("CancelAllTasks", BindingFlags.Instance | BindingFlags.NonPublic);

    public static void Run()
    {
        bool playing = DynamicGlobalSettings.playing;
        bool allowDrop = DynamicGlobalSettings.allowDrop;
        try
        {
            SetUp();
            DynamicGlobalSettings.playing = true;
            DynamicGlobalSettings.allowDrop = true;
            var characterProto = CharacterProductForm.defaultData;
            characterProto.uid = 97500;
            CharacterProductForm.AddData(characterProto);
            var characterA = Character(97501, 97511, characterProto.uid, new Vector3(5, 0, 5));
            var characterB = Character(97502, 97512, characterProto.uid, new Vector3(7, 0, 5));
            Check(GameCmdDataForm.DataByName.TryGetValue("GenerateCharacter", out var generateMetadata)
                && generateMetadata.prmTypes[0] == "character" && generateMetadata.retTypes[0] == "character",
                "GenerateCharacter exposes its character input and return type");
            var generateType = typeof(GameManager).Assembly.GetType("Z_Code.GenerateCharacterCmd");
            Check(generateType != null, "GenerateCharacter runtime command exists");
            var invalidGeneration = Invoke((CmdBase)Activator.CreateInstance(generateType), Text("invalid"), Vector(Vector3.zero));
            Check(invalidGeneration.IsComplete() && invalidGeneration.res[0].num == 0,
                "invalid character generation returns zero without placing a character");
            Equal(Read(Invoke(new GetPositionCmd(), Text("$ch$ +97500 $ch$")).res[0]),
                GameManager.MapPosToPlayerPos(characterA.data.pos), "scene getters accept the existing signed/padded Character UID");
            CheckGroup("Character", "$ch$97500$ch$", "$ch$97501$ch$", new MapUnit[] { characterA, characterB });

            var objectTemplate = MapObjectForm.defaultData;
            objectTemplate.id = 97600;
            objectTemplate.name = "scene-command-template";
            objectTemplate.objectUid = 97601;
            MapObjectForm.AddData(objectTemplate);
            var objectPrototype = ObjectProductForm.defaultData;
            objectPrototype.uid = 97601;
            objectPrototype.name = "object-prototype";
            ObjectProductForm.AddData(objectPrototype);
            var objectA = Object(97602, 97611, 97601, objectTemplate.id, new Vector3(5, 0, 7));
            var objectB = Object(97603, 97612, 97601, objectTemplate.id, new Vector3(7, 0, 7));
            int objectCount = ObjectUnitForm.DataByUid.Count;
            int characterCount = CharacterUnitForm.DataByUid.Count;
            var unsupported = Vector(GameManager.MapPosToPlayerPos(new Vector3(100, 0, 100)));
            var objectResult = Invoke(new GenerateObjectCmd(), Text("$so$97601$so$"), unsupported);
            var characterResult = Invoke((CmdBase)Activator.CreateInstance(generateType), Text("$ch$97500$ch$"), unsupported);
            Check(objectResult.res[0].num == 0 && characterResult.res[0].num == 0
                && ObjectUnitForm.DataByUid.Count == objectCount && CharacterUnitForm.DataByUid.Count == characterCount,
                "generation outside the map returns zero without creating Object or Character units");
            CheckGroup("Object", "$so$97601$so$", "$so$97602$so$", new MapUnit[] { objectA, objectB });
            foreach (string alias in new[] { "$so$97601$so$" })
            {
                objectA.data.euler = Vector3.zero;
                objectB.data.euler = Vector3.zero;
                Invoke(new SetRotationCmd(), Text(alias), Number(90), Number(0));
                Check(objectA.data.euler.y == 90 && objectB.data.euler.y == 90,
                    "legacy/prototype Object alias rotates all current placements");
            }
            Invoke(new SetPositionRelativeCmd(), Text(GlobalEventHelper.GetSceneUnitReference(97611)), Vector(Vector3.right));
            Check(objectA.data.pos.x == 6 && objectB.data.pos.x == 7,
                "explicit scene-Unit reference still mutates exactly one Unit");
            Check(Read(Invoke(new GetPositionCmd(), Text("$so$t:999999$so$")).res[0]) == Vector3.zero,
                "missing template getter remains a zero vector");
            map.data.mainData.mapUnitSize = new Vector3(2, 3, 4);
            CheckGroup("nonuniform Character", "$ch$97500$ch$", "$ch$97501$ch$", new MapUnit[] { characterA, characterB });
            CheckGroup("nonuniform Object", "$so$97601$so$", "$so$97602$so$", new MapUnit[] { objectA, objectB });
            CheckWorldUnits(characterA, characterB, objectA);
            CheckUnchangedRotation(characterA, objectA);
            Debug.Log("SCENE_PRODUCT_COMMAND_REGRESSION_PASS checks=" + checks);
            EditorApplication.Exit(0);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            EditorApplication.Exit(1);
        }
        finally
        {
            if (events != null) Clear.Invoke(events, null);
            DynamicGlobalSettings.playing = playing;
            DynamicGlobalSettings.allowDrop = allowDrop;
        }
    }

    private static void SetUp()
    {
        // EditMode does not run RuntimeInitializeOnLoadMethod table registration.
        GameCmdDataForm.InitInternal();
        EventInterpretDataForm.InitInternal();
        var game = CreateManager<GameManager>();
        var play = CreateManager<PlayManager>();
        map = CreateManager<MapManager>();
        map.data = new MapInfo
        {
            mainData = new MapMainForm.Data(1, Vector3.one, new Vector3Int(20, 2, 20),
                new Vector3Int(10, 1, 10), "", "", "", ""),
            maps = new Dictionary<(int, int, int), TileUnitForm.Data>(),
            mapXZ2Y = new Dictionary<(int, int), SortedSet<int>>()
        };
        map.utilCtrl = new MapUtilController(map);
        map.updateCtrl = new MapUpdateController(map);
        // No navigation build, map Begin, renderer, camera, or production prefab is needed.
        var pool = CreateManager<InstancePoolManager>();
        pool.Init();
        pool.defaultRoot = pool.transform;
        var prefab = new GameObject("scene-command-empty-prefab");
        prefab.SetActive(false);
        pool.AddPool(prefab);
        prefabName = prefab.name;
        UnitForm.InitInternal();
        TileUnitForm.InitInternal();
        ObjectUnitForm.InitInternal();
        CharacterUnitForm.InitInternal();
        CharacterProductForm.InitInternal();
        MapObjectForm.InitInternal();
        ObjectProductForm.InitInternal();
        for (int x = 0; x < 15; x++)
        for (int z = 0; z < 15; z++)
            map.data.RegisterMap(new TileUnitForm.Data(97000 + x * 15 + z, "", new Dictionary<int, int>(),
                new Vector3Int(x, 0, z), prefabName, new Vector3(x, 0, z), Vector3.zero, Vector3.one,
                UpdateType.ShowOnly, new List<int>(), "", true, false, new List<int>()));
        events = new GameEventController(game);
        game.evtCtrl = events;
        play.sceneCtrl = new PlaySceneController(play);
    }

    private static CharacterUnit Character(int productUid, int unitUid, int protoUid, Vector3 pos)
    {
        var product = CharacterProductForm.defaultData;
        product.uid = productUid;
        product.protoUid = protoUid;
        CharacterProductForm.AddData(product);
        var data = new CharacterUnitForm.Data(unitUid, false, Vector3.zero, 1, 1, 1, false, "", prefabName,
            pos, Vector3.zero, Vector3.one, UpdateType.ShowOnly, new List<int>(), "", true, 0, new List<int>());
        data.unit.productInfo = (productUid, -1);
        CharacterUnitForm.AddData(data);
        UnitForm.DataByUid[data.uid] = data;
        map.updateCtrl.characterTileDic.Add(data.unit, map.utilCtrl.GetTile(5, 0, 5));
        return data.unit;
    }

    private static ObjectUnit Object(int productUid, int unitUid, int protoUid, int templateId, Vector3 pos)
    {
        var product = ObjectProductForm.defaultData;
        product.uid = productUid;
        product.name = "scene-command-product-" + productUid;
        product.protoUid = protoUid;
        ObjectProductForm.AddData(product);
        var data = new ObjectUnitForm.Data(unitUid, true, "scene-command-object", prefabName, pos,
            Vector3.zero, Vector3.one, UpdateType.ShowOnly, new List<int>(),
            MapUnit.GetProductInfoString(new JObject(),
                (productUid, -1)), false, 0);
        ObjectUnitForm.AddData(data);
        UnitForm.DataByUid[data.uid] = data;
        map.updateCtrl.objectTileDic.Add(data.unit, map.utilCtrl.GetTile(5, 0, 7));
        return data.unit;
    }

    private static void CheckGroup(string family, string template, string exact, MapUnit[] units)
    {
        var initial = units.Select(unit => unit.data.pos).ToArray();
        Invoke(new SetPositionRelativeCmd(), Text(template), Vector(Vector3.forward));
        for (int i = 0; i < units.Length; i++)
            Equal(units[i].data.pos, initial[i] + Vector3.forward, family + " relative Set keeps each own origin");
        Equal(Read(Invoke(new GetPositionCmd(), Text(template)).res[0]),
            GameManager.MapPosToPlayerPos(units[0].data.pos),
            family + " scalar/vector getter selects deterministic first target");
        // Keep both typed placements and their warm index, but invalidate the root identity of the first.
        var first = units[0].data;
        UnitForm.DataByUid[first.uid] = units[1].data;
        try
        {
            Equal(Read(Invoke(new GetPositionCmd(), Text(template)).res[0]),
                GameManager.MapPosToPlayerPos(units[1].data.pos), family + " getter skips a stale lowest-UID placement");
            Equal(Read(Invoke(new GetPositionCmd(), Text(exact)).res[0]), Vector3.zero,
                family + " getter rejects a stale exact placement");
        }
        finally
        {
            UnitForm.DataByUid[first.uid] = first;
        }
        var second = units[1].data.pos;
        Invoke(new SetPositionRelativeCmd(), Text(exact), Vector(Vector3.right));
        Equal(units[0].data.pos, initial[0] + Vector3.forward + Vector3.right, family + " exact Product moves its target");
        Equal(units[1].data.pos, second, family + " exact Product leaves its sibling untouched");
        foreach (CmdBase alias in new CmdBase[] { new SetCharacterRotationCmd(), new SetObjectRotationCmd() })
        {
            units[0].data.euler = new Vector3(1, 350, 2);
            units[1].data.euler = new Vector3(3, 10, 4);
            Invoke(alias, Text(template), Number(90), Number(0));
            Equal(units[0].data.euler, new Vector3(1, 90, 2), family + " rotation alias preserves first X/Z");
            Equal(units[1].data.euler, new Vector3(3, 90, 4), family + " rotation alias fans out and preserves second X/Z");
        }

        Restore(units, initial);
        var moving = Invoke(new MoveRelativeCmd(), Text(template), Vector(Vector3.forward * 2), Number(2));
        float delta = MapUtilController.MovementDeltaTime;
        Check(moving.IsComplete() && delta <= .020001f, family + " relative movement returns immediately");
        typeof(GameEventController).GetMethod("UpdateSceneMoves", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(events, null);
        for (int i = 0; i < units.Length; i++)
            Equal(units[i].data.pos, initial[i] + Vector3.forward * 2 * delta, family + " MoveRelative uses speed and captures independent target/start");
        Check(((System.Collections.IList)typeof(GameEventController).GetField("sceneMoves", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(events)).Count == 1,
            family + " movement stays scheduled after the command finishes");
        events.ClearSceneEvent();
        Check(((System.Collections.IList)typeof(GameEventController).GetField("sceneMoves", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(events)).Count == 0,
            family + " scene exit clears background movement");
        Clear.Invoke(events, null);

        Restore(units, initial);
        CmdBase durationAlias = units[0] is CharacterUnit ? (CmdBase)new MoveCharacterRelativeCmd() : new MoveObjectRelativeCmd();
        var legacyMove = Invoke(durationAlias, Text(template), Vector(Vector3.forward * 2), Number(2));
        Check(!legacyMove.IsComplete(), family + " legacy duration alias remains pending");
        for (int i = 0; i < units.Length; i++)
            Equal(units[i].data.pos, initial[i] + Vector3.forward * delta, family + " legacy Move retains duration, not speed");
        Clear.Invoke(events, null);

        Restore(units, initial);
        units[0].data.euler = new Vector3(1, 350, 2);
        units[1].data.euler = new Vector3(3, 270, 4);
        var rotating = Invoke(new SetRotationCmd(), Text(template), Number(10), Number(1));
        Check(!rotating.IsComplete(), family + " timed rotation waits for every captured target");
        Equal(units[0].data.euler, new Vector3(1, Mathf.Repeat(Mathf.LerpAngle(350, 10, delta), 360), 2),
            family + " timed rotation uses first own shortest-path start");
        Equal(units[1].data.euler, new Vector3(3, Mathf.Repeat(Mathf.LerpAngle(270, 10, delta), 360), 4),
            family + " timed rotation uses second own shortest-path start");
        CheckCapturedRemoval(family + " Rotation", rotating, units);

        Restore(units, initial);
        var advance = typeof(MoveCmd).GetMethod("AdvanceMove", BindingFlags.Static | BindingFlags.NonPublic);
        var moveArgs = new object[] { units[0], initial[0] + Vector3.forward, 2f, .5f, 1f };
        Check(!(bool)advance.Invoke(null, moveArgs) && Mathf.Abs((float)moveArgs[3] - .48f) < .00001f,
            family + " explicit lag consumes only .02 seconds of movement duration");
        Equal(units[0].data.pos, initial[0] + Vector3.forward * .04f, family + " explicit lag moves only one capped step");
        var rotate = typeof(SetRotationCmd).GetMethod("AdvanceRotation", BindingFlags.Static | BindingFlags.NonPublic);
        units[0].data.euler = new Vector3(1, 350, 2);
        var rotateArgs = new object[] { units[0], 350f, 10f, .04f, 0f, 1f };
        Check(!(bool)rotate.Invoke(null, rotateArgs) && Mathf.Abs((float)rotateArgs[4] - .02f) < .00001f,
            family + " explicit lag rotates for only .02 seconds and remains waiting");
        Equal(units[0].data.euler, new Vector3(1, 0, 2), family + " shortest-path intermediate rotation wraps correctly");
        Check((bool)rotate.Invoke(null, rotateArgs), family + " second bounded rotation step completes");
        Equal(units[0].data.euler, new Vector3(1, 10, 2), family + " rotation reaches the exact final yaw");
        Restore(units, initial);
    }

    private static void CheckWorldUnits(CharacterUnit character, CharacterUnit other, ObjectUnit obj)
    {
        var handle = Text(GlobalEventHelper.GetSceneUnitReference(character.data.uid));
        var world = new Vector3(8, 1, 8);
        var authored = GameManager.MapPosToPlayerPos(world);
        Invoke(new SetPositionCmd(), handle, Vector(authored));
        Equal(character.data.pos, world, "absolute Set applies only the +500 offset on nonuniform cells");
        Equal(Read(Invoke(new GetPositionCmd(), handle).res[0]), authored, "absolute Get applies only the -500 offset");
        var offset = new Vector3(1, 2, 3);
        Invoke(new SetPositionRelativeCmd(), handle, Vector(offset));
        Equal(character.data.pos, world + offset, "relative Set keeps world metres on all axes");
        Equal(Read(Invoke(new GetPositionRelativeCmd(), handle,
            Text(GlobalEventHelper.GetSceneUnitReference(obj.data.uid))).res[0]), character.data.pos - obj.data.pos,
            "relative Get keeps world metres on all axes");
        Equal(Read(Invoke(new GetNormalizedVectorCmd(), Vector(offset)).res[0]), offset.normalized,
            "normalization does not distort direction using cell dimensions");
        Check(Mathf.Abs(Invoke(new GetLookAtRotationCmd(), Vector(Vector3.zero),
            Vector(new Vector3(1, 999, 1))).res[0].num - 45) < .0001f,
            "yaw uses world X/Z without cell scaling or height");

        map.updateCtrl.ApplyMove(character, new Vector3(5, 0, 5), character.data.euler, true);
        map.updateCtrl.ApplyMove(other, new Vector3(7, 0, 5), other.data.euler, true);
        var nearby = Invoke(new GetCharactersInAreaCmd(), Vector(GameManager.MapPosToPlayerPos(character.data.pos)), Number(1.5f));
        Check(nearby.res[0].dic.Values.Count(value => value.str == "$ch$97501$ch$") == 1
            && nearby.res[0].dic.Values.All(value => value.str != "$ch$97502$ch$"),
            "area radius is a world sphere and excludes a Character two metres away");
        Invoke(new SetCharacterNavigateCmd(), Text("$ch$97501$ch$"), Vector(authored));
        Equal(character.data.destination, world, "absolute navigation applies only the origin offset");
        Invoke(new SetCharacterNavigateRelativeCmd(), Text("$ch$97501$ch$"), Vector(offset));
        Equal(character.data.destination, character.data.pos + offset, "relative navigation uses world displacement");
        var mission = MissionForm.defaultData;
        mission.targetPos = authored;
        Equal(MissionGuide.GetTargetWorldPosition(mission), world, "mission target uses offset world coordinates");
        Check(Mathf.Abs(MissionGuide.GetDistance(mission, world + new Vector3(3, 4, 0)) - 5) < .0001f,
            "mission distance remains five world metres across nonuniform cells");
    }

    private static void CheckUnchangedRotation(CharacterUnit character, ObjectUnit obj)
    {
        var meshes = obj.GetMeshes(CollideType.All);
        var tiles = map.updateCtrl.objectTileDic.Get(obj);
        var instant = Invoke(new SetRotationCmd(), Text(GlobalEventHelper.GetSceneUnitReference(obj.data.uid)),
            Number(obj.data.euler.y), Number(0));
        Check(instant.IsComplete() && ReferenceEquals(meshes, obj.GetMeshes(CollideType.All))
            && ReferenceEquals(tiles, map.updateCtrl.objectTileDic.Get(obj)),
            "same-yaw instant rotation retains geometry and spatial association buffers");
        character.forceEuler = null;
        Invoke(new SetRotationCmd(), Text(GlobalEventHelper.GetSceneUnitReference(character.data.uid)),
            Number(character.data.euler.y), Number(0));
        Check(character.forceEuler.HasValue && character.forceEuler.Value == character.data.euler,
            "same-yaw Character rotation still overrides navigation facing");
        var timed = Invoke(new SetRotationCmd(), Text(GlobalEventHelper.GetSceneUnitReference(obj.data.uid)),
            Number(obj.data.euler.y), Number(1));
        Check(!timed.IsComplete(), "same-yaw timed rotation retains its authored duration");
        Clear.Invoke(events, null);
    }

    private static void CheckCapturedRemoval(string label, InterpretAsyncTask task, MapUnit[] units)
    {
        var pending = ((System.Collections.IEnumerable)Tasks.GetValue(events)).Cast<object>().Last();
        var firstData = units[0].data;
        UnitForm.Data replacement = units[0] is CharacterUnit character ? (UnitForm.Data)character.data.Copy()
            : ((ObjectUnit)units[0]).data.Copy();
        var originalPosition = firstData.pos;
        var originalEuler = firstData.euler;
        var replacementPosition = replacement.pos;
        var replacementEuler = replacement.euler;
        UnitForm.DataByUid[firstData.uid] = replacement;
        pending.GetType().GetMethod("Update").Invoke(pending, null);
        Check(!task.IsComplete(), label + " one replaced target does not finish surviving target");
        Equal(firstData.pos, originalPosition, label + " old removed Data is not moved");
        Equal(firstData.euler, originalEuler, label + " old removed Data is not rotated");
        Equal(replacement.pos, replacementPosition, label + " recycled UID is not re-resolved/moved");
        Equal(replacement.euler, replacementEuler, label + " recycled UID is not re-resolved/rotated");
        UnitForm.DataByUid.Remove(units[1].data.uid);
        pending.GetType().GetMethod("Update").Invoke(pending, null);
        Check(task.IsComplete(), label + " completes only after all original captured targets end");
        foreach (var unit in units) UnitForm.DataByUid[unit.data.uid] = unit.data;
        Clear.Invoke(events, null);
    }

    private static void Restore(MapUnit[] units, Vector3[] positions)
    {
        Clear.Invoke(events, null);
        for (int i = 0; i < units.Length; i++)
        {
            UnitForm.DataByUid[units[i].data.uid] = units[i].data;
            map.updateCtrl.ApplyMove(units[i], positions[i], Vector3.zero, teleport: true);
        }
    }

    private static InterpretAsyncTask Invoke(CmdBase command, params BoxDataForm.Data[] arguments)
    {
        var data = new InterpretDataForm.Data(-1, new List<BoxDataForm.Data>(), new Dictionary<string, BoxDataForm.Data>(),
            null, 0, -1, 0, null, new List<BoxDataForm.Data>(), 0);
        var task = new InterpretAsyncTask(new Interpreter(data));
        task.Run();
        if ((bool)command.GetType().GetMethod("ExecuteInternal", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(command, new object[] { arguments, task })) task.Complete();
        checks++;
        return task;
    }

    private static BoxDataForm.Data Text(string value) => CodeHelper.CreateBoxByStr(value);
    private static BoxDataForm.Data Number(float value) => CodeHelper.CreateBoxByNum(value);
    private static BoxDataForm.Data Vector(Vector3 value)
    {
        var box = CodeHelper.CreateBox();
        box.dic["x"] = Number(value.x);
        box.dic["height"] = Number(value.y);
        box.dic["y"] = Number(value.z);
        return box;
    }
    private static Vector3 Read(BoxDataForm.Data value) => new Vector3(value.dic["x"].num, value.dic["height"].num, value.dic["y"].num);
    private static void Equal(Vector3 actual, Vector3 expected, string message) =>
        Check(Vector3.Distance(actual, expected) < .0001f, message + ": actual=" + actual + " expected=" + expected);
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
        checks++;
    }
    private static T CreateManager<T>() where T : Z_MonoSingleton<T>
    {
        var root = new GameObject(typeof(T).Name);
        root.SetActive(false);
        var manager = root.AddComponent<T>();
        typeof(Z_MonoSingleton<T>).GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, manager);
        return manager;
    }
}
