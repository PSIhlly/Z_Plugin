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

public static class GlobalVariableTriggerBindings
{
    // MapMain stores the Unit arrays as JSON strings, and each Unit stores extra
    // as another JSON string. Remove only this dictionary key, preserving all
    // unrelated fields and Any bindings before the global UID can be reused.
    public static bool RemoveFromJson(string json, string key, out string result)
    {
        result = json;
        if (string.IsNullOrEmpty(json) || !json.Contains(key)) return false;
        var token = JToken.Parse(json);
        if (!Remove(token, key)) return false;
        result = token.ToString(Newtonsoft.Json.Formatting.None);
        return true;
    }
    private static bool Remove(JToken token, string key)
    {
        bool changed = false;
        if (token is JObject obj)
        {
            if (obj["events"] is JArray entries)
                foreach (var entry in entries.OfType<JObject>().Where(entry => (string)entry["k"] == key).ToArray())
                { entry.Remove(); changed = true; }
            foreach (var property in obj.Properties().ToArray())
            {
                if (property.Value.Type == JTokenType.String && (property.Name == "extra" || property.Name == "mapJa"
                    || property.Name == "characterJa" || property.Name == "objectJa" || property.Name == "itemJa"))
                {
                    if (RemoveFromJson((string)property.Value, key, out var nested))
                    { property.Value = nested; changed = true; }
                }
                else changed |= Remove(property.Value, key);
            }
        }
        else if (token is JArray array)
            foreach (var child in array) changed |= Remove(child, key);
        return changed;
    }
}
public class GameEventStoryTriggerController : Z_Controller<GameEventController>, IZ_Listener<StoryLifeEvent>, IZ_Listener<StoryItemEvent>, IZ_Listener<StoryCharacterEvent>, IZ_Listener<CharacterSkillEvent>, IZ_Listener<GlobalVariableChangedEvent>
{

    public GameEventStoryTriggerController(GameEventController super) : base(super)
    {
        Z_EventHelper.Register<StoryLifeEvent>(this);
        Z_EventHelper.Register<StoryItemEvent>(this);
        Z_EventHelper.Register<StoryCharacterEvent>(this);
        Z_EventHelper.Register<CharacterSkillEvent>(this);
        Z_EventHelper.Register<GlobalVariableChangedEvent>(this);
    }

    public void OnEvent(GlobalVariableChangedEvent evt)
    {
        if (evt == null || !DynamicGlobalSettings.playing)
            return;
        var game = GameManager.instance;
        var scene = game.curScene;
        if (scene?.events == null || game.curProgress == null || game.curProgress.eventState == EventState.Leave)
            return;

        var heap = new Dictionary<string, BoxDataForm.Data>
        {
            { "param1", CodeHelper.CreateBoxByNum(0) },
            { "param2", CodeHelper.CreateBoxByStr(evt.name) },
            { "param3", CodeHelper.CreateBoxByNum(GetGlobalVariableDelta(evt)) },
            // Keep the full new-value snapshot available to existing Unit handlers.
            { "param4", evt.newValue ?? CodeHelper.CreateBoxByNum(0) }
        };
        _super.TriggerEventExecute(scene.events.GetDv("onGlobalVarChangeEvent$", null), 0, heap, copyDefaultHeap: true);
        if (ReferenceEquals(game.curScene, scene))
            _super.TriggerEventExecute(scene.events.GetDv($"onGlobalVarChangeEvent${evt.uid}", null), 0, heap, copyDefaultHeap: true);
        if (ReferenceEquals(game.curScene, scene))
            _super.unitSubscriptions.GlobalChanged(evt, heap);
    }

    private static float GetGlobalVariableDelta(GlobalVariableChangedEvent evt)
    {
        // Globals may also hold text/dictionaries. They still trigger, but have
        // no numeric delta; missing values have the setter's effective zero.
        var before = evt.oldValue;
        var after = evt.newValue;
        if (before?.str != null || after?.str != null
            || (before?.dic?.Count ?? 0) > 0 || (after?.dic?.Count ?? 0) > 0)
            return 0;
        return (after?.num ?? 0) - (before?.num ?? 0);
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
                        var unitEvt = unit.GetEvt("onLeaveSceneEvent");
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
        var heap = new Dictionary<string, BoxDataForm.Data>() { { "param1", CodeHelper.CreateBoxByStr(GlobalEventHelper.GetName(GlobalEventHelper.ITEM, evt.data.uid.ToString())) } };
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
                heap["param2"] = heap["param1"];
                heap["param1"] = CodeHelper.CreateBoxByStr(GlobalEventHelper.GetName(GlobalEventHelper.CHARACTER, GameManager.instance.curProgress.characterUid.ToString()));
                _super.TriggerEventExecute(evt.data.events.GetDv("onUseEvent", null), evt.data.uid, heap);
                break;
        }
    }

    public void OnEvent(StoryCharacterEvent evt)
    {
        if (evt.data == null)
            return;
        var heap = new Dictionary<string, BoxDataForm.Data>() { { "param1", CodeHelper.CreateBoxByStr(GlobalEventHelper.GetName(GlobalEventHelper.CHARACTER, evt.data.uid.ToString())) }, };

        switch (evt.type)
        {
            case StoryCharacterEventType.ParamChange:
                heap["param2"] = CodeHelper.CreateBoxByStr(evt.name);
                heap["param3"] = CodeHelper.CreateBoxByNum((float)evt.obj);
                _super.TriggerEventExecute(GameManager.instance.curScene.events.GetDv("onCharacterParamChangeEvent", null), 0, heap);
                _super.TriggerEventExecute(GameManager.instance.curScene.events.GetDv($"onCharacterParamChangeEvent$${evt.name}", null), 0, heap);
                _super.TriggerEventExecute(GameManager.instance.curScene.events.GetDv($"onCharacterParamChangeEvent${evt.data.protoUid}$", null), 0, heap);
                _super.TriggerEventExecute(GameManager.instance.curScene.events.GetDv($"onCharacterParamChangeEvent${evt.data.protoUid}${evt.name}", null), 0, heap);
                break;
            case StoryCharacterEventType.SkillParamChange:
                heap["param2"] = CodeHelper.CreateBoxByStr(GlobalEventHelper.GetName(GlobalEventHelper.SKILL, evt.name));
                break;
        }
    }
    public void OnEvent(CharacterSkillEvent evt)
    {
        switch (evt.type)
        {
            case CharacterSkillEventType.Use:
                var heap = new Dictionary<string, BoxDataForm.Data>() { 
                  { "param2", CodeHelper.CreateBoxByStr(GlobalEventHelper.GetName(GlobalEventHelper.SKILL, evt.skillUid.ToString())) },
                  {"param1", CodeHelper.CreateBoxByStr(GlobalEventHelper.GetName(GlobalEventHelper.CHARACTER, evt.data.uid.ToString())) }
                };
                _super.TriggerEventExecute(SkillProductForm.DataByUid[evt.skillUid].events.GetDv("invoke", null), 0, heap);
                break;
        }
    }
}
