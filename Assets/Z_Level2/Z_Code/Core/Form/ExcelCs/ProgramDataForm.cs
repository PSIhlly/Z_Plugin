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

    public static partial class ProgramDataForm
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

        public static Action<Data,int,int> changeUidAction;
                
        public static Action<Data,string,string> changeNameAction;
                
        public static Action<Data,string,string> changeCodeAction;
                
        public static Action<Data,List<string>,List<string>> changeZcodeAction;
                
        public static Action<Data,int,int> changeParamcountAction;
                
        public static Action<Data,string,string> changeReturnvalueAction;
                


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
                    
                    private string  _name;
                    /// <summary>
                    ///名称
                    ///</summary>
                    public string  name{
                                get{return _name;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeName(this,_name,value); 
                    }
        
                _name = value;
                }
                 
                     }
                    
                    private string  _code;
                    /// <summary>
                    ///源代码
                    ///</summary>
                    public string  code{
                                get{return _code;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeCode(this,_code,value); 
                    }
        
                _code = value;
                }
                 
                     }
                    
                    private List<string>  _zCode;
                    /// <summary>
                    ///z代码
                    ///</summary>
                    public List<string>  zCode{
                                get{return _zCode;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeZcode(this,_zCode,value); 
                    }
        
                _zCode = value;
                }
                 
                     }
                    
                    private int  _paramCount;
                    /// <summary>
                    ///参数数量
                    ///</summary>
                    public int  paramCount{
                                get{return _paramCount;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeParamcount(this,_paramCount,value); 
                    }
        
                _paramCount = value;
                }
                 
                     }
                    
                    private string  _returnValue;
                    /// <summary>
                    ///返回值
                    ///</summary>
                    public string  returnValue{
                                get{return _returnValue;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeReturnvalue(this,_returnValue,value); 
                    }
        
                _returnValue = value;
                }
                 
                     }
                    
            public Data(int uid,string name,string code,List<string> zCode,int paramCount,string returnValue)
            {

             this.uid = uid;
             this.name = name;
             this.code = code;
             this.zCode = zCode;
             this.paramCount = paramCount;
             this.returnValue = returnValue;

            }
            public void Reset(Data data)
            {

             this.uid = data.uid;
             this.name = data.name;
             this.code = data.code;
             this.zCode = data.zCode;
             this.paramCount = data.paramCount;
             this.returnValue = data.returnValue;
            }

                public Data Copy(bool sameId = true)
                {
        return new Data(sameId? uid:uidChain.GetId(),name,code,new List<string>(zCode),paramCount,returnValue);
                }
            
        }

                   private static Data _defaultData=new Data(0,"","",null,0,"");
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

                {1,new Data(1,"","",null,0,"")},

                };
                    _DataByName = new Dictionary<string, Data>() {
    
                        {"",_DataByUid[1]},
    
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

                jo.Get<string>("name"),

                jo.Get<string>("code"),

                jo.Get<List<string>>("zCode"),

                jo.Get<int>("paramCount"),

                jo.Get<string>("returnValue")
                    );

            return data;
        }

        public static JObject GetJoByData(Data data)
        {
            Init();

            JObject jo=new JObject();

            jo.Set<int>("uid",data.uid);

            jo.Set<string>("name",data.name);

            jo.Set<string>("code",data.code);

            jo.Set<List<string>>("zCode",data.zCode);

            jo.Set<int>("paramCount",data.paramCount);

            jo.Set<string>("returnValue",data.returnValue);

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
            
            public static void ChangeName(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                    DataByName.Remove(oldV);
                    DataByName[newV]=data;
 
                changeNameAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeCode(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeCodeAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeZcode(Data superData,List<string> oldV,List<string> newV)
            {
                if(superData is Data data)
                {

                changeZcodeAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeParamcount(Data superData,int oldV,int newV)
            {
                if(superData is Data data)
                {

                changeParamcountAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeReturnvalue(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeReturnvalueAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
    }
}
        