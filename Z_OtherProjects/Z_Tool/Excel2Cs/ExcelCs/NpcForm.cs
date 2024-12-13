using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
namespace Form.NpcForm
{

    public static partial class NpcForm
    {
        public class Data
        {

            public readonly int id;

            /// <summary>
            ///Ãû³Æ
            ///</summary>
            public readonly string name;

            /// <summary>
            ///Í·ÏñÍ¼Æ¬Id
            ///</summary>
            public readonly int avatar_imgId;

            public Data(int id,string name,int avatar_imgId)
            {

                this.id = id;
                this.name = name;
                this.avatar_imgId = avatar_imgId;
            }
            
        }
        static IReadOnlyDictionary<int, Data> _Datas = null;
        public static IReadOnlyDictionary<int, Data> Datas
        {
            get
            {
                Init();
                return _Datas;
            }
        }

        static void Init()
        {
            _Datas = new Dictionary<int, Data>() {

                {0,new Data(0,"",0)},

                {1,new Data(1,"human",100001)},

                {2,new Data(2,"pig",100002)},

                {3,new Data(3,"dog",100003)},

                {4,new Data(4,"chicken",100004)},

            };
        }
    }
}
        