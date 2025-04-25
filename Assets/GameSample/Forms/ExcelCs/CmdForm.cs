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

namespace Form
{

    public static partial class CmdForm
    {
public static readonly int autoUidCnt=100;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Register()
        {



            Z_Json.extra[typeof(Data)]=((obj)=>{
            if(obj is Data data)
                return GetJoByData(data);
            return null;
            },(jo)=>{
            return GetDataByJo(jo);
            });
        }
        
        private static bool inited;

        public static Z_Chain.Chain uidChain ;

        public static Action childInitAction;
        public static Action<Data> childRemoveAction;
        public static Action<Data> childAddAction;

        public static Action<Data,int,int> changeUidAction;
                
        public static Action<Data,string,string> changeNameAction;
                
        public static Action<Data,int,int> changePrmcntAction;
                
        public static Action<Data,int,int> changeRescntAction;
                
        public static Action<Data,float,float> changeConstvAction;
                
        public static Action<Data,string,string> changeLabAction;
                


        public partial class Data
        {

                    private int  _uid;
                    /// <summary>
                    ///
                    ///</summary>
                    public int  uid{
                                get{return _uid;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeUid(this,_uid,value); 
                    }
        
                _uid = value;
                }
                 
                     }
                    
                    private string  _name;
                    /// <summary>
                    ///名称
                    ///</summary>
                    public string  name{
                                get{return _name;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeName(this,_name,value); 
                    }
        
                _name = value;
                }
                 
                     }
                    
                    private int  _prmCnt;
                    /// <summary>
                    ///参数数量
                    ///</summary>
                    public int  prmCnt{
                                get{return _prmCnt;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangePrmcnt(this,_prmCnt,value); 
                    }
        
                _prmCnt = value;
                }
                 
                     }
                    
                    private int  _resCnt;
                    /// <summary>
                    ///结果数量
                    ///</summary>
                    public int  resCnt{
                                get{return _resCnt;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeRescnt(this,_resCnt,value); 
                    }
        
                _resCnt = value;
                }
                 
                     }
                    
                    private float  _constV;
                    /// <summary>
                    ///常量
                    ///</summary>
                    public float  constV{
                                get{return _constV;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeConstv(this,_constV,value); 
                    }
        
                _constV = value;
                }
                 
                     }
                    
                    private string  _lab;
                    /// <summary>
                    ///一级标签
                    ///</summary>
                    public string  lab{
                                get{return _lab;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeLab(this,_lab,value); 
                    }
        
                _lab = value;
                }
                 
                     }
                    
            public Data(int uid,string name,int prmCnt,int resCnt,float constV,string lab)
            {

             this.uid = uid;
             this.name = name;
             this.prmCnt = prmCnt;
             this.resCnt = resCnt;
             this.constV = constV;
             this.lab = lab;

            }

                public Data Copy()
                {
        return new Data(-1,name,prmCnt,resCnt,constV,lab);
                }
            
        }

                   public static Data defaultData=new Data(0,"",0,0,0f,"");


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
    
            static Dictionary<string, List<Data>> _DatasByLab;
            public static Dictionary<string, List<Data>> DatasByLab
            {
                get
                {
                    Init();
                    return _DatasByLab;
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
uidChain=new Z_Chain.Chain (autoUidCnt);

                _DataByUid = new Dictionary<int, Data>() {

                {1,new Data(1,"dialog",1,0,0f,"弹窗")},

                {2,new Data(2,"tips",1,0,0f,"提示")},

                };
                    _DataByName = new Dictionary<string, Data>() {
    
                        {"dialog",_DataByUid[1]},
    
                        {"tips",_DataByUid[2]},
    
                    };
    
                    _DatasByLab = new Dictionary<string, List<Data>>() {
    
                            {"弹窗",new List<Data>()},
        
                            {"提示",new List<Data>()},
        
                };

                    _DatasByLab["弹窗"].Add(_DataByUid[1]);

                    _DatasByLab["提示"].Add(_DataByUid[2]);


            childInitAction?.Invoke();
            

foreach(var k in _DataByUid.Keys){ uidChain.PopId(k); }
             
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

                jo.Get<int>("prmCnt"),

                jo.Get<int>("resCnt"),

                jo.Get<float>("constV"),

                jo.Get<string>("lab")
                    );

            return data;
        }

        public static JObject GetJoByData(Data data)
        {
            Init();

            JObject jo=new JObject();

            jo.Set<int>("uid",data.uid);

            jo.Set<string>("name",data.name);

            jo.Set<int>("prmCnt",data.prmCnt);

            jo.Set<int>("resCnt",data.resCnt);

            jo.Set<float>("constV",data.constV);

            jo.Set<string>("lab",data.lab);

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
    
                    if(!DatasByLab.ContainsKey(data.lab))
                        DatasByLab[data.lab]=new List<Data>();
                    DatasByLab[data.lab].Add(data);
    

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
    
                    DatasByLab[data.lab].Remove(data);
    

            uidChain.PushId(data.uid);
            childRemoveAction?.Invoke(data);
        }
        public static void Clear()
        {
            Init();

                    DataByUid.Clear();
    
                    DataByName.Clear();
    
                    DatasByLab.Clear();
    
            uidChain.Clear();
        }

         private static void RemoveChildren(Data data)
        {
            Init();
            if(data is Data)
               RemoveData(data.uid);      
        }
         private static void AddChildren(Data superData)
        {
            Init();
            if(superData is Data data)
               AddData(data);      
        }
        




            public static void ChangeUid(Data superData,int oldV,int newV)
            {
                if(superData is Data data)
                {

                changeUidAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeName(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                    DataByName.Remove(oldV);
                    DataByName[newV]=data;
 
                changeNameAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangePrmcnt(Data superData,int oldV,int newV)
            {
                if(superData is Data data)
                {

                changePrmcntAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeRescnt(Data superData,int oldV,int newV)
            {
                if(superData is Data data)
                {

                changeRescntAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeConstv(Data superData,float oldV,float newV)
            {
                if(superData is Data data)
                {

                changeConstvAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeLab(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                    DatasByLab[oldV].Remove(data);
                    DatasByLab[newV].Add(data);
 
                changeLabAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
    }
}
        