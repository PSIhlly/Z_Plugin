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

    public static partial class ProgressForm
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
                
        public static Action<Data,float,float> changeSecondsAction;
                
        public static Action<Data,int,int> changeSceneidAction;
                
        public static Action<Data,Vector3,Vector3> changePosAction;
                
        public static Action<Data,int,int> changeCharacteruidAction;
                
        public static Action<Data,List<int>,List<int>> changeBagAction;
                
        public static Action<Data,List<int>,List<int>> changeTeamAction;
                
        public static Action<Data,List<int>,List<int>> changeTeamactiveAction;
                
        public static Action<Data,Dictionary<string,string>,Dictionary<string,string>> changeUistyleimagenameAction;
                
        public static Action<Data,Dictionary<string,EventTriggerForm.Data>,Dictionary<string,EventTriggerForm.Data>> changeEventsAction;
                
        public static Action<Data,CameraMode,CameraMode> changeCameramodeAction;
                
        public static Action<Data,ClipForm.Data,ClipForm.Data> changeDialogcacheAction;
                
        public static Action<Data,Dictionary<int,List<string>>,Dictionary<int,List<string>>> changeTriggeredonceevtsAction;
                
        public static Action<Data,bool,bool> changeNotfirsttimeAction;
                
        public static Action<Data,int,int> changeBlockprogramuidAction;
                
        public static Action<Data,EditorStyle,EditorStyle> changeEditorstyleAction;
                


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
                    
                    private float  _seconds;
                    /// <summary>
                    ///时长
                    ///</summary>
                    public float  seconds{
                                get{return _seconds;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeSeconds(this,_seconds,value); 
                    }
        
                _seconds = value;
                }
                 
                     }
                    
                    private int  _sceneId;
                    /// <summary>
                    ///玩家所处sceneId
                    ///</summary>
                    public int  sceneId{
                                get{return _sceneId;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeSceneid(this,_sceneId,value); 
                    }
        
                _sceneId = value;
                }
                 
                     }
                    
                    private Vector3  _pos;
                    /// <summary>
                    ///玩家位置
                    ///</summary>
                    public Vector3  pos{
                                get{return _pos;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangePos(this,_pos,value); 
                    }
        
                _pos = value;
                }
                 
                     }
                    
                    private int  _characterUid;
                    /// <summary>
                    ///玩家角色Uid
                    ///</summary>
                    public int  characterUid{
                                get{return _characterUid;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeCharacteruid(this,_characterUid,value); 
                    }
        
                _characterUid = value;
                }
                 
                     }
                    
                    private List<int>  _bag;
                    /// <summary>
                    ///背包（道具uid）
                    ///</summary>
                    public List<int>  bag{
                                get{return _bag;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeBag(this,_bag,value); 
                    }
        
                _bag = value;
                }
                 
                     }
                    
                    private List<int>  _team;
                    /// <summary>
                    ///队伍（人物uid）
                    ///</summary>
                    public List<int>  team{
                                get{return _team;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeTeam(this,_team,value); 
                    }
        
                _team = value;
                }
                 
                     }
                    
                    private List<int>  _teamActive;
                    /// <summary>
                    ///出战队伍（人物uid）
                    ///</summary>
                    public List<int>  teamActive{
                                get{return _teamActive;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeTeamactive(this,_teamActive,value); 
                    }
        
                _teamActive = value;
                }
                 
                     }
                    
                    private Dictionary<string,string>  _uiStyleImageName;
                    /// <summary>
                    ///ui样式图片名称
                    ///</summary>
                    public Dictionary<string,string>  uiStyleImageName{
                                get{return _uiStyleImageName;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeUistyleimagename(this,_uiStyleImageName,value); 
                    }
        
                _uiStyleImageName = value;
                }
                 
                     }
                    
                    private Dictionary<string,EventTriggerForm.Data>  _events;
                    /// <summary>
                    ///事件
                    ///</summary>
                    public Dictionary<string,EventTriggerForm.Data>  events{
                                get{return _events;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeEvents(this,_events,value); 
                    }
        
                _events = value;
                }
                 
                     }
                    
                    private CameraMode  _cameraMode;
                    /// <summary>
                    ///相机视角
                    ///</summary>
                    public CameraMode  cameraMode{
                                get{return _cameraMode;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeCameramode(this,_cameraMode,value); 
                    }
        
                _cameraMode = value;
                }
                 
                     }
                    
                    private ClipForm.Data  _dialogCache;
                    /// <summary>
                    ///对话缓存
                    ///</summary>
                    public ClipForm.Data  dialogCache{
                                get{return _dialogCache;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeDialogcache(this,_dialogCache,value); 
                    }
        
                _dialogCache = value;
                }
                 
                     }
                    
                    private Dictionary<int,List<string>>  _triggeredOnceEvts;
                    /// <summary>
                    ///触发过的一次性事件
                    ///</summary>
                    public Dictionary<int,List<string>>  triggeredOnceEvts{
                                get{return _triggeredOnceEvts;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
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

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeNotfirsttime(this,_notFirstTime,value); 
                    }
        
                _notFirstTime = value;
                }
                 
                     }
                    
                    private int  _blockProgramUid;
                    /// <summary>
                    ///阻塞的程序uid
                    ///</summary>
                    public int  blockProgramUid{
                                get{return _blockProgramUid;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeBlockprogramuid(this,_blockProgramUid,value); 
                    }
        
                _blockProgramUid = value;
                }
                 
                     }
                    
                    private EditorStyle  _editorStyle;
                    /// <summary>
                    ///编辑模式
                    ///</summary>
                    public EditorStyle  editorStyle{
                                get{return _editorStyle;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeEditorstyle(this,_editorStyle,value); 
                    }
        
                _editorStyle = value;
                }
                 
                     }
                    
            public Data(int uid,float seconds,int sceneId,Vector3 pos,int characterUid,List<int> bag,List<int> team,List<int> teamActive,Dictionary<string,string> uiStyleImageName,Dictionary<string,EventTriggerForm.Data> events,CameraMode cameraMode,ClipForm.Data dialogCache,Dictionary<int,List<string>> triggeredOnceEvts,bool notFirstTime,int blockProgramUid,EditorStyle editorStyle)
            {

             this.uid = uid;
             this.seconds = seconds;
             this.sceneId = sceneId;
             this.pos = pos;
             this.characterUid = characterUid;
             this.bag = bag;
             this.team = team;
             this.teamActive = teamActive;
             this.uiStyleImageName = uiStyleImageName;
             this.events = events;
             this.cameraMode = cameraMode;
             this.dialogCache = dialogCache;
             this.triggeredOnceEvts = triggeredOnceEvts;
             this.notFirstTime = notFirstTime;
             this.blockProgramUid = blockProgramUid;
             this.editorStyle = editorStyle;

            }
            public void Reset(Data data)
            {

             this.uid = data.uid;
             this.seconds = data.seconds;
             this.sceneId = data.sceneId;
             this.pos = data.pos;
             this.characterUid = data.characterUid;
             this.bag = data.bag;
             this.team = data.team;
             this.teamActive = data.teamActive;
             this.uiStyleImageName = data.uiStyleImageName;
             this.events = data.events;
             this.cameraMode = data.cameraMode;
             this.dialogCache = data.dialogCache;
             this.triggeredOnceEvts = data.triggeredOnceEvts;
             this.notFirstTime = data.notFirstTime;
             this.blockProgramUid = data.blockProgramUid;
             this.editorStyle = data.editorStyle;
            }

                public Data Copy(bool sameId = true)
                {
        return new Data(sameId? uid:uidChain.GetId(),seconds,sceneId,pos,characterUid,new List<int>(bag),new List<int>(team),new List<int>(teamActive),new Dictionary<string,string>(uiStyleImageName),new Dictionary<string,EventTriggerForm.Data>(events),cameraMode,dialogCache,new Dictionary<int,List<string>>(triggeredOnceEvts),notFirstTime,blockProgramUid,editorStyle);
                }
            
            public virtual  void BeforeGet()
            {
                
                ProgressForm.beforeGetAction?.Invoke(this);
            }
        }

                   private static Data _defaultData=new Data(0,0f,0,Vector3.zero,0,null,null,null,new Dictionary<string,string>(){},new Dictionary<string,EventTriggerForm.Data>(){},CameraMode.Overhead,ClipForm.defaultData,new Dictionary<int,List<string>>(){},false,0,EditorStyle.Avg);
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

                jo.SelectToken("uid")==null?defaultData.uid:jo.Get<int>("uid"),

                jo.SelectToken("seconds")==null?defaultData.seconds:jo.Get<float>("seconds"),

                jo.SelectToken("sceneId")==null?defaultData.sceneId:jo.Get<int>("sceneId"),

                jo.SelectToken("pos")==null?defaultData.pos:jo.Get<Vector3>("pos"),

                jo.SelectToken("characterUid")==null?defaultData.characterUid:jo.Get<int>("characterUid"),

                jo.SelectToken("bag")==null?defaultData.bag:jo.Get<List<int>>("bag"),

                jo.SelectToken("team")==null?defaultData.team:jo.Get<List<int>>("team"),

                jo.SelectToken("teamActive")==null?defaultData.teamActive:jo.Get<List<int>>("teamActive"),

                jo.SelectToken("uiStyleImageName")==null?defaultData.uiStyleImageName:jo.Get<Dictionary<string,string>>("uiStyleImageName"),

                jo.SelectToken("events")==null?defaultData.events:jo.Get<Dictionary<string,EventTriggerForm.Data>>("events"),

                jo.SelectToken("cameraMode")==null?defaultData.cameraMode:jo.Get<CameraMode>("cameraMode"),

                jo.SelectToken("dialogCache")==null?defaultData.dialogCache:jo.Get<ClipForm.Data>("dialogCache"),

                jo.SelectToken("triggeredOnceEvts")==null?defaultData.triggeredOnceEvts:jo.Get<Dictionary<int,List<string>>>("triggeredOnceEvts"),

                jo.SelectToken("notFirstTime")==null?defaultData.notFirstTime:jo.Get<bool>("notFirstTime"),

                jo.SelectToken("blockProgramUid")==null?defaultData.blockProgramUid:jo.Get<int>("blockProgramUid"),

                jo.SelectToken("editorStyle")==null?defaultData.editorStyle:jo.Get<EditorStyle>("editorStyle")
                    );

            return data;
        }

        public static JObject GetJoByData(Data data)
        {
            Init();
            data.BeforeGet();

            JObject jo=new JObject();

            jo.Set<int>("uid",data.uid);

            jo.Set<float>("seconds",data.seconds);

            jo.Set<int>("sceneId",data.sceneId);

            jo.Set<Vector3>("pos",data.pos);

            jo.Set<int>("characterUid",data.characterUid);

            jo.Set<List<int>>("bag",data.bag);

            jo.Set<List<int>>("team",data.team);

            jo.Set<List<int>>("teamActive",data.teamActive);

            jo.Set<Dictionary<string,string>>("uiStyleImageName",data.uiStyleImageName);

            jo.Set<Dictionary<string,EventTriggerForm.Data>>("events",data.events);

            jo.Set<CameraMode>("cameraMode",data.cameraMode);

            jo.Set<ClipForm.Data>("dialogCache",data.dialogCache);

            jo.Set<Dictionary<int,List<string>>>("triggeredOnceEvts",data.triggeredOnceEvts);

            jo.Set<bool>("notFirstTime",data.notFirstTime);

            jo.Set<int>("blockProgramUid",data.blockProgramUid);

            jo.Set<EditorStyle>("editorStyle",data.editorStyle);

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
            
            public static void ChangeSeconds(Data superData,float oldV,float newV)
            {
                if(superData is Data data)
                {

                changeSecondsAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeSceneid(Data superData,int oldV,int newV)
            {
                if(superData is Data data)
                {

                changeSceneidAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangePos(Data superData,Vector3 oldV,Vector3 newV)
            {
                if(superData is Data data)
                {

                changePosAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeCharacteruid(Data superData,int oldV,int newV)
            {
                if(superData is Data data)
                {

                changeCharacteruidAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeBag(Data superData,List<int> oldV,List<int> newV)
            {
                if(superData is Data data)
                {

                changeBagAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeTeam(Data superData,List<int> oldV,List<int> newV)
            {
                if(superData is Data data)
                {

                changeTeamAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeTeamactive(Data superData,List<int> oldV,List<int> newV)
            {
                if(superData is Data data)
                {

                changeTeamactiveAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeUistyleimagename(Data superData,Dictionary<string,string> oldV,Dictionary<string,string> newV)
            {
                if(superData is Data data)
                {

                changeUistyleimagenameAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeEvents(Data superData,Dictionary<string,EventTriggerForm.Data> oldV,Dictionary<string,EventTriggerForm.Data> newV)
            {
                if(superData is Data data)
                {

                changeEventsAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeCameramode(Data superData,CameraMode oldV,CameraMode newV)
            {
                if(superData is Data data)
                {

                changeCameramodeAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeDialogcache(Data superData,ClipForm.Data oldV,ClipForm.Data newV)
            {
                if(superData is Data data)
                {

                changeDialogcacheAction?.Invoke(data,oldV,newV);
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
            
            public static void ChangeBlockprogramuid(Data superData,int oldV,int newV)
            {
                if(superData is Data data)
                {

                changeBlockprogramuidAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeEditorstyle(Data superData,EditorStyle oldV,EditorStyle newV)
            {
                if(superData is Data data)
                {

                changeEditorstyleAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
    }
}
        