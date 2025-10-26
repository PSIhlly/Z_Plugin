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

    public static partial class StoryTexAssetForm
    {

        

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Register()
        {

                TexAssetForm.childInitAction+=InitInternal;


                TexAssetForm.childRemoveAction+=RemoveChildren;
                TexAssetForm.childAddAction+=AddChildren;
            

            TexAssetForm.changeIdAction+=ChangeId;

            TexAssetForm.changeNameAction+=ChangeName;

            TexAssetForm.changePathAction+=ChangePath;

            TexAssetForm.changeForcetexAction+=ChangeForcetex;

            Z_Json.extra[typeof(Data)]=((obj)=>{
            if(obj is Data data)
                return GetJoByData(data);
            return null;
            },(jo)=>{
            return GetDataByJo(jo);
            });
        }
        
        private static bool inited;

        public static Z_Chain.Chain idChain =>TexAssetForm.idChain;

        public static Action<Data> addAction;
        public static Action<Data> removeAction;
        public static Action childInitAction;
        public static Action<Data> childRemoveAction;
        public static Action<Data> childAddAction;

        public static Action<Data,int,int> changeIdAction;
                
        public static Action<Data,string,string> changeNameAction;
                
        public static Action<Data,string,string> changePathAction;
                
        public static Action<Data,Texture,Texture> changeForcetexAction;
                


        public partial class Data : TexAssetForm.Data
        {

            public Data(int id,string name,string path,Texture forceTex):base(id,name,path,forceTex)
            {

             this.id = id;
             this.name = name;
             this.path = path;
             this.forceTex = forceTex;

            }

                public Data Copy(bool sameId = true)
                {
        return new Data(sameId? id:idChain.GetId(),name,path,forceTex);
                }
            
        }

                   private static Data _defaultData=new Data(0,"","",Texture2D.whiteTexture);
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

            TexAssetForm.Init();

        }
        public static void InitInternal()
        {
            if(inited)
                return;
            inited=true;  

        

                _DataById = new Dictionary<int, Data>() {

                };
                    _DataByName = new Dictionary<string, Data>() {
    
                    };
    
                    _DatasByPath = new Dictionary<string, List<Data>>() {
    
                };


            childInitAction?.Invoke();
            

            foreach(var data in DataById.Values)
            {
                TexAssetForm.AddData(data);
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

                jo.Get<string>("name"),

                    _defaultData.path,

                jo.Get<Texture>("forceTex")
                    );

            return data;
        }

        public static JObject GetJoByData(Data data)
        {
            Init();

            JObject jo=new JObject();

            jo.Set<int>("id",data.id);

            jo.Set<string>("name",data.name);

            jo.Set<Texture>("forceTex",data.forceTex);

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
    
                    DataByName[data.name]=data;
    
                    if(!DatasByPath.ContainsKey(data.path))
                        DatasByPath[data.path]=new List<Data>();
                    DatasByPath[data.path].Add(data);
    
TexAssetForm.AddData(data);
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
    
                    DataByName.Remove(data.name);
    
                    DatasByPath[data.path].Remove(data);
                    if(DatasByPath[data.path].Count==0)
                        DatasByPath.Remove(data.path);
    
TexAssetForm.RemoveData(id);
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

         private static void RemoveChildren(TexAssetForm.Data data)
        {
            Init();
            if(data is Data)
               RemoveData(data.id);      
        }
         private static void AddChildren(TexAssetForm.Data superData)
        {
            Init();
            if(superData is Data data)
               AddData(data);      
        }
        




            public static void ChangeId(TexAssetForm.Data superData,int oldV,int newV)
            {
                if(superData is Data data)
                {

                changeIdAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeName(TexAssetForm.Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                    DataByName.Remove(oldV);
                    DataByName[newV]=data;
 
                changeNameAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangePath(TexAssetForm.Data superData,string oldV,string newV)
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
            
            public static void ChangeForcetex(TexAssetForm.Data superData,Texture oldV,Texture newV)
            {
                if(superData is Data data)
                {

                changeForcetexAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
    }
}
        