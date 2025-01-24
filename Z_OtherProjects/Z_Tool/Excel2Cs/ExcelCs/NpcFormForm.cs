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

    public static partial class NpcFormForm
    {
        private static bool inited;
        private static Queue<int> freeIdQueue;
        public static Action childInitAction;

        static NpcFormForm()
        {

        }
        public partial class Data
        {

                    public readonly int id;

                    /// <summary>
                    ///Ãû³Æ
                    ///</summary>
                    public readonly string name;

                    /// <summary>
                    ///Í·ÏñÍ¼Æ¬Id
                    ///</summary>
                    public readonly int avatar_imgId;

            public Data(int id,string name,int avatar_imgId)
            {

                this.id = id;
                this.name = name;
                this.avatar_imgId = avatar_imgId;
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

        static Dictionary<string, List<Data>> _DatasByName = null;
        public static Dictionary<string, List<Data>> DatasByName
        {
            get
            {
                Init();
                return _DatasByName;
            }
        }

        static Dictionary<int, Data> _DataByAvatar_imgid = null;
        public static Dictionary<int, Data> DataByAvatar_imgid
        {
            get
            {
                Init();
                return _DataByAvatar_imgid;
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

                {0,new Data(0,"",0)},

                {1,new Data(1,"human",100001)},

                {2,new Data(2,"pig",100002)},

                {3,new Data(3,"dog",100003)},

                {4,new Data(4,"chicken",100004)},

                };

                _DatasByName = new Dictionary<string, List<Data>>() {

                    {"",new List<Data>()},

                    {"human",new List<Data>()},

                    {"pig",new List<Data>()},

                    {"dog",new List<Data>()},

                    {"chicken",new List<Data>()},

                };

                    _DatasByName[""].Add(_DataById[0]);

                    _DatasByName["human"].Add(_DataById[1]);

                    _DatasByName["pig"].Add(_DataById[2]);

                    _DatasByName["dog"].Add(_DataById[3]);

                    _DatasByName["chicken"].Add(_DataById[4]);

                _DataByAvatar_imgid = new Dictionary<int, Data>() {

                    {0,_DataById[0]},

                    {100001,_DataById[1]},

                    {100002,_DataById[2]},

                    {100003,_DataById[3]},

                    {100004,_DataById[4]},

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

                    _DataById[0].name,

                    _DataById[0].avatar_imgId
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
        