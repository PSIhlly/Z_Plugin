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
                
        public static Action<Data,int,int> changeMaincharacteruidAction;
                
        public static Action<Data,List<int>,List<int>> changeDefaultteamAction;
                
        public static Action<Data,List<int>,List<int>> changeDefaultteamactiveAction;
                
        public static Action<Data,List<int>,List<int>> changeDefaultbagAction;
                
        public static Action<Data,string,string> changeOnbegineventAction;
                
        public static Action<Data,string,string> changeOnendeventAction;
                
        public static Action<Data,string,string> changeMinimapAction;
                


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
                    
                    private int  _mainCharacterUid;
                    /// <summary>
                    ///玩家初始角色名
                    ///</summary>
                    public int  mainCharacterUid{
                                get{return _mainCharacterUid;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeMaincharacteruid(this,_mainCharacterUid,value); 
                    }
        
                _mainCharacterUid = value;
                }
                 
                     }
                    
                    private List<int>  _defaultTeam;
                    /// <summary>
                    ///玩家初始队伍
                    ///</summary>
                    public List<int>  defaultTeam{
                                get{return _defaultTeam;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeDefaultteam(this,_defaultTeam,value); 
                    }
        
                _defaultTeam = value;
                }
                 
                     }
                    
                    private List<int>  _defaultTeamActive;
                    /// <summary>
                    ///玩家初始出战队伍
                    ///</summary>
                    public List<int>  defaultTeamActive{
                                get{return _defaultTeamActive;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeDefaultteamactive(this,_defaultTeamActive,value); 
                    }
        
                _defaultTeamActive = value;
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
                    
                    private string  _miniMap;
                    /// <summary>
                    ///小地图
                    ///</summary>
                    public string  miniMap{
                                get{return _miniMap;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeMinimap(this,_miniMap,value); 
                    }
        
                _miniMap = value;
                }
                 
                     }
                    
            public Data(int uid,int startSceneId,Vector3 startpos,int mainCharacterUid,List<int> defaultTeam,List<int> defaultTeamActive,List<int> defaultBag,string onBeginEvent,string onEndEvent,string miniMap)
            {

             this.uid = uid;
             this.startSceneId = startSceneId;
             this.startpos = startpos;
             this.mainCharacterUid = mainCharacterUid;
             this.defaultTeam = defaultTeam;
             this.defaultTeamActive = defaultTeamActive;
             this.defaultBag = defaultBag;
             this.onBeginEvent = onBeginEvent;
             this.onEndEvent = onEndEvent;
             this.miniMap = miniMap;

            }

                public Data Copy(bool sameId = true)
                {
        return new Data(sameId? uid:uidChain.GetId(),startSceneId,startpos,mainCharacterUid,new List<int>(defaultTeam),new List<int>(defaultTeamActive),new List<int>(defaultBag),onBeginEvent,onEndEvent,miniMap);
                }
            
        }

                   private static Data _defaultData=new Data(0,0,Vector3.zero,0,null,null,null,"","","");
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

                jo.Get<int>("mainCharacterUid"),

                jo.Get<List<int>>("defaultTeam"),

                jo.Get<List<int>>("defaultTeamActive"),

                jo.Get<List<int>>("defaultBag"),

                jo.Get<string>("onBeginEvent"),

                jo.Get<string>("onEndEvent"),

                jo.Get<string>("miniMap")
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

            jo.Set<int>("mainCharacterUid",data.mainCharacterUid);

            jo.Set<List<int>>("defaultTeam",data.defaultTeam);

            jo.Set<List<int>>("defaultTeamActive",data.defaultTeamActive);

            jo.Set<List<int>>("defaultBag",data.defaultBag);

            jo.Set<string>("onBeginEvent",data.onBeginEvent);

            jo.Set<string>("onEndEvent",data.onEndEvent);

            jo.Set<string>("miniMap",data.miniMap);

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
            
            public static void ChangeMaincharacteruid(Data superData,int oldV,int newV)
            {
                if(superData is Data data)
                {

                changeMaincharacteruidAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeDefaultteam(Data superData,List<int> oldV,List<int> newV)
            {
                if(superData is Data data)
                {

                changeDefaultteamAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeDefaultteamactive(Data superData,List<int> oldV,List<int> newV)
            {
                if(superData is Data data)
                {

                changeDefaultteamactiveAction?.Invoke(data,oldV,newV);
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
            
            public static void ChangeMinimap(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeMinimapAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
    }
}
        