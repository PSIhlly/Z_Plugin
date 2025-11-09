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

public enum EventType
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
                    jo.Set(evtKey,value);
                }else
                {
                    jo.Set(evtKey, new Dictionary<string, EventTriggerForm.Data>());
                }
                data.extra=jo.ToString();
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
public static partial class GlobalEventHelper
{
    public static bool IsEventTex(string name) => name.Split(AssetDefines.IMAGE_MARK).Length==3&& name.StartsWith(AssetDefines.IMAGE_MARK) && name.EndsWith(AssetDefines.IMAGE_MARK);
    public static string GetEventTexName(string name = "") => AssetDefines.IMAGE_MARK + name + AssetDefines.IMAGE_MARK;
    public static string GetEventAssetTexName(string name)
    {
        string res=name.Split(AssetDefines.IMAGE_MARK)[1];
        if (res == "")
            res = GlobalNameHelper.GetDefaultTexName();
        return res;
    }
    public static string GetGameRetType(Desc desc)
    {
        var res = desc.retType;
        if (IsEventTex(desc.code))
            res = "img";
        return res;
    }
}
public static partial class GlobalSettings
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
    public void Execute(EventProgramDataForm.Data evt, int uid = -1)
    {
        EventInterpretDataForm.AddData(new EventInterpretDataForm.Data(-1, new List<Z_Code.Form.BoxDataForm.Data>(), new Dictionary<string, Z_Code.Form.BoxDataForm.Data>(), evt.Copy(), 0, -1, uid));
    }
    public EntryItem GetEventEntry(EventType objectType, string retType)
    {
        var res = new EntryItem();
        foreach (var data in EventProgramDataForm.DataByName.Values)
        {
            var cat = data.category == "" ? TextManager.instance.GetTxt("unclassified") : data.category;
            var type = data.type == "" ? TextManager.instance.GetTxt("unclassified") : data.type;
            if (!res.subs.ContainsKey(cat))
            {
                res.Add(cat);
            }
            if (!res.subs[cat].subs.ContainsKey(type))
            {
                res.subs[cat].Add(type);
            }
            res.subs[cat].subs[type].Add(data.name);
        }
        return res;
    }
    public EntryItem GetTriggerConditionEntry()
    {
        var res = new EntryItem();
        foreach (var data in EventTriggerForm.DataByName.Values)
        {
            res.Add(TextManager.instance.GetTxt(data.name),id:data.uid);
          
        }
        return res;
    }
    public EntryItem GetCmdEntry(EventType objectType, string retType)
    {
        var res = new EntryItem();
        foreach (var data in GameCmdDataForm.DataByName.Values)
        {
            if (data.retTypes == null)
            {
                if (retType != CmdTypeDataForm.defaultData.name)
                    continue;
            }
            else
            {
                if (data.retTypes[0] != retType)
                    continue;
            }

            if (!res.subs.ContainsKey(data.category))
            {
                res.Add(data.category);
            }
            if (!res.subs[data.category].subs.ContainsKey(data.type))
            {
                res.subs[data.category].Add(data.type);
            }
            res.subs[data.category].subs[data.type].Add(data.name);
        }
        return res;
    }

}
