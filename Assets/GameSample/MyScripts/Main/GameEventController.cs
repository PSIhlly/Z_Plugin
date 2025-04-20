using Form;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_DesignStyle;
using Z_Map;
using Z_UnitSystem;
public enum EventSetType
{
    Global=0,
    Terrain=1,
    Object=2,
}
namespace Form
{
    public class EventContentController
    {
        public EventContentController(EventContentForm.Data data)
        {
            this.data = data;
        }
        public EventContentForm.Data data;
    }
}

namespace Z_Map
{
    public partial class MapUnit
    {
        private static string _evtKey = "evt";
        private Dictionary<string,EventForm.Data> _evtSet;
        public Dictionary<string, EventForm.Data> evtDic
        {
            get
            {
                if (_evtSet == null)
                {
                    _evtSet = new Dictionary<string, EventForm.Data>();
                    if (!string.IsNullOrEmpty(data.extra))
                    {
                        var jo = JObject.Parse(data.extra);
                        if (jo != null && jo[_evtKey] != null)
                        {
                            foreach (JObject j in (JArray)jo[_evtKey])
                            {
                                var data = EventForm.GetDataByJo(j);
                                _evtSet[data.name] = data;
                            }
                        }
                    }
                }
                return _evtSet;
            }
            set
            {
                var jo = JObject.Parse(data.extra);
                if (jo == null)
                    jo = new JObject();

                JArray ja = new JArray();
                jo[_evtKey] = ja;
                if (value != null)
                {
                    foreach (var evt in value.Values)
                    {
                        ja.Add(EventForm.GetJoByData(evt));
                    }
                }
                data.extra = ja.ToString();
                _evtSet = value;
            }

        }
        public void ExecuteEvt(string name)
        {
            if(evtDic.ContainsKey(name))
            {
                GameManager.instance.evtCtrl.Execute(evtDic[name]);
            }
        }

    }
}
public class GameEventController:Z_Controller<GameManager>,IZ_Listener<CollideEvent>,IZ_Listener<TileEvent>,IZ_Listener<ObjectEvent>, IZ_Listener<CharacterEvent>
{
    

    public GameEventController(GameManager super) : base(super)
    {
        Z_EventHelper.Register<CollideEvent>(this);
        Z_EventHelper.Register<TileEvent>(this);
        Z_EventHelper.Register<ObjectEvent>(this);
        Z_EventHelper.Register<CharacterEvent>(this);
    }

    public static CmdBase GetCmd(string name)
    {
        switch (name)
        {
            case "dialog":
                return new DialogCmd();
            default:
                return new TipsCmd();
        }
    }

  

    public List<EventForm.Data> GetEventSet(EventSetType type)
    {
        List<EventForm.Data> lst = new List<EventForm.Data>();
        foreach(var data in EventForm.DataByUid.Values)
        {
            switch (type)
            {
                case EventSetType.Global:
                    if (data.globalEnable)
                        lst.Add(data);
                    break;
                case EventSetType.Terrain:
                    if (data.terrainEnable)
                        lst.Add(data);
                    break;
                case EventSetType.Object:
                    if (data.objectEnable)
                        lst.Add(data);
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
    public void Execute(EventForm.Data evt)
    {
        EventContentForm.AddData(new EventContentForm.Data(-1, EventForm.GetJoByData(evt).ToString(), 0, 0));
    }
}
