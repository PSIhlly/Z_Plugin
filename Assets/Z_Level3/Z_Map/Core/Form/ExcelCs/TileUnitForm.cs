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

    public static partial class TileUnitForm
    {

        

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Register()
        {

                UnitForm.childInitAction+=InitInternal;


                UnitForm.childRemoveAction+=RemoveChildren;
                UnitForm.childAddAction+=AddChildren;
            

            UnitForm.changeUidAction+=ChangeUid;

            UnitForm.changeNameAction+=ChangeName;

            UnitForm.changePrefabnameAction+=ChangePrefabname;

            UnitForm.changePosAction+=ChangePos;

            UnitForm.changeEulerAction+=ChangeEuler;

            UnitForm.changeScaleAction+=ChangeScale;

            UnitForm.changeUpdatetypeAction+=ChangeUpdatetype;

            UnitForm.changeCollidingunituidAction+=ChangeCollidingunituid;

            UnitForm.changeExtraAction+=ChangeExtra;

            Z_Json.extra[typeof(Data)]=((obj)=>{
            if(obj is Data data)
                return GetJoByData(data);
            return null;
            },(jo)=>{
            return GetDataByJo(jo);
            });
        }
        
        private static bool inited;

        public static Z_Chain.Chain uidChain =>UnitForm.uidChain;

        public static Action<Data> addAction;
        public static Action<Data> removeAction;
        public static Action childInitAction;
        public static Action<Data> childRemoveAction;
        public static Action<Data> childAddAction;

        public static Action<Data,int,int> changeUidAction;
                
        public static Action<Data,string,string> changeNameAction;
                
        public static Action<Data,Dictionary<int,string>,Dictionary<int,string>> changeTexnamedicAction;
                
        public static Action<Data,Vector3Int,Vector3Int> changeMapposAction;
                
        public static Action<Data,string,string> changePrefabnameAction;
                
        public static Action<Data,Vector3,Vector3> changePosAction;
                
        public static Action<Data,Vector3,Vector3> changeEulerAction;
                
        public static Action<Data,Vector3,Vector3> changeScaleAction;
                
        public static Action<Data,UpdateType,UpdateType> changeUpdatetypeAction;
                
        public static Action<Data,List<int>,List<int>> changeCollidingunituidAction;
                
        public static Action<Data,string,string> changeExtraAction;
                
        public static Action<Data,bool,bool> changeEnteredsceneAction;
                


        public partial class Data : UnitForm.Data
        {

                /// <summary>
                ///单位逻辑
                ///</summary>
                public TileUnit unit
                {
                    get
                    {
                        return (TileUnit) _unit;
                    }
                }

                    private Dictionary<int,string>  _texNameDic;
                    /// <summary>
                    ///纹理名字（索引）
                    ///</summary>
                    public Dictionary<int,string>  texNameDic{
                                get{return _texNameDic;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeTexnamedic(this,_texNameDic,value); 
                    }
        
                _texNameDic = value;
                }
                 
                     }
                    
                    private Vector3Int  _mapPos;
                    /// <summary>
                    ///离散位置
                    ///</summary>
                    public Vector3Int  mapPos{
                                get{return _mapPos;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeMappos(this,_mapPos,value); 
                    }
        
                _mapPos = value;
                }
                 
                     }
                    
                    private bool  _enteredScene;
                    /// <summary>
                    ///进入过所属scene
                    ///</summary>
                    public bool  enteredScene{
                                get{return _enteredScene;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeEnteredscene(this,_enteredScene,value); 
                    }
        
                _enteredScene = value;
                }
                 
                     }
                    
            public Data(UnitForm.Data data):base(data.uid,data.name,data.prefabName,data.pos,data.euler,data.scale,data.updateType,data.collidingUnitUid,data.extra)
            {
            }
            
            public Data(int uid,string name,Dictionary<int,string> texNameDic,Vector3Int mapPos,string prefabName,Vector3 pos,Vector3 euler,Vector3 scale,UpdateType updateType,List<int> collidingUnitUid,string extra,bool enteredScene):base(uid,name,prefabName,pos,euler,scale,updateType,collidingUnitUid,extra)
            {

             this.uid = uid;
             this.name = name;
             this.texNameDic = texNameDic;
             this.mapPos = mapPos;
             this.prefabName = prefabName;
             this.pos = pos;
             this.euler = euler;
             this.scale = scale;
             this.updateType = updateType;
             this.collidingUnitUid = collidingUnitUid;
             this.extra = extra;
             this.enteredScene = enteredScene;

                    _unit=new TileUnit(this);

            }

                public Data Copy(bool sameId = true)
                {
        return new Data(sameId? uid:uidChain.GetId(),name,new Dictionary<int,string>(texNameDic),mapPos,prefabName,pos,euler,scale,updateType,new List<int>(collidingUnitUid),extra,enteredScene);
                }
            
        }

                   private static Data _defaultData=new Data(0,"",new Dictionary<int,string>(){},Vector3Int.zero,"",Vector3.zero,Vector3.zero,Vector3.zero,UpdateType.ShowOnly,null,"",false);
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
    
            static Dictionary<Vector3Int, Data> _DataByMappos;
            public static Dictionary<Vector3Int, Data> DataByMappos
            {
                get
                {
                    Init();
                    return _DataByMappos;
                }
            }
    

        static public void Init()
        {

            UnitForm.Init();

        }
        public static void InitInternal()
        {
            if(inited)
                return;
            inited=true;  

        

                _DataByUid = new Dictionary<int, Data>() {

                };
                    _DataByMappos = new Dictionary<Vector3Int, Data>() {
    
                    };
    

            childInitAction?.Invoke();
            

            foreach(var data in DataByUid.Values)
            {
                UnitForm.AddData(data);
            }


        
             
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

                jo.Get<string>("name"),

                jo.Get<Dictionary<int,string>>("texNameDic"),

                jo.Get<Vector3Int>("mapPos"),

                jo.Get<string>("prefabName"),

                jo.Get<Vector3>("pos"),

                jo.Get<Vector3>("euler"),

                jo.Get<Vector3>("scale"),

                jo.Get<UpdateType>("updateType"),

                jo.Get<List<int>>("collidingUnitUid"),

                jo.Get<string>("extra"),

                jo.Get<bool>("enteredScene")
                    );

            return data;
        }

        public static JObject GetJoByData(Data data)
        {
            Init();

            JObject jo=new JObject();

            jo.Set<int>("uid",data.uid);

            jo.Set<string>("name",data.name);

            jo.Set<Dictionary<int,string>>("texNameDic",data.texNameDic);

            jo.Set<Vector3Int>("mapPos",data.mapPos);

            jo.Set<string>("prefabName",data.prefabName);

            jo.Set<Vector3>("pos",data.pos);

            jo.Set<Vector3>("euler",data.euler);

            jo.Set<Vector3>("scale",data.scale);

            jo.Set<UpdateType>("updateType",data.updateType);

            jo.Set<List<int>>("collidingUnitUid",data.collidingUnitUid);

            jo.Set<string>("extra",data.extra);

            jo.Set<bool>("enteredScene",data.enteredScene);

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
    
                    DataByMappos[data.mapPos]=data;
    
UnitForm.AddData(data);
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
    
                    DataByMappos.Remove(data.mapPos);
    
UnitForm.RemoveData(uid);
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

         private static void RemoveChildren(UnitForm.Data data)
        {
            Init();
            if(data is Data)
               RemoveData(data.uid);      
        }
         private static void AddChildren(UnitForm.Data superData)
        {
            Init();
            if(superData is Data data)
               AddData(data);      
        }
        




            public static void ChangeUid(UnitForm.Data superData,int oldV,int newV)
            {
                if(superData is Data data)
                {

                changeUidAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeName(UnitForm.Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeNameAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeTexnamedic(Data superData,Dictionary<int,string> oldV,Dictionary<int,string> newV)
            {
                if(superData is Data data)
                {

                changeTexnamedicAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeMappos(Data superData,Vector3Int oldV,Vector3Int newV)
            {
                if(superData is Data data)
                {

                    DataByMappos.Remove(oldV);
                    DataByMappos[newV]=data;
 
                changeMapposAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangePrefabname(UnitForm.Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changePrefabnameAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangePos(UnitForm.Data superData,Vector3 oldV,Vector3 newV)
            {
                if(superData is Data data)
                {

                changePosAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeEuler(UnitForm.Data superData,Vector3 oldV,Vector3 newV)
            {
                if(superData is Data data)
                {

                changeEulerAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeScale(UnitForm.Data superData,Vector3 oldV,Vector3 newV)
            {
                if(superData is Data data)
                {

                changeScaleAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeUpdatetype(UnitForm.Data superData,UpdateType oldV,UpdateType newV)
            {
                if(superData is Data data)
                {

                changeUpdatetypeAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeCollidingunituid(UnitForm.Data superData,List<int> oldV,List<int> newV)
            {
                if(superData is Data data)
                {

                changeCollidingunituidAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeExtra(UnitForm.Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeExtraAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeEnteredscene(Data superData,bool oldV,bool newV)
            {
                if(superData is Data data)
                {

                changeEnteredsceneAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
    }
}
        