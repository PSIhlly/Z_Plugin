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

    public static partial class EventTriggerForm
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
                
        public static Action<Data,string,string> changeNameAction;
                
        public static Action<Data,string,string> changeEvtAction;
                
        public static Action<Data,bool,bool> changeGlobalenableAction;
                
        public static Action<Data,bool,bool> changeTerrainenableAction;
                
        public static Action<Data,bool,bool> changeObjectenableAction;
                
        public static Action<Data,bool,bool> changeCharacterenableAction;
                


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
                    
                    private string  _name;
                    /// <summary>
                    ///名称
                    ///</summary>
                    public string  name{
                                get{return _name;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeName(this,_name,value); 
                    }
        
                _name = value;
                }
                 
                     }
                    
                    private string  _evt;
                    /// <summary>
                    ///事件名称
                    ///</summary>
                    public string  evt{
                                get{return _evt;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeEvt(this,_evt,value); 
                    }
        
                _evt = value;
                }
                 
                     }
                    
                    private bool  _globalEnable;
                    /// <summary>
                    ///允许全局
                    ///</summary>
                    public bool  globalEnable{
                                get{return _globalEnable;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeGlobalenable(this,_globalEnable,value); 
                    }
        
                _globalEnable = value;
                }
                 
                     }
                    
                    private bool  _terrainEnable;
                    /// <summary>
                    ///允许地形用
                    ///</summary>
                    public bool  terrainEnable{
                                get{return _terrainEnable;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeTerrainenable(this,_terrainEnable,value); 
                    }
        
                _terrainEnable = value;
                }
                 
                     }
                    
                    private bool  _objectEnable;
                    /// <summary>
                    ///允许物体用
                    ///</summary>
                    public bool  objectEnable{
                                get{return _objectEnable;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeObjectenable(this,_objectEnable,value); 
                    }
        
                _objectEnable = value;
                }
                 
                     }
                    
                    private bool  _characterEnable;
                    /// <summary>
                    ///允许角色用
                    ///</summary>
                    public bool  characterEnable{
                                get{return _characterEnable;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeCharacterenable(this,_characterEnable,value); 
                    }
        
                _characterEnable = value;
                }
                 
                     }
                    
            public Data(int uid,string name,string evt,bool globalEnable,bool terrainEnable,bool objectEnable,bool characterEnable)
            {

             this.uid = uid;
             this.name = name;
             this.evt = evt;
             this.globalEnable = globalEnable;
             this.terrainEnable = terrainEnable;
             this.objectEnable = objectEnable;
             this.characterEnable = characterEnable;

            }

                public Data Copy(bool sameId = true)
                {
        return new Data(sameId? uid:uidChain.GetId(),name,evt,globalEnable,terrainEnable,objectEnable,characterEnable);
                }
            
        }

                   public static Data defaultData=new Data(0,"","",false,false,false,false);


            static Dictionary<int, Data> _DataByUid;
            public static Dictionary<int, Data> DataByUid
            {
                get
                {
                    Init();
                    return _DataByUid;
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
uidChain=new Z_Chain.Chain (autoUidCnt);

                _DataByUid = new Dictionary<int, Data>() {

                {1,new Data(1,"OnTouch","",false,true,true,true)},

                {2,new Data(2,"OnLeave","",false,true,true,true)},

                {3,new Data(3,"OnShow","",false,true,true,true)},

                };
                    _DataByName = new Dictionary<string, Data>() {
    
                        {"OnTouch",_DataByUid[1]},
    
                        {"OnLeave",_DataByUid[2]},
    
                        {"OnShow",_DataByUid[3]},
    
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

                jo.Get<string>("name"),

                jo.Get<string>("evt"),

                jo.Get<bool>("globalEnable"),

                jo.Get<bool>("terrainEnable"),

                jo.Get<bool>("objectEnable"),

                jo.Get<bool>("characterEnable")
                    );

            return data;
        }

        public static JObject GetJoByData(Data data)
        {
            Init();

            JObject jo=new JObject();

            jo.Set<int>("uid",data.uid);

            jo.Set<string>("name",data.name);

            jo.Set<string>("evt",data.evt);

            jo.Set<bool>("globalEnable",data.globalEnable);

            jo.Set<bool>("terrainEnable",data.terrainEnable);

            jo.Set<bool>("objectEnable",data.objectEnable);

            jo.Set<bool>("characterEnable",data.characterEnable);

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
    
                    DataByName[data.name]=data;
    

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
    
                    DataByName.Remove(data.name);
    

            uidChain.PushId(data.uid);
            childRemoveAction?.Invoke(data);
        }
        public static void Clear()
        {
            Init();

                    DataByUid.Clear();
    
                    DataByName.Clear();
    
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
            
            public static void ChangeName(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                    DataByName.Remove(oldV);
                    DataByName[newV]=data;
 
                changeNameAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeEvt(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeEvtAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeGlobalenable(Data superData,bool oldV,bool newV)
            {
                if(superData is Data data)
                {

                changeGlobalenableAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeTerrainenable(Data superData,bool oldV,bool newV)
            {
                if(superData is Data data)
                {

                changeTerrainenableAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeObjectenable(Data superData,bool oldV,bool newV)
            {
                if(superData is Data data)
                {

                changeObjectenableAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeCharacterenable(Data superData,bool oldV,bool newV)
            {
                if(superData is Data data)
                {

                changeCharacterenableAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
    }
}
        