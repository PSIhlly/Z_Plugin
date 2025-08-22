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

    public static partial class GameCmdDataForm
    {

        

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Register()
        {

                CmdDataForm.childInitAction+=InitInternal;


                CmdDataForm.childRemoveAction+=RemoveChildren;
                CmdDataForm.childAddAction+=AddChildren;
            

            CmdDataForm.changeUidAction+=ChangeUid;

            CmdDataForm.changeNameAction+=ChangeName;

            CmdDataForm.changePrmnamesAction+=ChangePrmnames;

            CmdDataForm.changeRetnamesAction+=ChangeRetnames;

            CmdDataForm.changeDescAction+=ChangeDesc;

            CmdDataForm.changePrmtypesAction+=ChangePrmtypes;

            CmdDataForm.changeRettypesAction+=ChangeRettypes;

            Z_Json.extra[typeof(Data)]=((obj)=>{
            if(obj is Data data)
                return GetJoByData(data);
            return null;
            },(jo)=>{
            return GetDataByJo(jo);
            });
        }
        
        private static bool inited;

        public static Z_Chain.Chain uidChain =>CmdDataForm.uidChain;

        public static Action childInitAction;
        public static Action<Data> childRemoveAction;
        public static Action<Data> childAddAction;

        public static Action<Data,int,int> changeUidAction;
                
        public static Action<Data,string,string> changeNameAction;
                
        public static Action<Data,List<string>,List<string>> changePrmnamesAction;
                
        public static Action<Data,List<string>,List<string>> changeRetnamesAction;
                
        public static Action<Data,string,string> changeDescAction;
                
        public static Action<Data,List<string>,List<string>> changePrmtypesAction;
                
        public static Action<Data,List<string>,List<string>> changeRettypesAction;
                


        public partial class Data : CmdDataForm.Data
        {

            public Data(int uid,string name,List<string> prmNames,List<string> retNames,string desc,List<string> prmTypes,List<string> retTypes):base(uid,name,prmNames,retNames,desc,prmTypes,retTypes)
            {

             this.uid = uid;
             this.name = name;
             this.prmNames = prmNames;
             this.retNames = retNames;
             this.desc = desc;
             this.prmTypes = prmTypes;
             this.retTypes = retTypes;

            }

                public Data Copy(bool sameId = true)
                {
        return new Data(sameId? uid:uidChain.GetId(),name,new List<string>(prmNames),new List<string>(retNames),desc,new List<string>(prmTypes),new List<string>(retTypes));
                }
            
        }

                   private static Data _defaultData=new Data(0,"",null,null,"",null,null);
                   public static Data defaultData=>_defaultData.Copy();


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

            CmdDataForm.Init();

        }
        public static void InitInternal()
        {
            if(inited)
                return;
            inited=true;  

        

                _DataByUid = new Dictionary<int, Data>() {

                {100001,new Data(100001,"ShowTip",new List<string>(){"content",},null,"Show Tip: {0}",new List<string>(){"string",},null)},

                };
                    _DataByName = new Dictionary<string, Data>() {
    
                        {"ShowTip",_DataByUid[100001]},
    
                    };
    

            childInitAction?.Invoke();
            

            foreach(var data in DataByUid.Values)
            {
                CmdDataForm.AddData(data);
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

                jo.Get<List<string>>("prmNames"),

                jo.Get<List<string>>("retNames"),

                jo.Get<string>("desc"),

                jo.Get<List<string>>("prmTypes"),

                jo.Get<List<string>>("retTypes")
                    );

            return data;
        }

        public static JObject GetJoByData(Data data)
        {
            Init();

            JObject jo=new JObject();

            jo.Set<int>("uid",data.uid);

            jo.Set<string>("name",data.name);

            jo.Set<List<string>>("prmNames",data.prmNames);

            jo.Set<List<string>>("retNames",data.retNames);

            jo.Set<string>("desc",data.desc);

            jo.Set<List<string>>("prmTypes",data.prmTypes);

            jo.Set<List<string>>("retTypes",data.retTypes);

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
            uidChain.PopId(data.uid);

        DataByUid[data.uid]=data;
    
                    DataByName[data.name]=data;
    
CmdDataForm.AddData(data);
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
    
CmdDataForm.RemoveData(uid);
            uidChain.PushId(data.uid);
            childRemoveAction?.Invoke(data);
        }
        public static void Clear()
        {
            Init();
            var keys = new List<int>(DataByUid.Keys);
            foreach(var key in keys)
            {
                    RemoveData(key);
            }

        }
        
        public static void ClearAuto()
        {
            Init();
            var keys = new List<int>(DataByUid.Keys);
            foreach(var key in keys)
            {
                if(key < uidChain.cnt)
                    RemoveData(key);
            }
        }

         private static void RemoveChildren(CmdDataForm.Data data)
        {
            Init();
            if(data is Data)
               RemoveData(data.uid);      
        }
         private static void AddChildren(CmdDataForm.Data superData)
        {
            Init();
            if(superData is Data data)
               AddData(data);      
        }
        




            public static void ChangeUid(CmdDataForm.Data superData,int oldV,int newV)
            {
                if(superData is Data data)
                {

                changeUidAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeName(CmdDataForm.Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                    DataByName.Remove(oldV);
                    DataByName[newV]=data;
 
                changeNameAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangePrmnames(CmdDataForm.Data superData,List<string> oldV,List<string> newV)
            {
                if(superData is Data data)
                {

                changePrmnamesAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeRetnames(CmdDataForm.Data superData,List<string> oldV,List<string> newV)
            {
                if(superData is Data data)
                {

                changeRetnamesAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeDesc(CmdDataForm.Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeDescAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangePrmtypes(CmdDataForm.Data superData,List<string> oldV,List<string> newV)
            {
                if(superData is Data data)
                {

                changePrmtypesAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeRettypes(CmdDataForm.Data superData,List<string> oldV,List<string> newV)
            {
                if(superData is Data data)
                {

                changeRettypesAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
    }
}
        