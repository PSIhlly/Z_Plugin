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

    public static partial class SceneForm
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

        public static Action childInitAction;
        public static Action<Data> childRemoveAction;
        public static Action<Data> childAddAction;

        public static Action<Data,int,int> changeIdAction;
                
        public static Action<Data,string,string> changeNameAction;
                
        public static Action<Data,string,string> changeMapAction;
                
        public static Action<Data,string,string> changeContentAction;
                
        public static Action<Data,(float,float),(float,float)> changePosAction;
                


        public partial class Data
        {

                    private int  _id;
                    /// <summary>
                    ///
                    ///</summary>
                    public int  id{
                                get{return _id;}
 set{

                    if(_DataById!=null&&_DataById.ContainsValue(this))
                    {
                       ChangeId(this,_id,value); 
                    }
        
                _id = value;
                }
                 
                     }
                    
                    private string  _name;
                    /// <summary>
                    ///名字
                    ///</summary>
                    public string  name{
                                get{return _name;}
 set{

                    if(_DataById!=null&&_DataById.ContainsValue(this))
                    {
                       ChangeName(this,_name,value); 
                    }
        
                _name = value;
                }
                 
                     }
                    
                    private string  _map;
                    /// <summary>
                    ///地图
                    ///</summary>
                    public string  map{
                                get{return _map;}
 set{

                    if(_DataById!=null&&_DataById.ContainsValue(this))
                    {
                       ChangeMap(this,_map,value); 
                    }
        
                _map = value;
                }
                 
                     }
                    
                    private string  _content;
                    /// <summary>
                    ///内容
                    ///</summary>
                    public string  content{
                                get{return _content;}
 set{

                    if(_DataById!=null&&_DataById.ContainsValue(this))
                    {
                       ChangeContent(this,_content,value); 
                    }
        
                _content = value;
                }
                 
                     }
                    
                    private (float,float)  _pos;
                    /// <summary>
                    ///相对位置
                    ///</summary>
                    public (float,float)  pos{
                                get{return _pos;}
 set{

                    if(_DataById!=null&&_DataById.ContainsValue(this))
                    {
                       ChangePos(this,_pos,value); 
                    }
        
                _pos = value;
                }
                 
                     }
                    
            public Data(int id,string name,string map,string content,(float,float) pos)
            {

             this.id = id;
             this.name = name;
             this.map = map;
             this.content = content;
             this.pos = pos;

            }

                public Data Copy(bool sameId = true)
                {
        return new Data(sameId? id:idChain.GetId(),name,map,content,pos);
                }
            
        }

                   private static Data _defaultData=new Data(0,"","","",(0f,0f));
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

                jo.Get<string>("name"),

                jo.Get<string>("map"),

                jo.Get<string>("content"),

                jo.Get<(float,float)>("pos")
                    );

            return data;
        }

        public static JObject GetJoByData(Data data)
        {
            Init();

            JObject jo=new JObject();

            jo.Set<int>("id",data.id);

            jo.Set<string>("name",data.name);

            jo.Set<string>("map",data.map);

            jo.Set<string>("content",data.content);

            jo.Set<(float,float)>("pos",data.pos);

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
    

            idChain.PushId(data.id);
            childRemoveAction?.Invoke(data);
        }
        public static void Clear()
        {
            Init();

                    DataById.Clear();
    
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

         private static void RemoveChildren(Data data)
        {
            Init();
            if(data is Data)
               RemoveData(data.id);      
        }
         private static void AddChildren(Data superData)
        {
            Init();
            if(superData is Data data)
               AddData(data);      
        }
        




            public static void ChangeId(Data superData,int oldV,int newV)
            {
                if(superData is Data data)
                {

                changeIdAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeName(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeNameAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeMap(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeMapAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeContent(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeContentAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangePos(Data superData,(float,float) oldV,(float,float) newV)
            {
                if(superData is Data data)
                {

                changePosAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
    }
}
        