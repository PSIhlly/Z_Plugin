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

    public static partial class MapObjectParamForm
    {

        

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Register()
        {

                GameParamForm.childInitAction+=InitInternal;


                GameParamForm.childRemoveAction+=RemoveChildren;
                GameParamForm.childAddAction+=AddChildren;
            

            GameParamForm.changeUidAction+=ChangeUid;

            GameParamForm.changeNameAction+=ChangeName;

            GameParamForm.changeValuetypeAction+=ChangeValuetype;

            GameParamForm.changeMinAction+=ChangeMin;

            GameParamForm.changeVAction+=ChangeV;

            GameParamForm.changeMaxAction+=ChangeMax;

            Z_Json.extra[typeof(Data)]=((obj)=>{
            if(obj is Data data)
                return GetJoByData(data);
            return null;
            },(jo)=>{
            return GetDataByJo(jo);
            });
        }
        
        private static bool inited;

        public static Z_Chain.Chain uidChain =>GameParamForm.uidChain;

        public static Action<Data> addAction;
        public static Action<Data> removeAction;
        public static Action childInitAction;
        public static Action<Data> childRemoveAction;
        public static Action<Data> childAddAction;
        
        public static Action<Data> beforeGetAction;

        public static Action<Data,int,int> changeUidAction;
                
        public static Action<Data,string,string> changeNameAction;
                
        public static Action<Data,ValType,ValType> changeValuetypeAction;
                
        public static Action<Data,string,string> changeMinAction;
                
        public static Action<Data,string,string> changeVAction;
                
        public static Action<Data,string,string> changeMaxAction;
                


        public partial class Data : GameParamForm.Data
        {

            public Data(GameParamForm.Data data):base(data.uid,data.name,data.valueType,data.min,data.v,data.max)
            {
            }
            
            public Data(int uid,string name,ValType valueType,string min,string v,string max):base(uid,name,valueType,min,v,max)
            {

             this.uid = uid;
             this.name = name;
             this.valueType = valueType;
             this.min = min;
             this.v = v;
             this.max = max;

            }
            public void Reset(Data data)
            {

             this.uid = data.uid;
             this.name = data.name;
             this.valueType = data.valueType;
             this.min = data.min;
             this.v = data.v;
             this.max = data.max;
            }

                public Data Copy(bool sameId = true)
                {
        return new Data(sameId? uid:uidChain.GetId(),name,valueType,min,v,max);
                }
            
            public override  void BeforeGet()
            {
                base.BeforeGet();
                MapObjectParamForm.beforeGetAction?.Invoke(this);
            }
        }

                   private static Data _defaultData=new Data(0,"",default,"","","");
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

            GameParamForm.Init();

        }
        public static void InitInternal()
        {
            if(inited)
                return;
            inited=true;  

        

                _DataByUid = new Dictionary<int, Data>() {

                };
                _DatasHashSet=new HashSet<Data>();
                
                    _DataByName = new Dictionary<string, Data>() {
    
                    
                    };
                    foreach(var v in _DataByUid.Values)
                    {
                        _DatasHashSet.Add(v);
                    }
    

            childInitAction?.Invoke();
            

            foreach(var data in DataByUid.Values)
            {
                GameParamForm.AddData(data);
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

                jo.SelectToken("valueType")==null?defaultData.valueType:jo.Get<ValType>("valueType"),

                jo.SelectToken("min")==null?defaultData.min:jo.Get<string>("min"),

                jo.SelectToken("v")==null?defaultData.v:jo.Get<string>("v"),

                jo.SelectToken("max")==null?defaultData.max:jo.Get<string>("max")
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

            jo.Set<ValType>("valueType",data.valueType);

            jo.Set<string>("min",data.min);

            jo.Set<string>("v",data.v);

            jo.Set<string>("max",data.max);

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
    
                    DataByName[data.name]=data;
    
GameParamForm.AddData(data);
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
                    
    
                    DataByName.Remove(data.name);
    
GameParamForm.RemoveData(uid);
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

         private static void RemoveChildren(GameParamForm.Data data)
        {
            Init();
            if(data is Data)
               RemoveData(data.uid);      
        }
         private static void AddChildren(GameParamForm.Data superData)
        {
            Init();
            if(superData is Data data)
               AddData(data);      
        }
        




            public static void ChangeUid(GameParamForm.Data superData,int oldV,int newV)
            {
                if(superData is Data data)
                {

                changeUidAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeName(GameParamForm.Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                    DataByName.Remove(oldV);
                    DataByName[newV]=data;
 
                changeNameAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeValuetype(GameParamForm.Data superData,ValType oldV,ValType newV)
            {
                if(superData is Data data)
                {

                changeValuetypeAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeMin(GameParamForm.Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeMinAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeV(GameParamForm.Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeVAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeMax(GameParamForm.Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeMaxAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
    }
}
        