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

    public static partial class MapTextureForm
    {

        

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Register()
        {

                MapBaseForm.childInitAction+=InitInternal;


                MapBaseForm.childRemoveAction+=RemoveChildren;
                MapBaseForm.childAddAction+=AddChildren;
            

            MapBaseForm.changeIdAction+=ChangeId;

            MapBaseForm.changeNameAction+=ChangeName;

            MapBaseForm.changeIconAction+=ChangeIcon;

            MapBaseForm.changeLabelAction+=ChangeLabel;

            Z_Json.extra[typeof(Data)]=((obj)=>{
            if(obj is Data data)
                return GetJoByData(data);
            return null;
            },(jo)=>{
            return GetDataByJo(jo);
            });
        }
        
        private static bool inited;

        public static Z_Chain.Chain idChain =>MapBaseForm.idChain;

        public static Action<Data> addAction;
        public static Action<Data> removeAction;
        public static Action childInitAction;
        public static Action<Data> childRemoveAction;
        public static Action<Data> childAddAction;
        
        public static Action<Data> beforeGetAction;

        public static Action<Data,int,int> changeIdAction;
                
        public static Action<Data,string,string> changeNameAction;
                
        public static Action<Data,int,int> changeIconAction;
                
        public static Action<Data,float,float> changeAnimtimeintervalAction;
                
        public static Action<Data,List<int>,List<int>> changeTexsAction;
                
        public static Action<Data,string,string> changeLabelAction;
                
        public static Action<Data,Dictionary<string,EventTriggerForm.Data>,Dictionary<string,EventTriggerForm.Data>> changeEventsAction;
                


        public partial class Data : MapBaseForm.Data
        {

                    private float  _animTimeInterval;
                    /// <summary>
                    ///播放间隔时间
                    ///</summary>
                    public float  animTimeInterval{
                                get{return _animTimeInterval;}
 set{

                    if(_DataById!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeAnimtimeinterval(this,_animTimeInterval,value); 
                    }
        
                _animTimeInterval = value;
                }
                 
                     }
                    
                    private List<int>  _texs;
                    /// <summary>
                    ///贴图名称
                    ///</summary>
                    public List<int>  texs{
                                get{return _texs;}
 set{

                    if(_DataById!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeTexs(this,_texs,value); 
                    }
        
                _texs = value;
                }
                 
                     }
                    
                    private Dictionary<string,EventTriggerForm.Data>  _events;
                    /// <summary>
                    ///事件
                    ///</summary>
                    public Dictionary<string,EventTriggerForm.Data>  events{
                                get{return _events;}
 set{

                    if(_DataById!=null&&_DatasHashSet.Contains(this))
                    {
                       ChangeEvents(this,_events,value); 
                    }
        
                _events = value;
                }
                 
                     }
                    
            public Data(MapBaseForm.Data data):base(data.id,data.name,data.icon,data.label)
            {
            }
            
            public Data(int id,string name,int icon,float animTimeInterval,List<int> texs,string label,Dictionary<string,EventTriggerForm.Data> events):base(id,name,icon,label)
            {

             this.id = id;
             this.name = name;
             this.icon = icon;
             this.animTimeInterval = animTimeInterval;
             this.texs = texs;
             this.label = label;
             this.events = events;

            }
            public void Reset(Data data)
            {

             this.id = data.id;
             this.name = data.name;
             this.icon = data.icon;
             this.animTimeInterval = data.animTimeInterval;
             this.texs = data.texs;
             this.label = data.label;
             this.events = data.events;
            }

                public Data Copy(bool sameId = true)
                {
        return new Data(sameId? id:idChain.GetId(),name,icon,animTimeInterval,texs==null?new List<int>():new List<int>(texs),label,events==null?new Dictionary<string,EventTriggerForm.Data>():new Dictionary<string,EventTriggerForm.Data>(events));
                }
            
            public override  void BeforeGet()
            {
                base.BeforeGet();
                MapTextureForm.beforeGetAction?.Invoke(this);
            }
        }

                   private static Data _defaultData=new Data(0,"",0,0f,null,"",new Dictionary<string,EventTriggerForm.Data>(){});
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
    
            static Dictionary<string, List<Data>> _DatasByName;
            public static Dictionary<string, List<Data>> DatasByName
            {
                get
                {
                    Init();
                    return _DatasByName;
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
    

        static public void Init()
        {

            MapBaseForm.Init();

        }
        public static void InitInternal()
        {
            if(inited)
                return;
            inited=true;  

        

                _DataById = new Dictionary<int, Data>() {

                };
                _DatasHashSet=new HashSet<Data>();
                
                    _DatasByName = new Dictionary<string, List<Data>>() {
    
                };

                    _DatasByLabel = new Dictionary<string, List<Data>>() {
    
                };


            childInitAction?.Invoke();
            

            foreach(var data in DataById.Values)
            {
                MapBaseForm.AddData(data);
            }


        
             
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

                jo.SelectToken("icon")==null?defaultData.icon:jo.Get<int>("icon"),

                jo.SelectToken("animTimeInterval")==null?defaultData.animTimeInterval:jo.Get<float>("animTimeInterval"),

                jo.SelectToken("texs")==null?defaultData.texs:jo.Get<List<int>>("texs"),

                jo.SelectToken("label")==null?defaultData.label:jo.Get<string>("label"),

                jo.SelectToken("events")==null?defaultData.events:jo.Get<Dictionary<string,EventTriggerForm.Data>>("events")
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

            jo.Set<int>("icon",data.icon);

            jo.Set<float>("animTimeInterval",data.animTimeInterval);

            jo.Set<List<int>>("texs",data.texs);

            jo.Set<string>("label",data.label);

            jo.Set<Dictionary<string,EventTriggerForm.Data>>("events",data.events);

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
    
                    if(!DatasByName.ContainsKey(data.name))
                        DatasByName[data.name]=new List<Data>();
                    DatasByName[data.name].Add(data);
    
                    if(!DatasByLabel.ContainsKey(data.label))
                        DatasByLabel[data.label]=new List<Data>();
                    DatasByLabel[data.label].Add(data);
    
MapBaseForm.AddData(data);
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
                    
    
                    if(DatasByName.ContainsKey(data.name))
                    {
                        DatasByName[data.name].Remove(data);
                        if(DatasByName[data.name].Count==0)
                            DatasByName.Remove(data.name);
                    }
                    
    
                    if(DatasByLabel.ContainsKey(data.label))
                    {
                        DatasByLabel[data.label].Remove(data);
                        if(DatasByLabel[data.label].Count==0)
                            DatasByLabel.Remove(data.label);
                    }
                    
    
MapBaseForm.RemoveData(id);
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

         private static void RemoveChildren(MapBaseForm.Data data)
        {
            Init();
            if(data is Data)
               RemoveData(data.id);      
        }
         private static void AddChildren(MapBaseForm.Data superData)
        {
            Init();
            if(superData is Data data)
               AddData(data);      
        }
        




            public static void ChangeId(MapBaseForm.Data superData,int oldV,int newV)
            {
                if(superData is Data data)
                {

                changeIdAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeName(MapBaseForm.Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                    DatasByName[oldV].Remove(data);
                    if(DatasByName[oldV].Count==0)
                        DatasByName.Remove(oldV);
                    if(!DatasByName.ContainsKey(newV))
                        DatasByName[newV]=new List<Data>();
                    DatasByName[newV].Add(data);
 
                changeNameAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeIcon(MapBaseForm.Data superData,int oldV,int newV)
            {
                if(superData is Data data)
                {

                changeIconAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeAnimtimeinterval(Data superData,float oldV,float newV)
            {
                if(superData is Data data)
                {

                changeAnimtimeintervalAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeTexs(Data superData,List<int> oldV,List<int> newV)
            {
                if(superData is Data data)
                {

                changeTexsAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeLabel(MapBaseForm.Data superData,string oldV,string newV)
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
            
            public static void ChangeEvents(Data superData,Dictionary<string,EventTriggerForm.Data> oldV,Dictionary<string,EventTriggerForm.Data> newV)
            {
                if(superData is Data data)
                {

                changeEventsAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
    }
}
        