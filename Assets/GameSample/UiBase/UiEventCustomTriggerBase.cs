
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;
using Z_Texture;
 
using UnityEngine.Video;
namespace Ui.EventCustomTrigger

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
        public UiEventCustomTriggerCtrl parent=>(UiEventCustomTriggerCtrl)uiHolder.parent.ctrl;

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
    public partial class UiEventCustomTriggerParam:UiParam
    {
    }

    public partial class UiEventCustomTriggerView:UiView
    {

            public GameObject go_trigger;
            public UiTriggerCtrl sub_trigger;
        public UiEventCustomTriggerView(UiHolder uiHolder):base(uiHolder)
        {

            go_trigger = uiHolder.elementTrsLst[0].gameObject;
            sub_trigger = (UiTriggerCtrl) uiHolder.elementTrsLst[1].GetComponent<UiHolder>().ctrl;
        }

    }
    public partial class UiEventCustomTriggerCtrl:UiCtrl
    {
        public UiEventCustomTriggerView view;
        public UiEventCustomTriggerModel model;
        public UiEventCustomTriggerParam param;
        

        public override void SetParam(UiParam param)
        {
            this.param = (UiEventCustomTriggerParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiEventCustomTriggerView(uiHolder);
            model=new UiEventCustomTriggerModel();


            view.sub_trigger = new UiTriggerCtrl();
            view.sub_trigger.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
        }

    }
    public partial class UiEventCustomTriggerModel:UiModel
    {
        
    }
}
