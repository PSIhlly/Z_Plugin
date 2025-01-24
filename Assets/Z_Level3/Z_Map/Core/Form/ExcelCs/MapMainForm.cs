using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Z_ByteSerialize;
using Z_UnitSystem.Form;
namespace Z_Map.Form
{

    public static partial class MapMainForm
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Register()
        {

        }

        private static bool inited;
        private static Queue<int> freeUidQueue;
        public static Action childInitAction;

        public partial class Data
        {

                public int uid;

                /// <summary>
                ///uid总数
                ///</summary>
                public int uidCnt;

                /// <summary>
                ///单位图块大小
                ///</summary>
                public Vector3 mapUnitSize;

                /// <summary>
                ///总尺寸
                ///</summary>
                public Vector3Int size;

                /// <summary>
                ///视口大小
                ///</summary>
                public Vector3Int viewSize;

                /// <summary>
                ///地图数据
                ///</summary>
                public string mapJa;

                /// <summary>
                ///物体数据
                ///</summary>
                public string ItemJa;

                /// <summary>
                ///单位数据
                ///</summary>
                public string CharacterJa;

            public Data(int uid,int uidCnt,Vector3 mapUnitSize,Vector3Int size,Vector3Int viewSize,string mapJa,string ItemJa,string CharacterJa)
            {

                this.uid = uid;
                this.uidCnt = uidCnt;
                this.mapUnitSize = mapUnitSize;
                this.size = size;
                this.viewSize = viewSize;
                this.mapJa = mapJa;
                this.ItemJa = ItemJa;
                this.CharacterJa = CharacterJa;

            }
            
        }

                   public static Data defaultData=new Data(0,0,Vector3.zero,Vector3Int.zero,Vector3Int.zero,"","","");


        static Dictionary<int, Data> _DataByUid = null;
        public static Dictionary<int, Data> DataByUid
        {
            get
            {
                Init();
                return _DataByUid;
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
            freeUidQueue=new Queue<int> (Enumerable.Range(0, 100));

                _DataByUid = new Dictionary<int, Data>() {

                };


            childInitAction?.Invoke();
            


             foreach(var k in _DataByUid.Keys)
            {
                freeUidQueue.Enqueue(k);
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

                jo.Get<int>("uidCnt"),

                jo.Get<Vector3>("mapUnitSize"),

                jo.Get<Vector3Int>("size"),

                jo.Get<Vector3Int>("viewSize"),

                jo.Get<string>("mapJa"),

                jo.Get<string>("ItemJa"),

                jo.Get<string>("CharacterJa")
                    );

            return data;
        }

        public static JObject GetJoByData(Data data)
        {
            Init();

            JObject jo=new JObject();

            jo.Set<int>("uid",data.uid);

            jo.Set<int>("uidCnt",data.uidCnt);

            jo.Set<Vector3>("mapUnitSize",data.mapUnitSize);

            jo.Set<Vector3Int>("size",data.size);

            jo.Set<Vector3Int>("viewSize",data.viewSize);

            jo.Set<string>("mapJa",data.mapJa);

            jo.Set<string>("ItemJa",data.ItemJa);

            jo.Set<string>("CharacterJa",data.CharacterJa);

            return jo;
        }


        public static int AddData(Data data,bool autoId=false)
        {
            Init();
            if(autoId)
            { 
                if(freeUidQueue.Count==0)
                return -1;
                int uid=freeUidQueue.Dequeue();
                data.uid=uid;  
            }

                _DataByUid[data.uid]=data;

            

            return data.uid;
        }
        public static void RemoveData(int uid)
        {            
            Init();
            if(!_DataByUid.ContainsKey(uid))
                return;
                
            var data=_DataByUid[uid];

                _DataByUid.Remove(data.uid);


        }
        public static void Clear()
        {
            Init();

                _DataByUid.Clear();

        }

    }
}
        