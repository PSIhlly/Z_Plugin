using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
namespace Z_Ui.Form.Dialog
{

    public static partial class Dialog
    {
        public class Data
        {

            public readonly int id;

            /// <summary>
            ///组号
            ///</summary>
            public readonly int groupId;

            /// <summary>
            ///说话者Id
            ///</summary>
            public readonly int speaker_npcId;

            /// <summary>
            ///对话背景图片Id
            ///</summary>
            public readonly int background_imgId;

            /// <summary>
            ///对话文本
            ///</summary>
            public readonly string text;

            public Data(int id,int groupId,int speaker_npcId,int background_imgId,string text)
            {

                this.id = id;
                this.groupId = groupId;
                this.speaker_npcId = speaker_npcId;
                this.background_imgId = background_imgId;
                this.text = text;
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

                {0,new Data(0,0,0,0,"")},

                {1,new Data(1,1,1,200001,"hello")},

                {2,new Data(2,1,1,200002,"你好")},

                {3,new Data(3,1,2,200002,"world")},

                {4,new Data(4,2,2,200001,"ok")},

                {5,new Data(5,2,1,200001,"fine")},

            };
        }
    }
}
        