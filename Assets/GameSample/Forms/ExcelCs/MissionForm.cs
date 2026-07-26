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

    public static partial class MissionForm
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
                
        public static Action<Data,string,string> changeNameAction;
                
        public static Action<Data,string,string> changeLabelAction;
                
        public static Action<Data,string,string> changeDescAction;
                
        public static Action<Data,bool,bool> changeShowAction;
                
        public static Action<Data,bool,bool> changeReceivedAction;
                
        public static Action<Data,bool,bool> changeDoneAction;
                
        public static Action<Data,bool,bool> changeFailAction;
                
        public static Action<Data,int,int> changeTargetsceneidAction;
                
        public static Action<Data,Vector3,Vector3> changeTargetposAction;
                
        public static Action<Data,float,float> changeRadiusAction;
                


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
                    
                    private string  _name;
                    /// <summary>
                    ///名字
                    ///</summary>
                    public string  name{
                                get{return _name;}
 set{

                    if(_DataById!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeName(this,_name,value); 
                    }
        
                _name = value;
                }
                 
                     }
                    
                    private string  _label;
                    /// <summary>
                    ///标签
                    ///</summary>
                    public string  label{
                                get{return _label;}
 set{

                    if(_DataById!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeLabel(this,_label,value); 
                    }
        
                _label = value;
                }
                 
                     }
                    
                    private string  _desc;
                    /// <summary>
                    ///说明
                    ///</summary>
                    public string  desc{
                                get{return _desc;}
 set{

                    if(_DataById!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeDesc(this,_desc,value); 
                    }
        
                _desc = value;
                }
                 
                     }
                    
                    private bool  _show;
                    /// <summary>
                    ///显示
                    ///</summary>
                    public bool  show{
                                get{return _show;}
 set{

                    if(_DataById!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeShow(this,_show,value); 
                    }
        
                _show = value;
                }
                 
                     }
                    
                    private bool  _received;
                    /// <summary>
                    ///已接收
                    ///</summary>
                    public bool  received{
                                get{return _received;}
 set{

                    if(_DataById!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeReceived(this,_received,value); 
                    }
        
                _received = value;
                }
                 
                     }
                    
                    private bool  _done;
                    /// <summary>
                    ///已完成
                    ///</summary>
                    public bool  done{
                                get{return _done;}
 set{

                    if(_DataById!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeDone(this,_done,value); 
                    }
        
                _done = value;
                }
                 
                     }
                    
                    private bool  _fail;
                    /// <summary>
                    ///失败
                    ///</summary>
                    public bool  fail{
                                get{return _fail;}
 set{

                    if(_DataById!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeFail(this,_fail,value); 
                    }
        
                _fail = value;
                }
                 
                     }
                    
                    private int  _targetSceneId;
                    /// <summary>
                    ///目标场景id
                    ///</summary>
                    public int  targetSceneId{
                                get{return _targetSceneId;}
 set{

                    if(_DataById!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeTargetsceneid(this,_targetSceneId,value); 
                    }
        
                _targetSceneId = value;
                }
                 
                     }
                    
                    private Vector3  _targetPos;
                    /// <summary>
                    ///目标位置
                    ///</summary>
                    public Vector3  targetPos{
                                get{return _targetPos;}
 set{

                    if(_DataById!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeTargetpos(this,_targetPos,value); 
                    }
        
                _targetPos = value;
                }
                 
                     }
                    
                    private float  _radius;
                    /// <summary>
                    ///半径
                    ///</summary>
                    public float  radius{
                                get{return _radius;}
 set{

                    if(_DataById!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeRadius(this,_radius,value); 
                    }
        
                _radius = value;
                }
                 
                     }
                    
            public Data(int id,string name,string label,string desc,bool show,bool received,bool done,bool fail,int targetSceneId,Vector3 targetPos,float radius)
            {

             this.id = id;
             this.name = name;
             this.label = label;
             this.desc = desc;
             this.show = show;
             this.received = received;
             this.done = done;
             this.fail = fail;
             this.targetSceneId = targetSceneId;
             this.targetPos = targetPos;
             this.radius = radius;

            }
            public void Reset(Data data)
            {

             this.id = data.id;
             this.name = data.name;
             this.label = data.label;
             this.desc = data.desc;
             this.show = data.show;
             this.received = data.received;
             this.done = data.done;
             this.fail = data.fail;
             this.targetSceneId = data.targetSceneId;
             this.targetPos = data.targetPos;
             this.radius = data.radius;
            }

                public Data Copy(bool sameId = true)
                {
        return new Data(sameId? id:idChain.GetId(),name,label,desc,show,received,done,fail,targetSceneId,targetPos,radius);
                }
            
            public virtual  void BeforeGet()
            {
                
                MissionForm.beforeGetAction?.Invoke(this);
            }
        }

                   private static Data _defaultData=new Data(0,"","","",false,false,false,false,0,Vector3.zero,0f);
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
    
            static Dictionary<string, List<Data>> _DatasByLabel;
            public static Dictionary<string, List<Data>> DatasByLabel
            {
                get
                {
                    Init();
                    return _DatasByLabel;
                }
            }
    
            static Dictionary<string, Data> _DataByName;
            public static Dictionary<string, Data> DataByName
            {
                get
                {
                    Init();
                    return _DataByName;
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
                
                    _DataByName = new Dictionary<string, Data>() {
    
                    
                    };
                    foreach(var v in _DataById.Values)
                    {
                        _DatasHashSet.Add(v);
                    }
    
                    _DatasByLabel = new Dictionary<string, List<Data>>() {
    
                };


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

                jo.SelectToken("name")==null?defaultData.name:jo.Get<string>("name"),

                jo.SelectToken("label")==null?defaultData.label:jo.Get<string>("label"),

                jo.SelectToken("desc")==null?defaultData.desc:jo.Get<string>("desc"),

                jo.SelectToken("show")==null?defaultData.show:jo.Get<bool>("show"),

                jo.SelectToken("received")==null?defaultData.received:jo.Get<bool>("received"),

                jo.SelectToken("done")==null?defaultData.done:jo.Get<bool>("done"),

                jo.SelectToken("fail")==null?defaultData.fail:jo.Get<bool>("fail"),

                jo.SelectToken("targetSceneId")==null?defaultData.targetSceneId:jo.Get<int>("targetSceneId"),

                jo.SelectToken("targetPos")==null?defaultData.targetPos:jo.Get<Vector3>("targetPos"),

                jo.SelectToken("radius")==null?defaultData.radius:jo.Get<float>("radius")
                    );

            return data;
        }

        public static JObject GetJoByData(Data data)
        {
            Init();
            data.BeforeGet();

            JObject jo=new JObject();

            jo.Set<int>("id",data.id);

            jo.Set<string>("name",data.name);

            jo.Set<string>("label",data.label);

            jo.Set<string>("desc",data.desc);

            jo.Set<bool>("show",data.show);

            jo.Set<bool>("received",data.received);

            jo.Set<bool>("done",data.done);

            jo.Set<bool>("fail",data.fail);

            jo.Set<int>("targetSceneId",data.targetSceneId);

            jo.Set<Vector3>("targetPos",data.targetPos);

            jo.Set<float>("radius",data.radius);

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
    
                    DataByName[data.name]=data;
    
                    if(!DatasByLabel.ContainsKey(data.label))
                        DatasByLabel[data.label]=new List<Data>();
                    DatasByLabel[data.label].Add(data);
    

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
                    
    
                    DataByName.Remove(data.name);
    
                    if(DatasByLabel.ContainsKey(data.label))
                    {
                        DatasByLabel[data.label].Remove(data);
                        if(DatasByLabel[data.label].Count==0)
                            DatasByLabel.Remove(data.label);
                    }
                    
    

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
            
            public static void ChangeName(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                    DataByName.Remove(oldV);
                    DataByName[newV]=data;
 
                changeNameAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeLabel(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                    DatasByLabel[oldV].Remove(data);
                    if(DatasByLabel[oldV].Count==0)
                        DatasByLabel.Remove(oldV);
                    if(!DatasByLabel.ContainsKey(newV))
                        DatasByLabel[newV]=new List<Data>();
                    DatasByLabel[newV].Add(data);
 
                changeLabelAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeDesc(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeDescAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeShow(Data superData,bool oldV,bool newV)
            {
                if(superData is Data data)
                {

                changeShowAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeReceived(Data superData,bool oldV,bool newV)
            {
                if(superData is Data data)
                {

                changeReceivedAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeDone(Data superData,bool oldV,bool newV)
            {
                if(superData is Data data)
                {

                changeDoneAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeFail(Data superData,bool oldV,bool newV)
            {
                if(superData is Data data)
                {

                changeFailAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeTargetsceneid(Data superData,int oldV,int newV)
            {
                if(superData is Data data)
                {

                changeTargetsceneidAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeTargetpos(Data superData,Vector3 oldV,Vector3 newV)
            {
                if(superData is Data data)
                {

                changeTargetposAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeRadius(Data superData,float oldV,float newV)
            {
                if(superData is Data data)
                {

                changeRadiusAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
    }
}
        