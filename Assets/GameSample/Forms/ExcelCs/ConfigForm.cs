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

    public static partial class ConfigForm
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
                
        public static Action<Data,int,int> changeStartsceneidAction;
                
        public static Action<Data,Vector3,Vector3> changeStartposAction;
                
        public static Action<Data,string,string> changeMaincharacternameAction;
                
        public static Action<Data,List<int>,List<int>> changeDefaultbagAction;
                
        public static Action<Data,string,string> changeOnbegineventAction;
                
        public static Action<Data,string,string> changeOnendeventAction;
                


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
                    
                    private int  _startSceneId;
                    /// <summary>
                    ///玩家初始sceneId
                    ///</summary>
                    public int  startSceneId{
                                get{return _startSceneId;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeStartsceneid(this,_startSceneId,value); 
                    }
        
                _startSceneId = value;
                }
                 
                     }
                    
                    private Vector3  _startpos;
                    /// <summary>
                    ///玩家初始位置
                    ///</summary>
                    public Vector3  startpos{
                                get{return _startpos;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeStartpos(this,_startpos,value); 
                    }
        
                _startpos = value;
                }
                 
                     }
                    
                    private string  _mainCharacterName;
                    /// <summary>
                    ///玩家初始角色名
                    ///</summary>
                    public string  mainCharacterName{
                                get{return _mainCharacterName;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeMaincharactername(this,_mainCharacterName,value); 
                    }
        
                _mainCharacterName = value;
                }
                 
                     }
                    
                    private List<int>  _defaultBag;
                    /// <summary>
                    ///玩家初始背包
                    ///</summary>
                    public List<int>  defaultBag{
                                get{return _defaultBag;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeDefaultbag(this,_defaultBag,value); 
                    }
        
                _defaultBag = value;
                }
                 
                     }
                    
                    private string  _onBeginEvent;
                    /// <summary>
                    ///开始事件
                    ///</summary>
                    public string  onBeginEvent{
                                get{return _onBeginEvent;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeOnbeginevent(this,_onBeginEvent,value); 
                    }
        
                _onBeginEvent = value;
                }
                 
                     }
                    
                    private string  _onEndEvent;
                    /// <summary>
                    ///结束事件
                    ///</summary>
                    public string  onEndEvent{
                                get{return _onEndEvent;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeOnendevent(this,_onEndEvent,value); 
                    }
        
                _onEndEvent = value;
                }
                 
                     }
                    
            public Data(int uid,int startSceneId,Vector3 startpos,string mainCharacterName,List<int> defaultBag,string onBeginEvent,string onEndEvent)
            {

             this.uid = uid;
             this.startSceneId = startSceneId;
             this.startpos = startpos;
             this.mainCharacterName = mainCharacterName;
             this.defaultBag = defaultBag;
             this.onBeginEvent = onBeginEvent;
             this.onEndEvent = onEndEvent;

            }

                public Data Copy(bool sameId = true)
                {
        return new Data(sameId? uid:uidChain.GetId(),startSceneId,startpos,mainCharacterName,new List<int>(defaultBag),onBeginEvent,onEndEvent);
                }
            
        }

                   private static Data _defaultData=new Data(0,0,Vector3.zero,"",null,"","");
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

                jo.Get<int>("startSceneId"),

                jo.Get<Vector3>("startpos"),

                jo.Get<string>("mainCharacterName"),

                jo.Get<List<int>>("defaultBag"),

                jo.Get<string>("onBeginEvent"),

                jo.Get<string>("onEndEvent")
                    );

            return data;
        }

        public static JObject GetJoByData(Data data)
        {
            Init();

            JObject jo=new JObject();

            jo.Set<int>("uid",data.uid);

            jo.Set<int>("startSceneId",data.startSceneId);

            jo.Set<Vector3>("startpos",data.startpos);

            jo.Set<string>("mainCharacterName",data.mainCharacterName);

            jo.Set<List<int>>("defaultBag",data.defaultBag);

            jo.Set<string>("onBeginEvent",data.onBeginEvent);

            jo.Set<string>("onEndEvent",data.onEndEvent);

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
            
            public static void ChangeStartsceneid(Data superData,int oldV,int newV)
            {
                if(superData is Data data)
                {

                changeStartsceneidAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeStartpos(Data superData,Vector3 oldV,Vector3 newV)
            {
                if(superData is Data data)
                {

                changeStartposAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeMaincharactername(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeMaincharacternameAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeDefaultbag(Data superData,List<int> oldV,List<int> newV)
            {
                if(superData is Data data)
                {

                changeDefaultbagAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeOnbeginevent(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeOnbegineventAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeOnendevent(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeOnendeventAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
    }
}
        