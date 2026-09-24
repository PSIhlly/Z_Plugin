
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;
using Z_Texture;using RenderHeads.Media.AVProVideo;
namespace Ui.ModStoryEventCmdChooseWindow

{




    public partial class UiModStoryEventCmdChooseWindowParam:UiParam
    {
    }

    public partial class UiModStoryEventCmdChooseWindowView:UiView
    {

            public Btn btn_bbg;
            public Btn btn_close;
            public Btn btn_lab;
            public Sta sta_lab;
            public Btn btn_;
        public UiModStoryEventCmdChooseWindowView(UiHolder uiHolder):base(uiHolder)
        {

            btn_bbg = uiHolder.elementTrsLst[0].GetComponent<Btn>();
            btn_close = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            btn_lab = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            sta_lab = uiHolder.elementTrsLst[3].GetComponent<Sta>();
            btn_ = uiHolder.elementTrsLst[4].GetComponent<Btn>();
        }

    }
    public partial class UiModStoryEventCmdChooseWindowCtrl:UiCtrl
    {
        public UiModStoryEventCmdChooseWindowView view;
        public UiModStoryEventCmdChooseWindowModel model;
        public UiModStoryEventCmdChooseWindowParam param;
        

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryEventCmdChooseWindowParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryEventCmdChooseWindowView(uiHolder);
            model=new UiModStoryEventCmdChooseWindowModel();


        }

    }
    public partial class UiModStoryEventCmdChooseWindowModel:UiModel
    {
        
    }
}
