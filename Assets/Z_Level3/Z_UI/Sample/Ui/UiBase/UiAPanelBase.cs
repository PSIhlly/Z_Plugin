
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;

namespace Ui.APanel
{






    public partial class UiJBParam:UiParam
    {
    }

    public partial class UiJBView:UiView
    {

            public GameObject go_JB;
            public Img img_lj;
            public Btn btn_lj;
            public Txt txt_;
        public UiJBView(UiHolder uiHolder):base(uiHolder)
        {

            go_JB = uiHolder.elementTrsLst[0].gameObject;
            img_lj = uiHolder.elementTrsLst[1].GetComponent<Img>();
            btn_lj = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            txt_ = uiHolder.elementTrsLst[3].GetComponent<Txt>();
        }

    }
    public partial class UiJBCtrl:UiCtrl
    {
        public UiJBView view;
        public UiJBModel model;
        public UiJBParam param;
        public UiAPanelCtrl parent=>(UiAPanelCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiJBParam)param;
        }

        public override void BindHolder(UiHolder uiHolder)
        {


            base.BindHolder(uiHolder);

            view = new UiJBView(uiHolder);
            model=new UiJBModel();
        }

    }
    public partial class UiJBModel:UiModel
    {
        
    }

    public partial class UiAPanelParam:UiParam
    {
    }

    public partial class UiAPanelView:UiView
    {

            public Txt txt_ojbk;
            public Img img_aka;
            public ScrView scr_tt;
            public GameObject go_JB;
        public UiAPanelView(UiHolder uiHolder):base(uiHolder)
        {

            txt_ojbk = uiHolder.elementTrsLst[0].GetComponent<Txt>();
            img_aka = uiHolder.elementTrsLst[1].GetComponent<Img>();
            scr_tt = uiHolder.elementTrsLst[2].GetComponent<ScrView>();
            go_JB = uiHolder.elementTrsLst[3].gameObject;
        }

    }
    public partial class UiAPanelCtrl:UiCtrl
    {
        public UiAPanelView view;
        public UiAPanelModel model;
        public UiAPanelParam param;
        

        public override void SetParam(UiParam param)
        {
            this.param = (UiAPanelParam)param;
        }

        public override void BindHolder(UiHolder uiHolder)
        {


            base.BindHolder(uiHolder);

            view = new UiAPanelView(uiHolder);
            model=new UiAPanelModel();
        }

    }
    public partial class UiAPanelModel:UiModel
    {
        
    }
}
