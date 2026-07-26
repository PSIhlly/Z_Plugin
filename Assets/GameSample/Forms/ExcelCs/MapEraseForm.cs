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

    public static partial class MapEraseForm
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
                
        public static Action<Data,string,string> changeLabelAction;
                


        public partial class Data : MapBaseForm.Data
        {

                    private bool  _terrain;
                    /// <summary>
                    ///清理地板
                    ///</summary>
                    public bool  terrain{
                                get{return _terrain;}
private set{
        
                _terrain = value;
                }
                 
                     }
                    
                    private bool  _mObject;
                    /// <summary>
                    ///清理景物
                    ///</summary>
                    public bool  mObject{
                                get{return _mObject;}
private set{
        
                _mObject = value;
                }
                 
                     }
                    
                    private bool  _item;
                    /// <summary>
                    ///清理物品
                    ///</summary>
                    public bool  item{
                                get{return _item;}
private set{
        
                _item = value;
                }
                 
                     }
                    
                    private bool  _character;
                    /// <summary>
                    ///清理角色
                    ///</summary>
                    public bool  character{
                                get{return _character;}
private set{
        
                _character = value;
                }
                 
                     }
                    
                    private bool  _texture;
                    /// <summary>
                    ///清理贴图
                    ///</summary>
                    public bool  texture{
                                get{return _texture;}
private set{
        
                _texture = value;
                }
                 
                     }
                    
            public Data(MapBaseForm.Data data):base(data.id,data.name,data.icon,data.label)
            {
            }
            
            public Data(int id,string name,int icon,string label,bool terrain,bool mObject,bool item,bool character,bool texture):base(id,name,icon,label)
            {

             this.id = id;
             this.name = name;
             this.icon = icon;
             this.label = label;
             this.terrain = terrain;
             this.mObject = mObject;
             this.item = item;
             this.character = character;
             this.texture = texture;

            }
            public void Reset(Data data)
            {

             this.id = data.id;
             this.name = data.name;
             this.icon = data.icon;
             this.label = data.label;
             this.terrain = data.terrain;
             this.mObject = data.mObject;
             this.item = data.item;
             this.character = data.character;
             this.texture = data.texture;
            }

                public Data Copy(bool sameId = true)
                {
        return new Data(sameId? id:idChain.GetId(),name,icon,label,terrain,mObject,item,character,texture);
                }
            
            public override  void BeforeGet()
            {
                base.BeforeGet();
                MapEraseForm.beforeGetAction?.Invoke(this);
            }
        }

                   private static Data _defaultData=new Data(0,"",0,"",false,false,false,false,false);
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

            MapBaseForm.Init();

        }
        public static void InitInternal()
        {
            if(inited)
                return;
            inited=true;  

        

                _DataById = new Dictionary<int, Data>() {

                {10000001,new Data(10000001,"all erase",0,"",true,true,true,true,false)},

                {10000002,new Data(10000002,"texture only",0,"",false,false,false,false,true)},

                {10000003,new Data(10000003,"remain terrain",0,"",false,true,true,true,false)},

                };
                _DatasHashSet=new HashSet<Data>();
                
                    _DataByName = new Dictionary<string, Data>() {
    
                        {"all erase",_DataById[10000001]},
    
                        {"texture only",_DataById[10000002]},
    
                        {"remain terrain",_DataById[10000003]},
    
                    
                    };
                    foreach(var v in _DataById.Values)
                    {
                        _DatasHashSet.Add(v);
                    }
    
                    _DatasByLabel = new Dictionary<string, List<Data>>() {
    
                            {"",new List<Data>()},
        
                };

                    _DatasByLabel[""].Add(_DataById[10000001]);

                    _DatasByLabel[""].Add(_DataById[10000002]);

                    _DatasByLabel[""].Add(_DataById[10000003]);


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

                jo.SelectToken("label")==null?defaultData.label:jo.Get<string>("label"),

                    _defaultData.terrain,

                    _defaultData.mObject,

                    _defaultData.item,

                    _defaultData.character,

                    _defaultData.texture
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

            jo.Set<string>("label",data.label);

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
                    
    
                    DataByName.Remove(data.name);
    
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

                    DataByName.Remove(oldV);
                    DataByName[newV]=data;
 
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
            
    }
}
        