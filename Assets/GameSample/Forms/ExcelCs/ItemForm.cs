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

    public static partial class ItemForm
    {
public static readonly int autoIdCnt=100;

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

        public static Z_Chain.Chain idChain ;

        public static Action<Data> addAction;
        public static Action<Data> removeAction;
        public static Action childInitAction;
        public static Action<Data> childRemoveAction;
        public static Action<Data> childAddAction;

        public static Action<Data,int,int> changeCountAction;
                


        public partial class Data
        {

                    private int  _id;
                    /// <summary>
                    ///
                    ///</summary>
                    public int  id{
                                get{return _id;}
private set{
        
                _id = value;
                }
                 
                     }
                    
                    private string  _name;
                    /// <summary>
                    ///名字
                    ///</summary>
                    public string  name{
                                get{return _name;}
private set{
        
                _name = value;
                }
                 
                     }
                    
                    private string  _icon;
                    /// <summary>
                    ///图标
                    ///</summary>
                    public string  icon{
                                get{return _icon;}
private set{
        
                _icon = value;
                }
                 
                     }
                    
                    private int  _count;
                    /// <summary>
                    ///拥有数
                    ///</summary>
                    public int  count{
                                get{return _count;}
 set{

                    if(_DataById!=null&&_DataById.ContainsValue(this))
                    {
                       ChangeCount(this,_count,value); 
                    }
        
                _count = value;
                }
                 
                     }
                    
            public Data(int id,string name,string icon,int count)
            {

             this.id = id;
             this.name = name;
             this.icon = icon;
             this.count = count;

            }
            public void Reset(Data data)
            {

             this.id = data.id;
             this.name = data.name;
             this.icon = data.icon;
             this.count = data.count;
            }

                public Data Copy(bool sameId = true)
                {
        return new Data(sameId? id:idChain.GetId(),name,icon,count);
                }
            
        }

                   private static Data _defaultData=new Data(0,"","",0);
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
    

        static public void Init()
        {

            InitInternal();
        }
        public static void InitInternal()
        {
            if(inited)
                return;
            inited=true;  
idChain=new Z_Chain.Chain (autoIdCnt);

                _DataById = new Dictionary<int, Data>() {

                {1,new Data(1,"Space-Time Fragment","\\GameSample\\Imgs\\Item\\ST fragment.png",500)},

                {2,new Data(2,"Soul power","\\GameSample\\Imgs\\Item\\Soul Power.png",0)},

                };

            childInitAction?.Invoke();
            

foreach(var k in _DataById.Keys){ idChain.PopId(k); }
             
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

                    _defaultData.name,

                    _defaultData.icon,

                jo.Get<int>("count")
                    );

            return data;
        }

        public static JObject GetJoByData(Data data)
        {
            Init();

            JObject jo=new JObject();

            jo.Set<int>("id",data.id);

            jo.Set<int>("count",data.count);

            return jo;
        }




            public static void ChangeCount(Data superData,int oldV,int newV)
            {
                if(superData is Data data)
                {

                changeCountAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
    }
}
        