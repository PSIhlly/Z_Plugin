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

    public static partial class MapTypeForm
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Register()
        {

        }

        private static bool inited;
        public static Z_Chain.Chain idChain;
        public static Action childInitAction;

        public partial class Data
        {

                public readonly int id;

                /// <summary>
                ///名称索引
                ///</summary>
                public readonly string NameKey;

                /// <summary>
                ///图标
                ///</summary>
                public readonly string icon;

                /// <summary>
                ///是否需要层级设置
                ///</summary>
                public readonly bool needLayer;

            public Data(int id,string NameKey,string icon,bool needLayer)
            {

                this.id = id;
                this.NameKey = NameKey;
                this.icon = icon;
                this.needLayer = needLayer;

            }
            
        }

                   public static Data defaultData=new Data(0,"","",false);


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
            idChain=new Z_Chain.Chain (100);
            

                _DataById = new Dictionary<int, Data>() {

                {1,new Data(1,"terrain","",false)},

                {2,new Data(2,"texture","",true)},

                {3,new Data(3,"transition mask","",true)},

                {4,new Data(4,"obstacle","",false)},

                {100,new Data(100,"erase","",false)},

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

                    defaultData.NameKey,

                    defaultData.icon,

                    defaultData.needLayer
                    );

            return data;
        }

        public static JObject GetJoByData(Data data)
        {
            Init();

            JObject jo=new JObject();

            jo.Set<int>("id",data.id);

            return jo;
        }


    }
}
        