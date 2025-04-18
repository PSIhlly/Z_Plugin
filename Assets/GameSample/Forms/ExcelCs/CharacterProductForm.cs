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
                
        public static Action<Data,string,string> changeAvatartexnameAction;
                
        public static Action<Data,Dictionary<string,(float,float,float)>,Dictionary<string,(float,float,float)>> changeParamdicAction;
                
        public static Action<Data,bool,bool> changeIsprotoAction;
                
        public static Action<Data,List<string>,List<string>> changeAnimjoAction;
                
        public static Action<Data,string,string> changeIdleanimnameAction;
                
        public static Action<Data,string,string> changeMoveanimnameAction;
                
        public static Action<Data,string,string> changeSpeedparamnameAction;
                
        public static Action<Data,string,string> changeHpparamnameAction;
                


        public partial class Data : ProductForm.Data
        {

                    private string  _avatarTexName;
                    /// <summary>
                    ///头像名称
                    ///</summary>
                    public string  avatarTexName{
                                get{return _avatarTexName;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeAvatartexname(this,_avatarTexName,value); 
                    }
        
                _avatarTexName = value;
                }
                 
                     }
                    
                    private List<string>  _animJo;
                    /// <summary>
                    ///动画
                    ///</summary>
                    public List<string>  animJo{
                                get{return _animJo;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeAnimjo(this,_animJo,value); 
                    }
        
                _animJo = value;
                }
                 
                     }
                    
                    private string  _idleAnimName;
                    /// <summary>
                    ///闲置动画名
                    ///</summary>
                    public string  idleAnimName{
                                get{return _idleAnimName;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeIdleanimname(this,_idleAnimName,value); 
                    }
        
                _idleAnimName = value;
                }
                 
                     }
                    
                    private string  _moveAnimName;
                    /// <summary>
                    ///移动动画名
                    ///</summary>
                    public string  moveAnimName{
                                get{return _moveAnimName;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeMoveanimname(this,_moveAnimName,value); 
                    }
        
                _moveAnimName = value;
                }
                 
                     }
                    
                    private string  _speedParamName;
                    /// <summary>
                    ///速度参数名
                    ///</summary>
                    public string  speedParamName{
                                get{return _speedParamName;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeSpeedparamname(this,_speedParamName,value); 
                    }
        
                _speedParamName = value;
                }
                 
                     }
                    
                    private string  _hpParamName;
                    /// <summary>
                    ///血量参数名
                    ///</summary>
                    public string  hpParamName{
                                get{return _hpParamName;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeHpparamname(this,_hpParamName,value); 
                    }
        
                _hpParamName = value;
                }
                 
                     }
                    
            public Data(int uid,string name,string avatarTexName,Dictionary<string,(float,float,float)> paramDic,bool isProto,List<string> animJo,string idleAnimName,string moveAnimName,string speedParamName,string hpParamName):base(uid,name,paramDic,isProto)
            {

             this.uid = uid;
             this.name = name;
             this.avatarTexName = avatarTexName;
             this.paramDic = paramDic;
             this.isProto = isProto;
             this.animJo = animJo;
             this.idleAnimName = idleAnimName;
             this.moveAnimName = moveAnimName;
             this.speedParamName = speedParamName;
             this.hpParamName = hpParamName;

            }
            
        }

                   public static Data defaultData=new Data(0,"","",new Dictionary<string,(float,float,float)>(){},false,null,"","","","");


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

                jo.Get<string>("avatarTexName"),

                jo.Get<Dictionary<string,(float,float,float)>>("paramDic"),

                jo.Get<bool>("isProto"),

                jo.Get<List<string>>("animJo"),

                jo.Get<string>("idleAnimName"),

                jo.Get<string>("moveAnimName"),

                jo.Get<string>("speedParamName"),

                jo.Get<string>("hpParamName")
                    );

            return data;
        }

        public static JObject GetJoByData(Data data)
        {
            Init();

            JObject jo=new JObject();

            jo.Set<int>("uid",data.uid);

            jo.Set<string>("name",data.name);

            jo.Set<string>("avatarTexName",data.avatarTexName);

            jo.Set<Dictionary<string,(float,float,float)>>("paramDic",data.paramDic);

            jo.Set<bool>("isProto",data.isProto);

            jo.Set<List<string>>("animJo",data.animJo);

            jo.Set<string>("idleAnimName",data.idleAnimName);

            jo.Set<string>("moveAnimName",data.moveAnimName);

            jo.Set<string>("speedParamName",data.speedParamName);

            jo.Set<string>("hpParamName",data.hpParamName);

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
            
            public static void ChangeAvatartexname(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeAvatartexnameAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeParamdic(ProductForm.Data superData,Dictionary<string,(float,float,float)> oldV,Dictionary<string,(float,float,float)> newV)
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
            
            public static void ChangeAnimjo(Data superData,List<string> oldV,List<string> newV)
            {
                if(superData is Data data)
                {

                changeAnimjoAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeIdleanimname(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeIdleanimnameAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeMoveanimname(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeMoveanimnameAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeSpeedparamname(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeSpeedparamnameAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeHpparamname(Data superData,string oldV,string newV)
            {
                if(superData is Data data)
                {

                changeHpparamnameAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
    }
}
        