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

    public static partial class EventProgramDataForm
    {

        

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Register()
        {

                ProgramDataForm.childInitAction+=InitInternal;


                ProgramDataForm.childRemoveAction+=RemoveChildren;
                ProgramDataForm.childAddAction+=AddChildren;
            

            ProgramDataForm.changeUidAction+=ChangeUid;

            ProgramDataForm.changeNameAction+=ChangeName;

            ProgramDataForm.changeCodeAction+=ChangeCode;

            ProgramDataForm.changeZcodeAction+=ChangeZcode;

            ProgramDataForm.changeZcodemapAction+=ChangeZcodemap;

            ProgramDataForm.changeParamcountAction+=ChangeParamcount;

            ProgramDataForm.changeReturnvalueAction+=ChangeReturnvalue;

            Z_Json.extra[typeof(Data)]=((obj)=>{
            if(obj is Data data)
                return GetJoByData(data);
            return null;
            },(jo)=>{
            return GetDataByJo(jo);
            });
        }
        
        private static bool inited;

        public static Z_Chain.Chain uidChain =>ProgramDataForm.uidChain;

        public static Action<Data> addAction;
        public static Action<Data> removeAction;
        public static Action childInitAction;
        public static Action<Data> childRemoveAction;
        public static Action<Data> childAddAction;
        
        public static Action<Data> beforeGetAction;

        public static Action<Data,int,int> changeUidAction;
                
        public static Action<Data,string,string> changeNameAction;
                
        public static Action<Data,string,string> changeCodeAction;
                
        public static Action<Data,List<string>,List<string>> changeZcodeAction;
                
        public static Action<Data,List<int>,List<int>> changeZcodemapAction;
                
        public static Action<Data,int,int> changeParamcountAction;
                
        public static Action<Data,string,string> changeReturnvalueAction;
                
        public static Action<Data,string,string> changeCategoryAction;
                
        public static Action<Data,string,string> changeTypeAction;
                


        public partial class Data : ProgramDataForm.Data
        {

                    private string  _category;
                    /// <summary>
                    ///一级标签
                    ///</summary>
                    public string  category{
                                get{return _category;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
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

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeType(this,_type,value); 
                    }
        
                _type = value;
                }
                 
                     }
                    
            public Data(ProgramDataForm.Data data):base(data.uid,data.name,data.code,data.zCode,data.zCodeMap,data.paramCount,data.returnValue)
            {
            }
            
            public Data(int uid,string name,string code,List<string> zCode,List<int> zCodeMap,int paramCount,string returnValue,string category,string type):base(uid,name,code,zCode,zCodeMap,paramCount,returnValue)
            {

             this.uid = uid;
             this.name = name;
             this.code = code;
             this.zCode = zCode;
             this.zCodeMap = zCodeMap;
             this.paramCount = paramCount;
             this.returnValue = returnValue;
             this.category = category;
             this.type = type;

            }
            public void Reset(Data data)
            {

             this.uid = data.uid;
             this.name = data.name;
             this.code = data.code;
             this.zCode = data.zCode;
             this.zCodeMap = data.zCodeMap;
             this.paramCount = data.paramCount;
             this.returnValue = data.returnValue;
             this.category = data.category;
             this.type = data.type;
            }

                public Data Copy(bool sameId = true)
                {
        return new Data(sameId? uid:uidChain.GetId(),name,code,zCode==null?new List<string>():new List<string>(zCode),zCodeMap==null?new List<int>():new List<int>(zCodeMap),paramCount,returnValue,category,type);
                }
            
            public override  void BeforeGet()
            {
                base.BeforeGet();
                EventProgramDataForm.beforeGetAction?.Invoke(this);
            }
        }

                   private static Data _defaultData=new Data(0,"","",null,null,0,"","","");
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
    
            static Dictionary<string, List<Data>> _DatasByName;
            public static Dictionary<string, List<Data>> DatasByName
            {
                get
                {
                    Init();
                    return _DatasByName;
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
    

        static public void Init()
        {

            ProgramDataForm.Init();

        }
        public static void InitInternal()
        {
            if(inited)
                return;
            inited=true;  

        

                _DataByUid = new Dictionary<int, Data>() {

                };
                _DatasHashSet=new HashSet<Data>();
                
                    _DatasByName = new Dictionary<string, List<Data>>() {
    
                };

                    _DatasByCategoryType = new Dictionary<(string,string), List<Data>>() {
    
                };

                    _DatasByCategory = new Dictionary<string, List<Data>>() {
    
                };


            childInitAction?.Invoke();
            

            foreach(var data in DataByUid.Values)
            {
                ProgramDataForm.AddData(data);
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

                jo.SelectToken("code")==null?defaultData.code:jo.Get<string>("code"),

                jo.SelectToken("zCode")==null?defaultData.zCode:jo.Get<List<string>>("zCode"),

                jo.SelectToken("zCodeMap")==null?defaultData.zCodeMap:jo.Get<List<int>>("zCodeMap"),

                jo.SelectToken("paramCount")==null?defaultData.paramCount:jo.Get<int>("paramCount"),

                jo.SelectToken("returnValue")==null?defaultData.returnValue:jo.Get<string>("returnValue"),

                jo.SelectToken("category")==null?defaultData.category:jo.Get<string>("category"),

                jo.SelectToken("type")==null?defaultData.type:jo.Get<string>("type")
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

            jo.Set<string>("code",data.code);

            jo.Set<List<string>>("zCode",data.zCode);

            jo.Set<List<int>>("zCodeMap",data.zCodeMap);

            jo.Set<int>("paramCount",data.paramCount);

            jo.Set<string>("returnValue",data.returnValue);

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
        _DatasHashSet.Add(data);
    
                    if(!DatasByName.ContainsKey(data.name))
                        DatasByName[data.name]=new List<Data>();
                    DatasByName[data.name].Add(data);
    
                    if(!DatasByCategoryType.ContainsKey((data.category,data.type)))
                        DatasByCategoryType[(data.category,data.type)]=new List<Data>();
                    DatasByCategoryType[(data.category,data.type)].Add(data);
    
                    if(!DatasByCategory.ContainsKey(data.category))
                        DatasByCategory[data.category]=new List<Data>();
                    DatasByCategory[data.category].Add(data);
    
ProgramDataForm.AddData(data);
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
                    
    
                    if(DatasByName.ContainsKey(data.name))
                    {
                        DatasByName[data.name].Remove(data);
                        if(DatasByName[data.name].Count==0)
                            DatasByName.Remove(data.name);
                    }
                    
    
                    if(DatasByCategoryType.ContainsKey((data.category,data.type)))
                    {
                        DatasByCategoryType[(data.category,data.type)].Remove(data);
                        if(DatasByCategoryType[(data.category,data.type)].Count==0)
                            DatasByCategoryType.Remove((data.category,data.type));
                    }
                    
    
                    if(DatasByCategory.ContainsKey(data.category))
                    {
                        DatasByCategory[data.category].Remove(data);
                        if(DatasByCategory[data.category].Count==0)
                            DatasByCategory.Remove(data.category);
                    }
                    
    
ProgramDataForm.RemoveData(uid);
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

         private static void RemoveChildren(ProgramDataForm.Data data)
        {
            Init();
            if(data is Data)
               RemoveData(data.uid);      
        }
         private static void AddChildren(ProgramDataForm.Data superData)
        {
            Init();
            if(superData is Data data)
               AddData(data);      
        }
        




            public static void ChangeUid(ProgramDataForm.Data superData,int oldV,int newV)
            {
                if(superData is Data data)
                {

                changeUidAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeName(ProgramDataForm.Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                    DatasByName[oldV].Remove(data);
                    if(DatasByName[oldV].Count==0)
                        DatasByName.Remove(oldV);
                    if(!DatasByName.ContainsKey(newV))
                        DatasByName[newV]=new List<Data>();
                    DatasByName[newV].Add(data);
 
                changeNameAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeCode(ProgramDataForm.Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeCodeAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeZcode(ProgramDataForm.Data superData,List<string> oldV,List<string> newV)
            {
                if(superData is Data data)
                {

                changeZcodeAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeZcodemap(ProgramDataForm.Data superData,List<int> oldV,List<int> newV)
            {
                if(superData is Data data)
                {

                changeZcodemapAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeParamcount(ProgramDataForm.Data superData,int oldV,int newV)
            {
                if(superData is Data data)
                {

                changeParamcountAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeReturnvalue(ProgramDataForm.Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeReturnvalueAction?.Invoke(data,oldV,newV);
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
        