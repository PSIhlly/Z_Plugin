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

    public static partial class ModTextForm
    {

        

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Register()
        {

                TextBaseForm.childInitAction+=InitInternal;


                TextBaseForm.childRemoveAction+=RemoveChildren;
                TextBaseForm.childAddAction+=AddChildren;
            

            TextBaseForm.changeIdAction+=ChangeId;

            TextBaseForm.changeKeyAction+=ChangeKey;

            TextBaseForm.changeContentenAction+=ChangeContenten;

            TextBaseForm.changeContentcnAction+=ChangeContentcn;

            Z_Json.extra[typeof(Data)]=((obj)=>{
            if(obj is Data data)
                return GetJoByData(data);
            return null;
            },(jo)=>{
            return GetDataByJo(jo);
            });
        }
        
        private static bool inited;

        public static Z_Chain.Chain idChain =>TextBaseForm.idChain;

        public static Action childInitAction;
        public static Action<Data> childRemoveAction;
        public static Action<Data> childAddAction;

        public static Action<Data,int,int> changeIdAction;
                
        public static Action<Data,string,string> changeKeyAction;
                
        public static Action<Data,string,string> changeContentenAction;
                
        public static Action<Data,string,string> changeContentcnAction;
                


        public partial class Data : TextBaseForm.Data
        {

            public Data(int id,string key,string contentEn,string contentCn):base(id,key,contentEn,contentCn)
            {

             this.id = id;
             this.key = key;
             this.contentEn = contentEn;
             this.contentCn = contentCn;

            }

                public Data Copy(bool sameId = true)
                {
        return new Data(sameId? id:idChain.GetId(),key,contentEn,contentCn);
                }
            
        }

                   private static Data _defaultData=new Data(0,"","","");
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
    
            static Dictionary<string, Data> _DataByKey;
            public static Dictionary<string, Data> DataByKey
            {
                get
                {
                    Init();
                    return _DataByKey;
                }
            }
    

        static public void Init()
        {

            TextBaseForm.Init();

        }
        public static void InitInternal()
        {
            if(inited)
                return;
            inited=true;  

        

                _DataById = new Dictionary<int, Data>() {

                {1000001,new Data(1000001,"terrain","Terrain","地形")},

                {1000002,new Data(1000002,"texture","Texture","贴图")},

                {1000003,new Data(1000003,"transitionMask","Transition mask","过渡遮罩")},

                {1000004,new Data(1000004,"object","Object","物体")},

                {1000005,new Data(1000005,"item","item","道具")},

                {1000100,new Data(1000100,"erase","Erase","清除")},

                {1000101,new Data(1000101,"all erase","All erase","全部清除")},

                {1000102,new Data(1000102,"remain terrain","Remain terrain","保留地面")},

                {1000103,new Data(1000103,"texture only","Terrain texture only","仅地面贴图")},

                {1001000,new Data(1001000,"maxYTip","The height must be less than the ceiling of this level.","高度必须小于该层天花板")},

                {1001001,new Data(1001001,"minYTip","The height must be greater than the floor of this level.","高度必须大于该层地板")},

                {1100001,new Data(1100001,"Choose main character","Choose main character","选择主角")},

                {1110001,new Data(1110001,"Choose Hp param","Choose Hp param","选择血量参数")},

                {1110002,new Data(1110002,"Choose Speed param","Choose Speed param","选择移速参数")},

                {1110003,new Data(1110003,"Choose Idle anim","Choose Idle anim","选择闲置动画")},

                {1110004,new Data(1110004,"Choose Move anim","Choose Move anim","选择移动动画")},

                {1110005,new Data(1110005,"input value","Input value","输入值")},

                {1110006,new Data(1110006,"icon","Icon","图标")},

                {1110007,new Data(1110007,"avatar","Avatar","头像")},

                {1110008,new Data(1110008,"overview","Overview","简介")},

                {1110009,new Data(1110009,"parameter","Parameter","参数")},

                {1110010,new Data(1110010,"character","Character","人物")},

                {1110011,new Data(1110011,"mapObject","Map object","地图元素")},

                {1110012,new Data(1110012,"globalParameter","Global parameter","全局参数")},

                {1110013,new Data(1110013,"characterParameter","Character parameter","人物参数")},

                {1110014,new Data(1110014,"itemParameter","Item parameter","道具参数")},

                {1110015,new Data(1110015,"config","Config","设定")},

                {1110016,new Data(1110016,"event","Event","事件")},

                {1110017,new Data(1110017,"scene","Scene","场景")},

                {1110018,new Data(1110018,"name","Name","名称")},

                {1110019,new Data(1110019,"introduction","Introduction","介绍")},

                {1110020,new Data(1110020,"mainCharacter","Main character","主角")},

                {1110021,new Data(1110021,"appearance","Appearance ","外观")},

                {1210001,new Data(1210001,"dialog","dialog","对话")},

                {1210002,new Data(1210002,"tips","tips","提示")},

                {1210003,new Data(1210003,"empty","empty","空")},

                {1210004,new Data(1210004,"if","if","如果")},

                {1210005,new Data(1210005,"then","then","满足执行")},

                {1210006,new Data(1210006,"else","else","不满足执行")},

                {1210007,new Data(1210007,"content","content","内容")},

                {1210008,new Data(1210008,"conditionJudge","condition judge","条件")},

                {1210009,new Data(1210009,"execute","execute","执行内容")},

                {1210010,new Data(1210010,"text","text","文本")},

                {1210011,new Data(1210011,"num","num","数值")},

                {1210012,new Data(1210012,"dialogClip","dialog clip","对话片段")},

                {1210013,new Data(1210013,"value","value","值")},

                {1210014,new Data(1210014,"logic","logic","逻辑")},

                {1210015,new Data(1210015,"window","window","窗体")},

                {1210016,new Data(1210016,"min","Min","最小")},

                {1210017,new Data(1210017,"max","Max","最大")},

                {1210018,new Data(1210018,"hpParameter","Hp parameter","血量参数")},

                {1210019,new Data(1210019,"moveSpeedParameter","Move speed parameter","移速参数")},

                {1210020,new Data(1210020,"idleAnim","Idle anim","待机动画")},

                {1210021,new Data(1210021,"moveAnim","Move anim","移动动画")},

                {1210022,new Data(1210022,"interval(s)","Interval(s)","间隔(秒)")},

                {1210023,new Data(1210023,"import","Import","导入")},

                {1210024,new Data(1210024,"upperPart","Upper part","上半身")},

                {1210025,new Data(1210025,"lowerPart","Lower part","下半身")},

                {1210026,new Data(1210026,"equipSetting","Equip setting","装备设置")},

                {1210027,new Data(1210027,"layer","Layer","层级")},

                {1210028,new Data(1210028,"scale","Scale","缩放")},

                {1210029,new Data(1210029,"height","Height","高")},

                {1210030,new Data(1210030,"width","Width","宽")},

                {1210031,new Data(1210031,"length","Length","长")},

                {1210032,new Data(1210032,"model","Model","模型")},

                {1210033,new Data(1210033,"style","Style","样式")},

                {1210034,new Data(1210034,"onUseEvent","On use event","使用事件")},

                {1210035,new Data(1210035,"price(coins)","Price(Coins)","价格(货币数)")},

                {1210036,new Data(1210036,"canEquipped","Can equipped","可装备")},

                {1210037,new Data(1210037,"onEquipEvent","On equip event","装备事件")},

                {1210038,new Data(1210038,"onDIsequipEvent","On disequip event","卸下事件")},

                {1210039,new Data(1210039,"part_1","Part","部位")},

                {1210040,new Data(1210040,"cube","Cube","方块")},

                {1210041,new Data(1210041,"sphere","Sphere","球")},

                {1210042,new Data(1210042,"verticalView","vertical view","俯视图")},

                {1210043,new Data(1210043,"leftView","leftView","左视图")},

                {1210044,new Data(1210044,"frontView","frontView","前视图")},

                {1210045,new Data(1210045,"condition","Condition","条件")},

                {1210046,new Data(1210046,"fixed","Fixed","固定")},

                {1210047,new Data(1210047,"onTouchEvent","On touch event","接触事件")},

                {1210048,new Data(1210048,"onLeaveEvent","On leave event","离开事件")},

                {1210049,new Data(1210049,"onShowEvent","OnShow","出现事件")},

                {1210050,new Data(1210050,"customEvent","Custom event","自定义事件")},

                {1210051,new Data(1210051,"globalEvent","Global event","全局事件")},

                {1210052,new Data(1210052,"category","Category","一级分类")},

                {1210053,new Data(1210053,"type","Type","二级分类")},

                {1210054,new Data(1210054,"edit","Edit","编辑")},

                {1210055,new Data(1210055,"onBeginEvent","On begin event","开幕事件")},

                {1210056,new Data(1210056,"onEndEvent","On end event","结局事件")},

                {1210057,new Data(1210057,"skillEvent","Skill event","技能事件")},

                {1210058,new Data(1210058,"map","Map","地图")},

                {1210059,new Data(1210059,"setPos","Set position","设置位置")},

                {1210060,new Data(1210060,"partSetting","Part setting","部位设置")},

                {1210061,new Data(1210061,"enablePart","Enable part","启用部位")},

                {1210062,new Data(1210062,"itemStyle","Item style","道具样式")},

                {1210063,new Data(1210063,"code","Code","代码")},

                {1210064,new Data(1210064,"entry","Entry","列表")},

                {1210065,new Data(1210065,"skillEvent","Skill event","技能事件")},

                {1210066,new Data(1210066,"chooseModel","Choose model","选择模型")},

                {1210067,new Data(1210067,"chooseEquipPart","Choose equip part","选择装备部位")},

                {1210068,new Data(1210068,"","","无")},

                {1210069,new Data(1210069,"LeftHand","Left hand","左手")},

                {1210070,new Data(1210070,"RightHand","Right hand","右手")},

                {1210071,new Data(1210071,"Head","Head","头")},

                {1210072,new Data(1210072,"Body","Body","身体")},

                {1210073,new Data(1210073,"label","Label","标签")},

                };
                    _DataByKey = new Dictionary<string, Data>() {
    
                        {"terrain",_DataById[1000001]},
    
                        {"texture",_DataById[1000002]},
    
                        {"transitionMask",_DataById[1000003]},
    
                        {"object",_DataById[1000004]},
    
                        {"item",_DataById[1000005]},
    
                        {"erase",_DataById[1000100]},
    
                        {"all erase",_DataById[1000101]},
    
                        {"remain terrain",_DataById[1000102]},
    
                        {"texture only",_DataById[1000103]},
    
                        {"maxYTip",_DataById[1001000]},
    
                        {"minYTip",_DataById[1001001]},
    
                        {"Choose main character",_DataById[1100001]},
    
                        {"Choose Hp param",_DataById[1110001]},
    
                        {"Choose Speed param",_DataById[1110002]},
    
                        {"Choose Idle anim",_DataById[1110003]},
    
                        {"Choose Move anim",_DataById[1110004]},
    
                        {"input value",_DataById[1110005]},
    
                        {"icon",_DataById[1110006]},
    
                        {"avatar",_DataById[1110007]},
    
                        {"overview",_DataById[1110008]},
    
                        {"parameter",_DataById[1110009]},
    
                        {"character",_DataById[1110010]},
    
                        {"mapObject",_DataById[1110011]},
    
                        {"globalParameter",_DataById[1110012]},
    
                        {"characterParameter",_DataById[1110013]},
    
                        {"itemParameter",_DataById[1110014]},
    
                        {"config",_DataById[1110015]},
    
                        {"event",_DataById[1110016]},
    
                        {"scene",_DataById[1110017]},
    
                        {"name",_DataById[1110018]},
    
                        {"introduction",_DataById[1110019]},
    
                        {"mainCharacter",_DataById[1110020]},
    
                        {"appearance",_DataById[1110021]},
    
                        {"dialog",_DataById[1210001]},
    
                        {"tips",_DataById[1210002]},
    
                        {"empty",_DataById[1210003]},
    
                        {"if",_DataById[1210004]},
    
                        {"then",_DataById[1210005]},
    
                        {"else",_DataById[1210006]},
    
                        {"content",_DataById[1210007]},
    
                        {"conditionJudge",_DataById[1210008]},
    
                        {"execute",_DataById[1210009]},
    
                        {"text",_DataById[1210010]},
    
                        {"num",_DataById[1210011]},
    
                        {"dialogClip",_DataById[1210012]},
    
                        {"value",_DataById[1210013]},
    
                        {"logic",_DataById[1210014]},
    
                        {"window",_DataById[1210015]},
    
                        {"min",_DataById[1210016]},
    
                        {"max",_DataById[1210017]},
    
                        {"hpParameter",_DataById[1210018]},
    
                        {"moveSpeedParameter",_DataById[1210019]},
    
                        {"idleAnim",_DataById[1210020]},
    
                        {"moveAnim",_DataById[1210021]},
    
                        {"interval(s)",_DataById[1210022]},
    
                        {"import",_DataById[1210023]},
    
                        {"upperPart",_DataById[1210024]},
    
                        {"lowerPart",_DataById[1210025]},
    
                        {"equipSetting",_DataById[1210026]},
    
                        {"layer",_DataById[1210027]},
    
                        {"scale",_DataById[1210028]},
    
                        {"height",_DataById[1210029]},
    
                        {"width",_DataById[1210030]},
    
                        {"length",_DataById[1210031]},
    
                        {"model",_DataById[1210032]},
    
                        {"style",_DataById[1210033]},
    
                        {"onUseEvent",_DataById[1210034]},
    
                        {"price(coins)",_DataById[1210035]},
    
                        {"canEquipped",_DataById[1210036]},
    
                        {"onEquipEvent",_DataById[1210037]},
    
                        {"onDIsequipEvent",_DataById[1210038]},
    
                        {"part_1",_DataById[1210039]},
    
                        {"cube",_DataById[1210040]},
    
                        {"sphere",_DataById[1210041]},
    
                        {"verticalView",_DataById[1210042]},
    
                        {"leftView",_DataById[1210043]},
    
                        {"frontView",_DataById[1210044]},
    
                        {"condition",_DataById[1210045]},
    
                        {"fixed",_DataById[1210046]},
    
                        {"onTouchEvent",_DataById[1210047]},
    
                        {"onLeaveEvent",_DataById[1210048]},
    
                        {"onShowEvent",_DataById[1210049]},
    
                        {"customEvent",_DataById[1210050]},
    
                        {"globalEvent",_DataById[1210051]},
    
                        {"category",_DataById[1210052]},
    
                        {"type",_DataById[1210053]},
    
                        {"edit",_DataById[1210054]},
    
                        {"onBeginEvent",_DataById[1210055]},
    
                        {"onEndEvent",_DataById[1210056]},
    
                        {"skillEvent",_DataById[1210057]},
    
                        {"map",_DataById[1210058]},
    
                        {"setPos",_DataById[1210059]},
    
                        {"partSetting",_DataById[1210060]},
    
                        {"enablePart",_DataById[1210061]},
    
                        {"itemStyle",_DataById[1210062]},
    
                        {"code",_DataById[1210063]},
    
                        {"entry",_DataById[1210064]},
    
                        {"skillEvent",_DataById[1210065]},
    
                        {"chooseModel",_DataById[1210066]},
    
                        {"chooseEquipPart",_DataById[1210067]},
    
                        {"",_DataById[1210068]},
    
                        {"LeftHand",_DataById[1210069]},
    
                        {"RightHand",_DataById[1210070]},
    
                        {"Head",_DataById[1210071]},
    
                        {"Body",_DataById[1210072]},
    
                        {"label",_DataById[1210073]},
    
                    };
    

            childInitAction?.Invoke();
            

            foreach(var data in DataById.Values)
            {
                TextBaseForm.AddData(data);
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

                jo.Get<string>("key"),

                jo.Get<string>("contentEn"),

                jo.Get<string>("contentCn")
                    );

            return data;
        }

        public static JObject GetJoByData(Data data)
        {
            Init();

            JObject jo=new JObject();

            jo.Set<int>("id",data.id);

            jo.Set<string>("key",data.key);

            jo.Set<string>("contentEn",data.contentEn);

            jo.Set<string>("contentCn",data.contentCn);

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
    
                    DataByKey[data.key]=data;
    
TextBaseForm.AddData(data);
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
    
                    DataByKey.Remove(data.key);
    
TextBaseForm.RemoveData(id);
            idChain.PushId(data.id);
            childRemoveAction?.Invoke(data);
        }
        public static void Clear()
        {
            Init();

                    DataById.Clear();
    
                    DataByKey.Clear();
    
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

         private static void RemoveChildren(TextBaseForm.Data data)
        {
            Init();
            if(data is Data)
               RemoveData(data.id);      
        }
         private static void AddChildren(TextBaseForm.Data superData)
        {
            Init();
            if(superData is Data data)
               AddData(data);      
        }
        




            public static void ChangeId(TextBaseForm.Data superData,int oldV,int newV)
            {
                if(superData is Data data)
                {

                changeIdAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeKey(TextBaseForm.Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                    DataByKey.Remove(oldV);
                    DataByKey[newV]=data;
 
                changeKeyAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeContenten(TextBaseForm.Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeContentenAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeContentcn(TextBaseForm.Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeContentcnAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
    }
}
        