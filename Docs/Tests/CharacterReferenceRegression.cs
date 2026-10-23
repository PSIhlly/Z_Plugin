using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Form;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using Z_Code;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Map;
using Z_Map.Form;
using Z_UnitSystem.Form;

// Run only in an isolated regression project, never against game/player saves.
public static class CharacterReferenceRegression
{
    private static int checks;

    public static void Run()
    {
        bool previousPlaying = DynamicGlobalSettings.playing;
        try
        {
            var game = CreateManager<GameManager>();
            var play = CreateManager<PlayManager>();
            var scene = new PlaySceneController(play);
            play.sceneCtrl = scene;
            foreach (var form in new[] { typeof(CharacterProductForm), typeof(SkillProductForm) })
                form.GetMethod("Register", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
            CharacterProductForm.InitInternal();
            CharacterUnitForm.InitInternal();
            UnitForm.InitInternal();
            SkillProductForm.InitInternal();
            ProgressForm.InitInternal();
            var progress = ProgressForm.defaultData;
            progress.uid = 1;
            progress.seconds = 20f;
            progress.bag = new List<int>();
            ProgressForm.AddData(progress);
            DynamicGlobalSettings.playing = true;

            var template = Product(97101, 0, false);
            var copyA = Product(97102, template.uid, false);
            var copyB = Product(97103, template.uid, false);
            var unique = Product(97104, 0, true);
            var unrelated = Product(97105, 0, false);
            var unitB = Unit(97201, copyB.uid);
            var unitA = Unit(97202, copyA.uid);
            var uniqueUnit = Unit(97203, unique.uid);

            Check(CharacterProductForm.ResolveReferences(template.uid).SequenceEqual(new[] { copyA, copyB }),
                "template Products resolve every runtime copy without returning the mutable template");
            Check(CharacterProductForm.ResolveReferences(copyA.uid).SequenceEqual(new[] { copyA }),
                "concrete runtime Product resolves exactly one target");
            Check(scene.GetCharacterUnits(template.uid).SequenceEqual(new[] { unitB, unitA }),
                "template scene lookup returns all live copies ordered by scene UID");
            Check(scene.GetCharacterUnit(template.uid) == unitB
                && CharacterProductForm.ResolveReference(template.uid) == copyB,
                "scalar representative consistently prefers the first current-scene unit");
            Check(scene.GetCharacterUnits(copyA.uid).SequenceEqual(new[] { unitA }),
                "runtime Product scene lookup does not widen to sibling copies");
            Check(CharacterProductForm.ResolveReferences(unique.uid).SequenceEqual(new[] { unique })
                && scene.GetCharacterUnits(unique.uid).SequenceEqual(new[] { uniqueUnit }),
                "authored unique Product retains its singleton identity");
            Check(CharacterProductForm.ResolveReferences(unrelated.uid).Count == 0
                && CharacterProductForm.ResolveReference(unrelated.uid) == unrelated,
                "missing Play copy cannot mutate a template but read-only fallback remains available");

            foreach (string malformed in new[] { null, "", "$ch$$ch$", "$ch$x$ch$", "$ch$0$ch$",
                "$ch$-1$ch$", "$ch$1$ch$extra", "$ch$999999999999999999$ch$", "$so$97102$so$" })
            {
                Check(!CharacterProductForm.TryGetReferenceUid(malformed, out _)
                    && CharacterProductForm.ResolveReferences(malformed).Count == 0
                    && CharacterProductForm.ResolveReference(malformed) == null,
                    "malformed Character reference is rejected safely: " + malformed);
                Execute(new CharacterStunCmd(), malformed, 9f);
                Execute(new SetCharacterNavigateCmd(), malformed);
                Execute(new SetCharacterNavigateRelativeCmd(), malformed);
                Execute(new StopCharacterNavigateCmd(), malformed);
            }
            Execute(new CharacterStunCmd(), Handle(template.uid), 4f);
            Check(copyA.recoveryTime == 24f && copyB.recoveryTime == 24f && template.recoveryTime == 0f,
                "template mutation fans out and leaves authored data untouched");
            Execute(new CharacterStunCmd(), Handle(copyA.uid), 8f);
            Check(copyA.recoveryTime == 28f && copyB.recoveryTime == 24f,
                "concrete runtime mutation updates only its own Product");
            Execute(new CharacterStunCmd(), Handle(unique.uid), 3f);
            Check(unique.recoveryTime == 23f, "unique authored singleton remains mutable in Play");

            var captured = scene.GetCharacterUnits(template.uid);
            CharacterUnitForm.RemoveData(unitB.uid);
            var replacement = Unit(unitB.uid, unrelated.uid);
            Check(captured.Count == 2 && ReferenceEquals(captured[0], unitB),
                "plural lookup is a stable snapshot during removal");
            Check(scene.GetCharacterUnits(template.uid).SequenceEqual(new[] { unitA })
                && scene.GetCharacterUnit(copyB.uid) == null,
                "reused scene UID with another Product cannot revive the old reference");
            UnitForm.DataByUid[unitA.uid] = replacement;
            Check(scene.GetCharacterUnits(template.uid).Count == 0,
                "scene lookup rejects a Form row whose root registration has a different Data identity");
            UnitForm.DataByUid[unitA.uid] = unitA;

            // Team initialization must publish the copied identities and preserve
            // unique team members; skills are copied from the pristine template.
            var skill = SkillProductForm.defaultData;
            skill.uid = 97301;
            skill.name = "reference-template-skill";
            skill.protoUid = 0;
            SkillProductForm.AddData(skill);
            template.skill = new Dictionary<SkillType, int> { [SkillType.LightAttack] = skill.uid };
            progress.team = new List<int> { template.uid, unique.uid };
            progress.teamActive = new List<int> { template.uid, unique.uid };
            progress.characterUid = template.uid;
            PlayManager.FirstPlayInit();
            var teamCopy = CharacterProductForm.DataByUid[progress.team[0]];
            Check(teamCopy.uid != template.uid && teamCopy.protoUid == template.uid
                && progress.team[1] == unique.uid && progress.teamActive.SequenceEqual(progress.team)
                && progress.characterUid == teamCopy.uid,
                "FirstPlayInit publishes copied team/active/current identities and retains unique members");
            Check(template.skill[SkillType.LightAttack] == skill.uid
                && teamCopy.skill[SkillType.LightAttack] != skill.uid
                && SkillProductForm.DataByUid[teamCopy.skill[SkillType.LightAttack]].characterUid == teamCopy.uid,
                "team runtime skill belongs to its copied Character, not the authored template");

            Debug.Log("CHARACTER_REFERENCE_REGRESSION_PASS checks=" + checks);
            EditorApplication.Exit(0);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            EditorApplication.Exit(1);
        }
        finally
        {
            DynamicGlobalSettings.playing = previousPlaying;
        }
    }

    private static CharacterProductForm.Data Product(int uid, int protoUid, bool unique)
    {
        var data = CharacterProductForm.defaultData;
        data.uid = uid;
        data.name = "character-reference-" + uid;
        data.protoUid = protoUid;
        data.unique = unique;
        data.recoveryTime = 0;
        data.paramDic = new Dictionary<string, CharacterParamForm.Data>();
        data.skill = new Dictionary<SkillType, int>();
        CharacterProductForm.AddData(data);
        return data;
    }

    private static CharacterUnitForm.Data Unit(int uid, int productUid)
    {
        var data = new CharacterUnitForm.Data(uid, false, Vector3.zero, 1, 1, 1, false, "reference-unit",
            "unused-reference-prefab", Vector3.zero, Vector3.zero, Vector3.one, UpdateType.ShowOnly,
            new List<int>(), MapUnit.GetProductInfoString(new JObject(), (productUid, -1)),
            false, 0, new List<int>());
        CharacterUnitForm.AddData(data);
        return data;
    }

    private static string Handle(int uid) => "$ch$" + uid + "$ch$";

    private static void Execute(CmdBase command, string reference, float number = 0)
    {
        var arguments = new[] { CodeHelper.CreateBoxByStr(reference), CodeHelper.CreateBoxByNum(number) };
        command.GetType().GetMethod("ExecuteInternal", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(command, new object[] { arguments, null });
        checks++;
    }

    private static T CreateManager<T>() where T : Z_MonoSingleton<T>
    {
        var root = new GameObject(typeof(T).Name);
        root.SetActive(false);
        var manager = root.AddComponent<T>();
        typeof(Z_MonoSingleton<T>).GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic)
            .SetValue(null, manager);
        return manager;
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
        checks++;
    }
}
