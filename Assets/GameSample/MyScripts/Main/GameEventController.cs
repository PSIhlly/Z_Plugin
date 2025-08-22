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

public enum EventType
{
    Global = 0,
    Tile = 1,
    Object = 2,
    Item = 2,
    Character = 3
}
namespace Z_Map
{
    public partial class MapUnit
    {
        public static string evtKey = "evt";
        private Dictionary<string, EventTriggerForm.Data> _evtSet;
        public Dictionary<string, EventTriggerForm.Data> evtDic
        {
            get
            {
                if (_evtSet == null)
                {
                    bool needCreate = true;
                    _evtSet = new Dictionary<string, EventTriggerForm.Data>();
                    if (!string.IsNullOrEmpty(data.extra))
                    {
                        var jo = JObject.Parse(data.extra);
                        if (jo != null && jo[evtKey] != null)
                        {
                            var ja = (JArray)jo[evtKey];
                            foreach (var subJo in ja)
                            {
                                var data = EventTriggerForm.GetDataByJo((JObject)subJo);
                                _evtSet[data.name] = data;
                            }
                            needCreate = false;
                        }
                    }
                    if(needCreate)
                    {
                        if (this is CharacterUnit)
                        {
                            _evtSet = GameEventController.GetEventTriggerDic(EventType.Character);
                        }
                        else if (this is ObjectUnit)
                        {
                            _evtSet = GameEventController.GetEventTriggerDic(EventType.Object);
                        }
                        else if (this is ItemUnit)
                        {
                            _evtSet = GameEventController.GetEventTriggerDic(EventType.Item);
                        }
                        else if (this is TileUnit)
                        {
                            _evtSet = GameEventController.GetEventTriggerDic(EventType.Tile);
                        }
                    }
                }
                
                return _evtSet;
            }
            set
            {
                
                var jo = string.IsNullOrEmpty(data.extra)?new JObject():JObject.Parse(data.extra);

                JArray ja = new JArray();
                jo[evtKey] = ja;
                if (value != null)
                {
                    foreach (var evt in value.Values)
                    {
                        ja.Add(EventTriggerForm.GetJoByData(evt));
                    }
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
public static partial class GlobalMaxSettings
{
    public static int CUSTOM_EVENT_MAX => 1000000;
}
public class GameEventController : Z_Controller<GameManager>, IZ_Listener<CollideEvent>, IZ_Listener<TileEvent>, IZ_Listener<ItemEvent>, IZ_Listener<ObjectEvent>, IZ_Listener<CharacterEvent>
{


    public GameEventController(GameManager super) : base(super)
    {
        Z_EventHelper.Register<CollideEvent>(this);
        Z_EventHelper.Register<TileEvent>(this);
        Z_EventHelper.Register<ItemEvent>(this);
        Z_EventHelper.Register<ObjectEvent>(this);
        Z_EventHelper.Register<CharacterEvent>(this);
    }

   
    public void LateUpdate()
    {
        var lst = new List<EventInterpretDataForm.Data>(EventInterpretDataForm.DataByUid.Values);
        foreach (var data in lst)
        {
            if (data.Interpret())
            {
                EventInterpretDataForm.RemoveData(data.uid);
            }
        }

    }

    public static Dictionary<string, EventTriggerForm.Data> GetEventTriggerDic(EventType type)
    {
        var dic = new Dictionary<string, EventTriggerForm.Data>();
        var lst = GetEventTrigger(type);
        foreach (var data in lst)
        {
            dic[data.name] = data;
        }
        return dic;
    }
    public static JArray GetEventTriggerJa(EventType type)
    {
        var lst = GetEventTrigger(type);
        JArray ja = new JArray();
        foreach (var data in lst)
        {
            ja.Add(EventTriggerForm.GetJoByData(data));
        }
        return ja;
    }
    public static List<EventTriggerForm.Data> GetEventTrigger(EventType type)
    {
        var lst = new List<EventTriggerForm.Data>();
        foreach (var data in EventTriggerForm.DataByUid.Values)
        {
            switch (type)
            {
                case EventType.Global:
                    if (data.globalEnable)
                        lst.Add(data.Copy());
                    break;
                case EventType.Tile:
                    if (data.terrainEnable)
                        lst.Add(data.Copy());
                    break;
                case EventType.Object:
                    if (data.objectEnable)
                        lst.Add(data.Copy());
                    break;
                case EventType.Character:
                    if (data.characterEnable)
                        lst.Add(data.Copy());
                    break;
            }
        }
        return lst;
    }
    public void OnEvent(CollideEvent evt)
    {
        if (evt.a is MapInstance mapIns)
        {
            switch (evt.type)
            {
                case CollideEventType.TriggerEnter:
                    mapIns.unit.ExecuteEvt("OnTouch");
                    break;
                case CollideEventType.TriggerExit:
                    mapIns.unit.ExecuteEvt("OnLeave");
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
                    mapUnit.ExecuteEvt("OnShow");
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
                    mapUnit.ExecuteEvt("OnShow");
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
                    mapUnit.ExecuteEvt("OnShow");
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
                    mapUnit.ExecuteEvt("OnShow");
                    break;
            }
        }
    }
    public void Execute(EventProgramDataForm.Data evt, int uid = -1)
    {
        EventInterpretDataForm.AddData(new EventInterpretDataForm.Data(-1, new  List<Z_Code.Form.BoxDataForm.Data>(),new Dictionary<string, Z_Code.Form.BoxDataForm.Data>(), evt.Copy(), 0, -1, uid));
    }
    public void GetEvents(EventType objectType,CmdTypeDataForm.Data retType,out Dictionary<string, (Sprite, object)> sub)
    {
        sub = new Dictionary<string,(Sprite,object)>();
        foreach(var data in EventProgramDataForm.DataByName.Values)
        {
            if(!sub.ContainsKey(data.category))
            {
                sub[data.category]= (null, new Dictionary<string, (Sprite, object)>());
            }
            var sub2 = sub[data.category].Item2 as Dictionary<string, (Sprite, object)>;
            if (!sub2.ContainsKey(data.type))
            {
                sub2[data.type] = (null, new Dictionary<string, (Sprite, object)>());
            }
            var sub3 = sub2[data.category].Item2 as Dictionary<string, (Sprite, object)>;
            sub3[data.name] = (null, null);
        }

    }
   

}
