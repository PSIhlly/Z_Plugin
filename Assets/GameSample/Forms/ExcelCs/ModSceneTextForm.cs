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

    public static partial class ModSceneTextForm
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Register()
        {

                TextBaseForm.childInitAction+=InitInternal;

        }

        private static bool inited;
        public static Z_Chain.Chain idChain=>TextBaseForm.idChain;
        public static Action childInitAction;

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


        static Dictionary<int, Data> _DataById = null;
        public static Dictionary<int, Data> DataById
        {
            get
            {
                Init();
                return _DataById;
            }
        }

        static Dictionary<string, Data> _DataByKey = null;
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

                {1000001,new Data(1000001,"terrain","Terrain","地形")},

                {1000002,new Data(1000002,"texture","Texture","贴图")},

                {1000003,new Data(1000003,"transition mask","Transition mask","过渡遮罩")},

                {1000004,new Data(1000004,"obstacle","Obstacle","障碍物")},

                {1000100,new Data(1000100,"erase","Erase","清除")},

                {1000101,new Data(1000101,"all erase","All erase","全部清除")},

                {1000102,new Data(1000102,"remain terrain","Remain terrain","保留地面")},

                {1000103,new Data(1000103,"texture only","Terrain texture only","仅地面贴图")},

                {1001000,new Data(1001000,"maxYTip","The height must be less than the ceiling of this level.","高度必须小于该层天花板")},

                {1001001,new Data(1001001,"minYTip","The height must be greater than the floor of this level.","高度必须大于该层地板")},

                };

                _DataByKey = new Dictionary<string, Data>() {

                    {"terrain",_DataById[1000001]},

                    {"texture",_DataById[1000002]},

                    {"transition mask",_DataById[1000003]},

                    {"obstacle",_DataById[1000004]},

                    {"erase",_DataById[1000100]},

                    {"all erase",_DataById[1000101]},

                    {"remain terrain",_DataById[1000102]},

                    {"texture only",_DataById[1000103]},

                    {"maxYTip",_DataById[1001000]},

                    {"minYTip",_DataById[1001001]},

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
            if(data.id==-1)
            { 
                int id=idChain.GetId();
                if(id==-1)
                    return -1;
                data.id=id;  
            }

                _DataById[data.id]=data;

                _DataByKey[data.key]=data;

            
TextBaseForm.AddData(data);
            return data.id;
        }
        public static void RemoveData(int id)
        {            
            Init();
            if(!_DataById.ContainsKey(id))
                return;
                
            var data=_DataById[id];

                _DataById.Remove(data.id);

                _DataByKey.Remove(data.key);

TextBaseForm.RemoveData(id);
        }
        public static void Clear()
        {
            Init();

                _DataById.Clear();

                _DataByKey.Clear();

            idChain.Clear();
        }

    }
}
        