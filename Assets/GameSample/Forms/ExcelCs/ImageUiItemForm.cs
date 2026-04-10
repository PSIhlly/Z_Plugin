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

    public static partial class ImageUiItemForm
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
                
        public static Action<Data,string,string> changeTexnameAction;
                
        public static Action<Data,Vector2,Vector2> changeSizeAction;
                
        public static Action<Data,Vector2,Vector2> changeOldposAction;
                
        public static Action<Data,float,float> changePosprogressAction;
                
        public static Action<Data,float,float> changePostimeAction;
                
        public static Action<Data,Vector2,Vector2> changeTarposAction;
                
        public static Action<Data,float,float> changeOldopacityAction;
                
        public static Action<Data,float,float> changeOpacityprogressAction;
                
        public static Action<Data,float,float> changeOpacitytimeAction;
                
        public static Action<Data,float,float> changeTaropacityAction;
                
        public static Action<Data,float,float> changeOldeulerAction;
                
        public static Action<Data,float,float> changeEulerprogressAction;
                
        public static Action<Data,float,float> changeEulertimeAction;
                
        public static Action<Data,float,float> changeTareulerAction;
                
        public static Action<Data,float,float> changeRemovetimeAction;
                


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
                    
                    private string  _texName;
                    /// <summary>
                    ///图片名称
                    ///</summary>
                    public string  texName{
                                get{return _texName;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeTexname(this,_texName,value); 
                    }
        
                _texName = value;
                }
                 
                     }
                    
                    private Vector2  _size;
                    /// <summary>
                    ///尺寸
                    ///</summary>
                    public Vector2  size{
                                get{return _size;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeSize(this,_size,value); 
                    }
        
                _size = value;
                }
                 
                     }
                    
                    private Vector2  _oldPos;
                    /// <summary>
                    ///原位置
                    ///</summary>
                    public Vector2  oldPos{
                                get{return _oldPos;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeOldpos(this,_oldPos,value); 
                    }
        
                _oldPos = value;
                }
                 
                     }
                    
                    private float  _posProgress;
                    /// <summary>
                    ///位置进度
                    ///</summary>
                    public float  posProgress{
                                get{return _posProgress;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangePosprogress(this,_posProgress,value); 
                    }
        
                _posProgress = value;
                }
                 
                     }
                    
                    private float  _posTime;
                    /// <summary>
                    ///位置总耗时
                    ///</summary>
                    public float  posTime{
                                get{return _posTime;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangePostime(this,_posTime,value); 
                    }
        
                _posTime = value;
                }
                 
                     }
                    
                    private Vector2  _tarPos;
                    /// <summary>
                    ///目标位置
                    ///</summary>
                    public Vector2  tarPos{
                                get{return _tarPos;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeTarpos(this,_tarPos,value); 
                    }
        
                _tarPos = value;
                }
                 
                     }
                    
                    private float  _oldOpacity;
                    /// <summary>
                    ///原不透明度
                    ///</summary>
                    public float  oldOpacity{
                                get{return _oldOpacity;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeOldopacity(this,_oldOpacity,value); 
                    }
        
                _oldOpacity = value;
                }
                 
                     }
                    
                    private float  _opacityProgress;
                    /// <summary>
                    ///不透明度进度
                    ///</summary>
                    public float  opacityProgress{
                                get{return _opacityProgress;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeOpacityprogress(this,_opacityProgress,value); 
                    }
        
                _opacityProgress = value;
                }
                 
                     }
                    
                    private float  _opacityTime;
                    /// <summary>
                    ///不透明度总耗时
                    ///</summary>
                    public float  opacityTime{
                                get{return _opacityTime;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeOpacitytime(this,_opacityTime,value); 
                    }
        
                _opacityTime = value;
                }
                 
                     }
                    
                    private float  _tarOpacity;
                    /// <summary>
                    ///目标不透明度
                    ///</summary>
                    public float  tarOpacity{
                                get{return _tarOpacity;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeTaropacity(this,_tarOpacity,value); 
                    }
        
                _tarOpacity = value;
                }
                 
                     }
                    
                    private float  _oldEuler;
                    /// <summary>
                    ///原旋转
                    ///</summary>
                    public float  oldEuler{
                                get{return _oldEuler;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeOldeuler(this,_oldEuler,value); 
                    }
        
                _oldEuler = value;
                }
                 
                     }
                    
                    private float  _eulerProgress;
                    /// <summary>
                    ///旋转进度
                    ///</summary>
                    public float  eulerProgress{
                                get{return _eulerProgress;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeEulerprogress(this,_eulerProgress,value); 
                    }
        
                _eulerProgress = value;
                }
                 
                     }
                    
                    private float  _eulerTime;
                    /// <summary>
                    ///旋转总耗时
                    ///</summary>
                    public float  eulerTime{
                                get{return _eulerTime;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeEulertime(this,_eulerTime,value); 
                    }
        
                _eulerTime = value;
                }
                 
                     }
                    
                    private float  _tarEuler;
                    /// <summary>
                    ///目标旋转
                    ///</summary>
                    public float  tarEuler{
                                get{return _tarEuler;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeTareuler(this,_tarEuler,value); 
                    }
        
                _tarEuler = value;
                }
                 
                     }
                    
                    private float  _removeTime;
                    /// <summary>
                    ///移除时间
                    ///</summary>
                    public float  removeTime{
                                get{return _removeTime;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeRemovetime(this,_removeTime,value); 
                    }
        
                _removeTime = value;
                }
                 
                     }
                    
            public Data(int uid,string texName,Vector2 size,Vector2 oldPos,float posProgress,float posTime,Vector2 tarPos,float oldOpacity,float opacityProgress,float opacityTime,float tarOpacity,float oldEuler,float eulerProgress,float eulerTime,float tarEuler,float removeTime)
            {

             this.uid = uid;
             this.texName = texName;
             this.size = size;
             this.oldPos = oldPos;
             this.posProgress = posProgress;
             this.posTime = posTime;
             this.tarPos = tarPos;
             this.oldOpacity = oldOpacity;
             this.opacityProgress = opacityProgress;
             this.opacityTime = opacityTime;
             this.tarOpacity = tarOpacity;
             this.oldEuler = oldEuler;
             this.eulerProgress = eulerProgress;
             this.eulerTime = eulerTime;
             this.tarEuler = tarEuler;
             this.removeTime = removeTime;

            }
            public void Reset(Data data)
            {

             this.uid = data.uid;
             this.texName = data.texName;
             this.size = data.size;
             this.oldPos = data.oldPos;
             this.posProgress = data.posProgress;
             this.posTime = data.posTime;
             this.tarPos = data.tarPos;
             this.oldOpacity = data.oldOpacity;
             this.opacityProgress = data.opacityProgress;
             this.opacityTime = data.opacityTime;
             this.tarOpacity = data.tarOpacity;
             this.oldEuler = data.oldEuler;
             this.eulerProgress = data.eulerProgress;
             this.eulerTime = data.eulerTime;
             this.tarEuler = data.tarEuler;
             this.removeTime = data.removeTime;
            }

                public Data Copy(bool sameId = true)
                {
        return new Data(sameId? uid:uidChain.GetId(),texName,size,oldPos,posProgress,posTime,tarPos,oldOpacity,opacityProgress,opacityTime,tarOpacity,oldEuler,eulerProgress,eulerTime,tarEuler,removeTime);
                }
            
            public virtual  void BeforeGet()
            {
                
                ImageUiItemForm.beforeGetAction?.Invoke(this);
            }
        }

                   private static Data _defaultData=new Data(0,"",default,default,0f,0f,default,0f,0f,0f,0f,0f,0f,0f,0f,999999f);
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

                jo.SelectToken("texName")==null?defaultData.texName:jo.Get<string>("texName"),

                jo.SelectToken("size")==null?defaultData.size:jo.Get<Vector2>("size"),

                jo.SelectToken("oldPos")==null?defaultData.oldPos:jo.Get<Vector2>("oldPos"),

                jo.SelectToken("posProgress")==null?defaultData.posProgress:jo.Get<float>("posProgress"),

                jo.SelectToken("posTime")==null?defaultData.posTime:jo.Get<float>("posTime"),

                jo.SelectToken("tarPos")==null?defaultData.tarPos:jo.Get<Vector2>("tarPos"),

                jo.SelectToken("oldOpacity")==null?defaultData.oldOpacity:jo.Get<float>("oldOpacity"),

                jo.SelectToken("opacityProgress")==null?defaultData.opacityProgress:jo.Get<float>("opacityProgress"),

                jo.SelectToken("opacityTime")==null?defaultData.opacityTime:jo.Get<float>("opacityTime"),

                jo.SelectToken("tarOpacity")==null?defaultData.tarOpacity:jo.Get<float>("tarOpacity"),

                jo.SelectToken("oldEuler")==null?defaultData.oldEuler:jo.Get<float>("oldEuler"),

                jo.SelectToken("eulerProgress")==null?defaultData.eulerProgress:jo.Get<float>("eulerProgress"),

                jo.SelectToken("eulerTime")==null?defaultData.eulerTime:jo.Get<float>("eulerTime"),

                jo.SelectToken("tarEuler")==null?defaultData.tarEuler:jo.Get<float>("tarEuler"),

                jo.SelectToken("removeTime")==null?defaultData.removeTime:jo.Get<float>("removeTime")
                    );

            return data;
        }

        public static JObject GetJoByData(Data data)
        {
            Init();
            data.BeforeGet();

            JObject jo=new JObject();

            jo.Set<int>("uid",data.uid);

            jo.Set<string>("texName",data.texName);

            jo.Set<Vector2>("size",data.size);

            jo.Set<Vector2>("oldPos",data.oldPos);

            jo.Set<float>("posProgress",data.posProgress);

            jo.Set<float>("posTime",data.posTime);

            jo.Set<Vector2>("tarPos",data.tarPos);

            jo.Set<float>("oldOpacity",data.oldOpacity);

            jo.Set<float>("opacityProgress",data.opacityProgress);

            jo.Set<float>("opacityTime",data.opacityTime);

            jo.Set<float>("tarOpacity",data.tarOpacity);

            jo.Set<float>("oldEuler",data.oldEuler);

            jo.Set<float>("eulerProgress",data.eulerProgress);

            jo.Set<float>("eulerTime",data.eulerTime);

            jo.Set<float>("tarEuler",data.tarEuler);

            jo.Set<float>("removeTime",data.removeTime);

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
            
            public static void ChangeTexname(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeTexnameAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeSize(Data superData,Vector2 oldV,Vector2 newV)
            {
                if(superData is Data data)
                {

                changeSizeAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeOldpos(Data superData,Vector2 oldV,Vector2 newV)
            {
                if(superData is Data data)
                {

                changeOldposAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangePosprogress(Data superData,float oldV,float newV)
            {
                if(superData is Data data)
                {

                changePosprogressAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangePostime(Data superData,float oldV,float newV)
            {
                if(superData is Data data)
                {

                changePostimeAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeTarpos(Data superData,Vector2 oldV,Vector2 newV)
            {
                if(superData is Data data)
                {

                changeTarposAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeOldopacity(Data superData,float oldV,float newV)
            {
                if(superData is Data data)
                {

                changeOldopacityAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeOpacityprogress(Data superData,float oldV,float newV)
            {
                if(superData is Data data)
                {

                changeOpacityprogressAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeOpacitytime(Data superData,float oldV,float newV)
            {
                if(superData is Data data)
                {

                changeOpacitytimeAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeTaropacity(Data superData,float oldV,float newV)
            {
                if(superData is Data data)
                {

                changeTaropacityAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeOldeuler(Data superData,float oldV,float newV)
            {
                if(superData is Data data)
                {

                changeOldeulerAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeEulerprogress(Data superData,float oldV,float newV)
            {
                if(superData is Data data)
                {

                changeEulerprogressAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeEulertime(Data superData,float oldV,float newV)
            {
                if(superData is Data data)
                {

                changeEulertimeAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeTareuler(Data superData,float oldV,float newV)
            {
                if(superData is Data data)
                {

                changeTareulerAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeRemovetime(Data superData,float oldV,float newV)
            {
                if(superData is Data data)
                {

                changeRemovetimeAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
    }
}
        