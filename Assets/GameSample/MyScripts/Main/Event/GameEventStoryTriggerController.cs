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
using Z_DataSystem.Form;
using Z_Time;
using Z_Math;
using System.Xml.Linq;


public enum StoryLifeEventType
{
    FirstEnter = 0,
    EverySecond = 1,
}
public class StoryLifeEvent : Z_Event
{
    public StoryLifeEventType type;
}

public class GameEventStoryTriggerController : Z_Controller<GameEventController>, IZ_Listener<StoryLifeEvent>, IZ_Listener<StoryItemEvent>, IZ_Listener<StoryCharacterEvent>
{

    public GameEventStoryTriggerController(GameEventController super) : base(super)
    {
        Z_EventHelper.Register<StoryLifeEvent>(this);
    }
    public void OnEvent(StoryLifeEvent evt)
    {
        switch (evt.type)
        {
            case StoryLifeEventType.FirstEnter:
                _super.TriggerEventExecute(GameManager.instance.curProgress.events.GetDv("onBeginEvent", null), 0, null);
                break;
            case StoryLifeEventType.EverySecond:
                _super.TriggerEventExecute(GameManager.instance.curProgress.events.GetDv("onPerSecondEvent", null), 0, null);
                break;
        }
    }

    public void OnEvent(StoryItemEvent evt)
    {
        var args = new List<BoxDataForm.Data>() { CodeHelper.CreateBoxByNum(evt.data.uid) };
        switch (evt.type)
        {
            case StoryItemEventType.Add:
                _super.TriggerEventExecute(GameManager.instance.curProgress.events.GetDv("onGainItemEvent", null), 0, args);
                _super.TriggerEventExecute(GameManager.instance.curProgress.events.GetDv($"onGainItemEvent${evt.data.uid}", null), 0, args);
                _super.TriggerEventExecute(GameManager.instance.curProgress.events.GetDv($"onGainItemEvent$", null), 0, args);
                break;
            case StoryItemEventType.Remove:
                _super.TriggerEventExecute(GameManager.instance.curProgress.events.GetDv("onLostItemEvent", null), 0, args);
                _super.TriggerEventExecute(GameManager.instance.curProgress.events.GetDv($"onLostItemEvent${evt.data.uid}", null), 0, args);
                _super.TriggerEventExecute(GameManager.instance.curProgress.events.GetDv($"onLostItemEvent$", null), 0, args);
                break;
        }
    }

    public void OnEvent(StoryCharacterEvent evt)
    {
        var args = new List<BoxDataForm.Data>() { CodeHelper.CreateBoxByNum(evt.data.uid), CodeHelper.CreateBoxByStr(evt.name) };
        switch (evt.type)
        {
            case StoryCharacterEventType.ParamChange:
                _super.TriggerEventExecute(GameManager.instance.curProgress.events.GetDv("onCharacterParamChangeEvent", null), 0, args);
                _super.TriggerEventExecute(GameManager.instance.curProgress.events.GetDv($"onCharacterParamChangeEvent$${evt.name}", null), 0, args);
                _super.TriggerEventExecute(GameManager.instance.curProgress.events.GetDv($"onCharacterParamChangeEvent${evt.data.uid}$", null), 0, args);
                _super.TriggerEventExecute(GameManager.instance.curProgress.events.GetDv($"onCharacterParamChangeEvent${evt.data.uid}${evt.name}", null), 0, args);
                break;

        }
    }
}
