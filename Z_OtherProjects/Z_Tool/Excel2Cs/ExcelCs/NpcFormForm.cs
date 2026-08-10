using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Z_ByteSerialize;
using Z_DesignStyle;

namespace Form
{

    public static partial class NpcFormForm
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
        
        public static Action<Data> beforeGetAction;



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
                    ///名称
                    ///</summary>
                    public string  name{
                                get{return _name;}
private set{
        
                _name = value;
                }
                 
                     }
                    
                    private int  _avatar_imgId;
                    /// <summary>
                    ///头像图片Id
                    ///</summary>
                    public int  avatar_imgId{
                                get{return _avatar_imgId;}
private set{
        
                _avatar_imgId = value;
                }
                 
                     }
                    
            public Data(int id,string name,int avatar_imgId)
            {

             this.id = id;
             this.name = name;
             this.avatar_imgId = avatar_imgId;

            }
            public void Reset(Data data)
            {

             this.id = data.id;
             this.name = data.name;
             this.avatar_imgId = data.avatar_imgId;
            }

                public Data Copy(bool sameId = true)
                {
        return new Data(sameId? id:idChain.GetId(),name,avatar_imgId);
                }
            
            public virtual  void BeforeGet()
            {
                
                NpcFormForm.beforeGetAction?.Invoke(this);
            }
        }

                   private static Data _defaultData=new Data(0,"",0);
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
    
            static Dictionary<string, List<Data>> _DatasByName;
            public static Dictionary<string, List<Data>> DatasByName
            {
                get
                {
                    Init();
                    return _DatasByName;
                }
            }
    
            static Dictionary<int, Data> _DataByAvatar_imgid;
            public static Dictionary<int, Data> DataByAvatar_imgid
            {
                get
                {
                    Init();
                    return _DataByAvatar_imgid;
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

                {1,new Data(1,"human",100001)},

                {2,new Data(2,"pig",100002)},

                {3,new Data(3,"dog",100003)},

                {4,new Data(4,"chicken",100004)},

                };
                _DatasHashSet=new HashSet<Data>();
                
                    _DataByAvatar_imgid = new Dictionary<int, Data>() {
    
                        {100001,_DataById[1]},
    
                        {100002,_DataById[2]},
    
                        {100003,_DataById[3]},
    
                        {100004,_DataById[4]},
    
                    
                    };
                    foreach(var v in _DataById.Values)
                    {
                        _DatasHashSet.Add(v);
                    }
    
                    _DatasByName = new Dictionary<string, List<Data>>() {
    
                            {"human",new List<Data>()},
        
                            {"pig",new List<Data>()},
        
                            {"dog",new List<Data>()},
        
                            {"chicken",new List<Data>()},
        
                };

                    _DatasByName["human"].Add(_DataById[1]);

                    _DatasByName["pig"].Add(_DataById[2]);

                    _DatasByName["dog"].Add(_DataById[3]);

                    _DatasByName["chicken"].Add(_DataById[4]);


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

                jo.SelectToken("id")==null?defaultData.id:jo.Get<int>("id"),

                    _defaultData.name,

                    _defaultData.avatar_imgId
                    );

            return data;
        }

        public static JObject GetJoByData(Data data)
        {
            Init();
            data.BeforeGet();

            JObject jo=new JObject();

            jo.Set<int>("id",data.id);

            return jo;
        }




    }
}
        