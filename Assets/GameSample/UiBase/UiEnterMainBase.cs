
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;

namespace Ui.EnterMain

{


    public partial class UiEnterMainParam:UiParam
    {
    }

    public partial class UiEnterMainView:UiView
    {

            public Btn btn_news;
            public Btn btn_start;
            public Btn btn_warRoom;
            public Btn btn_setting;
            public Btn btn_quit;
            public Btn btn_email;
        public UiEnterMainView(UiHolder uiHolder):base(uiHolder)
        {

            btn_news = uiHolder.elementTrsLst[0].GetComponent<Btn>();
            btn_start = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            btn_warRoom = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            btn_setting = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            btn_quit = uiHolder.elementTrsLst[4].GetComponent<Btn>();
            btn_email = uiHolder.elementTrsLst[5].GetComponent<Btn>();
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
