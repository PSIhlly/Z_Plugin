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
            
        }
        
        private static bool inited;
        public static Z_Chain.Chain idChain=>TextBaseForm.idChain;
        public static Action childInitAction;
        public static Action<Data> childRemoveAction;
        public static Action<Data> childAddAction;

        public partial class Data : TextBaseForm.Data
        {

            public Data(int id,string key,string contentEn,string contentCn):base(id,key,contentEn,contentCn)
            {

                this.id = id;
                this.key = key;
                this.contentEn = contentEn;
                this.contentCn = contentCn;

            }
            
        }

                   public static Data defaultData=new Data(0,"","","");


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

                {10001,new Data(10001,"savePopupTitle","Do you need Save?","需要保存吗?")},

                };

                _DataByKey = new Dictionary<string, Data>() {

                    {"yes",_DataById[1]},

                    {"no",_DataById[2]},

                    {"savePopupTitle",_DataById[10001]},

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
            childRemoveAction?.Invoke(data);
        }
        public static void Clear()
        {
            Init();

                DataById.Clear();

                DataByKey.Clear();

            idChain.Clear();
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
        


    }
}
        