
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;
using Z_Texture;
using Z_Video;
using UnityEngine.Video;
namespace Ui.EventChoose

{




    public partial class UiEventChooseParam:UiParam
    {
    }

    public partial class UiEventChooseView:UiView
    {

            public Txt txt_name;
            public Btn btn_onEvent;
            public Btn btn_onEventTrigger;
            public Txt txt_onEvent;
            public Txt txt_onEventTrigger;
        public UiEventChooseView(UiHolder uiHolder):base(uiHolder)
        {

            txt_name = uiHolder.elementTrsLst[0].GetComponent<Txt>();
            btn_onEvent = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            btn_onEventTrigger = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            txt_onEvent = uiHolder.elementTrsLst[3].GetComponent<Txt>();
            txt_onEventTrigger = uiHolder.elementTrsLst[4].GetComponent<Txt>();
        }

    }
    public partial class UiEventChooseCtrl:UiCtrl
    {
        public UiEventChooseView view;
        public UiEventChooseModel model;
        public UiEventChooseParam param;
        

        public override void SetParam(UiParam param)
        {
            this.param = (UiEventChooseParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiEventChooseView(uiHolder);
            model=new UiEventChooseModel();


        }

    }
    public partial class UiEventChooseModel:UiModel
    {
        
    }
}
