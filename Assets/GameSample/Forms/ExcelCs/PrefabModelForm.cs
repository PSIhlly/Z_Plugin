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

    public static partial class PrefabModelForm
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
                
        public static Action<Data,string,string> changeKeyAction;
                


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
                    ///Ãû³Æ
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
                    
                    private string  _key;
                    /// <summary>
                    ///¶ÔÓ¦key
                    ///</summary>
                    public string  key{
                                get{return _key;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeKey(this,_key,value); 
                    }
        
                _key = value;
                }
                 
                     }
                    
            public Data(int uid,string name,string key)
            {

             this.uid = uid;
             this.name = name;
             this.key = key;

            }

                public Data Copy(bool sameId = true)
                {
        return new Data(sameId? uid:uidChain.GetId(),name,key);
                }
            
        }

                   private static Data _defaultData=new Data(0,"","");
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

                {1,new Data(1,"cube","cube")},

                {2,new Data(2,"sphere","sphere")},

                };
                    _DataByName = new Dictionary<string, Data>() {
    
                        {"cube",_DataByUid[1]},
    
                        {"sphere",_DataByUid[2]},
    
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

                jo.Get<string>("key")
                    );

            return data;
        }

        public static JObject GetJoByData(Data data)
        {
            Init();

            JObject jo=new JObject();

            jo.Set<int>("uid",data.uid);

            jo.Set<string>("name",data.name);

            jo.Set<string>("key",data.key);

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
        }
        public static void Clear()
        {
            Init();

                    DataByUid.Clear();
    
                    DataByName.Clear();
    
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
            
            public static void ChangeName(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                    DataByName.Remove(oldV);
                    DataByName[newV]=data;
 
                changeNameAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeKey(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeKeyAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
    }
}
        