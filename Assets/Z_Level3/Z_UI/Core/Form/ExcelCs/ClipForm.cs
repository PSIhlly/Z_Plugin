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

        public static Action<Data,int,int> changeUidAction;
                
        public static Action<Data,string,string> changeTitleAction;
                
        public static Action<Data,string,string> changeMaintextAction;
                
        public static Action<Data,string,string> changeMainpicturenameAction;
                
        public static Action<Data,string,string> changeMainvideonameAction;
                
        public static Action<Data,string,string> changeProfilepicturenameAction;
                


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
                    
                    private string  _title;
                    /// <summary>
                    ///±êÌâ
                    ///</summary>
                    public string  title{
                                get{return _title;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeTitle(this,_title,value); 
                    }
        
                _title = value;
                }
                 
                     }
                    
                    private string  _mainText;
                    /// <summary>
                    ///ÄÚÈÝ
                    ///</summary>
                    public string  mainText{
                                get{return _mainText;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeMaintext(this,_mainText,value); 
                    }
        
                _mainText = value;
                }
                 
                     }
                    
                    private string  _mainPictureName;
                    /// <summary>
                    ///±³¾°Í¼Æ¬
                    ///</summary>
                    public string  mainPictureName{
                                get{return _mainPictureName;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeMainpicturename(this,_mainPictureName,value); 
                    }
        
                _mainPictureName = value;
                }
                 
                     }
                    
                    private string  _mainVideoName;
                    /// <summary>
                    ///±³¾°ÊÓÆµ
                    ///</summary>
                    public string  mainVideoName{
                                get{return _mainVideoName;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeMainvideoname(this,_mainVideoName,value); 
                    }
        
                _mainVideoName = value;
                }
                 
                     }
                    
                    private string  _profilePictureName;
                    /// <summary>
                    ///Í·ÏñÍ¼Æ¬
                    ///</summary>
                    public string  profilePictureName{
                                get{return _profilePictureName;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeProfilepicturename(this,_profilePictureName,value); 
                    }
        
                _profilePictureName = value;
                }
                 
                     }
                    
            public Data(int uid,string title,string mainText,string mainPictureName,string mainVideoName,string profilePictureName)
            {

             this.uid = uid;
             this.title = title;
             this.mainText = mainText;
             this.mainPictureName = mainPictureName;
             this.mainVideoName = mainVideoName;
             this.profilePictureName = profilePictureName;

            }

                public Data Copy(bool sameId = true)
                {
        return new Data(sameId? uid:uidChain.GetId(),title,mainText,mainPictureName,mainVideoName,profilePictureName);
                }
            
        }

                   private static Data _defaultData=new Data(0,"","","","","");
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

                jo.Get<string>("title"),

                jo.Get<string>("mainText"),

                jo.Get<string>("mainPictureName"),

                jo.Get<string>("mainVideoName"),

                jo.Get<string>("profilePictureName")
                    );

            return data;
        }

        public static JObject GetJoByData(Data data)
        {
            Init();

            JObject jo=new JObject();

            jo.Set<int>("uid",data.uid);

            jo.Set<string>("title",data.title);

            jo.Set<string>("mainText",data.mainText);

            jo.Set<string>("mainPictureName",data.mainPictureName);

            jo.Set<string>("mainVideoName",data.mainVideoName);

            jo.Set<string>("profilePictureName",data.profilePictureName);

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
            
            public static void ChangeMainpicturename(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeMainpicturenameAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeMainvideoname(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeMainvideonameAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeProfilepicturename(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeProfilepicturenameAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
    }
}
        