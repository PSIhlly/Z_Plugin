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

    public static partial class MapObstacleForm
    {
        

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Register()
        {

                MapBaseForm.childInitAction+=InitInternal;


                MapBaseForm.childRemoveAction+=RemoveChildren;
                MapBaseForm.childAddAction+=AddChildren;
            
        }
        
        private static bool inited;
        public static Z_Chain.Chain idChain=>MapBaseForm.idChain;
        public static Action childInitAction;
        public static Action<Data> childRemoveAction;
        public static Action<Data> childAddAction;

        public partial class Data : MapBaseForm.Data
        {

                private string _prefabName;

                /// <summary>
                ///Ô¤ÖÆÃû³Æ
                ///</summary>
                public string prefabName{
                            get{return _prefabName;}
                             set{
                            
                            _prefabName = value;
                            }
                        }

            public Data(int id,string name,string prefabName,string icon):base(id,name,icon)
            {

                this.id = id;
                this.name = name;
                this.prefabName = prefabName;
                this.icon = icon;

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

            MapBaseForm.Init();

        }
        public static void InitInternal()
        {
            if(inited)
                return;
            inited=true;  
            
            

                _DataById = new Dictionary<int, Data>() {

                {400001,new Data(400001,"wall","wall1","")},

                };

                _DataByName = new Dictionary<string, Data>() {

                    {"wall",_DataById[400001]},

                };


            childInitAction?.Invoke();
            

            foreach(var data in DataById.Values)
            {
                MapBaseForm.AddData(data);
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

                jo.Get<string>("prefabName"),

                jo.Get<string>("icon")
                    );

            return data;
        }

        public static JObject GetJoByData(Data data)
        {
            Init();

            JObject jo=new JObject();

            jo.Set<int>("id",data.id);

            jo.Set<string>("name",data.name);

            jo.Set<string>("prefabName",data.prefabName);

            jo.Set<string>("icon",data.icon);

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

            
MapBaseForm.AddData(data);
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

MapBaseForm.RemoveData(id);
            childRemoveAction?.Invoke(data);
        }
        public static void Clear()
        {
            Init();

                DataById.Clear();

                DataByName.Clear();

            idChain.Clear();
        }

         private static void RemoveChildren(MapBaseForm.Data data)
        {
            Init();
            if(data is Data)
               RemoveData(data.id);      
        }
         private static void AddChildren(MapBaseForm.Data superData)
        {
            Init();
            if(superData is Data data)
               AddData(data);      
        }
        


    }
}
        