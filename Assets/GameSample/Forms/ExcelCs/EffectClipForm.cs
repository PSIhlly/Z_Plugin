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

    public static partial class EffectClipForm
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
                
        public static Action<Data,string,string> changeTexAction;
                
        public static Action<Data,float,float> changeTimeAction;
                
        public static Action<Data,Vector3,Vector3> changePosAction;
                
        public static Action<Data,float,float> changeRotAction;
                
        public static Action<Data,Vector3,Vector3> changeScaleAction;
                
        public static Action<Data,float,float> changeOpacityAction;
                
        public static Action<Data,bool,bool> changeTransitionAction;
                


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
                    
                    private string  _tex;
                    /// <summary>
                    ///图片名
                    ///</summary>
                    public string  tex{
                                get{return _tex;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeTex(this,_tex,value); 
                    }
        
                _tex = value;
                }
                 
                     }
                    
                    private float  _time;
                    /// <summary>
                    ///关键时刻（秒）
                    ///</summary>
                    public float  time{
                                get{return _time;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeTime(this,_time,value); 
                    }
        
                _time = value;
                }
                 
                     }
                    
                    private Vector3  _pos;
                    /// <summary>
                    ///位置
                    ///</summary>
                    public Vector3  pos{
                                get{return _pos;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangePos(this,_pos,value); 
                    }
        
                _pos = value;
                }
                 
                     }
                    
                    private float  _rot;
                    /// <summary>
                    ///旋转
                    ///</summary>
                    public float  rot{
                                get{return _rot;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeRot(this,_rot,value); 
                    }
        
                _rot = value;
                }
                 
                     }
                    
                    private Vector3  _scale;
                    /// <summary>
                    ///大小
                    ///</summary>
                    public Vector3  scale{
                                get{return _scale;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeScale(this,_scale,value); 
                    }
        
                _scale = value;
                }
                 
                     }
                    
                    private float  _opacity;
                    /// <summary>
                    ///透明度
                    ///</summary>
                    public float  opacity{
                                get{return _opacity;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeOpacity(this,_opacity,value); 
                    }
        
                _opacity = value;
                }
                 
                     }
                    
                    private bool  _transition;
                    /// <summary>
                    ///过渡
                    ///</summary>
                    public bool  transition{
                                get{return _transition;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeTransition(this,_transition,value); 
                    }
        
                _transition = value;
                }
                 
                     }
                    
            public Data(int uid,string tex,float time,Vector3 pos,float rot,Vector3 scale,float opacity,bool transition)
            {

             this.uid = uid;
             this.tex = tex;
             this.time = time;
             this.pos = pos;
             this.rot = rot;
             this.scale = scale;
             this.opacity = opacity;
             this.transition = transition;

            }
            public void Reset(Data data)
            {

             this.uid = data.uid;
             this.tex = data.tex;
             this.time = data.time;
             this.pos = data.pos;
             this.rot = data.rot;
             this.scale = data.scale;
             this.opacity = data.opacity;
             this.transition = data.transition;
            }

                public Data Copy(bool sameId = true)
                {
        return new Data(sameId? uid:uidChain.GetId(),tex,time,pos,rot,scale,opacity,transition);
                }
            
            public virtual  void BeforeGet()
            {
                
                EffectClipForm.beforeGetAction?.Invoke(this);
            }
        }

                   private static Data _defaultData=new Data(0,"",0f,Vector3.zero,0f,Vector3.one,1f,false);
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

                jo.SelectToken("tex")==null?defaultData.tex:jo.Get<string>("tex"),

                jo.SelectToken("time")==null?defaultData.time:jo.Get<float>("time"),

                jo.SelectToken("pos")==null?defaultData.pos:jo.Get<Vector3>("pos"),

                jo.SelectToken("rot")==null?defaultData.rot:jo.Get<float>("rot"),

                jo.SelectToken("scale")==null?defaultData.scale:jo.Get<Vector3>("scale"),

                jo.SelectToken("opacity")==null?defaultData.opacity:jo.Get<float>("opacity"),

                jo.SelectToken("transition")==null?defaultData.transition:jo.Get<bool>("transition")
                    );

            return data;
        }

        public static JObject GetJoByData(Data data)
        {
            Init();
            data.BeforeGet();

            JObject jo=new JObject();

            jo.Set<int>("uid",data.uid);

            jo.Set<string>("tex",data.tex);

            jo.Set<float>("time",data.time);

            jo.Set<Vector3>("pos",data.pos);

            jo.Set<float>("rot",data.rot);

            jo.Set<Vector3>("scale",data.scale);

            jo.Set<float>("opacity",data.opacity);

            jo.Set<bool>("transition",data.transition);

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
            
            public static void ChangeTex(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeTexAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeTime(Data superData,float oldV,float newV)
            {
                if(superData is Data data)
                {

                changeTimeAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangePos(Data superData,Vector3 oldV,Vector3 newV)
            {
                if(superData is Data data)
                {

                changePosAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeRot(Data superData,float oldV,float newV)
            {
                if(superData is Data data)
                {

                changeRotAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeScale(Data superData,Vector3 oldV,Vector3 newV)
            {
                if(superData is Data data)
                {

                changeScaleAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeOpacity(Data superData,float oldV,float newV)
            {
                if(superData is Data data)
                {

                changeOpacityAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeTransition(Data superData,bool oldV,bool newV)
            {
                if(superData is Data data)
                {

                changeTransitionAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
    }
}
        