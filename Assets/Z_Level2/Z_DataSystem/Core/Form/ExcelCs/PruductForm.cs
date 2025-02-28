using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Z_ByteSerialize;
using Z_DesignStyle;

namespace Z_DataSystem.Form
{

    public static partial class PruductForm
    {
        public static readonly int autoUidCnt=100;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Register()
        {


        }
        
        private static bool inited;
        public static Z_Chain.Chain uidChain;
        public static Action childInitAction;
        public static Action<Data> childRemoveAction;
        public static Action<Data> childAddAction;

        public partial class Data
        {

                private int _uid;

                public int uid{
                            get{return _uid;}
                             set{
                            
                            _uid = value;
                            }
                        }

                private string _name;

                /// <summary>
                ///Ãû³Æ
                ///</summary>
                public string name{
                            get{return _name;}
                             set{
                            if(_DataByUid!=null&&_DataByUid.ContainsValue(this)){RemoveData(uid); _name = value;AddData(this);}else
                            _name = value;
                            }
                        }

                private Dictionary<string,object> _paramDic;

                /// <summary>
                ///Êý¾Ý
                ///</summary>
                public Dictionary<string,object> paramDic{
                            get{return _paramDic;}
                             set{
                            
                            _paramDic = value;
                            }
                        }

            public Data(int uid,string name,Dictionary<string,object> paramDic)
            {

                this.uid = uid;
                this.name = name;
                this.paramDic = paramDic;

            }
            
        }

                   public static Data defaultData=new Data(0,"",new Dictionary<string,object>(){});


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

            InitInternal();
        }
        public static void InitInternal()
        {
            if(inited)
                return;
            inited=true;  
            uidChain=new Z_Chain.Chain (autoUidCnt);
            

                _DataByUid = new Dictionary<int, Data>() {

                };

                _DataByName = new Dictionary<string, Data>() {

                };


            childInitAction?.Invoke();
            


            foreach(var k in _DataByUid.Keys){ uidChain.PopId(k); }
             
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


            childRemoveAction?.Invoke(data);
        }
        public static void Clear()
        {
            Init();

                DataByUid.Clear();

                DataByName.Clear();

            uidChain.Clear();
        }

         private static void RemoveChildren(Data data)
        {
            Init();
            if(data is Data)
               RemoveData(data.uid);      
        }
         private static void AddChildren(Data superData)
        {
            Init();
            if(superData is Data data)
               AddData(data);      
        }
        


    }
}
        