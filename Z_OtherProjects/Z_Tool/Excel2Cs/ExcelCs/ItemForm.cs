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

    public static partial class ItemFormForm
    {
        private static bool inited;
        private static Queue<int> freeIdQueue;

        public partial class Data
        {

                public readonly int id;

                /// <summary>
                ///名字
                ///</summary>
                public readonly string name;

                /// <summary>
                ///图标
                ///</summary>
                public readonly string icon;

                /// <summary>
                ///拥有数
                ///</summary>
                public int count;

            public Data(int id,string name,string icon,int count)
            {

                this.id = id;
                this.name = name;
                this.icon = icon;
                this.count = count;
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

                {0,new Data(0,"","",0)},

                {1,new Data(1,"Space-Time Fragment","\\GameSample\\Imgs\\Item\\ST fragment.png",500)},

                {2,new Data(2,"Soul power","\\GameSample\\Imgs\\Item\\Soul Power.png",0)},

                };


            foreach(var k in _DataById.Keys)
            {
                freeIdQueue.Enqueue(k);
            }

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

                    _DataById[0].name,

                    _DataById[0].icon,

                        jo.Get<int>("count")
                    );

            return data;
        }

        public static JObject GetJoByData(Data data)
        {
            Init();

                    JObject jo=new JObject();

                    jo.Set<int>("id",data.id);

                    jo.Set<int>("count",data.count);

            return jo;
        }


    }
}
        