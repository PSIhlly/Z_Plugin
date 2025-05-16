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
                UnitForm.childAddAction+=AddChildren;
            

            UnitForm.changeUidAction+=ChangeUid;

            UnitForm.changeNameAction+=ChangeName;

            UnitForm.changePrefabnameAction+=ChangePrefabname;

            UnitForm.changePosAction+=ChangePos;

            UnitForm.changeEulerAction+=ChangeEuler;

            UnitForm.changeScaleAction+=ChangeScale;

            UnitForm.changeUpdatetypeAction+=ChangeUpdatetype;

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

        public static Action childInitAction;
        public static Action<Data> childRemoveAction;
        public static Action<Data> childAddAction;

        public static Action<Data,int,int> changeUidAction;
                
        public static Action<Data,string,string> changeNameAction;
                
        public static Action<Data,int,int> changeFightuidAction;
                
        public static Action<Data,List<int>,List<int>> changeWeaponbulletsidAction;
                
        public static Action<Data,int,int> changeCurweaponbulletaidAction;
                
        public static Action<Data,float,float> changeCdremainAction;
                
        public static Action<Data,int,int> changeMagazineremainAction;
                
        public static Action<Data,string,string> changePrefabnameAction;
                
        public static Action<Data,Vector3,Vector3> changePosAction;
                
        public static Action<Data,Vector3,Vector3> changeEulerAction;
                
        public static Action<Data,Vector3,Vector3> changeScaleAction;
                
        public static Action<Data,UpdateType,UpdateType> changeUpdatetypeAction;
                
        public static Action<Data,string,string> changeExtraAction;
                


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

                    private int  _fightUid;
                    /// <summary>
                    ///持有者
                    ///</summary>
                    public int  fightUid{
                                get{return _fightUid;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeFightuid(this,_fightUid,value); 
                    }
        
                _fightUid = value;
                }
                 
                     }
                    
                    private List<int>  _weaponBulletsId;
                    /// <summary>
                    ///子弹类型列表
                    ///</summary>
                    public List<int>  weaponBulletsId{
                                get{return _weaponBulletsId;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeWeaponbulletsid(this,_weaponBulletsId,value); 
                    }
        
                _weaponBulletsId = value;
                }
                 
                     }
                    
                    private int  _curWeaponBulletAid;
                    /// <summary>
                    ///当前使用子弹id
                    ///</summary>
                    public int  curWeaponBulletAid{
                                get{return _curWeaponBulletAid;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeCurweaponbulletaid(this,_curWeaponBulletAid,value); 
                    }
        
                _curWeaponBulletAid = value;
                }
                 
                     }
                    
                    private float  _cdRemain;
                    /// <summary>
                    ///射速冷却时长余剩
                    ///</summary>
                    public float  cdRemain{
                                get{return _cdRemain;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeCdremain(this,_cdRemain,value); 
                    }
        
                _cdRemain = value;
                }
                 
                     }
                    
                    private int  _magazineRemain;
                    /// <summary>
                    ///弹夹余剩
                    ///</summary>
                    public int  magazineRemain{
                                get{return _magazineRemain;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeMagazineremain(this,_magazineRemain,value); 
                    }
        
                _magazineRemain = value;
                }
                 
                     }
                    
            public Data(int uid,string name,int fightUid,List<int> weaponBulletsId,int curWeaponBulletAid,float cdRemain,int magazineRemain,string prefabName,Vector3 pos,Vector3 euler,Vector3 scale,UpdateType updateType,string extra):base(uid,name,prefabName,pos,euler,scale,updateType,extra)
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
             this.extra = extra;

                    _unit=new WeaponUnit(this);

            }

                public Data Copy(bool sameId = true)
                {
        return new Data(sameId? uid:uidChain.GetId(),name,fightUid,new List<int>(weaponBulletsId),curWeaponBulletAid,cdRemain,magazineRemain,prefabName,pos,euler,scale,updateType,extra);
                }
            
        }

                   private static Data _defaultData=new Data(0,"",0,null,0,0f,0,"",Vector3.zero,Vector3.zero,Vector3.zero,UpdateType.ShowOnly,"");
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

                jo.Get<int>("fightUid"),

                jo.Get<List<int>>("weaponBulletsId"),

                jo.Get<int>("curWeaponBulletAid"),

                jo.Get<float>("cdRemain"),

                jo.Get<int>("magazineRemain"),

                jo.Get<string>("prefabName"),

                jo.Get<Vector3>("pos"),

                jo.Get<Vector3>("euler"),

                jo.Get<Vector3>("scale"),

                jo.Get<UpdateType>("updateType"),

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

            jo.Set<int>("fightUid",data.fightUid);

            jo.Set<List<int>>("weaponBulletsId",data.weaponBulletsId);

            jo.Set<int>("curWeaponBulletAid",data.curWeaponBulletAid);

            jo.Set<float>("cdRemain",data.cdRemain);

            jo.Set<int>("magazineRemain",data.magazineRemain);

            jo.Set<string>("prefabName",data.prefabName);

            jo.Set<Vector3>("pos",data.pos);

            jo.Set<Vector3>("euler",data.euler);

            jo.Set<Vector3>("scale",data.scale);

            jo.Set<UpdateType>("updateType",data.updateType);

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
        }
        public static void Clear()
        {
            Init();

                    DataByUid.Clear();
    
            uidChain.Clear();
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
            
            public static void ChangeFightuid(Data superData,int oldV,int newV)
            {
                if(superData is Data data)
                {

                changeFightuidAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeWeaponbulletsid(Data superData,List<int> oldV,List<int> newV)
            {
                if(superData is Data data)
                {

                changeWeaponbulletsidAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeCurweaponbulletaid(Data superData,int oldV,int newV)
            {
                if(superData is Data data)
                {

                changeCurweaponbulletaidAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeCdremain(Data superData,float oldV,float newV)
            {
                if(superData is Data data)
                {

                changeCdremainAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeMagazineremain(Data superData,int oldV,int newV)
            {
                if(superData is Data data)
                {

                changeMagazineremainAction?.Invoke(data,oldV,newV);
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
            
            public static void ChangeExtra(UnitForm.Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeExtraAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
    }
}
        