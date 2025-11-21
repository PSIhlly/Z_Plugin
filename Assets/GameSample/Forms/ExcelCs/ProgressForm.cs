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

        public static Action<Data,int,int> changeUidAction;
                
        public static Action<Data,int,int> changeSceneidAction;
                
        public static Action<Data,Vector3,Vector3> changePosAction;
                
        public static Action<Data,int,int> changeCharacteruidAction;
                
        public static Action<Data,List<int>,List<int>> changeBagAction;
                
        public static Action<Data,List<int>,List<int>> changeTeamAction;
                
        public static Action<Data,List<int>,List<int>> changeTeamactiveAction;
                
        public static Action<Data,ClipForm.Data,ClipForm.Data> changeDialogcacheAction;
                


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
                    
            public Data(int uid,int sceneId,Vector3 pos,int characterUid,List<int> bag,List<int> team,List<int> teamActive,ClipForm.Data dialogCache)
            {

             this.uid = uid;
             this.sceneId = sceneId;
             this.pos = pos;
             this.characterUid = characterUid;
             this.bag = bag;
             this.team = team;
             this.teamActive = teamActive;
             this.dialogCache = dialogCache;

            }

                public Data Copy(bool sameId = true)
                {
        return new Data(sameId? uid:uidChain.GetId(),sceneId,pos,characterUid,new List<int>(bag),new List<int>(team),new List<int>(teamActive),dialogCache);
                }
            
        }

                   private static Data _defaultData=new Data(0,0,Vector3.zero,0,null,null,null,ClipForm.defaultData);
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

                jo.Get<int>("sceneId"),

                jo.Get<Vector3>("pos"),

                jo.Get<int>("characterUid"),

                jo.Get<List<int>>("bag"),

                jo.Get<List<int>>("team"),

                jo.Get<List<int>>("teamActive"),

                jo.Get<ClipForm.Data>("dialogCache")
                    );

            return data;
        }

        public static JObject GetJoByData(Data data)
        {
            Init();

            JObject jo=new JObject();

            jo.Set<int>("uid",data.uid);

            jo.Set<int>("sceneId",data.sceneId);

            jo.Set<Vector3>("pos",data.pos);

            jo.Set<int>("characterUid",data.characterUid);

            jo.Set<List<int>>("bag",data.bag);

            jo.Set<List<int>>("team",data.team);

            jo.Set<List<int>>("teamActive",data.teamActive);

            jo.Set<ClipForm.Data>("dialogCache",data.dialogCache);

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
            
            public static void ChangeDialogcache(Data superData,ClipForm.Data oldV,ClipForm.Data newV)
            {
                if(superData is Data data)
                {

                changeDialogcacheAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
    }
}
        