using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Z_ByteSerialize;
using Z_DesignStyle;

namespace Z_DataSystem.Form
{

    public static partial class AssetForm
    {
public static readonly int autoIdCnt=10000;

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

        public static Action<Data,int,int> changeIdAction;
                
        public static Action<Data,string,string> changeNameAction;
                
        public static Action<Data,string,string> changePathAction;
                
        public static Action<Data,byte[],byte[]> changeBytesAction;
                
        public static Action<Data,string,string> changeHashAction;
                
        public static Action<Data,object,object> changeAssetAction;
                


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
                    ///名称（索引）
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
                    
                    private string  _path;
                    /// <summary>
                    ///路径
                    ///</summary>
                    public string  path{
                                get{return _path;}
 set{

                    if(_DataById!=null&&_DataById.ContainsValue(this))
                    {
                       ChangePath(this,_path,value); 
                    }
        
                _path = value;
                }
                 
                     }
                    
                    private byte[]  _bytes;
                    /// <summary>
                    ///二进制文件
                    ///</summary>
                    public byte[]  bytes{
                                get{return _bytes;}
 set{

                    if(_DataById!=null&&_DataById.ContainsValue(this))
                    {
                       ChangeBytes(this,_bytes,value); 
                    }
        
                _bytes = value;
                }
                 
                     }
                    
                    private string  _hash;
                    /// <summary>
                    ///哈希
                    ///</summary>
                    public string  hash{
                                get{return _hash;}
 set{

                    if(_DataById!=null&&_DataById.ContainsValue(this))
                    {
                       ChangeHash(this,_hash,value); 
                    }
        
                _hash = value;
                }
                 
                     }
                    
                    private object  _asset;
                    /// <summary>
                    ///运行时资源
                    ///</summary>
                    public object  asset{
                                get{return _asset;}
 set{

                    if(_DataById!=null&&_DataById.ContainsValue(this))
                    {
                       ChangeAsset(this,_asset,value); 
                    }
        
                _asset = value;
                }
                 
                     }
                    
            public Data(int id,string name,string path,byte[] bytes,string hash,object asset)
            {

             this.id = id;
             this.name = name;
             this.path = path;
             this.bytes = bytes;
             this.hash = hash;
             this.asset = asset;

            }
            public void Reset(Data data)
            {

             this.id = data.id;
             this.name = data.name;
             this.path = data.path;
             this.bytes = data.bytes;
             this.hash = data.hash;
             this.asset = data.asset;
            }

                public Data Copy(bool sameId = true)
                {
        return new Data(sameId? id:idChain.GetId(),name,path,bytes,hash,asset);
                }
            
        }

                   private static Data _defaultData=new Data(0,"","",null,"",null);
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
    
            static Dictionary<string, List<Data>> _DatasByPath;
            public static Dictionary<string, List<Data>> DatasByPath
            {
                get
                {
                    Init();
                    return _DatasByPath;
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
                    _DatasByPath = new Dictionary<string, List<Data>>() {
    
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

                jo.Get<string>("path"),

                jo.Get<byte[]>("bytes"),

                jo.Get<string>("hash"),

                    _defaultData.asset
                    );

            return data;
        }

        public static JObject GetJoByData(Data data)
        {
            Init();

            JObject jo=new JObject();

            jo.Set<int>("id",data.id);

            jo.Set<string>("name",data.name);

            jo.Set<string>("path",data.path);

            jo.Set<byte[]>("bytes",data.bytes);

            jo.Set<string>("hash",data.hash);

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
    
                    if(!DatasByPath.ContainsKey(data.path))
                        DatasByPath[data.path]=new List<Data>();
                    DatasByPath[data.path].Add(data);
    

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
    
                    DatasByPath[data.path].Remove(data);
                    if(DatasByPath[data.path].Count==0)
                        DatasByPath.Remove(data.path);
    

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
            
            public static void ChangePath(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                    DatasByPath[oldV].Remove(data);
                    if(DatasByPath[oldV].Count==0)
                        DatasByPath.Remove(oldV);
                    if(!DatasByPath.ContainsKey(newV))
                        DatasByPath[newV]=new List<Data>();
                    DatasByPath[newV].Add(data);
 
                changePathAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeBytes(Data superData,byte[] oldV,byte[] newV)
            {
                if(superData is Data data)
                {

                changeBytesAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeHash(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeHashAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeAsset(Data superData,object oldV,object newV)
            {
                if(superData is Data data)
                {

                changeAssetAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
    }
}
        