
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;
using Z_Texture;using RenderHeads.Media.AVProVideo;
namespace Ui.TriggerEventChoose

{







    public partial class UiTriggerParam:UiParam
    {
    }

    public partial class UiTriggerView:UiView
    {

            public GameObject go_trigger;
            public Btn btn_;
            public Sta sta_;
            public Txt txt_;
        public UiTriggerView(UiHolder uiHolder):base(uiHolder)
        {

            go_trigger = uiHolder.elementTrsLst[0].gameObject;
            btn_ = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            sta_ = uiHolder.elementTrsLst[2].GetComponent<Sta>();
            txt_ = uiHolder.elementTrsLst[3].GetComponent<Txt>();
        }

    }
    public partial class UiTriggerCtrl:UiCtrl
    {
        public UiTriggerView view;
        public UiTriggerModel model;
        public UiTriggerParam param;
        public UiTriggerEventChooseCtrl parent=>(UiTriggerEventChooseCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiTriggerParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiTriggerView(uiHolder);
            model=new UiTriggerModel();


        }

    }
    public partial class UiTriggerModel:UiModel
    {
        
    }
    public partial class UiTriggerEventChooseParam:UiParam
    {
    }

    public partial class UiTriggerEventChooseView:UiView
    {

            public GameObject go_trigger;
            public UiTriggerCtrl sub_trigger;
        public UiTriggerEventChooseView(UiHolder uiHolder):base(uiHolder)
        {

            go_trigger = uiHolder.elementTrsLst[0].gameObject;
            sub_trigger = (UiTriggerCtrl) uiHolder.elementTrsLst[1].GetComponent<UiHolder>().ctrl;
        }

    }
    public partial class UiTriggerEventChooseCtrl:UiCtrl
    {
        public UiTriggerEventChooseView view;
        public UiTriggerEventChooseModel model;
        public UiTriggerEventChooseParam param;
        

        public override void SetParam(UiParam param)
        {
            this.param = (UiTriggerEventChooseParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiTriggerEventChooseView(uiHolder);
            model=new UiTriggerEventChooseModel();


            view.sub_trigger = new UiTriggerCtrl();
            view.sub_trigger.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
        }

    }
    public partial class UiTriggerEventChooseModel:UiModel
    {
        
    }
}
