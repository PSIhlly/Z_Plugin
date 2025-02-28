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

namespace Z_Fight.Form
{

    public static partial class WeaponBulletForm
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
                             set{
                            
                            _id = value;
                            }
                        }

                private int _itemId;

                /// <summary>
                ///武器道具id
                ///</summary>
                public int itemId{
                            get{return _itemId;}
                             set{
                            
                            _itemId = value;
                            }
                        }

                private int _damage;

                /// <summary>
                ///伤害
                ///</summary>
                public int damage{
                            get{return _damage;}
                             set{
                            
                            _damage = value;
                            }
                        }

                private string _prefabName;

                /// <summary>
                ///预制名字（索引）
                ///</summary>
                public string prefabName{
                            get{return _prefabName;}
                             set{
                            
                            _prefabName = value;
                            }
                        }

                private int _magazineCapacity;

                /// <summary>
                ///弹夹总量
                ///</summary>
                public int magazineCapacity{
                            get{return _magazineCapacity;}
                             set{
                            
                            _magazineCapacity = value;
                            }
                        }

                private float _cdTime;

                /// <summary>
                ///射速冷却时长
                ///</summary>
                public float cdTime{
                            get{return _cdTime;}
                             set{
                            
                            _cdTime = value;
                            }
                        }

                private float _reloadTime;

                /// <summary>
                ///装填时长
                ///</summary>
                public float reloadTime{
                            get{return _reloadTime;}
                             set{
                            
                            _reloadTime = value;
                            }
                        }

                private float _speed;

                /// <summary>
                ///弹速
                ///</summary>
                public float speed{
                            get{return _speed;}
                             set{
                            
                            _speed = value;
                            }
                        }

                private float _range;

                /// <summary>
                ///射程
                ///</summary>
                public float range{
                            get{return _range;}
                             set{
                            
                            _range = value;
                            }
                        }

                private Vector3 _attackPos;

                /// <summary>
                ///枪口
                ///</summary>
                public Vector3 attackPos{
                            get{return _attackPos;}
                             set{
                            
                            _attackPos = value;
                            }
                        }

                private Vector3 _attackDir;

                /// <summary>
                ///方向
                ///</summary>
                public Vector3 attackDir{
                            get{return _attackDir;}
                             set{
                            
                            _attackDir = value;
                            }
                        }

                private bool _selfHurt;

                /// <summary>
                ///自己伤害
                ///</summary>
                public bool selfHurt{
                            get{return _selfHurt;}
                             set{
                            
                            _selfHurt = value;
                            }
                        }

                private float _accuracy;

                /// <summary>
                ///精度
                ///</summary>
                public float accuracy{
                            get{return _accuracy;}
                             set{
                            
                            _accuracy = value;
                            }
                        }

                private int _bulletsPer;

                /// <summary>
                ///单次开火弹数
                ///</summary>
                public int bulletsPer{
                            get{return _bulletsPer;}
                             set{
                            
                            _bulletsPer = value;
                            }
                        }

            public Data(int id,int itemId,int damage,string prefabName,int magazineCapacity,float cdTime,float reloadTime,float speed,float range,Vector3 attackPos,Vector3 attackDir,bool selfHurt,float accuracy,int bulletsPer)
            {

                this.id = id;
                this.itemId = itemId;
                this.damage = damage;
                this.prefabName = prefabName;
                this.magazineCapacity = magazineCapacity;
                this.cdTime = cdTime;
                this.reloadTime = reloadTime;
                this.speed = speed;
                this.range = range;
                this.attackPos = attackPos;
                this.attackDir = attackDir;
                this.selfHurt = selfHurt;
                this.accuracy = accuracy;
                this.bulletsPer = bulletsPer;

            }
            
        }

                   public static Data defaultData=new Data(0,0,0,"",0,0f,0f,0f,0f,Vector3.zero,Vector3.zero,false,0f,0);


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

                jo.Get<int>("itemId"),

                jo.Get<int>("damage"),

                jo.Get<string>("prefabName"),

                jo.Get<int>("magazineCapacity"),

                jo.Get<float>("cdTime"),

                jo.Get<float>("reloadTime"),

                jo.Get<float>("speed"),

                jo.Get<float>("range"),

                jo.Get<Vector3>("attackPos"),

                jo.Get<Vector3>("attackDir"),

                jo.Get<bool>("selfHurt"),

                jo.Get<float>("accuracy"),

                jo.Get<int>("bulletsPer")
                    );

            return data;
        }

        public static JObject GetJoByData(Data data)
        {
            Init();

            JObject jo=new JObject();

            jo.Set<int>("id",data.id);

            jo.Set<int>("itemId",data.itemId);

            jo.Set<int>("damage",data.damage);

            jo.Set<string>("prefabName",data.prefabName);

            jo.Set<int>("magazineCapacity",data.magazineCapacity);

            jo.Set<float>("cdTime",data.cdTime);

            jo.Set<float>("reloadTime",data.reloadTime);

            jo.Set<float>("speed",data.speed);

            jo.Set<float>("range",data.range);

            jo.Set<Vector3>("attackPos",data.attackPos);

            jo.Set<Vector3>("attackDir",data.attackDir);

            jo.Set<bool>("selfHurt",data.selfHurt);

            jo.Set<float>("accuracy",data.accuracy);

            jo.Set<int>("bulletsPer",data.bulletsPer);

            return jo;
        }


        public static int AddData(Data data)
        {
            Init();
            if(DataById.ContainsKey(data.id))
                return data.id;
            if(data.id==-1)
            { 
                int id=idChain.GetId();
                if(id==-1)
                    return -1;
                data.id=id;  
            }

                DataById[data.id]=data;

            

            childAddAction?.Invoke(data);
            return data.id;
        }
        public static void RemoveData(int id)
        {            
            Init();
            if(!DataById.ContainsKey(id))
                return;
                
            var data=DataById[id];

                DataById.Remove(data.id);


            childRemoveAction?.Invoke(data);
        }
        public static void Clear()
        {
            Init();

                DataById.Clear();

            idChain.Clear();
        }

         private static void RemoveChildren(Data data)
        {
            Init();
            if(data is Data)
               RemoveData(data.id);      
        }
         private static void AddChildren(Data superData)
        {
            Init();
            if(superData is Data data)
               AddData(data);      
        }
        


    }
}
        