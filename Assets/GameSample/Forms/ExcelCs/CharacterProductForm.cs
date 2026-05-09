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

    public static partial class CharacterProductForm
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
                
        public static Action<Data,string,string> changeAvatartexnameAction;
                
        public static Action<Data,Dictionary<string,CharacterParamForm.Data>,Dictionary<string,CharacterParamForm.Data>> changeParamdicAction;
                
        public static Action<Data,int,int> changeProtouidAction;
                
        public static Action<Data,Dictionary<string,CharacterAnimForm.Data>,Dictionary<string,CharacterAnimForm.Data>> changeAnimdicAction;
                
        public static Action<Data,Dictionary<string,string>,Dictionary<string,string>> changeDefaultanimnameAction;
                
        public static Action<Data,FaceType,FaceType> changeFacetypeAction;
                
        public static Action<Data,string,string> changeSpeedparamnameAction;
                
        public static Action<Data,string,string> changeHpparamnameAction;
                
        public static Action<Data,Dictionary<string,EventTriggerForm.Data>,Dictionary<string,EventTriggerForm.Data>> changeEventsAction;
                
        public static Action<Data,Dictionary<EquipPartType,int>,Dictionary<EquipPartType,int>> changeEquipsAction;
                
        public static Action<Data,string,string> changeDescAction;
                
        public static Action<Data,string,string> changeIllustrationAction;
                
        public static Action<Data,bool,bool> changeUniqueAction;
                
        public static Action<Data,Dictionary<SkillType,int>,Dictionary<SkillType,int>> changeSkillAction;
                
        public static Action<Data,float,float> changeRecoverytimeAction;
                
        public static Action<Data,bool,bool> changeEnablenavAction;
                
        public static Action<Data,string,string> changeMinimapiconAction;
                


        public partial class Data : ProductForm.Data
        {

                    private string  _avatarTexName;
                    /// <summary>
                    ///头像名称
                    ///</summary>
                    public string  avatarTexName{
                                get{return _avatarTexName;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeAvatartexname(this,_avatarTexName,value); 
                    }
        
                _avatarTexName = value;
                }
                 
                     }
                    
                    private Dictionary<string,CharacterParamForm.Data>  _paramDic;
                    /// <summary>
                    ///数据
                    ///</summary>
                    public Dictionary<string,CharacterParamForm.Data>  paramDic{
                                get{return _paramDic;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeParamdic(this,_paramDic,value); 
                    }
        
                _paramDic = value;
                }
                 
                     }
                    
                    private Dictionary<string,CharacterAnimForm.Data>  _animDic;
                    /// <summary>
                    ///动画
                    ///</summary>
                    public Dictionary<string,CharacterAnimForm.Data>  animDic{
                                get{return _animDic;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeAnimdic(this,_animDic,value); 
                    }
        
                _animDic = value;
                }
                 
                     }
                    
                    private Dictionary<string,string>  _defaultAnimName;
                    /// <summary>
                    ///预设动画名
                    ///</summary>
                    public Dictionary<string,string>  defaultAnimName{
                                get{return _defaultAnimName;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeDefaultanimname(this,_defaultAnimName,value); 
                    }
        
                _defaultAnimName = value;
                }
                 
                     }
                    
                    private FaceType  _faceType;
                    /// <summary>
                    ///朝向类型
                    ///</summary>
                    public FaceType  faceType{
                                get{return _faceType;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeFacetype(this,_faceType,value); 
                    }
        
                _faceType = value;
                }
                 
                     }
                    
                    private string  _speedParamName;
                    /// <summary>
                    ///速度参数名
                    ///</summary>
                    public string  speedParamName{
                                get{return _speedParamName;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeSpeedparamname(this,_speedParamName,value); 
                    }
        
                _speedParamName = value;
                }
                 
                     }
                    
                    private string  _hpParamName;
                    /// <summary>
                    ///血量参数名
                    ///</summary>
                    public string  hpParamName{
                                get{return _hpParamName;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeHpparamname(this,_hpParamName,value); 
                    }
        
                _hpParamName = value;
                }
                 
                     }
                    
                    private Dictionary<string,EventTriggerForm.Data>  _events;
                    /// <summary>
                    ///事件
                    ///</summary>
                    public Dictionary<string,EventTriggerForm.Data>  events{
                                get{return _events;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeEvents(this,_events,value); 
                    }
        
                _events = value;
                }
                 
                     }
                    
                    private Dictionary<EquipPartType,int>  _equips;
                    /// <summary>
                    ///装备（道具uid）
                    ///</summary>
                    public Dictionary<EquipPartType,int>  equips{
                                get{return _equips;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeEquips(this,_equips,value); 
                    }
        
                _equips = value;
                }
                 
                     }
                    
                    private string  _desc;
                    /// <summary>
                    ///描述
                    ///</summary>
                    public string  desc{
                                get{return _desc;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeDesc(this,_desc,value); 
                    }
        
                _desc = value;
                }
                 
                     }
                    
                    private string  _illustration;
                    /// <summary>
                    ///立绘
                    ///</summary>
                    public string  illustration{
                                get{return _illustration;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeIllustration(this,_illustration,value); 
                    }
        
                _illustration = value;
                }
                 
                     }
                    
                    private bool  _unique;
                    /// <summary>
                    ///本人
                    ///</summary>
                    public bool  unique{
                                get{return _unique;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeUnique(this,_unique,value); 
                    }
        
                _unique = value;
                }
                 
                     }
                    
                    private Dictionary<SkillType,int>  _skill;
                    /// <summary>
                    ///技能
                    ///</summary>
                    public Dictionary<SkillType,int>  skill{
                                get{return _skill;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeSkill(this,_skill,value); 
                    }
        
                _skill = value;
                }
                 
                     }
                    
                    private float  _recoveryTime;
                    /// <summary>
                    ///取消僵直时间戳(s)
                    ///</summary>
                    public float  recoveryTime{
                                get{return _recoveryTime;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeRecoverytime(this,_recoveryTime,value); 
                    }
        
                _recoveryTime = value;
                }
                 
                     }
                    
                    private bool  _enableNav;
                    /// <summary>
                    ///开启寻路
                    ///</summary>
                    public bool  enableNav{
                                get{return _enableNav;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeEnablenav(this,_enableNav,value); 
                    }
        
                _enableNav = value;
                }
                 
                     }
                    
                    private string  _minimapIcon;
                    /// <summary>
                    ///小地图标识
                    ///</summary>
                    public string  minimapIcon{
                                get{return _minimapIcon;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeMinimapicon(this,_minimapIcon,value); 
                    }
        
                _minimapIcon = value;
                }
                 
                     }
                    
            public Data(ProductForm.Data data):base(data.uid,data.name,data.label,data.protoUid)
            {
            }
            
            public Data(int uid,string name,string label,string avatarTexName,Dictionary<string,CharacterParamForm.Data> paramDic,int protoUid,Dictionary<string,CharacterAnimForm.Data> animDic,Dictionary<string,string> defaultAnimName,FaceType faceType,string speedParamName,string hpParamName,Dictionary<string,EventTriggerForm.Data> events,Dictionary<EquipPartType,int> equips,string desc,string illustration,bool unique,Dictionary<SkillType,int> skill,float recoveryTime,bool enableNav,string minimapIcon):base(uid,name,label,protoUid)
            {

             this.uid = uid;
             this.name = name;
             this.label = label;
             this.avatarTexName = avatarTexName;
             this.paramDic = paramDic;
             this.protoUid = protoUid;
             this.animDic = animDic;
             this.defaultAnimName = defaultAnimName;
             this.faceType = faceType;
             this.speedParamName = speedParamName;
             this.hpParamName = hpParamName;
             this.events = events;
             this.equips = equips;
             this.desc = desc;
             this.illustration = illustration;
             this.unique = unique;
             this.skill = skill;
             this.recoveryTime = recoveryTime;
             this.enableNav = enableNav;
             this.minimapIcon = minimapIcon;

            }
            public void Reset(Data data)
            {

             this.uid = data.uid;
             this.name = data.name;
             this.label = data.label;
             this.avatarTexName = data.avatarTexName;
             this.paramDic = data.paramDic;
             this.protoUid = data.protoUid;
             this.animDic = data.animDic;
             this.defaultAnimName = data.defaultAnimName;
             this.faceType = data.faceType;
             this.speedParamName = data.speedParamName;
             this.hpParamName = data.hpParamName;
             this.events = data.events;
             this.equips = data.equips;
             this.desc = data.desc;
             this.illustration = data.illustration;
             this.unique = data.unique;
             this.skill = data.skill;
             this.recoveryTime = data.recoveryTime;
             this.enableNav = data.enableNav;
             this.minimapIcon = data.minimapIcon;
            }

                public Data Copy(bool sameId = true)
                {
        return new Data(sameId? uid:uidChain.GetId(),name,label,avatarTexName,paramDic==null?new Dictionary<string,CharacterParamForm.Data>():new Dictionary<string,CharacterParamForm.Data>(paramDic),protoUid,animDic==null?new Dictionary<string,CharacterAnimForm.Data>():new Dictionary<string,CharacterAnimForm.Data>(animDic),defaultAnimName==null?new Dictionary<string,string>():new Dictionary<string,string>(defaultAnimName),faceType,speedParamName,hpParamName,events==null?new Dictionary<string,EventTriggerForm.Data>():new Dictionary<string,EventTriggerForm.Data>(events),equips==null?new Dictionary<EquipPartType,int>():new Dictionary<EquipPartType,int>(equips),desc,illustration,unique,skill==null?new Dictionary<SkillType,int>():new Dictionary<SkillType,int>(skill),recoveryTime,enableNav,minimapIcon);
                }
            
            public override  void BeforeGet()
            {
                base.BeforeGet();
                CharacterProductForm.beforeGetAction?.Invoke(this);
            }
        }

                   private static Data _defaultData=new Data(0,"","","",new Dictionary<string,CharacterParamForm.Data>(){},0,new Dictionary<string,CharacterAnimForm.Data>(){},new Dictionary<string,string>(){},FaceType.Fixed,"","",new Dictionary<string,EventTriggerForm.Data>(){},new Dictionary<EquipPartType,int>(){},"","",false,new Dictionary<SkillType,int>(){},0f,false,"");
                   public static Data defaultData=>_defaultData.Copy();


            static HashSet<Data> _DatasHashSet;
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
                _DatasHashSet=new HashSet<Data>();
                
                    _DataByNameProtouid = new Dictionary<(string,int), Data>() {
    
                    
                    };
                    foreach(var v in _DataByUid.Values)
                    {
                        _DatasHashSet.Add(v);
                    }
    
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

                jo.SelectToken("avatarTexName")==null?defaultData.avatarTexName:jo.Get<string>("avatarTexName"),

                jo.SelectToken("paramDic")==null?defaultData.paramDic:jo.Get<Dictionary<string,CharacterParamForm.Data>>("paramDic"),

                jo.SelectToken("protoUid")==null?defaultData.protoUid:jo.Get<int>("protoUid"),

                jo.SelectToken("animDic")==null?defaultData.animDic:jo.Get<Dictionary<string,CharacterAnimForm.Data>>("animDic"),

                jo.SelectToken("defaultAnimName")==null?defaultData.defaultAnimName:jo.Get<Dictionary<string,string>>("defaultAnimName"),

                jo.SelectToken("faceType")==null?defaultData.faceType:jo.Get<FaceType>("faceType"),

                jo.SelectToken("speedParamName")==null?defaultData.speedParamName:jo.Get<string>("speedParamName"),

                jo.SelectToken("hpParamName")==null?defaultData.hpParamName:jo.Get<string>("hpParamName"),

                jo.SelectToken("events")==null?defaultData.events:jo.Get<Dictionary<string,EventTriggerForm.Data>>("events"),

                jo.SelectToken("equips")==null?defaultData.equips:jo.Get<Dictionary<EquipPartType,int>>("equips"),

                jo.SelectToken("desc")==null?defaultData.desc:jo.Get<string>("desc"),

                jo.SelectToken("illustration")==null?defaultData.illustration:jo.Get<string>("illustration"),

                jo.SelectToken("unique")==null?defaultData.unique:jo.Get<bool>("unique"),

                jo.SelectToken("skill")==null?defaultData.skill:jo.Get<Dictionary<SkillType,int>>("skill"),

                jo.SelectToken("recoveryTime")==null?defaultData.recoveryTime:jo.Get<float>("recoveryTime"),

                jo.SelectToken("enableNav")==null?defaultData.enableNav:jo.Get<bool>("enableNav"),

                jo.SelectToken("minimapIcon")==null?defaultData.minimapIcon:jo.Get<string>("minimapIcon")
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

            jo.Set<string>("avatarTexName",data.avatarTexName);

            jo.Set<Dictionary<string,CharacterParamForm.Data>>("paramDic",data.paramDic);

            jo.Set<int>("protoUid",data.protoUid);

            jo.Set<Dictionary<string,CharacterAnimForm.Data>>("animDic",data.animDic);

            jo.Set<Dictionary<string,string>>("defaultAnimName",data.defaultAnimName);

            jo.Set<FaceType>("faceType",data.faceType);

            jo.Set<string>("speedParamName",data.speedParamName);

            jo.Set<string>("hpParamName",data.hpParamName);

            jo.Set<Dictionary<string,EventTriggerForm.Data>>("events",data.events);

            jo.Set<Dictionary<EquipPartType,int>>("equips",data.equips);

            jo.Set<string>("desc",data.desc);

            jo.Set<string>("illustration",data.illustration);

            jo.Set<bool>("unique",data.unique);

            jo.Set<Dictionary<SkillType,int>>("skill",data.skill);

            jo.Set<float>("recoveryTime",data.recoveryTime);

            jo.Set<bool>("enableNav",data.enableNav);

            jo.Set<string>("minimapIcon",data.minimapIcon);

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
        _DatasHashSet.Add(data);
    
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

                    _DatasHashSet.Remove(DataByUid[data.uid]);
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
                    {
                        RemoveData(key);
                    }
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
            
            public static void ChangeAvatartexname(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeAvatartexnameAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeParamdic(Data superData,Dictionary<string,CharacterParamForm.Data> oldV,Dictionary<string,CharacterParamForm.Data> newV)
            {
                if(superData is Data data)
                {

                changeParamdicAction?.Invoke(data,oldV,newV);
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
            
            public static void ChangeAnimdic(Data superData,Dictionary<string,CharacterAnimForm.Data> oldV,Dictionary<string,CharacterAnimForm.Data> newV)
            {
                if(superData is Data data)
                {

                changeAnimdicAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeDefaultanimname(Data superData,Dictionary<string,string> oldV,Dictionary<string,string> newV)
            {
                if(superData is Data data)
                {

                changeDefaultanimnameAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeFacetype(Data superData,FaceType oldV,FaceType newV)
            {
                if(superData is Data data)
                {

                changeFacetypeAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeSpeedparamname(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeSpeedparamnameAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeHpparamname(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeHpparamnameAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeEvents(Data superData,Dictionary<string,EventTriggerForm.Data> oldV,Dictionary<string,EventTriggerForm.Data> newV)
            {
                if(superData is Data data)
                {

                changeEventsAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeEquips(Data superData,Dictionary<EquipPartType,int> oldV,Dictionary<EquipPartType,int> newV)
            {
                if(superData is Data data)
                {

                changeEquipsAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeDesc(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeDescAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeIllustration(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeIllustrationAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeUnique(Data superData,bool oldV,bool newV)
            {
                if(superData is Data data)
                {

                changeUniqueAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeSkill(Data superData,Dictionary<SkillType,int> oldV,Dictionary<SkillType,int> newV)
            {
                if(superData is Data data)
                {

                changeSkillAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeRecoverytime(Data superData,float oldV,float newV)
            {
                if(superData is Data data)
                {

                changeRecoverytimeAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeEnablenav(Data superData,bool oldV,bool newV)
            {
                if(superData is Data data)
                {

                changeEnablenavAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeMinimapicon(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeMinimapiconAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
    }
}
        