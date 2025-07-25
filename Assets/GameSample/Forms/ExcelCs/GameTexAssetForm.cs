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

    public static partial class GameTexAssetForm
    {

        

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Register()
        {

                TexAssetForm.childInitAction+=InitInternal;


                TexAssetForm.childRemoveAction+=RemoveChildren;
                TexAssetForm.childAddAction+=AddChildren;
            

            TexAssetForm.changeIdAction+=ChangeId;

            TexAssetForm.changeNameAction+=ChangeName;

            TexAssetForm.changeTexAction+=ChangeTex;

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

        public static Action childInitAction;
        public static Action<Data> childRemoveAction;
        public static Action<Data> childAddAction;

        public static Action<Data,int,int> changeIdAction;
                
        public static Action<Data,string,string> changeNameAction;
                
        public static Action<Data,Texture,Texture> changeTexAction;
                


        public partial class Data : TexAssetForm.Data
        {

            public Data(int id,string name,Texture tex):base(id,name,tex)
            {

             this.id = id;
             this.name = name;
             this.tex = tex;

            }

                public Data Copy(bool sameId = true)
                {
        return new Data(sameId? id:idChain.GetId(),name,tex);
                }
            
        }

                   private static Data _defaultData=new Data(0,"",Texture2D.blackTexture);
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
    
            static Dictionary<Texture, List<Data>> _DatasByTex;
            public static Dictionary<Texture, List<Data>> DatasByTex
            {
                get
                {
                    Init();
                    return _DatasByTex;
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
    
                    _DatasByTex = new Dictionary<Texture, List<Data>>() {
    
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

                    _defaultData.tex
                    );

            return data;
        }

        public static JObject GetJoByData(Data data)
        {
            Init();

            JObject jo=new JObject();

            jo.Set<int>("id",data.id);

            jo.Set<string>("name",data.name);

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
    
                    if(!DatasByTex.ContainsKey(data.tex))
                        DatasByTex[data.tex]=new List<Data>();
                    DatasByTex[data.tex].Add(data);
    
TexAssetForm.AddData(data);
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
    
                    DataByName.Remove(data.name);
    
                    DatasByTex[data.tex].Remove(data);
                    if(DatasByTex[data.tex].Count==0)
                        DatasByTex.Remove(data.tex);
    
TexAssetForm.RemoveData(id);
            idChain.PushId(data.id);
            childRemoveAction?.Invoke(data);
        }
        public static void Clear()
        {
            Init();

                    DataById.Clear();
    
                    DataByName.Clear();
    
                    DatasByTex.Clear();
    
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
            
            public static void ChangeTex(TexAssetForm.Data superData,Texture oldV,Texture newV)
            {
                if(superData is Data data)
                {

                    DatasByTex[oldV].Remove(data);
                    if(DatasByTex[oldV].Count==0)
                        DatasByTex.Remove(oldV);
                    if(!DatasByTex.ContainsKey(newV))
                        DatasByTex[newV]=new List<Data>();
                    DatasByTex[newV].Add(data);
 
                changeTexAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
    }
}
        