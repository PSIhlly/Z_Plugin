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

    public static partial class MapTextureForm
    {
        

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Register()
        {

                MapBaseForm.childInitAction+=InitInternal;


                MapBaseForm.childRemoveAction+=RemoveChildren;
            
        }
        
        private static bool inited;
        public static Z_Chain.Chain idChain=>MapBaseForm.idChain;
        public static Action childInitAction;
        public static Action<Data> childRemoveAction;

        public partial class Data : MapBaseForm.Data
        {

                /// <summary>
                ///播放速度
                ///</summary>
                public float animSpeed;

            public Data(int id,string name,string icon,float animSpeed):base(id,name,icon)
            {

                this.id = id;
                this.name = name;
                this.icon = icon;
                this.animSpeed = animSpeed;

            }
            
        }

                   public static Data defaultData=new Data(0,"","",0f);


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

            MapBaseForm.Init();

        }
        public static void InitInternal()
        {
            if(inited)
                return;
            inited=true;  
            
            

                _DataById = new Dictionary<int, Data>() {

                {200001,new Data(200001,"floor","",999f)},

                {200002,new Data(200002,"grass","",999f)},

                {200003,new Data(200003,"road","",999f)},

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

                jo.Get<string>("icon"),

                jo.Get<float>("animSpeed")
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

            jo.Set<float>("animSpeed",data.animSpeed);

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

            
MapBaseForm.AddData(data);
            return data.id;
        }
        public static void RemoveData(int id)
        {            
            Init();
            if(!_DataById.ContainsKey(id))
                return;
                
            var data=_DataById[id];

                _DataById.Remove(data.id);

MapBaseForm.RemoveData(id);
            childRemoveAction?.Invoke(data);
        }
        public static void Clear()
        {
            Init();

                _DataById.Clear();

            idChain.Clear();
        }

         private static void RemoveChildren(MapBaseForm.Data data)
        {
            Init();
            if(data is Data)
               RemoveData(data.id);      
        }


    }
}
        