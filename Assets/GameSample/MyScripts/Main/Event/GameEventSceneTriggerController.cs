using Form;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Z_ByteSerialize;
using Z_Code;
using Z_Code.Form;
using Z_DataSystem;
using Z_DataSystem.Form;
using Z_Debug;
using Z_DesignStyle;
using Z_Map;
using Z_Map.Form;
using Z_Math;
using Z_Text;
using Z_Time;
using Z_Ui.Notify;
using Z_UnitSystem;

public enum SceneEventType
{
    All = 0,
    Global = 1,
    Tile = 2,
    Object = 3,
    Item = 4,
    Character = 5
}

public class SceneActionEvent : Z_Event
{
    public int unitUid;
    public SceneActionEventType type;
}
public enum SceneActionEventType
{
    Add,
    Remove
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
        public EventTriggerForm.Data GetEvt(string name)
        {
            EventTriggerForm.Data trigger = null;
            if (evtDic.ContainsKey(name) && evtDic[name].evt.Count > 0)
            {
                trigger = evtDic[name];
            }
            else
            {
                if (this is CharacterUnit chU && CharacterProductForm.DataByUid.TryGetValue(chU.productInfo.Item1, out var ch) && ch.events.ContainsKey(name))
                {
                    trigger = ch.events[name];
                }
                else if (this is ObjectUnit oU && MapObjectForm.DataById.TryGetValue(oU.productInfo.Item1, out var ob) && ob.events.ContainsKey(name))
                {
                    trigger = ob.events[name];
                }
                else if (this is ItemUnit iU && ItemProductForm.DataByUid.TryGetValue(iU.productInfo.Item1, out var it) && it.events.ContainsKey(name))
                {
                    trigger = it.events[name];
                }
            }
            return trigger;
        }
        public bool ExecuteEvt(string name, Dictionary<string, BoxDataForm.Data> defaultHeap)
        {
            var trigger= GetEvt(name);
            if (trigger != null)
            {
                GameManager.instance.evtCtrl.TriggerEventExecute(trigger, data.uid, defaultHeap);
            }
            return true;
        }

    }
}

public class GameEventSceneTriggerController : Z_Controller<GameEventController>, IZ_Listener<CollideEvent>, IZ_Listener<TileEvent>, IZ_Listener<ItemEvent>, IZ_Listener<ObjectEvent>, IZ_Listener<CharacterEvent>, IZ_Listener<StoryLifeEvent>
{
    public Action evts;
    public GameEventSceneTriggerController(GameEventController super) : base(super)
    {
        Z_EventHelper.Register<CollideEvent>(this);
        Z_EventHelper.Register<TileEvent>(this);
        Z_EventHelper.Register<ItemEvent>(this);
        Z_EventHelper.Register<ObjectEvent>(this);
        Z_EventHelper.Register<CharacterEvent>(this);
        Z_EventHelper.Register<StoryLifeEvent>(this);
    }

