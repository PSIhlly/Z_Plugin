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

    public static partial class SceneForm
    {
        public static readonly int autoIdCnt=100;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Register()
        {


        }
        
        private static bool inited;
        public static Z_Chain.Chain idChain;
        public static Action childInitAction;
        public static Action<Data> childRemoveAction;

        public partial class Data
        {

                public int id;

                /// <summary>
                ///Ãû×Ö
                ///</summary>
                public string name;

                /// <summary>
                ///·âÃæ
                ///</summary>
                public string icon;

                /// <summary>
                ///ÄÚÈÝ
                ///</summary>
                public string content;

            public Data(int id,string name,string icon,string content)
            {

                this.id = id;
                this.name = name;
                this.icon = icon;
                this.content = content;

            }
            
        }

                   public static Data defaultData=new Data(0,"","","");


        static Dictionary<int, Data> _DataById = null;
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

                jo.Get<string>("icon"),

                jo.Get<string>("content")
                    );

            return data;
        }

        public static JObject GetJoByData(Data data)
        {
            Init();

            JObject jo=new JObject();

            jo.Set<int>("id",data.id);

            jo.Set<string>("name",data.name);

            jo.Set<string>("icon",data.icon);

            jo.Set<string>("content",data.content);

            return jo;
        }


        public static int AddData(Data data)
        {
            Init();
            if(data.id==-1)
            { 
                int id=idChain.GetId();
                if(id==-1)
                    return -1;
                data.id=id;  
            }

                _DataById[data.id]=data;

            

            return data.id;
        }
        public static void RemoveData(int id)
        {            
            Init();
            if(!_DataById.ContainsKey(id))
                return;
                
            var data=_DataById[id];

                _DataById.Remove(data.id);


            childRemoveAction?.Invoke(data);
        }
        public static void Clear()
        {
            Init();

                _DataById.Clear();

            idChain.Clear();
        }

         private static void RemoveChildren(Data data)
        {
            Init();
            if(data is Data)
               RemoveData(data.id);      
        }


    }
}
        