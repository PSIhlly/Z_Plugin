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

    public static partial class CharacterAnimClipForm
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

        public static Action<Data> addAction;
        public static Action<Data> removeAction;
        public static Action childInitAction;
        public static Action<Data> childRemoveAction;
        public static Action<Data> childAddAction;
        
        public static Action<Data> beforeGetAction;

        public static Action<Data,int,int> changeUidAction;
                
        public static Action<Data,Dictionary<EquipPartType,ItemStyle>,Dictionary<EquipPartType,ItemStyle>> changeEquipstyleAction;
                
        public static Action<Data,Dictionary<EquipPartType,(float,float,int,float)>,Dictionary<EquipPartType,(float,float,int,float)>> changeEquiptrsAction;
                
        public static Action<Data,Dictionary<BodyPartType,string>,Dictionary<BodyPartType,string>> changeParttexAction;
                


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
                    
                    private Dictionary<EquipPartType,ItemStyle>  _equipStyle;
                    /// <summary>
                    ///装备样式
                    ///</summary>
                    public Dictionary<EquipPartType,ItemStyle>  equipStyle{
                                get{return _equipStyle;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeEquipstyle(this,_equipStyle,value); 
                    }
        
                _equipStyle = value;
                }
                 
                     }
                    
                    private Dictionary<EquipPartType,(float,float,int,float)>  _equipTrs;
                    /// <summary>
                    ///装备位置
                    ///</summary>
                    public Dictionary<EquipPartType,(float,float,int,float)>  equipTrs{
                                get{return _equipTrs;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeEquiptrs(this,_equipTrs,value); 
                    }
        
                _equipTrs = value;
                }
                 
                     }
                    
                    private Dictionary<BodyPartType,string>  _partTex;
                    /// <summary>
                    ///贴图名称
                    ///</summary>
                    public Dictionary<BodyPartType,string>  partTex{
                                get{return _partTex;}
 set{

                    if(_DataByUid!=null&&_DataByUid.ContainsValue(this))
                    {
                       ChangeParttex(this,_partTex,value); 
                    }
        
                _partTex = value;
                }
                 
                     }
                    
            public Data(int uid,Dictionary<EquipPartType,ItemStyle> equipStyle,Dictionary<EquipPartType,(float,float,int,float)> equipTrs,Dictionary<BodyPartType,string> partTex)
            {

             this.uid = uid;
             this.equipStyle = equipStyle;
             this.equipTrs = equipTrs;
             this.partTex = partTex;

            }
            public void Reset(Data data)
            {

             this.uid = data.uid;
             this.equipStyle = data.equipStyle;
             this.equipTrs = data.equipTrs;
             this.partTex = data.partTex;
            }

                public Data Copy(bool sameId = true)
                {
        return new Data(sameId? uid:uidChain.GetId(),new Dictionary<EquipPartType,ItemStyle>(equipStyle),new Dictionary<EquipPartType,(float,float,int,float)>(equipTrs),new Dictionary<BodyPartType,string>(partTex));
                }
            
            public virtual  void BeforeGet()
            {
                
                CharacterAnimClipForm.beforeGetAction?.Invoke(this);
            }
        }

                   private static Data _defaultData=new Data(0,new Dictionary<EquipPartType,ItemStyle>(){},new Dictionary<EquipPartType,(float,float,int,float)>(){},new Dictionary<BodyPartType,string>(){});
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

                };

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

                jo.SelectToken("uid")==null?defaultData.uid:jo.Get<int>("uid"),

                jo.SelectToken("equipStyle")==null?defaultData.equipStyle:jo.Get<Dictionary<EquipPartType,ItemStyle>>("equipStyle"),

                jo.SelectToken("equipTrs")==null?defaultData.equipTrs:jo.Get<Dictionary<EquipPartType,(float,float,int,float)>>("equipTrs"),

                jo.SelectToken("partTex")==null?defaultData.partTex:jo.Get<Dictionary<BodyPartType,string>>("partTex")
                    );

            return data;
        }

        public static JObject GetJoByData(Data data)
        {
            Init();
            data.BeforeGet();

            JObject jo=new JObject();

            jo.Set<int>("uid",data.uid);

            jo.Set<Dictionary<EquipPartType,ItemStyle>>("equipStyle",data.equipStyle);

            jo.Set<Dictionary<EquipPartType,(float,float,int,float)>>("equipTrs",data.equipTrs);

            jo.Set<Dictionary<BodyPartType,string>>("partTex",data.partTex);

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
    

            childAddAction?.Invoke(data);
            addAction?.Invoke(data);
            return data.uid;
        }
        public static void RemoveData(int uid)
        {            
            Init();
            if(!DataByUid.ContainsKey(uid))
                return;
               
            var data=DataByUid[uid];

                    DataByUid.Remove(data.uid);
    

            uidChain.PushId(data.uid);
            childRemoveAction?.Invoke(data);
            removeAction?.Invoke(data);
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
            
            public static void ChangeEquipstyle(Data superData,Dictionary<EquipPartType,ItemStyle> oldV,Dictionary<EquipPartType,ItemStyle> newV)
            {
                if(superData is Data data)
                {

                changeEquipstyleAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeEquiptrs(Data superData,Dictionary<EquipPartType,(float,float,int,float)> oldV,Dictionary<EquipPartType,(float,float,int,float)> newV)
            {
                if(superData is Data data)
                {

                changeEquiptrsAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
            public static void ChangeParttex(Data superData,Dictionary<BodyPartType,string> oldV,Dictionary<BodyPartType,string> newV)
            {
                if(superData is Data data)
                {

                changeParttexAction?.Invoke(data,oldV,newV);
                }
                    
            }
            
    }
}
        