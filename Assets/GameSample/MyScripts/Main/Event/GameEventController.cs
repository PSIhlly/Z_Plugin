using Form;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using Ui.ModStoryEventTrigger;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using Z_ByteSerialize;
using Z_Code;
using Z_Code.Form;
using Z_DataSystem;
using Z_DataSystem.Form;
using Z_Debug;
using Z_DesignStyle;
using Z_Map;
using Z_Text;
using Z_Ui.Notify;
using Z_UnitSystem;
using Z_UnitSystem.Form;
using static UnityEditor.PlayerSettings;
using static Z_Code.Form.InterpretDataForm;
public class EventModifyEvent : Z_Event
{

}
namespace Form
{

    public static partial class SkillProductForm
    {
        public static Data GetByEvtName(string evtName, int characterUid = 0)
        {
            var id = GlobalEventHelper.GetId(evtName, GlobalEventHelper.SKILL);
            var ch = CharacterProductForm.DataByUid.GetDv(characterUid, null);
            if (id < 0)
            {

                if (ch != null)
                {
                    id = ch.skill.GetDv((SkillType)(-id), 0);
                    return DataByUid.GetDv(id, null);
                }
            }
            else
            {
                if (ch != null)
                {
                    foreach (var sk in ch.skill.Values)
                    {
                        var cur = DataByUid.GetDv(sk, null);
                        if (cur != null)
                        {
                            if (cur.protoUid == id)
                            {
                                return cur;
                            }
                        }
                    }
                    return DataByUid.GetDv(id, null);
                }
                DataByUid.GetDv(id, null);
            }
            return null;
        }
    }
    public static partial class EventInterpretDataForm
    {
        public partial class Data
        {
            public override RetInfo Interpret()
            {
                if (_interpreter == null)
                {
                    _interpreter = new Interpreter(this);
                }
                return _interpreter.Interpret();
            }
        }
    }
}
public enum TriggerType
{
    NoLimit,
    Once,
    OnceDuring
}

public enum EventState
{
    Normal,
    Leave,
}
public static partial class GlobalEventHelper
{
    public static string CHARACTER = "$ch$";
    public static string ITEM = "$it$";
    public static string SCENEOBJECT = "$so$";
    public static string EFFECT = "$ef$";
    public static string UIIMAGE = "$ui$";
    public static string VECTOR = "$vt$";
    public static string SKILL = "$sk$";
    public static Dictionary<(string, string), int> idCache = new Dictionary<(string, string), int>();
    public static Dictionary<(string, string), string> nameCache = new Dictionary<(string, string), string>();

    public static string GetName(string mark, string name = "")
    {
        if (!nameCache.ContainsKey((name, mark)))
        {
            nameCache[(name, mark)] = $"{mark}{name}{mark}";
        }
        return nameCache[(name, mark)];
    }
    public static int GetId(string name, string mark)
    {
        if (!idCache.ContainsKey((name, mark)))
        {
            if (IsAsset(name, mark))
            {
                idCache[(name, mark)] = int.Parse(name.Split(mark)[1]);
            }
            else
            {
                return 0;
            }
        }
        return idCache[(name, mark)];
    }
    public static bool IsAsset(string name, string mark)
    {
        var parts = name.Split(mark);
        if (parts.Length == 3 && string.IsNullOrEmpty(parts[0]) && string.IsNullOrEmpty(parts[2]))
        {
            return true;
        }
        return false;
    }
    public static string GetGameRetType(Desc desc)
    {
        var res = desc.retType;
        if (desc.type == CodeType.VarName)
            res = "var";
        else if (AssetManager.instance.texCtrl.IsAsset(desc.code))
            res = "img";
        else if (AssetManager.instance.videoCtrl.IsAsset(desc.code))
            res = "video";
        else if (AssetManager.instance.audioCtrl.IsAsset(desc.code))
            res = "audio";
        else if (IsAsset(desc.code, CHARACTER))
            res = "character";
        else if (IsAsset(desc.code, ITEM))
            res = "item";
        else if (IsAsset(desc.code, SCENEOBJECT))
            res = "sceneObject";
        else if (IsAsset(desc.code, EFFECT))
            res = "effect";
        else if (IsAsset(desc.code, UIIMAGE))
            res = "uiImg";
        else if (IsAsset(desc.code, VECTOR))
            res = "vector";
        else if (IsAsset(desc.code, SKILL))
            res = "skill";
        return res;
    }
    public static bool IsSceneTrigger(string triggerName, int user)
    {
        if (user <= 0)
        {
            return false;
        }
        switch (triggerName)
        {
            case "onCharacterTouchEvent":
            case "onCharacterLeaveEvent":
            case "onObjectTouchEvent":
            case "onObjectLeaveEvent":
            case "onShowEvent":
            case "onBoundaryTouchEvent":
            case "onPerSecondEvent":
            case "onInteractEvent":
                return true;
        }
        return false;
    }
}
public static partial class GlobalSettings
{
    public static int CUSTOM_EVENT_MAX => 1000000;

}
public class GameEventController : Z_Controller<GameManager>
{

