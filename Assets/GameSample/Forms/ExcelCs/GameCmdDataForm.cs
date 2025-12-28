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
                
        public static Action<Data,bool,bool> changeCancreateAction;
                
        public static Action<Data,EditorStyle,EditorStyle> changeLowesteditorstyleAction;
                


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
                    
                    private bool  _canCreate;
                    /// <summary>
                    ///可被entry创建
                    ///</summary>
                    public bool  canCreate{
                                get{return _canCreate;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeCancreate(this,_canCreate,value); 
                    }
        
                _canCreate = value;
                }
                 
                     }
                    
                    private EditorStyle  _lowestEditorStyle;
                    /// <summary>
                    ///最低许可编辑模式
                    ///</summary>
                    public EditorStyle  lowestEditorStyle{
                                get{return _lowestEditorStyle;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeLowesteditorstyle(this,_lowestEditorStyle,value); 
                    }
        
                _lowestEditorStyle = value;
                }
                 
                     }
                    
            public Data(CmdDataForm.Data data):base(data.uid,data.name,data.prmNames,data.prmTypes,data.retNames,data.retTypes,data.desc,data.defaultCode)
            {
            }
            
            public Data(int uid,string name,List<string> prmNames,List<string> prmTypes,List<string> retNames,List<string> retTypes,string desc,string defaultCode,string category,string type,bool canCreate,EditorStyle lowestEditorStyle):base(uid,name,prmNames,prmTypes,retNames,retTypes,desc,defaultCode)
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
             this.canCreate = canCreate;
             this.lowestEditorStyle = lowestEditorStyle;

            }

                public Data Copy(bool sameId = true)
                {
        return new Data(sameId? uid:uidChain.GetId(),name,new List<string>(prmNames),new List<string>(prmTypes),new List<string>(retNames),new List<string>(retTypes),desc,defaultCode,category,type,canCreate,lowestEditorStyle);
                }
            
        }

                   private static Data _defaultData=new Data(0,"",null,null,null,null,"","","","",false,default);
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
    
            static Dictionary<bool, List<Data>> _DatasByCancreate;
            public static Dictionary<bool, List<Data>> DatasByCancreate
            {
                get
                {
                    Init();
                    return _DatasByCancreate;
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

                {100001,new Data(100001,"ShowTip",new List<string>(){"content",},new List<string>(){"string",},null,new List<string>(){"void",},"Show Tip: {0}","ShowTip(\"empty\");","ui","notice",true,EditorStyle.AvgAdvanced)},

                {100002,new Data(100002,"Text",null,null,null,new List<string>(){"string",},"","\"\"","basic","const",false,EditorStyle.Avg)},

                {100003,new Data(100003,"Num",null,null,null,new List<string>(){"num",},"","1","basic","const",false,EditorStyle.AvgAdvanced)},

                {100004,new Data(100004,"Image",null,null,null,new List<string>(){"img",},"","\"$i$$i$\"","basic","const",false,EditorStyle.Avg)},

                {100005,new Data(100005,"Video",null,null,null,new List<string>(){"video",},"","\"$v$$v$\"","basic","const",false,EditorStyle.AvgAdvanced)},

                {100006,new Data(100006,"Audio",null,null,null,new List<string>(){"audio",},"","\"$a$$a$\"","basic","const",false,EditorStyle.AvgAdvanced)},

                {100007,new Data(100007,"LocalVar",null,null,null,new List<string>(){"var",},"","a","basic","variable",false,EditorStyle.AvgAdvanced)},

                {100008,new Data(100008,"SetLocalVar",null,null,null,new List<string>(){"void",},"","a = 0;","basic","variable",true,EditorStyle.AvgAdvanced)},

                {100009,new Data(100009,"ShowDialog",new List<string>(){"background","avatar","title","content",},new List<string>(){"img","img","string","string",},null,new List<string>(){"void",},"Show Dialog{1}{2}:{3} bg:{0}","ShowDialog(\"$i$$i$\",\"$i$$i$\",\"empty\",\"empty\");","ui","dialog",true,EditorStyle.AvgAdvanced)},

                {100010,new Data(100010,"ShowEffect",new List<string>(){"uid","x","y","height","angle",},new List<string>(){"num","num","num","num",},null,new List<string>(){"void",},"Show Effect{0} x {1} y {2} height:{3}, angle{4}","ShowEffect(0,0,0,0,0);","scene","effect",true,EditorStyle.RpgAdvanced)},

                {100011,new Data(100011,"CloseCurrentDialog",null,null,null,new List<string>(){"void",},"Close Current Dialog","CloseCurrentDialog();","ui","dialogAdvanced",true,EditorStyle.AvgAdvanced)},

                {100012,new Data(100012,"ShowAdvancedDialog",new List<string>(){"background","avatar","title","content","audio","video","canClickPass",},new List<string>(){"img","img","string","string","audio","video","bool",},new List<string>(){"dialogID",},new List<string>(){"num",},"Show Dialog{1}{2}:{3} bg:{0},can click pass?{6},audio:{4} video:{5}","ShowDialog(\"$i$$i$\",\"$i$$i$\",\"empty\",\"empty\",\"$a$$a$\",\"$v$$v$\",true);","ui","dialogAdvanced",true,EditorStyle.AvgAdvanced)},

                {100014,new Data(100014,"ShowImage",new List<string>(){"image","width","height","showTime",},new List<string>(){"img","num","num","num",},new List<string>(){"imageID",},new List<string>(){"num",},"Show Image:{0} ,width {1},height {2}, last {3} seconds","imageID = ShowImage(\"$i$$i$\",400,400,1);","ui","image",true,EditorStyle.Avg)},

                {100015,new Data(100015,"ShowImagePermanently",new List<string>(){"image","width","height",},new List<string>(){"img","num","num",},new List<string>(){"imageID",},new List<string>(){"num",},"Show Image Permanently {0},width {1},height {2}","imageID = ShowImagePermanently(\"$i$$i$\",400,400);","ui","imageAdvanced",true,EditorStyle.Avg)},

                {100016,new Data(100016,"DeleteImage",new List<string>(){"imageID","afterTime",},new List<string>(){"num","num",},null,new List<string>(){"void",},"Delete Image {0}  after {1} seconds","DeleteImage(imageID,1);","ui","imageAdvanced",true,EditorStyle.Avg)},

                {100017,new Data(100017,"SetImagePos",new List<string>(){"imageID","x","y","transitionTime",},new List<string>(){"num","num","num","num",},null,new List<string>(){"void",},"Set Image{0}: x {1} y {2} ,transition takes {3} seconds","SetImagePos(imageID,0.5,0.5,1);","ui","imageAdvanced",true,EditorStyle.AvgAdvanced)},

                {100018,new Data(100018,"SetImageOpacity",new List<string>(){"imageID","opacity","transitionTime",},new List<string>(){"num","num","num",},null,new List<string>(){"void",},"Set Image{0}: opacity {1},transition takes {2} seconds","SetImageOpacity(imageID,1,1);","ui","imageAdvanced",true,EditorStyle.AvgAdvanced)},

                {100019,new Data(100019,"SetImageRotate",new List<string>(){"imageID","angle","transitionTime",},new List<string>(){"num","num","num",},null,new List<string>(){"void",},"Set Image{0}: angle {1},transition takes {2} seconds","SetImageRotate(imageID,0,1);","ui","imageAdvanced",true,EditorStyle.AvgAdvanced)},

                {100020,new Data(100020,"Add",new List<string>(){"num","num",},new List<string>(){"num","num",},new List<string>(){"resault",},new List<string>(){"num",},"","1 + 1","basic","math",false,EditorStyle.AvgAdvanced)},

                {100021,new Data(100021,"Subtract",new List<string>(){"num","num",},new List<string>(){"num","num",},new List<string>(){"resault",},new List<string>(){"num",},"","1 - 1","basic","math",false,EditorStyle.AvgAdvanced)},

                {100022,new Data(100022,"Multiply",new List<string>(){"num","num",},new List<string>(){"num","num",},new List<string>(){"resault",},new List<string>(){"num",},"","1 * 1","basic","math",false,EditorStyle.AvgAdvanced)},

                {100023,new Data(100023,"Divide",new List<string>(){"num","num",},new List<string>(){"num","num",},new List<string>(){"resault",},new List<string>(){"num",},"","1 / 1","basic","math",false,EditorStyle.AvgAdvanced)},

                {100024,new Data(100024,"Wait",new List<string>(){"seconds",},new List<string>(){"num",},null,new List<string>(){"void",},"Wait {0} seconds","Wait(1);","basic","process",true,EditorStyle.AvgAdvanced)},

                {100025,new Data(100025,"If",null,null,null,new List<string>(){"void",},"","if(1){ }else{ }","basic","process",true,EditorStyle.AvgAdvanced)},

                {100026,new Data(100026,"For",null,null,null,new List<string>(){"void",},"","for(i=1;i<5;i=i+1){ }","basic","process",true,EditorStyle.AvgAdvanced)},

                {100027,new Data(100027,"Greater",new List<string>(){"num","num",},new List<string>(){"num","num",},new List<string>(){"resault",},new List<string>(){"num",},"","0 > 1","basic","math",false,EditorStyle.AvgAdvanced)},

                {100028,new Data(100028,"Less",new List<string>(){"num","num",},new List<string>(){"num","num",},new List<string>(){"resault",},new List<string>(){"num",},"","0 < 1","basic","math",false,EditorStyle.AvgAdvanced)},

                {100029,new Data(100029,"Equal",new List<string>(){"num","num",},new List<string>(){"num","num",},new List<string>(){"resault",},new List<string>(){"num",},"","0 == 1","basic","math",false,EditorStyle.AvgAdvanced)},

                {100030,new Data(100030,"NotEqual",new List<string>(){"num","num",},new List<string>(){"num","num",},new List<string>(){"resault",},new List<string>(){"num",},"","0 != 1","basic","math",false,EditorStyle.AvgAdvanced)},

                {100031,new Data(100031,"Pause",null,null,null,new List<string>(){"void",},"Pause","Pause();","basic","process",true,EditorStyle.Rpg)},

                {100032,new Data(100032,"Continue",null,null,null,new List<string>(){"void",},"Continue","Continue();","basic","process",true,EditorStyle.Rpg)},

                {100033,new Data(100033,"GameOver",null,null,null,new List<string>(){"void",},"Game over","GameOver();","basic","process",true,EditorStyle.Avg)},

                {100034,new Data(100034,"Save",null,null,null,new List<string>(){"void",},"Save","Save();","basic","process",true,EditorStyle.Avg)},

                {100035,new Data(100035,"Load",null,null,null,new List<string>(){"void",},"Load","Load();","basic","process",true,EditorStyle.Avg)},

                {100036,new Data(100036,"DestroyObject",new List<string>(){"objectID",},new List<string>(){"num",},null,new List<string>(){"void",},"object ID {0}: Destroy","DestroyObject(objectSelfID);","scene","object",true,EditorStyle.Rpg)},

                {100037,new Data(100037,"GenerateObject",new List<string>(){"name",},new List<string>(){"string",},new List<string>(){"objectID",},new List<string>(){"num",},"object name {0}: Generate:","newObjectID = GenerateObject(\"empty\");","scene","object",true,EditorStyle.Rpg)},

                {100038,new Data(100038,"MoveObject",new List<string>(){"objectID","x","y","height","transitionTime",},new List<string>(){"num","num","num","num.num",},null,new List<string>(){"void",},"object ID {0} : Move x {1} y{2} height{3}, last {4} seconds","MoveObject(0,1,1,0,1);","scene","object",true,EditorStyle.Rpg)},

                {100039,new Data(100039,"GetSelfObjectID",null,null,new List<string>(){"objectID",},new List<string>(){"num",},"Get object self ID","objectSelfID = GetSelfObjectID();","scene","object",true,EditorStyle.Rpg)},

                {100040,new Data(100040,"GetTriggerObjectID",null,null,new List<string>(){"objectID",},new List<string>(){"num",},"Get trigger object ID","objectTriggerID = GetTriggerObjectID();","scene","object",true,EditorStyle.Rpg)},

                {100041,new Data(100041,"GainItem",new List<string>(){"name","amount",},new List<string>(){"string","num",},null,new List<string>(){"void",},"Gain {0} x {1}","GainItem(\"empty\",1);","item","backpack",true,EditorStyle.Rpg)},

                {100042,new Data(100042,"LostItem",new List<string>(){"name","amount",},new List<string>(){"string","num",},null,new List<string>(){"void",},"Lost {0} x {1}","LostItem(\"empty\",1);","item","backpack",true,EditorStyle.Rpg)},

                {100043,new Data(100043,"GetSelfCharacterID",null,null,new List<string>(){"characterID",},new List<string>(){"num",},"Get character self ID","characterSelfID = GetSelfCharacterID();","scene","character",true,EditorStyle.Rpg)},

                {100044,new Data(100044,"GetTriggerCharacterID",null,null,new List<string>(){"characterID",},new List<string>(){"num",},"Get trigger character ID","characterTriggerID = GetTriggerCharacterID();","scene","character",true,EditorStyle.Rpg)},

                {100045,new Data(100045,"SetCharacterParameter",new List<string>(){"characterID","paramName","value",},new List<string>(){"num","string","num",},null,new List<string>(){"void",},"Set Character ID {0} 's {1} = {2}","SetCharacterParameter(characterSelfID,\"empty\",1);","character","parameter",true,EditorStyle.Rpg)},

                {100046,new Data(100046,"GetCurrentCharacterID",null,null,new List<string>(){"characterID",},new List<string>(){"num",},"Get currently used character ID","currentCharacterID = GetCurrentCharacterID();","character","system",true,EditorStyle.Rpg)},

                };
                    _DataByName = new Dictionary<string, Data>() {
    
                        {"ShowTip",_DataByUid[100001]},
    
                        {"Text",_DataByUid[100002]},
    
                        {"Num",_DataByUid[100003]},
    
                        {"Image",_DataByUid[100004]},
    
                        {"Video",_DataByUid[100005]},
    
                        {"Audio",_DataByUid[100006]},
    
                        {"LocalVar",_DataByUid[100007]},
    
                        {"SetLocalVar",_DataByUid[100008]},
    
                        {"ShowDialog",_DataByUid[100009]},
    
                        {"ShowEffect",_DataByUid[100010]},
    
                        {"CloseCurrentDialog",_DataByUid[100011]},
    
                        {"ShowAdvancedDialog",_DataByUid[100012]},
    
                        {"ShowImage",_DataByUid[100014]},
    
                        {"ShowImagePermanently",_DataByUid[100015]},
    
                        {"DeleteImage",_DataByUid[100016]},
    
                        {"SetImagePos",_DataByUid[100017]},
    
                        {"SetImageOpacity",_DataByUid[100018]},
    
                        {"SetImageRotate",_DataByUid[100019]},
    
                        {"Add",_DataByUid[100020]},
    
                        {"Subtract",_DataByUid[100021]},
    
                        {"Multiply",_DataByUid[100022]},
    
                        {"Divide",_DataByUid[100023]},
    
                        {"Wait",_DataByUid[100024]},
    
                        {"If",_DataByUid[100025]},
    
                        {"For",_DataByUid[100026]},
    
                        {"Greater",_DataByUid[100027]},
    
                        {"Less",_DataByUid[100028]},
    
                        {"Equal",_DataByUid[100029]},
    
                        {"NotEqual",_DataByUid[100030]},
    
                        {"Pause",_DataByUid[100031]},
    
                        {"Continue",_DataByUid[100032]},
    
                        {"GameOver",_DataByUid[100033]},
    
                        {"Save",_DataByUid[100034]},
    
                        {"Load",_DataByUid[100035]},
    
                        {"DestroyObject",_DataByUid[100036]},
    
                        {"GenerateObject",_DataByUid[100037]},
    
                        {"MoveObject",_DataByUid[100038]},
    
                        {"GetSelfObjectID",_DataByUid[100039]},
    
                        {"GetTriggerObjectID",_DataByUid[100040]},
    
                        {"GainItem",_DataByUid[100041]},
    
                        {"LostItem",_DataByUid[100042]},
    
                        {"GetSelfCharacterID",_DataByUid[100043]},
    
                        {"GetTriggerCharacterID",_DataByUid[100044]},
    
                        {"SetCharacterParameter",_DataByUid[100045]},
    
                        {"GetCurrentCharacterID",_DataByUid[100046]},
    
                    };
    
                    _DatasByCategoryType = new Dictionary<(string,string), List<Data>>() {
    
                            {("ui","notice"),new List<Data>()},
        
                            {("basic","const"),new List<Data>()},
        
                            {("basic","variable"),new List<Data>()},
        
                            {("ui","dialog"),new List<Data>()},
        
                            {("scene","effect"),new List<Data>()},
        
                            {("ui","dialogAdvanced"),new List<Data>()},
        
                            {("ui","image"),new List<Data>()},
        
                            {("ui","imageAdvanced"),new List<Data>()},
        
                            {("basic","math"),new List<Data>()},
        
                            {("basic","process"),new List<Data>()},
        
                            {("scene","object"),new List<Data>()},
        
                            {("item","backpack"),new List<Data>()},
        
                            {("scene","character"),new List<Data>()},
        
                            {("character","parameter"),new List<Data>()},
        
                            {("character","system"),new List<Data>()},
        
                };

                    _DatasByCategoryType[("ui","notice")].Add(_DataByUid[100001]);

                    _DatasByCategoryType[("basic","const")].Add(_DataByUid[100002]);

                    _DatasByCategoryType[("basic","const")].Add(_DataByUid[100003]);

                    _DatasByCategoryType[("basic","const")].Add(_DataByUid[100004]);

                    _DatasByCategoryType[("basic","const")].Add(_DataByUid[100005]);

                    _DatasByCategoryType[("basic","const")].Add(_DataByUid[100006]);

                    _DatasByCategoryType[("basic","variable")].Add(_DataByUid[100007]);

                    _DatasByCategoryType[("basic","variable")].Add(_DataByUid[100008]);

                    _DatasByCategoryType[("ui","dialog")].Add(_DataByUid[100009]);

                    _DatasByCategoryType[("scene","effect")].Add(_DataByUid[100010]);

                    _DatasByCategoryType[("ui","dialogAdvanced")].Add(_DataByUid[100011]);

                    _DatasByCategoryType[("ui","dialogAdvanced")].Add(_DataByUid[100012]);

                    _DatasByCategoryType[("ui","image")].Add(_DataByUid[100014]);

                    _DatasByCategoryType[("ui","imageAdvanced")].Add(_DataByUid[100015]);

                    _DatasByCategoryType[("ui","imageAdvanced")].Add(_DataByUid[100016]);

                    _DatasByCategoryType[("ui","imageAdvanced")].Add(_DataByUid[100017]);

                    _DatasByCategoryType[("ui","imageAdvanced")].Add(_DataByUid[100018]);

                    _DatasByCategoryType[("ui","imageAdvanced")].Add(_DataByUid[100019]);

                    _DatasByCategoryType[("basic","math")].Add(_DataByUid[100020]);

                    _DatasByCategoryType[("basic","math")].Add(_DataByUid[100021]);

                    _DatasByCategoryType[("basic","math")].Add(_DataByUid[100022]);

                    _DatasByCategoryType[("basic","math")].Add(_DataByUid[100023]);

                    _DatasByCategoryType[("basic","process")].Add(_DataByUid[100024]);

                    _DatasByCategoryType[("basic","process")].Add(_DataByUid[100025]);

                    _DatasByCategoryType[("basic","process")].Add(_DataByUid[100026]);

                    _DatasByCategoryType[("basic","math")].Add(_DataByUid[100027]);

                    _DatasByCategoryType[("basic","math")].Add(_DataByUid[100028]);

                    _DatasByCategoryType[("basic","math")].Add(_DataByUid[100029]);

                    _DatasByCategoryType[("basic","math")].Add(_DataByUid[100030]);

                    _DatasByCategoryType[("basic","process")].Add(_DataByUid[100031]);

                    _DatasByCategoryType[("basic","process")].Add(_DataByUid[100032]);

                    _DatasByCategoryType[("basic","process")].Add(_DataByUid[100033]);

                    _DatasByCategoryType[("basic","process")].Add(_DataByUid[100034]);

                    _DatasByCategoryType[("basic","process")].Add(_DataByUid[100035]);

                    _DatasByCategoryType[("scene","object")].Add(_DataByUid[100036]);

                    _DatasByCategoryType[("scene","object")].Add(_DataByUid[100037]);

                    _DatasByCategoryType[("scene","object")].Add(_DataByUid[100038]);

                    _DatasByCategoryType[("scene","object")].Add(_DataByUid[100039]);

                    _DatasByCategoryType[("scene","object")].Add(_DataByUid[100040]);

                    _DatasByCategoryType[("item","backpack")].Add(_DataByUid[100041]);

                    _DatasByCategoryType[("item","backpack")].Add(_DataByUid[100042]);

                    _DatasByCategoryType[("scene","character")].Add(_DataByUid[100043]);

                    _DatasByCategoryType[("scene","character")].Add(_DataByUid[100044]);

                    _DatasByCategoryType[("character","parameter")].Add(_DataByUid[100045]);

                    _DatasByCategoryType[("character","system")].Add(_DataByUid[100046]);

                    _DatasByCategory = new Dictionary<string, List<Data>>() {
    
                            {"ui",new List<Data>()},
        
                            {"basic",new List<Data>()},
        
                            {"scene",new List<Data>()},
        
                            {"item",new List<Data>()},
        
                            {"character",new List<Data>()},
        
                };

                    _DatasByCategory["ui"].Add(_DataByUid[100001]);

                    _DatasByCategory["basic"].Add(_DataByUid[100002]);

                    _DatasByCategory["basic"].Add(_DataByUid[100003]);

                    _DatasByCategory["basic"].Add(_DataByUid[100004]);

                    _DatasByCategory["basic"].Add(_DataByUid[100005]);

                    _DatasByCategory["basic"].Add(_DataByUid[100006]);

                    _DatasByCategory["basic"].Add(_DataByUid[100007]);

                    _DatasByCategory["basic"].Add(_DataByUid[100008]);

                    _DatasByCategory["ui"].Add(_DataByUid[100009]);

                    _DatasByCategory["scene"].Add(_DataByUid[100010]);

                    _DatasByCategory["ui"].Add(_DataByUid[100011]);

                    _DatasByCategory["ui"].Add(_DataByUid[100012]);

                    _DatasByCategory["ui"].Add(_DataByUid[100014]);

                    _DatasByCategory["ui"].Add(_DataByUid[100015]);

                    _DatasByCategory["ui"].Add(_DataByUid[100016]);

                    _DatasByCategory["ui"].Add(_DataByUid[100017]);

                    _DatasByCategory["ui"].Add(_DataByUid[100018]);

                    _DatasByCategory["ui"].Add(_DataByUid[100019]);

                    _DatasByCategory["basic"].Add(_DataByUid[100020]);

                    _DatasByCategory["basic"].Add(_DataByUid[100021]);

                    _DatasByCategory["basic"].Add(_DataByUid[100022]);

                    _DatasByCategory["basic"].Add(_DataByUid[100023]);

                    _DatasByCategory["basic"].Add(_DataByUid[100024]);

                    _DatasByCategory["basic"].Add(_DataByUid[100025]);

                    _DatasByCategory["basic"].Add(_DataByUid[100026]);

                    _DatasByCategory["basic"].Add(_DataByUid[100027]);

                    _DatasByCategory["basic"].Add(_DataByUid[100028]);

                    _DatasByCategory["basic"].Add(_DataByUid[100029]);

                    _DatasByCategory["basic"].Add(_DataByUid[100030]);

                    _DatasByCategory["basic"].Add(_DataByUid[100031]);

                    _DatasByCategory["basic"].Add(_DataByUid[100032]);

                    _DatasByCategory["basic"].Add(_DataByUid[100033]);

                    _DatasByCategory["basic"].Add(_DataByUid[100034]);

                    _DatasByCategory["basic"].Add(_DataByUid[100035]);

                    _DatasByCategory["scene"].Add(_DataByUid[100036]);

                    _DatasByCategory["scene"].Add(_DataByUid[100037]);

                    _DatasByCategory["scene"].Add(_DataByUid[100038]);

                    _DatasByCategory["scene"].Add(_DataByUid[100039]);

                    _DatasByCategory["scene"].Add(_DataByUid[100040]);

                    _DatasByCategory["item"].Add(_DataByUid[100041]);

                    _DatasByCategory["item"].Add(_DataByUid[100042]);

                    _DatasByCategory["scene"].Add(_DataByUid[100043]);

                    _DatasByCategory["scene"].Add(_DataByUid[100044]);

                    _DatasByCategory["character"].Add(_DataByUid[100045]);

                    _DatasByCategory["character"].Add(_DataByUid[100046]);

                    _DatasByCancreate = new Dictionary<bool, List<Data>>() {
    
                            {true,new List<Data>()},
        
                            {false,new List<Data>()},
        
                };

                    _DatasByCancreate[true].Add(_DataByUid[100001]);

                    _DatasByCancreate[false].Add(_DataByUid[100002]);

                    _DatasByCancreate[false].Add(_DataByUid[100003]);

                    _DatasByCancreate[false].Add(_DataByUid[100004]);

                    _DatasByCancreate[false].Add(_DataByUid[100005]);

                    _DatasByCancreate[false].Add(_DataByUid[100006]);

                    _DatasByCancreate[false].Add(_DataByUid[100007]);

                    _DatasByCancreate[true].Add(_DataByUid[100008]);

                    _DatasByCancreate[true].Add(_DataByUid[100009]);

                    _DatasByCancreate[true].Add(_DataByUid[100010]);

                    _DatasByCancreate[true].Add(_DataByUid[100011]);

                    _DatasByCancreate[true].Add(_DataByUid[100012]);

                    _DatasByCancreate[true].Add(_DataByUid[100014]);

                    _DatasByCancreate[true].Add(_DataByUid[100015]);

                    _DatasByCancreate[true].Add(_DataByUid[100016]);

                    _DatasByCancreate[true].Add(_DataByUid[100017]);

                    _DatasByCancreate[true].Add(_DataByUid[100018]);

                    _DatasByCancreate[true].Add(_DataByUid[100019]);

                    _DatasByCancreate[false].Add(_DataByUid[100020]);

                    _DatasByCancreate[false].Add(_DataByUid[100021]);

                    _DatasByCancreate[false].Add(_DataByUid[100022]);

                    _DatasByCancreate[false].Add(_DataByUid[100023]);

                    _DatasByCancreate[true].Add(_DataByUid[100024]);

                    _DatasByCancreate[true].Add(_DataByUid[100025]);

                    _DatasByCancreate[true].Add(_DataByUid[100026]);

                    _DatasByCancreate[false].Add(_DataByUid[100027]);

                    _DatasByCancreate[false].Add(_DataByUid[100028]);

                    _DatasByCancreate[false].Add(_DataByUid[100029]);

                    _DatasByCancreate[false].Add(_DataByUid[100030]);

                    _DatasByCancreate[true].Add(_DataByUid[100031]);

                    _DatasByCancreate[true].Add(_DataByUid[100032]);

                    _DatasByCancreate[true].Add(_DataByUid[100033]);

                    _DatasByCancreate[true].Add(_DataByUid[100034]);

                    _DatasByCancreate[true].Add(_DataByUid[100035]);

                    _DatasByCancreate[true].Add(_DataByUid[100036]);

                    _DatasByCancreate[true].Add(_DataByUid[100037]);

                    _DatasByCancreate[true].Add(_DataByUid[100038]);

                    _DatasByCancreate[true].Add(_DataByUid[100039]);

                    _DatasByCancreate[true].Add(_DataByUid[100040]);

                    _DatasByCancreate[true].Add(_DataByUid[100041]);

                    _DatasByCancreate[true].Add(_DataByUid[100042]);

                    _DatasByCancreate[true].Add(_DataByUid[100043]);

                    _DatasByCancreate[true].Add(_DataByUid[100044]);

                    _DatasByCancreate[true].Add(_DataByUid[100045]);

                    _DatasByCancreate[true].Add(_DataByUid[100046]);


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

                jo.Get<string>("type"),

                jo.Get<bool>("canCreate"),

                jo.Get<EditorStyle>("lowestEditorStyle")
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

            jo.Set<bool>("canCreate",data.canCreate);

            jo.Set<EditorStyle>("lowestEditorStyle",data.lowestEditorStyle);

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
    
                    if(!DatasByCancreate.ContainsKey(data.canCreate))
                        DatasByCancreate[data.canCreate]=new List<Data>();
                    DatasByCancreate[data.canCreate].Add(data);
    
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
    
                    DatasByCancreate[data.canCreate].Remove(data);
                    if(DatasByCancreate[data.canCreate].Count==0)
                        DatasByCancreate.Remove(data.canCreate);
    
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
            
            public static void ChangeCancreate(Data superData,bool oldV,bool newV)
            {
                if(superData is Data data)
                {

                    DatasByCancreate[oldV].Remove(data);
                    if(DatasByCancreate[oldV].Count==0)
                        DatasByCancreate.Remove(oldV);
                    if(!DatasByCancreate.ContainsKey(newV))
                        DatasByCancreate[newV]=new List<Data>();
                    DatasByCancreate[newV].Add(data);
 
                changeCancreateAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeLowesteditorstyle(Data superData,EditorStyle oldV,EditorStyle newV)
            {
                if(superData is Data data)
                {

                changeLowesteditorstyleAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
    }
}
        