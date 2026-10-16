using Form;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using Ui.ModStoryEventTrigger;
using Unity.VisualScripting;
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
            // Runtime-only owner identity for scene events. The uid can be reused
            // after removal, so checking the integer alone cannot identify a stale event.
            public UnitForm.Data sceneOwner;

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
    public static string SCENEOBJECTPROTO = "$sop$";
    public static string EFFECT = "$ef$";
    public static string UIIMAGE = "$ui$";
    public static string VECTOR = "$vt$";
    public static string SKILL = "$sk$";
    public static string MISSION = "$m$";
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
    public static bool TryGetMission(string reference, out MissionForm.Data data)
    {
        data = null;
        if (string.IsNullOrEmpty(reference))
            return false;

        if (IsAsset(reference, MISSION))
        {
            string idText = reference.Substring(MISSION.Length, reference.Length - MISSION.Length * 2);
            if (!int.TryParse(idText, System.Globalization.NumberStyles.None,
                System.Globalization.CultureInfo.InvariantCulture, out int id) || id <= 0)
                return false;
            return MissionForm.DataById.TryGetValue(id, out data);
        }

        // Older stories passed mission names instead of stable IDs.
        return MissionForm.DataByName.TryGetValue(reference, out data);
    }
    public static string GetGameRetType(Desc desc)
    {
        var res = desc.retType;
        if (desc.type == CodeType.VarName)
            res = "var";
        else if (desc.code != null && IsAsset(desc.code, MISSION))
            res = "mission";
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
            case "onTileTouchEvent":
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
    private sealed class IdentityComparer<T> : IEqualityComparer<T> where T : class
    {
        public static readonly IdentityComparer<T> Instance = new IdentityComparer<T>();
        public bool Equals(T x, T y) => ReferenceEquals(x, y);
        public int GetHashCode(T obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
    }

    private sealed class EventTask
    {
        private readonly InterpretAsyncTask asyncTask;
        private readonly InterpretDataForm.Data interpretOwner;
        public readonly EventInterpretDataForm.Data eventOwner;
        public readonly UnitForm.Data sceneOwner;
        public EventTask eventPrevious, eventNext;
        public EventTask scenePrevious, sceneNext;
        private readonly Func<bool> update;
        private bool isCancelled;

        public EventTask(InterpretAsyncTask asyncTask, InterpretDataForm.Data interpretOwner,
            EventInterpretDataForm.Data eventOwner, Func<bool> update)
        {
            this.asyncTask = asyncTask;
            this.interpretOwner = interpretOwner;
            this.eventOwner = eventOwner;
            sceneOwner = eventOwner?.sceneOwner;
            this.update = update;
        }

        public void Cancel()
        {
            isCancelled = true;
            asyncTask.Cancel();
        }

        public bool Update()
        {
            if (isCancelled || interpretOwner.IsCancelled || asyncTask.IsCancelled())
                return true;

            if (eventOwner != null && (!IsRegisteredEvent(eventOwner) || !IsSceneEventOwnerAlive(eventOwner)))
            {
                Cancel();
                return true;
            }

            bool complete = update();
            if (eventOwner != null && (!IsRegisteredEvent(eventOwner) || !IsSceneEventOwnerAlive(eventOwner)))
            {
                Cancel();
                return true;
            }
            return complete || isCancelled || interpretOwner.IsCancelled || asyncTask.IsCancelled();
        }
    }

    public GameEventSceneTriggerController sceneTriggerCtrl;
    public GameEventStoryTriggerController storyTriggerCtrl;
    List<EventTask> tasks;
    readonly Dictionary<EventInterpretDataForm.Data, EventTask> tasksByEvent =
        new Dictionary<EventInterpretDataForm.Data, EventTask>(IdentityComparer<EventInterpretDataForm.Data>.Instance);
    readonly Dictionary<UnitForm.Data, EventTask> tasksBySceneOwner =
        new Dictionary<UnitForm.Data, EventTask>(IdentityComparer<UnitForm.Data>.Instance);
    readonly List<EventInterpretDataForm.Data> eventSnapshot = new List<EventInterpretDataForm.Data>();
    HashSet<(int, string)> releaseTriggerTuple;
    EventInterpretDataForm.Data currentEventData;
    int taskVersion;
    bool isManagingEvents;
    public GameEventController(GameManager super) : base(super)
    {
        sceneTriggerCtrl = new GameEventSceneTriggerController(this);
        storyTriggerCtrl = new GameEventStoryTriggerController(this);
        tasks = new List<EventTask>();
        releaseTriggerTuple = new HashSet<(int, string)>();

    }
    public void Reset()
    {
        sceneTriggerCtrl.evts -= sceneTriggerCtrl.evts;
        foreach (var data in EventInterpretDataForm.DataByUid.Values)
        {
            data.Cancel();
        }
        CancelAllTasks();
        EventInterpretDataForm.Clear();
        releaseTriggerTuple.Clear();
        currentEventData = null;
    }
    public void ClearSceneEvent()
    {
        var clearLst = new List<EventInterpretDataForm.Data>();
        foreach (var data in EventInterpretDataForm.DataByUid.Values)
        {
            if (GlobalEventHelper.IsSceneTrigger(data.releaseTrigger, data.user))
            {
                data.Cancel();
                CancelEventTasks(data);
                clearLst.Add(data);
            }
        }
        foreach (var data in clearLst)
        {
            EventInterpretDataForm.RemoveData(data.uid);
        }
    }
    public void StartTask(InterpretAsyncTask asyncTask, Func<bool> func)
    {
        var interpretData = asyncTask.interpreter.data;
        var eventOwner = interpretData as EventInterpretDataForm.Data;
        if (eventOwner == null && interpretData.rootUid > 0)
        {
            if (!EventInterpretDataForm.DataByUid.TryGetValue(interpretData.rootUid, out eventOwner))
            {
                asyncTask.Cancel();
                return;
            }
        }
        InterpretDataForm.Data taskOwner = eventOwner ?? interpretData;
        var task = new EventTask(asyncTask, taskOwner, eventOwner, func);
        int version = taskVersion;
        bool complete = task.Update();
        // An immediate task callback may unload the scene and create new work.
        // Never append an old-lifetime task into that new queue.
        if (version != taskVersion)
        {
            task.Cancel();
            return;
        }
        if (!complete)
        {
            tasks.Add(task);
            TrackTask(task);
        }
    }
    private void TrackTask(EventTask task)
    {
        if (task.eventOwner != null)
        {
            tasksByEvent.TryGetValue(task.eventOwner, out var head);
            task.eventNext = head;
            if (head != null)
                head.eventPrevious = task;
            tasksByEvent[task.eventOwner] = task;
        }
        if (task.sceneOwner != null)
        {
            tasksBySceneOwner.TryGetValue(task.sceneOwner, out var head);
            task.sceneNext = head;
            if (head != null)
                head.scenePrevious = task;
            tasksBySceneOwner[task.sceneOwner] = task;
        }
    }
    private void UntrackTask(EventTask task)
    {
        if (task.eventOwner != null)
        {
            if (task.eventPrevious != null)
                task.eventPrevious.eventNext = task.eventNext;
            else if (task.eventNext != null)
                tasksByEvent[task.eventOwner] = task.eventNext;
            else
                tasksByEvent.Remove(task.eventOwner);
            if (task.eventNext != null)
                task.eventNext.eventPrevious = task.eventPrevious;
            task.eventPrevious = task.eventNext = null;
        }
        if (task.sceneOwner != null)
        {
            if (task.scenePrevious != null)
                task.scenePrevious.sceneNext = task.sceneNext;
            else if (task.sceneNext != null)
                tasksBySceneOwner[task.sceneOwner] = task.sceneNext;
            else
                tasksBySceneOwner.Remove(task.sceneOwner);
            if (task.sceneNext != null)
                task.sceneNext.scenePrevious = task.scenePrevious;
            task.scenePrevious = task.sceneNext = null;
        }
    }
    private void CancelEventTasks(EventInterpretDataForm.Data eventData)
    {
        if (tasksByEvent.TryGetValue(eventData, out var task))
        {
            for (; task != null; task = task.eventNext)
                task.Cancel();
        }
    }
    private void CancelAllTasks()
    {
        taskVersion++;
        foreach (var task in tasks)
        {
            task.Cancel();
        }
        tasks.Clear();
        tasksByEvent.Clear();
        tasksBySceneOwner.Clear();
    }
    public void CompleteUnitEvents(UnitForm.Data unitData)
    {
        if (unitData == null)
            return;

        if (tasksBySceneOwner.TryGetValue(unitData, out var task))
        {
            for (; task != null; task = task.sceneNext)
                task.Cancel();
        }

        // Cancellation does not add/remove Form rows, so no extra snapshot is needed here.
        foreach (var data in EventInterpretDataForm.DataByUid.Values)
        {
            if (!ReferenceEquals(data.sceneOwner, unitData)
                || !GlobalEventHelper.IsSceneTrigger(data.releaseTrigger, data.user)
                || ReferenceEquals(data, currentEventData))
                continue;

            data.Cancel();
            CancelEventTasks(data);
            ReleaseSceneTrigger(data.user, data.releaseTrigger);

            if (GameManager.instance.curProgress.blockProgramUid == data.uid)
                GameManager.instance.curProgress.blockProgramUid = 0;
        }
    }
    private static bool IsRegisteredEvent(EventInterpretDataForm.Data data)
    {
        return EventInterpretDataForm.DataByUid.TryGetValue(data.uid, out var registered)
            && ReferenceEquals(registered, data);
    }
    private static bool IsSceneEventOwnerAlive(EventInterpretDataForm.Data data)
    {
        if (!GlobalEventHelper.IsSceneTrigger(data.releaseTrigger, data.user))
            return true;

        return data.sceneOwner != null
            && UnitForm.DataByUid.TryGetValue(data.user, out var registered)
            && ReferenceEquals(registered, data.sceneOwner);
    }
    private static void ReleaseSceneTrigger(int unitUid, string triggerName)
    {
        if (string.IsNullOrEmpty(triggerName)
            || GameManager.instance.curScene == null
            || !GameManager.instance.curScene.triggeringOnceDuringEvts.TryGetValue(unitUid, out var triggers))
            return;

        triggers.Remove(triggerName);
        if (triggers.Count == 0)
            GameManager.instance.curScene.triggeringOnceDuringEvts.Remove(unitUid);
    }
    public void ClearScene()
    {
        foreach (var data in EventInterpretDataForm.DataByUid.Values)
        {
            data.Cancel();
        }
        CancelAllTasks();
        EventInterpretDataForm.DataByUid.Clear();
        GameManager.instance.curScene.triggeringOnceDuringEvts.Clear();
        releaseTriggerTuple.Clear();
        currentEventData = null;
    }

    public void LateUpdate()
    {
        // Keep additions deferred until the next scheduler pass, without allocating
        // a fresh list each frame. Reentrant calls must not mutate the live snapshot.
        if (isManagingEvents)
            return;
        isManagingEvents = true;
        try
        {
            eventSnapshot.AddRange(EventInterpretDataForm.DataByUid.Values);
            ManageEventDatas(eventSnapshot);
        }
        finally
        {
            eventSnapshot.Clear();
            isManagingEvents = false;
        }
    }
    private void ManageEventDatas(List<EventInterpretDataForm.Data> lst)
    {
        releaseTriggerTuple.Clear();
        foreach (var data in lst)
        {
            if (!IsRegisteredEvent(data))
            {
                CancelEventTasks(data);
                continue;
            }
            bool complete = data.IsCancelled;
            if (!complete && GameManager.instance.curProgress.blockProgramUid != 0 && GameManager.instance.curProgress.blockProgramUid != data.uid)
            {
                continue;
            }
            if (!complete && !IsSceneEventOwnerAlive(data))
            {
                data.Cancel();
                complete = true;
            }
            else if (!complete)
            {
                RetInfo retInfo;
                currentEventData = data;
                try
                {
                    retInfo = data.Interpret();
                }
                finally
                {
                    currentEventData = null;
                }
                if (!IsRegisteredEvent(data))
                {
                    CancelEventTasks(data);
                    continue;
                }
                complete = retInfo.complete;

                // DestroyObject may remove this event's owner while Interpret is on
                // the stack. Let that current pass finish, then stop any continuation
                // and cancel tasks created by the event before polling them below.
                if (!IsSceneEventOwnerAlive(data))
                {
                    data.Cancel();
                    complete = true;
                }
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
                CancelEventTasks(data);
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
        UpdateTasks();
        foreach (var data in EventInterpretDataForm.DataByUid.Values)
        {
            if (releaseTriggerTuple.Contains((data.user, data.releaseTrigger)))
            {
                releaseTriggerTuple.Remove((data.user, data.releaseTrigger));
            }
        }
        foreach (var trigger in releaseTriggerTuple)
        {
            if (GameManager.instance.curScene.triggeringOnceDuringEvts.ContainsKey(trigger.Item1))
                GameManager.instance.curScene.triggeringOnceDuringEvts[trigger.Item1].Remove(trigger.Item2);
        }
    }

    private void UpdateTasks()
    {
        int version = taskVersion;
        int read = 0;
        int write = 0;
        try
        {
            // The count intentionally stays live: tasks appended by a callback keep
            // their existing same-pass polling order. Survivors are compacted stably.
            while (read < tasks.Count)
            {
                var task = tasks[read];
                bool complete = task.Update();
                if (version != taskVersion)
                    return;
                read++;
                if (complete)
                    UntrackTask(task);
                else
                    tasks[write++] = task;
            }
        }
        finally
        {
            if (version == taskVersion)
            {
                // Preserve unpolled work if a callback throws. A scene/reset callback
                // owns its replacement queue, so it must never be compacted here.
                while (read < tasks.Count)
                    tasks[write++] = tasks[read++];
                if (write < tasks.Count)
                    tasks.RemoveRange(write, tasks.Count - write);
            }
        }
    }


    public void TriggerEventExecute(EventTriggerForm.Data trigger, int user, Dictionary<string, Z_Code.Form.BoxDataForm.Data> defaultHeap)
    {
        if (trigger == null)
        {
            return;
        }
        List<string> triggered;
        
        switch (trigger.type)
        {
            case TriggerType.Once:
                { 
                var dict = GameManager.instance.curScene.triggeredOnceEvts;
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

                break;
                }
            case TriggerType.OnceDuring:
                {
                var dict = GameManager.instance.curScene.triggeringOnceDuringEvts;
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
                }
            default:
                foreach (var nm in trigger.evt)
                {
                    Execute(EventProgramDataForm.DataByUid.GetDv(nm, null), user, defaultHeap, trigger.name);
                }
                break;
        }
    }
    int debugId = 0;
    public void Execute(EventProgramDataForm.Data evt, int user, Dictionary<string, Z_Code.Form.BoxDataForm.Data> defaultHeap, string releaseTrigger = "", bool noreleaseTrigger = false)
    {
        if (evt == null)
            return;
        if (GameManager.instance.curProgress.eventState == EventState.Leave && releaseTrigger != "onLeaveSceneEvent" && releaseTrigger != "onLeaveEvent")
            return;
        var data = new EventInterpretDataForm.Data(-1, new List<Z_Code.Form.BoxDataForm.Data>(), defaultHeap == null ? new Dictionary<string, Z_Code.Form.BoxDataForm.Data>() : defaultHeap, evt.CreateRuntimeSnapshot(), 0, -1, user, null, new List<BoxDataForm.Data>(), 0, releaseTrigger, 0);
        data.debugId = debugId++;
        if (GlobalEventHelper.IsSceneTrigger(releaseTrigger, user))
        {
            data.sceneOwner = UnitForm.DataByUid.GetDv(user, null);
        }
        EventInterpretDataForm.AddData(data);

    }
    public EntryItem GetEventEntry(SceneEventType objectType, string retType)
    {
        var res = new EntryItem();
        foreach (var data in EventProgramDataForm.DataByUid.Values.OrderBy(data => data.labId).ThenBy(data => data.uid))
        {
            if (!IsCorrect(retType, data.returnValue))
                continue;
            LabForm.TryGetData(data.labId, out var lab);
            var cat = string.IsNullOrEmpty(lab?.lv1Lab) ? TextManager.instance.GetTxt(GlobalDefaultHelper.defaultLab) : lab.lv1Lab;
            if (!res.subs.ContainsKey(cat))
            {
                res.Add(cat);
            }
            var name = data.name;
            if (res.subs[cat].subs.ContainsKey(name))
                name = $"{name} ({data.uid})";
            res.subs[cat].Add(name, null, data.uid);
        }
        return SortFormEntryChildren(res);
    }
    public EntryItem GetTriggerConditionEntry()
    {
        var res = new EntryItem();
        foreach (var data in EventTriggerForm.DataByName.Values.OrderBy(data => data.uid))
        {
            res.Add(TextManager.instance.GetTxt(data.name.Split("$")[0]), id: data.uid);

        }
        return res;
    }
    public EntryItem GetCmdEntry(SceneEventType objectType, string retType, out EntryItem defaultItem)
    {
        defaultItem = null;
        var res = new EntryItem();
        foreach (var data in GameCmdDataForm.DataByName.Values.OrderBy(data => data.labId).ThenBy(data => data.uid))
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
            CmdLabForm.DataById.TryGetValue(data.labId, out var lab);
            string category = TextManager.instance.GetTxt(lab?.lv1Lab ?? string.Empty);
            if (!res.subs.ContainsKey(category))
            {
                res.Add(category);
            }
            // Mission is a constant command; reuse the existing localized type
            // label while retaining its canonical command name/UID in metadata.
            string name = TextManager.instance.GetTxt(data.name == "Mission" ? "mission" : data.name);
            if (res.subs[category].subs.ContainsKey(name))
                name = $"{name} ({data.uid})";
            res.subs[category].Add(name, null, data.uid);
            if (defaultItem == null)
                defaultItem = res.subs[category].subs[name];
        }
        return SortFormEntryChildren(res);
    }

    private static EntryItem SortFormEntryChildren(EntryItem entries)
    {
        // Legacy aliases can merge several Lab IDs into one displayed category.
        // Keep category order by Lab ID, but sort its combined rows by Data UID.
        foreach (var category in entries.subs.Values)
            category.subs = category.subs.OrderBy(pair => pair.Value.id)
                .ToDictionary(pair => pair.Key, pair => pair.Value);
        return entries;
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
