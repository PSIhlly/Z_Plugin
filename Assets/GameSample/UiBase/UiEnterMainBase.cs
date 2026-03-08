
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;
using Z_Texture;using RenderHeads.Media.AVProVideo;
namespace Ui.EnterMain

{




    public partial class UiEnterMainParam:UiParam
    {
    }

    public partial class UiEnterMainView:UiView
    {

            public Btn btn_email;
            public Btn btn_news;
            public Btn btn_play;
            public Btn btn_mod;
            public Btn btn_lounge;
            public Btn btn_setting;
            public Btn btn_quit;
        public UiEnterMainView(UiHolder uiHolder):base(uiHolder)
        {

            btn_email = uiHolder.elementTrsLst[0].GetComponent<Btn>();
            btn_news = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            btn_play = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            btn_mod = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            btn_lounge = uiHolder.elementTrsLst[4].GetComponent<Btn>();
            btn_setting = uiHolder.elementTrsLst[5].GetComponent<Btn>();
            btn_quit = uiHolder.elementTrsLst[6].GetComponent<Btn>();
        }

    }
    public partial class UiEnterMainCtrl:UiCtrl
    {
        public UiEnterMainView view;
        public UiEnterMainModel model;
        public UiEnterMainParam param;
        

        public override void SetParam(UiParam param)
        {
            this.param = (UiEnterMainParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiEnterMainView(uiHolder);
            model=new UiEnterMainModel();


        }

    }
    public partial class UiEnterMainModel:UiModel
    {
        
    }
}
