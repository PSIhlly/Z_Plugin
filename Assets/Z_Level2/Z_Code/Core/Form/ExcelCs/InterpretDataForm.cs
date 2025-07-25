using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Z_ByteSerialize;
using Z_DesignStyle;

namespace Z_Code.Form
{

    public static partial class InterpretDataForm
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

        public static Action childInitAction;
        public static Action<Data> childRemoveAction;
        public static Action<Data> childAddAction;

        public static Action<Data,int,int> changeUidAction;
                
        public static Action<Data,List<BoxDataForm.Data>,List<BoxDataForm.Data>> changeStackAction;
                
        public static Action<Data,Dictionary<string,BoxDataForm.Data>,Dictionary<string,BoxDataForm.Data>> changeHeapAction;
                
        public static Action<Data,ProgramDataForm.Data,ProgramDataForm.Data> changeProgramAction;
                
        public static Action<Data,int,int> changePAction;
                
        public static Action<Data,int,int> changeTopAction;
                


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
                    
                    private List<BoxDataForm.Data>  _stack;
                    /// <summary>
                    ///Õ»
                    ///</summary>
                    public List<BoxDataForm.Data>  stack{
                                get{return _stack;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeStack(this,_stack,value); 
                    }
        
                _stack = value;
                }
                 
                     }
                    
                    private Dictionary<string,BoxDataForm.Data>  _heap;
                    /// <summary>
                    ///¶Ñ
                    ///</summary>
                    public Dictionary<string,BoxDataForm.Data>  heap{
                                get{return _heap;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeHeap(this,_heap,value); 
                    }
        
                _heap = value;
                }
                 
                     }
                    
                    private ProgramDataForm.Data  _program;
                    /// <summary>
                    ///³ÌÐò
                    ///</summary>
                    public ProgramDataForm.Data  program{
                                get{return _program;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeProgram(this,_program,value); 
                    }
        
                _program = value;
                }
                 
                     }
                    
                    private int  _p;
                    /// <summary>
                    ///³ÌÐò¼ÆÊýÆ÷
                    ///</summary>
                    public int  p{
                                get{return _p;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeP(this,_p,value); 
                    }
        
                _p = value;
                }
                 
                     }
                    
                    private int  _top;
                    /// <summary>
                    ///Õ»¶¥
                    ///</summary>
                    public int  top{
                                get{return _top;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeTop(this,_top,value); 
                    }
        
                _top = value;
                }
                 
                     }
                    
            public Data(int uid,List<BoxDataForm.Data> stack,Dictionary<string,BoxDataForm.Data> heap,ProgramDataForm.Data program,int p,int top)
            {

             this.uid = uid;
             this.stack = stack;
             this.heap = heap;
             this.program = program;
             this.p = p;
             this.top = top;

            }

                public Data Copy(bool sameId = true)
                {
        return new Data(sameId? uid:uidChain.GetId(),new List<BoxDataForm.Data>(stack),new Dictionary<string,BoxDataForm.Data>(heap),program,p,top);
                }
            
        }

                   private static Data _defaultData=new Data(0,null,null,null,0,0);
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

                jo.Get<List<BoxDataForm.Data>>("stack"),

                jo.Get<Dictionary<string,BoxDataForm.Data>>("heap"),

                jo.Get<ProgramDataForm.Data>("program"),

                jo.Get<int>("p"),

                jo.Get<int>("top")
                    );

            return data;
        }

        public static JObject GetJoByData(Data data)
        {
            Init();

            JObject jo=new JObject();

            jo.Set<int>("uid",data.uid);

            jo.Set<List<BoxDataForm.Data>>("stack",data.stack);

            jo.Set<Dictionary<string,BoxDataForm.Data>>("heap",data.heap);

            jo.Set<ProgramDataForm.Data>("program",data.program);

            jo.Set<int>("p",data.p);

            jo.Set<int>("top",data.top);

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
            
            public static void ChangeStack(Data superData,List<BoxDataForm.Data> oldV,List<BoxDataForm.Data> newV)
            {
                if(superData is Data data)
                {

                changeStackAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeHeap(Data superData,Dictionary<string,BoxDataForm.Data> oldV,Dictionary<string,BoxDataForm.Data> newV)
            {
                if(superData is Data data)
                {

                changeHeapAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeProgram(Data superData,ProgramDataForm.Data oldV,ProgramDataForm.Data newV)
            {
                if(superData is Data data)
                {

                changeProgramAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeP(Data superData,int oldV,int newV)
            {
                if(superData is Data data)
                {

                changePAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeTop(Data superData,int oldV,int newV)
            {
                if(superData is Data data)
                {

                changeTopAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
    }
}
        