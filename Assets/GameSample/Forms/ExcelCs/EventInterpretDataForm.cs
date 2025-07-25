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

    public static partial class EventInterpretDataForm
    {

        

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Register()
        {

                InterpretDataForm.childInitAction+=InitInternal;


                InterpretDataForm.childRemoveAction+=RemoveChildren;
                InterpretDataForm.childAddAction+=AddChildren;
            

            InterpretDataForm.changeUidAction+=ChangeUid;

            InterpretDataForm.changeStackAction+=ChangeStack;

            InterpretDataForm.changeHeapAction+=ChangeHeap;

            InterpretDataForm.changeProgramAction+=ChangeProgram;

            InterpretDataForm.changePAction+=ChangeP;

            InterpretDataForm.changeTopAction+=ChangeTop;

            Z_Json.extra[typeof(Data)]=((obj)=>{
            if(obj is Data data)
                return GetJoByData(data);
            return null;
            },(jo)=>{
            return GetDataByJo(jo);
            });
        }
        
        private static bool inited;

        public static Z_Chain.Chain uidChain =>InterpretDataForm.uidChain;

        public static Action childInitAction;
        public static Action<Data> childRemoveAction;
        public static Action<Data> childAddAction;

        public static Action<Data,int,int> changeUidAction;
                
        public static Action<Data,List<BoxDataForm.Data>,List<BoxDataForm.Data>> changeStackAction;
                
        public static Action<Data,Dictionary<string,BoxDataForm.Data>,Dictionary<string,BoxDataForm.Data>> changeHeapAction;
                
        public static Action<Data,ProgramDataForm.Data,ProgramDataForm.Data> changeProgramAction;
                
        public static Action<Data,int,int> changePAction;
                
        public static Action<Data,int,int> changeTopAction;
                
        public static Action<Data,int,int> changeUserAction;
                


        public partial class Data : InterpretDataForm.Data
        {

                    private int  _user;
                    /// <summary>
                    ///µ÷ÓÃÕßProductuid
                    ///</summary>
                    public int  user{
                                get{return _user;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeUser(this,_user,value); 
                    }
        
                _user = value;
                }
                 
                     }
                    
            public Data(int uid,List<BoxDataForm.Data> stack,Dictionary<string,BoxDataForm.Data> heap,ProgramDataForm.Data program,int p,int top,int user):base(uid,stack,heap,program,p,top)
            {

             this.uid = uid;
             this.stack = stack;
             this.heap = heap;
             this.program = program;
             this.p = p;
             this.top = top;
             this.user = user;

            }

                public Data Copy(bool sameId = true)
                {
        return new Data(sameId? uid:uidChain.GetId(),new List<BoxDataForm.Data>(stack),new Dictionary<string,BoxDataForm.Data>(heap),program,p,top,user);
                }
            
        }

                   private static Data _defaultData=new Data(0,null,null,null,0,0,0);
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

            InterpretDataForm.Init();

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
                InterpretDataForm.AddData(data);
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

                jo.Get<List<BoxDataForm.Data>>("stack"),

                jo.Get<Dictionary<string,BoxDataForm.Data>>("heap"),

                jo.Get<ProgramDataForm.Data>("program"),

                jo.Get<int>("p"),

                jo.Get<int>("top"),

                jo.Get<int>("user")
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

            jo.Set<int>("user",data.user);

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
    
InterpretDataForm.AddData(data);
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
    
InterpretDataForm.RemoveData(uid);
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

         private static void RemoveChildren(InterpretDataForm.Data data)
        {
            Init();
            if(data is Data)
               RemoveData(data.uid);      
        }
         private static void AddChildren(InterpretDataForm.Data superData)
        {
            Init();
            if(superData is Data data)
               AddData(data);      
        }
        




            public static void ChangeUid(InterpretDataForm.Data superData,int oldV,int newV)
            {
                if(superData is Data data)
                {

                changeUidAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeStack(InterpretDataForm.Data superData,List<BoxDataForm.Data> oldV,List<BoxDataForm.Data> newV)
            {
                if(superData is Data data)
                {

                changeStackAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeHeap(InterpretDataForm.Data superData,Dictionary<string,BoxDataForm.Data> oldV,Dictionary<string,BoxDataForm.Data> newV)
            {
                if(superData is Data data)
                {

                changeHeapAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeProgram(InterpretDataForm.Data superData,ProgramDataForm.Data oldV,ProgramDataForm.Data newV)
            {
                if(superData is Data data)
                {

                changeProgramAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeP(InterpretDataForm.Data superData,int oldV,int newV)
            {
                if(superData is Data data)
                {

                changePAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeTop(InterpretDataForm.Data superData,int oldV,int newV)
            {
                if(superData is Data data)
                {

                changeTopAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeUser(Data superData,int oldV,int newV)
            {
                if(superData is Data data)
                {

                changeUserAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
    }
}
        