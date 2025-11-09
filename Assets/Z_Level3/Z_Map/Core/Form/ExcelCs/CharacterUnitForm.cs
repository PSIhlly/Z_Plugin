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

        public static Action<Data> addAction;
        public static Action<Data> removeAction;
        public static Action childInitAction;
        public static Action<Data> childRemoveAction;
        public static Action<Data> childAddAction;

        public static Action<Data,int,int> changeUidAction;
                
        public static Action<Data,bool,bool> changeNavenabledAction;
                
        public static Action<Data,Vector3,Vector3> changeDestinationAction;
                
        public static Action<Data,float,float> changeSpeedAction;
                
        public static Action<Data,float,float> changeAlertdisAction;
                
        public static Action<Data,float,float> changePathdisAction;
                
        public static Action<Data,bool,bool> changeIsmineAction;
                
        public static Action<Data,string,string> changeNameAction;
                
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
                public CharacterUnit unit
                {
                    get
                    {
                        return (CharacterUnit) _unit;
                    }
                }

                    private bool  _navEnabled;
                    /// <summary>
                    ///启用
                    ///</summary>
                    public bool  navEnabled{
                                get{return _navEnabled;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeNavenabled(this,_navEnabled,value); 
                    }
        
                _navEnabled = value;
                }
                 
                     }
                    
                    private Vector3  _destination;
                    /// <summary>
                    ///目的地
                    ///</summary>
                    public Vector3  destination{
                                get{return _destination;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeDestination(this,_destination,value); 
                    }
        
                _destination = value;
                }
                 
                     }
                    
                    private float  _speed;
                    /// <summary>
                    ///速度
                    ///</summary>
                    public float  speed{
                                get{return _speed;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeSpeed(this,_speed,value); 
                    }
        
                _speed = value;
                }
                 
                     }
                    
                    private float  _alertDis;
                    /// <summary>
                    ///启动距离
                    ///</summary>
                    public float  alertDis{
                                get{return _alertDis;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeAlertdis(this,_alertDis,value); 
                    }
        
                _alertDis = value;
                }
                 
                     }
                    
                    private float  _pathDis;
                    /// <summary>
                    ///寻路距离上限
                    ///</summary>
                    public float  pathDis{
                                get{return _pathDis;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangePathdis(this,_pathDis,value); 
                    }
        
                _pathDis = value;
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
                    
            public Data(UnitForm.Data data):base(data.uid,data.name,data.prefabName,data.pos,data.euler,data.scale,data.updateType,data.extra)
            {
            }
            
            public Data(int uid,bool navEnabled,Vector3 destination,float speed,float alertDis,float pathDis,bool isMine,string name,string prefabName,Vector3 pos,Vector3 euler,Vector3 scale,UpdateType updateType,string extra):base(uid,name,prefabName,pos,euler,scale,updateType,extra)
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
             this.extra = extra;

                    _unit=new CharacterUnit(this);

            }

                public Data Copy(bool sameId = true)
                {
        return new Data(sameId? uid:uidChain.GetId(),navEnabled,destination,speed,alertDis,pathDis,isMine,name,prefabName,pos,euler,scale,updateType,extra);
                }
            
        }

                   private static Data _defaultData=new Data(0,false,Vector3.zero,0f,0f,0f,false,"","",Vector3.zero,Vector3.zero,Vector3.zero,UpdateType.ShowOnly,"");
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
            
            public static void ChangeNavenabled(Data superData,bool oldV,bool newV)
            {
                if(superData is Data data)
                {

                changeNavenabledAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeDestination(Data superData,Vector3 oldV,Vector3 newV)
            {
                if(superData is Data data)
                {

                changeDestinationAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeSpeed(Data superData,float oldV,float newV)
            {
                if(superData is Data data)
                {

                changeSpeedAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeAlertdis(Data superData,float oldV,float newV)
            {
                if(superData is Data data)
                {

                changeAlertdisAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangePathdis(Data superData,float oldV,float newV)
            {
                if(superData is Data data)
                {

                changePathdisAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeIsmine(Data superData,bool oldV,bool newV)
            {
                if(superData is Data data)
                {

                changeIsmineAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeName(UnitForm.Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeNameAction?.Invoke(data,oldV,newV);
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
        