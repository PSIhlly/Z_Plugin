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

namespace Form
{

    public static partial class CharacterParamForm
    {

        

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Register()
        {

                ParamForm.childInitAction+=InitInternal;


                ParamForm.childRemoveAction+=RemoveChildren;
                ParamForm.childAddAction+=AddChildren;
            

            ParamForm.changeUidAction+=ChangeUid;

            ParamForm.changeNameAction+=ChangeName;

            ParamForm.changeValuetypeAction+=ChangeValuetype;

            ParamForm.changeMinAction+=ChangeMin;

            ParamForm.changeVAction+=ChangeV;

            ParamForm.changeMaxAction+=ChangeMax;

            Z_Json.extra[typeof(Data)]=((obj)=>{
            if(obj is Data data)
                return GetJoByData(data);
            return null;
            },(jo)=>{
            return GetDataByJo(jo);
            });
        }
        
        private static bool inited;

        public static Z_Chain.Chain uidChain =>ParamForm.uidChain;

        public static Action childInitAction;
        public static Action<Data> childRemoveAction;
        public static Action<Data> childAddAction;

        public static Action<Data,int,int> changeUidAction;
                
        public static Action<Data,string,string> changeNameAction;
                
        public static Action<Data,int,int> changeValuetypeAction;
                
        public static Action<Data,float,float> changeMinAction;
                
        public static Action<Data,float,float> changeVAction;
                
        public static Action<Data,float,float> changeMaxAction;
                


        public partial class Data : ParamForm.Data
        {

                    private int  _SpecialType;
                    /// <summary>
                    ///Ãÿ ‚¿‡–Õ
                    ///</summary>
                    public int  SpecialType{
                                get{return _SpecialType;}
private set{
        
                _SpecialType = value;
                }
                 
                     }
                    
            public Data(int uid,string name,int valueType,float min,float v,float max,int SpecialType):base(uid,name,valueType,min,v,max)
            {

             this.uid = uid;
             this.name = name;
             this.valueType = valueType;
             this.min = min;
             this.v = v;
             this.max = max;
             this.SpecialType = SpecialType;

            }

                public Data Copy()
                {
        return new Data(-1,name,valueType,min,v,max,SpecialType);
                }
            
        }

                   public static Data defaultData=new Data(0,"",0,0f,0f,0f,0);


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
    
            static Dictionary<int, Data> _DataBySpecialtype;
            public static Dictionary<int, Data> DataBySpecialtype
            {
                get
                {
                    Init();
                    return _DataBySpecialtype;
                }
            }
    

        static public void Init()
        {

            ParamForm.Init();

        }
        public static void InitInternal()
        {
            if(inited)
                return;
            inited=true;  

        

                _DataByUid = new Dictionary<int, Data>() {

                };
                    _DataByName = new Dictionary<string, Data>() {
    
                    };
    
                    _DataBySpecialtype = new Dictionary<int, Data>() {
    
                    };
    

            childInitAction?.Invoke();
            

            foreach(var data in DataByUid.Values)
            {
                ParamForm.AddData(data);
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

                jo.Get<int>("valueType"),

                jo.Get<float>("min"),

                jo.Get<float>("v"),

                jo.Get<float>("max"),

                    defaultData.SpecialType
                    );

            return data;
        }

        public static JObject GetJoByData(Data data)
        {
            Init();

            JObject jo=new JObject();

            jo.Set<int>("uid",data.uid);

            jo.Set<string>("name",data.name);

            jo.Set<int>("valueType",data.valueType);

            jo.Set<float>("min",data.min);

            jo.Set<float>("v",data.v);

            jo.Set<float>("max",data.max);

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
    
                    DataBySpecialtype[data.SpecialType]=data;
    
ParamForm.AddData(data);
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
    
                    DataByName.Remove(data.name);
    
                    DataBySpecialtype.Remove(data.SpecialType);
    
ParamForm.RemoveData(uid);
            uidChain.PushId(data.uid);
            childRemoveAction?.Invoke(data);
        }
        public static void Clear()
        {
            Init();

                    DataByUid.Clear();
    
                    DataByName.Clear();
    
                    DataBySpecialtype.Clear();
    
            uidChain.Clear();
        }
        
        public static void ClearAuto()
        {
            Init();
            foreach(var data in DataByUid.Values)
            {
                if(data.uid<uidChain.cnt)
                    RemoveData(data.uid);
            }
            
        }

         private static void RemoveChildren(ParamForm.Data data)
        {
            Init();
            if(data is Data)
               RemoveData(data.uid);      
        }
         private static void AddChildren(ParamForm.Data superData)
        {
            Init();
            if(superData is Data data)
               AddData(data);      
        }
        




            public static void ChangeUid(ParamForm.Data superData,int oldV,int newV)
            {
                if(superData is Data data)
                {

                changeUidAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeName(ParamForm.Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                    DataByName.Remove(oldV);
                    DataByName[newV]=data;
 
                changeNameAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeValuetype(ParamForm.Data superData,int oldV,int newV)
            {
                if(superData is Data data)
                {

                changeValuetypeAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeMin(ParamForm.Data superData,float oldV,float newV)
            {
                if(superData is Data data)
                {

                changeMinAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeV(ParamForm.Data superData,float oldV,float newV)
            {
                if(superData is Data data)
                {

                changeVAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeMax(ParamForm.Data superData,float oldV,float newV)
            {
                if(superData is Data data)
                {

                changeMaxAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
    }
}
        