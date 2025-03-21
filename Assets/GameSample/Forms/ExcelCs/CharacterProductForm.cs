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

    public static partial class CharacterProductForm
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
                
        public static Action<Data,Dictionary<string,(int,int,int)>,Dictionary<string,(int,int,int)>> changeParamdicAction;
                
        public static Action<Data,bool,bool> changeIsprotoAction;
                
        public static Action<Data,List<string>,List<string>> changeAnimnameAction;
                
        public static Action<Data,List<Vector2>,List<Vector2>> changeAnimposAction;
                
        public static Action<Data,List<float>,List<float>> changeAnimtimeintervalAction;
                


        public partial class Data : ProductForm.Data
        {

                    private List<string>  _animName;
                    /// <summary>
                    ///动画
                    ///</summary>
                    public List<string>  animName{
                                get{return _animName;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeAnimname(this,_animName,value); 
                    }
        
                _animName = value;
                }
                 
                     }
                    
                    private List<Vector2>  _animPos;
                    /// <summary>
                    ///动画关键位置
                    ///</summary>
                    public List<Vector2>  animPos{
                                get{return _animPos;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeAnimpos(this,_animPos,value); 
                    }
        
                _animPos = value;
                }
                 
                     }
                    
                    private List<float>  _animTimeInterval;
                    /// <summary>
                    ///动画间隔
                    ///</summary>
                    public List<float>  animTimeInterval{
                                get{return _animTimeInterval;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeAnimtimeinterval(this,_animTimeInterval,value); 
                    }
        
                _animTimeInterval = value;
                }
                 
                     }
                    
            public Data(int uid,string name,Dictionary<string,(int,int,int)> paramDic,bool isProto,List<string> animName,List<Vector2> animPos,List<float> animTimeInterval):base(uid,name,paramDic,isProto)
            {

             this.uid = uid;
             this.name = name;
             this.paramDic = paramDic;
             this.isProto = isProto;
             this.animName = animName;
             this.animPos = animPos;
             this.animTimeInterval = animTimeInterval;

            }
            
        }

                   public static Data defaultData=new Data(0,"",new Dictionary<string,(int,int,int)>(){},false,null,null,null);


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

                jo.Get<Dictionary<string,(int,int,int)>>("paramDic"),

                jo.Get<bool>("isProto"),

                jo.Get<List<string>>("animName"),

                jo.Get<List<Vector2>>("animPos"),

                jo.Get<List<float>>("animTimeInterval")
                    );

            return data;
        }

        public static JObject GetJoByData(Data data)
        {
            Init();

            JObject jo=new JObject();

            jo.Set<int>("uid",data.uid);

            jo.Set<string>("name",data.name);

            jo.Set<Dictionary<string,(int,int,int)>>("paramDic",data.paramDic);

            jo.Set<bool>("isProto",data.isProto);

            jo.Set<List<string>>("animName",data.animName);

            jo.Set<List<Vector2>>("animPos",data.animPos);

            jo.Set<List<float>>("animTimeInterval",data.animTimeInterval);

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
            
            public static void ChangeParamdic(ProductForm.Data superData,Dictionary<string,(int,int,int)> oldV,Dictionary<string,(int,int,int)> newV)
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
            
            public static void ChangeAnimname(Data superData,List<string> oldV,List<string> newV)
            {
                if(superData is Data data)
                {

                changeAnimnameAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeAnimpos(Data superData,List<Vector2> oldV,List<Vector2> newV)
            {
                if(superData is Data data)
                {

                changeAnimposAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeAnimtimeinterval(Data superData,List<float> oldV,List<float> newV)
            {
                if(superData is Data data)
                {

                changeAnimtimeintervalAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
    }
}
        