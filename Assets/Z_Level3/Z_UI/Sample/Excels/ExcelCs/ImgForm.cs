using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
namespace Z_Ui.Form.ImgForm
{

    public static partial class ImgForm
    {
        public class Data
        {

            public readonly int id;

            /// <summary>
            ///头像目录
            ///</summary>
            public readonly string path;

            public Data(int id,string path)
            {

                this.id = id;
                this.path = path;
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

                {0,new Data(0,"")},

                {100001,new Data(100001,"\\Z_Level2\\Z_UI\\Sample\\Imgs\\npc1")},

                {100002,new Data(100002,"\\Z_Level2\\Z_UI\\Sample\\Imgs\\npc2")},

                {100003,new Data(100003,"\\Z_Level2\\Z_UI\\Sample\\Imgs\\npc3")},

                {100004,new Data(100004,"\\Z_Level2\\Z_UI\\Sample\\Imgs\\npc4")},

                {200001,new Data(200001,"\\Z_Level2\\Z_UI\\Sample\\Imgs\\bg1")},

                {200002,new Data(200002,"\\Z_Level2\\Z_UI\\Sample\\Imgs\\bg2")},

            };
        }
    }
}
        