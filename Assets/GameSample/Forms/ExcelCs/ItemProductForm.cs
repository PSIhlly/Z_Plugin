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

        public static Action<Data> addAction;
        public static Action<Data> removeAction;
        public static Action childInitAction;
        public static Action<Data> childRemoveAction;
        public static Action<Data> childAddAction;

        public static Action<Data,int,int> changeUidAction;
                
        public static Action<Data,string,string> changeNameAction;
                
        public static Action<Data,string,string> changeLabelAction;
                
        public static Action<Data,string,string> changeIcontexnameAction;
                
        public static Action<Data,Dictionary<string,ItemParamForm.Data>,Dictionary<string,ItemParamForm.Data>> changeParamdicAction;
                
        public static Action<Data,bool,bool> changeIsprotoAction;
                
        public static Action<Data,MapModelForm.Data,MapModelForm.Data> changeModelAction;
                
        public static Action<Data,string,string> changeDescAction;
                
        public static Action<Data,int,int> changeAmountAction;
                
        public static Action<Data,int,int> changeMaxamountperAction;
                
        public static Action<Data,EquipPartType,EquipPartType> changeEquipAction;
                
        public static Action<Data,Dictionary<ItemStyle,string>,Dictionary<ItemStyle,string>> changeStyletexAction;
                
        public static Action<Data,int,int> changePriceAction;
                
        public static Action<Data,bool,bool> changeCanequipeAction;
                
        public static Action<Data,Dictionary<string,EventTriggerForm.Data>,Dictionary<string,EventTriggerForm.Data>> changeEventsAction;
                


        public partial class Data : ProductForm.Data
        {

                    private string  _iconTexName;
                    /// <summary>
                    ///图标名称
                    ///</summary>
                    public string  iconTexName{
                                get{return _iconTexName;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
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

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
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

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
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

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
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

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
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

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
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

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeEquip(this,_equip,value); 
                    }
        
                _equip = value;
                }
                 
                     }
                    
                    private Dictionary<ItemStyle,string>  _styleTex;
                    /// <summary>
                    ///装备位置
                    ///</summary>
                    public Dictionary<ItemStyle,string>  styleTex{
                                get{return _styleTex;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
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

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
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

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
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

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeEvents(this,_events,value); 
                    }
        
                _events = value;
                }
                 
                     }
                    
            public Data(ProductForm.Data data):base(data.uid,data.name,data.label,data.isProto)
            {
            }
            
            public Data(int uid,string name,string label,string iconTexName,Dictionary<string,ItemParamForm.Data> paramDic,bool isProto,MapModelForm.Data model,string desc,int amount,int maxAmountPer,EquipPartType equip,Dictionary<ItemStyle,string> styleTex,int price,bool canEquipe,Dictionary<string,EventTriggerForm.Data> events):base(uid,name,label,isProto)
            {

             this.uid = uid;
             this.name = name;
             this.label = label;
             this.iconTexName = iconTexName;
             this.paramDic = paramDic;
             this.isProto = isProto;
             this.model = model;
             this.desc = desc;
             this.amount = amount;
             this.maxAmountPer = maxAmountPer;
             this.equip = equip;
             this.styleTex = styleTex;
             this.price = price;
             this.canEquipe = canEquipe;
             this.events = events;

            }
            public void Reset(Data data)
            {

             this.uid = data.uid;
             this.name = data.name;
             this.label = data.label;
             this.iconTexName = data.iconTexName;
             this.paramDic = data.paramDic;
             this.isProto = data.isProto;
             this.model = data.model;
             this.desc = data.desc;
             this.amount = data.amount;
             this.maxAmountPer = data.maxAmountPer;
             this.equip = data.equip;
             this.styleTex = data.styleTex;
             this.price = data.price;
             this.canEquipe = data.canEquipe;
             this.events = data.events;
            }

                public Data Copy(bool sameId = true)
                {
        return new Data(sameId? uid:uidChain.GetId(),name,label,iconTexName,new Dictionary<string,ItemParamForm.Data>(paramDic),isProto,model,desc,amount,maxAmountPer,equip,new Dictionary<ItemStyle,string>(styleTex),price,canEquipe,new Dictionary<string,EventTriggerForm.Data>(events));
                }
            
        }

                   private static Data _defaultData=new Data(0,"","","",new Dictionary<string,ItemParamForm.Data>(){},false,MapModelForm.defaultData,"",1,1,default,new Dictionary<ItemStyle,string>(){},0,false,new Dictionary<string,EventTriggerForm.Data>(){});
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

                jo.Get<string>("iconTexName"),

                jo.Get<Dictionary<string,ItemParamForm.Data>>("paramDic"),

                jo.Get<bool>("isProto"),

                jo.Get<MapModelForm.Data>("model"),

                jo.Get<string>("desc"),

                jo.Get<int>("amount"),

                jo.Get<int>("maxAmountPer"),

                jo.Get<EquipPartType>("equip"),

                jo.Get<Dictionary<ItemStyle,string>>("styleTex"),

                jo.Get<int>("price"),

                jo.Get<bool>("canEquipe"),

                jo.Get<Dictionary<string,EventTriggerForm.Data>>("events")
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

            jo.Set<string>("iconTexName",data.iconTexName);

            jo.Set<Dictionary<string,ItemParamForm.Data>>("paramDic",data.paramDic);

            jo.Set<bool>("isProto",data.isProto);

            jo.Set<MapModelForm.Data>("model",data.model);

            jo.Set<string>("desc",data.desc);

            jo.Set<int>("amount",data.amount);

            jo.Set<int>("maxAmountPer",data.maxAmountPer);

            jo.Set<EquipPartType>("equip",data.equip);

            jo.Set<Dictionary<ItemStyle,string>>("styleTex",data.styleTex);

            jo.Set<int>("price",data.price);

            jo.Set<bool>("canEquipe",data.canEquipe);

            jo.Set<Dictionary<string,EventTriggerForm.Data>>("events",data.events);

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
            
            public static void ChangeIcontexname(Data superData,string oldV,string newV)
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
            
            public static void ChangeStyletex(Data superData,Dictionary<ItemStyle,string> oldV,Dictionary<ItemStyle,string> newV)
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
            
    }
}
        