    string interActKey = "onInteractEvent";
    public void OnEvent(CollideEvent evt)
    {
        GameManager.instance.evtCtrl.sceneTriggerCtrl.evts += () =>
        {
            if (evt.a is MapUnit mapUnit)
            {
                var heap = new Dictionary<string, BoxDataForm.Data>();
                if (evt.a is CharacterUnit ch)
                {
                    heap["self"] = CodeHelper.CreateBoxByStr(GlobalEventHelper.GetName(GlobalEventHelper.CHARACTER, ch.productInfo.Item1.ToString()));
                }
                else if (evt.a is ObjectUnit o)
                {
                    heap["self"] = CodeHelper.CreateBoxByStr(GlobalEventHelper.GetName(GlobalEventHelper.SCENEOBJECT, o.data.uid.ToString()));
                }

                if (evt.b is CharacterUnit ch2)
                {
                    heap["target"] = CodeHelper.CreateBoxByStr(GlobalEventHelper.GetName(GlobalEventHelper.CHARACTER, ch2.productInfo.Item1.ToString()));
                }
                else if (evt.b is ObjectUnit o2)
                {
                    heap["target"] = CodeHelper.CreateBoxByStr(GlobalEventHelper.GetName(GlobalEventHelper.SCENEOBJECT, o2.data.uid.ToString()));
                }

                switch (evt.type)
                {
                    case CollideEventType.TriggerEnter:
                        if (evt.b is CharacterUnit chU)
                        {
                            mapUnit.ExecuteEvt("onCharacterTouchEvent", heap);
                            if (mapUnit.GetEvt(interActKey) != null && chU.data.uid == PlayManager.instance.sceneCtrl.playerM.uid)
                                Z_EventHelper.Invoke(new SceneActionEvent() { unitUid = mapUnit.data.uid, type = SceneActionEventType.Add });
                        }
                        else if (evt.b is ObjectUnit)
                        {
                            mapUnit.ExecuteEvt("onObjectTouchEvent", heap);
                        }
                        else if(evt.b is TileUnit)
                        {
                            mapUnit.ExecuteEvt("onBoundaryTouchEvent", heap);
                        }
                       
                        
                        break;
                    case CollideEventType.TriggerExit:
                        if (evt.b is CharacterUnit chU2)
                        {
                            mapUnit.ExecuteEvt("onCharacterLeaveEvent", heap);
                            if (chU2.data.uid == PlayManager.instance.sceneCtrl.playerM.uid)
                                Z_EventHelper.Invoke(new SceneActionEvent() { unitUid = mapUnit.data.uid, type = SceneActionEventType.Remove });
                        }
                        else if (evt.b is ObjectUnit)
                        {
                            mapUnit.ExecuteEvt("onObjectLeaveEvent", heap);
                        }
                        break;
                }
            }
        };
    }
    public void OnEvent(TileEvent evt)
    {
        GameManager.instance.evtCtrl.sceneTriggerCtrl.evts += () =>
        {
            if (evt.unit is MapUnit mapUnit)
            {
                var heap = new Dictionary<string, BoxDataForm.Data>();
                heap["self"] = CodeHelper.CreateBoxByStr(GlobalEventHelper.GetName(GlobalEventHelper.SCENEOBJECT, evt.unit.data.uid.ToString()));

                switch (evt.type)
                {
                    case MapEventType.Create:
                        mapUnit.ExecuteEvt("onShowEvent", heap);//create enterScene leaveScene destroy
                        break;
                }
            }
        };
    }
    public void OnEvent(CharacterEvent evt)
    {
        GameManager.instance.evtCtrl.sceneTriggerCtrl.evts += () =>
        {
            if (evt.unit is MapUnit mapUnit)
            {

                switch (evt.type)
                {
                    case MapEventType.Create:
                        {
                            var heap = new Dictionary<string, BoxDataForm.Data>();
                            heap["self"] = CodeHelper.CreateBoxByStr(GlobalEventHelper.GetName(GlobalEventHelper.CHARACTER, evt.unit.productInfo.Item1.ToString()));

                            mapUnit.ExecuteEvt("onShowEvent", heap);
                        }
                        break;
                    case MapEventType.BoundaryTouch:
                        {
                            var heap = new Dictionary<string, BoxDataForm.Data>();
                            heap["self"] = CodeHelper.CreateBoxByStr(GlobalEventHelper.GetName(GlobalEventHelper.SCENEOBJECT, evt.unit.data.uid.ToString()));
                            mapUnit.ExecuteEvt("onBoundaryTouchEvent", heap);
                        }

                        break;
                }
            }
        };
    }
    public void OnEvent(ItemEvent evt)
    {
        GameManager.instance.evtCtrl.sceneTriggerCtrl.evts += () =>
        {
            if (evt.unit is MapUnit mapUnit)
            {

                switch (evt.type)
                {
                    case MapEventType.Create:
                        var heap = new Dictionary<string, BoxDataForm.Data>();
                        heap["self"] = CodeHelper.CreateBoxByStr(GlobalEventHelper.GetName(GlobalEventHelper.SCENEOBJECT, evt.unit.data.uid.ToString()));

                        mapUnit.ExecuteEvt("onShowEvent", heap);
                        break;
                }
            }
        };
    }
    public void OnEvent(ObjectEvent evt)
    {
        GameManager.instance.evtCtrl.sceneTriggerCtrl.evts += () =>
        {
            if (evt.unit is MapUnit mapUnit)
            {


                switch (evt.type)
                {
                    case MapEventType.Create:
                        {
                            var heap = new Dictionary<string, BoxDataForm.Data>();
                            heap["self"] = CodeHelper.CreateBoxByStr(GlobalEventHelper.GetName(GlobalEventHelper.SCENEOBJECT, evt.unit.data.uid.ToString()));

                            mapUnit.ExecuteEvt("onShowEvent", heap);
                        }
                        break;
                    case MapEventType.BoundaryTouch:
                        {
                            var heap = new Dictionary<string, BoxDataForm.Data>();
                            heap["self"] = CodeHelper.CreateBoxByStr(GlobalEventHelper.GetName(GlobalEventHelper.SCENEOBJECT, evt.unit.data.uid.ToString()));
                            mapUnit.ExecuteEvt("onBoundaryTouchEvent", heap);
                        }

                        break;
                }
            }
        };
    }

    public void OnEvent(StoryLifeEvent evt)
    {
        GameManager.instance.evtCtrl.sceneTriggerCtrl.evts += () =>
        {
            if (evt.type == StoryLifeEventType.EverySecond)
            {
                foreach (var data in ItemUnitForm.DataByUid.Values)
                {
                    var heap = new Dictionary<string, BoxDataForm.Data>();
                    heap["self"] = CodeHelper.CreateBoxByStr(GlobalEventHelper.GetName(GlobalEventHelper.SCENEOBJECT, data.uid.ToString()));

                    data.unit.ExecuteEvt("onPerSecondEvent", heap);
                }
                foreach (var data in CharacterUnitForm.DataByUid.Values)
                {
                    var heap = new Dictionary<string, BoxDataForm.Data>();
                    heap["self"] = CodeHelper.CreateBoxByStr(GlobalEventHelper.GetName(GlobalEventHelper.CHARACTER, data.unit.productInfo.Item1.ToString()));

                    data.unit.ExecuteEvt("onPerSecondEvent", heap);
                }
                foreach (var data in ObjectUnitForm.DataByUid.Values)
                {
                    var heap = new Dictionary<string, BoxDataForm.Data>();
                    heap["self"] = CodeHelper.CreateBoxByStr(GlobalEventHelper.GetName(GlobalEventHelper.SCENEOBJECT, data.uid.ToString()));

                    data.unit.ExecuteEvt("onPerSecondEvent", heap);
                }
            }
        };
    }
}
