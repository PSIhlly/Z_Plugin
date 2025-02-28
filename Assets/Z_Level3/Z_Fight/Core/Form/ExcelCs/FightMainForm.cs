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

    public static partial class FightMainForm
    {
        public static readonly int autoUidCnt=100;

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
                ///uid总数
                ///</summary>
                public int uidCnt;

                /// <summary>
                ///战斗数据
                ///</summary>
                public string fightJa;

                /// <summary>
                ///武器数据
                ///</summary>
                public string weaponJa;

                /// <summary>
                ///子弹数据
                ///</summary>
                public string bulletJa;

                /// <summary>
                ///武器子弹数据
                ///</summary>
                public string weaponBulletJa;

            public Data(int uid,int uidCnt,string fightJa,string weaponJa,string bulletJa,string weaponBulletJa)
            {

                this.uid = uid;
                this.uidCnt = uidCnt;
                this.fightJa = fightJa;
                this.weaponJa = weaponJa;
                this.bulletJa = bulletJa;
                this.weaponBulletJa = weaponBulletJa;

            }
            
        }

                   public static Data defaultData=new Data(0,0,"","","","");


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

                jo.Get<int>("uidCnt"),

                jo.Get<string>("fightJa"),

                jo.Get<string>("weaponJa"),

                jo.Get<string>("bulletJa"),

                jo.Get<string>("weaponBulletJa")
                    );

            return data;
        }

        public static JObject GetJoByData(Data data)
        {
            Init();

            JObject jo=new JObject();

            jo.Set<int>("uid",data.uid);

            jo.Set<int>("uidCnt",data.uidCnt);

            jo.Set<string>("fightJa",data.fightJa);

            jo.Set<string>("weaponJa",data.weaponJa);

            jo.Set<string>("bulletJa",data.bulletJa);

            jo.Set<string>("weaponBulletJa",data.weaponBulletJa);

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
        