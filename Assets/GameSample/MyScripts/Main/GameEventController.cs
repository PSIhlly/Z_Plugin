using Form;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_DataSystem;
using Z_DesignStyle;
using Z_Map;
using Z_UnitSystem;
using Z_DataSystem.Form;
public enum EventType
{
    Global=0,
    Tile=1,
    Object=2,
    Character=3
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
        public bool InterpretFrame()
        {
            if (data.evt.cmds == null)
                return true;
            while(data.cur<data.evt.cmds.Count)
            {
                var cmd = GameEventController.GetCmd(data.evt.cmds[data.cur].name);
                if (cmd != null)
                {
                    var prms = new VarForm.Data[data.evt.cmds[data.cur].prmCnt];
                    for(int i=0;i< data.evt.cmds[data.cur].prmCnt;i++)
                    {
                        prms[i] = data.stack[data.stack.Count-1];
                        data.stack.RemoveAt(data.stack.Count - 1);
                    }
                    var res=cmd.Execute(prms);
                    for(int i=0;i < data.evt.cmds[data.cur].resCnt; i++)
                    {
                        data.stack.Add(res.v[i]);
                    }
                }
                Nxt();
            }
            return true;
        }
        private void Nxt()
        {
            data.cur++;
        }
    }
}

namespace Z_Map
{
    public partial class MapUnit
    {
        public static string evtKey = "evt";
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
                        if (jo != null && jo[evtKey] != null)
                        {
                            foreach (JObject j in (JArray)jo[evtKey])
                            {
                                Debug.Log(j);
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
                jo[evtKey] = ja;
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
                GameManager.instance.evtCtrl.Execute(evtDic[name],data.uid);
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

    public void Update()
    {
        var lst = new List<EventContentForm.Data>(EventContentForm.DataByUid.Values);
        foreach(var data in lst)
        {
            if(data.ctrl.InterpretFrame())
            {
                EventContentForm.RemoveData(data.uid);
            }
        }
        
    }

    public JArray GetEventJa(EventType type)
    {
        JArray ja = new JArray();
        foreach(var data in EventForm.DataByUid.Values)
        {
            switch (type)
            {
                case EventType.Global:
                    if (data.globalEnable)
                        ja.Add( EventForm.GetJoByData(data));
                    break;
                case EventType.Tile:
                    if (data.terrainEnable)
                        ja.Add(EventForm.GetJoByData(data));
                    break;
                case EventType.Object:
                    if (data.objectEnable)
                        ja.Add(EventForm.GetJoByData(data));
                    break;
            }
        }
        return ja;
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
    public void Execute(EventForm.Data evt,int uid=-1)
    {
        var initPrs = new List<VarForm.Data>();
        if (uid!=-1)
        {
            initPrs.Add(new VarForm.Data(-1, "ref", (int)Type.Unit, 0, "", uid));
        }
        EventContentForm.AddData(new EventContentForm.Data(-1, evt.Copy(), 0, initPrs));
    }
}
