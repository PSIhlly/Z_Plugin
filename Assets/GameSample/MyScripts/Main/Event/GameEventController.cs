using Form;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_DataSystem;
using Z_DesignStyle;
using Z_Map;
using Z_UnitSystem;
using Z_Debug;
using Z_Code.Form;
using Z_Ui.Notify;
using Z_Text;
using System;
using Z_ByteSerialize;
using Z_Code;
using Unity.VisualScripting;
using Ui.ModStoryEventTrigger;
using Z_DataSystem.Form;
namespace Form
{

    public static partial class EventInterpretDataForm
    {
        public partial class Data
        {
            public override bool Interpret()
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

public static partial class GlobalEventHelper
{

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

        return res;
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

    public GameEventController(GameManager super) : base(super)
    {
        sceneTriggerCtrl = new GameEventSceneTriggerController(this);
        storyTriggerCtrl = new GameEventStoryTriggerController(this);
        tasks= new List<Func<bool>>();
    }
    public void Reset()
    {
        tasks.Clear();
    }
    public void StartTask(Func<bool> func)
    {
        tasks.Add(func);
    }

    public void LateUpdate()
    {
        //lifeEvent
        if (!GameManager.instance.curProgress.notFirstTime)
        {
            GameManager.instance.curProgress.notFirstTime = true;
            Z_EventHelper.Invoke(new StoryLifeEvent() { type = StoryLifeEventType.FirstEnter });
        }

        var lst = new List<EventInterpretDataForm.Data>(EventInterpretDataForm.DataByUid.Values);
        foreach (var data in lst)
        {
            if(GameManager.instance.curProgress.blockProgramUid>0&& GameManager.instance.curProgress.blockProgramUid!=data.uid)
            {
                continue;
            }
            if (data.Interpret())
            {
                if(!string.IsNullOrEmpty(data.releaseTrigger))
                {
                    GameManager.instance.curProgress.triggeredOnceEvts[data.user].Remove(data.releaseTrigger);
                }
                EventInterpretDataForm.RemoveData(data.uid);
                if(data.uid == GameManager.instance.curProgress.blockProgramUid)
                {
                    GameManager.instance.curProgress.blockProgramUid = 0;
                    break;
                }
            }
        }
        for(int i=0;i< tasks.Count;i++)
        {
            if(tasks[i]())
            {
                tasks.RemoveAt(i);
                i--;
            }
        }
    }



    public void TriggerEventExecute(EventTriggerForm.Data trigger,int user, List<BoxDataForm.Data> args)
    {
        List<string> triggered;
        var dict = GameManager.instance.curProgress.triggeredOnceEvts;
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
                Execute(EventProgramDataForm.DataByName.GetDv(trigger.evt, null), user, args);
                break;
            case TriggerType.OnceDuring:
                if (dict.TryGetValue(user, out triggered))
                {
                    if (triggered.Contains(trigger.name))
                        return;
                }else
                {
                    dict[user] = new List<string>();
                }
                dict[user].Add(trigger.name);
                    Execute(EventProgramDataForm.DataByName.GetDv(trigger.evt, null), user, args, trigger.name);
                break;
            default:
                Execute(EventProgramDataForm.DataByName.GetDv(trigger.evt,null), user, args);
                break;
        }
    }

    public void Execute(EventProgramDataForm.Data evt,int user, List<BoxDataForm.Data> args, string releaseTrigger="")
    {
        if (evt == null)
            return;
        EventInterpretDataForm.AddData(new EventInterpretDataForm.Data(-1,  new List<Z_Code.Form.BoxDataForm.Data>(),  new Dictionary<string, Z_Code.Form.BoxDataForm.Data>(), evt.Copy(), 0, -1, user, args, releaseTrigger,0));
    }
    public EntryItem GetEventEntry(SceneEventType objectType, string retType)
    {
        var res = new EntryItem();
        foreach (var data in EventProgramDataForm.DataByName.Values)
        {
            var cat = data.category == "" ? TextManager.instance.GetTxt("unclassified") : data.category;
            var type = data.type == "" ? TextManager.instance.GetTxt("unclassified") : data.type;
            if (!res.subs.ContainsKey(cat))
            {
                res.Add(cat);
            }
            if (!res.subs[cat].subs.ContainsKey(type))
            {
                res.subs[cat].Add(type);
            }
            res.subs[cat].subs[type].Add(data.name);
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
    public EntryItem GetCmdEntry(SceneEventType objectType, string retType, bool createOnly,out EntryItem defaultItem)
    {
        defaultItem = null;
        var res = new EntryItem();
        foreach (var data in GameCmdDataForm.DataByName.Values)
        {
            if (createOnly && !data.canCreate)
            {
                continue;
            }
            if (!string.IsNullOrEmpty(retType))
            {
                if (data.retTypes == null)
                {
                    if (retType != CmdTypeDataForm.defaultData.name)
                        continue;
                }
                else
                {
                    if (data.retTypes[0] != retType && data.retTypes[0] != "var")
                        continue;
                }
            }
            string category = TextManager.instance.GetTxt(data.category);
            if (!res.subs.ContainsKey(category))
            {
                res.Add(category);
            }
            string type = TextManager.instance.GetTxt(data.type);
            if (!res.subs[category].subs.ContainsKey(type))
            {
                res.subs[category].Add(type);
            }
            string name = TextManager.instance.GetTxt(data.name);
            res.subs[category].subs[type].Add(name,null,data.uid); 
            if (defaultItem == null)
                defaultItem = res.subs[category].subs[type].subs[name];
        }
        return res;
    }
    public static EventTriggerForm.Data CreateTrigger(string key, string evt,TriggerType type)
    {
        return new EventTriggerForm.Data(-1, key, evt, type);
    }

}
