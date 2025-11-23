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

    public static partial class GameCmdDataForm
    {

        

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Register()
        {

                CmdDataForm.childInitAction+=InitInternal;


                CmdDataForm.childRemoveAction+=RemoveChildren;
                CmdDataForm.childAddAction+=AddChildren;
            

            CmdDataForm.changeUidAction+=ChangeUid;

            CmdDataForm.changeNameAction+=ChangeName;

            CmdDataForm.changePrmnamesAction+=ChangePrmnames;

            CmdDataForm.changePrmtypesAction+=ChangePrmtypes;

            CmdDataForm.changeRetnamesAction+=ChangeRetnames;

            CmdDataForm.changeRettypesAction+=ChangeRettypes;

            CmdDataForm.changeDescAction+=ChangeDesc;

            CmdDataForm.changeDefaultcodeAction+=ChangeDefaultcode;

            Z_Json.extra[typeof(Data)]=((obj)=>{
            if(obj is Data data)
                return GetJoByData(data);
            return null;
            },(jo)=>{
            return GetDataByJo(jo);
            });
        }
        
        private static bool inited;

        public static Z_Chain.Chain uidChain =>CmdDataForm.uidChain;

        public static Action<Data> addAction;
        public static Action<Data> removeAction;
        public static Action childInitAction;
        public static Action<Data> childRemoveAction;
        public static Action<Data> childAddAction;

        public static Action<Data,int,int> changeUidAction;
                
        public static Action<Data,string,string> changeNameAction;
                
        public static Action<Data,List<string>,List<string>> changePrmnamesAction;
                
        public static Action<Data,List<string>,List<string>> changePrmtypesAction;
                
        public static Action<Data,List<string>,List<string>> changeRetnamesAction;
                
        public static Action<Data,List<string>,List<string>> changeRettypesAction;
                
        public static Action<Data,string,string> changeDescAction;
                
        public static Action<Data,string,string> changeDefaultcodeAction;
                
        public static Action<Data,string,string> changeCategoryAction;
                
        public static Action<Data,string,string> changeTypeAction;
                


        public partial class Data : CmdDataForm.Data
        {

                    private string  _category;
                    /// <summary>
                    ///一级标签
                    ///</summary>
                    public string  category{
                                get{return _category;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeCategory(this,_category,value); 
                    }
        
                _category = value;
                }
                 
                     }
                    
                    private string  _type;
                    /// <summary>
                    ///二级标签
                    ///</summary>
                    public string  type{
                                get{return _type;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeType(this,_type,value); 
                    }
        
                _type = value;
                }
                 
                     }
                    
            public Data(CmdDataForm.Data data):base(data.uid,data.name,data.prmNames,data.prmTypes,data.retNames,data.retTypes,data.desc,data.defaultCode)
            {
            }
            
            public Data(int uid,string name,List<string> prmNames,List<string> prmTypes,List<string> retNames,List<string> retTypes,string desc,string defaultCode,string category,string type):base(uid,name,prmNames,prmTypes,retNames,retTypes,desc,defaultCode)
            {

             this.uid = uid;
             this.name = name;
             this.prmNames = prmNames;
             this.prmTypes = prmTypes;
             this.retNames = retNames;
             this.retTypes = retTypes;
             this.desc = desc;
             this.defaultCode = defaultCode;
             this.category = category;
             this.type = type;

            }

                public Data Copy(bool sameId = true)
                {
        return new Data(sameId? uid:uidChain.GetId(),name,new List<string>(prmNames),new List<string>(prmTypes),new List<string>(retNames),new List<string>(retTypes),desc,defaultCode,category,type);
                }
            
        }

                   private static Data _defaultData=new Data(0,"",null,null,null,null,"","","","");
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
    
            static Dictionary<(string,string), List<Data>> _DatasByCategoryType;
            public static Dictionary<(string,string), List<Data>> DatasByCategoryType
            {
                get
                {
                    Init();
                    return _DatasByCategoryType;
                }
            }
    
            static Dictionary<string, List<Data>> _DatasByCategory;
            public static Dictionary<string, List<Data>> DatasByCategory
            {
                get
                {
                    Init();
                    return _DatasByCategory;
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

            CmdDataForm.Init();

        }
        public static void InitInternal()
        {
            if(inited)
                return;
            inited=true;  

        

                _DataByUid = new Dictionary<int, Data>() {

                {100001,new Data(100001,"ShowTip",new List<string>(){"content",},new List<string>(){"string",},null,new List<string>(){"void",},"Show Tip: {0}","ShowTip(\"empty\");","ui","notice")},

                {100002,new Data(100002,"Text",null,null,null,new List<string>(){"string",},"","\"\"","basic","const")},

                {100003,new Data(100003,"Num",null,null,null,new List<string>(){"num",},"","1","basic","const")},

                {100004,new Data(100004,"Image",null,null,null,new List<string>(){"img",},"","\"$i$$i$\"","basic","const")},

                {100005,new Data(100005,"Video",null,null,null,new List<string>(){"video",},"","\"$v$$v$\"","basic","const")},

                {100006,new Data(100006,"Audio",null,null,null,new List<string>(){"audio",},"","\"$a$$a$\"","basic","const")},

                {100007,new Data(100007,"ShowDialog",new List<string>(){"background","avatar","title","content",},new List<string>(){"img","img","string","string",},null,new List<string>(){"void",},"Show Dialog{1}{2}:{3} bg:{0}","ShowDialog(\"$i$$i$\",\"$i$$i$\",\"empty\",\"empty\");","ui","dialog")},

                {100008,new Data(100008,"ShowEffect",new List<string>(){"uid","positionX","positionY","positionZ","angle",},new List<string>(){"num","num","num","num",},null,new List<string>(){"void",},"Show Effect{0} position({1},{2},{3}) angle{4}","ShowEffect(0,0,0,0,0);","scene","effect")},

                {100009,new Data(100009,"ShowCurrentDialog",new List<string>(){"canClickOver",},new List<string>(){"bool",},null,new List<string>(){"void",},"Show Current Dialog, over when click?{0}","ShowCurrentDialog(1);","ui","dialogAdvanced")},

                {100010,new Data(100010,"CloseCurrentDialog",null,null,null,new List<string>(){"void",},"Close Current Dialog","CloseCurrentDialog();","ui","dialogAdvanced")},

                {100011,new Data(100011,"SetDialogBackground",new List<string>(){"background",},new List<string>(){"img",},null,new List<string>(){"void",},"Set Dialog bg:{0}","SetDialogBackground(\"$i$$i$\");","ui","dialogAdvanced")},

                {100012,new Data(100012,"SetDialogContent",new List<string>(){"content",},new List<string>(){"string",},null,new List<string>(){"void",},"Set Dialog content:{0}","SetDialogContent(\"empty\")","ui","dialogAdvanced")},

                {100013,new Data(100013,"SetDialogAvatar",new List<string>(){"avatar",},new List<string>(){"img",},null,new List<string>(){"void",},"Set Dialog avatar:{0}","SetDialogAvatar(\"$i$$i$\");","ui","dialogAdvanced")},

                {100014,new Data(100014,"SetDialogTitle",new List<string>(){"title",},new List<string>(){"string",},null,new List<string>(){"void",},"Set Dialog title:{0}","SetDialogTitle(\"empty\");","ui","dialogAdvanced")},

                {100015,new Data(100015,"SetDialogVideo",new List<string>(){"video",},new List<string>(){"video",},null,new List<string>(){"void",},"Set Dialog video:{0}","SetDialogVideo(\"$v$$v$\");","ui","dialogAdvanced")},

                {100016,new Data(100016,"SetDialogAudio",new List<string>(){"audio",},new List<string>(){"audio",},null,new List<string>(){"void",},"Set Dialog audio:{0}","SetDialogContent(\"$a$$a$\")","ui","dialogAdvanced")},

                {100017,new Data(100017,"ResetDialog",null,null,null,new List<string>(){"void",},"Reset Dialog","ResetDialog();","ui","dialogAdvanced")},

                {100018,new Data(100018,"ShowImage",new List<string>(){"image","width","height","showTime",},new List<string>(){"img","num","num","num",},new List<string>(){"imageId",},new List<string>(){"num",},"Show Image:{0} ,width {1},height {2}, last {3} seconds","ShowImage(\"$i$$i$\",400,400,1);","ui","image")},

                {100019,new Data(100019,"CreateImage",new List<string>(){"image","width","height",},new List<string>(){"img","num","num",},new List<string>(){"imageId",},new List<string>(){"num",},"Create Image {0},width {1},height {2}","CreateImage(\"$i$$i$\",400,400);","ui","imageAdvanced")},

                {100020,new Data(100020,"DeleteImage",new List<string>(){"imageId","afterTime",},new List<string>(){"num","num",},null,new List<string>(){"void",},"Delete Image {0}  after {1} seconds","DeleteImage(1,1);","ui","imageAdvanced")},

                {100021,new Data(100021,"SetImagePos",new List<string>(){"imageId","x","y","transitionTime",},new List<string>(){"num","num","num","num",},null,new List<string>(){"void",},"Set Image{0}: x {1} y {2} ,transition takes {3} seconds","SetImagePos(1,0.5,0.5,1);","ui","imageAdvanced")},

                {100022,new Data(100022,"SetImageOpacity",new List<string>(){"imageId","opacity","transitionTime",},new List<string>(){"num","num","num",},null,new List<string>(){"void",},"Set Image{0}: opacity {1},transition takes {2} seconds","SetImageOpacity(1,1,1);","ui","imageAdvanced")},

                {100023,new Data(100023,"SetImageRotate",new List<string>(){"imageId","angle","transitionTime",},new List<string>(){"num","num","num",},null,new List<string>(){"void",},"Set Image{0}: angle {1},transition takes {2} seconds","SetImageRotate(1,0,1);","ui","imageAdvanced")},

                };
                    _DataByName = new Dictionary<string, Data>() {
    
                        {"ShowTip",_DataByUid[100001]},
    
                        {"Text",_DataByUid[100002]},
    
                        {"Num",_DataByUid[100003]},
    
                        {"Image",_DataByUid[100004]},
    
                        {"Video",_DataByUid[100005]},
    
                        {"Audio",_DataByUid[100006]},
    
                        {"ShowDialog",_DataByUid[100007]},
    
                        {"ShowEffect",_DataByUid[100008]},
    
                        {"ShowCurrentDialog",_DataByUid[100009]},
    
                        {"CloseCurrentDialog",_DataByUid[100010]},
    
                        {"SetDialogBackground",_DataByUid[100011]},
    
                        {"SetDialogContent",_DataByUid[100012]},
    
                        {"SetDialogAvatar",_DataByUid[100013]},
    
                        {"SetDialogTitle",_DataByUid[100014]},
    
                        {"SetDialogVideo",_DataByUid[100015]},
    
                        {"SetDialogAudio",_DataByUid[100016]},
    
                        {"ResetDialog",_DataByUid[100017]},
    
                        {"ShowImage",_DataByUid[100018]},
    
                        {"CreateImage",_DataByUid[100019]},
    
                        {"DeleteImage",_DataByUid[100020]},
    
                        {"SetImagePos",_DataByUid[100021]},
    
                        {"SetImageOpacity",_DataByUid[100022]},
    
                        {"SetImageRotate",_DataByUid[100023]},
    
                    };
    
                    _DatasByCategoryType = new Dictionary<(string,string), List<Data>>() {
    
                            {("ui","notice"),new List<Data>()},
        
                            {("basic","const"),new List<Data>()},
        
                            {("ui","dialog"),new List<Data>()},
        
                            {("scene","effect"),new List<Data>()},
        
                            {("ui","dialogAdvanced"),new List<Data>()},
        
                            {("ui","image"),new List<Data>()},
        
                            {("ui","imageAdvanced"),new List<Data>()},
        
                };

                    _DatasByCategoryType[("ui","notice")].Add(_DataByUid[100001]);

                    _DatasByCategoryType[("basic","const")].Add(_DataByUid[100002]);

                    _DatasByCategoryType[("basic","const")].Add(_DataByUid[100003]);

                    _DatasByCategoryType[("basic","const")].Add(_DataByUid[100004]);

                    _DatasByCategoryType[("basic","const")].Add(_DataByUid[100005]);

                    _DatasByCategoryType[("basic","const")].Add(_DataByUid[100006]);

                    _DatasByCategoryType[("ui","dialog")].Add(_DataByUid[100007]);

                    _DatasByCategoryType[("scene","effect")].Add(_DataByUid[100008]);

                    _DatasByCategoryType[("ui","dialogAdvanced")].Add(_DataByUid[100009]);

                    _DatasByCategoryType[("ui","dialogAdvanced")].Add(_DataByUid[100010]);

                    _DatasByCategoryType[("ui","dialogAdvanced")].Add(_DataByUid[100011]);

                    _DatasByCategoryType[("ui","dialogAdvanced")].Add(_DataByUid[100012]);

                    _DatasByCategoryType[("ui","dialogAdvanced")].Add(_DataByUid[100013]);

                    _DatasByCategoryType[("ui","dialogAdvanced")].Add(_DataByUid[100014]);

                    _DatasByCategoryType[("ui","dialogAdvanced")].Add(_DataByUid[100015]);

                    _DatasByCategoryType[("ui","dialogAdvanced")].Add(_DataByUid[100016]);

                    _DatasByCategoryType[("ui","dialogAdvanced")].Add(_DataByUid[100017]);

                    _DatasByCategoryType[("ui","image")].Add(_DataByUid[100018]);

                    _DatasByCategoryType[("ui","imageAdvanced")].Add(_DataByUid[100019]);

                    _DatasByCategoryType[("ui","imageAdvanced")].Add(_DataByUid[100020]);

                    _DatasByCategoryType[("ui","imageAdvanced")].Add(_DataByUid[100021]);

                    _DatasByCategoryType[("ui","imageAdvanced")].Add(_DataByUid[100022]);

                    _DatasByCategoryType[("ui","imageAdvanced")].Add(_DataByUid[100023]);

                    _DatasByCategory = new Dictionary<string, List<Data>>() {
    
                            {"ui",new List<Data>()},
        
                            {"basic",new List<Data>()},
        
                            {"scene",new List<Data>()},
        
                };

                    _DatasByCategory["ui"].Add(_DataByUid[100001]);

                    _DatasByCategory["basic"].Add(_DataByUid[100002]);

                    _DatasByCategory["basic"].Add(_DataByUid[100003]);

                    _DatasByCategory["basic"].Add(_DataByUid[100004]);

                    _DatasByCategory["basic"].Add(_DataByUid[100005]);

                    _DatasByCategory["basic"].Add(_DataByUid[100006]);

                    _DatasByCategory["ui"].Add(_DataByUid[100007]);

                    _DatasByCategory["scene"].Add(_DataByUid[100008]);

                    _DatasByCategory["ui"].Add(_DataByUid[100009]);

                    _DatasByCategory["ui"].Add(_DataByUid[100010]);

                    _DatasByCategory["ui"].Add(_DataByUid[100011]);

                    _DatasByCategory["ui"].Add(_DataByUid[100012]);

                    _DatasByCategory["ui"].Add(_DataByUid[100013]);

                    _DatasByCategory["ui"].Add(_DataByUid[100014]);

                    _DatasByCategory["ui"].Add(_DataByUid[100015]);

                    _DatasByCategory["ui"].Add(_DataByUid[100016]);

                    _DatasByCategory["ui"].Add(_DataByUid[100017]);

                    _DatasByCategory["ui"].Add(_DataByUid[100018]);

                    _DatasByCategory["ui"].Add(_DataByUid[100019]);

                    _DatasByCategory["ui"].Add(_DataByUid[100020]);

                    _DatasByCategory["ui"].Add(_DataByUid[100021]);

                    _DatasByCategory["ui"].Add(_DataByUid[100022]);

                    _DatasByCategory["ui"].Add(_DataByUid[100023]);


            childInitAction?.Invoke();
            

            foreach(var data in DataByUid.Values)
            {
                CmdDataForm.AddData(data);
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

                jo.Get<List<string>>("prmNames"),

                jo.Get<List<string>>("prmTypes"),

                jo.Get<List<string>>("retNames"),

                jo.Get<List<string>>("retTypes"),

                jo.Get<string>("desc"),

                jo.Get<string>("defaultCode"),

                jo.Get<string>("category"),

                jo.Get<string>("type")
                    );

            return data;
        }

        public static JObject GetJoByData(Data data)
        {
            Init();

            JObject jo=new JObject();

            jo.Set<int>("uid",data.uid);

            jo.Set<string>("name",data.name);

            jo.Set<List<string>>("prmNames",data.prmNames);

            jo.Set<List<string>>("prmTypes",data.prmTypes);

            jo.Set<List<string>>("retNames",data.retNames);

            jo.Set<List<string>>("retTypes",data.retTypes);

            jo.Set<string>("desc",data.desc);

            jo.Set<string>("defaultCode",data.defaultCode);

            jo.Set<string>("category",data.category);

            jo.Set<string>("type",data.type);

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
    
                    if(!DatasByCategoryType.ContainsKey((data.category,data.type)))
                        DatasByCategoryType[(data.category,data.type)]=new List<Data>();
                    DatasByCategoryType[(data.category,data.type)].Add(data);
    
                    if(!DatasByCategory.ContainsKey(data.category))
                        DatasByCategory[data.category]=new List<Data>();
                    DatasByCategory[data.category].Add(data);
    
CmdDataForm.AddData(data);
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
    
                    DataByName.Remove(data.name);
    
                    DatasByCategoryType[(data.category,data.type)].Remove(data);
                    if(DatasByCategoryType[(data.category,data.type)].Count==0)
                        DatasByCategoryType.Remove((data.category,data.type));
    
                    DatasByCategory[data.category].Remove(data);
                    if(DatasByCategory[data.category].Count==0)
                        DatasByCategory.Remove(data.category);
    
CmdDataForm.RemoveData(uid);
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

         private static void RemoveChildren(CmdDataForm.Data data)
        {
            Init();
            if(data is Data)
               RemoveData(data.uid);      
        }
         private static void AddChildren(CmdDataForm.Data superData)
        {
            Init();
            if(superData is Data data)
               AddData(data);      
        }
        




            public static void ChangeUid(CmdDataForm.Data superData,int oldV,int newV)
            {
                if(superData is Data data)
                {

                changeUidAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeName(CmdDataForm.Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                    DataByName.Remove(oldV);
                    DataByName[newV]=data;
 
                changeNameAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangePrmnames(CmdDataForm.Data superData,List<string> oldV,List<string> newV)
            {
                if(superData is Data data)
                {

                changePrmnamesAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangePrmtypes(CmdDataForm.Data superData,List<string> oldV,List<string> newV)
            {
                if(superData is Data data)
                {

                changePrmtypesAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeRetnames(CmdDataForm.Data superData,List<string> oldV,List<string> newV)
            {
                if(superData is Data data)
                {

                changeRetnamesAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeRettypes(CmdDataForm.Data superData,List<string> oldV,List<string> newV)
            {
                if(superData is Data data)
                {

                changeRettypesAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeDesc(CmdDataForm.Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeDescAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeDefaultcode(CmdDataForm.Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeDefaultcodeAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeCategory(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                    DatasByCategory[oldV].Remove(data);
                    if(DatasByCategory[oldV].Count==0)
                        DatasByCategory.Remove(oldV);
                    if(!DatasByCategory.ContainsKey(newV))
                        DatasByCategory[newV]=new List<Data>();
                    DatasByCategory[newV].Add(data);
 
                    DatasByCategoryType[(oldV,data.type)].Remove(data);
                    if(DatasByCategoryType[(oldV,data.type)].Count==0)
                        DatasByCategoryType.Remove((oldV,data.type));
                    if(!DatasByCategoryType.ContainsKey((newV,data.type)))
                        DatasByCategoryType[(newV,data.type)]=new List<Data>();
                    DatasByCategoryType[(newV,data.type)].Add(data);
 
                changeCategoryAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeType(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                    DatasByCategoryType[(data.category,oldV)].Remove(data);
                    if(DatasByCategoryType[(data.category,oldV)].Count==0)
                        DatasByCategoryType.Remove((data.category,oldV));
                    if(!DatasByCategoryType.ContainsKey((data.category,newV)))
                        DatasByCategoryType[(data.category,newV)]=new List<Data>();
                    DatasByCategoryType[(data.category,newV)].Add(data);
 
                changeTypeAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
    }
}
        