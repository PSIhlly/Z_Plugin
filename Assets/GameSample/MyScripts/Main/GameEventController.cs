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
public enum EvtValType
{
    Float = 0,
    Bool = 1,
    String = 2,
    Unit = 3,
    Object = 4,
    Action = 5,
    Image = 6,
    Clips=7,
}
public enum EventType
{
    Global = 0,
    Tile = 1,
    Object = 2,
    Character = 3
}
public class ShowCmd
{
    public CmdForm.Data data;
    public int depth;
    public int oriId;
    public ShowCmd belong;
    public List<ShowCmd> prms = new List<ShowCmd>();
    public int prmId;
}
namespace Form
{
    public partial class CmdForm
    {
        public partial class Data
        {
            public bool isValue => lab == "value";
        }
    }
    public class EventContentController
    {
        public EventContentController(EventContentForm.Data data)
        {
            this.data = data;
        }
        public EventContentForm.Data data;
        public CmdBase cmd;
        public bool InterpretFrame()
        {
            if (data.evt.cmds == null)
                return true;
            while (data.cur < data.evt.cmds.Count)
            {
                if(cmd==null)
                    cmd = GameEventController.GetCmd(data.evt.cmds[data.cur].name);
                if (cmd != null)
                {
                    var prms = new VarForm.Data[0];
                    if (data.evt.cmds[data.cur].prmName != null)
                    {
                        prms = new VarForm.Data[data.evt.cmds[data.cur].prmName.Count];
                    }

                    for (int i = 0; i < prms.Length; i++)
                    {
                        prms[i] = data.stack[data.stack.Count - i - 1];
                    }

                    var res = cmd.Execute(data.evt.cmds[data.cur], prms, data.progress);
                    data.progress = res.progress;
                    if (data.progress != 1)
                        return false;

                    for (int i = 0; i < prms.Length; i++)
                    {
                        data.stack.RemoveAt(data.stack.Count - 1);
                    }


                    if (!string.IsNullOrEmpty(res.ignoreUntilCmd))
                    {
                        Nxt();
                        int times = 0;
                        while (data.evt.cmds.Count > data.cur)
                        {
                            if (data.evt.cmds[data.cur].name == res.ignoreUntilCmd)
                            {
                                times--;
                                if (times < 0)
                                    break;
                            }
                            else if (data.evt.cmds[data.cur].name == res.ignoreTimesCmd)
                            {
                                times++;
                            }
                            Nxt();
                        }
                    }

                    if (data.evt.cmds[data.cur].resTypes != null)
                    {
                        for (int i = 0; i < data.evt.cmds[data.cur].resTypes.Count; i++)
                        {
                            data.stack.Add(res.v[i]);
                        }
                    }


                }
                Nxt();
            }
            return true;
        }
        private void Nxt()
        {
            data.cur++;
            data.progress = 0;
            cmd = null;
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
            if (evtDic.ContainsKey(name) && EventForm.DataByName.ContainsKey(evtDic[name].evt))
            {
                GameManager.instance.evtCtrl.Execute(EventForm.DataByName[evtDic[name].evt], productInfo.Item2);
            }
        }

    }
}
public static partial class GlobalMaxSettings
{
    public static int CUSTOM_EVENT_MAX => 1000000;
}
public class GameEventController : Z_Controller<GameManager>, IZ_Listener<CollideEvent>, IZ_Listener<TileEvent>, IZ_Listener<ObjectEvent>, IZ_Listener<CharacterEvent>
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
            case "tips":
                return new TipsCmd();
            case "num":
                return new NumCmd();
            case "text":
                return new TextCmd();
            case "dialogClip":
                return new DialogClipCmd();
            case "if":
                return new IfCmd();
            case "then":
                return new ThenCmd();
            default:
                return new EmptyCmd();
        }
    }

    public void LateUpdate()
    {
        var lst = new List<EventContentForm.Data>(EventContentForm.DataByUid.Values);
        foreach (var data in lst)
        {
            if (data.ctrl.InterpretFrame())
            {
                EventContentForm.RemoveData(data.uid);
            }
        }

    }
    public static void InsertCmd(List<CmdForm.Data> lst, int id, CmdForm.Data cmd)
    {
        lst.Insert(id, cmd.Copy());
        if (cmd.additionCmds != null)
        {
            for (int i = cmd.additionCmds.Count - 1; i >= 0; i--)
                InsertCmd(lst, id + 1, CmdForm.DataByName[cmd.additionCmds[i]].Copy());
        }
        if (cmd.prmName != null)
        {
            for (int i = 0; i < cmd.prmName.Count; i++)
                InsertCmd(lst, id, CmdForm.defaultData);
        }
    }
    public static void DeleteCmd(List<CmdForm.Data> lst, int id)
    {
        var cur = lst[id];
        if (cur.additionCmds != null && cur.additionCmds.Count > 0)
        {
            int times = 0;
            while (id + 1 < lst.Count)
            {
                if (lst[id + 1].name == cur.name)
                {
                    times++;
                }
                else if (lst[id + 1].name == cur.additionCmds[cur.additionCmds.Count - 1])
                {
                    times--;
                    if (times < 0)
                        break;
                }
                lst.RemoveAt(id + 1);
            }
            lst.RemoveAt(id + 1);
        }
        if (cur.prmName != null)
        {
            int cmdCounnt = cur.prmName.Count;
            while (cmdCounnt > 0)
            {
                id--;
                if (lst[id].prmTypes != null && !lst[id].prmTypes.Contains(EvtValType.Action))
                    cmdCounnt += lst[id].prmName.Count;
                if (lst[id].resTypes != null)
                    cmdCounnt -= lst[id].resTypes.Count;
                lst.RemoveAt(id);
            }
        }
        lst.RemoveAt(id);
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
    public void Execute(EventForm.Data evt, int uid = -1)
    {
        var initPrs = new List<VarForm.Data>();
        EventContentForm.AddData(new EventContentForm.Data(-1, evt.Copy(), 0, initPrs, 1, uid));
    }

    public List<ShowCmd> GetShowCmds(List<CmdForm.Data> cmds)
    {
        Dictionary<int, ShowCmd> dic = new Dictionary<int, ShowCmd>();
        HashSet<int> ignore = new HashSet<int>();
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

            switch (dic[i].data.name)
            {
                case "then":
                    while (stack.Peek().data.name != "if" || ignore.Contains(stack.Peek().oriId))
                    {
                        var prm = stack.Pop();
                        prm.belong = dic[i];
                        prm.prmId = dic[i].prms.Count;
                        dic[i].prms.Add(prm);
                    }
                    ignore.Add(stack.Peek().oriId);
                    break;
                case "else":
                    while (stack.Peek().data.name != "then" || ignore.Contains(stack.Peek().oriId))
                    {
                        var prm = stack.Pop();
                        prm.belong = dic[i];
                        prm.prmId = dic[i].prms.Count;
                        dic[i].prms.Add(prm);
                    }
                    ignore.Add(stack.Peek().oriId);
                    break;
                default:
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
                    break;
            }
            stack.Push(dic[i]);
        }

        Queue<ShowCmd> depthQ = new Queue<ShowCmd>();
        var res = new List<ShowCmd>(stack.ToArray());
        res.Reverse();
        foreach (var data in res)
        {
            data.depth = 0;
            depthQ.Enqueue(data);
        }
        while (depthQ.Count > 0)
        {
            var q = depthQ.Dequeue();
            foreach (var q2 in q.prms)
            {
                q2.depth = q.depth + 1;
                depthQ.Enqueue(q2);
                res.Insert(res.FindIndex(e => e == q) + 1, q2);
            }
        }
        return res;

    }

}
