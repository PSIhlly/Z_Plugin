
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;
using Z_Texture;using RenderHeads.Media.AVProVideo;
namespace Ui.ModSceneBehaviourUnit

{

using Ui.TriggerEventChoose;






    public partial class UiParamParam:UiParam
    {
    }

    public partial class UiParamView:UiView
    {

            public GameObject go_param;
            public Txt txt_;
            public Btn btn_value;
            public Txt txt_value;
        public UiParamView(UiHolder uiHolder):base(uiHolder)
        {

            go_param = uiHolder.elementTrsLst[0].gameObject;
            txt_ = uiHolder.elementTrsLst[1].GetComponent<Txt>();
            btn_value = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            txt_value = uiHolder.elementTrsLst[3].GetComponent<Txt>();
        }

    }
    public partial class UiParamCtrl:UiCtrl
    {
        public UiParamView view;
        public UiParamModel model;
        public UiParamParam param;
        public UiModSceneBehaviourUnitCtrl parent=>(UiModSceneBehaviourUnitCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiParamParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiParamView(uiHolder);
            model=new UiParamModel();


        }

    }
    public partial class UiParamModel:UiModel
    {
        
    }



    public partial class UiEventTriggerParam:UiParam
    {
    }

    public partial class UiEventTriggerView:UiView
    {

            public GameObject go_trigger;
            public Btn btn_;
            public Txt txt_;
        public UiEventTriggerView(UiHolder uiHolder):base(uiHolder)
        {

            go_trigger = uiHolder.elementTrsLst[0].gameObject;
            btn_ = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            txt_ = uiHolder.elementTrsLst[2].GetComponent<Txt>();
        }

    }
    public partial class UiEventTriggerCtrl:UiCtrl
    {
        public UiEventTriggerView view;
        public UiEventTriggerModel model;
        public UiEventTriggerParam param;
        public UiModSceneBehaviourUnitCtrl parent=>(UiModSceneBehaviourUnitCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiEventTriggerParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiEventTriggerView(uiHolder);
            model=new UiEventTriggerModel();


        }

    }
    public partial class UiEventTriggerModel:UiModel
    {
        
    }
    public partial class UiModSceneBehaviourUnitParam:UiParam
    {
    }

    public partial class UiModSceneBehaviourUnitView:UiView
    {

            public Btn btn_bbg;
            public Img img_;
            public Txt txt_name;
            public Btn btn_close;
            public Btn btn_delete;
            public GameObject go_triggerEventChoose;
            public UiTriggerEventChooseCtrl model_triggerEventChoose;
            public GameObject go_param;
            public UiParamCtrl sub_param;
            public GameObject go_trigger;
            public UiEventTriggerCtrl sub_trigger;
        public UiModSceneBehaviourUnitView(UiHolder uiHolder):base(uiHolder)
        {

            btn_bbg = uiHolder.elementTrsLst[0].GetComponent<Btn>();
            img_ = uiHolder.elementTrsLst[1].GetComponent<Img>();
            txt_name = uiHolder.elementTrsLst[2].GetComponent<Txt>();
            btn_close = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            btn_delete = uiHolder.elementTrsLst[4].GetComponent<Btn>();
            go_triggerEventChoose = uiHolder.elementTrsLst[5].gameObject;
            model_triggerEventChoose = (UiTriggerEventChooseCtrl) uiHolder.elementTrsLst[6].GetComponent<UiHolder>().ctrl;
            go_param = uiHolder.elementTrsLst[7].gameObject;
            sub_param = (UiParamCtrl) uiHolder.elementTrsLst[8].GetComponent<UiHolder>().ctrl;
            go_trigger = uiHolder.elementTrsLst[9].gameObject;
            sub_trigger = (UiEventTriggerCtrl) uiHolder.elementTrsLst[10].GetComponent<UiHolder>().ctrl;
        }

    }
    public partial class UiModSceneBehaviourUnitCtrl:UiCtrl
    {
        public UiModSceneBehaviourUnitView view;
        public UiModSceneBehaviourUnitModel model;
        public UiModSceneBehaviourUnitParam param;
        

        public override void SetParam(UiParam param)
        {
            this.param = (UiModSceneBehaviourUnitParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModSceneBehaviourUnitView(uiHolder);
            model=new UiModSceneBehaviourUnitModel();


            view.model_triggerEventChoose = new UiTriggerEventChooseCtrl();
            view.model_triggerEventChoose.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
            view.sub_param = new UiParamCtrl();
            view.sub_param.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
            view.sub_trigger = new UiEventTriggerCtrl();
            view.sub_trigger.BindHolderRecursively(uiHolder.subUiHolderLst[2]);
        }

    }
    public partial class UiModSceneBehaviourUnitModel:UiModel
    {
        
    }
}
