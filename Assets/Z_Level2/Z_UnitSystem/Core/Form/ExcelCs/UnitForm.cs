using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Z_ByteSerialize;
using Z_DesignStyle;

namespace Z_UnitSystem.Form
{

    public static partial class UnitForm
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
                
        public static Action<Data,string,string> changeNameAction;
                
        public static Action<Data,string,string> changePrefabnameAction;
                
        public static Action<Data,Vector3,Vector3> changePosAction;
                
        public static Action<Data,Vector3,Vector3> changeEulerAction;
                
        public static Action<Data,Vector3,Vector3> changeScaleAction;
                
        public static Action<Data,int,int> changeUpdatetypeAction;
                
        public static Action<Data,string,string> changeExtraAction;
                


        public partial class Data
        {

                protected Unit _unit;

                /// <summary>
                ///单位逻辑
                ///</summary>
                public Unit unit
                {
                    get
                    {
                        return (Unit) _unit;
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
                    
                    private string  _prefabName;
                    /// <summary>
                    ///预制名字（索引）
                    ///</summary>
                    public string  prefabName{
                                get{return _prefabName;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangePrefabname(this,_prefabName,value); 
                    }
        
                _prefabName = value;
                }
                 
                     }
                    
                    private Vector3  _pos;
                    /// <summary>
                    ///位置
                    ///</summary>
                    public Vector3  pos{
                                get{return _pos;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangePos(this,_pos,value); 
                    }
        
                _pos = value;
                }
                 
                     }
                    
                    private Vector3  _euler;
                    /// <summary>
                    ///欧拉旋转
                    ///</summary>
                    public Vector3  euler{
                                get{return _euler;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeEuler(this,_euler,value); 
                    }
        
                _euler = value;
                }
                 
                     }
                    
                    private Vector3  _scale;
                    /// <summary>
                    ///缩放
                    ///</summary>
                    public Vector3  scale{
                                get{return _scale;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeScale(this,_scale,value); 
                    }
        
                _scale = value;
                }
                 
                     }
                    
                    private int  _updateType;
                    /// <summary>
                    ///更新方式
                    ///</summary>
                    public int  updateType{
                                get{return _updateType;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeUpdatetype(this,_updateType,value); 
                    }
        
                _updateType = value;
                }
                 
                     }
                    
                    private string  _extra;
                    /// <summary>
                    ///扩展位
                    ///</summary>
                    public string  extra{
                                get{return _extra;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeExtra(this,_extra,value); 
                    }
        
                _extra = value;
                }
                 
                     }
                    
            public Data(int uid,string name,string prefabName,Vector3 pos,Vector3 euler,Vector3 scale,int updateType,string extra)
            {

             this.uid = uid;
             this.name = name;
             this.prefabName = prefabName;
             this.pos = pos;
             this.euler = euler;
             this.scale = scale;
             this.updateType = updateType;
             this.extra = extra;

                    _unit=new Unit(this);

            }

                public Data Copy()
                {
        return new Data(-1,name,prefabName,pos,euler,scale,updateType,extra);
                }
            
        }

                   public static Data defaultData=new Data(0,"","",Vector3.zero,Vector3.zero,Vector3.zero,0,"");


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

                jo.Get<string>("prefabName"),

                jo.Get<Vector3>("pos"),

                jo.Get<Vector3>("euler"),

                jo.Get<Vector3>("scale"),

                jo.Get<int>("updateType"),

                jo.Get<string>("extra")
                    );

            return data;
        }

        public static JObject GetJoByData(Data data)
        {
            Init();

            JObject jo=new JObject();

            jo.Set<int>("uid",data.uid);

            jo.Set<string>("name",data.name);

            jo.Set<string>("prefabName",data.prefabName);

            jo.Set<Vector3>("pos",data.pos);

            jo.Set<Vector3>("euler",data.euler);

            jo.Set<Vector3>("scale",data.scale);

            jo.Set<int>("updateType",data.updateType);

            jo.Set<string>("extra",data.extra);

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
            
            public static void ChangeName(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeNameAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangePrefabname(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changePrefabnameAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangePos(Data superData,Vector3 oldV,Vector3 newV)
            {
                if(superData is Data data)
                {

                changePosAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeEuler(Data superData,Vector3 oldV,Vector3 newV)
            {
                if(superData is Data data)
                {

                changeEulerAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeScale(Data superData,Vector3 oldV,Vector3 newV)
            {
                if(superData is Data data)
                {

                changeScaleAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeUpdatetype(Data superData,int oldV,int newV)
            {
                if(superData is Data data)
                {

                changeUpdatetypeAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeExtra(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeExtraAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
    }
}
        