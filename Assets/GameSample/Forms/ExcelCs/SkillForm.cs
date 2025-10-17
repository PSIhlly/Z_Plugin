using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Z_ByteSerialize;
using Z_DesignStyle;
using Z_UnitSystem.Form;
using Z_Text.Form;
using Z_DataSystem.Form;
using Z_Map.Form;
using Z_Map;
using Z_Ui.Form;
using Z_Code.Form;

namespace Form
{

    public static partial class SkillForm
    {
public static readonly int autoUidCnt=100;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Register()
        {



            Z_Json.extra[typeof(Data)]=((obj)=>{
            if(obj is Data data)
                return GetJoByData(data);
            return null;
            },(jo)=>{
            return GetDataByJo(jo);
            });
        }
        
        private static bool inited;

        public static Z_Chain.Chain uidChain ;

        public static Action<Data> addAction;
        public static Action<Data> removeAction;
        public static Action childInitAction;
        public static Action<Data> childRemoveAction;
        public static Action<Data> childAddAction;

        public static Action<Data,int,int> changeUidAction;
                
        public static Action<Data,string,string> changeNameAction;
                
        public static Action<Data,string,string> changeLabelAction;
                
        public static Action<Data,string,string> changeIconAction;
                
        public static Action<Data,List<SkillType>,List<SkillType>> changeSkilltypesAction;
                
        public static Action<Data,float,float> changeCdAction;
                
        public static Action<Data,Dictionary<string,EventTriggerForm.Data>,Dictionary<string,EventTriggerForm.Data>> changeEventsAction;
                
        public static Action<Data,int,int> changeTriggerconditionuidAction;
                


        public partial class Data
        {

                    private int  _uid;
                    /// <summary>
                    ///
                    ///</summary>
                    public int  uid{
                                get{return _uid;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeUid(this,_uid,value); 
                    }
        
                _uid = value;
                }
                 
                     }
                    
                    private string  _name;
                    /// <summary>
                    ///名称
                    ///</summary>
                    public string  name{
                                get{return _name;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeName(this,_name,value); 
                    }
        
                _name = value;
                }
                 
                     }
                    
