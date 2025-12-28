
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;
using Z_Texture;
 
using UnityEngine.Video;
namespace Ui.ModStoryEventTriggerWindow

{







    public partial class UiPrmParam:UiParam
    {
    }

    public partial class UiPrmView:UiView
    {

            public GameObject go_prm;
            public Btn btn_edit;
            public Txt txt_;
        public UiPrmView(UiHolder uiHolder):base(uiHolder)
        {

            go_prm = uiHolder.elementTrsLst[0].gameObject;
            btn_edit = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            txt_ = uiHolder.elementTrsLst[2].GetComponent<Txt>();
        }

    }
    public partial class UiPrmCtrl:UiCtrl
    {
        public UiPrmView view;
        public UiPrmModel model;
        public UiPrmParam param;
        public UiModStoryEventTriggerWindowCtrl parent=>(UiModStoryEventTriggerWindowCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiPrmParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiPrmView(uiHolder);
            model=new UiPrmModel();


        }

    }
    public partial class UiPrmModel:UiModel
    {
        
    }



    public partial class UiEventParam:UiParam
    {
    }

    public partial class UiEventView:UiView
    {

            public GameObject go_event;
            public Sta sta_;
            public Btn btn_add;
            public Txt txt_;
            public Btn btn_delete;
            public Btn btn_edit;
        public UiEventView(UiHolder uiHolder):base(uiHolder)
        {

            go_event = uiHolder.elementTrsLst[0].gameObject;
            sta_ = uiHolder.elementTrsLst[1].GetComponent<Sta>();
            btn_add = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            txt_ = uiHolder.elementTrsLst[3].GetComponent<Txt>();
            btn_delete = uiHolder.elementTrsLst[4].GetComponent<Btn>();
            btn_edit = uiHolder.elementTrsLst[5].GetComponent<Btn>();
        }

    }
    public partial class UiEventCtrl:UiCtrl
    {
        public UiEventView view;
        public UiEventModel model;
        public UiEventParam param;
        public UiModStoryEventTriggerWindowCtrl parent=>(UiModStoryEventTriggerWindowCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiEventParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiEventView(uiHolder);
            model=new UiEventModel();


        }

    }
    public partial class UiEventModel:UiModel
    {
        
    }
    public partial class UiModStoryEventTriggerWindowParam:UiParam
    {
    }

    public partial class UiModStoryEventTriggerWindowView:UiView
    {

            public Btn btn_bbg;
            public GameObject go_params;
            public Txt txt_name;
            public Btn btn_delete;
            public ScrView scr_;
            public Btn btn_close;
            public GameObject go_prm;
            public UiPrmCtrl sub_prm;
            public GameObject go_event;
            public UiEventCtrl sub_event;
        public UiModStoryEventTriggerWindowView(UiHolder uiHolder):base(uiHolder)
        {

            btn_bbg = uiHolder.elementTrsLst[0].GetComponent<Btn>();
            go_params = uiHolder.elementTrsLst[1].gameObject;
            txt_name = uiHolder.elementTrsLst[2].GetComponent<Txt>();
            btn_delete = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            scr_ = uiHolder.elementTrsLst[4].GetComponent<ScrView>();
            btn_close = uiHolder.elementTrsLst[5].GetComponent<Btn>();
            go_prm = uiHolder.elementTrsLst[6].gameObject;
            sub_prm = (UiPrmCtrl) uiHolder.elementTrsLst[7].GetComponent<UiHolder>().ctrl;
            go_event = uiHolder.elementTrsLst[8].gameObject;
            sub_event = (UiEventCtrl) uiHolder.elementTrsLst[9].GetComponent<UiHolder>().ctrl;
        }

    }
    public partial class UiModStoryEventTriggerWindowCtrl:UiCtrl
    {
        public UiModStoryEventTriggerWindowView view;
        public UiModStoryEventTriggerWindowModel model;
        public UiModStoryEventTriggerWindowParam param;
        

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryEventTriggerWindowParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryEventTriggerWindowView(uiHolder);
            model=new UiModStoryEventTriggerWindowModel();


            view.sub_prm = new UiPrmCtrl();
            view.sub_prm.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
            view.sub_event = new UiEventCtrl();
            view.sub_event.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
        }

    }
    public partial class UiModStoryEventTriggerWindowModel:UiModel
    {
        
    }
}
