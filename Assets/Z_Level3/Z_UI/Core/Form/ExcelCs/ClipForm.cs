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

namespace Z_Ui.Form
{

    public static partial class ClipForm
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
                
        public static Action<Data,string,string> changeTitleAction;
                
        public static Action<Data,string,string> changeMaintextAction;
                
        public static Action<Data,int,int> changeMainpictureAction;
                
        public static Action<Data,int,int> changeMainvideoAction;
                
        public static Action<Data,int,int> changeProfilepictureAction;
                
        public static Action<Data,int,int> changeMainaudioAction;
                


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
                    
                    private string  _title;
                    /// <summary>
                    ///标题
                    ///</summary>
                    public string  title{
                                get{return _title;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeTitle(this,_title,value); 
                    }
        
                _title = value;
                }
                 
                     }
                    
                    private string  _mainText;
                    /// <summary>
                    ///内容
                    ///</summary>
                    public string  mainText{
                                get{return _mainText;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeMaintext(this,_mainText,value); 
                    }
        
                _mainText = value;
                }
                 
                     }
                    
                    private int  _mainPicture;
                    /// <summary>
                    ///背景图片
                    ///</summary>
                    public int  mainPicture{
                                get{return _mainPicture;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeMainpicture(this,_mainPicture,value); 
                    }
        
                _mainPicture = value;
                }
                 
                     }
                    
                    private int  _mainVideo;
                    /// <summary>
                    ///背景视频
                    ///</summary>
                    public int  mainVideo{
                                get{return _mainVideo;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeMainvideo(this,_mainVideo,value); 
                    }
        
                _mainVideo = value;
                }
                 
                     }
                    
                    private int  _profilePicture;
                    /// <summary>
                    ///头像图片
                    ///</summary>
                    public int  profilePicture{
                                get{return _profilePicture;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeProfilepicture(this,_profilePicture,value); 
                    }
        
                _profilePicture = value;
                }
                 
                     }
                    
                    private int  _mainAudio;
                    /// <summary>
                    ///音声
                    ///</summary>
                    public int  mainAudio{
                                get{return _mainAudio;}
 set{

                    if(_DataByUid!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeMainaudio(this,_mainAudio,value); 
                    }
        
                _mainAudio = value;
                }
                 
                     }
                    
            public Data(int uid,string title,string mainText,int mainPicture,int mainVideo,int profilePicture,int mainAudio)
            {

             this.uid = uid;
             this.title = title;
             this.mainText = mainText;
             this.mainPicture = mainPicture;
             this.mainVideo = mainVideo;
             this.profilePicture = profilePicture;
             this.mainAudio = mainAudio;

            }
            public void Reset(Data data)
            {

             this.uid = data.uid;
             this.title = data.title;
             this.mainText = data.mainText;
             this.mainPicture = data.mainPicture;
             this.mainVideo = data.mainVideo;
             this.profilePicture = data.profilePicture;
             this.mainAudio = data.mainAudio;
            }

                public Data Copy(bool sameId = true)
                {
        return new Data(sameId? uid:uidChain.GetId(),title,mainText,mainPicture,mainVideo,profilePicture,mainAudio);
                }
            
            public virtual  void BeforeGet()
            {
                
                ClipForm.beforeGetAction?.Invoke(this);
            }
        }

                   private static Data _defaultData=new Data(0,"","",0,0,0,0);
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

                jo.SelectToken("title")==null?defaultData.title:jo.Get<string>("title"),

                jo.SelectToken("mainText")==null?defaultData.mainText:jo.Get<string>("mainText"),

                jo.SelectToken("mainPicture")==null?defaultData.mainPicture:jo.Get<int>("mainPicture"),

                jo.SelectToken("mainVideo")==null?defaultData.mainVideo:jo.Get<int>("mainVideo"),

                jo.SelectToken("profilePicture")==null?defaultData.profilePicture:jo.Get<int>("profilePicture"),

                jo.SelectToken("mainAudio")==null?defaultData.mainAudio:jo.Get<int>("mainAudio")
                    );

            return data;
        }

        public static JObject GetJoByData(Data data)
        {
            Init();
            data.BeforeGet();

            JObject jo=new JObject();

            jo.Set<int>("uid",data.uid);

            jo.Set<string>("title",data.title);

            jo.Set<string>("mainText",data.mainText);

            jo.Set<int>("mainPicture",data.mainPicture);

            jo.Set<int>("mainVideo",data.mainVideo);

            jo.Set<int>("profilePicture",data.profilePicture);

            jo.Set<int>("mainAudio",data.mainAudio);

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
            
            public static void ChangeTitle(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeTitleAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeMaintext(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeMaintextAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeMainpicture(Data superData,int oldV,int newV)
            {
                if(superData is Data data)
                {

                changeMainpictureAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeMainvideo(Data superData,int oldV,int newV)
            {
                if(superData is Data data)
                {

                changeMainvideoAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeProfilepicture(Data superData,int oldV,int newV)
            {
                if(superData is Data data)
                {

                changeProfilepictureAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeMainaudio(Data superData,int oldV,int newV)
            {
                if(superData is Data data)
                {

                changeMainaudioAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
    }
}
        