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

namespace Z_Map.Form
{

    public static partial class MapMainForm
    {
        public static readonly int autoUidCnt=1000000;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Register()
        {


        }
        
        private static bool inited;
        public static Z_Chain.Chain uidChain;
        public static Action childInitAction;
        public static Action<Data> childRemoveAction;

        public partial class Data
        {

                public int uid;

                /// <summary>
                ///单位图块大小
                ///</summary>
                public Vector3 mapUnitSize;

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
                public string itemJa;

                /// <summary>
                ///单位数据
                ///</summary>
                public string characterJa;

            public Data(int uid,Vector3 mapUnitSize,Vector3Int viewSize,string mapJa,string itemJa,string characterJa)
            {

                this.uid = uid;
                this.mapUnitSize = mapUnitSize;
                this.viewSize = viewSize;
                this.mapJa = mapJa;
                this.itemJa = itemJa;
                this.characterJa = characterJa;

            }
            
        }

                   public static Data defaultData=new Data(0,Vector3.zero,Vector3Int.zero,"","","");


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
            uidChain=new Z_Chain.Chain (autoUidCnt);
            

                _DataByUid = new Dictionary<int, Data>() {

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

                jo.Get<Vector3>("mapUnitSize"),

                jo.Get<Vector3Int>("viewSize"),

                jo.Get<string>("mapJa"),

                jo.Get<string>("itemJa"),

                jo.Get<string>("characterJa")
                    );

            return data;
        }

        public static JObject GetJoByData(Data data)
        {
            Init();

            JObject jo=new JObject();

            jo.Set<int>("uid",data.uid);

            jo.Set<Vector3>("mapUnitSize",data.mapUnitSize);

            jo.Set<Vector3Int>("viewSize",data.viewSize);

            jo.Set<string>("mapJa",data.mapJa);

            jo.Set<string>("itemJa",data.itemJa);

            jo.Set<string>("characterJa",data.characterJa);

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

            

            return data.uid;
        }
        public static void RemoveData(int uid)
        {            
            Init();
            if(!_DataByUid.ContainsKey(uid))
                return;
                
            var data=_DataByUid[uid];

                _DataByUid.Remove(data.uid);


            childRemoveAction?.Invoke(data);
        }
        public static void Clear()
        {
            Init();

                _DataByUid.Clear();

            uidChain.Clear();
        }

         private static void RemoveChildren(Data data)
        {
            Init();
            if(data is Data)
               RemoveData(data.uid);      
        }


    }
}
        