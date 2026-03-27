using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Z_ByteSerialize;
using Z_DesignStyle;

namespace Z_Code.Form
{

    public static partial class BoxDataForm
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
                
        public static Action<Data,string,string> changeStrAction;
                
        public static Action<Data,string,string> changeValnameAction;
                
        public static Action<Data,float,float> changeNumAction;
                
        public static Action<Data,Dictionary<string,BoxDataForm.Data>,Dictionary<string,BoxDataForm.Data>> changeDicAction;
                


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
                    
                    private string  _str;
                    /// <summary>
                    ///字符串
                    ///</summary>
                    public string  str{
                                get{return _str;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeStr(this,_str,value); 
                    }
        
                _str = value;
                }
                 
                     }
                    
                    private string  _valName;
                    /// <summary>
                    ///指向名称
                    ///</summary>
                    public string  valName{
                                get{return _valName;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeValname(this,_valName,value); 
                    }
        
                _valName = value;
                }
                 
                     }
                    
                    private float  _num;
                    /// <summary>
                    ///数值
                    ///</summary>
                    public float  num{
                                get{return _num;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeNum(this,_num,value); 
                    }
        
                _num = value;
                }
                 
                     }
                    
                    private Dictionary<string,BoxDataForm.Data>  _dic;
                    /// <summary>
                    ///字典
                    ///</summary>
                    public Dictionary<string,BoxDataForm.Data>  dic{
                                get{return _dic;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeDic(this,_dic,value); 
                    }
        
                _dic = value;
                }
                 
                     }
                    
            public Data(int uid,string str,string valName,float num,Dictionary<string,BoxDataForm.Data> dic)
            {

             this.uid = uid;
             this.str = str;
             this.valName = valName;
             this.num = num;
             this.dic = dic;

            }
            public void Reset(Data data)
            {

             this.uid = data.uid;
             this.str = data.str;
             this.valName = data.valName;
             this.num = data.num;
             this.dic = data.dic;
            }

                public Data Copy(bool sameId = true)
                {
        return new Data(sameId? uid:uidChain.GetId(),str,valName,num,new Dictionary<string,BoxDataForm.Data>(dic));
                }
            
            public virtual  void BeforeGet()
            {
                
                BoxDataForm.beforeGetAction?.Invoke(this);
            }
        }

                   private static Data _defaultData=new Data(0,null,null,0f,new Dictionary<string,BoxDataForm.Data>(){});
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

                jo.SelectToken("uid")==null?defaultData.uid:jo.Get<int>("uid"),

                jo.SelectToken("str")==null?defaultData.str:jo.Get<string>("str"),

                jo.SelectToken("valName")==null?defaultData.valName:jo.Get<string>("valName"),

                jo.SelectToken("num")==null?defaultData.num:jo.Get<float>("num"),

                jo.SelectToken("dic")==null?defaultData.dic:jo.Get<Dictionary<string,BoxDataForm.Data>>("dic")
                    );

            return data;
        }

        public static JObject GetJoByData(Data data)
        {
            Init();
            data.BeforeGet();

            JObject jo=new JObject();

            jo.Set<int>("uid",data.uid);

            jo.Set<string>("str",data.str);

            jo.Set<string>("valName",data.valName);

            jo.Set<float>("num",data.num);

            jo.Set<Dictionary<string,BoxDataForm.Data>>("dic",data.dic);

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
            
            public static void ChangeStr(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeStrAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeValname(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeValnameAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeNum(Data superData,float oldV,float newV)
            {
                if(superData is Data data)
                {

                changeNumAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeDic(Data superData,Dictionary<string,BoxDataForm.Data> oldV,Dictionary<string,BoxDataForm.Data> newV)
            {
                if(superData is Data data)
                {

                changeDicAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
    }
}
        