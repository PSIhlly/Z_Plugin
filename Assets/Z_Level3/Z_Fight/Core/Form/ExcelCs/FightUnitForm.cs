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

    public static partial class FightUnitForm
    {

        

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Register()
        {

                UnitForm.childInitAction+=InitInternal;


                UnitForm.childRemoveAction+=RemoveChildren;
                UnitForm.childAddAction+=AddChildren;
            

            UnitForm.changeUidAction+=ChangeUid;

            UnitForm.changeNameAction+=ChangeName;

            UnitForm.changePrefabnameAction+=ChangePrefabname;

            UnitForm.changePosAction+=ChangePos;

            UnitForm.changeEulerAction+=ChangeEuler;

            UnitForm.changeScaleAction+=ChangeScale;

            UnitForm.changeUpdatetypeAction+=ChangeUpdatetype;

            UnitForm.changeCollidingunituidAction+=ChangeCollidingunituid;

            UnitForm.changeExtraAction+=ChangeExtra;

            Z_Json.extra[typeof(Data)]=((obj)=>{
            if(obj is Data data)
                return GetJoByData(data);
            return null;
            },(jo)=>{
            return GetDataByJo(jo);
            });
        }
        
        private static bool inited;

        public static Z_Chain.Chain uidChain =>UnitForm.uidChain;

        public static Action<Data> addAction;
        public static Action<Data> removeAction;
        public static Action childInitAction;
        public static Action<Data> childRemoveAction;
        public static Action<Data> childAddAction;

        public static Action<Data,int,int> changeUidAction;
                
        public static Action<Data,string,string> changeNameAction;
                
        public static Action<Data,Dictionary<int,int>,Dictionary<int,int>> changeItemidcountdicAction;
                
        public static Action<Data,List<int>,List<int>> changeCurusingweaponssidAction;
                
        public static Action<Data,List<int>,List<int>> changeCurreloadweaponssidAction;
                
        public static Action<Data,float,float> changeAlertdistanceAction;
                
        public static Action<Data,float,float> changeHpAction;
                
        public static Action<Data,float,float> changeHpmaxAction;
                
        public static Action<Data,float,float> changeDefenceAction;
                
        public static Action<Data,int,int> changeTargetfightuidAction;
                
        public static Action<Data,float,float> changeReloadtimeAction;
                
        public static Action<Data,bool,bool> changeIsmineAction;
                
        public static Action<Data,string,string> changePrefabnameAction;
                
        public static Action<Data,Vector3,Vector3> changePosAction;
                
        public static Action<Data,Vector3,Vector3> changeEulerAction;
                
        public static Action<Data,Vector3,Vector3> changeScaleAction;
                
        public static Action<Data,UpdateType,UpdateType> changeUpdatetypeAction;
                
        public static Action<Data,List<int>,List<int>> changeCollidingunituidAction;
                
        public static Action<Data,string,string> changeExtraAction;
                


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

                    private Dictionary<int,int>  _itemIdCountDic;
                    /// <summary>
                    ///道具持有数字典
                    ///</summary>
                    public Dictionary<int,int>  itemIdCountDic{
                                get{return _itemIdCountDic;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeItemidcountdic(this,_itemIdCountDic,value); 
                    }
        
                _itemIdCountDic = value;
                }
                 
                     }
                    
                    private List<int>  _curUsingWeaponsSid;
                    /// <summary>
                    ///使用中subId
                    ///</summary>
                    public List<int>  curUsingWeaponsSid{
                                get{return _curUsingWeaponsSid;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeCurusingweaponssid(this,_curUsingWeaponsSid,value); 
                    }
        
                _curUsingWeaponsSid = value;
                }
                 
                     }
                    
                    private List<int>  _curReloadWeaponsSid;
                    /// <summary>
                    ///装填中subId
                    ///</summary>
                    public List<int>  curReloadWeaponsSid{
                                get{return _curReloadWeaponsSid;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeCurreloadweaponssid(this,_curReloadWeaponsSid,value); 
                    }
        
                _curReloadWeaponsSid = value;
                }
                 
                     }
                    
                    private float  _alertDistance;
                    /// <summary>
                    ///战斗触发距离
                    ///</summary>
                    public float  alertDistance{
                                get{return _alertDistance;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeAlertdistance(this,_alertDistance,value); 
                    }
        
                _alertDistance = value;
                }
                 
                     }
                    
                    private float  _hp;
                    /// <summary>
                    ///血量
                    ///</summary>
                    public float  hp{
                                get{return _hp;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeHp(this,_hp,value); 
                    }
        
                _hp = value;
                }
                 
                     }
                    
                    private float  _hpMax;
                    /// <summary>
                    ///血量上限
                    ///</summary>
                    public float  hpMax{
                                get{return _hpMax;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeHpmax(this,_hpMax,value); 
                    }
        
                _hpMax = value;
                }
                 
                     }
                    
                    private float  _defence;
                    /// <summary>
                    ///护甲
                    ///</summary>
                    public float  defence{
                                get{return _defence;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeDefence(this,_defence,value); 
                    }
        
                _defence = value;
                }
                 
                     }
                    
                    private int  _targetFightUid;
                    /// <summary>
                    ///目标uid
                    ///</summary>
                    public int  targetFightUid{
                                get{return _targetFightUid;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeTargetfightuid(this,_targetFightUid,value); 
                    }
        
                _targetFightUid = value;
                }
                 
                     }
                    
                    private float  _reloadTime;
                    /// <summary>
                    ///装填持续时间
                    ///</summary>
                    public float  reloadTime{
                                get{return _reloadTime;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeReloadtime(this,_reloadTime,value); 
                    }
        
                _reloadTime = value;
                }
                 
                     }
                    
                    private bool  _isMine;
                    /// <summary>
                    ///是我自己
                    ///</summary>
                    public bool  isMine{
                                get{return _isMine;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeIsmine(this,_isMine,value); 
                    }
        
                _isMine = value;
                }
                 
                     }
                    
            public Data(UnitForm.Data data):base(data.uid,data.name,data.prefabName,data.pos,data.euler,data.scale,data.updateType,data.collidingUnitUid,data.extra)
            {
            }
            
            public Data(int uid,string name,Dictionary<int,int> itemIdCountDic,List<int> curUsingWeaponsSid,List<int> curReloadWeaponsSid,float alertDistance,float hp,float hpMax,float defence,int targetFightUid,float reloadTime,bool isMine,string prefabName,Vector3 pos,Vector3 euler,Vector3 scale,UpdateType updateType,List<int> collidingUnitUid,string extra):base(uid,name,prefabName,pos,euler,scale,updateType,collidingUnitUid,extra)
            {

             this.uid = uid;
             this.name = name;
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
             this.collidingUnitUid = collidingUnitUid;
             this.extra = extra;

                    _unit=new FightUnit(this);

            }
            public void Reset(Data data)
            {

             this.uid = data.uid;
             this.name = data.name;
             this.itemIdCountDic = data.itemIdCountDic;
             this.curUsingWeaponsSid = data.curUsingWeaponsSid;
             this.curReloadWeaponsSid = data.curReloadWeaponsSid;
             this.alertDistance = data.alertDistance;
             this.hp = data.hp;
             this.hpMax = data.hpMax;
             this.defence = data.defence;
             this.targetFightUid = data.targetFightUid;
             this.reloadTime = data.reloadTime;
             this.isMine = data.isMine;
             this.prefabName = data.prefabName;
             this.pos = data.pos;
             this.euler = data.euler;
             this.scale = data.scale;
             this.updateType = data.updateType;
             this.collidingUnitUid = data.collidingUnitUid;
             this.extra = data.extra;
            }

                public Data Copy(bool sameId = true)
                {
        return new Data(sameId? uid:uidChain.GetId(),name,new Dictionary<int,int>(itemIdCountDic),new List<int>(curUsingWeaponsSid),new List<int>(curReloadWeaponsSid),alertDistance,hp,hpMax,defence,targetFightUid,reloadTime,isMine,prefabName,pos,euler,scale,updateType,new List<int>(collidingUnitUid),extra);
                }
            
        }

                   private static Data _defaultData=new Data(0,"",new Dictionary<int,int>(){},null,null,0f,0f,0f,0f,0,0f,false,"",Vector3.zero,Vector3.zero,Vector3.zero,UpdateType.ShowOnly,null,"");
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

                jo.Get<UpdateType>("updateType"),

                jo.Get<List<int>>("collidingUnitUid"),

                jo.Get<string>("extra")
                    );

            return data;
        }

        public static JObject GetJoByData(Data data)
        {
            Init();

            JObject jo=new JObject();

            jo.Set<int>("uid",data.uid);

            jo.Set<string>("name",data.name);

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

            jo.Set<UpdateType>("updateType",data.updateType);

            jo.Set<List<int>>("collidingUnitUid",data.collidingUnitUid);

            jo.Set<string>("extra",data.extra);

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
    
UnitForm.AddData(data);
            childAddAction?.Invoke(data);
            addAction?.Invoke(data);
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
            uidChain.PushId(data.uid);
            childRemoveAction?.Invoke(data);
            removeAction?.Invoke(data);
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
        




            public static void ChangeUid(UnitForm.Data superData,int oldV,int newV)
            {
                if(superData is Data data)
                {

                changeUidAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeName(UnitForm.Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeNameAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeItemidcountdic(Data superData,Dictionary<int,int> oldV,Dictionary<int,int> newV)
            {
                if(superData is Data data)
                {

                changeItemidcountdicAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeCurusingweaponssid(Data superData,List<int> oldV,List<int> newV)
            {
                if(superData is Data data)
                {

                changeCurusingweaponssidAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeCurreloadweaponssid(Data superData,List<int> oldV,List<int> newV)
            {
                if(superData is Data data)
                {

                changeCurreloadweaponssidAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeAlertdistance(Data superData,float oldV,float newV)
            {
                if(superData is Data data)
                {

                changeAlertdistanceAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeHp(Data superData,float oldV,float newV)
            {
                if(superData is Data data)
                {

                changeHpAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeHpmax(Data superData,float oldV,float newV)
            {
                if(superData is Data data)
                {

                changeHpmaxAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeDefence(Data superData,float oldV,float newV)
            {
                if(superData is Data data)
                {

                changeDefenceAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeTargetfightuid(Data superData,int oldV,int newV)
            {
                if(superData is Data data)
                {

                changeTargetfightuidAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeReloadtime(Data superData,float oldV,float newV)
            {
                if(superData is Data data)
                {

                changeReloadtimeAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeIsmine(Data superData,bool oldV,bool newV)
            {
                if(superData is Data data)
                {

                changeIsmineAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangePrefabname(UnitForm.Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changePrefabnameAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangePos(UnitForm.Data superData,Vector3 oldV,Vector3 newV)
            {
                if(superData is Data data)
                {

                changePosAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeEuler(UnitForm.Data superData,Vector3 oldV,Vector3 newV)
            {
                if(superData is Data data)
                {

                changeEulerAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeScale(UnitForm.Data superData,Vector3 oldV,Vector3 newV)
            {
                if(superData is Data data)
                {

                changeScaleAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeUpdatetype(UnitForm.Data superData,UpdateType oldV,UpdateType newV)
            {
                if(superData is Data data)
                {

                changeUpdatetypeAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeCollidingunituid(UnitForm.Data superData,List<int> oldV,List<int> newV)
            {
                if(superData is Data data)
                {

                changeCollidingunituidAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeExtra(UnitForm.Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeExtraAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
    }
}
        