using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Z_ByteSerialize;
using Z_UnitSystem.Form;
namespace Z_Fight.Form
{

    public static partial class FightUnitForm
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Register()
        {

                UnitForm.childInitAction+=InitInternal;

        }

        private static bool inited;
        private static Queue<int> freeUidQueue;
        public static Action childInitAction;

        public partial class Data : UnitForm.Data
        {

                /// <summary>
                ///单位逻辑
                ///</summary>
                public FightUnit unit
                {
                    get
                    {
                        return (FightUnit) _unit;
                    }
                }

                /// <summary>
                ///道具持有数字典
                ///</summary>
                public Dictionary<int,int> itemIdCountDic;

                /// <summary>
                ///使用中subId
                ///</summary>
                public List<int> curUsingWeaponsSid;

                /// <summary>
                ///装填中subId
                ///</summary>
                public List<int> curReloadWeaponsSid;

                /// <summary>
                ///战斗触发距离
                ///</summary>
                public float alertDistance;

                /// <summary>
                ///血量
                ///</summary>
                public float hp;

                /// <summary>
                ///血量上限
                ///</summary>
                public float hpMax;

                /// <summary>
                ///护甲
                ///</summary>
                public float defence;

                /// <summary>
                ///目标uid
                ///</summary>
                public int targetFightUid;

                /// <summary>
                ///装填持续时间
                ///</summary>
                public float reloadTime;

                /// <summary>
                ///是我自己
                ///</summary>
                public bool isMine;

            public Data(int uid,Dictionary<int,int> itemIdCountDic,List<int> curUsingWeaponsSid,List<int> curReloadWeaponsSid,float alertDistance,float hp,float hpMax,float defence,int targetFightUid,float reloadTime,bool isMine,string prefabName,Vector3 pos,Vector3 euler,Vector3 scale,int updateType):base(uid,prefabName,pos,euler,scale,updateType)
            {

                this.uid = uid;
                this.itemIdCountDic = itemIdCountDic;
                this.curUsingWeaponsSid = curUsingWeaponsSid;
                this.curReloadWeaponsSid = curReloadWeaponsSid;
                this.alertDistance = alertDistance;
                this.hp = hp;
                this.hpMax = hpMax;
                this.defence = defence;
                this.targetFightUid = targetFightUid;
                this.reloadTime = reloadTime;
                this.isMine = isMine;
                this.prefabName = prefabName;
                this.pos = pos;
                this.euler = euler;
                this.scale = scale;
                this.updateType = updateType;

                    _unit=new FightUnit(this);

            }
            
        }

                   public static Data defaultData=new Data(0,new Dictionary<int,int>(){},null,null,0f,0f,0f,0f,0,0f,false,"",Vector3.zero,Vector3.zero,Vector3.zero,0);


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

            UnitForm.Init();

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
            

            foreach(var data in DataByUid.Values)
            {
                UnitForm.AddData(data,false);
            }


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

                jo.Get<Dictionary<int,int>>("itemIdCountDic"),

                jo.Get<List<int>>("curUsingWeaponsSid"),

                jo.Get<List<int>>("curReloadWeaponsSid"),

                jo.Get<float>("alertDistance"),

                jo.Get<float>("hp"),

                jo.Get<float>("hpMax"),

                jo.Get<float>("defence"),

                jo.Get<int>("targetFightUid"),

                jo.Get<float>("reloadTime"),

                jo.Get<bool>("isMine"),

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

            jo.Set<Dictionary<int,int>>("itemIdCountDic",data.itemIdCountDic);

            jo.Set<List<int>>("curUsingWeaponsSid",data.curUsingWeaponsSid);

            jo.Set<List<int>>("curReloadWeaponsSid",data.curReloadWeaponsSid);

            jo.Set<float>("alertDistance",data.alertDistance);

            jo.Set<float>("hp",data.hp);

            jo.Set<float>("hpMax",data.hpMax);

            jo.Set<float>("defence",data.defence);

            jo.Set<int>("targetFightUid",data.targetFightUid);

            jo.Set<float>("reloadTime",data.reloadTime);

            jo.Set<bool>("isMine",data.isMine);

            jo.Set<string>("prefabName",data.prefabName);

            jo.Set<Vector3>("pos",data.pos);

            jo.Set<Vector3>("euler",data.euler);

            jo.Set<Vector3>("scale",data.scale);

            jo.Set<int>("updateType",data.updateType);

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

UnitForm.RemoveData(uid);
        }
        public static void Clear()
        {
            Init();

                _DataByUid.Clear();

        }

    }
}
        