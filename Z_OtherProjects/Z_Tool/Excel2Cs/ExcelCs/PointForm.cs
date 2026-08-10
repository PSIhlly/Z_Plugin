using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Z_ByteSerialize;
using Z_DesignStyle;

namespace Form
{

    public static partial class PointForm
    {
public static readonly int autoIdCnt=100;

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

        public static Z_Chain.Chain idChain ;

        public static Action<Data> addAction;
        public static Action<Data> removeAction;
        public static Action childInitAction;
        public static Action<Data> childRemoveAction;
        public static Action<Data> childAddAction;
        
        public static Action<Data> beforeGetAction;

        public static Action<Data,int,int> changeIdAction;
                
        public static Action<Data,string,string> changeQuestionAction;
                
        public static Action<Data,string,string> changeAnswerAction;
                
        public static Action<Data,int,int> changeCorrecttimesAction;
                
        public static Action<Data,long,long> changeReviewtimeAction;
                
        public static Action<Data,int,int> changeParentidAction;
                


        public partial class Data
        {

                    private int  _id;
                    /// <summary>
                    ///
                    ///</summary>
                    public int  id{
                                get{return _id;}
 set{

                    if(_DataById!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeId(this,_id,value); 
                    }
        
                _id = value;
                }
                 
                     }
                    
                    private string  _question;
                    /// <summary>
                    ///问题
                    ///</summary>
                    public string  question{
                                get{return _question;}
 set{

                    if(_DataById!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeQuestion(this,_question,value); 
                    }
        
                _question = value;
                }
                 
                     }
                    
                    private string  _answer;
                    /// <summary>
                    ///答案
                    ///</summary>
                    public string  answer{
                                get{return _answer;}
 set{

                    if(_DataById!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeAnswer(this,_answer,value); 
                    }
        
                _answer = value;
                }
                 
                     }
                    
                    private int  _correctTimes;
                    /// <summary>
                    ///回答正确次数
                    ///</summary>
                    public int  correctTimes{
                                get{return _correctTimes;}
 set{

                    if(_DataById!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeCorrecttimes(this,_correctTimes,value); 
                    }
        
                _correctTimes = value;
                }
                 
                     }
                    
                    private long  _reviewTime;
                    /// <summary>
                    ///复习时间戳
                    ///</summary>
                    public long  reviewTime{
                                get{return _reviewTime;}
 set{

                    if(_DataById!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeReviewtime(this,_reviewTime,value); 
                    }
        
                _reviewTime = value;
                }
                 
                     }
                    
                    private int  _parentId;
                    /// <summary>
                    ///父节点Id
                    ///</summary>
                    public int  parentId{
                                get{return _parentId;}
 set{

                    if(_DataById!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeParentid(this,_parentId,value); 
                    }
        
                _parentId = value;
                }
                 
                     }
                    
            public Data(int id,string question,string answer,int correctTimes,long reviewTime,int parentId)
            {

             this.id = id;
             this.question = question;
             this.answer = answer;
             this.correctTimes = correctTimes;
             this.reviewTime = reviewTime;
             this.parentId = parentId;

            }
            public void Reset(Data data)
            {

             this.id = data.id;
             this.question = data.question;
             this.answer = data.answer;
             this.correctTimes = data.correctTimes;
             this.reviewTime = data.reviewTime;
             this.parentId = data.parentId;
            }

                public Data Copy(bool sameId = true)
                {
        return new Data(sameId? id:idChain.GetId(),question,answer,correctTimes,reviewTime,parentId);
                }
            
            public virtual  void BeforeGet()
            {
                
                PointForm.beforeGetAction?.Invoke(this);
            }
        }

                   private static Data _defaultData=new Data(0,"","",0,0,0);
                   public static Data defaultData=>_defaultData.Copy();


