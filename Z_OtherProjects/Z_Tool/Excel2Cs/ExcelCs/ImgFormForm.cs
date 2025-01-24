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

    public static partial class ImgFormForm
    {
        private static bool inited;
        private static Queue<int> freeIdQueue;
        public static Action childInitAction;

        static ImgFormForm()
        {

        }
        public partial class Data
        {

                    public readonly int id;

                    /// <summary>
                    ///Í·ÏñÄ¿Â¼
                    ///</summary>
                    public readonly string path;

            public Data(int id,string path)
            {

                this.id = id;
                this.path = path;
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

                {0,new Data(0,"")},

                {100001,new Data(100001,"\\Z_Level2\\Z_UI\\Sample\\Imgs\\npc1")},

                {100002,new Data(100002,"\\Z_Level2\\Z_UI\\Sample\\Imgs\\npc2")},

                {100003,new Data(100003,"\\Z_Level2\\Z_UI\\Sample\\Imgs\\npc3")},

                {100004,new Data(100004,"\\Z_Level2\\Z_UI\\Sample\\Imgs\\npc4")},

                {200001,new Data(200001,"\\Z_Level2\\Z_UI\\Sample\\Imgs\\bg1")},

                {200002,new Data(200002,"\\Z_Level2\\Z_UI\\Sample\\Imgs\\bg2")},

                };


            childInitAction?.Invoke();
            


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

                    _DataById[0].path
                    );

            return data;
        }

        public static JObject GetJoByData(Data data)
        {
            Init();

                    JObject jo=new JObject();

                    jo.Set<int>("id",data.id);

            return jo;
        }


    }
}
        