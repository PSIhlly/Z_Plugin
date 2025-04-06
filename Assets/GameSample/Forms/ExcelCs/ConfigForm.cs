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

namespace Form
{

    public static partial class ConfigForm
    {
public static readonly int autoUidCnt=100;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Register()
        {



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
                    
            public Data(int uid,int startSceneId,Vector3 startpos,string mainCharacterName)
            {

             this.uid = uid;
             this.startSceneId = startSceneId;
             this.startpos = startpos;
             this.mainCharacterName = mainCharacterName;

            }
            
        }

                   public static Data defaultData=new Data(0,0,Vector3.zero,"");


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

                jo.Get<string>("mainCharacterName")
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
    

            childRemoveAction?.Invoke(data);
        }
        public static void Clear()
        {
            Init();

                    DataByUid.Clear();
    
            uidChain.Clear();
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
            
    }
}
        