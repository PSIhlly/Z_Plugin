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

    public static partial class GameCmdDataForm
    {

        

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Register()
        {

                CmdDataForm.childInitAction+=InitInternal;


                CmdDataForm.childRemoveAction+=RemoveChildren;
                CmdDataForm.childAddAction+=AddChildren;
            

            CmdDataForm.changeUidAction+=ChangeUid;

            CmdDataForm.changeNameAction+=ChangeName;

            CmdDataForm.changePrmnamesAction+=ChangePrmnames;

            CmdDataForm.changePrmtypesAction+=ChangePrmtypes;

            CmdDataForm.changeRetnamesAction+=ChangeRetnames;

            CmdDataForm.changeRettypesAction+=ChangeRettypes;

            CmdDataForm.changeDescAction+=ChangeDesc;

            CmdDataForm.changeDefaultcodeAction+=ChangeDefaultcode;

            Z_Json.extra[typeof(Data)]=((obj)=>{
            if(obj is Data data)
                return GetJoByData(data);
            return null;
            },(jo)=>{
            return GetDataByJo(jo);
            });
        }
        
        private static bool inited;

        public static Z_Chain.Chain uidChain =>CmdDataForm.uidChain;

        public static Action<Data> addAction;
        public static Action<Data> removeAction;
        public static Action childInitAction;
        public static Action<Data> childRemoveAction;
        public static Action<Data> childAddAction;

        public static Action<Data,int,int> changeUidAction;
                
        public static Action<Data,string,string> changeNameAction;
                
        public static Action<Data,List<string>,List<string>> changePrmnamesAction;
                
        public static Action<Data,List<string>,List<string>> changePrmtypesAction;
                
        public static Action<Data,List<string>,List<string>> changeRetnamesAction;
                
        public static Action<Data,List<string>,List<string>> changeRettypesAction;
                
        public static Action<Data,string,string> changeDescAction;
                
        public static Action<Data,string,string> changeDefaultcodeAction;
                
        public static Action<Data,string,string> changeCategoryAction;
                
        public static Action<Data,string,string> changeTypeAction;
                


        public partial class Data : CmdDataForm.Data
        {

                    private string  _category;
                    /// <summary>
                    ///一级标签
                    ///</summary>
                    public string  category{
                                get{return _category;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeCategory(this,_category,value); 
                    }
        
                _category = value;
                }
                 
                     }
                    
                    private string  _type;
                    /// <summary>
                    ///二级标签
                    ///</summary>
                    public string  type{
                                get{return _type;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeType(this,_type,value); 
                    }
        
                _type = value;
                }
                 
                     }
                    
            public Data(CmdDataForm.Data data):base(data.uid,data.name,data.prmNames,data.prmTypes,data.retNames,data.retTypes,data.desc,data.defaultCode)
            {
            }
            
            public Data(int uid,string name,List<string> prmNames,List<string> prmTypes,List<string> retNames,List<string> retTypes,string desc,string defaultCode,string category,string type):base(uid,name,prmNames,prmTypes,retNames,retTypes,desc,defaultCode)
            {

             this.uid = uid;
             this.name = name;
             this.prmNames = prmNames;
             this.prmTypes = prmTypes;
             this.retNames = retNames;
             this.retTypes = retTypes;
             this.desc = desc;
             this.defaultCode = defaultCode;
             this.category = category;
             this.type = type;

            }

                public Data Copy(bool sameId = true)
                {
        return new Data(sameId? uid:uidChain.GetId(),name,new List<string>(prmNames),new List<string>(prmTypes),new List<string>(retNames),new List<string>(retTypes),desc,defaultCode,category,type);
                }
            
        }

                   private static Data _defaultData=new Data(0,"",null,null,null,null,"","","","");
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
    
            static Dictionary<(string,string), List<Data>> _DatasByCategoryType;
            public static Dictionary<(string,string), List<Data>> DatasByCategoryType
            {
                get
                {
                    Init();
                    return _DatasByCategoryType;
                }
            }
    
            static Dictionary<string, List<Data>> _DatasByCategory;
            public static Dictionary<string, List<Data>> DatasByCategory
            {
                get
                {
                    Init();
                    return _DatasByCategory;
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

            CmdDataForm.Init();

        }
        public static void InitInternal()
        {
            if(inited)
                return;
            inited=true;  

        

                _DataByUid = new Dictionary<int, Data>() {

                {100001,new Data(100001,"ShowTip",new List<string>(){"content",},new List<string>(){"string",},null,new List<string>(){"void",},"Show Tip: {0}","ShowTip(\"empty\");","ui","notice")},

                {100002,new Data(100002,"Text",null,null,null,new List<string>(){"string",},"","\"\"","basic","const")},

                {100003,new Data(100003,"Num",null,null,null,new List<string>(){"num",},"","1","basic","const")},

                {100004,new Data(100004,"Image",null,null,null,new List<string>(){"img",},"","\"$img$$img$\"","basic","comst")},

                {100005,new Data(100005,"ShowDialog",new List<string>(){"backGround","avatar","title","content",},new List<string>(){"img","img","string","string",},null,new List<string>(){"void",},"Show Dialog{1}{2}:{3} bg:{0}","ShowDialog(\"$img$$img$\",\"$img$$img$\",\"empty\",\"empty\");","ui","dialog")},

                {100006,new Data(100006,"ShowEffect",new List<string>(){"uid","positionX","positionY","positionZ","angle",},new List<string>(){"num","num","num","num",},null,new List<string>(){"void",},"Show Effect{0} position({1},{2},{3}) angle{4}","ShowDialog(0,0,0,0,0);","scene","effect")},

                };
                    _DataByName = new Dictionary<string, Data>() {
    
                        {"ShowTip",_DataByUid[100001]},
    
                        {"Text",_DataByUid[100002]},
    
                        {"Num",_DataByUid[100003]},
    
                        {"Image",_DataByUid[100004]},
    
                        {"ShowDialog",_DataByUid[100005]},
    
                        {"ShowEffect",_DataByUid[100006]},
    
                    };
    
                    _DatasByCategoryType = new Dictionary<(string,string), List<Data>>() {
    
                            {("ui","notice"),new List<Data>()},
        
                            {("basic","const"),new List<Data>()},
        
                            {("basic","comst"),new List<Data>()},
        
                            {("ui","dialog"),new List<Data>()},
        
                            {("scene","effect"),new List<Data>()},
        
                };

                    _DatasByCategoryType[("ui","notice")].Add(_DataByUid[100001]);

                    _DatasByCategoryType[("basic","const")].Add(_DataByUid[100002]);

                    _DatasByCategoryType[("basic","const")].Add(_DataByUid[100003]);

                    _DatasByCategoryType[("basic","comst")].Add(_DataByUid[100004]);

                    _DatasByCategoryType[("ui","dialog")].Add(_DataByUid[100005]);

                    _DatasByCategoryType[("scene","effect")].Add(_DataByUid[100006]);

                    _DatasByCategory = new Dictionary<string, List<Data>>() {
    
                            {"ui",new List<Data>()},
        
                            {"basic",new List<Data>()},
        
                            {"scene",new List<Data>()},
        
                };

                    _DatasByCategory["ui"].Add(_DataByUid[100001]);

                    _DatasByCategory["basic"].Add(_DataByUid[100002]);

                    _DatasByCategory["basic"].Add(_DataByUid[100003]);

                    _DatasByCategory["basic"].Add(_DataByUid[100004]);

                    _DatasByCategory["ui"].Add(_DataByUid[100005]);

                    _DatasByCategory["scene"].Add(_DataByUid[100006]);


            childInitAction?.Invoke();
            

            foreach(var data in DataByUid.Values)
            {
                CmdDataForm.AddData(data);
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

                jo.Get<List<string>>("prmNames"),

                jo.Get<List<string>>("prmTypes"),

                jo.Get<List<string>>("retNames"),

                jo.Get<List<string>>("retTypes"),

                jo.Get<string>("desc"),

                jo.Get<string>("defaultCode"),

                jo.Get<string>("category"),

                jo.Get<string>("type")
                    );

            return data;
        }

        public static JObject GetJoByData(Data data)
        {
            Init();

            JObject jo=new JObject();

            jo.Set<int>("uid",data.uid);

            jo.Set<string>("name",data.name);

            jo.Set<List<string>>("prmNames",data.prmNames);

            jo.Set<List<string>>("prmTypes",data.prmTypes);

            jo.Set<List<string>>("retNames",data.retNames);

            jo.Set<List<string>>("retTypes",data.retTypes);

            jo.Set<string>("desc",data.desc);

            jo.Set<string>("defaultCode",data.defaultCode);

            jo.Set<string>("category",data.category);

            jo.Set<string>("type",data.type);

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
    
                    if(!DatasByCategoryType.ContainsKey((data.category,data.type)))
                        DatasByCategoryType[(data.category,data.type)]=new List<Data>();
                    DatasByCategoryType[(data.category,data.type)].Add(data);
    
                    if(!DatasByCategory.ContainsKey(data.category))
                        DatasByCategory[data.category]=new List<Data>();
                    DatasByCategory[data.category].Add(data);
    
CmdDataForm.AddData(data);
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
    
                    DatasByCategoryType[(data.category,data.type)].Remove(data);
                    if(DatasByCategoryType[(data.category,data.type)].Count==0)
                        DatasByCategoryType.Remove((data.category,data.type));
    
                    DatasByCategory[data.category].Remove(data);
                    if(DatasByCategory[data.category].Count==0)
                        DatasByCategory.Remove(data.category);
    
CmdDataForm.RemoveData(uid);
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

         private static void RemoveChildren(CmdDataForm.Data data)
        {
            Init();
            if(data is Data)
               RemoveData(data.uid);      
        }
         private static void AddChildren(CmdDataForm.Data superData)
        {
            Init();
            if(superData is Data data)
               AddData(data);      
        }
        




            public static void ChangeUid(CmdDataForm.Data superData,int oldV,int newV)
            {
                if(superData is Data data)
                {

                changeUidAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeName(CmdDataForm.Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                    DataByName.Remove(oldV);
                    DataByName[newV]=data;
 
                changeNameAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangePrmnames(CmdDataForm.Data superData,List<string> oldV,List<string> newV)
            {
                if(superData is Data data)
                {

                changePrmnamesAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangePrmtypes(CmdDataForm.Data superData,List<string> oldV,List<string> newV)
            {
                if(superData is Data data)
                {

                changePrmtypesAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeRetnames(CmdDataForm.Data superData,List<string> oldV,List<string> newV)
            {
                if(superData is Data data)
                {

                changeRetnamesAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeRettypes(CmdDataForm.Data superData,List<string> oldV,List<string> newV)
            {
                if(superData is Data data)
                {

                changeRettypesAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeDesc(CmdDataForm.Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeDescAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeDefaultcode(CmdDataForm.Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeDefaultcodeAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeCategory(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                    DatasByCategory[oldV].Remove(data);
                    if(DatasByCategory[oldV].Count==0)
                        DatasByCategory.Remove(oldV);
                    if(!DatasByCategory.ContainsKey(newV))
                        DatasByCategory[newV]=new List<Data>();
                    DatasByCategory[newV].Add(data);
 
                    DatasByCategoryType[(oldV,data.type)].Remove(data);
                    if(DatasByCategoryType[(oldV,data.type)].Count==0)
                        DatasByCategoryType.Remove((oldV,data.type));
                    if(!DatasByCategoryType.ContainsKey((newV,data.type)))
                        DatasByCategoryType[(newV,data.type)]=new List<Data>();
                    DatasByCategoryType[(newV,data.type)].Add(data);
 
                changeCategoryAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeType(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                    DatasByCategoryType[(data.category,oldV)].Remove(data);
                    if(DatasByCategoryType[(data.category,oldV)].Count==0)
                        DatasByCategoryType.Remove((data.category,oldV));
                    if(!DatasByCategoryType.ContainsKey((data.category,newV)))
                        DatasByCategoryType[(data.category,newV)]=new List<Data>();
                    DatasByCategoryType[(data.category,newV)].Add(data);
 
                changeTypeAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
    }
}
        