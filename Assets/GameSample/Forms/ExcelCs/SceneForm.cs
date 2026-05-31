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
using Z_Text.Form;
using Z_DataSystem.Form;
using Z_Map.Form;
using Z_Map;
using Z_Ui.Form;
using Z_Code.Form;

namespace Form
{

    public static partial class SceneForm
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

        public static Action<Data> addAction;
        public static Action<Data> removeAction;
        public static Action childInitAction;
        public static Action<Data> childRemoveAction;
        public static Action<Data> childAddAction;
        
        public static Action<Data> beforeGetAction;

        public static Action<Data,int,int> changeUidAction;
                
        public static Action<Data,string,string> changeNameAction;
                
        public static Action<Data,string,string> changeMinimapAction;
                
        public static Action<Data,Vector2,Vector2> changePosAction;
                
        public static Action<Data,bool,bool> changeUnlockAction;
                
        public static Action<Data,bool,bool> changeHideinlargemapAction;
                
        public static Action<Data,Dictionary<string,EventTriggerForm.Data>,Dictionary<string,EventTriggerForm.Data>> changeEventsAction;
                
        public static Action<Data,Dictionary<int,List<string>>,Dictionary<int,List<string>>> changeTriggeredonceevtsAction;
                
        public static Action<Data,bool,bool> changeNotfirsttimeAction;
                


        public partial class Data
        {

                    private int  _uid;
                    /// <summary>
                    ///
                    ///</summary>
                    public int  uid{
                                get{return _uid;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeUid(this,_uid,value); 
                    }
        
                _uid = value;
                }
                 
                     }
                    
