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

            ProductForm.changeIsprotoAction+=ChangeIsproto;

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

        public static Action childInitAction;
        public static Action<Data> childRemoveAction;
        public static Action<Data> childAddAction;

        public static Action<Data,int,int> changeUidAction;
                
        public static Action<Data,string,string> changeNameAction;
                
        public static Action<Data,string,string> changeLabelAction;
                
        public static Action<Data,string,string> changeAvatartexnameAction;
                
        public static Action<Data,Dictionary<string,CharacterParamForm.Data>,Dictionary<string,CharacterParamForm.Data>> changeParamdicAction;
                
        public static Action<Data,bool,bool> changeIsprotoAction;
                
        public static Action<Data,Dictionary<string,CharacterAnimForm.Data>,Dictionary<string,CharacterAnimForm.Data>> changeAnimdicAction;
                
        public static Action<Data,string,string> changeIdleanimnameAction;
                
        public static Action<Data,string,string> changeMoveanimnameAction;
                
        public static Action<Data,string,string> changeSpeedparamnameAction;
                
        public static Action<Data,string,string> changeHpparamnameAction;
                
        public static Action<Data,Dictionary<string,EventTriggerForm.Data>,Dictionary<string,EventTriggerForm.Data>> changeEventsAction;
                
        public static Action<Data,Dictionary<EquipPartType,int>,Dictionary<EquipPartType,int>> changeEquipsAction;
                
        public static Action<Data,string,string> changeDescAction;
                
        public static Action<Data,string,string> changeTachieAction;
                


        public partial class Data : ProductForm.Data
        {

                    private string  _avatarTexName;
                    /// <summary>
                    ///头像名称
                    ///</summary>
                    public string  avatarTexName{
                                get{return _avatarTexName;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
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

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
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

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeAnimdic(this,_animDic,value); 
                    }
        
                _animDic = value;
                }
                 
                     }
                    
                    private string  _idleAnimName;
                    /// <summary>
                    ///闲置动画名
                    ///</summary>
                    public string  idleAnimName{
                                get{return _idleAnimName;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeIdleanimname(this,_idleAnimName,value); 
                    }
        
                _idleAnimName = value;
                }
                 
                     }
                    
                    private string  _moveAnimName;
                    /// <summary>
                    ///移动动画名
                    ///</summary>
                    public string  moveAnimName{
                                get{return _moveAnimName;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeMoveanimname(this,_moveAnimName,value); 
                    }
        
                _moveAnimName = value;
                }
                 
                     }
                    
                    private string  _speedParamName;
                    /// <summary>
                    ///速度参数名
                    ///</summary>
                    public string  speedParamName{
                                get{return _speedParamName;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
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

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
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

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
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

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
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

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeDesc(this,_desc,value); 
                    }
        
                _desc = value;
                }
                 
                     }
                    
                    private string  _tachie;
                    /// <summary>
                    ///立绘
                    ///</summary>
                    public string  tachie{
                                get{return _tachie;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeTachie(this,_tachie,value); 
                    }
        
                _tachie = value;
                }
                 
                     }
                    
            public Data(int uid,string name,string label,string avatarTexName,Dictionary<string,CharacterParamForm.Data> paramDic,bool isProto,Dictionary<string,CharacterAnimForm.Data> animDic,string idleAnimName,string moveAnimName,string speedParamName,string hpParamName,Dictionary<string,EventTriggerForm.Data> events,Dictionary<EquipPartType,int> equips,string desc,string tachie):base(uid,name,label,isProto)
            {

             this.uid = uid;
             this.name = name;
             this.label = label;
             this.avatarTexName = avatarTexName;
             this.paramDic = paramDic;
             this.isProto = isProto;
             this.animDic = animDic;
             this.idleAnimName = idleAnimName;
             this.moveAnimName = moveAnimName;
             this.speedParamName = speedParamName;
             this.hpParamName = hpParamName;
             this.events = events;
             this.equips = equips;
             this.desc = desc;
             this.tachie = tachie;

            }

                public Data Copy(bool sameId = true)
                {
        return new Data(sameId? uid:uidChain.GetId(),name,label,avatarTexName,new Dictionary<string,CharacterParamForm.Data>(paramDic),isProto,new Dictionary<string,CharacterAnimForm.Data>(animDic),idleAnimName,moveAnimName,speedParamName,hpParamName,new Dictionary<string,EventTriggerForm.Data>(events),new Dictionary<EquipPartType,int>(equips),desc,tachie);
                }
            
        }

                   private static Data _defaultData=new Data(0,"","","",new Dictionary<string,CharacterParamForm.Data>(){},false,null,"","","","",new Dictionary<string,EventTriggerForm.Data>(){},new Dictionary<EquipPartType,int>(){},"","");
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
    
            static Dictionary<(string,bool), List<Data>> _DatasByLabelIsproto;
            public static Dictionary<(string,bool), List<Data>> DatasByLabelIsproto
            {
                get
                {
                    Init();
                    return _DatasByLabelIsproto;
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
    
            static Dictionary<bool, List<Data>> _DatasByIsproto;
            public static Dictionary<bool, List<Data>> DatasByIsproto
            {
                get
                {
                    Init();
                    return _DatasByIsproto;
                }
            }
    
            static Dictionary<(string,bool), Data> _DataByNameIsproto;
            public static Dictionary<(string,bool), Data> DataByNameIsproto
            {
                get
                {
                    Init();
                    return _DataByNameIsproto;
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
                    _DataByNameIsproto = new Dictionary<(string,bool), Data>() {
    
                    };
    
                    _DatasByLabelIsproto = new Dictionary<(string,bool), List<Data>>() {
    
                };

                    _DatasByLabel = new Dictionary<string, List<Data>>() {
    
                };

                    _DatasByIsproto = new Dictionary<bool, List<Data>>() {
    
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

                jo.Get<int>("uid"),

                jo.Get<string>("name"),

                jo.Get<string>("label"),

                jo.Get<string>("avatarTexName"),

                jo.Get<Dictionary<string,CharacterParamForm.Data>>("paramDic"),

                jo.Get<bool>("isProto"),

                jo.Get<Dictionary<string,CharacterAnimForm.Data>>("animDic"),

                jo.Get<string>("idleAnimName"),

                jo.Get<string>("moveAnimName"),

                jo.Get<string>("speedParamName"),

                jo.Get<string>("hpParamName"),

                jo.Get<Dictionary<string,EventTriggerForm.Data>>("events"),

                jo.Get<Dictionary<EquipPartType,int>>("equips"),

                jo.Get<string>("desc"),

                jo.Get<string>("tachie")
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

            jo.Set<string>("avatarTexName",data.avatarTexName);

            jo.Set<Dictionary<string,CharacterParamForm.Data>>("paramDic",data.paramDic);

            jo.Set<bool>("isProto",data.isProto);

            jo.Set<Dictionary<string,CharacterAnimForm.Data>>("animDic",data.animDic);

            jo.Set<string>("idleAnimName",data.idleAnimName);

            jo.Set<string>("moveAnimName",data.moveAnimName);

            jo.Set<string>("speedParamName",data.speedParamName);

            jo.Set<string>("hpParamName",data.hpParamName);

            jo.Set<Dictionary<string,EventTriggerForm.Data>>("events",data.events);

            jo.Set<Dictionary<EquipPartType,int>>("equips",data.equips);

            jo.Set<string>("desc",data.desc);

            jo.Set<string>("tachie",data.tachie);

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
    
                    DataByNameIsproto[(data.name,data.isProto)]=data;
    
                    if(!DatasByLabelIsproto.ContainsKey((data.label,data.isProto)))
                        DatasByLabelIsproto[(data.label,data.isProto)]=new List<Data>();
                    DatasByLabelIsproto[(data.label,data.isProto)].Add(data);
    
                    if(!DatasByLabel.ContainsKey(data.label))
                        DatasByLabel[data.label]=new List<Data>();
                    DatasByLabel[data.label].Add(data);
    
                    if(!DatasByIsproto.ContainsKey(data.isProto))
                        DatasByIsproto[data.isProto]=new List<Data>();
                    DatasByIsproto[data.isProto].Add(data);
    
ProductForm.AddData(data);
            childAddAction?.Invoke(data);
            return data.uid;
        }
        public static void RemoveData(int uid)
        {            
            Init();
            if(!DataByUid.ContainsKey(uid))
                return;
               
            var data=DataByUid[uid];

                    DataByUid.Remove(data.uid);
    
                    DataByNameIsproto.Remove((data.name,data.isProto));
    
                    DatasByLabelIsproto[(data.label,data.isProto)].Remove(data);
                    if(DatasByLabelIsproto[(data.label,data.isProto)].Count==0)
                        DatasByLabelIsproto.Remove((data.label,data.isProto));
    
                    DatasByLabel[data.label].Remove(data);
                    if(DatasByLabel[data.label].Count==0)
                        DatasByLabel.Remove(data.label);
    
                    DatasByIsproto[data.isProto].Remove(data);
                    if(DatasByIsproto[data.isProto].Count==0)
                        DatasByIsproto.Remove(data.isProto);
    
ProductForm.RemoveData(uid);
            uidChain.PushId(data.uid);
            childRemoveAction?.Invoke(data);
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

                    DataByNameIsproto.Remove((oldV,data.isProto));
                    DataByNameIsproto[(newV,data.isProto)]=data;
 
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
 
                    DatasByLabelIsproto[(oldV,data.isProto)].Remove(data);
                    if(DatasByLabelIsproto[(oldV,data.isProto)].Count==0)
                        DatasByLabelIsproto.Remove((oldV,data.isProto));
                    if(!DatasByLabelIsproto.ContainsKey((newV,data.isProto)))
                        DatasByLabelIsproto[(newV,data.isProto)]=new List<Data>();
                    DatasByLabelIsproto[(newV,data.isProto)].Add(data);
 
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
            
            public static void ChangeIsproto(ProductForm.Data superData,bool oldV,bool newV)
            {
                if(superData is Data data)
                {

                    DatasByIsproto[oldV].Remove(data);
                    if(DatasByIsproto[oldV].Count==0)
                        DatasByIsproto.Remove(oldV);
                    if(!DatasByIsproto.ContainsKey(newV))
                        DatasByIsproto[newV]=new List<Data>();
                    DatasByIsproto[newV].Add(data);
 
                    DataByNameIsproto.Remove((data.name,oldV));
                    DataByNameIsproto[(data.name,newV)]=data;
 
                    DatasByLabelIsproto[(data.label,oldV)].Remove(data);
                    if(DatasByLabelIsproto[(data.label,oldV)].Count==0)
                        DatasByLabelIsproto.Remove((data.label,oldV));
                    if(!DatasByLabelIsproto.ContainsKey((data.label,newV)))
                        DatasByLabelIsproto[(data.label,newV)]=new List<Data>();
                    DatasByLabelIsproto[(data.label,newV)].Add(data);
 
                changeIsprotoAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeAnimdic(Data superData,Dictionary<string,CharacterAnimForm.Data> oldV,Dictionary<string,CharacterAnimForm.Data> newV)
            {
                if(superData is Data data)
                {

                changeAnimdicAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeIdleanimname(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeIdleanimnameAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeMoveanimname(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeMoveanimnameAction?.Invoke(data,oldV,newV);
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
            
            public static void ChangeTachie(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeTachieAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
    }
}
        