    public GameEventSceneTriggerController sceneTriggerCtrl;
    public GameEventStoryTriggerController storyTriggerCtrl;
    List<Func<bool>> tasks;
    HashSet<(int, string)> releaseTriggerTuple;
    public GameEventController(GameManager super) : base(super)
    {
        sceneTriggerCtrl = new GameEventSceneTriggerController(this);
        storyTriggerCtrl = new GameEventStoryTriggerController(this);
        tasks = new List<Func<bool>>();
        releaseTriggerTuple = new HashSet<(int, string)>();

    }
    public void Reset()
    {
        sceneTriggerCtrl.evts -= sceneTriggerCtrl.evts;
        EventInterpretDataForm.Clear();
        tasks.Clear();
        releaseTriggerTuple.Clear();
    }
    public void ClearSceneEvent()
    {
        var clearLst = new List<int>();
        foreach (var data in EventInterpretDataForm.DataByUid.Values)
        {
            if (GlobalEventHelper.IsSceneTrigger(data.releaseTrigger, data.user))
            {
                clearLst.Add(data.uid);
            }
        }
        foreach (var id in clearLst)
        {
            EventInterpretDataForm.RemoveData(id);
        }
    }
    public void StartTask(Func<bool> func)
    {
        if (!func())
        {
            tasks.Add(func);
        }
    }
    public void ClearScene()
    {
        EventInterpretDataForm.DataByUid.Clear();
        GameManager.instance.curScene.triggeredOnceEvts.Clear();
        tasks.Clear();
    }

    public void LateUpdate()
    {

        var lst = new List<EventInterpretDataForm.Data>(EventInterpretDataForm.DataByUid.Values);
        ManageEventDatas(lst);

    }
    private void ManageEventDatas(List<EventInterpretDataForm.Data> lst)
    {
        releaseTriggerTuple.Clear();
        foreach (var data in lst)
        {
            if (GameManager.instance.curProgress.blockProgramUid != 0 && GameManager.instance.curProgress.blockProgramUid != data.uid)
            {
                continue;
            }
            bool complete = false;
            if (GlobalEventHelper.IsSceneTrigger(data.releaseTrigger, data.user) && !UnitForm.DataByUid.ContainsKey(data.user))
            {
                complete = true;
            }
            else
            {
                var retInfo = data.Interpret();
                complete = retInfo.complete;
                if (retInfo.errors.Count > 0)
                {
                    var errSb = new System.Text.StringBuilder();
                    errSb.AppendLine($"程序 {data.program.name} 运行时错误:");
                    for (int i = 0; i < retInfo.errors.Count && i < 10; i++)
                    {
                        errSb.AppendLine($"  - {retInfo.errors[i]}");
                    }
                    if (retInfo.errors.Count > 10)
                        errSb.AppendLine($"  ... 还有 {retInfo.errors.Count - 10} 个");
                    NotifyManager.instance.AddPopup("运行时错误", errSb.ToString(), true);
                }
            }
            if (complete)
            {
                if (!string.IsNullOrEmpty(data.releaseTrigger))
                {
                    releaseTriggerTuple.Add((data.user, data.releaseTrigger));
                }
                EventInterpretDataForm.RemoveData(data.uid);
                if (data.uid == GameManager.instance.curProgress.blockProgramUid)
                {
                    GameManager.instance.curProgress.blockProgramUid = 0;
                    break;
                }
            }
        }
        for (int i = 0; i < tasks.Count; i++)
        {
            if (tasks[i]())
            {
                tasks.RemoveAt(i);
                i--;
            }
        }
        foreach (var data in EventInterpretDataForm.DataByUid.Values)
        {
            if (releaseTriggerTuple.Contains((data.user, data.releaseTrigger)))
            {
                releaseTriggerTuple.Remove((data.user, data.releaseTrigger));
            }
        }
        foreach (var trigger in releaseTriggerTuple)
        {
            if (GameManager.instance.curScene.triggeredOnceEvts.ContainsKey(trigger.Item1))
                GameManager.instance.curScene.triggeredOnceEvts[trigger.Item1].Remove(trigger.Item2);
        }
    }


