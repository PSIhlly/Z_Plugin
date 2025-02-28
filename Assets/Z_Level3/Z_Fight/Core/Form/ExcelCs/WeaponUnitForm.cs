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

    public static partial class WeaponUnitForm
    {
        

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Register()
        {

                UnitForm.childInitAction+=InitInternal;


                UnitForm.childRemoveAction+=RemoveChildren;
            
        }
        
        private static bool inited;
        public static Z_Chain.Chain uidChain=>UnitForm.uidChain;
        public static Action childInitAction;
        public static Action<Data> childRemoveAction;

        public partial class Data : UnitForm.Data
        {

                /// <summary>
                ///单位逻辑
                ///</summary>
                public WeaponUnit unit
                {
                    get
                    {
                        return (WeaponUnit) _unit;
                    }
                }

                /// <summary>
                ///持有者
                ///</summary>
                public int fightUid;

                /// <summary>
                ///子弹类型列表
                ///</summary>
                public List<int> weaponBulletsId;

                /// <summary>
                ///当前使用子弹id
                ///</summary>
                public int curWeaponBulletAid;

                /// <summary>
                ///射速冷却时长余剩
                ///</summary>
                public float cdRemain;

                /// <summary>
                ///弹夹余剩
                ///</summary>
                public int magazineRemain;

            public Data(int uid,string name,int fightUid,List<int> weaponBulletsId,int curWeaponBulletAid,float cdRemain,int magazineRemain,string prefabName,Vector3 pos,Vector3 euler,Vector3 scale,int updateType):base(uid,name,prefabName,pos,euler,scale,updateType)
            {

                this.uid = uid;
                this.name = name;
                this.fightUid = fightUid;
                this.weaponBulletsId = weaponBulletsId;
                this.curWeaponBulletAid = curWeaponBulletAid;
                this.cdRemain = cdRemain;
                this.magazineRemain = magazineRemain;
                this.prefabName = prefabName;
                this.pos = pos;
                this.euler = euler;
                this.scale = scale;
                this.updateType = updateType;

                    _unit=new WeaponUnit(this);

            }
            
        }

                   public static Data defaultData=new Data(0,"",0,null,0,0f,0,"",Vector3.zero,Vector3.zero,Vector3.zero,0);


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

                jo.Get<string>("name"),

                jo.Get<int>("fightUid"),

                jo.Get<List<int>>("weaponBulletsId"),

                jo.Get<int>("curWeaponBulletAid"),

                jo.Get<float>("cdRemain"),

                jo.Get<int>("magazineRemain"),

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

            jo.Set<int>("fightUid",data.fightUid);

            jo.Set<List<int>>("weaponBulletsId",data.weaponBulletsId);

            jo.Set<int>("curWeaponBulletAid",data.curWeaponBulletAid);

            jo.Set<float>("cdRemain",data.cdRemain);

            jo.Set<int>("magazineRemain",data.magazineRemain);

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
            childRemoveAction?.Invoke(data);
        }
        public static void Clear()
        {
            Init();

                _DataByUid.Clear();

            uidChain.Clear();
        }

         private static void RemoveChildren(UnitForm.Data data)
        {
            Init();
            if(data is Data)
               RemoveData(data.uid);      
        }


    }
}
        