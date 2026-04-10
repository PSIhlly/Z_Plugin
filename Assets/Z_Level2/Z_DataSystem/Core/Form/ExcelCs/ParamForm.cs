using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Z_ByteSerialize;
using Z_DesignStyle;

namespace Z_DataSystem.Form
{

    public static partial class ParamForm
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
                
        public static Action<Data,ValType,ValType> changeValuetypeAction;
                
        public static Action<Data,string,string> changeMinAction;
                
        public static Action<Data,string,string> changeVAction;
                
        public static Action<Data,string,string> changeMaxAction;
                


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
                    
                    private ValType  _valueType;
                    /// <summary>
                    ///数据类型
                    ///</summary>
                    public ValType  valueType{
                                get{return _valueType;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeValuetype(this,_valueType,value); 
                    }
        
                _valueType = value;
                }
                 
                     }
                    
                    private string  _min;
                    /// <summary>
                    ///最小值
                    ///</summary>
                    public string  min{
                                get{return _min;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeMin(this,_min,value); 
                    }
        
                _min = value;
                }
                 
                     }
                    
                    private string  _v;
                    /// <summary>
                    ///当前值
                    ///</summary>
                    public string  v{
                                get{return _v;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeV(this,_v,value); 
                    }
        
                _v = value;
                }
                 
                     }
                    
                    private string  _max;
                    /// <summary>
                    ///最大值
                    ///</summary>
                    public string  max{
                                get{return _max;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeMax(this,_max,value); 
                    }
        
                _max = value;
                }
                 
                     }
                    
            public Data(int uid,string name,ValType valueType,string min,string v,string max)
            {

             this.uid = uid;
             this.name = name;
             this.valueType = valueType;
             this.min = min;
             this.v = v;
             this.max = max;

            }
            public void Reset(Data data)
            {

             this.uid = data.uid;
             this.name = data.name;
             this.valueType = data.valueType;
             this.min = data.min;
             this.v = data.v;
             this.max = data.max;
            }

                public Data Copy(bool sameId = true)
                {
        return new Data(sameId? uid:uidChain.GetId(),name,valueType,min,v,max);
                }
            
            public virtual  void BeforeGet()
            {
                
                ParamForm.beforeGetAction?.Invoke(this);
            }
        }

                   private static Data _defaultData=new Data(0,"",default,"","","");
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

                jo.SelectToken("valueType")==null?defaultData.valueType:jo.Get<ValType>("valueType"),

                jo.SelectToken("min")==null?defaultData.min:jo.Get<string>("min"),

                jo.SelectToken("v")==null?defaultData.v:jo.Get<string>("v"),

                jo.SelectToken("max")==null?defaultData.max:jo.Get<string>("max")
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

            jo.Set<ValType>("valueType",data.valueType);

            jo.Set<string>("min",data.min);

            jo.Set<string>("v",data.v);

            jo.Set<string>("max",data.max);

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

                changeNameAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeValuetype(Data superData,ValType oldV,ValType newV)
            {
                if(superData is Data data)
                {

                changeValuetypeAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeMin(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeMinAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeV(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeVAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeMax(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeMaxAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
    }
}
        