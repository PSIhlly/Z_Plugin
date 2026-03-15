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

    public static partial class CmdDataForm
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
                
        public static Action<Data,string,string> changeNameAction;
                
        public static Action<Data,List<string>,List<string>> changePrmnamesAction;
                
        public static Action<Data,List<string>,List<string>> changePrmtypesAction;
                
        public static Action<Data,List<string>,List<string>> changeRetnamesAction;
                
        public static Action<Data,List<string>,List<string>> changeRettypesAction;
                
        public static Action<Data,string,string> changeDescAction;
                
        public static Action<Data,string,string> changeDefaultcodeAction;
                


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
                    
                    private List<string>  _prmNames;
                    /// <summary>
                    ///参数名称
                    ///</summary>
                    public List<string>  prmNames{
                                get{return _prmNames;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangePrmnames(this,_prmNames,value); 
                    }
        
                _prmNames = value;
                }
                 
                     }
                    
                    private List<string>  _prmTypes;
                    /// <summary>
                    ///参数类型
                    ///</summary>
                    public List<string>  prmTypes{
                                get{return _prmTypes;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangePrmtypes(this,_prmTypes,value); 
                    }
        
                _prmTypes = value;
                }
                 
                     }
                    
                    private List<string>  _retNames;
                    /// <summary>
                    ///返回值名称
                    ///</summary>
                    public List<string>  retNames{
                                get{return _retNames;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeRetnames(this,_retNames,value); 
                    }
        
                _retNames = value;
                }
                 
                     }
                    
                    private List<string>  _retTypes;
                    /// <summary>
                    ///返回值类型
                    ///</summary>
                    public List<string>  retTypes{
                                get{return _retTypes;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeRettypes(this,_retTypes,value); 
                    }
        
                _retTypes = value;
                }
                 
                     }
                    
                    private string  _desc;
                    /// <summary>
                    ///描述
                    ///</summary>
                    public string  desc{
                                get{return _desc;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeDesc(this,_desc,value); 
                    }
        
                _desc = value;
                }
                 
                     }
                    
                    private string  _defaultCode;
                    /// <summary>
                    ///默认代码
                    ///</summary>
                    public string  defaultCode{
                                get{return _defaultCode;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeDefaultcode(this,_defaultCode,value); 
                    }
        
                _defaultCode = value;
                }
                 
                     }
                    
            public Data(int uid,string name,List<string> prmNames,List<string> prmTypes,List<string> retNames,List<string> retTypes,string desc,string defaultCode)
            {

             this.uid = uid;
             this.name = name;
             this.prmNames = prmNames;
             this.prmTypes = prmTypes;
             this.retNames = retNames;
             this.retTypes = retTypes;
             this.desc = desc;
             this.defaultCode = defaultCode;

            }
            public void Reset(Data data)
            {

             this.uid = data.uid;
             this.name = data.name;
             this.prmNames = data.prmNames;
             this.prmTypes = data.prmTypes;
             this.retNames = data.retNames;
             this.retTypes = data.retTypes;
             this.desc = data.desc;
             this.defaultCode = data.defaultCode;
            }

                public Data Copy(bool sameId = true)
                {
        return new Data(sameId? uid:uidChain.GetId(),name,new List<string>(prmNames),new List<string>(prmTypes),new List<string>(retNames),new List<string>(retTypes),desc,defaultCode);
                }
            
        }

                   private static Data _defaultData=new Data(0,"",null,null,null,null,"","");
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

                {1,new Data(1,"Print",new List<string>(){"content",},new List<string>(){"string",},null,new List<string>(){"void",},"Print {0}","Print(\"\");")},

                {2,new Data(2,"+",new List<string>(){"a","b",},new List<string>(){"num","num",},new List<string>(){"result",},new List<string>(){"num",},"{1} + {0}","1+1")},

                {3,new Data(3,"-",new List<string>(){"a","b",},new List<string>(){"num","num",},new List<string>(){"result",},new List<string>(){"num",},"{1} - {0}","1-1")},

                {4,new Data(4,"*",new List<string>(){"a","b",},new List<string>(){"num","num",},new List<string>(){"result",},new List<string>(){"num",},"（{1}）*（{0}）","1*1")},

                {5,new Data(5,"/",new List<string>(){"a","b",},new List<string>(){"num","num",},new List<string>(){"result",},new List<string>(){"num",},"（{1}）/（{0}）","1/1")},

                {6,new Data(6,"=",new List<string>(){"a","b",},new List<string>(){"num","num",},new List<string>(){"result",},new List<string>(){"num",},"{1} = {0}","var=1;")},

                {7,new Data(7,"==",new List<string>(){"a","b",},new List<string>(){"num","num",},new List<string>(){"result",},new List<string>(){"num",},"{1} equal to {0}","1==1")},

                {8,new Data(8,">",new List<string>(){"a","b",},new List<string>(){"num","num",},new List<string>(){"result",},new List<string>(){"num",},"{1} greater than {0}","2>1")},

                {9,new Data(9,"<",new List<string>(){"a","b",},new List<string>(){"num","num",},new List<string>(){"result",},new List<string>(){"num",},"{1} less than {0}","1<2")},

                {10,new Data(10,">=",new List<string>(){"a","b",},new List<string>(){"num","num",},new List<string>(){"result",},new List<string>(){"num",},"{1} not less than {0}","1<2")},

                {11,new Data(11,"<=",new List<string>(){"a","b",},new List<string>(){"num","num",},new List<string>(){"result",},new List<string>(){"num",},"{1} not greater than {0}","1<2")},

                {12,new Data(12,"!=",new List<string>(){"a","b",},new List<string>(){"num","num",},new List<string>(){"result",},new List<string>(){"num",},"{1} not equal to {0}","1!=2")},

                {13,new Data(13,"if",new List<string>(){"condition",},null,new List<string>(){"result",},new List<string>(){"bool",},"if {0}","if(1){ }else{ }")},

                {14,new Data(14,"else",null,null,null,new List<string>(){"void",},"else","")},

                {15,new Data(15,"then",null,null,null,new List<string>(){"void",},"then","")},

                {16,new Data(16,"for",new List<string>(){"init","condition","turnOver",},null,null,new List<string>(){"void",},"{0}, if {1} keep do, after every times do{2}","for(id=0;id<3;id=id+1){ }")},

                {17,new Data(17,"Wait",new List<string>(){"time",},new List<string>(){"num",},null,new List<string>(){"void",},"wait for {0} seconds","Wait(1);")},

                {18,new Data(18,"Return",new List<string>(){"result",},new List<string>(){"var",},null,new List<string>(){"void",},"return {0}","Return result;")},

                };
                    _DataByName = new Dictionary<string, Data>() {
    
                        {"Print",_DataByUid[1]},
    
                        {"+",_DataByUid[2]},
    
                        {"-",_DataByUid[3]},
    
                        {"*",_DataByUid[4]},
    
                        {"/",_DataByUid[5]},
    
                        {"=",_DataByUid[6]},
    
                        {"==",_DataByUid[7]},
    
                        {">",_DataByUid[8]},
    
                        {"<",_DataByUid[9]},
    
                        {">=",_DataByUid[10]},
    
                        {"<=",_DataByUid[11]},
    
                        {"!=",_DataByUid[12]},
    
                        {"if",_DataByUid[13]},
    
                        {"else",_DataByUid[14]},
    
                        {"then",_DataByUid[15]},
    
                        {"for",_DataByUid[16]},
    
                        {"Wait",_DataByUid[17]},
    
                        {"Return",_DataByUid[18]},
    
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

                jo.Get<List<string>>("prmNames"),

                jo.Get<List<string>>("prmTypes"),

                jo.Get<List<string>>("retNames"),

                jo.Get<List<string>>("retTypes"),

                jo.Get<string>("desc"),

                jo.Get<string>("defaultCode")
                    );

            return data;
        }

        public static JObject GetJoByData(Data data)
        {
            Init();

            JObject jo=new JObject();

            jo.Set<int>("uid",data.uid);

            jo.Set<string>("name",data.name);

            jo.Set<List<string>>("prmNames",data.prmNames);

            jo.Set<List<string>>("prmTypes",data.prmTypes);

            jo.Set<List<string>>("retNames",data.retNames);

            jo.Set<List<string>>("retTypes",data.retTypes);

            jo.Set<string>("desc",data.desc);

            jo.Set<string>("defaultCode",data.defaultCode);

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
            
            public static void ChangePrmnames(Data superData,List<string> oldV,List<string> newV)
            {
                if(superData is Data data)
                {

                changePrmnamesAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangePrmtypes(Data superData,List<string> oldV,List<string> newV)
            {
                if(superData is Data data)
                {

                changePrmtypesAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeRetnames(Data superData,List<string> oldV,List<string> newV)
            {
                if(superData is Data data)
                {

                changeRetnamesAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeRettypes(Data superData,List<string> oldV,List<string> newV)
            {
                if(superData is Data data)
                {

                changeRettypesAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeDesc(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeDescAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeDefaultcode(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeDefaultcodeAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
    }
}
        