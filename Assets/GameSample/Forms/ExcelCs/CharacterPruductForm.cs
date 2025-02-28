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

    public static partial class CharacterPruductForm
    {
        

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Register()
        {

                PruductForm.childInitAction+=InitInternal;


                PruductForm.childRemoveAction+=RemoveChildren;
            
        }
        
        private static bool inited;
        public static Z_Chain.Chain uidChain=>PruductForm.uidChain;
        public static Action childInitAction;
        public static Action<Data> childRemoveAction;

        public partial class Data : PruductForm.Data
        {

            public Data(int uid,string name,Dictionary<string,object> paramDic):base(uid,name,paramDic)
            {

                this.uid = uid;
                this.name = name;
                this.paramDic = paramDic;

            }
            
        }

                   public static Data defaultData=new Data(0,"",new Dictionary<string,object>(){});


        static Dictionary<int, Data> _DataByUid = null;
        public static Dictionary<int, Data> DataByUid
        {
            get
            {
                Init();
                return _DataByUid;
            }
        }

        static Dictionary<string, Data> _DataByName = null;
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

            PruductForm.Init();

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
                PruductForm.AddData(data);
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

                jo.Get<Dictionary<string,object>>("paramDic")
                    );

            return data;
        }

        public static JObject GetJoByData(Data data)
        {
            Init();

            JObject jo=new JObject();

            jo.Set<int>("uid",data.uid);

            jo.Set<string>("name",data.name);

            jo.Set<Dictionary<string,object>>("paramDic",data.paramDic);

            return jo;
        }


        public static int AddData(Data data)
        {
            Init();
            if(data.uid==-1)
            { 
                int uid=uidChain.GetId();
                if(uid==-1)
                    return -1;
                data.uid=uid;  
            }

                _DataByUid[data.uid]=data;

                _DataByName[data.name]=data;

            
PruductForm.AddData(data);
            return data.uid;
        }
        public static void RemoveData(int uid)
        {            
            Init();
            if(!_DataByUid.ContainsKey(uid))
                return;
                
            var data=_DataByUid[uid];

                _DataByUid.Remove(data.uid);

                _DataByName.Remove(data.name);

PruductForm.RemoveData(uid);
            childRemoveAction?.Invoke(data);
        }
        public static void Clear()
        {
            Init();

                _DataByUid.Clear();

                _DataByName.Clear();

            uidChain.Clear();
        }

         private static void RemoveChildren(PruductForm.Data data)
        {
            Init();
            if(data is Data)
               RemoveData(data.uid);      
        }


    }
}
        