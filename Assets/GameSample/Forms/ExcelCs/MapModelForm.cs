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

namespace Form
{

    public static partial class MapModelForm
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

        public static Action childInitAction;
        public static Action<Data> childRemoveAction;
        public static Action<Data> childAddAction;

        public static Action<Data,int,int> changeIdAction;
                
        public static Action<Data,List<string>,List<string>> changeSubprefabunitnameAction;
                
        public static Action<Data,List<Vector3>,List<Vector3>> changeSubprefabunitposAction;
                
        public static Action<Data,List<Vector3>,List<Vector3>> changeSubprefabunitscaleAction;
                
        public static Action<Data,List<string>,List<string>> changeSubunittexsnameAction;
                
        public static Action<Data,bool,bool> changeIsobstacleAction;
                


        public partial class Data
        {

                    private int  _id;
                    /// <summary>
                    ///
                    ///</summary>
                    public int  id{
                                get{return _id;}
 set{

                    if(_DataById!=null&&_DataById.ContainsValue(this))
                    {
                       ChangeId(this,_id,value); 
                    }
        
                _id = value;
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
                    ///子预制件贴图
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
                    
            public Data(int id,List<string> subPrefabUnitName,List<Vector3> subPrefabUnitPos,List<Vector3> subPrefabUnitScale,List<string> subUnitTexsName,bool isObstacle)
            {

             this.id = id;
             this.subPrefabUnitName = subPrefabUnitName;
             this.subPrefabUnitPos = subPrefabUnitPos;
             this.subPrefabUnitScale = subPrefabUnitScale;
             this.subUnitTexsName = subUnitTexsName;
             this.isObstacle = isObstacle;

            }

                public Data Copy(bool sameId = true)
                {
        return new Data(sameId? id:idChain.GetId(),new List<string>(subPrefabUnitName),new List<Vector3>(subPrefabUnitPos),new List<Vector3>(subPrefabUnitScale),new List<string>(subUnitTexsName),isObstacle);
                }
            
        }

                   private static Data _defaultData=new Data(0,new List<string>(){"Cube",},new List<Vector3>(){Vector3.zero,},new List<Vector3>(){Vector3.one,},new List<string>(){"z_map_b$floor$0",},false);
                   public static Data defaultData=>_defaultData.Copy();


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

                jo.Get<int>("id"),

                jo.Get<List<string>>("subPrefabUnitName"),

                jo.Get<List<Vector3>>("subPrefabUnitPos"),

                jo.Get<List<Vector3>>("subPrefabUnitScale"),

                jo.Get<List<string>>("subUnitTexsName"),

                jo.Get<bool>("isObstacle")
                    );

            return data;
        }

        public static JObject GetJoByData(Data data)
        {
            Init();

            JObject jo=new JObject();

            jo.Set<int>("id",data.id);

            jo.Set<List<string>>("subPrefabUnitName",data.subPrefabUnitName);

            jo.Set<List<Vector3>>("subPrefabUnitPos",data.subPrefabUnitPos);

            jo.Set<List<Vector3>>("subPrefabUnitScale",data.subPrefabUnitScale);

            jo.Set<List<string>>("subUnitTexsName",data.subUnitTexsName);

            jo.Set<bool>("isObstacle",data.isObstacle);

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
    

            idChain.PushId(data.id);
            childRemoveAction?.Invoke(data);
        }
        public static void Clear()
        {
            Init();

                    DataById.Clear();
    
            idChain.Clear();
        }
        
        public static void ClearAuto()
        {
            Init();
            var keys = new List<int>(DataById.Keys);
            foreach(var key in keys)
            {
                if(key < idChain.cnt)
                    RemoveData(key);
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
            
            public static void ChangeIsobstacle(Data superData,bool oldV,bool newV)
            {
                if(superData is Data data)
                {

                changeIsobstacleAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
    }
}
        