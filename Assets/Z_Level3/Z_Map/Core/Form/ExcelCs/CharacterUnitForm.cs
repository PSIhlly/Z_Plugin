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

    public static partial class CharacterUnitForm
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
                public CharacterUnit unit
                {
                    get
                    {
                        return (CharacterUnit) _unit;
                    }
                }

                private bool _navEnabled;

                /// <summary>
                ///启用
                ///</summary>
                public bool navEnabled{
                            get{return _navEnabled;}
                             set{
                            
                            _navEnabled = value;
                            }
                        }

                private Vector3 _destination;

                /// <summary>
                ///目的地
                ///</summary>
                public Vector3 destination{
                            get{return _destination;}
                             set{
                            
                            _destination = value;
                            }
                        }

                private float _speed;

                /// <summary>
                ///速度
                ///</summary>
                public float speed{
                            get{return _speed;}
                             set{
                            
                            _speed = value;
                            }
                        }

                private float _alertDis;

                /// <summary>
                ///启动距离
                ///</summary>
                public float alertDis{
                            get{return _alertDis;}
                             set{
                            
                            _alertDis = value;
                            }
                        }

                private float _pathDis;

                /// <summary>
                ///寻路距离上限
                ///</summary>
                public float pathDis{
                            get{return _pathDis;}
                             set{
                            
                            _pathDis = value;
                            }
                        }

                private bool _isMine;

                /// <summary>
                ///是我自己
                ///</summary>
                public bool isMine{
                            get{return _isMine;}
                             set{
                            
                            _isMine = value;
                            }
                        }

            public Data(int uid,bool navEnabled,Vector3 destination,float speed,float alertDis,float pathDis,bool isMine,string name,string prefabName,Vector3 pos,Vector3 euler,Vector3 scale,int updateType):base(uid,name,prefabName,pos,euler,scale,updateType)
            {

                this.uid = uid;
                this.navEnabled = navEnabled;
                this.destination = destination;
                this.speed = speed;
                this.alertDis = alertDis;
                this.pathDis = pathDis;
                this.isMine = isMine;
                this.name = name;
                this.prefabName = prefabName;
                this.pos = pos;
                this.euler = euler;
                this.scale = scale;
                this.updateType = updateType;

                    _unit=new CharacterUnit(this);

            }
            
        }

                   public static Data defaultData=new Data(0,false,Vector3.zero,0f,0f,0f,false,"","",Vector3.zero,Vector3.zero,Vector3.zero,0);


        static Dictionary<int, Data> _DataByUid;
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

            UnitForm.Init();

        }
        public static void InitInternal()
        {
            if(inited)
                return;
            inited=true;  
            
            

                _DataByUid = new Dictionary<int, Data>() {

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

                jo.Get<bool>("navEnabled"),

                jo.Get<Vector3>("destination"),

                jo.Get<float>("speed"),

                jo.Get<float>("alertDis"),

                jo.Get<float>("pathDis"),

                jo.Get<bool>("isMine"),

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

            jo.Set<bool>("navEnabled",data.navEnabled);

            jo.Set<Vector3>("destination",data.destination);

            jo.Set<float>("speed",data.speed);

            jo.Set<float>("alertDis",data.alertDis);

            jo.Set<float>("pathDis",data.pathDis);

            jo.Set<bool>("isMine",data.isMine);

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

UnitForm.RemoveData(uid);
            childRemoveAction?.Invoke(data);
        }
        public static void Clear()
        {
            Init();

                DataByUid.Clear();

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
        