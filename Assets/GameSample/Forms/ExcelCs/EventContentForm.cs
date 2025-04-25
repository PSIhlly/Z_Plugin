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

namespace Form
{

    public static partial class EventContentForm
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
                
        public static Action<Data,EventForm.Data,EventForm.Data> changeEvtAction;
                
        public static Action<Data,int,int> changeCurAction;
                
        public static Action<Data,List<VarForm.Data>,List<VarForm.Data>> changeStackAction;
                


        public partial class Data
        {

                protected EventContentController _ctrl;

                /// <summary>
                ///逻辑
                ///</summary>
                public EventContentController ctrl
                {
                    get
                    {
                        return (EventContentController) _ctrl;
                    }
                }

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
                    
                    private EventForm.Data  _evt;
                    /// <summary>
                    ///事件全部event
                    ///</summary>
                    public EventForm.Data  evt{
                                get{return _evt;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeEvt(this,_evt,value); 
                    }
        
                _evt = value;
                }
                 
                     }
                    
                    private int  _cur;
                    /// <summary>
                    ///程序计数器
                    ///</summary>
                    public int  cur{
                                get{return _cur;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeCur(this,_cur,value); 
                    }
        
                _cur = value;
                }
                 
                     }
                    
                    private List<VarForm.Data>  _stack;
                    /// <summary>
                    ///结果栈
                    ///</summary>
                    public List<VarForm.Data>  stack{
                                get{return _stack;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeStack(this,_stack,value); 
                    }
        
                _stack = value;
                }
                 
                     }
                    
            public Data(int uid,EventForm.Data evt,int cur,List<VarForm.Data> stack)
            {

             this.uid = uid;
             this.evt = evt;
             this.cur = cur;
             this.stack = stack;

                    _ctrl=new EventContentController(this);

            }

                public Data Copy()
                {
        return new Data(-1,evt,cur,stack);
                }
            
        }

                   public static Data defaultData=new Data(0,null,0,null);


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

                jo.Get<EventForm.Data>("evt"),

                jo.Get<int>("cur"),

                jo.Get<List<VarForm.Data>>("stack")
                    );

            return data;
        }

        public static JObject GetJoByData(Data data)
        {
            Init();

            JObject jo=new JObject();

            jo.Set<int>("uid",data.uid);

            jo.Set<EventForm.Data>("evt",data.evt);

            jo.Set<int>("cur",data.cur);

            jo.Set<List<VarForm.Data>>("stack",data.stack);

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
            
            public static void ChangeEvt(Data superData,EventForm.Data oldV,EventForm.Data newV)
            {
                if(superData is Data data)
                {

                changeEvtAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeCur(Data superData,int oldV,int newV)
            {
                if(superData is Data data)
                {

                changeCurAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeStack(Data superData,List<VarForm.Data> oldV,List<VarForm.Data> newV)
            {
                if(superData is Data data)
                {

                changeStackAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
    }
}
        