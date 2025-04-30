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

    public static partial class CharacterAnimForm
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
                
        public static Action<Data,List<(float,float)>,List<(float,float)>> changeAnimposAction;
                
        public static Action<Data,float,float> changeAnimtimeintervalAction;
                
        public static Action<Data,List<List<string>>,List<List<string>>> changePartanimtexsnameAction;
                


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
                    
                    private List<(float,float)>  _animPos;
                    /// <summary>
                    ///动画关键位置
                    ///</summary>
                    public List<(float,float)>  animPos{
                                get{return _animPos;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeAnimpos(this,_animPos,value); 
                    }
        
                _animPos = value;
                }
                 
                     }
                    
                    private float  _animTimeInterval;
                    /// <summary>
                    ///动画间隔
                    ///</summary>
                    public float  animTimeInterval{
                                get{return _animTimeInterval;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeAnimtimeinterval(this,_animTimeInterval,value); 
                    }
        
                _animTimeInterval = value;
                }
                 
                     }
                    
                    private List<List<string>>  _partAnimTexsName;
                    /// <summary>
                    ///贴图名称
                    ///</summary>
                    public List<List<string>>  partAnimTexsName{
                                get{return _partAnimTexsName;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangePartanimtexsname(this,_partAnimTexsName,value); 
                    }
        
                _partAnimTexsName = value;
                }
                 
                     }
                    
            public Data(int uid,string name,List<(float,float)> animPos,float animTimeInterval,List<List<string>> partAnimTexsName)
            {

             this.uid = uid;
             this.name = name;
             this.animPos = animPos;
             this.animTimeInterval = animTimeInterval;
             this.partAnimTexsName = partAnimTexsName;

            }

                public Data Copy()
                {
        return new Data(-1,name,animPos,animTimeInterval,partAnimTexsName);
                }
            
        }

                   public static Data defaultData=new Data(0,"",null,0f,null);


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

                jo.Get<List<(float,float)>>("animPos"),

                jo.Get<float>("animTimeInterval"),

                jo.Get<List<List<string>>>("partAnimTexsName")
                    );

            return data;
        }

        public static JObject GetJoByData(Data data)
        {
            Init();

            JObject jo=new JObject();

            jo.Set<int>("uid",data.uid);

            jo.Set<string>("name",data.name);

            jo.Set<List<(float,float)>>("animPos",data.animPos);

            jo.Set<float>("animTimeInterval",data.animTimeInterval);

            jo.Set<List<List<string>>>("partAnimTexsName",data.partAnimTexsName);

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
            
            public static void ChangeAnimpos(Data superData,List<(float,float)> oldV,List<(float,float)> newV)
            {
                if(superData is Data data)
                {

                changeAnimposAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeAnimtimeinterval(Data superData,float oldV,float newV)
            {
                if(superData is Data data)
                {

                changeAnimtimeintervalAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangePartanimtexsname(Data superData,List<List<string>> oldV,List<List<string>> newV)
            {
                if(superData is Data data)
                {

                changePartanimtexsnameAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
    }
}
        