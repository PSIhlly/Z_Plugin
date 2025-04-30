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

namespace Z_Map.Form
{

    public static partial class MapMainForm
    {
public static readonly int autoUidCnt=1000000;

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
                
        public static Action<Data,Vector3,Vector3> changeMapunitsizeAction;
                
        public static Action<Data,Vector3Int,Vector3Int> changeLogicsizeAction;
                
        public static Action<Data,Vector3Int,Vector3Int> changeViewsizeAction;
                
        public static Action<Data,string,string> changeMapjaAction;
                
        public static Action<Data,string,string> changeObjectjaAction;
                
        public static Action<Data,string,string> changeCharacterjaAction;
                


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
                    
                    private Vector3  _mapUnitSize;
                    /// <summary>
                    ///单位图块大小
                    ///</summary>
                    public Vector3  mapUnitSize{
                                get{return _mapUnitSize;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeMapunitsize(this,_mapUnitSize,value); 
                    }
        
                _mapUnitSize = value;
                }
                 
                     }
                    
                    private Vector3Int  _logicSize;
                    /// <summary>
                    ///逻辑大小
                    ///</summary>
                    public Vector3Int  logicSize{
                                get{return _logicSize;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeLogicsize(this,_logicSize,value); 
                    }
        
                _logicSize = value;
                }
                 
                     }
                    
                    private Vector3Int  _viewSize;
                    /// <summary>
                    ///视口大小
                    ///</summary>
                    public Vector3Int  viewSize{
                                get{return _viewSize;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeViewsize(this,_viewSize,value); 
                    }
        
                _viewSize = value;
                }
                 
                     }
                    
                    private string  _mapJa;
                    /// <summary>
                    ///地图数据
                    ///</summary>
                    public string  mapJa{
                                get{return _mapJa;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeMapja(this,_mapJa,value); 
                    }
        
                _mapJa = value;
                }
                 
                     }
                    
                    private string  _objectJa;
                    /// <summary>
                    ///物体数据
                    ///</summary>
                    public string  objectJa{
                                get{return _objectJa;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeObjectja(this,_objectJa,value); 
                    }
        
                _objectJa = value;
                }
                 
                     }
                    
                    private string  _characterJa;
                    /// <summary>
                    ///单位数据
                    ///</summary>
                    public string  characterJa{
                                get{return _characterJa;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeCharacterja(this,_characterJa,value); 
                    }
        
                _characterJa = value;
                }
                 
                     }
                    
            public Data(int uid,Vector3 mapUnitSize,Vector3Int logicSize,Vector3Int viewSize,string mapJa,string objectJa,string characterJa)
            {

             this.uid = uid;
             this.mapUnitSize = mapUnitSize;
             this.logicSize = logicSize;
             this.viewSize = viewSize;
             this.mapJa = mapJa;
             this.objectJa = objectJa;
             this.characterJa = characterJa;

            }

                public Data Copy()
                {
        return new Data(-1,mapUnitSize,logicSize,viewSize,mapJa,objectJa,characterJa);
                }
            
        }

                   public static Data defaultData=new Data(0,Vector3.zero,Vector3Int.zero,Vector3Int.zero,"","","");


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

                jo.Get<Vector3>("mapUnitSize"),

                jo.Get<Vector3Int>("logicSize"),

                jo.Get<Vector3Int>("viewSize"),

                jo.Get<string>("mapJa"),

                jo.Get<string>("objectJa"),

                jo.Get<string>("characterJa")
                    );

            return data;
        }

        public static JObject GetJoByData(Data data)
        {
            Init();

            JObject jo=new JObject();

            jo.Set<int>("uid",data.uid);

            jo.Set<Vector3>("mapUnitSize",data.mapUnitSize);

            jo.Set<Vector3Int>("logicSize",data.logicSize);

            jo.Set<Vector3Int>("viewSize",data.viewSize);

            jo.Set<string>("mapJa",data.mapJa);

            jo.Set<string>("objectJa",data.objectJa);

            jo.Set<string>("characterJa",data.characterJa);

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
            foreach(var data in DataByUid.Values)
            {
                if(data.uid<uidChain.cnt)
                    RemoveData(data.uid);
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
            
            public static void ChangeMapunitsize(Data superData,Vector3 oldV,Vector3 newV)
            {
                if(superData is Data data)
                {

                changeMapunitsizeAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeLogicsize(Data superData,Vector3Int oldV,Vector3Int newV)
            {
                if(superData is Data data)
                {

                changeLogicsizeAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeViewsize(Data superData,Vector3Int oldV,Vector3Int newV)
            {
                if(superData is Data data)
                {

                changeViewsizeAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeMapja(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeMapjaAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeObjectja(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeObjectjaAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeCharacterja(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeCharacterjaAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
    }
}
        