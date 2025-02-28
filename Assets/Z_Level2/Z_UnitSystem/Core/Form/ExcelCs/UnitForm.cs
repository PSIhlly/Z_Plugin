using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Z_ByteSerialize;
using Z_DesignStyle;

namespace Z_UnitSystem.Form
{

    public static partial class UnitForm
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

                    protected Unit _unit;

                /// <summary>
                ///单位逻辑
                ///</summary>
                public Unit unit
                {
                    get
                    {
                        return (Unit) _unit;
                    }
                }

                public int uid;

                /// <summary>
                ///名称
                ///</summary>
                public string name;

                /// <summary>
                ///预制名字（索引）
                ///</summary>
                public string prefabName;

                /// <summary>
                ///位置
                ///</summary>
                public Vector3 pos;

                /// <summary>
                ///欧拉旋转
                ///</summary>
                public Vector3 euler;

                /// <summary>
                ///缩放
                ///</summary>
                public Vector3 scale;

                /// <summary>
                ///更新方式
                ///</summary>
                public int updateType;

            public Data(int uid,string name,string prefabName,Vector3 pos,Vector3 euler,Vector3 scale,int updateType)
            {

                this.uid = uid;
                this.name = name;
                this.prefabName = prefabName;
                this.pos = pos;
                this.euler = euler;
                this.scale = scale;
                this.updateType = updateType;

                    _unit=new Unit(this);

            }
            
        }

                   public static Data defaultData=new Data(0,"","",Vector3.zero,Vector3.zero,Vector3.zero,0);


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

                jo.Get<string>("name"),

                jo.Get<string>("prefabName"),

                jo.Get<Vector3>("pos"),

                jo.Get<Vector3>("euler"),

                jo.Get<Vector3>("scale"),

                jo.Get<int>("updateType")
                    );

            return data;
        }

        public static JObject GetJoByData(Data data)
        {
            Init();

            JObject jo=new JObject();

            jo.Set<int>("uid",data.uid);

            jo.Set<string>("name",data.name);

            jo.Set<string>("prefabName",data.prefabName);

            jo.Set<Vector3>("pos",data.pos);

            jo.Set<Vector3>("euler",data.euler);

            jo.Set<Vector3>("scale",data.scale);

            jo.Set<int>("updateType",data.updateType);

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
        