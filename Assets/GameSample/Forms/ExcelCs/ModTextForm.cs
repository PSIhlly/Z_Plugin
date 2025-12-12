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

        public static Action<Data> addAction;
        public static Action<Data> removeAction;
        public static Action childInitAction;
        public static Action<Data> childRemoveAction;
        public static Action<Data> childAddAction;

        public static Action<Data,int,int> changeIdAction;
                
        public static Action<Data,string,string> changeKeyAction;
                
        public static Action<Data,string,string> changeContentenAction;
                
        public static Action<Data,string,string> changeContentcnAction;
                


        public partial class Data : TextBaseForm.Data
        {

            public Data(TextBaseForm.Data data):base(data.id,data.key,data.contentEn,data.contentCn)
            {
            }
            
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

                {1000006,new Data(1000006,"erase","Erase","清除")},

                {1000007,new Data(1000007,"all erase","All erase","全部清除")},

                {1000008,new Data(1000008,"remain terrain","Remain terrain","保留地面")},

                {1000009,new Data(1000009,"texture only","Terrain texture only","仅地面贴图")},

                {1000010,new Data(1000010,"maxYTip","The height must be less than the ceiling of this level.","高度必须小于该层天花板")},

                {1000011,new Data(1000011,"minYTip","The height must be greater than the floor of this level.","高度必须大于该层地板")},

                {1000012,new Data(1000012,"Choose main character","Choose main character","选择主角")},

                {1000013,new Data(1000013,"Choose Hp param","Choose Hp param","选择血量参数")},

                {1000014,new Data(1000014,"Choose Speed param","Choose Speed param","选择移速参数")},

                {1000015,new Data(1000015,"Choose anim","Choose anim","选择动画")},

                {1000016,new Data(1000016,"input value","Input value","输入值")},

                {1000017,new Data(1000017,"icon","Icon","图标")},

                {1000018,new Data(1000018,"avatar","Avatar","头像")},

                {1000019,new Data(1000019,"overview","Overview","简介")},

                {1000020,new Data(1000020,"parameter","Parameter","参数")},

                {1000021,new Data(1000021,"character","Character","人物")},

                {1000022,new Data(1000022,"mapObject","Map object","地图元素")},

                {1000023,new Data(1000023,"globalParameter","Global parameter","全局参数")},

                {1000024,new Data(1000024,"characterParameter","Character parameter","人物参数")},

                {1000025,new Data(1000025,"itemParameter","Item parameter","道具参数")},

                {1000026,new Data(1000026,"config","Config","设定")},

                {1000027,new Data(1000027,"event","Event","事件")},

                {1000028,new Data(1000028,"scene","Scene","场景")},

                {1000029,new Data(1000029,"name","Name","名称")},

                {1000030,new Data(1000030,"introduction","Introduction","介绍")},

                {1000031,new Data(1000031,"mainCharacter","Main character","主角")},

                {1000032,new Data(1000032,"appearance","Appearance ","外观")},

                {1000033,new Data(1000033,"dialog","dialog","对话")},

                {1000034,new Data(1000034,"tips","tips","提示")},

                {1000035,new Data(1000035,"empty","empty","空")},

                {1000036,new Data(1000036,"if","if","如果")},

                {1000037,new Data(1000037,"then","then","满足执行")},

                {1000038,new Data(1000038,"else","else","不满足执行")},

                {1000039,new Data(1000039,"content","content","内容")},

                {1000040,new Data(1000040,"conditionJudge","condition judge","条件")},

                {1000041,new Data(1000041,"execute","execute","执行内容")},

                {1000042,new Data(1000042,"text","text","文本")},

                {1000043,new Data(1000043,"num","num","数值")},

                {1000044,new Data(1000044,"dialogClip","dialog clip","对话片段")},

                {1000045,new Data(1000045,"value","value","值")},

                {1000046,new Data(1000046,"logic","logic","逻辑")},

                {1000047,new Data(1000047,"window","window","窗体")},

                {1000048,new Data(1000048,"min","Min","最小")},

                {1000049,new Data(1000049,"max","Max","最大")},

                {1000050,new Data(1000050,"hpParameter","Hp parameter","血量参数")},

                {1000051,new Data(1000051,"moveSpeedParameter","Move speed parameter","移速参数")},

                {1000052,new Data(1000052,"idleAnim","Idle anim","待机动画")},

                {1000053,new Data(1000053,"moveAnim","Move anim","移动动画")},

                {1000054,new Data(1000054,"interval(s)","Interval(s)","间隔(秒)")},

                {1000055,new Data(1000055,"import","Import","导入")},

                {1000056,new Data(1000056,"UpperPart","Upper part","上半身")},

                {1000057,new Data(1000057,"LowerPart","Lower part","下半身")},

                {1000058,new Data(1000058,"equipSetting","Equip setting","装备设置")},

                {1000059,new Data(1000059,"layer","Layer","层级")},

                {1000060,new Data(1000060,"scale","Scale","缩放")},

                {1000061,new Data(1000061,"height","Height","高")},

                {1000062,new Data(1000062,"width","Width","宽")},

                {1000063,new Data(1000063,"length","Length","长")},

                {1000064,new Data(1000064,"model","Model","模型")},

                {1000065,new Data(1000065,"style","Style","样式")},

                {1000066,new Data(1000066,"onUseEvent","On use event","使用事件")},

                {1000067,new Data(1000067,"price(coins)","Price(Coins)","价格(货币数)")},

                {1000068,new Data(1000068,"canEquip","Can equipped","可装备")},

                {1000069,new Data(1000069,"onEquipEvent","On equip event","装备事件")},

                {1000070,new Data(1000070,"onDisequipEvent","On disequip event","卸下事件")},

                {1000071,new Data(1000071,"part_1","Part","部位")},

                {1000072,new Data(1000072,"Cube","Cube","方块")},

                {1000073,new Data(1000073,"Sphere","Sphere","球")},

                {1000074,new Data(1000074,"verticalView","vertical view","俯视图")},

                {1000075,new Data(1000075,"leftView","leftView","左视图")},

                {1000076,new Data(1000076,"frontView","frontView","前视图")},

                {1000077,new Data(1000077,"condition","Condition","条件")},

                {1000078,new Data(1000078,"fixed","Fixed","固定")},

                {1000079,new Data(1000079,"onCharacterTouchEvent","On character touch event","人物接触事件")},

                {1000080,new Data(1000080,"onCharacterLeaveEvent","On character leave event","人物离开事件")},

                {1000081,new Data(1000081,"onObjectTouchEvent","On object touch event","物体接触事件")},

                {1000082,new Data(1000082,"onObjectLeaveEvent","On object leave event","物体离开事件")},

                {1000083,new Data(1000083,"onShowEvent","OnShow","出现事件")},

                {1000084,new Data(1000084,"customEvent","Custom event","自定义事件")},

                {1000085,new Data(1000085,"globalEvent","Global event","全局事件")},

                {1000086,new Data(1000086,"category","Category","一级分类")},

                {1000087,new Data(1000087,"type","Type","二级分类")},

                {1000088,new Data(1000088,"edit","Edit","编辑")},

                {1000089,new Data(1000089,"onBeginEvent","On begin event","开幕事件")},

                {1000090,new Data(1000090,"skillEvent","Skill event","技能事件")},

                {1000091,new Data(1000091,"map","Map","地图")},

                {1000092,new Data(1000092,"setPos","Set position","设置位置")},

                {1000093,new Data(1000093,"partSetting","Part setting","部位设置")},

                {1000094,new Data(1000094,"enablePart","Enable part","启用部位")},

                {1000095,new Data(1000095,"itemStyle","Item style","道具外观样式")},

                {1000096,new Data(1000096,"code","Code","代码")},

                {1000097,new Data(1000097,"entry","Entry","列表")},

                {1000098,new Data(1000098,"chooseModel","Choose model","选择模型")},

                {1000099,new Data(1000099,"chooseEquipPart","Choose equip part","选择装备部位")},

                {1000100,new Data(1000100,"","","无")},

                {1000101,new Data(1000101,"LeftHand","Left hand","左手")},

                {1000102,new Data(1000102,"RightHand","Right hand","右手")},

                {1000103,new Data(1000103,"Head","Head","头")},

                {1000104,new Data(1000104,"Body","Body","身体")},

                {1000105,new Data(1000105,"label","Label","标签")},

                {1000106,new Data(1000106,"Choose show equipped item style","Choose show equipped item style","选择装备的道具外观样式")},

                {1000107,new Data(1000107,"Quad","Quad","面")},

                {1000108,new Data(1000108,"Capsule","Capsule","胶囊")},

                {1000109,new Data(1000109,"unclassified","Unclassified","未分类")},

                {1000110,new Data(1000110,"Please create a new one or select one from the following","Please create a new one or select one from the following.","请从下方新建或选择一项")},

                {1000111,new Data(1000111,"center_1","Center","居中")},

                {1000112,new Data(1000112,"Texture Layer Set","Texture layer set","贴图层级设置")},

                {1000113,new Data(1000113,"view position","View position","当前位置")},

                {1000114,new Data(1000114,"tool","Tool","工具")},

                {1000115,new Data(1000115,"rotation","Rotation","旋转")},

                {1000116,new Data(1000116,"position","Position","位置")},

                {1000117,new Data(1000117,"rotate 90°","Rotate 90°","旋转90°")},

                {1000118,new Data(1000118,"align","Align","对齐")},

                {1000119,new Data(1000119,"resetCount","Reset count","重置数量")},

                {1000120,new Data(1000120,"ui","UI","界面")},

                {1000121,new Data(1000121,"notice","Notice","通知")},

                {1000122,new Data(1000122,"Choose command","Choose command","选择命令")},

                {1000123,new Data(1000123,"show type","Show type","显示方式")},

                {1000124,new Data(1000124,"AlwaysWithPanel","Always with panel","总是且在面板")},

                {1000125,new Data(1000125,"Always","Always","总是")},

                {1000126,new Data(1000126,"OnlyNotZero","Only not zero","非零")},

                {1000127,new Data(1000127,"Hide","Hide","隐藏")},

                {1000128,new Data(1000128,"tachie","Tachie","立绘")},

                {1000129,new Data(1000129,"description","Description","简介")},

                {1000130,new Data(1000130,"effect","Effect","特效")},

                {1000131,new Data(1000131,"sustain(s)","Sustain(s)","持续(秒)")},

                {1000132,new Data(1000132,"transition","Transition","过渡")},

                {1000133,new Data(1000133,"opacity","Opacity","不透明度")},

                {1000134,new Data(1000134,"image","Image","图片")},

                {1000135,new Data(1000135,"onTriggerEvent","On trigger event","触发事件")},

                {1000136,new Data(1000136,"triggerCondition","Trigger condition","触发条件")},

                {1000137,new Data(1000137,"Choose effect","Choose effect","选择特效")},

                {1000138,new Data(1000138,"Choose perspective","Choose perspective","选择视角")},

                {1000139,new Data(1000139,"perspective","Perspective","视角")},

                {1000140,new Data(1000140,"Overhead","Overhead","俯视")},

                {1000141,new Data(1000141,"Isometric","Isometric","斜视")},

                {1000142,new Data(1000142,"dialogAdvanced","Dialog(Advanced)","对话(高级)")},

                {1000143,new Data(1000143,"const","Const","常量")},

                {1000144,new Data(1000144,"trigger","Trigger","触发")},

                {1000145,new Data(1000145,"NoLimit","no limit","不限制")},

                {1000146,new Data(1000146,"Once","once","一次性")},

                {1000147,new Data(1000147,"OnceDuring","once during triggering","期间一次")},

                {1000148,new Data(1000148,"unique","unique","唯一的")},

                {1000149,new Data(1000149,"Choose trigger condition","Choose trigger condition","选择触发条件")},

                {1000150,new Data(1000150,"insert","insert","插入")},

                {1000151,new Data(1000151,"basic","Basic","基础")},

                {1000152,new Data(1000152,"Text","Text","文本")},

                {1000153,new Data(1000153,"variable","Variable","变量")},

                {1000154,new Data(1000154,"LocalVar","Local variable","局部变量")},

                {1000155,new Data(1000155,"ShowTip","Show Tip","显示提示")},

                {1000156,new Data(1000156,"SetLocalVar","Set local variable","设置局部变量")},

                {1000157,new Data(1000157,"ShowDialog","Show dialog","显示对话")},

                {1000158,new Data(1000158,"ShowEffect","Show effect","显示特效")},

                {1000159,new Data(1000159,"ShowCurrentDialog","Show current dialog","显示当前对话")},

                {1000160,new Data(1000160,"CloseCurrentDialog","Close current dialog","关闭当前对话")},

                {1000161,new Data(1000161,"SetDialogBackground","Set dialog background","设置对话背景")},

                {1000162,new Data(1000162,"SetDialogContent","Set dialog content","设置对话内容")},

                {1000163,new Data(1000163,"SetDialogAvatar","Set dialog avatar","设置对话头像")},

                {1000164,new Data(1000164,"SetDialogTitle","Set dialog title","设置对话标题")},

                {1000165,new Data(1000165,"SetDialogVideo","Set dialog video","设置对话视频")},

                {1000166,new Data(1000166,"SetDialogAudio","Set dialog audio","设置对话音频")},

                {1000167,new Data(1000167,"ResetDialog","Reset dialog","重置对话")},

                {1000168,new Data(1000168,"ShowImage","Show image","显示图片")},

                {1000169,new Data(1000169,"imageAdvanced","Image(Advanced)","图片(高级)")},

                {1000170,new Data(1000170,"CreateImage","Create image","创建图片")},

                {1000171,new Data(1000171,"DeleteImage","Delete image","删除图片")},

                {1000172,new Data(1000172,"SetImagePos","Set image position","设置图片位置")},

                {1000173,new Data(1000173,"SetImageOpacity","Set image opacity","设置图片不透明度")},

                {1000174,new Data(1000174,"SetImageRotate","Set image rotate","设置图片旋转")},

                {1000175,new Data(1000175,"process","Process","流程")},

                {1000176,new Data(1000176,"Wait","Wait","等待")},

                {1000177,new Data(1000177,"If","If","如果")},

                {1000178,new Data(1000178,"For","For","循环")},

                {1000179,new Data(1000179,"Pause","Pause","暂停")},

                {1000180,new Data(1000180,"Continue","Continue","继续")},

                {1000181,new Data(1000181,"GameOver","Game over","游戏结束")},

                {1000182,new Data(1000182,"Save","Save","保存")},

                {1000183,new Data(1000183,"Load","Load","读取")},

                {1000184,new Data(1000184,"DestroyObject","Destroy object","销毁物体")},

                {1000185,new Data(1000185,"GenerateObject","Generate object","创建物体")},

                {1000186,new Data(1000186,"MoveObject","Move object","移动物体")},

                {1000187,new Data(1000187,"GetSelfObject","Get self object","获取物体自己")},

                {1000188,new Data(1000188,"GetTriggerObject","Get trigger object","获取触发物体")},

                {1000189,new Data(1000189,"Num","Number","数字")},

                {1000190,new Data(1000190,"math","Math","数学")},

                {1000191,new Data(1000191,"Add","Add","加")},

                {1000192,new Data(1000192,"Subtract","Subtract","减")},

                {1000193,new Data(1000193,"Multiply","Multiply","乘")},

                {1000194,new Data(1000194,"Divide","Divide","除")},

                {1000195,new Data(1000195,"Greater","Greater","大于")},

                {1000196,new Data(1000196,"Less","Less","小于")},

                {1000197,new Data(1000197,"Equal","Equal","等于")},

                {1000198,new Data(1000198,"NotEqual","Not equal","不等于")},

                {1000199,new Data(1000199,"idle","idle anim","闲置动画")},

                {1000200,new Data(1000200,"idle0","idle anim with up","朝上闲置动画")},

                {1000201,new Data(1000201,"idle1","idle anim with down","朝下闲置动画")},

                {1000202,new Data(1000202,"idle2","idle anim with left","朝左闲置动画")},

                {1000203,new Data(1000203,"idle3","idle anim with right","朝右闲置动画")},

                {1000204,new Data(1000204,"move","move anim","移动动画")},

                {1000205,new Data(1000205,"move0","move anim with up","朝上移动动画")},

                {1000206,new Data(1000206,"move1","move anim with down","朝下移动动画")},

                {1000207,new Data(1000207,"move2","move anim with left","朝左移动动画")},

                {1000208,new Data(1000208,"move3","move anim with right","朝右移动动画")},

                {1000209,new Data(1000209,"Choose faceType","Choose face type","选择朝向类型")},

                {1000210,new Data(1000210,"Fixed","Fixed","固定")},

                {1000211,new Data(1000211,"FourDirection","Four direction","四向")},

                {1000212,new Data(1000212,"Flexible","Flexible","灵活")},

                {1000213,new Data(1000213,"faceType","Face type","朝向类型")},

                };
                    _DataByKey = new Dictionary<string, Data>() {
    
                        {"terrain",_DataById[1000001]},
    
                        {"texture",_DataById[1000002]},
    
                        {"transitionMask",_DataById[1000003]},
    
                        {"object",_DataById[1000004]},
    
                        {"item",_DataById[1000005]},
    
                        {"erase",_DataById[1000006]},
    
                        {"all erase",_DataById[1000007]},
    
                        {"remain terrain",_DataById[1000008]},
    
                        {"texture only",_DataById[1000009]},
    
                        {"maxYTip",_DataById[1000010]},
    
                        {"minYTip",_DataById[1000011]},
    
                        {"Choose main character",_DataById[1000012]},
    
                        {"Choose Hp param",_DataById[1000013]},
    
                        {"Choose Speed param",_DataById[1000014]},
    
                        {"Choose anim",_DataById[1000015]},
    
                        {"input value",_DataById[1000016]},
    
                        {"icon",_DataById[1000017]},
    
                        {"avatar",_DataById[1000018]},
    
                        {"overview",_DataById[1000019]},
    
                        {"parameter",_DataById[1000020]},
    
                        {"character",_DataById[1000021]},
    
                        {"mapObject",_DataById[1000022]},
    
                        {"globalParameter",_DataById[1000023]},
    
                        {"characterParameter",_DataById[1000024]},
    
                        {"itemParameter",_DataById[1000025]},
    
                        {"config",_DataById[1000026]},
    
                        {"event",_DataById[1000027]},
    
                        {"scene",_DataById[1000028]},
    
                        {"name",_DataById[1000029]},
    
                        {"introduction",_DataById[1000030]},
    
                        {"mainCharacter",_DataById[1000031]},
    
                        {"appearance",_DataById[1000032]},
    
                        {"dialog",_DataById[1000033]},
    
                        {"tips",_DataById[1000034]},
    
                        {"empty",_DataById[1000035]},
    
                        {"if",_DataById[1000036]},
    
                        {"then",_DataById[1000037]},
    
                        {"else",_DataById[1000038]},
    
                        {"content",_DataById[1000039]},
    
                        {"conditionJudge",_DataById[1000040]},
    
                        {"execute",_DataById[1000041]},
    
                        {"text",_DataById[1000042]},
    
                        {"num",_DataById[1000043]},
    
                        {"dialogClip",_DataById[1000044]},
    
                        {"value",_DataById[1000045]},
    
                        {"logic",_DataById[1000046]},
    
                        {"window",_DataById[1000047]},
    
                        {"min",_DataById[1000048]},
    
                        {"max",_DataById[1000049]},
    
                        {"hpParameter",_DataById[1000050]},
    
                        {"moveSpeedParameter",_DataById[1000051]},
    
                        {"idleAnim",_DataById[1000052]},
    
                        {"moveAnim",_DataById[1000053]},
    
                        {"interval(s)",_DataById[1000054]},
    
                        {"import",_DataById[1000055]},
    
                        {"UpperPart",_DataById[1000056]},
    
                        {"LowerPart",_DataById[1000057]},
    
                        {"equipSetting",_DataById[1000058]},
    
                        {"layer",_DataById[1000059]},
    
                        {"scale",_DataById[1000060]},
    
                        {"height",_DataById[1000061]},
    
                        {"width",_DataById[1000062]},
    
                        {"length",_DataById[1000063]},
    
                        {"model",_DataById[1000064]},
    
                        {"style",_DataById[1000065]},
    
                        {"onUseEvent",_DataById[1000066]},
    
                        {"price(coins)",_DataById[1000067]},
    
                        {"canEquip",_DataById[1000068]},
    
                        {"onEquipEvent",_DataById[1000069]},
    
                        {"onDisequipEvent",_DataById[1000070]},
    
                        {"part_1",_DataById[1000071]},
    
                        {"Cube",_DataById[1000072]},
    
                        {"Sphere",_DataById[1000073]},
    
                        {"verticalView",_DataById[1000074]},
    
                        {"leftView",_DataById[1000075]},
    
                        {"frontView",_DataById[1000076]},
    
                        {"condition",_DataById[1000077]},
    
                        {"fixed",_DataById[1000078]},
    
                        {"onCharacterTouchEvent",_DataById[1000079]},
    
                        {"onCharacterLeaveEvent",_DataById[1000080]},
    
                        {"onObjectTouchEvent",_DataById[1000081]},
    
                        {"onObjectLeaveEvent",_DataById[1000082]},
    
                        {"onShowEvent",_DataById[1000083]},
    
                        {"customEvent",_DataById[1000084]},
    
                        {"globalEvent",_DataById[1000085]},
    
                        {"category",_DataById[1000086]},
    
                        {"type",_DataById[1000087]},
    
                        {"edit",_DataById[1000088]},
    
                        {"onBeginEvent",_DataById[1000089]},
    
                        {"skillEvent",_DataById[1000090]},
    
                        {"map",_DataById[1000091]},
    
                        {"setPos",_DataById[1000092]},
    
                        {"partSetting",_DataById[1000093]},
    
                        {"enablePart",_DataById[1000094]},
    
                        {"itemStyle",_DataById[1000095]},
    
                        {"code",_DataById[1000096]},
    
                        {"entry",_DataById[1000097]},
    
                        {"chooseModel",_DataById[1000098]},
    
                        {"chooseEquipPart",_DataById[1000099]},
    
                        {"",_DataById[1000100]},
    
                        {"LeftHand",_DataById[1000101]},
    
                        {"RightHand",_DataById[1000102]},
    
                        {"Head",_DataById[1000103]},
    
                        {"Body",_DataById[1000104]},
    
                        {"label",_DataById[1000105]},
    
                        {"Choose show equipped item style",_DataById[1000106]},
    
                        {"Quad",_DataById[1000107]},
    
                        {"Capsule",_DataById[1000108]},
    
                        {"unclassified",_DataById[1000109]},
    
                        {"Please create a new one or select one from the following",_DataById[1000110]},
    
                        {"center_1",_DataById[1000111]},
    
                        {"Texture Layer Set",_DataById[1000112]},
    
                        {"view position",_DataById[1000113]},
    
                        {"tool",_DataById[1000114]},
    
                        {"rotation",_DataById[1000115]},
    
                        {"position",_DataById[1000116]},
    
                        {"rotate 90°",_DataById[1000117]},
    
                        {"align",_DataById[1000118]},
    
                        {"resetCount",_DataById[1000119]},
    
                        {"ui",_DataById[1000120]},
    
                        {"notice",_DataById[1000121]},
    
                        {"Choose command",_DataById[1000122]},
    
                        {"show type",_DataById[1000123]},
    
                        {"AlwaysWithPanel",_DataById[1000124]},
    
                        {"Always",_DataById[1000125]},
    
                        {"OnlyNotZero",_DataById[1000126]},
    
                        {"Hide",_DataById[1000127]},
    
                        {"tachie",_DataById[1000128]},
    
                        {"description",_DataById[1000129]},
    
                        {"effect",_DataById[1000130]},
    
                        {"sustain(s)",_DataById[1000131]},
    
                        {"transition",_DataById[1000132]},
    
                        {"opacity",_DataById[1000133]},
    
                        {"image",_DataById[1000134]},
    
                        {"onTriggerEvent",_DataById[1000135]},
    
                        {"triggerCondition",_DataById[1000136]},
    
                        {"Choose effect",_DataById[1000137]},
    
                        {"Choose perspective",_DataById[1000138]},
    
                        {"perspective",_DataById[1000139]},
    
                        {"Overhead",_DataById[1000140]},
    
                        {"Isometric",_DataById[1000141]},
    
                        {"dialogAdvanced",_DataById[1000142]},
    
                        {"const",_DataById[1000143]},
    
                        {"trigger",_DataById[1000144]},
    
                        {"NoLimit",_DataById[1000145]},
    
                        {"Once",_DataById[1000146]},
    
                        {"OnceDuring",_DataById[1000147]},
    
                        {"unique",_DataById[1000148]},
    
                        {"Choose trigger condition",_DataById[1000149]},
    
                        {"insert",_DataById[1000150]},
    
                        {"basic",_DataById[1000151]},
    
                        {"Text",_DataById[1000152]},
    
                        {"variable",_DataById[1000153]},
    
                        {"LocalVar",_DataById[1000154]},
    
                        {"ShowTip",_DataById[1000155]},
    
                        {"SetLocalVar",_DataById[1000156]},
    
                        {"ShowDialog",_DataById[1000157]},
    
                        {"ShowEffect",_DataById[1000158]},
    
                        {"ShowCurrentDialog",_DataById[1000159]},
    
                        {"CloseCurrentDialog",_DataById[1000160]},
    
                        {"SetDialogBackground",_DataById[1000161]},
    
                        {"SetDialogContent",_DataById[1000162]},
    
                        {"SetDialogAvatar",_DataById[1000163]},
    
                        {"SetDialogTitle",_DataById[1000164]},
    
                        {"SetDialogVideo",_DataById[1000165]},
    
                        {"SetDialogAudio",_DataById[1000166]},
    
                        {"ResetDialog",_DataById[1000167]},
    
                        {"ShowImage",_DataById[1000168]},
    
                        {"imageAdvanced",_DataById[1000169]},
    
                        {"CreateImage",_DataById[1000170]},
    
                        {"DeleteImage",_DataById[1000171]},
    
                        {"SetImagePos",_DataById[1000172]},
    
                        {"SetImageOpacity",_DataById[1000173]},
    
                        {"SetImageRotate",_DataById[1000174]},
    
                        {"process",_DataById[1000175]},
    
                        {"Wait",_DataById[1000176]},
    
                        {"If",_DataById[1000177]},
    
                        {"For",_DataById[1000178]},
    
                        {"Pause",_DataById[1000179]},
    
                        {"Continue",_DataById[1000180]},
    
                        {"GameOver",_DataById[1000181]},
    
                        {"Save",_DataById[1000182]},
    
                        {"Load",_DataById[1000183]},
    
                        {"DestroyObject",_DataById[1000184]},
    
                        {"GenerateObject",_DataById[1000185]},
    
                        {"MoveObject",_DataById[1000186]},
    
                        {"GetSelfObject",_DataById[1000187]},
    
                        {"GetTriggerObject",_DataById[1000188]},
    
                        {"Num",_DataById[1000189]},
    
                        {"math",_DataById[1000190]},
    
                        {"Add",_DataById[1000191]},
    
                        {"Subtract",_DataById[1000192]},
    
                        {"Multiply",_DataById[1000193]},
    
                        {"Divide",_DataById[1000194]},
    
                        {"Greater",_DataById[1000195]},
    
                        {"Less",_DataById[1000196]},
    
                        {"Equal",_DataById[1000197]},
    
                        {"NotEqual",_DataById[1000198]},
    
                        {"idle",_DataById[1000199]},
    
                        {"idle0",_DataById[1000200]},
    
                        {"idle1",_DataById[1000201]},
    
                        {"idle2",_DataById[1000202]},
    
                        {"idle3",_DataById[1000203]},
    
                        {"move",_DataById[1000204]},
    
                        {"move0",_DataById[1000205]},
    
                        {"move1",_DataById[1000206]},
    
                        {"move2",_DataById[1000207]},
    
                        {"move3",_DataById[1000208]},
    
                        {"Choose faceType",_DataById[1000209]},
    
                        {"Fixed",_DataById[1000210]},
    
                        {"FourDirection",_DataById[1000211]},
    
                        {"Flexible",_DataById[1000212]},
    
                        {"faceType",_DataById[1000213]},
    
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
            addAction?.Invoke(data);
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
            removeAction?.Invoke(data);
        }
        public static void Clear()
        {
            Init();
            var keys = new List<int>(DataById.Keys);
            foreach(var key in keys)
            {
                    RemoveData(key);
            }

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
        