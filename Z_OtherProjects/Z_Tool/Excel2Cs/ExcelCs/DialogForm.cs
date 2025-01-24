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

    public static partial class DialogFormForm
    {
        private static bool inited;
        private static Queue<int> freeIdQueue;

        public partial class Data
        {

                public readonly int id;

                /// <summary>
                ///组号
                ///</summary>
                public readonly int groupId;

                /// <summary>
                ///说话者Id
                ///</summary>
                public readonly int speaker_npcId;

                /// <summary>
                ///对话背景图片Id
                ///</summary>
                public readonly int background_imgId;

                /// <summary>
                ///对话文本
                ///</summary>
                public readonly string text;

            public Data(int id,int groupId,int speaker_npcId,int background_imgId,string text)
            {

                this.id = id;
                this.groupId = groupId;
                this.speaker_npcId = speaker_npcId;
                this.background_imgId = background_imgId;
                this.text = text;
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

                {0,new Data(0,0,0,0,"")},

                {1,new Data(1,1,1,200001,"hello")},

                {2,new Data(2,1,1,200002,"你好")},

                {3,new Data(3,1,2,200002,"world")},

                {4,new Data(4,2,2,200001,"ok")},

                {5,new Data(5,2,1,200001,"fine")},

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

                    _DataById[0].groupId,

                    _DataById[0].speaker_npcId,

                    _DataById[0].background_imgId,

                    _DataById[0].text
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
        