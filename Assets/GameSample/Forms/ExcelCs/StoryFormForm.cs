using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Z_ByteSerialize;

namespace Form
{

    public static partial class StoryFormForm
    {
        private static bool inited;
        private static Queue<int> freeIdQueue;
        private static Action childInitAction;

        static StoryFormForm()
        {

        }
        public partial class Data
        {

                public int id;

                /// <summary>
                ///Ãû×Ö
                ///</summary>
                public string name;

                /// <summary>
                ///°üº¬³¡¾°id
                ///</summary>
                public List<int> sceneIds;

            public Data(int id,string name,List<int> sceneIds)
            {

                this.id = id;
                this.name = name;
                this.sceneIds = sceneIds;
            }
            
        }

        static Dictionary<int, Data> _DataById = null;
        public static Dictionary<int, Data> DataById
        {
            get
            {
                Init();
                return _DataById;
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
        freeIdQueue=new Queue<int> (Enumerable.Range(0, 100));

                _DataById = new Dictionary<int, Data>() {

                {0,new Data(0,"",null)},

                };


            foreach(var k in _DataById.Keys)
            {
                freeIdQueue.Enqueue(k);
            }
            childInitAction?.Invoke();
            inited=true;  
        }


        public static List<Data> GetDatasByJa(JArray ja)
        {
            Init();
            List<Data> lst=new List<Data>();
            foreach(JObject jo in ja)
            {
                lst.Add(GetDataByJo(jo));
            }
            return lst;
        }

        public static JArray GetJaByDatas()
        {
            Init();
            JArray ja=new JArray();
            foreach(Data data in _DataById.Values)
            {

                ja.Add(GetJoByData(data));
            }
            return ja;
        }

        public static Data GetDataByJo(JObject jo)
        {
            Init();

                    Data data=new Data(

                        jo.Get<int>("id"),

                        jo.Get<string>("name"),

                        jo.Get<List<int>>("sceneIds")
                    );

            return data;
        }

        public static JObject GetJoByData(Data data)
        {
            Init();

                    JObject jo=new JObject();

                    jo.Set<int>("id",data.id);

                    jo.Set<string>("name",data.name);

                    jo.Set<List<int>>("sceneIds",data.sceneIds);

            return jo;
        }


        public static int AddData(Data data,bool autoId=true)
        {
            Init();
           
            if(autoId)
            { 
                if(freeIdQueue.Count==0)
                return -1;
                int id=freeIdQueue.Dequeue();
                data.id=id;  
            }
            
                _DataById[data.id]=data;

            
            
            return data.id;
        }
        public static void RemoveData(int id)
        {
            Init();
            
            if(!_DataById.ContainsKey(id))
                return;
                
            var data=_DataById[id];
            
                _DataById.Remove(data.id);

            
        }

    }
}
        