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

    public static partial class SkillProductForm
    {

        

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Register()
        {

                ProductForm.childInitAction+=InitInternal;


                ProductForm.childRemoveAction+=RemoveChildren;
                ProductForm.childAddAction+=AddChildren;
            

            ProductForm.changeUidAction+=ChangeUid;

            ProductForm.changeNameAction+=ChangeName;

            ProductForm.changeLabelAction+=ChangeLabel;

            ProductForm.changeProtouidAction+=ChangeProtouid;

            Z_Json.extra[typeof(Data)]=((obj)=>{
            if(obj is Data data)
                return GetJoByData(data);
            return null;
            },(jo)=>{
            return GetDataByJo(jo);
            });
        }
        
        private static bool inited;

        public static Z_Chain.Chain uidChain =>ProductForm.uidChain;

        public static Action<Data> addAction;
        public static Action<Data> removeAction;
        public static Action childInitAction;
        public static Action<Data> childRemoveAction;
        public static Action<Data> childAddAction;
        
        public static Action<Data> beforeGetAction;

        public static Action<Data,int,int> changeUidAction;
                
        public static Action<Data,string,string> changeNameAction;
                
        public static Action<Data,string,string> changeLabelAction;
                
        public static Action<Data,int,int> changeProtouidAction;
                
        public static Action<Data,string,string> changeIconAction;
                
        public static Action<Data,Dictionary<string,SkillParamForm.Data>,Dictionary<string,SkillParamForm.Data>> changeParamdicAction;
                
        public static Action<Data,List<SkillType>,List<SkillType>> changeSkilltypesAction;
                
        public static Action<Data,int,int> changeCharacteruidAction;
                
        public static Action<Data,float,float> changeCdAction;
                
        public static Action<Data,float,float> changeLastusetimeAction;
                
        public static Action<Data,Dictionary<string,EventTriggerForm.Data>,Dictionary<string,EventTriggerForm.Data>> changeEventsAction;
                
        public static Action<Data,int,int> changeTriggerconditionuidAction;
                


        public partial class Data : ProductForm.Data
        {

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
                    
                    private Dictionary<string,SkillParamForm.Data>  _paramDic;
                    /// <summary>
                    ///数据
                    ///</summary>
                    public Dictionary<string,SkillParamForm.Data>  paramDic{
                                get{return _paramDic;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeParamdic(this,_paramDic,value); 
                    }
        
                _paramDic = value;
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
                    
                    private int  _characterUid;
                    /// <summary>
                    ///所属角色Uid
                    ///</summary>
                    public int  characterUid{
                                get{return _characterUid;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeCharacteruid(this,_characterUid,value); 
                    }
        
                _characterUid = value;
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
                    
                    private float  _lastUseTime;
                    /// <summary>
                    ///上次使用时间戳(s)
                    ///</summary>
                    public float  lastUseTime{
                                get{return _lastUseTime;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeLastusetime(this,_lastUseTime,value); 
                    }
        
                _lastUseTime = value;
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
                    
            public Data(ProductForm.Data data):base(data.uid,data.name,data.label,data.protoUid)
            {
            }
            
            public Data(int uid,string name,string label,int protoUid,string icon,Dictionary<string,SkillParamForm.Data> paramDic,List<SkillType> skillTypes,int characterUid,float cd,float lastUseTime,Dictionary<string,EventTriggerForm.Data> events,int triggerConditionUid):base(uid,name,label,protoUid)
            {

             this.uid = uid;
             this.name = name;
             this.label = label;
             this.protoUid = protoUid;
             this.icon = icon;
             this.paramDic = paramDic;
             this.skillTypes = skillTypes;
             this.characterUid = characterUid;
             this.cd = cd;
             this.lastUseTime = lastUseTime;
             this.events = events;
             this.triggerConditionUid = triggerConditionUid;

            }
            public void Reset(Data data)
            {

             this.uid = data.uid;
             this.name = data.name;
             this.label = data.label;
             this.protoUid = data.protoUid;
             this.icon = data.icon;
             this.paramDic = data.paramDic;
             this.skillTypes = data.skillTypes;
             this.characterUid = data.characterUid;
             this.cd = data.cd;
             this.lastUseTime = data.lastUseTime;
             this.events = data.events;
             this.triggerConditionUid = data.triggerConditionUid;
            }

                public Data Copy(bool sameId = true)
                {
        return new Data(sameId? uid:uidChain.GetId(),name,label,protoUid,icon,new Dictionary<string,SkillParamForm.Data>(paramDic),new List<SkillType>(skillTypes),characterUid,cd,lastUseTime,new Dictionary<string,EventTriggerForm.Data>(events),triggerConditionUid);
                }
            
            public override  void BeforeGet()
            {
                base.BeforeGet();
                SkillProductForm.beforeGetAction?.Invoke(this);
            }
        }

                   private static Data _defaultData=new Data(0,"","",0,"",new Dictionary<string,SkillParamForm.Data>(){},new List<SkillType>(),0,0f,0f,new Dictionary<string,EventTriggerForm.Data>(){},0);
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
    
            static Dictionary<(string,int), List<Data>> _DatasByLabelProtouid;
            public static Dictionary<(string,int), List<Data>> DatasByLabelProtouid
            {
                get
                {
                    Init();
                    return _DatasByLabelProtouid;
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
    
            static Dictionary<int, List<Data>> _DatasByProtouid;
            public static Dictionary<int, List<Data>> DatasByProtouid
            {
                get
                {
                    Init();
                    return _DatasByProtouid;
                }
            }
    
            static Dictionary<(string,int), Data> _DataByNameProtouid;
            public static Dictionary<(string,int), Data> DataByNameProtouid
            {
                get
                {
                    Init();
                    return _DataByNameProtouid;
                }
            }
    

        static public void Init()
        {

            ProductForm.Init();

        }
        public static void InitInternal()
        {
            if(inited)
                return;
            inited=true;  

        

                _DataByUid = new Dictionary<int, Data>() {

                };
                    _DataByNameProtouid = new Dictionary<(string,int), Data>() {
    
                    };
    
                    _DatasByLabelProtouid = new Dictionary<(string,int), List<Data>>() {
    
                };

                    _DatasByLabel = new Dictionary<string, List<Data>>() {
    
                };

                    _DatasByProtouid = new Dictionary<int, List<Data>>() {
    
                };


            childInitAction?.Invoke();
            

            foreach(var data in DataByUid.Values)
            {
                ProductForm.AddData(data);
            }


        
             
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

                jo.SelectToken("uid")==null?defaultData.uid:jo.Get<int>("uid"),

                jo.SelectToken("name")==null?defaultData.name:jo.Get<string>("name"),

                jo.SelectToken("label")==null?defaultData.label:jo.Get<string>("label"),

                jo.SelectToken("protoUid")==null?defaultData.protoUid:jo.Get<int>("protoUid"),

                jo.SelectToken("icon")==null?defaultData.icon:jo.Get<string>("icon"),

                jo.SelectToken("paramDic")==null?defaultData.paramDic:jo.Get<Dictionary<string,SkillParamForm.Data>>("paramDic"),

                jo.SelectToken("skillTypes")==null?defaultData.skillTypes:jo.Get<List<SkillType>>("skillTypes"),

                jo.SelectToken("characterUid")==null?defaultData.characterUid:jo.Get<int>("characterUid"),

                jo.SelectToken("cd")==null?defaultData.cd:jo.Get<float>("cd"),

                jo.SelectToken("lastUseTime")==null?defaultData.lastUseTime:jo.Get<float>("lastUseTime"),

                jo.SelectToken("events")==null?defaultData.events:jo.Get<Dictionary<string,EventTriggerForm.Data>>("events"),

                jo.SelectToken("triggerConditionUid")==null?defaultData.triggerConditionUid:jo.Get<int>("triggerConditionUid")
                    );

            return data;
        }

        public static JObject GetJoByData(Data data)
        {
            Init();
            data.BeforeGet();

            JObject jo=new JObject();

            jo.Set<int>("uid",data.uid);

            jo.Set<string>("name",data.name);

            jo.Set<string>("label",data.label);

            jo.Set<int>("protoUid",data.protoUid);

            jo.Set<string>("icon",data.icon);

            jo.Set<Dictionary<string,SkillParamForm.Data>>("paramDic",data.paramDic);

            jo.Set<List<SkillType>>("skillTypes",data.skillTypes);

            jo.Set<int>("characterUid",data.characterUid);

            jo.Set<float>("cd",data.cd);

            jo.Set<float>("lastUseTime",data.lastUseTime);

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
    
                    DataByNameProtouid[(data.name,data.protoUid)]=data;
    
                    if(!DatasByLabelProtouid.ContainsKey((data.label,data.protoUid)))
                        DatasByLabelProtouid[(data.label,data.protoUid)]=new List<Data>();
                    DatasByLabelProtouid[(data.label,data.protoUid)].Add(data);
    
                    if(!DatasByLabel.ContainsKey(data.label))
                        DatasByLabel[data.label]=new List<Data>();
                    DatasByLabel[data.label].Add(data);
    
                    if(!DatasByProtouid.ContainsKey(data.protoUid))
                        DatasByProtouid[data.protoUid]=new List<Data>();
                    DatasByProtouid[data.protoUid].Add(data);
    
ProductForm.AddData(data);
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
    
                    DataByNameProtouid.Remove((data.name,data.protoUid));
    
                    DatasByLabelProtouid[(data.label,data.protoUid)].Remove(data);
                    if(DatasByLabelProtouid[(data.label,data.protoUid)].Count==0)
                        DatasByLabelProtouid.Remove((data.label,data.protoUid));
    
                    DatasByLabel[data.label].Remove(data);
                    if(DatasByLabel[data.label].Count==0)
                        DatasByLabel.Remove(data.label);
    
                    DatasByProtouid[data.protoUid].Remove(data);
                    if(DatasByProtouid[data.protoUid].Count==0)
                        DatasByProtouid.Remove(data.protoUid);
    
ProductForm.RemoveData(uid);
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

         private static void RemoveChildren(ProductForm.Data data)
        {
            Init();
            if(data is Data)
               RemoveData(data.uid);      
        }
         private static void AddChildren(ProductForm.Data superData)
        {
            Init();
            if(superData is Data data)
               AddData(data);      
        }
        




            public static void ChangeUid(ProductForm.Data superData,int oldV,int newV)
            {
                if(superData is Data data)
                {

                changeUidAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeName(ProductForm.Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                    DataByNameProtouid.Remove((oldV,data.protoUid));
                    DataByNameProtouid[(newV,data.protoUid)]=data;
 
                changeNameAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeLabel(ProductForm.Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                    DatasByLabel[oldV].Remove(data);
                    if(DatasByLabel[oldV].Count==0)
                        DatasByLabel.Remove(oldV);
                    if(!DatasByLabel.ContainsKey(newV))
                        DatasByLabel[newV]=new List<Data>();
                    DatasByLabel[newV].Add(data);
 
                    DatasByLabelProtouid[(oldV,data.protoUid)].Remove(data);
                    if(DatasByLabelProtouid[(oldV,data.protoUid)].Count==0)
                        DatasByLabelProtouid.Remove((oldV,data.protoUid));
                    if(!DatasByLabelProtouid.ContainsKey((newV,data.protoUid)))
                        DatasByLabelProtouid[(newV,data.protoUid)]=new List<Data>();
                    DatasByLabelProtouid[(newV,data.protoUid)].Add(data);
 
                changeLabelAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeProtouid(ProductForm.Data superData,int oldV,int newV)
            {
                if(superData is Data data)
                {

                    DatasByProtouid[oldV].Remove(data);
                    if(DatasByProtouid[oldV].Count==0)
                        DatasByProtouid.Remove(oldV);
                    if(!DatasByProtouid.ContainsKey(newV))
                        DatasByProtouid[newV]=new List<Data>();
                    DatasByProtouid[newV].Add(data);
 
                    DataByNameProtouid.Remove((data.name,oldV));
                    DataByNameProtouid[(data.name,newV)]=data;
 
                    DatasByLabelProtouid[(data.label,oldV)].Remove(data);
                    if(DatasByLabelProtouid[(data.label,oldV)].Count==0)
                        DatasByLabelProtouid.Remove((data.label,oldV));
                    if(!DatasByLabelProtouid.ContainsKey((data.label,newV)))
                        DatasByLabelProtouid[(data.label,newV)]=new List<Data>();
                    DatasByLabelProtouid[(data.label,newV)].Add(data);
 
                changeProtouidAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeIcon(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeIconAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeParamdic(Data superData,Dictionary<string,SkillParamForm.Data> oldV,Dictionary<string,SkillParamForm.Data> newV)
            {
                if(superData is Data data)
                {

                changeParamdicAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeSkilltypes(Data superData,List<SkillType> oldV,List<SkillType> newV)
            {
                if(superData is Data data)
                {

                changeSkilltypesAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeCharacteruid(Data superData,int oldV,int newV)
            {
                if(superData is Data data)
                {

                changeCharacteruidAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeCd(Data superData,float oldV,float newV)
            {
                if(superData is Data data)
                {

                changeCdAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeLastusetime(Data superData,float oldV,float newV)
            {
                if(superData is Data data)
                {

                changeLastusetimeAction?.Invoke(data,oldV,newV);
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
        