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

                {1,new Data(1,"yes","Yes","是")},

                {2,new Data(2,"no","No","否")},

                {3,new Data(3,"new","New","新")},

                {4,new Data(4,"play_1","Play","播放")},

                {5,new Data(5,"stop","Stop","停止")},

                {6,new Data(6,"custom","Costom","自定义")},

                {7,new Data(7,"gain","Gain","获得")},

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

                };
                    _DataByKey = new Dictionary<string, Data>() {
    
                        {"yes",_DataById[1]},
    
                        {"no",_DataById[2]},
    
                        {"new",_DataById[3]},
    
                        {"play_1",_DataById[4]},
    
                        {"stop",_DataById[5]},
    
                        {"custom",_DataById[6]},
    
                        {"gain",_DataById[7]},
    
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
        