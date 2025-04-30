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

    public static partial class VarForm
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
                
        public static Action<Data,string,string> changeNameAction;
                
        public static Action<Data,int,int> changeTypeAction;
                
        public static Action<Data,float,float> changeVAction;
                
        public static Action<Data,string,string> changeSAction;
                
        public static Action<Data,int,int> changeUnituidAction;
                


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
                    
                    private int  _type;
                    /// <summary>
                    ///类型
                    ///</summary>
                    public int  type{
                                get{return _type;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeType(this,_type,value); 
                    }
        
                _type = value;
                }
                 
                     }
                    
                    private float  _v;
                    /// <summary>
                    ///值
                    ///</summary>
                    public float  v{
                                get{return _v;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeV(this,_v,value); 
                    }
        
                _v = value;
                }
                 
                     }
                    
                    private string  _s;
                    /// <summary>
                    ///值
                    ///</summary>
                    public string  s{
                                get{return _s;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeS(this,_s,value); 
                    }
        
                _s = value;
                }
                 
                     }
                    
                    private int  _unitUid;
                    /// <summary>
                    ///引用unit
                    ///</summary>
                    public int  unitUid{
                                get{return _unitUid;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeUnituid(this,_unitUid,value); 
                    }
        
                _unitUid = value;
                }
                 
                     }
                    
            public Data(int uid,string name,int type,float v,string s,int unitUid)
            {

             this.uid = uid;
             this.name = name;
             this.type = type;
             this.v = v;
             this.s = s;
             this.unitUid = unitUid;

            }

                public Data Copy()
                {
        return new Data(-1,name,type,v,s,unitUid);
                }
            
        }

                   public static Data defaultData=new Data(0,"",0,0f,"",0);


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

                jo.Get<string>("name"),

                jo.Get<int>("type"),

                jo.Get<float>("v"),

                jo.Get<string>("s"),

                jo.Get<int>("unitUid")
                    );

            return data;
        }

        public static JObject GetJoByData(Data data)
        {
            Init();

            JObject jo=new JObject();

            jo.Set<int>("uid",data.uid);

            jo.Set<string>("name",data.name);

            jo.Set<int>("type",data.type);

            jo.Set<float>("v",data.v);

            jo.Set<string>("s",data.s);

            jo.Set<int>("unitUid",data.unitUid);

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
            foreach(var data in DataByUid.Values)
            {
                if(data.uid<uidChain.cnt)
                    RemoveData(data.uid);
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
            
            public static void ChangeType(Data superData,int oldV,int newV)
            {
                if(superData is Data data)
                {

                changeTypeAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeV(Data superData,float oldV,float newV)
            {
                if(superData is Data data)
                {

                changeVAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeS(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeSAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeUnituid(Data superData,int oldV,int newV)
            {
                if(superData is Data data)
                {

                changeUnituidAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
    }
}
        