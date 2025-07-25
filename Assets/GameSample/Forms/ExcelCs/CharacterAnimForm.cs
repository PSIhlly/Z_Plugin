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

        public static Action childInitAction;
        public static Action<Data> childRemoveAction;
        public static Action<Data> childAddAction;

        public static Action<Data,int,int> changeUidAction;
                
        public static Action<Data,string,string> changeNameAction;
                
        public static Action<Data,List<CharacterAnimClipForm.Data>,List<CharacterAnimClipForm.Data>> changeAnimclipAction;
                
        public static Action<Data,float,float> changeAnimtimeintervalAction;
                
        public static Action<Data,float,float> changeScaleAction;
                


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
                    
                    private List<CharacterAnimClipForm.Data>  _animClip;
                    /// <summary>
                    ///装备
                    ///</summary>
                    public List<CharacterAnimClipForm.Data>  animClip{
                                get{return _animClip;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
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

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
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

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeScale(this,_scale,value); 
                    }
        
                _scale = value;
                }
                 
                     }
                    
            public Data(int uid,string name,List<CharacterAnimClipForm.Data> animClip,float animTimeInterval,float scale)
            {

             this.uid = uid;
             this.name = name;
             this.animClip = animClip;
             this.animTimeInterval = animTimeInterval;
             this.scale = scale;

            }

                public Data Copy(bool sameId = true)
                {
        return new Data(sameId? uid:uidChain.GetId(),name,new List<CharacterAnimClipForm.Data>(animClip),animTimeInterval,scale);
                }
            
        }

                   private static Data _defaultData=new Data(0,"",null,0f,0f);
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

                jo.Get<string>("name"),

                jo.Get<List<CharacterAnimClipForm.Data>>("animClip"),

                jo.Get<float>("animTimeInterval"),

                jo.Get<float>("scale")
                    );

            return data;
        }

        public static JObject GetJoByData(Data data)
        {
            Init();

            JObject jo=new JObject();

            jo.Set<int>("uid",data.uid);

            jo.Set<string>("name",data.name);

            jo.Set<List<CharacterAnimClipForm.Data>>("animClip",data.animClip);

            jo.Set<float>("animTimeInterval",data.animTimeInterval);

            jo.Set<float>("scale",data.scale);

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
            
            public static void ChangeName(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeNameAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeAnimclip(Data superData,List<CharacterAnimClipForm.Data> oldV,List<CharacterAnimClipForm.Data> newV)
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
            
    }
}
        