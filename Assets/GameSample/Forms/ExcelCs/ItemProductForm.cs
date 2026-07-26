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

    public static partial class ItemProductForm
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
                
        public static Action<Data,int,int> changeIcontexnameAction;
                
        public static Action<Data,Dictionary<string,ItemParamForm.Data>,Dictionary<string,ItemParamForm.Data>> changeParamdicAction;
                
        public static Action<Data,int,int> changeProtouidAction;
                
        public static Action<Data,MapModelForm.Data,MapModelForm.Data> changeModelAction;
                
        public static Action<Data,string,string> changeDescAction;
                
        public static Action<Data,int,int> changeAmountAction;
                
        public static Action<Data,int,int> changeMaxamountperAction;
                
        public static Action<Data,EquipPartType,EquipPartType> changeEquipAction;
                
        public static Action<Data,Dictionary<ItemStyle,int>,Dictionary<ItemStyle,int>> changeStyletexAction;
                
        public static Action<Data,int,int> changePriceAction;
                
        public static Action<Data,bool,bool> changeCanequipeAction;
                
        public static Action<Data,Dictionary<string,EventTriggerForm.Data>,Dictionary<string,EventTriggerForm.Data>> changeEventsAction;
                
        public static Action<Data,bool,bool> changeIsconsumeAction;
                
        public static Action<Data,Dictionary<string,CharacterParamForm.Data>,Dictionary<string,CharacterParamForm.Data>> changeParamdiccharacterAction;
                
        public static Action<Data,int,int> changeMinimapiconAction;
                


        public partial class Data : ProductForm.Data
        {

                    private int  _iconTexName;
                    /// <summary>
                    ///图标名称
                    ///</summary>
                    public int  iconTexName{
                                get{return _iconTexName;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeIcontexname(this,_iconTexName,value); 
                    }
        
                _iconTexName = value;
                }
                 
                     }
                    
                    private Dictionary<string,ItemParamForm.Data>  _paramDic;
                    /// <summary>
                    ///数据
                    ///</summary>
                    public Dictionary<string,ItemParamForm.Data>  paramDic{
                                get{return _paramDic;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeParamdic(this,_paramDic,value); 
                    }
        
                _paramDic = value;
                }
                 
                     }
                    
                    private MapModelForm.Data  _model;
                    /// <summary>
                    ///模型
                    ///</summary>
                    public MapModelForm.Data  model{
                                get{return _model;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeModel(this,_model,value); 
                    }
        
                _model = value;
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
                    
                    private int  _amount;
                    /// <summary>
                    ///数量
                    ///</summary>
                    public int  amount{
                                get{return _amount;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeAmount(this,_amount,value); 
                    }
        
                _amount = value;
                }
                 
                     }
                    
                    private int  _maxAmountPer;
                    /// <summary>
                    ///单体最大数量
                    ///</summary>
                    public int  maxAmountPer{
                                get{return _maxAmountPer;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeMaxamountper(this,_maxAmountPer,value); 
                    }
        
                _maxAmountPer = value;
                }
                 
                     }
                    
                    private EquipPartType  _equip;
                    /// <summary>
                    ///装备位置
                    ///</summary>
                    public EquipPartType  equip{
                                get{return _equip;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeEquip(this,_equip,value); 
                    }
        
                _equip = value;
                }
                 
                     }
                    
                    private Dictionary<ItemStyle,int>  _styleTex;
                    /// <summary>
                    ///装备位置
                    ///</summary>
                    public Dictionary<ItemStyle,int>  styleTex{
                                get{return _styleTex;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeStyletex(this,_styleTex,value); 
                    }
        
                _styleTex = value;
                }
                 
                     }
                    
                    private int  _price;
                    /// <summary>
                    ///价格
                    ///</summary>
                    public int  price{
                                get{return _price;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangePrice(this,_price,value); 
                    }
        
                _price = value;
                }
                 
                     }
                    
                    private bool  _canEquipe;
                    /// <summary>
                    ///可装备
                    ///</summary>
                    public bool  canEquipe{
                                get{return _canEquipe;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeCanequipe(this,_canEquipe,value); 
                    }
        
                _canEquipe = value;
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
                    
                    private bool  _isConsume;
                    /// <summary>
                    ///是消费品
                    ///</summary>
                    public bool  isConsume{
                                get{return _isConsume;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeIsconsume(this,_isConsume,value); 
                    }
        
                _isConsume = value;
                }
                 
                     }
                    
                    private Dictionary<string,CharacterParamForm.Data>  _paramDicCharacter;
                    /// <summary>
                    ///人物加成数据
                    ///</summary>
                    public Dictionary<string,CharacterParamForm.Data>  paramDicCharacter{
                                get{return _paramDicCharacter;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeParamdiccharacter(this,_paramDicCharacter,value); 
                    }
        
                _paramDicCharacter = value;
                }
                 
                     }
                    
                    private int  _minimapIcon;
                    /// <summary>
                    ///小地图标识
                    ///</summary>
                    public int  minimapIcon{
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
            
            public Data(int uid,string name,string label,int iconTexName,Dictionary<string,ItemParamForm.Data> paramDic,int protoUid,MapModelForm.Data model,string desc,int amount,int maxAmountPer,EquipPartType equip,Dictionary<ItemStyle,int> styleTex,int price,bool canEquipe,Dictionary<string,EventTriggerForm.Data> events,bool isConsume,Dictionary<string,CharacterParamForm.Data> paramDicCharacter,int minimapIcon):base(uid,name,label,protoUid)
            {

             this.uid = uid;
             this.name = name;
             this.label = label;
             this.iconTexName = iconTexName;
             this.paramDic = paramDic;
             this.protoUid = protoUid;
             this.model = model;
             this.desc = desc;
             this.amount = amount;
             this.maxAmountPer = maxAmountPer;
             this.equip = equip;
             this.styleTex = styleTex;
             this.price = price;
             this.canEquipe = canEquipe;
             this.events = events;
             this.isConsume = isConsume;
             this.paramDicCharacter = paramDicCharacter;
             this.minimapIcon = minimapIcon;

            }
            public void Reset(Data data)
            {

             this.uid = data.uid;
             this.name = data.name;
             this.label = data.label;
             this.iconTexName = data.iconTexName;
             this.paramDic = data.paramDic;
             this.protoUid = data.protoUid;
             this.model = data.model;
             this.desc = data.desc;
             this.amount = data.amount;
             this.maxAmountPer = data.maxAmountPer;
             this.equip = data.equip;
             this.styleTex = data.styleTex;
             this.price = data.price;
             this.canEquipe = data.canEquipe;
             this.events = data.events;
             this.isConsume = data.isConsume;
             this.paramDicCharacter = data.paramDicCharacter;
             this.minimapIcon = data.minimapIcon;
            }

                public Data Copy(bool sameId = true)
                {
        return new Data(sameId? uid:uidChain.GetId(),name,label,iconTexName,paramDic==null?new Dictionary<string,ItemParamForm.Data>():new Dictionary<string,ItemParamForm.Data>(paramDic),protoUid,model,desc,amount,maxAmountPer,equip,styleTex==null?new Dictionary<ItemStyle,int>():new Dictionary<ItemStyle,int>(styleTex),price,canEquipe,events==null?new Dictionary<string,EventTriggerForm.Data>():new Dictionary<string,EventTriggerForm.Data>(events),isConsume,paramDicCharacter==null?new Dictionary<string,CharacterParamForm.Data>():new Dictionary<string,CharacterParamForm.Data>(paramDicCharacter),minimapIcon);
                }
            
            public override  void BeforeGet()
            {
                base.BeforeGet();
                ItemProductForm.beforeGetAction?.Invoke(this);
            }
        }

                   private static Data _defaultData=new Data(0,"","",0,new Dictionary<string,ItemParamForm.Data>(){},0,MapModelForm.defaultData,"",1,1,default,new Dictionary<ItemStyle,int>(){},0,false,new Dictionary<string,EventTriggerForm.Data>(){},false,new Dictionary<string,CharacterParamForm.Data>(){},0);
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

                jo.SelectToken("iconTexName")==null?defaultData.iconTexName:jo.Get<int>("iconTexName"),

                jo.SelectToken("paramDic")==null?defaultData.paramDic:jo.Get<Dictionary<string,ItemParamForm.Data>>("paramDic"),

                jo.SelectToken("protoUid")==null?defaultData.protoUid:jo.Get<int>("protoUid"),

                jo.SelectToken("model")==null?defaultData.model:jo.Get<MapModelForm.Data>("model"),

                jo.SelectToken("desc")==null?defaultData.desc:jo.Get<string>("desc"),

                jo.SelectToken("amount")==null?defaultData.amount:jo.Get<int>("amount"),

                jo.SelectToken("maxAmountPer")==null?defaultData.maxAmountPer:jo.Get<int>("maxAmountPer"),

                jo.SelectToken("equip")==null?defaultData.equip:jo.Get<EquipPartType>("equip"),

                jo.SelectToken("styleTex")==null?defaultData.styleTex:jo.Get<Dictionary<ItemStyle,int>>("styleTex"),

                jo.SelectToken("price")==null?defaultData.price:jo.Get<int>("price"),

                jo.SelectToken("canEquipe")==null?defaultData.canEquipe:jo.Get<bool>("canEquipe"),

                jo.SelectToken("events")==null?defaultData.events:jo.Get<Dictionary<string,EventTriggerForm.Data>>("events"),

                jo.SelectToken("isConsume")==null?defaultData.isConsume:jo.Get<bool>("isConsume"),

                jo.SelectToken("paramDicCharacter")==null?defaultData.paramDicCharacter:jo.Get<Dictionary<string,CharacterParamForm.Data>>("paramDicCharacter"),

                jo.SelectToken("minimapIcon")==null?defaultData.minimapIcon:jo.Get<int>("minimapIcon")
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

            jo.Set<int>("iconTexName",data.iconTexName);

            jo.Set<Dictionary<string,ItemParamForm.Data>>("paramDic",data.paramDic);

            jo.Set<int>("protoUid",data.protoUid);

            jo.Set<MapModelForm.Data>("model",data.model);

            jo.Set<string>("desc",data.desc);

            jo.Set<int>("amount",data.amount);

            jo.Set<int>("maxAmountPer",data.maxAmountPer);

            jo.Set<EquipPartType>("equip",data.equip);

            jo.Set<Dictionary<ItemStyle,int>>("styleTex",data.styleTex);

            jo.Set<int>("price",data.price);

            jo.Set<bool>("canEquipe",data.canEquipe);

            jo.Set<Dictionary<string,EventTriggerForm.Data>>("events",data.events);

            jo.Set<bool>("isConsume",data.isConsume);

            jo.Set<Dictionary<string,CharacterParamForm.Data>>("paramDicCharacter",data.paramDicCharacter);

            jo.Set<int>("minimapIcon",data.minimapIcon);

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
    
                    if(DatasByLabelProtouid.ContainsKey((data.label,data.protoUid)))
                    {
                        DatasByLabelProtouid[(data.label,data.protoUid)].Remove(data);
                        if(DatasByLabelProtouid[(data.label,data.protoUid)].Count==0)
                            DatasByLabelProtouid.Remove((data.label,data.protoUid));
                    }
                    
    
                    if(DatasByLabel.ContainsKey(data.label))
                    {
                        DatasByLabel[data.label].Remove(data);
                        if(DatasByLabel[data.label].Count==0)
                            DatasByLabel.Remove(data.label);
                    }
                    
    
                    if(DatasByProtouid.ContainsKey(data.protoUid))
                    {
                        DatasByProtouid[data.protoUid].Remove(data);
                        if(DatasByProtouid[data.protoUid].Count==0)
                            DatasByProtouid.Remove(data.protoUid);
                    }
                    
    
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
            
            public static void ChangeIcontexname(Data superData,int oldV,int newV)
            {
                if(superData is Data data)
                {

                changeIcontexnameAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeParamdic(Data superData,Dictionary<string,ItemParamForm.Data> oldV,Dictionary<string,ItemParamForm.Data> newV)
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
            
            public static void ChangeModel(Data superData,MapModelForm.Data oldV,MapModelForm.Data newV)
            {
                if(superData is Data data)
                {

                changeModelAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeDesc(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeDescAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeAmount(Data superData,int oldV,int newV)
            {
                if(superData is Data data)
                {

                changeAmountAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeMaxamountper(Data superData,int oldV,int newV)
            {
                if(superData is Data data)
                {

                changeMaxamountperAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeEquip(Data superData,EquipPartType oldV,EquipPartType newV)
            {
                if(superData is Data data)
                {

                changeEquipAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeStyletex(Data superData,Dictionary<ItemStyle,int> oldV,Dictionary<ItemStyle,int> newV)
            {
                if(superData is Data data)
                {

                changeStyletexAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangePrice(Data superData,int oldV,int newV)
            {
                if(superData is Data data)
                {

                changePriceAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeCanequipe(Data superData,bool oldV,bool newV)
            {
                if(superData is Data data)
                {

                changeCanequipeAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeEvents(Data superData,Dictionary<string,EventTriggerForm.Data> oldV,Dictionary<string,EventTriggerForm.Data> newV)
            {
                if(superData is Data data)
                {

                changeEventsAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeIsconsume(Data superData,bool oldV,bool newV)
            {
                if(superData is Data data)
                {

                changeIsconsumeAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeParamdiccharacter(Data superData,Dictionary<string,CharacterParamForm.Data> oldV,Dictionary<string,CharacterParamForm.Data> newV)
            {
                if(superData is Data data)
                {

                changeParamdiccharacterAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeMinimapicon(Data superData,int oldV,int newV)
            {
                if(superData is Data data)
                {

                changeMinimapiconAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
    }
}
        