
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;
using Z_Texture;using RenderHeads.Media.AVProVideo;
namespace Ui.TeamSelectWindow

{







    public partial class UiActiveTeamerParam:UiParam
    {
    }

    public partial class UiActiveTeamerView:UiView
    {

            public GameObject go_activeTeamer;
            public Sta sta_exist;
            public Btn btn_new;
            public Btn btn_;
            public Sta sta_;
            public Img img_;
            public Txt txt_;
        public UiActiveTeamerView(UiHolder uiHolder):base(uiHolder)
        {

            go_activeTeamer = uiHolder.elementTrsLst[0].gameObject;
            sta_exist = uiHolder.elementTrsLst[1].GetComponent<Sta>();
            btn_new = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            btn_ = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            sta_ = uiHolder.elementTrsLst[4].GetComponent<Sta>();
            img_ = uiHolder.elementTrsLst[5].GetComponent<Img>();
            txt_ = uiHolder.elementTrsLst[6].GetComponent<Txt>();
        }

    }
    public partial class UiActiveTeamerCtrl:UiCtrl
    {
        public UiActiveTeamerView view;
        public UiActiveTeamerModel model;
        public UiActiveTeamerParam param;
        public UiTeamSelectWindowCtrl parent=>(UiTeamSelectWindowCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiActiveTeamerParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiActiveTeamerView(uiHolder);
            model=new UiActiveTeamerModel();


        }

    }
    public partial class UiActiveTeamerModel:UiModel
    {
        
    }



    public partial class UiTeamerParam:UiParam
    {
    }

    public partial class UiTeamerView:UiView
    {

            public GameObject go_teamer;
            public Sta sta_exist;
            public Btn btn_new;
            public Btn btn_;
            public Sta sta_;
            public Img img_;
            public Txt txt_;
        public UiTeamerView(UiHolder uiHolder):base(uiHolder)
        {

            go_teamer = uiHolder.elementTrsLst[0].gameObject;
            sta_exist = uiHolder.elementTrsLst[1].GetComponent<Sta>();
            btn_new = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            btn_ = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            sta_ = uiHolder.elementTrsLst[4].GetComponent<Sta>();
            img_ = uiHolder.elementTrsLst[5].GetComponent<Img>();
            txt_ = uiHolder.elementTrsLst[6].GetComponent<Txt>();
        }

    }
    public partial class UiTeamerCtrl:UiCtrl
    {
        public UiTeamerView view;
        public UiTeamerModel model;
        public UiTeamerParam param;
        public UiTeamSelectWindowCtrl parent=>(UiTeamSelectWindowCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiTeamerParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiTeamerView(uiHolder);
            model=new UiTeamerModel();


        }

    }
    public partial class UiTeamerModel:UiModel
    {
        
    }
    public partial class UiTeamSelectWindowParam:UiParam
    {
    }

    public partial class UiTeamSelectWindowView:UiView
    {

            public Btn btn_bg;
            public ScrView scr_activeTeamers;
            public ScrView scr_teamers;
            public Btn btn_close;
            public GameObject go_activeTeamer;
            public UiActiveTeamerCtrl sub_activeTeamer;
            public GameObject go_teamer;
            public UiTeamerCtrl sub_teamer;
        public UiTeamSelectWindowView(UiHolder uiHolder):base(uiHolder)
        {

            btn_bg = uiHolder.elementTrsLst[0].GetComponent<Btn>();
            scr_activeTeamers = uiHolder.elementTrsLst[1].GetComponent<ScrView>();
            scr_teamers = uiHolder.elementTrsLst[2].GetComponent<ScrView>();
            btn_close = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            go_activeTeamer = uiHolder.elementTrsLst[4].gameObject;
            sub_activeTeamer = (UiActiveTeamerCtrl) uiHolder.elementTrsLst[5].GetComponent<UiHolder>().ctrl;
            go_teamer = uiHolder.elementTrsLst[6].gameObject;
            sub_teamer = (UiTeamerCtrl) uiHolder.elementTrsLst[7].GetComponent<UiHolder>().ctrl;
        }

    }
    public partial class UiTeamSelectWindowCtrl:UiCtrl
    {
        public UiTeamSelectWindowView view;
        public UiTeamSelectWindowModel model;
        public UiTeamSelectWindowParam param;
        

        public override void SetParam(UiParam param)
        {
            this.param = (UiTeamSelectWindowParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiTeamSelectWindowView(uiHolder);
            model=new UiTeamSelectWindowModel();


            view.sub_activeTeamer = new UiActiveTeamerCtrl();
            view.sub_activeTeamer.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
            view.sub_teamer = new UiTeamerCtrl();
            view.sub_teamer.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
        }

    }
    public partial class UiTeamSelectWindowModel:UiModel
    {
        
    }
}
