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

    public static partial class CommonTextForm
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
        
        public static Action<Data> beforeGetAction;

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
            public void Reset(Data data)
            {

             this.id = data.id;
             this.key = data.key;
             this.contentEn = data.contentEn;
             this.contentCn = data.contentCn;
            }

                public Data Copy(bool sameId = true)
                {
        return new Data(sameId? id:idChain.GetId(),key,contentEn,contentCn);
                }
            
            public override  void BeforeGet()
            {
                base.BeforeGet();
                CommonTextForm.beforeGetAction?.Invoke(this);
            }
        }

                   private static Data _defaultData=new Data(0,"","","");
                   public static Data defaultData=>_defaultData.Copy();


            static HashSet<Data> _DatasHashSet;
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

                {1,new Data(1,"yes","Yes","是")},

                {2,new Data(2,"no","No","否")},

                {3,new Data(3,"new","New","新")},

                {4,new Data(4,"play_1","Play","播放")},

                {5,new Data(5,"stop","Stop","停止")},

                {6,new Data(6,"custom","Costom","自定义")},

                {7,new Data(7,"gain","Gain","获得")},

                {8,new Data(8,"lost","Lost","失去")},

                {10001,new Data(10001,"savePopupTitle","Do you need Save?","需要保存吗?")},

                {20001,new Data(20001,"play_2","Start","开始游戏")},

                {20002,new Data(20002,"workshop","Workshop","创意工坊")},

                {20003,new Data(20003,"lounge","Lounge","休息室")},

                {20004,new Data(20004,"setting","Settings","设置")},

                {20005,new Data(20005,"exit","Exit","退出")},

                {20006,new Data(20006,"news","News","资讯")},

                {20007,new Data(20007,"back","Back","返回")},

                {20008,new Data(20008,"backpack","Backpack","背包")},

                {20009,new Data(20009,"use","Use","使用")},

                {20010,new Data(20010,"drop","Drop","丢弃")},

                {20011,new Data(20011,"equip","Equip","装备")},

                {20012,new Data(20012,"character","Character","角色")},

                {20013,new Data(20013,"basicInformation","Basic Information","基础信息")},

                {20014,new Data(20014,"all","All","全部")},

                {20015,new Data(20015,"label","Label","标签")},

                {20016,new Data(20016,"leftHand","Left hand","左手")},

                {20017,new Data(20017,"rightHand","Right hand","右手")},

                {20018,new Data(20018,"head","Head","头")},

                {20019,new Data(20019,"body","Body","身体")},

                {20020,new Data(20020,"reset","Reset","重置")},

                {20021,new Data(20021,"apply","Apply","应用")},

                {20022,new Data(20022,"delete","Delete","删除")},

                {20023,new Data(20023,"map","Map","地图")},

                {20024,new Data(20024,"menu","Menu","菜单")},

                {20025,new Data(20025,"count","Count","数量")},

                {20026,new Data(20026,"save","Save","保存")},

                {20027,new Data(20027,"save success","Save success!","保存成功")},

                {20028,new Data(20028,"input value","Input value","输入值")},

                {20029,new Data(20029,"auto play","Auto play","自动播放")},

                {20030,new Data(20030,"skip","Skip","跳过")},

                {20031,new Data(20031,"hide","Hide","隐藏")},

                {20032,new Data(20032,"history","History","历史记录")},

                {20033,new Data(20033,"data","Data","数据")},

                {20034,new Data(20034,"skill","Skill","技能")},

                {20035,new Data(20035,"Please select one from the following","Please select one from the following","请从下方选择一项")},

                {20036,new Data(20036,"name","Name","名称")},

                {20037,new Data(20037,"cd(s)","CD(s)","冷却(秒)")},

                {20038,new Data(20038,"LightAttack","Light attack","轻击")},

                {20039,new Data(20039,"HeavyAttack","Heavy attack","重击")},

                {20040,new Data(20040,"E","E button","E技能")},

                {20041,new Data(20041,"Q","Q button","Q技能")},

                {20042,new Data(20042,"Passive","Passive","被动技能")},

                {20043,new Data(20043,"unequip","Unequip","卸下")},

                {20044,new Data(20044,"Equip character","Equip character","装备角色")},

                {20045,new Data(20045,"Equipment","Equipment","装备")},

                };
                _DatasHashSet=new HashSet<Data>();
                
                    _DataByKey = new Dictionary<string, Data>() {
    
                        {"yes",_DataById[1]},
    
                        {"no",_DataById[2]},
    
                        {"new",_DataById[3]},
    
                        {"play_1",_DataById[4]},
    
                        {"stop",_DataById[5]},
    
                        {"custom",_DataById[6]},
    
                        {"gain",_DataById[7]},
    
                        {"lost",_DataById[8]},
    
                        {"savePopupTitle",_DataById[10001]},
    
                        {"play_2",_DataById[20001]},
    
                        {"workshop",_DataById[20002]},
    
                        {"lounge",_DataById[20003]},
    
                        {"setting",_DataById[20004]},
    
                        {"exit",_DataById[20005]},
    
                        {"news",_DataById[20006]},
    
                        {"back",_DataById[20007]},
    
                        {"backpack",_DataById[20008]},
    
                        {"use",_DataById[20009]},
    
                        {"drop",_DataById[20010]},
    
                        {"equip",_DataById[20011]},
    
                        {"character",_DataById[20012]},
    
                        {"basicInformation",_DataById[20013]},
    
                        {"all",_DataById[20014]},
    
                        {"label",_DataById[20015]},
    
                        {"leftHand",_DataById[20016]},
    
                        {"rightHand",_DataById[20017]},
    
                        {"head",_DataById[20018]},
    
                        {"body",_DataById[20019]},
    
                        {"reset",_DataById[20020]},
    
                        {"apply",_DataById[20021]},
    
                        {"delete",_DataById[20022]},
    
                        {"map",_DataById[20023]},
    
                        {"menu",_DataById[20024]},
    
                        {"count",_DataById[20025]},
    
                        {"save",_DataById[20026]},
    
                        {"save success",_DataById[20027]},
    
                        {"input value",_DataById[20028]},
    
                        {"auto play",_DataById[20029]},
    
                        {"skip",_DataById[20030]},
    
                        {"hide",_DataById[20031]},
    
                        {"history",_DataById[20032]},
    
                        {"data",_DataById[20033]},
    
                        {"skill",_DataById[20034]},
    
                        {"Please select one from the following",_DataById[20035]},
    
                        {"name",_DataById[20036]},
    
                        {"cd(s)",_DataById[20037]},
    
                        {"LightAttack",_DataById[20038]},
    
                        {"HeavyAttack",_DataById[20039]},
    
                        {"E",_DataById[20040]},
    
                        {"Q",_DataById[20041]},
    
                        {"Passive",_DataById[20042]},
    
                        {"unequip",_DataById[20043]},
    
                        {"Equip character",_DataById[20044]},
    
                        {"Equipment",_DataById[20045]},
    
                    
                    };
                    foreach(var v in _DataById.Values)
                    {
                        _DatasHashSet.Add(v);
                    }
    

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

                jo.SelectToken("id")==null?defaultData.id:jo.Get<int>("id"),

                jo.SelectToken("key")==null?defaultData.key:jo.Get<string>("key"),

                jo.SelectToken("contentEn")==null?defaultData.contentEn:jo.Get<string>("contentEn"),

                jo.SelectToken("contentCn")==null?defaultData.contentCn:jo.Get<string>("contentCn")
                    );

            return data;
        }

        public static JObject GetJoByData(Data data)
        {
            Init();
            data.BeforeGet();

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
        _DatasHashSet.Add(data);
    
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

                    _DatasHashSet.Remove(DataById[data.id]);
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
                    {
                        RemoveData(key);
                    }
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
        