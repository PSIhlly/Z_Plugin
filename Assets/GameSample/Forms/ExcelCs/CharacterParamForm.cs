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
            
        }
        
        private static bool inited;
        public static Z_Chain.Chain uidChain=>ParamForm.uidChain;
        public static Action childInitAction;
        public static Action<Data> childRemoveAction;
        public static Action<Data> childAddAction;

        public partial class Data : ParamForm.Data
        {

            public Data(int uid,string name,int valueType):base(uid,name,valueType)
            {

                this.uid = uid;
                this.name = name;
                this.valueType = valueType;

            }
            
        }

                   public static Data defaultData=new Data(0,"",0);


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

                    defaultData.valueType
                    );

            return data;
        }

        public static JObject GetJoByData(Data data)
        {
            Init();

            JObject jo=new JObject();

            jo.Set<int>("uid",data.uid);

            jo.Set<string>("name",data.name);

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

                DataByUid[data.uid]=data;

                DataByName[data.name]=data;

            
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

ParamForm.RemoveData(uid);
            childRemoveAction?.Invoke(data);
        }
        public static void Clear()
        {
            Init();

                DataByUid.Clear();

                DataByName.Clear();

            uidChain.Clear();
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
        


    }
}
        