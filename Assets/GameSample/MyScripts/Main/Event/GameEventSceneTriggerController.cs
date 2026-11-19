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
using Z_UnitSystem.Form;

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
        private string _evtExtra;
        private Dictionary<string, EventTriggerForm.Data> combinedEvents;
        public Dictionary<string, EventTriggerForm.Data> evtDic
        {
            get
            {
                if (_evtSet == null || _evtExtra != data.extra)
                {
                    _evtExtra = data.extra;
                    combinedEvents?.Clear();
                    _evtSet = new Dictionary<string, EventTriggerForm.Data>();
                    if (!string.IsNullOrEmpty(data.extra))
                    {
                        var jo = JObject.Parse(data.extra);
                        if (jo != null && jo[evtKey] != null)
                        {
                            _evtSet = jo.Get<Dictionary<string, EventTriggerForm.Data>>(evtKey)
                                ?? new Dictionary<string, EventTriggerForm.Data>();
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
                _evtExtra = data.extra;
                combinedEvents?.Clear();
            }

        }
        public Dictionary<string, EventTriggerForm.Data> GetPrototypeEvents()
        {
            if (this is CharacterUnit chU && CharacterProductForm.DataByUid.TryGetValue(chU.productInfo.Item1, out var ch))
                return ch.events;
            if (this is ObjectUnit oU && oU.product is ObjectProductForm.Data ob)
                return ob.events;
            if (this is ItemUnit iU && ItemProductForm.DataByUid.TryGetValue(iU.productInfo.Item1, out var it))
                return it.events;
            if (this is TileUnit tile && tile.data.texDic.TryGetValue(0, out int textureId)
                && MapTextureForm.DataById.TryGetValue(textureId, out var texture))
                return texture.events;
            return null;
        }
        public EventTriggerForm.Data GetPrototypeEvt(string name) => GetPrototypeEvents()?.GetDv(name, null);

        public EventTriggerForm.Data GetEvt(string name)
        {
            var prototype = GetPrototypeEvt(name);
            evtDic.TryGetValue(name, out var local);
            if (local?.evt == null || local.evt.Count == 0)
                return prototype;
            if (prototype?.evt == null)
                return local;

            // Retain a runtime-only merged trigger; never put inherited IDs into extra.
            if (combinedEvents == null)
                combinedEvents = new Dictionary<string, EventTriggerForm.Data>();
            if (!combinedEvents.TryGetValue(name, out var combined))
                combinedEvents[name] = combined = GameEventController.CreateTrigger(name);
            combined.type = prototype.type;
            bool unchanged = combined.evt.Count == prototype.evt.Count + local.evt.Count;
            for (int i = 0; unchanged && i < combined.evt.Count; i++)
                unchanged = combined.evt[i] == (i < prototype.evt.Count
                    ? prototype.evt[i] : local.evt[i - prototype.evt.Count]);
            if (!unchanged)
            {
                var events = new List<int>(prototype.evt);
                events.AddRange(local.evt);
                combined.evt = events;
            }
            return combined;
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
            // A queued trigger can outlive either scene entity involved in it.
            if (IsRemovedSceneUnit(evt.a) || IsRemovedSceneUnit(evt.b))
                return;

            if (evt.a is MapUnit mapUnit)
            {
                var heap = new Dictionary<string, BoxDataForm.Data>();
                if (evt.a is CharacterUnit ch)
                {
                    heap["param1"] = CodeHelper.CreateBoxByStr(GlobalEventHelper.GetName(GlobalEventHelper.CHARACTER, ch.productInfo.Item1.ToString()));
                }
                else if (evt.a is MapUnit sceneObject)
                {
                    heap["param1"] = CodeHelper.CreateBoxByStr(GlobalEventHelper.GetSceneObjectReference(sceneObject));
                }

                if (evt.b is CharacterUnit ch2)
                {
                    heap["param2"] = CodeHelper.CreateBoxByStr(GlobalEventHelper.GetName(GlobalEventHelper.CHARACTER, ch2.productInfo.Item1.ToString()));
                }
                else if (evt.b is MapUnit targetSceneObject)
                {
                    heap["param2"] = CodeHelper.CreateBoxByStr(GlobalEventHelper.GetSceneObjectReference(targetSceneObject));
                }

                switch (evt.type)
                {
                    case CollideEventType.TriggerEnter:
                        if (evt.b is CharacterUnit chU)
                        {
                            mapUnit.ExecuteEvt("onCharacterTouchEvent", heap);
                            if (!IsRemovedSceneUnit(mapUnit) && mapUnit.GetEvt(interActKey) != null && chU.data.uid == PlayManager.instance.sceneCtrl.playerM?.uid)
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
                            if (chU2.data.uid == PlayManager.instance.sceneCtrl.playerM?.uid)
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
    private static bool IsRemovedSceneUnit(Z_UnitSystem.Unit unit)
    {
        if (unit is CharacterUnit character)
            return !CharacterUnitForm.DataByUid.TryGetValue(character.data.uid, out var registeredCharacter)
                || !ReferenceEquals(registeredCharacter, character.data);
        if (unit is ItemUnit item)
            return !ItemUnitForm.DataByUid.TryGetValue(item.data.uid, out var registeredItem)
                || !ReferenceEquals(registeredItem, item.data);
        return unit is ObjectUnit obj &&
               (!ObjectUnitForm.DataByUid.TryGetValue(obj.data.uid, out var registered) ||
                !ReferenceEquals(registered, obj.data));
    }

    // Resolve supported lifecycle types before allocating the deferred callback.
    // Character Create uses a product reference; BoundaryTouch keeps a scene-unit
    // reference, matching the event-language param1 contract.
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
        evts += () =>
        {
            // A deferred lifecycle callback can outlive the original unit. Check
            // both registration and identity so a reused uid cannot target a new unit.
            if (IsRemovedUnit(unit))
                return;

            ExecuteUnitEvent(unit, eventName, characterSelf);
        };
    }

    private static bool IsRemovedUnit(Z_UnitSystem.Unit unit)
    {
        return unit == null
            || unit.data == null
            || !UnitForm.DataByUid.TryGetValue(unit.data.uid, out var registered)
            || !ReferenceEquals(registered, unit.data);
    }

    private static void ExecuteUnitEvent(MapUnit unit, string eventName, bool characterSelf)
    {
        var trigger = unit.GetEvt(eventName);
        if (trigger == null) return;
        var heap = new Dictionary<string, BoxDataForm.Data>();
        heap["param1"] = CodeHelper.CreateBoxByStr(characterSelf
            ? GlobalEventHelper.GetName(GlobalEventHelper.CHARACTER, unit.productInfo.Item1.ToString())
            : GlobalEventHelper.GetSceneObjectReference(unit));
        GameManager.instance.evtCtrl.TriggerEventExecute(trigger, unit.data.uid, heap);
    }

    public void OnEvent(TileEvent evt)
    {
        QueueUnitEvent(evt.unit, evt.type);
    }

    public void OnEvent(CharacterEvent evt)
    {
        if (evt.type == MapEventType.Remove)
        {
            HandleUnitRemoval(evt.unit);
            return;
        }
        QueueUnitEvent(evt.unit, evt.type, allowBoundaryTouch: true, characterOnCreate: true);
    }

    public void OnEvent(ItemEvent evt)
    {
        if (evt.type == MapEventType.Remove)
        {
            HandleUnitRemoval(evt.unit);
            return;
        }
        QueueUnitEvent(evt.unit, evt.type);
    }

    public void OnEvent(ObjectEvent evt)
    {
        if (evt.type == MapEventType.Remove)
        {
            HandleUnitRemoval(evt.unit);
            return;
        }

        QueueUnitEvent(evt.unit, evt.type, allowBoundaryTouch: true);
    }

    private static void HandleUnitRemoval(MapUnit unit)
    {
        // The removing event may finish its current pass; other owned work stops now.
        GameManager.instance.evtCtrl.CompleteUnitEvents(unit.data);
        // Removal need not emit TriggerExit, so clear interaction options immediately.
        Z_EventHelper.Invoke(new SceneActionEvent()
        {
            unitUid = unit.data.uid,
            type = SceneActionEventType.Remove
        });
    }

    public void OnEvent(StoryLifeEvent evt)
    {
        if (evt.type != StoryLifeEventType.EverySecond)
            return;

        evts += () =>
        {
            _super.unitSubscriptions.EverySecond();
        };
    }
}
