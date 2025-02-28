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

    public static partial class TexAssetForm
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
        public static Action<Data> childAddAction;

        public partial class Data
        {

                private int _id;

                public int id{
                            get{return _id;}
                             set{
                            
                            _id = value;
                            }
                        }

                private string _name;

                /// <summary>
                ///Ãû³Æ£¨Ë÷Òý£©
                ///</summary>
                public string name{
                            get{return _name;}
                             set{
                            if(_DataById!=null&&_DataById.ContainsValue(this)){RemoveData(id); _name = value;AddData(this);}else
                            _name = value;
                            }
                        }

                private Texture _tex;

                public Texture tex{
                            get{return _tex;}
                             set{
                            
                            _tex = value;
                            }
                        }

            public Data(int id,string name,Texture tex)
            {

                this.id = id;
                this.name = name;
                this.tex = tex;

            }
            
        }

                   public static Data defaultData=new Data(0,"",Texture2D.blackTexture);


        static Dictionary<int, Data> _DataById;
        public static Dictionary<int, Data> DataById
        {
            get
            {
                Init();
                return _DataById;
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

                _DataByName = new Dictionary<string, Data>() {

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

                jo.Get<Texture>("tex")
                    );

            return data;
        }

        public static JObject GetJoByData(Data data)
        {
            Init();

            JObject jo=new JObject();

            jo.Set<int>("id",data.id);

            jo.Set<string>("name",data.name);

            jo.Set<Texture>("tex",data.tex);

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

                DataByName[data.name]=data;

            

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


            childRemoveAction?.Invoke(data);
        }
        public static void Clear()
        {
            Init();

                DataById.Clear();

                DataByName.Clear();

            idChain.Clear();
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
        


    }
}
        