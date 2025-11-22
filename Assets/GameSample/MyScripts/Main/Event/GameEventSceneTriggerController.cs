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

public enum SceneEventType
{
    All = 0,
    Global = 1,
    Tile = 2,
    Object = 3,
    Item = 4,
    Character = 5
}
namespace Z_Map
{

    public partial class MapUnit
    {
        public static string evtKey = "events";
        private Dictionary<string, EventTriggerForm.Data> _evtSet;
        public Dictionary<string, EventTriggerForm.Data> evtDic
        {
            get
            {
                if (_evtSet == null)
                {
                    _evtSet = new Dictionary<string, EventTriggerForm.Data>();
                    if (!string.IsNullOrEmpty(data.extra))
                    {
                        var jo = JObject.Parse(data.extra);
                        if (jo != null && jo[evtKey] != null)
                        {
                            _evtSet = jo.Get<Dictionary<string, EventTriggerForm.Data>>(evtKey);
                        }
                    }

                }

                return _evtSet;
            }
            set
            {
                var jo = string.IsNullOrEmpty(data.extra) ? new JObject() : JObject.Parse(data.extra);

                if (value != null)
                {
                    jo.Set(evtKey, value);
                }
                else
                {
                    jo.Set(evtKey, new Dictionary<string, EventTriggerForm.Data>());
                }
                data.extra = jo.ToString();
                _evtSet = value;
            }

        }
        public void ExecuteEvt(string name)
        {
            if (evtDic.ContainsKey(name) && EventProgramDataForm.DataByName.ContainsKey(evtDic[name].evt))
            {
                Debug.Log(evtDic[name].evt);
                GameManager.instance.evtCtrl.Execute(EventProgramDataForm.DataByName[evtDic[name].evt], productInfo.Item2);
            }
        }

    }
}
public class GameEventSceneTriggerController : Z_Controller<GameEventController>, IZ_Listener<CollideEvent>, IZ_Listener<TileEvent>, IZ_Listener<ItemEvent>, IZ_Listener<ObjectEvent>, IZ_Listener<CharacterEvent>
{
    
    public GameEventSceneTriggerController(GameEventController super) : base(super)
    {
        Z_EventHelper.Register<CollideEvent>(this);
        Z_EventHelper.Register<TileEvent>(this);
        Z_EventHelper.Register<ItemEvent>(this);
        Z_EventHelper.Register<ObjectEvent>(this);
        Z_EventHelper.Register<CharacterEvent>(this);
    }
    public void OnEvent(CollideEvent evt)
    {
        if (evt.a is MapUnit mapUnit)
        {
            switch (evt.type)
            {
                case CollideEventType.TriggerEnter:
                    mapUnit.ExecuteEvt("onTouchEvent");
                    break;
                case CollideEventType.TriggerExit:
                    mapUnit.ExecuteEvt("onLeaveEvent");
                    break;
            }
        }
    }
    public void OnEvent(TileEvent evt)
    {
        if (evt.unit is MapUnit mapUnit)
        {
            switch (evt.type)
            {
                case MapEventType.Show:
                    mapUnit.ExecuteEvt("onShowEvent");
                    break;
            }
        }
    }
    public void OnEvent(CharacterEvent evt)
    {
        if (evt.unit is MapUnit mapUnit)
        {
            switch (evt.type)
            {
                case MapEventType.Show:
                    mapUnit.ExecuteEvt("onShowEvent");
                    break;
            }
        }
    }
    public void OnEvent(ItemEvent evt)
    {
        if (evt.unit is MapUnit mapUnit)
        {

            switch (evt.type)
            {
                case MapEventType.Show:
                    mapUnit.ExecuteEvt("onShowEvent");
                    break;
            }
        }
    }
    public void OnEvent(ObjectEvent evt)
    {
        if (evt.unit is MapUnit mapUnit)
        {

            switch (evt.type)
            {
                case MapEventType.Show:
                    mapUnit.ExecuteEvt("onShowEvent");
                    break;
            }
        }
    }
}