            static HashSet<Data> _DatasHashSet;
            static Dictionary<int, Data> _DataById;
            public static Dictionary<int, Data> DataById
            {
                get
                {
                    Init();
                    return _DataById;
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
idChain=new Z_Chain.Chain (autoIdCnt);

                _DataById = new Dictionary<int, Data>() {

                };
                _DatasHashSet=new HashSet<Data>();
                

            childInitAction?.Invoke();
            

foreach(var k in _DataById.Keys){ idChain.PopId(k); }
             
        }


        public static List<Data> GetDatasByJa(JArray ja)
        {
            Init();
            List<Data> lst=new List<Data>();
            foreach(JObject jo in ja)
            {
                if(jo.Get<int>("id")==0)
                    continue;
                lst.Add(GetDataByJo(jo));
            }
            return lst;
        }

        public static JArray GetJaByDatas()
        {
            Init();
            JArray ja=new JArray();
            foreach(Data data in _DataById.Values)
            {
                if(data.id==0)
                    continue;
                ja.Add(GetJoByData(data));
            }
            return ja;
        }

        public static Data GetDataByJo(JObject jo)
        {
            Init();

            Data data=new Data(

                jo.SelectToken("id")==null?defaultData.id:jo.Get<int>("id"),

                jo.SelectToken("question")==null?defaultData.question:jo.Get<string>("question"),

                jo.SelectToken("answer")==null?defaultData.answer:jo.Get<string>("answer"),

                jo.SelectToken("correctTimes")==null?defaultData.correctTimes:jo.Get<int>("correctTimes"),

                jo.SelectToken("reviewTime")==null?defaultData.reviewTime:jo.Get<long>("reviewTime"),

                jo.SelectToken("parentId")==null?defaultData.parentId:jo.Get<int>("parentId")
                    );

            return data;
        }

        public static JObject GetJoByData(Data data)
        {
            Init();
            data.BeforeGet();

            JObject jo=new JObject();

            jo.Set<int>("id",data.id);

            jo.Set<string>("question",data.question);

            jo.Set<string>("answer",data.answer);

            jo.Set<int>("correctTimes",data.correctTimes);

            jo.Set<long>("reviewTime",data.reviewTime);

            jo.Set<int>("parentId",data.parentId);

            return jo;
        }


        public static int AddData(Data data)
        {
            Init();
            if(DataById.ContainsKey(data.id))
                return data.id;
            if(data.id==-1)
            { 
                int id=idChain.GetId();
                if(id==-1)
                    return -1;
                data.id=id;  
            }
            idChain.PopId(data.id);

        DataById[data.id]=data;
        _DatasHashSet.Add(data);
    

            childAddAction?.Invoke(data);
            addAction?.Invoke(data);
            return data.id;
        }
        public static void RemoveData(int id)
        {            
            Init();
            if(!DataById.ContainsKey(id))
                return;
               
            var data=DataById[id];

                    _DatasHashSet.Remove(DataById[data.id]);
                    DataById.Remove(data.id);
                    
    

            idChain.PushId(data.id);
            childRemoveAction?.Invoke(data);
            removeAction?.Invoke(data);
        }
        public static void Clear()
        {
            Init();
            var keys = new List<int>(DataById.Keys);
            foreach(var key in keys)
            {
                    RemoveData(key);
            }

        }
        
        public static void ClearAuto()
        {
            Init();
            var keys = new List<int>(DataById.Keys);
            foreach(var key in keys)
            {
                if(key < idChain.cnt)
                    {
                        RemoveData(key);
                    }
            }
        }

         private static void RemoveChildren(Data data)
        {
            Init();
            if(data is Data)
               RemoveData(data.id);      
        }
         private static void AddChildren(Data superData)
        {
            Init();
            if(superData is Data data)
               AddData(data);      
        }
        




            public static void ChangeId(Data superData,int oldV,int newV)
            {
                if(superData is Data data)
                {

                changeIdAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeQuestion(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeQuestionAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeAnswer(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeAnswerAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeCorrecttimes(Data superData,int oldV,int newV)
            {
                if(superData is Data data)
                {

                changeCorrecttimesAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeReviewtime(Data superData,long oldV,long newV)
            {
                if(superData is Data data)
                {

                changeReviewtimeAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeParentid(Data superData,int oldV,int newV)
            {
                if(superData is Data data)
                {

                changeParentidAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
    }
}
        