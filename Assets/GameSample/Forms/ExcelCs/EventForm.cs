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

    public static partial class EventForm
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
                
        public static Action<Data,List<CmdForm.Data>,List<CmdForm.Data>> changeCmdsAction;
                
        public static Action<Data,string,string> changeLabAction;
                
        public static Action<Data,string,string> changeSublabAction;
                
        public static Action<Data,bool,bool> changeGlobalenableAction;
                
        public static Action<Data,bool,bool> changeTerrainenableAction;
                
        public static Action<Data,bool,bool> changeObjectenableAction;
                


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
                    
                    private List<CmdForm.Data>  _cmds;
                    /// <summary>
                    ///语句
                    ///</summary>
                    public List<CmdForm.Data>  cmds{
                                get{return _cmds;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeCmds(this,_cmds,value); 
                    }
        
                _cmds = value;
                }
                 
                     }
                    
                    private string  _lab;
                    /// <summary>
                    ///一级标签
                    ///</summary>
                    public string  lab{
                                get{return _lab;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeLab(this,_lab,value); 
                    }
        
                _lab = value;
                }
                 
                     }
                    
                    private string  _subLab;
                    /// <summary>
                    ///二级标签
                    ///</summary>
                    public string  subLab{
                                get{return _subLab;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeSublab(this,_subLab,value); 
                    }
        
                _subLab = value;
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
                    
            public Data(int uid,string name,List<CmdForm.Data> cmds,string lab,string subLab,bool globalEnable,bool terrainEnable,bool objectEnable)
            {

             this.uid = uid;
             this.name = name;
             this.cmds = cmds;
             this.lab = lab;
             this.subLab = subLab;
             this.globalEnable = globalEnable;
             this.terrainEnable = terrainEnable;
             this.objectEnable = objectEnable;

            }

                public Data Copy()
                {
        return new Data(-1,name,cmds,lab,subLab,globalEnable,terrainEnable,objectEnable);
                }
            
        }

                   public static Data defaultData=new Data(0,"",null,"","",false,false,false);


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
    
            static Dictionary<string, List<Data>> _DatasByLab;
            public static Dictionary<string, List<Data>> DatasByLab
            {
                get
                {
                    Init();
                    return _DatasByLab;
                }
            }
    
            static Dictionary<string, List<Data>> _DatasBySublab;
            public static Dictionary<string, List<Data>> DatasBySublab
            {
                get
                {
                    Init();
                    return _DatasBySublab;
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

                {1000001,new Data(1000001,"OnTouch",null,"","",false,true,true)},

                {1000002,new Data(1000002,"OnLeave",null,"","",false,true,true)},

                {1000003,new Data(1000003,"OnShow",null,"","",false,true,true)},

                };
                    _DataByName = new Dictionary<string, Data>() {
    
                        {"OnTouch",_DataByUid[1000001]},
    
                        {"OnLeave",_DataByUid[1000002]},
    
                        {"OnShow",_DataByUid[1000003]},
    
                    };
    
                    _DatasByLab = new Dictionary<string, List<Data>>() {
    
                            {"",new List<Data>()},
        
                };

                    _DatasByLab[""].Add(_DataByUid[1000001]);

                    _DatasByLab[""].Add(_DataByUid[1000002]);

                    _DatasByLab[""].Add(_DataByUid[1000003]);

                    _DatasBySublab = new Dictionary<string, List<Data>>() {
    
                            {"",new List<Data>()},
        
                };

                    _DatasBySublab[""].Add(_DataByUid[1000001]);

                    _DatasBySublab[""].Add(_DataByUid[1000002]);

                    _DatasBySublab[""].Add(_DataByUid[1000003]);


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

                jo.Get<List<CmdForm.Data>>("cmds"),

                jo.Get<string>("lab"),

                jo.Get<string>("subLab"),

                jo.Get<bool>("globalEnable"),

                jo.Get<bool>("terrainEnable"),

                jo.Get<bool>("objectEnable")
                    );

            return data;
        }

        public static JObject GetJoByData(Data data)
        {
            Init();

            JObject jo=new JObject();

            jo.Set<int>("uid",data.uid);

            jo.Set<string>("name",data.name);

            jo.Set<List<CmdForm.Data>>("cmds",data.cmds);

            jo.Set<string>("lab",data.lab);

            jo.Set<string>("subLab",data.subLab);

            jo.Set<bool>("globalEnable",data.globalEnable);

            jo.Set<bool>("terrainEnable",data.terrainEnable);

            jo.Set<bool>("objectEnable",data.objectEnable);

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
    
                    if(!DatasByLab.ContainsKey(data.lab))
                        DatasByLab[data.lab]=new List<Data>();
                    DatasByLab[data.lab].Add(data);
    
                    if(!DatasBySublab.ContainsKey(data.subLab))
                        DatasBySublab[data.subLab]=new List<Data>();
                    DatasBySublab[data.subLab].Add(data);
    

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
    
                    DatasByLab[data.lab].Remove(data);
    
                    DatasBySublab[data.subLab].Remove(data);
    

            uidChain.PushId(data.uid);
            childRemoveAction?.Invoke(data);
        }
        public static void Clear()
        {
            Init();

                    DataByUid.Clear();
    
                    DataByName.Clear();
    
                    DatasByLab.Clear();
    
                    DatasBySublab.Clear();
    
            uidChain.Clear();
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
            
            public static void ChangeCmds(Data superData,List<CmdForm.Data> oldV,List<CmdForm.Data> newV)
            {
                if(superData is Data data)
                {

                changeCmdsAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeLab(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                    DatasByLab[oldV].Remove(data);
                    DatasByLab[newV].Add(data);
 
                changeLabAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeSublab(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                    DatasBySublab[oldV].Remove(data);
                    DatasBySublab[newV].Add(data);
 
                changeSublabAction?.Invoke(data,oldV,newV);
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
            
    }
}
        