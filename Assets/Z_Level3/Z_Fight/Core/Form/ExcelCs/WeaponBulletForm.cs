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



            Z_Json.extra[typeof(Data)]=((obj)=>{
            if(obj is Data data)
                return GetJoByData(data);
            return null;
            },(jo)=>{
            return GetDataByJo(jo);
            });
        }
        
        private static bool inited;

        public static Z_Chain.Chain idChain ;

        public static Action<Data> addAction;
        public static Action<Data> removeAction;
        public static Action childInitAction;
        public static Action<Data> childRemoveAction;
        public static Action<Data> childAddAction;
        
        public static Action<Data> beforeGetAction;

        public static Action<Data,int,int> changeIdAction;
                
        public static Action<Data,int,int> changeItemidAction;
                
        public static Action<Data,int,int> changeDamageAction;
                
        public static Action<Data,string,string> changePrefabnameAction;
                
        public static Action<Data,int,int> changeMagazinecapacityAction;
                
        public static Action<Data,float,float> changeCdtimeAction;
                
        public static Action<Data,float,float> changeReloadtimeAction;
                
        public static Action<Data,float,float> changeSpeedAction;
                
        public static Action<Data,float,float> changeRangeAction;
                
        public static Action<Data,Vector3,Vector3> changeAttackposAction;
                
        public static Action<Data,Vector3,Vector3> changeAttackdirAction;
                
        public static Action<Data,bool,bool> changeSelfhurtAction;
                
        public static Action<Data,float,float> changeAccuracyAction;
                
        public static Action<Data,int,int> changeBulletsperAction;
                


        public partial class Data
        {

                    private int  _id;
                    /// <summary>
                    ///
                    ///</summary>
                    public int  id{
                                get{return _id;}
 set{

                    if(_DataById!=null&&_DataById.ContainsValue(this))
                    {
                       ChangeId(this,_id,value); 
                    }
        
                _id = value;
                }
                 
                     }
                    
                    private int  _itemId;
                    /// <summary>
                    ///武器道具id
                    ///</summary>
                    public int  itemId{
                                get{return _itemId;}
 set{

                    if(_DataById!=null&&_DataById.ContainsValue(this))
                    {
                       ChangeItemid(this,_itemId,value); 
                    }
        
                _itemId = value;
                }
                 
                     }
                    
                    private int  _damage;
                    /// <summary>
                    ///伤害
                    ///</summary>
                    public int  damage{
                                get{return _damage;}
 set{

                    if(_DataById!=null&&_DataById.ContainsValue(this))
                    {
                       ChangeDamage(this,_damage,value); 
                    }
        
                _damage = value;
                }
                 
                     }
                    
                    private string  _prefabName;
                    /// <summary>
                    ///预制名字（索引）
                    ///</summary>
                    public string  prefabName{
                                get{return _prefabName;}
 set{

                    if(_DataById!=null&&_DataById.ContainsValue(this))
                    {
                       ChangePrefabname(this,_prefabName,value); 
                    }
        
                _prefabName = value;
                }
                 
                     }
                    
                    private int  _magazineCapacity;
                    /// <summary>
                    ///弹夹总量
                    ///</summary>
                    public int  magazineCapacity{
                                get{return _magazineCapacity;}
 set{

                    if(_DataById!=null&&_DataById.ContainsValue(this))
                    {
                       ChangeMagazinecapacity(this,_magazineCapacity,value); 
                    }
        
                _magazineCapacity = value;
                }
                 
                     }
                    
                    private float  _cdTime;
                    /// <summary>
                    ///射速冷却时长
                    ///</summary>
                    public float  cdTime{
                                get{return _cdTime;}
 set{

                    if(_DataById!=null&&_DataById.ContainsValue(this))
                    {
                       ChangeCdtime(this,_cdTime,value); 
                    }
        
                _cdTime = value;
                }
                 
                     }
                    
                    private float  _reloadTime;
                    /// <summary>
                    ///装填时长
                    ///</summary>
                    public float  reloadTime{
                                get{return _reloadTime;}
 set{

                    if(_DataById!=null&&_DataById.ContainsValue(this))
                    {
                       ChangeReloadtime(this,_reloadTime,value); 
                    }
        
                _reloadTime = value;
                }
                 
                     }
                    
                    private float  _speed;
                    /// <summary>
                    ///弹速
                    ///</summary>
                    public float  speed{
                                get{return _speed;}
 set{

                    if(_DataById!=null&&_DataById.ContainsValue(this))
                    {
                       ChangeSpeed(this,_speed,value); 
                    }
        
                _speed = value;
                }
                 
                     }
                    
                    private float  _range;
                    /// <summary>
                    ///射程
                    ///</summary>
                    public float  range{
                                get{return _range;}
 set{

                    if(_DataById!=null&&_DataById.ContainsValue(this))
                    {
                       ChangeRange(this,_range,value); 
                    }
        
                _range = value;
                }
                 
                     }
                    
                    private Vector3  _attackPos;
                    /// <summary>
                    ///枪口
                    ///</summary>
                    public Vector3  attackPos{
                                get{return _attackPos;}
 set{

                    if(_DataById!=null&&_DataById.ContainsValue(this))
                    {
                       ChangeAttackpos(this,_attackPos,value); 
                    }
        
                _attackPos = value;
                }
                 
                     }
                    
                    private Vector3  _attackDir;
                    /// <summary>
                    ///方向
                    ///</summary>
                    public Vector3  attackDir{
                                get{return _attackDir;}
 set{

                    if(_DataById!=null&&_DataById.ContainsValue(this))
                    {
                       ChangeAttackdir(this,_attackDir,value); 
                    }
        
                _attackDir = value;
                }
                 
                     }
                    
                    private bool  _selfHurt;
                    /// <summary>
                    ///自己伤害
                    ///</summary>
                    public bool  selfHurt{
                                get{return _selfHurt;}
 set{

                    if(_DataById!=null&&_DataById.ContainsValue(this))
                    {
                       ChangeSelfhurt(this,_selfHurt,value); 
                    }
        
                _selfHurt = value;
                }
                 
                     }
                    
                    private float  _accuracy;
                    /// <summary>
                    ///精度
                    ///</summary>
                    public float  accuracy{
                                get{return _accuracy;}
 set{

                    if(_DataById!=null&&_DataById.ContainsValue(this))
                    {
                       ChangeAccuracy(this,_accuracy,value); 
                    }
        
                _accuracy = value;
                }
                 
                     }
                    
                    private int  _bulletsPer;
                    /// <summary>
                    ///单次开火弹数
                    ///</summary>
                    public int  bulletsPer{
                                get{return _bulletsPer;}
 set{

                    if(_DataById!=null&&_DataById.ContainsValue(this))
                    {
                       ChangeBulletsper(this,_bulletsPer,value); 
                    }
        
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
            public void Reset(Data data)
            {

             this.id = data.id;
             this.itemId = data.itemId;
             this.damage = data.damage;
             this.prefabName = data.prefabName;
             this.magazineCapacity = data.magazineCapacity;
             this.cdTime = data.cdTime;
             this.reloadTime = data.reloadTime;
             this.speed = data.speed;
             this.range = data.range;
             this.attackPos = data.attackPos;
             this.attackDir = data.attackDir;
             this.selfHurt = data.selfHurt;
             this.accuracy = data.accuracy;
             this.bulletsPer = data.bulletsPer;
            }

                public Data Copy(bool sameId = true)
                {
        return new Data(sameId? id:idChain.GetId(),itemId,damage,prefabName,magazineCapacity,cdTime,reloadTime,speed,range,attackPos,attackDir,selfHurt,accuracy,bulletsPer);
                }
            
            public virtual  void BeforeGet()
            {
                
                WeaponBulletForm.beforeGetAction?.Invoke(this);
            }
        }

                   private static Data _defaultData=new Data(0,0,0,"",0,0f,0f,0f,0f,Vector3.zero,Vector3.zero,false,0f,0);
                   public static Data defaultData=>_defaultData.Copy();


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

                jo.SelectToken("id")==null?defaultData.id:jo.Get<int>("id"),

                jo.SelectToken("itemId")==null?defaultData.itemId:jo.Get<int>("itemId"),

                jo.SelectToken("damage")==null?defaultData.damage:jo.Get<int>("damage"),

                jo.SelectToken("prefabName")==null?defaultData.prefabName:jo.Get<string>("prefabName"),

                jo.SelectToken("magazineCapacity")==null?defaultData.magazineCapacity:jo.Get<int>("magazineCapacity"),

                jo.SelectToken("cdTime")==null?defaultData.cdTime:jo.Get<float>("cdTime"),

                jo.SelectToken("reloadTime")==null?defaultData.reloadTime:jo.Get<float>("reloadTime"),

                jo.SelectToken("speed")==null?defaultData.speed:jo.Get<float>("speed"),

                jo.SelectToken("range")==null?defaultData.range:jo.Get<float>("range"),

                jo.SelectToken("attackPos")==null?defaultData.attackPos:jo.Get<Vector3>("attackPos"),

                jo.SelectToken("attackDir")==null?defaultData.attackDir:jo.Get<Vector3>("attackDir"),

                jo.SelectToken("selfHurt")==null?defaultData.selfHurt:jo.Get<bool>("selfHurt"),

                jo.SelectToken("accuracy")==null?defaultData.accuracy:jo.Get<float>("accuracy"),

                jo.SelectToken("bulletsPer")==null?defaultData.bulletsPer:jo.Get<int>("bulletsPer")
                    );

            return data;
        }

        public static JObject GetJoByData(Data data)
        {
            Init();
            data.BeforeGet();

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
            idChain.PopId(data.id);

        DataById[data.id]=data;
    

            childAddAction?.Invoke(data);
            addAction?.Invoke(data);
            return data.id;
        }
        public static void RemoveData(int id)
        {            
            Init();
            if(!DataById.ContainsKey(id))
                return;
               
            var data=DataById[id];

                    DataById.Remove(data.id);
    

            idChain.PushId(data.id);
            childRemoveAction?.Invoke(data);
            removeAction?.Invoke(data);
        }
        public static void Clear()
        {
            Init();
            var keys = new List<int>(DataById.Keys);
            foreach(var key in keys)
            {
                    RemoveData(key);
            }

        }
        
        public static void ClearAuto()
        {
            Init();
            var keys = new List<int>(DataById.Keys);
            foreach(var key in keys)
            {
                if(key < idChain.cnt)
                    RemoveData(key);
            }
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
        




            public static void ChangeId(Data superData,int oldV,int newV)
            {
                if(superData is Data data)
                {

                changeIdAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeItemid(Data superData,int oldV,int newV)
            {
                if(superData is Data data)
                {

                changeItemidAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeDamage(Data superData,int oldV,int newV)
            {
                if(superData is Data data)
                {

                changeDamageAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangePrefabname(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changePrefabnameAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeMagazinecapacity(Data superData,int oldV,int newV)
            {
                if(superData is Data data)
                {

                changeMagazinecapacityAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeCdtime(Data superData,float oldV,float newV)
            {
                if(superData is Data data)
                {

                changeCdtimeAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeReloadtime(Data superData,float oldV,float newV)
            {
                if(superData is Data data)
                {

                changeReloadtimeAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeSpeed(Data superData,float oldV,float newV)
            {
                if(superData is Data data)
                {

                changeSpeedAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeRange(Data superData,float oldV,float newV)
            {
                if(superData is Data data)
                {

                changeRangeAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeAttackpos(Data superData,Vector3 oldV,Vector3 newV)
            {
                if(superData is Data data)
                {

                changeAttackposAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeAttackdir(Data superData,Vector3 oldV,Vector3 newV)
            {
                if(superData is Data data)
                {

                changeAttackdirAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeSelfhurt(Data superData,bool oldV,bool newV)
            {
                if(superData is Data data)
                {

                changeSelfhurtAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeAccuracy(Data superData,float oldV,float newV)
            {
                if(superData is Data data)
                {

                changeAccuracyAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeBulletsper(Data superData,int oldV,int newV)
            {
                if(superData is Data data)
                {

                changeBulletsperAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
    }
}
        