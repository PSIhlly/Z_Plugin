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


                UnitForm.childRemoveAction+=RemoveChildren;
                UnitForm.childAddAction+=AddChildren;
            
        }
        
        private static bool inited;
        public static Z_Chain.Chain uidChain=>UnitForm.uidChain;
        public static Action childInitAction;
        public static Action<Data> childRemoveAction;
        public static Action<Data> childAddAction;

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

                private Dictionary<int,string> _texNameDic;

                /// <summary>
                ///纹理名字（索引）
                ///</summary>
                public Dictionary<int,string> texNameDic{
                            get{return _texNameDic;}
                             set{
                            
                            _texNameDic = value;
                            }
                        }

                private Dictionary<int,string> _alphaTexNameDic;

                /// <summary>
                ///透明度纹理名字（索引）
                ///</summary>
                public Dictionary<int,string> alphaTexNameDic{
                            get{return _alphaTexNameDic;}
                             set{
                            
                            _alphaTexNameDic = value;
                            }
                        }

                private Dictionary<int,int> _animInterval;

                /// <summary>
                ///动画间隔
                ///</summary>
                public Dictionary<int,int> animInterval{
                            get{return _animInterval;}
                             set{
                            
                            _animInterval = value;
                            }
                        }

                private Vector3Int _mapPos;

                /// <summary>
                ///离散位置
                ///</summary>
                public Vector3Int mapPos{
                            get{return _mapPos;}
                             set{
                            if(_DataByUid!=null&&_DataByUid.ContainsValue(this)){RemoveData(uid); _mapPos = value;AddData(this);}else
                            _mapPos = value;
                            }
                        }

            public Data(int uid,string name,Dictionary<int,string> texNameDic,Dictionary<int,string> alphaTexNameDic,Dictionary<int,int> animInterval,Vector3Int mapPos,string prefabName,Vector3 pos,Vector3 euler,Vector3 scale,int updateType):base(uid,name,prefabName,pos,euler,scale,updateType)
            {

                this.uid = uid;
                this.name = name;
                this.texNameDic = texNameDic;
                this.alphaTexNameDic = alphaTexNameDic;
                this.animInterval = animInterval;
                this.mapPos = mapPos;
                this.prefabName = prefabName;
                this.pos = pos;
                this.euler = euler;
                this.scale = scale;
                this.updateType = updateType;

                    _unit=new MapUnit(this);

            }
            
        }

                   public static Data defaultData=new Data(0,"",new Dictionary<int,string>(){},new Dictionary<int,string>(){},new Dictionary<int,int>(){},Vector3Int.zero,"",Vector3.zero,Vector3.zero,Vector3.zero,0);


        static Dictionary<int, Data> _DataByUid;
        public static Dictionary<int, Data> DataByUid
        {
            get
            {
                Init();
                return _DataByUid;
            }
        }

        static Dictionary<Vector3Int, Data> _DataByMappos;
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

                jo.Get<Dictionary<int,int>>("animInterval"),

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

            jo.Set<Dictionary<int,int>>("animInterval",data.animInterval);

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
            if(DataByUid.ContainsKey(data.uid))
                return data.uid;
            if(data.uid==-1)
            { 
                int uid=uidChain.GetId();
                if(uid==-1)
                    return -1;
                data.uid=uid;  
            }

                DataByUid[data.uid]=data;

                DataByMappos[data.mapPos]=data;

            
UnitForm.AddData(data);
            childAddAction?.Invoke(data);
            return data.uid;
        }
        public static void RemoveData(int uid)
        {            
            Init();
            if(!DataByUid.ContainsKey(uid))
                return;
                
            var data=DataByUid[uid];

                DataByUid.Remove(data.uid);

                DataByMappos.Remove(data.mapPos);

UnitForm.RemoveData(uid);
            childRemoveAction?.Invoke(data);
        }
        public static void Clear()
        {
            Init();

                DataByUid.Clear();

                DataByMappos.Clear();

            uidChain.Clear();
        }

         private static void RemoveChildren(UnitForm.Data data)
        {
            Init();
            if(data is Data)
               RemoveData(data.uid);      
        }
         private static void AddChildren(UnitForm.Data superData)
        {
            Init();
            if(superData is Data data)
               AddData(data);      
        }
        


    }
}
        