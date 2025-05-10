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

        public static Action childInitAction;
        public static Action<Data> childRemoveAction;
        public static Action<Data> childAddAction;

        public static Action<Data,int,int> changeUidAction;
                
        public static Action<Data,string,string> changeTitleAction;
                
        public static Action<Data,string,string> changeMaintextAction;
                
        public static Action<Data,string,string> changeMainpictureAction;
                
        public static Action<Data,string,string> changeProfilepictureAction;
                


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
                    
                    private string  _mainPicture;
                    /// <summary>
                    ///±³¾°Í¼Æ¬
                    ///</summary>
                    public string  mainPicture{
                                get{return _mainPicture;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeMainpicture(this,_mainPicture,value); 
                    }
        
                _mainPicture = value;
                }
                 
                     }
                    
                    private string  _profilePicture;
                    /// <summary>
                    ///Í·ÏñÍ¼Æ¬
                    ///</summary>
                    public string  profilePicture{
                                get{return _profilePicture;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeProfilepicture(this,_profilePicture,value); 
                    }
        
                _profilePicture = value;
                }
                 
                     }
                    
            public Data(int uid,string title,string mainText,string mainPicture,string profilePicture)
            {

             this.uid = uid;
             this.title = title;
             this.mainText = mainText;
             this.mainPicture = mainPicture;
             this.profilePicture = profilePicture;

            }

                public Data Copy(bool sameId = true)
                {
        return new Data(sameId? uid:uidChain.GetId(),title,mainText,mainPicture,profilePicture);
                }
            
        }

                   public static Data defaultData=new Data(0,"","","","");


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

                jo.Get<string>("mainPicture"),

                jo.Get<string>("profilePicture")
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

            jo.Set<string>("mainPicture",data.mainPicture);

            jo.Set<string>("profilePicture",data.profilePicture);

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
            
            public static void ChangeMainpicture(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeMainpictureAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeProfilepicture(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeProfilepictureAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
    }
}
        