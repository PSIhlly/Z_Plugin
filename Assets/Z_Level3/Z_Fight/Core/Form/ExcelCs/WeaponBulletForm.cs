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
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Register()
        {

        }

        private static bool inited;
        public static Z_Chain.Chain idChain;
        public static Action childInitAction;

        public partial class Data
        {

                public int id;

                /// <summary>
                ///武器道具id
                ///</summary>
                public int itemId;

                /// <summary>
                ///伤害
                ///</summary>
                public int damage;

                /// <summary>
                ///预制名字（索引）
                ///</summary>
                public string prefabName;

                /// <summary>
                ///弹夹总量
                ///</summary>
                public int magazineCapacity;

                /// <summary>
                ///射速冷却时长
                ///</summary>
                public float cdTime;

                /// <summary>
                ///装填时长
                ///</summary>
                public float reloadTime;

                /// <summary>
                ///弹速
                ///</summary>
                public float speed;

                /// <summary>
                ///射程
                ///</summary>
                public float range;

                /// <summary>
                ///枪口
                ///</summary>
                public Vector3 attackPos;

                /// <summary>
                ///方向
                ///</summary>
                public Vector3 attackDir;

                /// <summary>
                ///自己伤害
                ///</summary>
                public bool selfHurt;

                /// <summary>
                ///精度
                ///</summary>
                public float accuracy;

                /// <summary>
                ///单次开火弹数
                ///</summary>
                public int bulletsPer;

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


        static Dictionary<int, Data> _DataById = null;
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
            idChain=new Z_Chain.Chain (100);
            

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
            if(data.id==-1)
            { 
                int id=idChain.GetId();
                if(id==-1)
                    return -1;
                data.id=id;  
            }

                _DataById[data.id]=data;

            

            return data.id;
        }
        public static void RemoveData(int id)
        {            
            Init();
            if(!_DataById.ContainsKey(id))
                return;
                
            var data=_DataById[id];

                _DataById.Remove(data.id);


        }
        public static void Clear()
        {
            Init();

                _DataById.Clear();

            idChain.Clear();
        }

    }
}
        