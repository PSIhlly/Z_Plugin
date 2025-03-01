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

    public static partial class ImgFormForm
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

                private string _path;

                /// <summary>
                ///Í·ÏñÄ¿Â¼
                ///</summary>
                public string path{
                            get{return _path;}
                            private set{
                            
                            _path = value;
                            }
                        }

            public Data(int id,string path)
            {

                this.id = id;
                this.path = path;

            }
            
        }

                   public static Data defaultData=new Data(0,"");


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

                {100001,new Data(100001,"\\Z_Level2\\Z_UI\\Sample\\Imgs\\npc1")},

                {100002,new Data(100002,"\\Z_Level2\\Z_UI\\Sample\\Imgs\\npc2")},

                {100003,new Data(100003,"\\Z_Level2\\Z_UI\\Sample\\Imgs\\npc3")},

                {100004,new Data(100004,"\\Z_Level2\\Z_UI\\Sample\\Imgs\\npc4")},

                {200001,new Data(200001,"\\Z_Level2\\Z_UI\\Sample\\Imgs\\bg1")},

                {200002,new Data(200002,"\\Z_Level2\\Z_UI\\Sample\\Imgs\\bg2")},

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

                    defaultData.path
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
        