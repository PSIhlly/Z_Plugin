using Form;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
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
using Z_Math;
using Z_Text;
using Z_Time;
using Z_Ui.Notify;
using Z_UnitSystem;
using Z_UnitSystem.Form;



public enum CharacterSkillEventType
{
    Use = 0,
    Ready = 1,
}

public class CharacterSkillEvent : Z_Event
{
    public CharacterSkillEventType type;
    public CharacterProductForm.Data data;
    public int skillUid;
}
public class GameEventStoryTriggerController : Z_Controller<GameEventController>, IZ_Listener<StoryLifeEvent>, IZ_Listener<StoryItemEvent>, IZ_Listener<StoryCharacterEvent>, IZ_Listener<CharacterSkillEvent>
{

    public GameEventStoryTriggerController(GameEventController super) : base(super)
    {
        Z_EventHelper.Register<StoryLifeEvent>(this);
        Z_EventHelper.Register<StoryItemEvent>(this);
        Z_EventHelper.Register<StoryCharacterEvent>(this);
        Z_EventHelper.Register<CharacterSkillEvent>(this);
    }
    public void OnEvent(StoryLifeEvent evt)
    {
        switch (evt.type)
        {
            case StoryLifeEventType.Enter:
            case StoryLifeEventType.FirstEnter:
                _super.TriggerEventExecute(GameManager.instance.curScene.events.GetDv("onEnterEvent", null), 0, null);
                break;
            case StoryLifeEventType.EverySecond:
                _super.TriggerEventExecute(GameManager.instance.curScene.events.GetDv("onPerSecondEvent", null), 0, null);
                break;
            case StoryLifeEventType.Leave:
                _super.ClearScene();
                _super.TriggerEventExecute(GameManager.instance.curScene.events.GetDv("onLeaveEvent", null), 0, null);
                foreach (var data in UnitForm.DataByUid.Values)
                {
                    if (data.unit is MapUnit unit)
                    {
                        var unitEvt = unit.evtDic.GetDv("onLeaveSceneEvent", null);
                        if (unitEvt != null)
                        {
                            _super.TriggerEventExecute(unitEvt, data.uid, null);
                        }
                    }
                }
                break;
        }
    }

    public void OnEvent(StoryItemEvent evt)
    {
        var heap = new Dictionary<string, BoxDataForm.Data>() { { "self", CodeHelper.CreateBoxByStr(GlobalEventHelper.GetName(GlobalEventHelper.ITEM, evt.data.uid.ToString())) } };
        switch (evt.type)
        {
            case StoryItemEventType.Add:
                _super.TriggerEventExecute(GameManager.instance.curScene.events.GetDv("onGainItemEvent", null), 0, heap);
                _super.TriggerEventExecute(GameManager.instance.curScene.events.GetDv($"onGainItemEvent${evt.data.uid}", null), 0, heap);
                _super.TriggerEventExecute(GameManager.instance.curScene.events.GetDv($"onGainItemEvent$", null), 0, heap);
                break;
            case StoryItemEventType.Remove:
                _super.TriggerEventExecute(GameManager.instance.curScene.events.GetDv("onLostItemEvent", null), 0, heap);
                _super.TriggerEventExecute(GameManager.instance.curScene.events.GetDv($"onLostItemEvent${evt.data.uid}", null), 0, heap);
                _super.TriggerEventExecute(GameManager.instance.curScene.events.GetDv($"onLostItemEvent$", null), 0, heap);
                break;
            case StoryItemEventType.Use:
                heap["target"] = heap["self"];
                heap["self"] = CodeHelper.CreateBoxByStr(GlobalEventHelper.GetName(GlobalEventHelper.CHARACTER, GameManager.instance.curProgress.characterUid.ToString()));
                _super.TriggerEventExecute(evt.data.events.GetDv("onUseEvent", null), evt.data.uid, heap);
                break;
        }
    }

    public void OnEvent(StoryCharacterEvent evt)
    {
        var heap = new Dictionary<string, BoxDataForm.Data>() { { "self", CodeHelper.CreateBoxByStr(GlobalEventHelper.GetName(GlobalEventHelper.CHARACTER, evt.data.uid.ToString())) }, };

        switch (evt.type)
        {
            case StoryCharacterEventType.ParamChange:
                heap["target"] = CodeHelper.CreateBoxByStr(evt.name);
                heap["delta"] = CodeHelper.CreateBoxByNum((float)evt.obj);
                _super.TriggerEventExecute(GameManager.instance.curScene.events.GetDv("onCharacterParamChangeEvent", null), 0, heap);
                _super.TriggerEventExecute(GameManager.instance.curScene.events.GetDv($"onCharacterParamChangeEvent$${evt.name}", null), 0, heap);
                _super.TriggerEventExecute(GameManager.instance.curScene.events.GetDv($"onCharacterParamChangeEvent${evt.data.protoUid}$", null), 0, heap);
                _super.TriggerEventExecute(GameManager.instance.curScene.events.GetDv($"onCharacterParamChangeEvent${evt.data.protoUid}${evt.name}", null), 0, heap);
                break;
            case StoryCharacterEventType.SkillParamChange:
                heap["target"] = CodeHelper.CreateBoxByStr(GlobalEventHelper.GetName(GlobalEventHelper.SKILL, evt.name));
                break;
        }
    }
    public void OnEvent(CharacterSkillEvent evt)
    {
        switch (evt.type)
        {
            case CharacterSkillEventType.Use:
                var heap = new Dictionary<string, BoxDataForm.Data>() { 
                  { "target", CodeHelper.CreateBoxByStr(GlobalEventHelper.GetName(GlobalEventHelper.SKILL, evt.skillUid.ToString())) },
                  {"self", CodeHelper.CreateBoxByStr(GlobalEventHelper.GetName(GlobalEventHelper.CHARACTER, evt.data.uid.ToString())) }
                };
                _super.TriggerEventExecute(SkillProductForm.DataByUid[evt.skillUid].events.GetDv("invoke", null), 0, heap);
                break;
        }
    }
}
