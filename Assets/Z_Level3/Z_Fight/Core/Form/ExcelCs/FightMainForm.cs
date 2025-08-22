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



            Z_Json.extra[typeof(Data)]=((obj)=>{
            if(obj is Data data)
                return GetJoByData(data);
            return null;
            },(jo)=>{
            return GetDataByJo(jo);
            });
        }
        
        private static bool inited;

        public static Z_Chain.Chain uidChain ;

        public static Action childInitAction;
        public static Action<Data> childRemoveAction;
        public static Action<Data> childAddAction;

        public static Action<Data,int,int> changeUidAction;
                
        public static Action<Data,int,int> changeUidcntAction;
                
        public static Action<Data,string,string> changeFightjaAction;
                
        public static Action<Data,string,string> changeWeaponjaAction;
                
        public static Action<Data,string,string> changeBulletjaAction;
                
        public static Action<Data,string,string> changeWeaponbulletjaAction;
                


        public partial class Data
        {

                    private int  _uid;
                    /// <summary>
                    ///
                    ///</summary>
                    public int  uid{
                                get{return _uid;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeUid(this,_uid,value); 
                    }
        
                _uid = value;
                }
                 
                     }
                    
                    private int  _uidCnt;
                    /// <summary>
                    ///uid总数
                    ///</summary>
                    public int  uidCnt{
                                get{return _uidCnt;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeUidcnt(this,_uidCnt,value); 
                    }
        
                _uidCnt = value;
                }
                 
                     }
                    
                    private string  _fightJa;
                    /// <summary>
                    ///战斗数据
                    ///</summary>
                    public string  fightJa{
                                get{return _fightJa;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeFightja(this,_fightJa,value); 
                    }
        
                _fightJa = value;
                }
                 
                     }
                    
                    private string  _weaponJa;
                    /// <summary>
                    ///武器数据
                    ///</summary>
                    public string  weaponJa{
                                get{return _weaponJa;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeWeaponja(this,_weaponJa,value); 
                    }
        
                _weaponJa = value;
                }
                 
                     }
                    
                    private string  _bulletJa;
                    /// <summary>
                    ///子弹数据
                    ///</summary>
                    public string  bulletJa{
                                get{return _bulletJa;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeBulletja(this,_bulletJa,value); 
                    }
        
                _bulletJa = value;
                }
                 
                     }
                    
                    private string  _weaponBulletJa;
                    /// <summary>
                    ///武器子弹数据
                    ///</summary>
                    public string  weaponBulletJa{
                                get{return _weaponBulletJa;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeWeaponbulletja(this,_weaponBulletJa,value); 
                    }
        
                _weaponBulletJa = value;
                }
                 
                     }
                    
            public Data(int uid,int uidCnt,string fightJa,string weaponJa,string bulletJa,string weaponBulletJa)
            {

             this.uid = uid;
             this.uidCnt = uidCnt;
             this.fightJa = fightJa;
             this.weaponJa = weaponJa;
             this.bulletJa = bulletJa;
             this.weaponBulletJa = weaponBulletJa;

            }

                public Data Copy(bool sameId = true)
                {
        return new Data(sameId? uid:uidChain.GetId(),uidCnt,fightJa,weaponJa,bulletJa,weaponBulletJa);
                }
            
        }

                   private static Data _defaultData=new Data(0,0,"","","","");
                   public static Data defaultData=>_defaultData.Copy();


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
            if(DataByUid.ContainsKey(data.uid))
                return data.uid;
            if(data.uid==-1)
            { 
                int uid=uidChain.GetId();
                if(uid==-1)
                    return -1;
                data.uid=uid;  
            }
            uidChain.PopId(data.uid);

        DataByUid[data.uid]=data;
    

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
    

            uidChain.PushId(data.uid);
            childRemoveAction?.Invoke(data);
        }
        public static void Clear()
        {
            Init();
            var keys = new List<int>(DataByUid.Keys);
            foreach(var key in keys)
            {
                    RemoveData(key);
            }

        }
        
        public static void ClearAuto()
        {
            Init();
            var keys = new List<int>(DataByUid.Keys);
            foreach(var key in keys)
            {
                if(key < uidChain.cnt)
                    RemoveData(key);
            }
        }

         private static void RemoveChildren(Data data)
        {
            Init();
            if(data is Data)
               RemoveData(data.uid);      
        }
         private static void AddChildren(Data superData)
        {
            Init();
            if(superData is Data data)
               AddData(data);      
        }
        




            public static void ChangeUid(Data superData,int oldV,int newV)
            {
                if(superData is Data data)
                {

                changeUidAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeUidcnt(Data superData,int oldV,int newV)
            {
                if(superData is Data data)
                {

                changeUidcntAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeFightja(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeFightjaAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeWeaponja(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeWeaponjaAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeBulletja(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeBulletjaAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeWeaponbulletja(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeWeaponbulletjaAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
    }
}
        