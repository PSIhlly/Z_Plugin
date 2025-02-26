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

    public static partial class MapUnitForm
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Register()
        {

                UnitForm.childInitAction+=InitInternal;

        }

        private static bool inited;
        public static Z_Chain.Chain uidChain=>UnitForm.uidChain;
        public static Action childInitAction;

        public partial class Data : UnitForm.Data
        {

                /// <summary>
                ///单位逻辑
                ///</summary>
                public MapUnit unit
                {
                    get
                    {
                        return (MapUnit) _unit;
                    }
                }

                /// <summary>
                ///纹理名字（索引）
                ///</summary>
                public Dictionary<int,string> texNameDic;

                /// <summary>
                ///透明度纹理名字（索引）
                ///</summary>
                public Dictionary<int,string> alphaTexNameDic;

                /// <summary>
                ///离散位置
                ///</summary>
                public Vector3Int mapPos;

            public Data(int uid,string name,Dictionary<int,string> texNameDic,Dictionary<int,string> alphaTexNameDic,Vector3Int mapPos,string prefabName,Vector3 pos,Vector3 euler,Vector3 scale,int updateType):base(uid,name,prefabName,pos,euler,scale,updateType)
            {

                this.uid = uid;
                this.name = name;
                this.texNameDic = texNameDic;
                this.alphaTexNameDic = alphaTexNameDic;
                this.mapPos = mapPos;
                this.prefabName = prefabName;
                this.pos = pos;
                this.euler = euler;
                this.scale = scale;
                this.updateType = updateType;

                    _unit=new MapUnit(this);

            }
            
        }

                   public static Data defaultData=new Data(0,"",new Dictionary<int,string>(){},new Dictionary<int,string>(){},Vector3Int.zero,"",Vector3.zero,Vector3.zero,Vector3.zero,0);


        static Dictionary<int, Data> _DataByUid = null;
        public static Dictionary<int, Data> DataByUid
        {
            get
            {
                Init();
                return _DataByUid;
            }
        }

        static Dictionary<Vector3Int, Data> _DataByMappos = null;
        public static Dictionary<Vector3Int, Data> DataByMappos
        {
            get
            {
                Init();
                return _DataByMappos;
            }
        }


        static public void Init()
        {

            UnitForm.Init();

        }
        public static void InitInternal()
        {
            if(inited)
                return;
            inited=true;  
            
            

                _DataByUid = new Dictionary<int, Data>() {

                };

                _DataByMappos = new Dictionary<Vector3Int, Data>() {

                };


            childInitAction?.Invoke();
            

            foreach(var data in DataByUid.Values)
            {
                UnitForm.AddData(data);
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

                jo.Get<string>("name"),

                jo.Get<Dictionary<int,string>>("texNameDic"),

                jo.Get<Dictionary<int,string>>("alphaTexNameDic"),

                jo.Get<Vector3Int>("mapPos"),

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

            jo.Set<Dictionary<int,string>>("texNameDic",data.texNameDic);

            jo.Set<Dictionary<int,string>>("alphaTexNameDic",data.alphaTexNameDic);

            jo.Set<Vector3Int>("mapPos",data.mapPos);

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

                _DataByMappos[data.mapPos]=data;

            
UnitForm.AddData(data);
            return data.uid;
        }
        public static void RemoveData(int uid)
        {            
            Init();
            if(!_DataByUid.ContainsKey(uid))
                return;
                
            var data=_DataByUid[uid];

                _DataByUid.Remove(data.uid);

                _DataByMappos.Remove(data.mapPos);

UnitForm.RemoveData(uid);
        }
        public static void Clear()
        {
            Init();

                _DataByUid.Clear();

                _DataByMappos.Clear();

            uidChain.Clear();
        }

    }
}
        