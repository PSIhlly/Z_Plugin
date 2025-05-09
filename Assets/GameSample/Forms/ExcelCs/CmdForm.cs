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

    public static partial class CmdForm
    {
public static readonly int autoUidCnt=100;

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
                
        public static Action<Data,List<int>,List<int>> changePrmtypesAction;
                
        public static Action<Data,List<string>,List<string>> changePrmnameAction;
                
        public static Action<Data,List<int>,List<int>> changeRestypesAction;
                
        public static Action<Data,string,string> changeConstvAction;
                
        public static Action<Data,string,string> changeLabAction;
                
        public static Action<Data,List<string>,List<string>> changeAdditioncmdsAction;
                
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
                    
                    private List<int>  _prmTypes;
                    /// <summary>
                    ///参数类型
                    ///</summary>
                    public List<int>  prmTypes{
                                get{return _prmTypes;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangePrmtypes(this,_prmTypes,value); 
                    }
        
                _prmTypes = value;
                }
                 
                     }
                    
                    private List<string>  _prmName;
                    /// <summary>
                    ///参数名称
                    ///</summary>
                    public List<string>  prmName{
                                get{return _prmName;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangePrmname(this,_prmName,value); 
                    }
        
                _prmName = value;
                }
                 
                     }
                    
                    private List<int>  _resTypes;
                    /// <summary>
                    ///结果类型
                    ///</summary>
                    public List<int>  resTypes{
                                get{return _resTypes;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeRestypes(this,_resTypes,value); 
                    }
        
                _resTypes = value;
                }
                 
                     }
                    
                    private string  _constV;
                    /// <summary>
                    ///常量
                    ///</summary>
                    public string  constV{
                                get{return _constV;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeConstv(this,_constV,value); 
                    }
        
                _constV = value;
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
                    
                    private List<string>  _additionCmds;
                    /// <summary>
                    ///附加语句
                    ///</summary>
                    public List<string>  additionCmds{
                                get{return _additionCmds;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeAdditioncmds(this,_additionCmds,value); 
                    }
        
                _additionCmds = value;
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
                    
            public Data(int uid,string name,List<int> prmTypes,List<string> prmName,List<int> resTypes,string constV,string lab,List<string> additionCmds,bool globalEnable,bool terrainEnable,bool objectEnable,bool characterEnable)
            {

             this.uid = uid;
             this.name = name;
             this.prmTypes = prmTypes;
             this.prmName = prmName;
             this.resTypes = resTypes;
             this.constV = constV;
             this.lab = lab;
             this.additionCmds = additionCmds;
             this.globalEnable = globalEnable;
             this.terrainEnable = terrainEnable;
             this.objectEnable = objectEnable;
             this.characterEnable = characterEnable;

            }

                public Data Copy(bool sameId = true)
                {
        return new Data(sameId? uid:uidChain.GetId(),name,prmTypes,prmName,resTypes,constV,lab,additionCmds,globalEnable,terrainEnable,objectEnable,characterEnable);
                }
            
        }

                   public static Data defaultData=new Data(0,"empty",null,null,new List<int>(){4,},"","",null,false,false,false,false);


            static Dictionary<int, Data> _DataByUid;
            public static Dictionary<int, Data> DataByUid
            {
                get
                {
                    Init();
                    return _DataByUid;
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

                {1,new Data(1,"dialog",new List<int>(){2,},new List<string>(){"content",},null,"","popup",null,true,true,true,true)},

                {2,new Data(2,"tips",new List<int>(){2,},new List<string>(){"content",},null,"","tips",null,true,true,true,true)},

                {3,new Data(3,"if",new List<int>(){1,},new List<string>(){"conditionJudge",},null,"","logic",new List<string>(){"then","else",},true,true,true,true)},

                {4,new Data(4,"then",new List<int>(){5,},new List<string>(){"execute",},null,"","",null,true,true,true,true)},

                {5,new Data(5,"else",new List<int>(){5,},new List<string>(){"execute",},null,"","",null,true,true,true,true)},

                {6,new Data(6,"num",null,null,new List<int>(){0,},"","value",null,true,true,true,true)},

                {7,new Data(7,"text",null,null,new List<int>(){2,},"","value",null,true,true,true,true)},

                };
                    _DataByName = new Dictionary<string, Data>() {
    
                        {"dialog",_DataByUid[1]},
    
                        {"tips",_DataByUid[2]},
    
                        {"if",_DataByUid[3]},
    
                        {"then",_DataByUid[4]},
    
                        {"else",_DataByUid[5]},
    
                        {"num",_DataByUid[6]},
    
                        {"text",_DataByUid[7]},
    
                    };
    
                    _DatasByLab = new Dictionary<string, List<Data>>() {
    
                            {"popup",new List<Data>()},
        
                            {"tips",new List<Data>()},
        
                            {"logic",new List<Data>()},
        
                            {"",new List<Data>()},
        
                            {"value",new List<Data>()},
        
                };

                    _DatasByLab["popup"].Add(_DataByUid[1]);

                    _DatasByLab["tips"].Add(_DataByUid[2]);

                    _DatasByLab["logic"].Add(_DataByUid[3]);

                    _DatasByLab[""].Add(_DataByUid[4]);

                    _DatasByLab[""].Add(_DataByUid[5]);

                    _DatasByLab["value"].Add(_DataByUid[6]);

                    _DatasByLab["value"].Add(_DataByUid[7]);


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

                jo.Get<List<int>>("prmTypes"),

                jo.Get<List<string>>("prmName"),

                jo.Get<List<int>>("resTypes"),

                jo.Get<string>("constV"),

                jo.Get<string>("lab"),

                jo.Get<List<string>>("additionCmds"),

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

            jo.Set<List<int>>("prmTypes",data.prmTypes);

            jo.Set<List<string>>("prmName",data.prmName);

            jo.Set<List<int>>("resTypes",data.resTypes);

            jo.Set<string>("constV",data.constV);

            jo.Set<string>("lab",data.lab);

            jo.Set<List<string>>("additionCmds",data.additionCmds);

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
    
                    if(!DatasByLab.ContainsKey(data.lab))
                        DatasByLab[data.lab]=new List<Data>();
                    DatasByLab[data.lab].Add(data);
    

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
                    if(DatasByLab[data.lab].Count==0)
                        DatasByLab.Remove(data.lab);
    

            uidChain.PushId(data.uid);
            childRemoveAction?.Invoke(data);
        }
        public static void Clear()
        {
            Init();

                    DataByUid.Clear();
    
                    DataByName.Clear();
    
                    DatasByLab.Clear();
    
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
            
            public static void ChangePrmtypes(Data superData,List<int> oldV,List<int> newV)
            {
                if(superData is Data data)
                {

                changePrmtypesAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangePrmname(Data superData,List<string> oldV,List<string> newV)
            {
                if(superData is Data data)
                {

                changePrmnameAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeRestypes(Data superData,List<int> oldV,List<int> newV)
            {
                if(superData is Data data)
                {

                changeRestypesAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeConstv(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeConstvAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeLab(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                    DatasByLab[oldV].Remove(data);
                    if(DatasByLab[oldV].Count==0)
                        DatasByLab.Remove(oldV);
                    if(!DatasByLab.ContainsKey(newV))
                        DatasByLab[newV]=new List<Data>();
                    DatasByLab[newV].Add(data);
 
                changeLabAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeAdditioncmds(Data superData,List<string> oldV,List<string> newV)
            {
                if(superData is Data data)
                {

                changeAdditioncmdsAction?.Invoke(data,oldV,newV);
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
        