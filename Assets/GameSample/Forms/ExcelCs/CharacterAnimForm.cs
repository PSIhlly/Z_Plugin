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

        public static Action<Data> addAction;
        public static Action<Data> removeAction;
        public static Action childInitAction;
        public static Action<Data> childRemoveAction;
        public static Action<Data> childAddAction;
        
        public static Action<Data> beforeGetAction;

        public static Action<Data,int,int> changeUidAction;
                
        public static Action<Data,string,string> changeNameAction;
                
        public static Action<Data,Dictionary<AnimDirecton,List<CharacterAnimClipForm.Data>>,Dictionary<AnimDirecton,List<CharacterAnimClipForm.Data>>> changeAnimclipAction;
                
        public static Action<Data,float,float> changeAnimtimeintervalAction;
                
        public static Action<Data,float,float> changeScaleAction;
                
        public static Action<Data,Dictionary<BodyPartType,bool>,Dictionary<BodyPartType,bool>> changePartenableAction;
                


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
                    
                    private Dictionary<AnimDirecton,List<CharacterAnimClipForm.Data>>  _animClip;
                    /// <summary>
                    ///片段
                    ///</summary>
                    public Dictionary<AnimDirecton,List<CharacterAnimClipForm.Data>>  animClip{
                                get{return _animClip;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeAnimclip(this,_animClip,value); 
                    }
        
                _animClip = value;
                }
                 
                     }
                    
                    private float  _animTimeInterval;
                    /// <summary>
                    ///动画间隔
                    ///</summary>
                    public float  animTimeInterval{
                                get{return _animTimeInterval;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeAnimtimeinterval(this,_animTimeInterval,value); 
                    }
        
                _animTimeInterval = value;
                }
                 
                     }
                    
                    private float  _scale;
                    /// <summary>
                    ///缩放
                    ///</summary>
                    public float  scale{
                                get{return _scale;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeScale(this,_scale,value); 
                    }
        
                _scale = value;
                }
                 
                     }
                    
                    private Dictionary<BodyPartType,bool>  _partEnable;
                    /// <summary>
                    ///部位启用
                    ///</summary>
                    public Dictionary<BodyPartType,bool>  partEnable{
                                get{return _partEnable;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangePartenable(this,_partEnable,value); 
                    }
        
                _partEnable = value;
                }
                 
                     }
                    
            public Data(int uid,string name,Dictionary<AnimDirecton,List<CharacterAnimClipForm.Data>> animClip,float animTimeInterval,float scale,Dictionary<BodyPartType,bool> partEnable)
            {

             this.uid = uid;
             this.name = name;
             this.animClip = animClip;
             this.animTimeInterval = animTimeInterval;
             this.scale = scale;
             this.partEnable = partEnable;

            }
            public void Reset(Data data)
            {

             this.uid = data.uid;
             this.name = data.name;
             this.animClip = data.animClip;
             this.animTimeInterval = data.animTimeInterval;
             this.scale = data.scale;
             this.partEnable = data.partEnable;
            }

                public Data Copy(bool sameId = true)
                {
        return new Data(sameId? uid:uidChain.GetId(),name,animClip==null?new Dictionary<AnimDirecton,List<CharacterAnimClipForm.Data>>():new Dictionary<AnimDirecton,List<CharacterAnimClipForm.Data>>(animClip),animTimeInterval,scale,partEnable==null?new Dictionary<BodyPartType,bool>():new Dictionary<BodyPartType,bool>(partEnable));
                }
            
            public virtual  void BeforeGet()
            {
                
                CharacterAnimForm.beforeGetAction?.Invoke(this);
            }
        }

                   private static Data _defaultData=new Data(0,"",new Dictionary<AnimDirecton,List<CharacterAnimClipForm.Data>>(){},0f,0f,new Dictionary<BodyPartType,bool>(){});
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

                jo.SelectToken("animClip")==null?defaultData.animClip:jo.Get<Dictionary<AnimDirecton,List<CharacterAnimClipForm.Data>>>("animClip"),

                jo.SelectToken("animTimeInterval")==null?defaultData.animTimeInterval:jo.Get<float>("animTimeInterval"),

                jo.SelectToken("scale")==null?defaultData.scale:jo.Get<float>("scale"),

                jo.SelectToken("partEnable")==null?defaultData.partEnable:jo.Get<Dictionary<BodyPartType,bool>>("partEnable")
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

            jo.Set<Dictionary<AnimDirecton,List<CharacterAnimClipForm.Data>>>("animClip",data.animClip);

            jo.Set<float>("animTimeInterval",data.animTimeInterval);

            jo.Set<float>("scale",data.scale);

            jo.Set<Dictionary<BodyPartType,bool>>("partEnable",data.partEnable);

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
            
            public static void ChangeAnimclip(Data superData,Dictionary<AnimDirecton,List<CharacterAnimClipForm.Data>> oldV,Dictionary<AnimDirecton,List<CharacterAnimClipForm.Data>> newV)
            {
                if(superData is Data data)
                {

                changeAnimclipAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeAnimtimeinterval(Data superData,float oldV,float newV)
            {
                if(superData is Data data)
                {

                changeAnimtimeintervalAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeScale(Data superData,float oldV,float newV)
            {
                if(superData is Data data)
                {

                changeScaleAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangePartenable(Data superData,Dictionary<BodyPartType,bool> oldV,Dictionary<BodyPartType,bool> newV)
            {
                if(superData is Data data)
                {

                changePartenableAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
    }
}
        