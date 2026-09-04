
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;
using Z_Texture;using RenderHeads.Media.AVProVideo;
namespace Ui.CmdInputArea

{




    public partial class UiCmdInputAreaParam:UiParam
    {
    }

    public partial class UiCmdInputAreaView:UiView
    {

            public Ipt ipt_;
            public Btn btn_;
            public GameObject go_close;
            public Btn btn_close;
            public Txt txt_title;
            public Txt txt_;
            public GameObject go_tips;
            public Btn btn_tips;
        public UiCmdInputAreaView(UiHolder uiHolder):base(uiHolder)
        {

            ipt_ = uiHolder.elementTrsLst[0].GetComponent<Ipt>();
            btn_ = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            go_close = uiHolder.elementTrsLst[2].gameObject;
            btn_close = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            txt_title = uiHolder.elementTrsLst[4].GetComponent<Txt>();
            txt_ = uiHolder.elementTrsLst[5].GetComponent<Txt>();
            go_tips = uiHolder.elementTrsLst[6].gameObject;
            btn_tips = uiHolder.elementTrsLst[7].GetComponent<Btn>();
        }

    }
    public partial class UiCmdInputAreaCtrl:UiCtrl
    {
        public UiCmdInputAreaView view;
        public UiCmdInputAreaModel model;
        public UiCmdInputAreaParam param;
        

        public override void SetParam(UiParam param)
        {
            this.param = (UiCmdInputAreaParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiCmdInputAreaView(uiHolder);
            model=new UiCmdInputAreaModel();


        }

    }
    public partial class UiCmdInputAreaModel:UiModel
    {
        
    }
}
