
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;

namespace Ui.ModStoryEventTrigger

{



    public partial class UiTriggerParam:UiParam
    {
    }

    public partial class UiTriggerView:UiView
    {

            public GameObject go_trigger;
            public Img img_;
            public Btn btn_;
            public Txt txt_triggerName;
            public Txt txt_eventName;
        public UiTriggerView(UiHolder uiHolder):base(uiHolder)
        {

            go_trigger = uiHolder.elementTrsLst[0].gameObject;
            img_ = uiHolder.elementTrsLst[1].GetComponent<Img>();
            btn_ = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            txt_triggerName = uiHolder.elementTrsLst[3].GetComponent<Txt>();
            txt_eventName = uiHolder.elementTrsLst[4].GetComponent<Txt>();
        }

    }
    public partial class UiTriggerCtrl:UiCtrl
    {
        public UiTriggerView view;
        public UiTriggerModel model;
        public UiTriggerParam param;
        public UiModStoryEventTriggerCtrl parent=>(UiModStoryEventTriggerCtrl)uiHolder.parent.ctrl;

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
    public partial class UiModStoryEventTriggerParam:UiParam
    {
    }

    public partial class UiModStoryEventTriggerView:UiView
    {

            public GameObject go_triggers;
            public ScrView scr_triggers;
            public Txt txt_title;
            public GameObject go_close;
            public Btn btn_close;
            public GameObject go_trigger;
            public UiTriggerCtrl sub_Trigger;
        public UiModStoryEventTriggerView(UiHolder uiHolder):base(uiHolder)
        {

            go_triggers = uiHolder.elementTrsLst[0].gameObject;
            scr_triggers = uiHolder.elementTrsLst[1].GetComponent<ScrView>();
            txt_title = uiHolder.elementTrsLst[2].GetComponent<Txt>();
            go_close = uiHolder.elementTrsLst[3].gameObject;
            btn_close = uiHolder.elementTrsLst[4].GetComponent<Btn>();
            go_trigger = uiHolder.elementTrsLst[5].gameObject;
            sub_Trigger = (UiTriggerCtrl) uiHolder.elementTrsLst[6].GetComponent<UiHolder>().ctrl;
        }

    }
    public partial class UiModStoryEventTriggerCtrl:UiCtrl
    {
        public UiModStoryEventTriggerView view;
        public UiModStoryEventTriggerModel model;
        public UiModStoryEventTriggerParam param;
        

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryEventTriggerParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryEventTriggerView(uiHolder);
            model=new UiModStoryEventTriggerModel();


            view.sub_Trigger = new UiTriggerCtrl();
            view.sub_Trigger.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
        }

    }
    public partial class UiModStoryEventTriggerModel:UiModel
    {
        
    }
}