                    private string  _name;
                    /// <summary>
                    ///名字
                    ///</summary>
                    public string  name{
                                get{return _name;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeName(this,_name,value); 
                    }
        
                _name = value;
                }
                 
                     }
                    
                    private string  _miniMap;
                    /// <summary>
                    ///小地图
                    ///</summary>
                    public string  miniMap{
                                get{return _miniMap;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeMinimap(this,_miniMap,value); 
                    }
        
                _miniMap = value;
                }
                 
                     }
                    
                    private Vector2  _pos;
                    /// <summary>
                    ///相对位置
                    ///</summary>
                    public Vector2  pos{
                                get{return _pos;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangePos(this,_pos,value); 
                    }
        
                _pos = value;
                }
                 
                     }
                    
                    private bool  _unlock;
                    /// <summary>
                    ///解锁
                    ///</summary>
                    public bool  unlock{
                                get{return _unlock;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeUnlock(this,_unlock,value); 
                    }
        
                _unlock = value;
                }
                 
                     }
                    
                    private bool  _hideInLargeMap;
                    /// <summary>
                    ///大地图隐藏
                    ///</summary>
                    public bool  hideInLargeMap{
                                get{return _hideInLargeMap;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeHideinlargemap(this,_hideInLargeMap,value); 
                    }
        
                _hideInLargeMap = value;
                }
                 
                     }
                    
                    private Dictionary<string,EventTriggerForm.Data>  _events;
                    /// <summary>
                    ///事件
                    ///</summary>
                    public Dictionary<string,EventTriggerForm.Data>  events{
                                get{return _events;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeEvents(this,_events,value); 
                    }
        
                _events = value;
                }
                 
                     }
                    
                    private Dictionary<int,List<string>>  _triggeredOnceEvts;
                    /// <summary>
                    ///触发过的一次性事件
                    ///</summary>
                    public Dictionary<int,List<string>>  triggeredOnceEvts{
                                get{return _triggeredOnceEvts;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeTriggeredonceevts(this,_triggeredOnceEvts,value); 
                    }
        
                _triggeredOnceEvts = value;
                }
                 
                     }
                    
                    private bool  _notFirstTime;
                    /// <summary>
                    ///非第一次进入
                    ///</summary>
                    public bool  notFirstTime{
                                get{return _notFirstTime;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeNotfirsttime(this,_notFirstTime,value); 
                    }
        
                _notFirstTime = value;
                }
                 
                     }
                    
            public Data(int uid,string name,string miniMap,Vector2 pos,bool unlock,bool hideInLargeMap,Dictionary<string,EventTriggerForm.Data> events,Dictionary<int,List<string>> triggeredOnceEvts,bool notFirstTime)
            {

             this.uid = uid;
             this.name = name;
             this.miniMap = miniMap;
             this.pos = pos;
             this.unlock = unlock;
             this.hideInLargeMap = hideInLargeMap;
             this.events = events;
             this.triggeredOnceEvts = triggeredOnceEvts;
             this.notFirstTime = notFirstTime;

            }
            public void Reset(Data data)
            {

             this.uid = data.uid;
             this.name = data.name;
             this.miniMap = data.miniMap;
             this.pos = data.pos;
             this.unlock = data.unlock;
             this.hideInLargeMap = data.hideInLargeMap;
             this.events = data.events;
             this.triggeredOnceEvts = data.triggeredOnceEvts;
             this.notFirstTime = data.notFirstTime;
            }

                public Data Copy(bool sameId = true)
                {
        return new Data(sameId? uid:uidChain.GetId(),name,miniMap,pos,unlock,hideInLargeMap,events==null?new Dictionary<string,EventTriggerForm.Data>():new Dictionary<string,EventTriggerForm.Data>(events),triggeredOnceEvts==null?new Dictionary<int,List<string>>():new Dictionary<int,List<string>>(triggeredOnceEvts),notFirstTime);
                }
            
            public virtual  void BeforeGet()
            {
                
                SceneForm.beforeGetAction?.Invoke(this);
            }
        }

                   private static Data _defaultData=new Data(0,"","",Vector2.zero,false,false,new Dictionary<string,EventTriggerForm.Data>(){},new Dictionary<int,List<string>>(){},false);
                   public static Data defaultData=>_defaultData.Copy();


            static HashSet<Data> _DatasHashSet;
            static Dictionary<int, Data> _DataByUid;
            public static Dictionary<int, Data> DataByUid
            {
                get
                {
                    Init();
                    return _DataByUid;
                }
            }
    
            static Dictionary<string, Data> _DataByName;
            public static Dictionary<string, Data> DataByName
            {
                get
                {
                    Init();
                    return _DataByName;
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
                _DatasHashSet=new HashSet<Data>();
                
                    _DataByName = new Dictionary<string, Data>() {
    
                    
                    };
                    foreach(var v in _DataByUid.Values)
                    {
                        _DatasHashSet.Add(v);
                    }
    

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

                jo.SelectToken("uid")==null?defaultData.uid:jo.Get<int>("uid"),

                jo.SelectToken("name")==null?defaultData.name:jo.Get<string>("name"),

                jo.SelectToken("miniMap")==null?defaultData.miniMap:jo.Get<string>("miniMap"),

                jo.SelectToken("pos")==null?defaultData.pos:jo.Get<Vector2>("pos"),

                jo.SelectToken("unlock")==null?defaultData.unlock:jo.Get<bool>("unlock"),

                jo.SelectToken("hideInLargeMap")==null?defaultData.hideInLargeMap:jo.Get<bool>("hideInLargeMap"),

                jo.SelectToken("events")==null?defaultData.events:jo.Get<Dictionary<string,EventTriggerForm.Data>>("events"),

                jo.SelectToken("triggeredOnceEvts")==null?defaultData.triggeredOnceEvts:jo.Get<Dictionary<int,List<string>>>("triggeredOnceEvts"),

                jo.SelectToken("notFirstTime")==null?defaultData.notFirstTime:jo.Get<bool>("notFirstTime")
                    );

            return data;
        }

        public static JObject GetJoByData(Data data)
        {
            Init();
            data.BeforeGet();

            JObject jo=new JObject();

            jo.Set<int>("uid",data.uid);

            jo.Set<string>("name",data.name);

            jo.Set<string>("miniMap",data.miniMap);

            jo.Set<Vector2>("pos",data.pos);

            jo.Set<bool>("unlock",data.unlock);

            jo.Set<bool>("hideInLargeMap",data.hideInLargeMap);

            jo.Set<Dictionary<string,EventTriggerForm.Data>>("events",data.events);

            jo.Set<Dictionary<int,List<string>>>("triggeredOnceEvts",data.triggeredOnceEvts);

            jo.Set<bool>("notFirstTime",data.notFirstTime);

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
        _DatasHashSet.Add(data);
    
                    DataByName[data.name]=data;
    

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

                    _DatasHashSet.Remove(DataByUid[data.uid]);
                    DataByUid.Remove(data.uid);
                    
    
                    DataByName.Remove(data.name);
    

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
                    {
                        RemoveData(key);
                    }
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
            
            public static void ChangeName(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                    DataByName.Remove(oldV);
                    DataByName[newV]=data;
 
                changeNameAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeMinimap(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeMinimapAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangePos(Data superData,Vector2 oldV,Vector2 newV)
            {
                if(superData is Data data)
                {

                changePosAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeUnlock(Data superData,bool oldV,bool newV)
            {
                if(superData is Data data)
                {

                changeUnlockAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeHideinlargemap(Data superData,bool oldV,bool newV)
            {
                if(superData is Data data)
                {

                changeHideinlargemapAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeEvents(Data superData,Dictionary<string,EventTriggerForm.Data> oldV,Dictionary<string,EventTriggerForm.Data> newV)
            {
                if(superData is Data data)
                {

                changeEventsAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeTriggeredonceevts(Data superData,Dictionary<int,List<string>> oldV,Dictionary<int,List<string>> newV)
            {
                if(superData is Data data)
                {

                changeTriggeredonceevtsAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeNotfirsttime(Data superData,bool oldV,bool newV)
            {
                if(superData is Data data)
                {

                changeNotfirsttimeAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
    }
}
        