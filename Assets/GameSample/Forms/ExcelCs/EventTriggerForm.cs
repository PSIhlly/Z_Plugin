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

    public static partial class EventTriggerForm
    {
public static readonly int autoUidCnt=1000000;

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
                
        public static Action<Data,List<string>,List<string>> changeEvtAction;
                
        public static Action<Data,TriggerType,TriggerType> changeTypeAction;
                


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
                    ///名称
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
                    
                    private List<string>  _evt;
                    /// <summary>
                    ///事件名称
                    ///</summary>
                    public List<string>  evt{
                                get{return _evt;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeEvt(this,_evt,value); 
                    }
        
                _evt = value;
                }
                 
                     }
                    
                    private TriggerType  _type;
                    /// <summary>
                    ///触发类型
                    ///</summary>
                    public TriggerType  type{
                                get{return _type;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeType(this,_type,value); 
                    }
        
                _type = value;
                }
                 
                     }
                    
            public Data(int uid,string name,List<string> evt,TriggerType type)
            {

             this.uid = uid;
             this.name = name;
             this.evt = evt;
             this.type = type;

            }
            public void Reset(Data data)
            {

             this.uid = data.uid;
             this.name = data.name;
             this.evt = data.evt;
             this.type = data.type;
            }

                public Data Copy(bool sameId = true)
                {
        return new Data(sameId? uid:uidChain.GetId(),name,evt==null?new List<string>():new List<string>(evt),type);
                }
            
            public virtual  void BeforeGet()
            {
                
                EventTriggerForm.beforeGetAction?.Invoke(this);
            }
        }

                   private static Data _defaultData=new Data(0,"",new List<string>(),default);
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

                jo.SelectToken("evt")==null?defaultData.evt:jo.Get<List<string>>("evt"),

                jo.SelectToken("type")==null?defaultData.type:jo.Get<TriggerType>("type")
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

            jo.Set<List<string>>("evt",data.evt);

            jo.Set<TriggerType>("type",data.type);

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
            
            public static void ChangeEvt(Data superData,List<string> oldV,List<string> newV)
            {
                if(superData is Data data)
                {

                changeEvtAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeType(Data superData,TriggerType oldV,TriggerType newV)
            {
                if(superData is Data data)
                {

                changeTypeAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
    }
}
        