                    private string  _label;
                    /// <summary>
                    ///标签
                    ///</summary>
                    public string  label{
                                get{return _label;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeLabel(this,_label,value); 
                    }
        
                _label = value;
                }
                 
                     }
                    
                    private string  _icon;
                    /// <summary>
                    ///图标
                    ///</summary>
                    public string  icon{
                                get{return _icon;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeIcon(this,_icon,value); 
                    }
        
                _icon = value;
                }
                 
                     }
                    
                    private List<SkillType>  _skillTypes;
                    /// <summary>
                    ///可装备类型
                    ///</summary>
                    public List<SkillType>  skillTypes{
                                get{return _skillTypes;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeSkilltypes(this,_skillTypes,value); 
                    }
        
                _skillTypes = value;
                }
                 
                     }
                    
                    private float  _cd;
                    /// <summary>
                    ///冷却(s)
                    ///</summary>
                    public float  cd{
                                get{return _cd;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeCd(this,_cd,value); 
                    }
        
                _cd = value;
                }
                 
                     }
                    
                    private Dictionary<string,EventTriggerForm.Data>  _events;
                    /// <summary>
                    ///事件
                    ///</summary>
                    public Dictionary<string,EventTriggerForm.Data>  events{
                                get{return _events;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeEvents(this,_events,value); 
                    }
        
                _events = value;
                }
                 
                     }
                    
                    private int  _triggerConditionUid;
                    /// <summary>
                    ///条件uid
                    ///</summary>
                    public int  triggerConditionUid{
                                get{return _triggerConditionUid;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeTriggerconditionuid(this,_triggerConditionUid,value); 
                    }
        
                _triggerConditionUid = value;
                }
                 
                     }
                    
            public Data(int uid,string name,string label,string icon,List<SkillType> skillTypes,float cd,Dictionary<string,EventTriggerForm.Data> events,int triggerConditionUid)
            {

             this.uid = uid;
             this.name = name;
             this.label = label;
             this.icon = icon;
             this.skillTypes = skillTypes;
             this.cd = cd;
             this.events = events;
             this.triggerConditionUid = triggerConditionUid;

            }

                public Data Copy(bool sameId = true)
                {
        return new Data(sameId? uid:uidChain.GetId(),name,label,icon,new List<SkillType>(skillTypes),cd,new Dictionary<string,EventTriggerForm.Data>(events),triggerConditionUid);
                }
            
        }

                   private static Data _defaultData=new Data(0,"","","",null,0f,new Dictionary<string,EventTriggerForm.Data>(){},0);
                   public static Data defaultData=>_defaultData.Copy();


            static Dictionary<int, Data> _DataByUid;
            public static Dictionary<int, Data> DataByUid
            {
                get
                {
                    Init();
                    return _DataByUid;
                }
            }
    
            static Dictionary<string, List<Data>> _DatasByLabel;
            public static Dictionary<string, List<Data>> DatasByLabel
            {
                get
                {
                    Init();
                    return _DatasByLabel;
                }
            }
    
            static Dictionary<string, Data> _DataByName;
            public static Dictionary<string, Data> DataByName
            {
                get
                {
                    Init();
                    return _DataByName;
                }
            }
    

        static public void Init()
        {

            InitInternal();
        }
        public static void InitInternal()
        {
            if(inited)
                return;
            inited=true;  
uidChain=new Z_Chain.Chain (autoUidCnt);

                _DataByUid = new Dictionary<int, Data>() {

                };
                    _DataByName = new Dictionary<string, Data>() {
    
                    };
    
                    _DatasByLabel = new Dictionary<string, List<Data>>() {
    
                };


            childInitAction?.Invoke();
            

foreach(var k in _DataByUid.Keys){ uidChain.PopId(k); }
             
        }


        public static List<Data> GetDatasByJa(JArray ja)
        {
            Init();
            List<Data> lst=new List<Data>();
            foreach(JObject jo in ja)
            {
                if(jo.Get<int>("uid")==0)
                    continue;
                lst.Add(GetDataByJo(jo));
            }
            return lst;
        }

        public static JArray GetJaByDatas()
        {
            Init();
            JArray ja=new JArray();
            foreach(Data data in _DataByUid.Values)
            {
                if(data.uid==0)
                    continue;
                ja.Add(GetJoByData(data));
            }
            return ja;
        }

        public static Data GetDataByJo(JObject jo)
        {
            Init();

            Data data=new Data(

                jo.Get<int>("uid"),

                jo.Get<string>("name"),

                jo.Get<string>("label"),

                jo.Get<string>("icon"),

                jo.Get<List<SkillType>>("skillTypes"),

                jo.Get<float>("cd"),

                jo.Get<Dictionary<string,EventTriggerForm.Data>>("events"),

                jo.Get<int>("triggerConditionUid")
                    );

            return data;
        }

        public static JObject GetJoByData(Data data)
        {
            Init();

            JObject jo=new JObject();

            jo.Set<int>("uid",data.uid);

            jo.Set<string>("name",data.name);

            jo.Set<string>("label",data.label);

            jo.Set<string>("icon",data.icon);

            jo.Set<List<SkillType>>("skillTypes",data.skillTypes);

            jo.Set<float>("cd",data.cd);

            jo.Set<Dictionary<string,EventTriggerForm.Data>>("events",data.events);

            jo.Set<int>("triggerConditionUid",data.triggerConditionUid);

            return jo;
        }


        public static int AddData(Data data)
        {
            Init();
            if(DataByUid.ContainsKey(data.uid))
                return data.uid;
            if(data.uid==-1)
            { 
                int uid=uidChain.GetId();
                if(uid==-1)
                    return -1;
                data.uid=uid;  
            }
            uidChain.PopId(data.uid);

        DataByUid[data.uid]=data;
    
                    DataByName[data.name]=data;
    
                    if(!DatasByLabel.ContainsKey(data.label))
                        DatasByLabel[data.label]=new List<Data>();
                    DatasByLabel[data.label].Add(data);
    

            childAddAction?.Invoke(data);
            addAction?.Invoke(data);
            return data.uid;
        }
        public static void RemoveData(int uid)
        {            
            Init();
            if(!DataByUid.ContainsKey(uid))
                return;
               
            var data=DataByUid[uid];

                    DataByUid.Remove(data.uid);
    
                    DataByName.Remove(data.name);
    
                    DatasByLabel[data.label].Remove(data);
                    if(DatasByLabel[data.label].Count==0)
                        DatasByLabel.Remove(data.label);
    

            uidChain.PushId(data.uid);
            childRemoveAction?.Invoke(data);
            removeAction?.Invoke(data);
        }
        public static void Clear()
        {
            Init();
            var keys = new List<int>(DataByUid.Keys);
            foreach(var key in keys)
            {
                    RemoveData(key);
            }

        }
        
        public static void ClearAuto()
        {
            Init();
            var keys = new List<int>(DataByUid.Keys);
            foreach(var key in keys)
            {
                if(key < uidChain.cnt)
                    RemoveData(key);
            }
        }

         private static void RemoveChildren(Data data)
        {
            Init();
            if(data is Data)
               RemoveData(data.uid);      
        }
         private static void AddChildren(Data superData)
        {
            Init();
            if(superData is Data data)
               AddData(data);      
        }
        




            public static void ChangeUid(Data superData,int oldV,int newV)
            {
                if(superData is Data data)
                {

                changeUidAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeName(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                    DataByName.Remove(oldV);
                    DataByName[newV]=data;
 
                changeNameAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeLabel(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                    DatasByLabel[oldV].Remove(data);
                    if(DatasByLabel[oldV].Count==0)
                        DatasByLabel.Remove(oldV);
                    if(!DatasByLabel.ContainsKey(newV))
                        DatasByLabel[newV]=new List<Data>();
                    DatasByLabel[newV].Add(data);
 
                changeLabelAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeIcon(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeIconAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeSkilltypes(Data superData,List<SkillType> oldV,List<SkillType> newV)
            {
                if(superData is Data data)
                {

                changeSkilltypesAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeCd(Data superData,float oldV,float newV)
            {
                if(superData is Data data)
                {

                changeCdAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeEvents(Data superData,Dictionary<string,EventTriggerForm.Data> oldV,Dictionary<string,EventTriggerForm.Data> newV)
            {
                if(superData is Data data)
                {

                changeEventsAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeTriggerconditionuid(Data superData,int oldV,int newV)
            {
                if(superData is Data data)
                {

                changeTriggerconditionuidAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
    }
}
        