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

    public static partial class CharacterPruductForm
    {

        

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Register()
        {

                ProductForm.childInitAction+=InitInternal;


                ProductForm.childRemoveAction+=RemoveChildren;
                ProductForm.childAddAction+=AddChildren;
            

            ProductForm.changeUidAction+=ChangeUid;

            ProductForm.changeNameAction+=ChangeName;

            ProductForm.changeParamdicAction+=ChangeParamdic;

            ProductForm.changeIsprotoAction+=ChangeIsproto;

        }
        
        private static bool inited;

        public static Z_Chain.Chain uidChain =>ProductForm.uidChain;

        public static Action childInitAction;
        public static Action<Data> childRemoveAction;
        public static Action<Data> childAddAction;

        public static Action<Data,int,int> changeUidAction;
                
        public static Action<Data,string,string> changeNameAction;
                
        public static Action<Data,Dictionary<string,object>,Dictionary<string,object>> changeParamdicAction;
                
        public static Action<Data,bool,bool> changeIsprotoAction;
                


        public partial class Data : ProductForm.Data
        {

            public Data(int uid,string name,Dictionary<string,object> paramDic,bool isProto):base(uid,name,paramDic,isProto)
            {

             this.uid = uid;
             this.name = name;
             this.paramDic = paramDic;
             this.isProto = isProto;

            }
            
        }

                   public static Data defaultData=new Data(0,"",new Dictionary<string,object>(){},false);


            static Dictionary<int, Data> _DataByUid;
            public static Dictionary<int, Data> DataByUid
            {
                get
                {
                    Init();
                    return _DataByUid;
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

            ProductForm.Init();

        }
        public static void InitInternal()
        {
            if(inited)
                return;
            inited=true;  

        

                _DataByUid = new Dictionary<int, Data>() {

                };
                    _DataByName = new Dictionary<string, Data>() {
    
                    };
    

            childInitAction?.Invoke();
            

            foreach(var data in DataByUid.Values)
            {
                ProductForm.AddData(data);
            }


        
             
        }


        public static List<Data> GetDatasByJa(JArray ja)
        {
            Init();
            List<Data> lst=new List<Data>();
            foreach(JObject jo in ja)
            {
                if(jo.Get<int>("uid")==0)
                    continue;
                lst.Add(GetDataByJo(jo));
            }
            return lst;
        }

        public static JArray GetJaByDatas()
        {
            Init();
            JArray ja=new JArray();
            foreach(Data data in _DataByUid.Values)
            {
                if(data.uid==0)
                    continue;
                ja.Add(GetJoByData(data));
            }
            return ja;
        }

        public static Data GetDataByJo(JObject jo)
        {
            Init();

            Data data=new Data(

                jo.Get<int>("uid"),

                jo.Get<string>("name"),

                jo.Get<Dictionary<string,object>>("paramDic"),

                jo.Get<bool>("isProto")
                    );

            return data;
        }

        public static JObject GetJoByData(Data data)
        {
            Init();

            JObject jo=new JObject();

            jo.Set<int>("uid",data.uid);

            jo.Set<string>("name",data.name);

            jo.Set<Dictionary<string,object>>("paramDic",data.paramDic);

            jo.Set<bool>("isProto",data.isProto);

            return jo;
        }


        public static int AddData(Data data)
        {
            Init();
            if(DataByUid.ContainsKey(data.uid))
                return data.uid;
            if(data.uid==-1)
            { 
                int uid=uidChain.GetId();
                if(uid==-1)
                    return -1;
                data.uid=uid;  
            }

        DataByUid[data.uid]=data;
    
                    DataByName[data.name]=data;
    
ProductForm.AddData(data);
            childAddAction?.Invoke(data);
            return data.uid;
        }
        public static void RemoveData(int uid)
        {            
            Init();
            if(!DataByUid.ContainsKey(uid))
                return;
               
            var data=DataByUid[uid];

                    DataByUid.Remove(data.uid);
    
                    DataByName.Remove(data.name);
    
ProductForm.RemoveData(uid);
            childRemoveAction?.Invoke(data);
        }
        public static void Clear()
        {
            Init();

                    DataByUid.Clear();
    
                    DataByName.Clear();
    
            uidChain.Clear();
        }

         private static void RemoveChildren(ProductForm.Data data)
        {
            Init();
            if(data is Data)
               RemoveData(data.uid);      
        }
         private static void AddChildren(ProductForm.Data superData)
        {
            Init();
            if(superData is Data data)
               AddData(data);      
        }
        




            public static void ChangeUid(ProductForm.Data superData,int oldV,int newV)
            {
                if(superData is Data data)
                {

                changeUidAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeName(ProductForm.Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                    DataByName.Remove(oldV);
                    DataByName[newV]=data;
 
                changeNameAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeParamdic(ProductForm.Data superData,Dictionary<string,object> oldV,Dictionary<string,object> newV)
            {
                if(superData is Data data)
                {

                changeParamdicAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeIsproto(ProductForm.Data superData,bool oldV,bool newV)
            {
                if(superData is Data data)
                {

                changeIsprotoAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
    }
}
        