    public void TriggerEventExecute(EventTriggerForm.Data trigger, int user, Dictionary<string, Z_Code.Form.BoxDataForm.Data> defaultHeap)
    {
        if (trigger == null)
        {
            return;
        }
        List<string> triggered;
        var dict = GameManager.instance.curScene.triggeredOnceEvts;
        switch (trigger.type)
        {
            case TriggerType.Once:
                if (dict.TryGetValue(user, out triggered))
                {
                    if (triggered.Contains(trigger.name))
                        return;
                }
                else
                {
                    dict[user] = new List<string>();
                }
                dict[user].Add(trigger.name);
                foreach (var id in trigger.evt)
                {
                    Execute(EventProgramDataForm.DataByUid.GetDv(id, null), user, defaultHeap, trigger.name);
                }
                trigger.evt.Clear();//once can clear
                break;
            case TriggerType.OnceDuring:
                if (dict.TryGetValue(user, out triggered))
                {
                    if (triggered.Contains(trigger.name))
                        return;
                }
                else
                {
                    dict[user] = new List<string>();
                }
                dict[user].Add(trigger.name);
                foreach (var nm in trigger.evt)
                {
                    Execute(EventProgramDataForm.DataByUid.GetDv(nm, null), user, defaultHeap, trigger.name);
                }
                break;
            default:
                foreach (var nm in trigger.evt)
                {
                    Execute(EventProgramDataForm.DataByUid.GetDv(nm, null), user, defaultHeap, trigger.name);
                }
                break;
        }
    }
    int debugId = 0;
    public void Execute(EventProgramDataForm.Data evt, int user, Dictionary<string, Z_Code.Form.BoxDataForm.Data> defaultHeap, string releaseTrigger = "")
    {
        if (evt == null)
            return;
        if (GameManager.instance.curProgress.eventState == EventState.Leave && releaseTrigger != "onLeaveSceneEvent"&& releaseTrigger != "onLeaveEvent")
            return;
        var data = new EventInterpretDataForm.Data(-1, new List<Z_Code.Form.BoxDataForm.Data>(), defaultHeap == null ? new Dictionary<string, Z_Code.Form.BoxDataForm.Data>() : defaultHeap, evt.Copy(), 0, -1, user, null, new List<BoxDataForm.Data>(), 0, releaseTrigger, 0);
        data.debugId = debugId++;
        EventInterpretDataForm.AddData(data);

    }
    public EntryItem GetEventEntry(SceneEventType objectType, string retType)
    {
        var res = new EntryItem();
        foreach (var data in EventProgramDataForm.DataByUid.Values)
        {
            if (!IsCorrect(retType, data.returnValue))
                continue;
            LabForm.TryGetData(data.labId, out var lab);
            var cat = string.IsNullOrEmpty(lab?.lv1Lab) ? TextManager.instance.GetTxt(GlobalDefaultHelper.defaultLab) : lab.lv1Lab;
            var type = string.IsNullOrEmpty(lab?.lv2Lab) ? TextManager.instance.GetTxt(GlobalDefaultHelper.defaultLab) : lab.lv2Lab;
            if (!res.subs.ContainsKey(cat))
            {
                res.Add(cat);
            }
            if (!res.subs[cat].subs.ContainsKey(type))
            {
                res.subs[cat].Add(type);
            }
            res.subs[cat].subs[type].Add(data.name,null,data.uid);
        }
        return res;
    }
    public EntryItem GetTriggerConditionEntry()
    {
        var res = new EntryItem();
        foreach (var data in EventTriggerForm.DataByName.Values)
        {
            res.Add(TextManager.instance.GetTxt(data.name), id: data.uid);

        }
        return res;
    }
    public EntryItem GetCmdEntry(SceneEventType objectType, string retType, out EntryItem defaultItem)
    {
        defaultItem = null;
        var res = new EntryItem();
        foreach (var data in GameCmdDataForm.DataByName.Values)
        {
            if (data.lowestEditorStyle > GameManager.instance.curProgress.editorStyle)
            {
                continue;
            }

            if (!IsCorrect(retType, data.retTypes == null ? "void" : data.retTypes[0]))
            {
                if (retType == "void" && !string.IsNullOrEmpty(data.allowAsVoid))
                {

                }
                else
                {
                    continue;
                }
            }
            LabForm.TryGetData(data.labId, out var lab);
            string category = TextManager.instance.GetTxt(lab?.lv1Lab ?? string.Empty);
            if (!res.subs.ContainsKey(category))
            {
                res.Add(category);
            }
            string type = TextManager.instance.GetTxt(lab?.lv2Lab ?? string.Empty);
            if (!res.subs[category].subs.ContainsKey(type))
            {
                res.subs[category].Add(type);
            }
            string name = TextManager.instance.GetTxt(data.name);
            res.subs[category].subs[type].Add(name, null, data.uid);
            if (defaultItem == null)
                defaultItem = res.subs[category].subs[type].subs[name];
        }
        return res;
    }

    public static EventTriggerForm.Data CreateTrigger(string key)
    {
        return new EventTriggerForm.Data(-1, key, new List<int>(), default);
    }
    public bool IsCorrect(string allowRetType, string retType)
    {
        if (string.IsNullOrEmpty(allowRetType))
            allowRetType = "void";
        if (string.IsNullOrEmpty(allowRetType))
            retType = "void";

        if (allowRetType == "void")//确定有类型
        {
            return retType == "void";
        }
        else if (allowRetType == "var")
        {
            return retType != "void";
        }
        else
        {
            if (retType == "var")
            {
                return true;
            }
            return allowRetType == retType;
        }
    }

}
