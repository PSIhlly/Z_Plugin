using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
namespace Form.表A
{

    public static partial class 表A
    {
        public class Data
        {

            public readonly int id;

            public string name;

            public bool enable;

            public readonly float rate;

            /// <summary>
            ///类型
            ///</summary>
            public readonly Vector3 type;

            public readonly List<int> subs;

            public readonly (int, int) pair;

            public readonly Dictionary<int, bool> dic;

            public Data(int id, string name, bool enable, float rate, Vector3 type, List<int> subs, (int, int) pair, Dictionary<int, bool> dic)
            {

                this.id = id;
                this.name = name;
                this.enable = enable;
                this.rate = rate;
                this.type = type;
                this.subs = subs;
                this.pair = pair;
                this.dic = dic;
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

                {0,new Data(0,"boy",true,3.3f,Vector3.down,new List<int>(){1,2,5,},(2,3),new Dictionary<int,bool>(){{2,true},})},

                {1,new Data(1,"",true,0f,Vector3.down,null,(0,0),new Dictionary<int,bool>(){})},

                {2,new Data(2,"girl",false,0f,Vector3.down,new List<int>(){1,},(0,0),new Dictionary<int,bool>(){{1,false},{0,false},})},

            };
        }
    }
}
