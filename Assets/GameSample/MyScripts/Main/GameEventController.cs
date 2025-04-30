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
public class ShowCmd
{
    public CmdForm.Data data;
    public int depth;
    public int oriId;
    public ShowCmd belong;
    public List<ShowCmd> prms=new List<ShowCmd>();
    public int prmId;
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
                    var prms = new VarForm.Data[data.evt.cmds[data.cur].prmName.Count];
                    for(int i=0;i< prms.Length;i++)
                    {
                        prms[i] = data.stack[data.stack.Count-1];
                        data.stack.RemoveAt(data.stack.Count - 1);
                    }
                    var res=cmd.Execute(prms);
                    for(int i=0;i < data.evt.cmds[data.cur].resTypes.Count; i++)
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
                            var ja = (JArray)jo[evtKey];
                            foreach (var subJo in ja)
                            {
                                var data= EventTriggerForm.GetDataByJo((JObject)subJo);
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
                        ja.Add(EventTriggerForm.GetJoByData(evt));
                    }
                }
                data.extra = jo.ToString();
                _evtSet = value;
            }

        }
        public void ExecuteEvt(string name)
        {
            if(evtDic.ContainsKey(name)&&EventForm.DataByName.ContainsKey(evtDic[name].evt))
            {
                GameManager.instance.evtCtrl.Execute(EventForm.DataByName[evtDic[name].evt], data.uid);
            }
        }

    }
}
public static partial class GlobalMaxSettings
{
    public static int CUSTOM_EVENT_MAX => 1000000;
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
    public static JArray GetEventTriggerJa(EventType type)
    {
        var lst = GetEventTrigger(type);
        JArray ja = new JArray();
        foreach(var data in lst)
        {
           ja.Add( EventTriggerForm.GetJoByData(data));
        }
        return ja;
    }
    public static List<EventTriggerForm.Data> GetEventTrigger(EventType type)
    {
        var lst = new List<EventTriggerForm.Data> ();
        foreach (var data in EventTriggerForm.DataByUid.Values)
        {
            if (data.uid < EventForm.autoUidCnt)
                continue;
            switch (type)
            {
                case EventType.Global:
                    if (data.globalEnable)
                        lst.Add(data);
                    break;
                case EventType.Tile:
                    if (data.terrainEnable)
                        lst.Add(data);
                    break;
                case EventType.Object:
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
    public void Execute(EventForm.Data evt,int uid=-1)
    {
        var initPrs = new List<VarForm.Data>();
        if (uid!=-1)
        {
            initPrs.Add(new VarForm.Data(-1, "ref", (int)Type.Unit, 0, "", uid));
        }
        EventContentForm.AddData(new EventContentForm.Data(-1, evt.Copy(), 0, initPrs));
    }

    public List<ShowCmd> GetShowCmds(List<CmdForm.Data> cmds)
    {
        Dictionary<int,ShowCmd> dic = new Dictionary<int, ShowCmd>();
        for (int i = 0; i < cmds.Count; i++)
        {
            dic[i] = new ShowCmd()
            {
                oriId = i,
                data = cmds[i]
            };
        }
        Stack<ShowCmd> stack = new Stack<ShowCmd>();
        for (int i = 0; i < cmds.Count; i++)
        {
            if (cmds[i].prmTypes != null)
            {
                for (int j = 0; j < cmds[i].prmTypes.Count; j++)
                {
                    var prm = stack.Pop();
                    prm.belong = dic[i];
                    prm.prmId = dic[i].prms.Count;
                    dic[i].prms.Add(prm);
                }
            }
           
            switch(dic[i].data.name)
            {
                case "then":
                    while(stack.Peek().data.name!="if")
                    {
                        stack.Pop().belong = dic[i];
                    }
                    break;
                case "else":
                    while (stack.Peek().data.name != "then")
                    {
                        stack.Pop().belong = dic[i];
                    }
                    break;
            }
            stack.Push(dic[i]);
        }

        Queue<ShowCmd> depthQ = new Queue<ShowCmd>();
        foreach(var data in stack)
        {
            data.depth = 0;
            depthQ.Enqueue(data);
        }
        while(depthQ.Count>0)
        {
            var q = depthQ.Dequeue();
            foreach(var q2 in q.prms)
            {
                q2.depth = q.depth + 1;
                depthQ.Enqueue(q2);
            }
        }

        var res = new List<ShowCmd>(stack.ToArray());
        res.Reverse();
        return res;

    }

}
