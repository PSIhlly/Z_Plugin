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
                
        public static Action<Data,EditorStyle,EditorStyle> changeLowesteditorstyleAction;
                
        public static Action<Data,string,string> changeAllowasvoidAction;
                


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
                    
                    private string  _allowAsVoid;
                    /// <summary>
                    ///允许独立声明，使用的语句前缀
                    ///</summary>
                    public string  allowAsVoid{
                                get{return _allowAsVoid;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeAllowasvoid(this,_allowAsVoid,value); 
                    }
        
                _allowAsVoid = value;
                }
                 
                     }
                    
            public Data(CmdDataForm.Data data):base(data.uid,data.name,data.prmNames,data.prmTypes,data.retNames,data.retTypes,data.desc,data.defaultCode)
            {
            }
            
            public Data(int uid,string name,List<string> prmNames,List<string> prmTypes,List<string> retNames,List<string> retTypes,string desc,string defaultCode,string category,string type,EditorStyle lowestEditorStyle,string allowAsVoid):base(uid,name,prmNames,prmTypes,retNames,retTypes,desc,defaultCode)
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
             this.lowestEditorStyle = lowestEditorStyle;
             this.allowAsVoid = allowAsVoid;

            }
            public void Reset(Data data)
            {

             this.uid = data.uid;
             this.name = data.name;
             this.prmNames = data.prmNames;
             this.prmTypes = data.prmTypes;
             this.retNames = data.retNames;
             this.retTypes = data.retTypes;
             this.desc = data.desc;
             this.defaultCode = data.defaultCode;
             this.category = data.category;
             this.type = data.type;
             this.lowestEditorStyle = data.lowestEditorStyle;
             this.allowAsVoid = data.allowAsVoid;
            }

                public Data Copy(bool sameId = true)
                {
        return new Data(sameId? uid:uidChain.GetId(),name,new List<string>(prmNames),new List<string>(prmTypes),new List<string>(retNames),new List<string>(retTypes),desc,defaultCode,category,type,lowestEditorStyle,allowAsVoid);
                }
            
        }

                   private static Data _defaultData=new Data(0,"",null,null,null,null,"","","","",default,"");
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

                {100001,new Data(100001,"ShowTip",new List<string>(){"content",},new List<string>(){"string",},null,new List<string>(){"void",},"Show Tip: {0}","ShowTip(\"empty\");","ui","notice",EditorStyle.AvgAdvanced,"")},

                {100002,new Data(100002,"Text",null,null,null,new List<string>(){"string",},"","\"\"","basic","const",EditorStyle.Avg,"")},

                {100003,new Data(100003,"Num",null,null,null,new List<string>(){"num",},"","1","basic","const",EditorStyle.AvgAdvanced,"")},

                {100004,new Data(100004,"Image",null,null,null,new List<string>(){"img",},"","\"$i$$i$\"","basic","const",EditorStyle.Avg,"")},

                {100005,new Data(100005,"Video",null,null,null,new List<string>(){"video",},"","\"$v$$v$\"","basic","const",EditorStyle.AvgAdvanced,"")},

                {100006,new Data(100006,"Audio",null,null,null,new List<string>(){"audio",},"","\"$a$$a$\"","basic","const",EditorStyle.AvgAdvanced,"")},

                {100007,new Data(100007,"LocalVar",null,null,null,new List<string>(){"var",},"","a","basic","variable",EditorStyle.AvgAdvanced,"")},

                {100008,new Data(100008,"SetLocalVar",null,null,null,new List<string>(){"void",},"","a = 0;","basic","variable",EditorStyle.AvgAdvanced,"")},

                {100009,new Data(100009,"ShowDialog",new List<string>(){"background","avatar","title","content",},new List<string>(){"img","img","string","string",},null,new List<string>(){"void",},"Show Dialog{1}{2}:{3} bg:{0}","ShowDialog(\"$i$$i$\",\"$i$$i$\",\"empty\",\"empty\");","ui","dialog",EditorStyle.Avg,"")},

                {100010,new Data(100010,"ShowEffect",new List<string>(){"effect","pos","angle",},new List<string>(){"effect","vector","num",},null,new List<string>(){"void",},"Show Effect{0} {1} , angle{2}","ShowEffect(\"$ef$$ef$\",NewVector(1,1,0),0);","scene","effect",EditorStyle.RpgAdvanced,"")},

                {100011,new Data(100011,"CloseCurrentDialog",null,null,null,new List<string>(){"void",},"Close Current Dialog","CloseCurrentDialog();","ui","dialog",EditorStyle.AvgAdvanced,"")},

                {100012,new Data(100012,"ShowAdvancedDialog",new List<string>(){"background","avatar","title","content","audio","video","canClickPass",},new List<string>(){"img","img","string","string","audio","video","bool",},null,new List<string>(){"void",},"Show Dialog{1}{2}:{3} bg:{0},can click pass?{6},audio:{4} video:{5}","ShowAdvancedDialog(\"$i$$i$\",\"$i$$i$\",\"empty\",\"empty\",\"$a$$a$\",\"$v$$v$\",true);","ui","dialog",EditorStyle.AvgAdvanced,"")},

                {100013,new Data(100013,"ShowImage",new List<string>(){"image","scale","showTime",},new List<string>(){"img","num","num",},new List<string>(){"image",},new List<string>(){"uiImg",},"Show Image:{0} ,scale {1}, last {2} seconds","ShowImage(\"$i$$i$\",1,1)","ui","image",EditorStyle.Avg,"image = ")},

                {100014,new Data(100014,"ShowImagePermanently",new List<string>(){"image","scale",},new List<string>(){"img","num",},new List<string>(){"image",},new List<string>(){"uiImg",},"Show Image Permanently {0},scale {1}","ShowImagePermanently(\"$i$$i$\",1)","ui","image",EditorStyle.Avg,"image = ")},

                {100015,new Data(100015,"DeleteImage",new List<string>(){"image","afterTime",},new List<string>(){"uiImg","num",},null,new List<string>(){"void",},"Delete Image {0}  after {1} seconds","DeleteImage(image,1);","ui","image",EditorStyle.Avg,"")},

                {100016,new Data(100016,"SetImagePos",new List<string>(){"image","x","y","transitionTime",},new List<string>(){"uiImg","num","num","num",},null,new List<string>(){"void",},"Set Image{0}: x {1} y {2} ,transition takes {3} seconds","SetImagePos(image,0.5,0.5,1);","ui","image",EditorStyle.AvgAdvanced,"")},

                {100017,new Data(100017,"SetImageOpacity",new List<string>(){"image","opacity","transitionTime",},new List<string>(){"uiImg","num","num",},null,new List<string>(){"void",},"Set Image{0}: opacity {1},transition takes {2} seconds","SetImageOpacity(image,1,1);","ui","image",EditorStyle.AvgAdvanced,"")},

                {100018,new Data(100018,"SetImageRotate",new List<string>(){"image","angle","transitionTime",},new List<string>(){"uiImg","num","num",},null,new List<string>(){"void",},"Set Image{0}: angle {1},transition takes {2} seconds","SetImageRotate(image,0,1);","ui","image",EditorStyle.AvgAdvanced,"")},

                {100019,new Data(100019,"Add",new List<string>(){"num","num",},new List<string>(){"num","num",},new List<string>(){"resault",},new List<string>(){"num",},"","1 + 1","basic","math",EditorStyle.AvgAdvanced,"")},

                {100020,new Data(100020,"Subtract",new List<string>(){"num","num",},new List<string>(){"num","num",},new List<string>(){"resault",},new List<string>(){"num",},"","1 - 1","basic","math",EditorStyle.AvgAdvanced,"")},

                {100021,new Data(100021,"Multiply",new List<string>(){"num","num",},new List<string>(){"num","num",},new List<string>(){"resault",},new List<string>(){"num",},"","1 * 1","basic","math",EditorStyle.AvgAdvanced,"")},

                {100022,new Data(100022,"Divide",new List<string>(){"num","num",},new List<string>(){"num","num",},new List<string>(){"resault",},new List<string>(){"num",},"","1 / 1","basic","math",EditorStyle.AvgAdvanced,"")},

                {100023,new Data(100023,"Wait",new List<string>(){"seconds",},new List<string>(){"num",},null,new List<string>(){"void",},"Wait {0} seconds","Wait(1);","basic","process",EditorStyle.AvgAdvanced,"")},

                {100024,new Data(100024,"If",null,null,null,new List<string>(){"void",},"","if(1){ }else{ }","basic","process",EditorStyle.AvgAdvanced,"")},

                {100025,new Data(100025,"For",null,null,null,new List<string>(){"void",},"","for(i=1;i<5;i=i+1){ }","basic","process",EditorStyle.AvgAdvanced,"")},

                {100026,new Data(100026,"Greater",new List<string>(){"num","num",},new List<string>(){"num","num",},new List<string>(){"resault",},new List<string>(){"num",},"","0 > 1","basic","math",EditorStyle.AvgAdvanced,"")},

                {100027,new Data(100027,"Less",new List<string>(){"num","num",},new List<string>(){"num","num",},new List<string>(){"resault",},new List<string>(){"num",},"","0 < 1","basic","math",EditorStyle.AvgAdvanced,"")},

                {100028,new Data(100028,"Equal",new List<string>(){"num","num",},new List<string>(){"num","num",},new List<string>(){"resault",},new List<string>(){"num",},"","0 == 1","basic","math",EditorStyle.AvgAdvanced,"")},

                {100029,new Data(100029,"NotEqual",new List<string>(){"num","num",},new List<string>(){"num","num",},new List<string>(){"resault",},new List<string>(){"num",},"","0 != 1","basic","math",EditorStyle.AvgAdvanced,"")},

                {100030,new Data(100030,"Pause",null,null,null,new List<string>(){"void",},"Pause","Pause();","basic","process",EditorStyle.Rpg,"")},

                {100031,new Data(100031,"Continue",null,null,null,new List<string>(){"void",},"Continue","Continue();","basic","process",EditorStyle.Rpg,"")},

                {100032,new Data(100032,"GameOver",null,null,null,new List<string>(){"void",},"Game over","GameOver();","basic","process",EditorStyle.Avg,"")},

                {100033,new Data(100033,"Save",null,null,null,new List<string>(){"void",},"Save","Save();","basic","process",EditorStyle.Avg,"")},

                {100034,new Data(100034,"Load",null,null,null,new List<string>(){"void",},"Load","Load();","basic","process",EditorStyle.Avg,"")},

                {100035,new Data(100035,"DestroyObject",new List<string>(){"object",},new List<string>(){"sceneObject",},null,new List<string>(){"void",},"object {0}: Destroy","DestroyObject(self);","scene","object",EditorStyle.Rpg,"")},

                {100036,new Data(100036,"GenerateObject",new List<string>(){"name",},new List<string>(){"string",},new List<string>(){"new Object",},new List<string>(){"sceneObject",},"object name {0}: Generate:","GenerateObject(\"empty\")","scene","object",EditorStyle.Rpg,"newObjectID = ")},

                {100037,new Data(100037,"MoveObjectRelative",new List<string>(){"object","pos","transitionTime",},new List<string>(){"sceneObject","vector","num",},null,new List<string>(){"void",},"object {0} : Move to relative coordinates({1}), last {2} seconds","MoveObjectRelative(self,NewVector(1,1,0),1);","scene","object",EditorStyle.Rpg,"")},

                {100038,new Data(100038,"MoveObjectAbsolute",new List<string>(){"object","pos","transitionTime",},new List<string>(){"sceneObject","vector","num",},null,new List<string>(){"void",},"object {0} : Move to absolute coordinates({1}), last {2} seconds","MoveObjectAbsolute(self,NewVector(1,1,0),1);","scene","object",EditorStyle.Rpg,"")},

                {100039,new Data(100039,"GainItem",new List<string>(){"item","amount",},new List<string>(){"item","num",},null,new List<string>(){"void",},"Gain {0} x {1}","GainItem(\"empty\",1);","item","backpack",EditorStyle.Rpg,"")},

                {100040,new Data(100040,"LostItem",new List<string>(){"item","amount",},new List<string>(){"item","num",},null,new List<string>(){"void",},"Lost {0} x {1}","LostItem(\"empty\",1);","item","backpack",EditorStyle.Rpg,"")},

                {100041,new Data(100041,"SetCharacterParameter",new List<string>(){"character","paramName","value",},new List<string>(){"character","string","num",},null,new List<string>(){"void",},"Set Character ID {0} 's {1} = {2}","SetCharacterParameter(self,\"empty\",1);","character","parameter",EditorStyle.Rpg,"")},

                {100042,new Data(100042,"GetCurrentCharacter",null,null,new List<string>(){"character",},new List<string>(){"character",},"Get currently used character ID","GetCurrentCharacter()","character","system",EditorStyle.Rpg,"")},

                {100043,new Data(100043,"MoveCharacterRelative",new List<string>(){"character","pos","transitionTime",},new List<string>(){"character","vector","num",},null,new List<string>(){"void",},"character {0} : Move to relative coordinates({1}),last {2} seconds","MoveCharacterRelative(self,NewVector(1,1,0),1);","scene","character",EditorStyle.RpgAdvanced,"")},

                {100044,new Data(100044,"MoveCharacterAbsolute",new List<string>(){"character","pos","transitionTime",},new List<string>(){"character","vector","num",},null,new List<string>(){"void",},"character {0} : Move to absolute coordinates({1}),last {2} seconds","MoveCharacterAbsolute(self,NewVector(1,1,0),1);","scene","character",EditorStyle.RpgAdvanced,"")},

                {100045,new Data(100045,"SetCharacterNavigateRelative",new List<string>(){"characterID","pos",},new List<string>(){"num","vector",},null,new List<string>(){"void",},"character {0} : Navigate to relative coordinates({1})","SetCharacterNavigateRelative(self,NewVector(1,1,0));","scene","character",EditorStyle.RpgAdvanced,"")},

                {100046,new Data(100046,"SetCharacterNavigateAbsolute",new List<string>(){"characterID","pos",},new List<string>(){"num","vector",},null,new List<string>(){"void",},"character {0} : Navigate to absolute coordinates({1})","SetCharacterNavigateAbsolute(self,NewVector(1,1,0));","scene","character",EditorStyle.RpgAdvanced,"")},

                {100047,new Data(100047,"GetCharacterPos",new List<string>(){"character",},new List<string>(){"character",},new List<string>(){"pos",},new List<string>(){"vector",},"character {0} position","GetCharacterPos(self)","scene","character",EditorStyle.RpgAdvanced,"")},

                {100048,new Data(100048,"NewVector",new List<string>(){"x","y","height",},new List<string>(){"num","num","num",},new List<string>(){"vector",},new List<string>(){"vector",},"vector({0},{1},{2})","NewVector(0,0,0);","scene","character",EditorStyle.RpgAdvanced,"")},

                {100050,new Data(100050,"StopCharacterNavigate",new List<string>(){"character",},new List<string>(){"character",},null,new List<string>(){"void",},"character {0} : Navigate stop","StopCharacterNavigate(self)","scene","character",EditorStyle.RpgAdvanced,"")},

                {100051,new Data(100051,"SetCharacterPositionRelative",new List<string>(){"character","pos",},new List<string>(){"character","vector",},null,new List<string>(){"void",},"character {0} : Set relative position ({1})","SetCharacterPositionRelative(self,NewVector(1,1,0));","scene","character",EditorStyle.RpgAdvanced,"")},

                {100052,new Data(100052,"SetCharacterPositionAbsolute",new List<string>(){"character","pos",},new List<string>(){"character","vector",},null,new List<string>(){"void",},"character {0} : Set absolute position ({1})","SetCharacterPositionAbsolute(self,NewVector(1,1,0));","scene","character",EditorStyle.RpgAdvanced,"")},

                {100053,new Data(100053,"GetCharacterParameter",new List<string>(){"character","paramName",},new List<string>(){"character","string",},new List<string>(){"value",},new List<string>(){"num",},"Get Character {0} 's {1}","GetCharacterParameter(self,\"empty\")","character","parameter",EditorStyle.Rpg,"")},

                {100054,new Data(100054,"SelfSceneObject",null,null,null,new List<string>(){"sceneObject",},"","self","scene","object",EditorStyle.Rpg,"")},

                {100055,new Data(100055,"TriggerSceneObject",null,null,null,new List<string>(){"sceneObject",},"","trigger","scene","object",EditorStyle.Rpg,"")},

                {100056,new Data(100056,"SelfCharacter",null,null,null,new List<string>(){"character",},"","self","scene","character",EditorStyle.Rpg,"")},

                {100057,new Data(100057,"TriggerCharacter",null,null,null,new List<string>(){"character",},"","trigger","scene","character",EditorStyle.Rpg,"")},

                {100058,new Data(100058,"Return",new List<string>(){"result",},new List<string>(){"var",},null,new List<string>(){"void",},"return {0}","Return result;","basic","process",EditorStyle.Avg,"")},

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
    
                        {"ShowImage",_DataByUid[100013]},
    
                        {"ShowImagePermanently",_DataByUid[100014]},
    
                        {"DeleteImage",_DataByUid[100015]},
    
                        {"SetImagePos",_DataByUid[100016]},
    
                        {"SetImageOpacity",_DataByUid[100017]},
    
                        {"SetImageRotate",_DataByUid[100018]},
    
                        {"Add",_DataByUid[100019]},
    
                        {"Subtract",_DataByUid[100020]},
    
                        {"Multiply",_DataByUid[100021]},
    
                        {"Divide",_DataByUid[100022]},
    
                        {"Wait",_DataByUid[100023]},
    
                        {"If",_DataByUid[100024]},
    
                        {"For",_DataByUid[100025]},
    
                        {"Greater",_DataByUid[100026]},
    
                        {"Less",_DataByUid[100027]},
    
                        {"Equal",_DataByUid[100028]},
    
                        {"NotEqual",_DataByUid[100029]},
    
                        {"Pause",_DataByUid[100030]},
    
                        {"Continue",_DataByUid[100031]},
    
                        {"GameOver",_DataByUid[100032]},
    
                        {"Save",_DataByUid[100033]},
    
                        {"Load",_DataByUid[100034]},
    
                        {"DestroyObject",_DataByUid[100035]},
    
                        {"GenerateObject",_DataByUid[100036]},
    
                        {"MoveObjectRelative",_DataByUid[100037]},
    
                        {"MoveObjectAbsolute",_DataByUid[100038]},
    
                        {"GainItem",_DataByUid[100039]},
    
                        {"LostItem",_DataByUid[100040]},
    
                        {"SetCharacterParameter",_DataByUid[100041]},
    
                        {"GetCurrentCharacter",_DataByUid[100042]},
    
                        {"MoveCharacterRelative",_DataByUid[100043]},
    
                        {"MoveCharacterAbsolute",_DataByUid[100044]},
    
                        {"SetCharacterNavigateRelative",_DataByUid[100045]},
    
                        {"SetCharacterNavigateAbsolute",_DataByUid[100046]},
    
                        {"GetCharacterPos",_DataByUid[100047]},
    
                        {"NewVector",_DataByUid[100048]},
    
                        {"StopCharacterNavigate",_DataByUid[100050]},
    
                        {"SetCharacterPositionRelative",_DataByUid[100051]},
    
                        {"SetCharacterPositionAbsolute",_DataByUid[100052]},
    
                        {"GetCharacterParameter",_DataByUid[100053]},
    
                        {"SelfSceneObject",_DataByUid[100054]},
    
                        {"TriggerSceneObject",_DataByUid[100055]},
    
                        {"SelfCharacter",_DataByUid[100056]},
    
                        {"TriggerCharacter",_DataByUid[100057]},
    
                        {"Return",_DataByUid[100058]},
    
                    };
    
                    _DatasByCategoryType = new Dictionary<(string,string), List<Data>>() {
    
                            {("ui","notice"),new List<Data>()},
        
                            {("basic","const"),new List<Data>()},
        
                            {("basic","variable"),new List<Data>()},
        
                            {("ui","dialog"),new List<Data>()},
        
                            {("scene","effect"),new List<Data>()},
        
                            {("ui","image"),new List<Data>()},
        
                            {("basic","math"),new List<Data>()},
        
                            {("basic","process"),new List<Data>()},
        
                            {("scene","object"),new List<Data>()},
        
                            {("item","backpack"),new List<Data>()},
        
                            {("character","parameter"),new List<Data>()},
        
                            {("character","system"),new List<Data>()},
        
                            {("scene","character"),new List<Data>()},
        
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

                    _DatasByCategoryType[("ui","dialog")].Add(_DataByUid[100011]);

                    _DatasByCategoryType[("ui","dialog")].Add(_DataByUid[100012]);

                    _DatasByCategoryType[("ui","image")].Add(_DataByUid[100013]);

                    _DatasByCategoryType[("ui","image")].Add(_DataByUid[100014]);

                    _DatasByCategoryType[("ui","image")].Add(_DataByUid[100015]);

                    _DatasByCategoryType[("ui","image")].Add(_DataByUid[100016]);

                    _DatasByCategoryType[("ui","image")].Add(_DataByUid[100017]);

                    _DatasByCategoryType[("ui","image")].Add(_DataByUid[100018]);

                    _DatasByCategoryType[("basic","math")].Add(_DataByUid[100019]);

                    _DatasByCategoryType[("basic","math")].Add(_DataByUid[100020]);

                    _DatasByCategoryType[("basic","math")].Add(_DataByUid[100021]);

                    _DatasByCategoryType[("basic","math")].Add(_DataByUid[100022]);

                    _DatasByCategoryType[("basic","process")].Add(_DataByUid[100023]);

                    _DatasByCategoryType[("basic","process")].Add(_DataByUid[100024]);

                    _DatasByCategoryType[("basic","process")].Add(_DataByUid[100025]);

                    _DatasByCategoryType[("basic","math")].Add(_DataByUid[100026]);

                    _DatasByCategoryType[("basic","math")].Add(_DataByUid[100027]);

                    _DatasByCategoryType[("basic","math")].Add(_DataByUid[100028]);

                    _DatasByCategoryType[("basic","math")].Add(_DataByUid[100029]);

                    _DatasByCategoryType[("basic","process")].Add(_DataByUid[100030]);

                    _DatasByCategoryType[("basic","process")].Add(_DataByUid[100031]);

                    _DatasByCategoryType[("basic","process")].Add(_DataByUid[100032]);

                    _DatasByCategoryType[("basic","process")].Add(_DataByUid[100033]);

                    _DatasByCategoryType[("basic","process")].Add(_DataByUid[100034]);

                    _DatasByCategoryType[("scene","object")].Add(_DataByUid[100035]);

                    _DatasByCategoryType[("scene","object")].Add(_DataByUid[100036]);

                    _DatasByCategoryType[("scene","object")].Add(_DataByUid[100037]);

                    _DatasByCategoryType[("scene","object")].Add(_DataByUid[100038]);

                    _DatasByCategoryType[("item","backpack")].Add(_DataByUid[100039]);

                    _DatasByCategoryType[("item","backpack")].Add(_DataByUid[100040]);

                    _DatasByCategoryType[("character","parameter")].Add(_DataByUid[100041]);

                    _DatasByCategoryType[("character","system")].Add(_DataByUid[100042]);

                    _DatasByCategoryType[("scene","character")].Add(_DataByUid[100043]);

                    _DatasByCategoryType[("scene","character")].Add(_DataByUid[100044]);

                    _DatasByCategoryType[("scene","character")].Add(_DataByUid[100045]);

                    _DatasByCategoryType[("scene","character")].Add(_DataByUid[100046]);

                    _DatasByCategoryType[("scene","character")].Add(_DataByUid[100047]);

                    _DatasByCategoryType[("scene","character")].Add(_DataByUid[100048]);

                    _DatasByCategoryType[("scene","character")].Add(_DataByUid[100050]);

                    _DatasByCategoryType[("scene","character")].Add(_DataByUid[100051]);

                    _DatasByCategoryType[("scene","character")].Add(_DataByUid[100052]);

                    _DatasByCategoryType[("character","parameter")].Add(_DataByUid[100053]);

                    _DatasByCategoryType[("scene","object")].Add(_DataByUid[100054]);

                    _DatasByCategoryType[("scene","object")].Add(_DataByUid[100055]);

                    _DatasByCategoryType[("scene","character")].Add(_DataByUid[100056]);

                    _DatasByCategoryType[("scene","character")].Add(_DataByUid[100057]);

                    _DatasByCategoryType[("basic","process")].Add(_DataByUid[100058]);

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

                    _DatasByCategory["ui"].Add(_DataByUid[100013]);

                    _DatasByCategory["ui"].Add(_DataByUid[100014]);

                    _DatasByCategory["ui"].Add(_DataByUid[100015]);

                    _DatasByCategory["ui"].Add(_DataByUid[100016]);

                    _DatasByCategory["ui"].Add(_DataByUid[100017]);

                    _DatasByCategory["ui"].Add(_DataByUid[100018]);

                    _DatasByCategory["basic"].Add(_DataByUid[100019]);

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

                    _DatasByCategory["scene"].Add(_DataByUid[100035]);

                    _DatasByCategory["scene"].Add(_DataByUid[100036]);

                    _DatasByCategory["scene"].Add(_DataByUid[100037]);

                    _DatasByCategory["scene"].Add(_DataByUid[100038]);

                    _DatasByCategory["item"].Add(_DataByUid[100039]);

                    _DatasByCategory["item"].Add(_DataByUid[100040]);

                    _DatasByCategory["character"].Add(_DataByUid[100041]);

                    _DatasByCategory["character"].Add(_DataByUid[100042]);

                    _DatasByCategory["scene"].Add(_DataByUid[100043]);

                    _DatasByCategory["scene"].Add(_DataByUid[100044]);

                    _DatasByCategory["scene"].Add(_DataByUid[100045]);

                    _DatasByCategory["scene"].Add(_DataByUid[100046]);

                    _DatasByCategory["scene"].Add(_DataByUid[100047]);

                    _DatasByCategory["scene"].Add(_DataByUid[100048]);

                    _DatasByCategory["scene"].Add(_DataByUid[100050]);

                    _DatasByCategory["scene"].Add(_DataByUid[100051]);

                    _DatasByCategory["scene"].Add(_DataByUid[100052]);

                    _DatasByCategory["character"].Add(_DataByUid[100053]);

                    _DatasByCategory["scene"].Add(_DataByUid[100054]);

                    _DatasByCategory["scene"].Add(_DataByUid[100055]);

                    _DatasByCategory["scene"].Add(_DataByUid[100056]);

                    _DatasByCategory["scene"].Add(_DataByUid[100057]);

                    _DatasByCategory["basic"].Add(_DataByUid[100058]);


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

                jo.Get<EditorStyle>("lowestEditorStyle"),

                jo.Get<string>("allowAsVoid")
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

            jo.Set<EditorStyle>("lowestEditorStyle",data.lowestEditorStyle);

            jo.Set<string>("allowAsVoid",data.allowAsVoid);

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
            
            public static void ChangeLowesteditorstyle(Data superData,EditorStyle oldV,EditorStyle newV)
            {
                if(superData is Data data)
                {

                changeLowesteditorstyleAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeAllowasvoid(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeAllowasvoidAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
    }
}
        