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
}
public class StoryLifeEvent : Z_Event
{
    public StoryLifeEventType type;
}

public class GameEventStoryTriggerController : Z_Controller<GameEventController>, IZ_Listener<StoryLifeEvent>
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
                _super.Execute(EventProgramDataForm.DataByName.GetDv(GameManager.instance.curProgress.events.GetDv("onBeginEvent", EventTriggerForm.defaultData).evt,null));
                break;
        }
    }
}
