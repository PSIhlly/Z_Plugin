using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Z_ByteSerialize;
using Z_DesignStyle;

namespace Form
{

    public static partial class DialogFormForm
    {
        public static readonly int autoIdCnt=100;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Register()
        {


        }
        
        private static bool inited;
        public static Z_Chain.Chain idChain;
        public static Action childInitAction;
        public static Action<Data> childRemoveAction;
        public static Action<Data> childAddAction;

        public partial class Data
        {

                private int _id;

                public int id{
                            get{return _id;}
                            private set{
                            
                            _id = value;
                            }
                        }

                private int _groupId;

                /// <summary>
                ///组号
                ///</summary>
                public int groupId{
                            get{return _groupId;}
                            private set{
                            
                            _groupId = value;
                            }
                        }

                private int _speaker_npcId;

                /// <summary>
                ///说话者Id
                ///</summary>
                public int speaker_npcId{
                            get{return _speaker_npcId;}
                            private set{
                            
                            _speaker_npcId = value;
                            }
                        }

                private int _background_imgId;

                /// <summary>
                ///对话背景图片Id
                ///</summary>
                public int background_imgId{
                            get{return _background_imgId;}
                            private set{
                            
                            _background_imgId = value;
                            }
                        }

                private string _text;

                /// <summary>
                ///对话文本
                ///</summary>
                public string text{
                            get{return _text;}
                            private set{
                            
                            _text = value;
                            }
                        }

            public Data(int id,int groupId,int speaker_npcId,int background_imgId,string text)
            {

                this.id = id;
                this.groupId = groupId;
                this.speaker_npcId = speaker_npcId;
                this.background_imgId = background_imgId;
                this.text = text;

            }
            
        }

                   public static Data defaultData=new Data(0,0,0,0,"");


        static Dictionary<int, Data> _DataById;
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
            inited=true;  
            idChain=new Z_Chain.Chain (autoIdCnt);
            

                _DataById = new Dictionary<int, Data>() {

                {1,new Data(1,1,1,200001,"hello")},

                {2,new Data(2,1,1,200002,"你好")},

                {3,new Data(3,1,2,200002,"world")},

                {4,new Data(4,2,2,200001,"ok")},

                {5,new Data(5,2,1,200001,"fine")},

                };


            childInitAction?.Invoke();
            


            foreach(var k in _DataById.Keys){ idChain.PopId(k); }
             
        }


        public static List<Data> GetDatasByJa(JArray ja)
        {
            Init();
            List<Data> lst=new List<Data>();
            foreach(JObject jo in ja)
            {
                if(jo.Get<int>("id")==0)
                    continue;
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
                if(data.id==0)
                    continue;
                ja.Add(GetJoByData(data));
            }
            return ja;
        }

        public static Data GetDataByJo(JObject jo)
        {
            Init();

            Data data=new Data(

                jo.Get<int>("id"),

                    defaultData.groupId,

                    defaultData.speaker_npcId,

                    defaultData.background_imgId,

                    defaultData.text
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
        