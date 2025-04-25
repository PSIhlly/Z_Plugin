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

namespace Form
{

    public static partial class MapObjectForm
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

        public static Action childInitAction;
        public static Action<Data> childRemoveAction;
        public static Action<Data> childAddAction;

        public static Action<Data,int,int> changeIdAction;
                
        public static Action<Data,string,string> changeNameAction;
                
        public static Action<Data,string,string> changeIconAction;
                
        public static Action<Data,bool,bool> changeIsobstacleAction;
                
        public static Action<Data,List<string>,List<string>> changeSubprefabunitnameAction;
                
        public static Action<Data,List<Vector3>,List<Vector3>> changeSubprefabunitposAction;
                
        public static Action<Data,List<Vector3>,List<Vector3>> changeSubprefabunitscaleAction;
                
        public static Action<Data,List<string>,List<string>> changeSubunittexsnameAction;
                


        public partial class Data : MapBaseForm.Data
        {

                    private bool  _isObstacle;
                    /// <summary>
                    ///是否可通行
                    ///</summary>
                    public bool  isObstacle{
                                get{return _isObstacle;}
 set{

                    if(_DataById!=null&&_DataById.ContainsValue(this))
                    {
                       ChangeIsobstacle(this,_isObstacle,value); 
                    }
        
                _isObstacle = value;
                }
                 
                     }
                    
                    private List<string>  _subPrefabUnitName;
                    /// <summary>
                    ///子预制件
                    ///</summary>
                    public List<string>  subPrefabUnitName{
                                get{return _subPrefabUnitName;}
 set{

                    if(_DataById!=null&&_DataById.ContainsValue(this))
                    {
                       ChangeSubprefabunitname(this,_subPrefabUnitName,value); 
                    }
        
                _subPrefabUnitName = value;
                }
                 
                     }
                    
                    private List<Vector3>  _subPrefabUnitPos;
                    /// <summary>
                    ///子预制件坐标
                    ///</summary>
                    public List<Vector3>  subPrefabUnitPos{
                                get{return _subPrefabUnitPos;}
 set{

                    if(_DataById!=null&&_DataById.ContainsValue(this))
                    {
                       ChangeSubprefabunitpos(this,_subPrefabUnitPos,value); 
                    }
        
                _subPrefabUnitPos = value;
                }
                 
                     }
                    
                    private List<Vector3>  _subPrefabUnitScale;
                    /// <summary>
                    ///子预制件缩放
                    ///</summary>
                    public List<Vector3>  subPrefabUnitScale{
                                get{return _subPrefabUnitScale;}
 set{

                    if(_DataById!=null&&_DataById.ContainsValue(this))
                    {
                       ChangeSubprefabunitscale(this,_subPrefabUnitScale,value); 
                    }
        
                _subPrefabUnitScale = value;
                }
                 
                     }
                    
                    private List<string>  _subUnitTexsName;
                    /// <summary>
                    ///子预制件
                    ///</summary>
                    public List<string>  subUnitTexsName{
                                get{return _subUnitTexsName;}
 set{

                    if(_DataById!=null&&_DataById.ContainsValue(this))
                    {
                       ChangeSubunittexsname(this,_subUnitTexsName,value); 
                    }
        
                _subUnitTexsName = value;
                }
                 
                     }
                    
            public Data(int id,string name,string icon,bool isObstacle,List<string> subPrefabUnitName,List<Vector3> subPrefabUnitPos,List<Vector3> subPrefabUnitScale,List<string> subUnitTexsName):base(id,name,icon)
            {

             this.id = id;
             this.name = name;
             this.icon = icon;
             this.isObstacle = isObstacle;
             this.subPrefabUnitName = subPrefabUnitName;
             this.subPrefabUnitPos = subPrefabUnitPos;
             this.subPrefabUnitScale = subPrefabUnitScale;
             this.subUnitTexsName = subUnitTexsName;

            }

                public Data Copy()
                {
        return new Data(-1,name,icon,isObstacle,subPrefabUnitName,subPrefabUnitPos,subPrefabUnitScale,subUnitTexsName);
                }
            
        }

                   public static Data defaultData=new Data(0,"","",false,null,new List<Vector3>(){Vector3.zero,},new List<Vector3>(){Vector3.one,},new List<string>(){"z_map_b$floor$0",});


            static Dictionary<int, Data> _DataById;
            public static Dictionary<int, Data> DataById
            {
                get
                {
                    Init();
                    return _DataById;
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

                {400001,new Data(400001,"wall","",false,new List<string>(){"Cube",},new List<Vector3>(){Vector3.zero,},new List<Vector3>(){Vector3.one,},new List<string>(){"z_map_b$floor$0",})},

                };
                    _DataByName = new Dictionary<string, Data>() {
    
                        {"wall",_DataById[400001]},
    
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

                jo.Get<int>("id"),

                jo.Get<string>("name"),

                jo.Get<string>("icon"),

                jo.Get<bool>("isObstacle"),

                jo.Get<List<string>>("subPrefabUnitName"),

                jo.Get<List<Vector3>>("subPrefabUnitPos"),

                jo.Get<List<Vector3>>("subPrefabUnitScale"),

                jo.Get<List<string>>("subUnitTexsName")
                    );

            return data;
        }

        public static JObject GetJoByData(Data data)
        {
            Init();

            JObject jo=new JObject();

            jo.Set<int>("id",data.id);

            jo.Set<string>("name",data.name);

            jo.Set<string>("icon",data.icon);

            jo.Set<bool>("isObstacle",data.isObstacle);

            jo.Set<List<string>>("subPrefabUnitName",data.subPrefabUnitName);

            jo.Set<List<Vector3>>("subPrefabUnitPos",data.subPrefabUnitPos);

            jo.Set<List<Vector3>>("subPrefabUnitScale",data.subPrefabUnitScale);

            jo.Set<List<string>>("subUnitTexsName",data.subUnitTexsName);

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
    
                    DataByName[data.name]=data;
    
MapBaseForm.AddData(data);
            childAddAction?.Invoke(data);
            return data.id;
        }
        public static void RemoveData(int id)
        {            
            Init();
            if(!DataById.ContainsKey(id))
                return;
               
            var data=DataById[id];

                    DataById.Remove(data.id);
    
                    DataByName.Remove(data.name);
    
MapBaseForm.RemoveData(id);
            idChain.PushId(data.id);
            childRemoveAction?.Invoke(data);
        }
        public static void Clear()
        {
            Init();

                    DataById.Clear();
    
                    DataByName.Clear();
    
            idChain.Clear();
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
            
            public static void ChangeIcon(MapBaseForm.Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeIconAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeIsobstacle(Data superData,bool oldV,bool newV)
            {
                if(superData is Data data)
                {

                changeIsobstacleAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeSubprefabunitname(Data superData,List<string> oldV,List<string> newV)
            {
                if(superData is Data data)
                {

                changeSubprefabunitnameAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeSubprefabunitpos(Data superData,List<Vector3> oldV,List<Vector3> newV)
            {
                if(superData is Data data)
                {

                changeSubprefabunitposAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeSubprefabunitscale(Data superData,List<Vector3> oldV,List<Vector3> newV)
            {
                if(superData is Data data)
                {

                changeSubprefabunitscaleAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeSubunittexsname(Data superData,List<string> oldV,List<string> newV)
            {
                if(superData is Data data)
                {

                changeSubunittexsnameAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
    }
}
        