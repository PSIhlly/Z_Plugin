using Form;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
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
            var trigger = GetEvt(name);
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
        if (!(evt.a is MapUnit)
            || (evt.type != CollideEventType.TriggerEnter && evt.type != CollideEventType.TriggerExit)
            || (!(evt.b is CharacterUnit) && !(evt.b is ObjectUnit)
                && !(evt.b is TileUnit && evt.type == CollideEventType.TriggerEnter)))
            return;

        QueueCollideEvent(evt);
    }

    private void QueueCollideEvent(CollideEvent evt)
    {
        evts += () =>
        {
            // A queued trigger can outlive either Object involved in it.
            if (IsRemovedObject(evt.a) || IsRemovedObject(evt.b))
                return;

            if (evt.a is MapUnit mapUnit)
            {
                var heap = new Dictionary<string, BoxDataForm.Data>();
                if (evt.a is CharacterUnit ch)
                {
                    heap["self"] = CodeHelper.CreateBoxByStr(GlobalEventHelper.GetName(GlobalEventHelper.CHARACTER, ch.productInfo.Item1.ToString()));
                }
                else if (evt.a is MapUnit sceneObject)
                {
                    heap["self"] = CodeHelper.CreateBoxByStr(GlobalEventHelper.GetName(GlobalEventHelper.SCENEOBJECT, sceneObject.data.uid.ToString()));
                }

                if (evt.b is CharacterUnit ch2)
                {
                    heap["target"] = CodeHelper.CreateBoxByStr(GlobalEventHelper.GetName(GlobalEventHelper.CHARACTER, ch2.productInfo.Item1.ToString()));
                }
                else if (evt.b is MapUnit targetSceneObject)
                {
                    heap["target"] = CodeHelper.CreateBoxByStr(GlobalEventHelper.GetName(GlobalEventHelper.SCENEOBJECT, targetSceneObject.data.uid.ToString()));
                }

                switch (evt.type)
                {
                    case CollideEventType.TriggerEnter:
                        if (evt.b is CharacterUnit chU)
                        {
                            mapUnit.ExecuteEvt("onCharacterTouchEvent", heap);
                            if (!IsRemovedObject(mapUnit) && mapUnit.GetEvt(interActKey) != null && chU.data.uid == PlayManager.instance.sceneCtrl.playerM.uid)
                                Z_EventHelper.Invoke(new SceneActionEvent() { unitUid = mapUnit.data.uid, type = SceneActionEventType.Add });
                        }
                        else if (evt.b is ObjectUnit)
                        {
                            mapUnit.ExecuteEvt("onObjectTouchEvent", heap);
                        }
                        else if (evt.b is TileUnit)
                        {
                            mapUnit.ExecuteEvt("onTileTouchEvent", heap);
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
    private static bool IsRemovedObject(Z_UnitSystem.Unit unit)
    {
        return unit is ObjectUnit obj &&
               (!ObjectUnitForm.DataByUid.TryGetValue(obj.data.uid, out var registered) ||
                !ReferenceEquals(registered, obj.data));
    }

    // Resolve supported lifecycle types before allocating the deferred callback.
    // Character Create uses a product reference; BoundaryTouch keeps a scene-unit
    // reference, matching the existing event-language self contract.
    private void QueueUnitEvent(MapUnit unit, MapEventType type,
        bool allowBoundaryTouch = false, bool characterOnCreate = false)
    {
        if (unit == null)
            return;

        string eventName;
        bool characterSelf;
        switch (type)
        {
            case MapEventType.Create:
                eventName = "onShowEvent";
                characterSelf = characterOnCreate;
                break;
            case MapEventType.BoundaryTouch when allowBoundaryTouch:
                eventName = "onBoundaryTouchEvent";
                characterSelf = false;
                break;
            default:
                return;
        }

        EnqueueUnitEvent(unit, eventName, characterSelf);
    }

    private void EnqueueUnitEvent(MapUnit unit, string eventName, bool characterSelf)
    {
        // Keep closure allocation out of the filtering method's early-return path.
        evts += () => ExecuteUnitEvent(unit, eventName, characterSelf);
    }

    private static void ExecuteUnitEvent(MapUnit unit, string eventName, bool characterSelf)
    {
        var heap = new Dictionary<string, BoxDataForm.Data>();
        heap["self"] = CodeHelper.CreateBoxByStr(GlobalEventHelper.GetName(
            characterSelf ? GlobalEventHelper.CHARACTER : GlobalEventHelper.SCENEOBJECT,
            characterSelf ? unit.productInfo.Item1.ToString() : unit.data.uid.ToString()));
        unit.ExecuteEvt(eventName, heap);
    }

    public void OnEvent(TileEvent evt)
    {
        QueueUnitEvent(evt.unit, evt.type);
    }

    public void OnEvent(CharacterEvent evt)
    {
        QueueUnitEvent(evt.unit, evt.type, allowBoundaryTouch: true, characterOnCreate: true);
    }

    public void OnEvent(ItemEvent evt)
    {
        QueueUnitEvent(evt.unit, evt.type);
    }

    public void OnEvent(ObjectEvent evt)
    {
        if (evt.type == MapEventType.Remove)
        {
            // Removal does not guarantee TriggerExit; clear the option immediately.
            Z_EventHelper.Invoke(new SceneActionEvent()
            {
                unitUid = evt.unit.data.uid,
                type = SceneActionEventType.Remove
            });
            return;
        }

        QueueUnitEvent(evt.unit, evt.type, allowBoundaryTouch: true);
    }

    public void OnEvent(StoryLifeEvent evt)
    {
        if (evt.type != StoryLifeEventType.EverySecond)
            return;

        evts += () =>
        {
            foreach (var data in ItemUnitForm.DataByUid.Values)
                ExecuteUnitEvent(data.unit, "onPerSecondEvent", false);
            foreach (var data in CharacterUnitForm.DataByUid.Values)
                ExecuteUnitEvent(data.unit, "onPerSecondEvent", true);
            foreach (var data in ObjectUnitForm.DataByUid.Values)
                ExecuteUnitEvent(data.unit, "onPerSecondEvent", false);
        };
    }
}
