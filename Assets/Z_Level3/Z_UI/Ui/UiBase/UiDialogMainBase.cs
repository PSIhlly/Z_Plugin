
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;
using Z_Texture;
using Z_Video;
using UnityEngine.Video;
namespace Ui.DialogMain

{




namespace MainText

{




    public partial class UiMainTextParam:UiParam
    {
    }

    public partial class UiMainTextView:UiView
    {

            public Img img_bg;
            public GameObject go_bg;
            public AudioSource as_;
            public Txt txt_;
        public UiMainTextView(UiHolder uiHolder):base(uiHolder)
        {

            img_bg = uiHolder.elementTrsLst[0].GetComponent<Img>();
            go_bg = uiHolder.elementTrsLst[1].gameObject;
            as_ = uiHolder.elementTrsLst[2].GetComponent<AudioSource>();
            txt_ = uiHolder.elementTrsLst[3].GetComponent<Txt>();
        }

    }
    public partial class UiMainTextCtrl:UiCtrl
    {
        public UiMainTextView view;
        public UiMainTextModel model;
        public UiMainTextParam param;
        public UiDialogMainCtrl parent=>(UiDialogMainCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiMainTextParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiMainTextView(uiHolder);
            model=new UiMainTextModel();


        }

    }
    public partial class UiMainTextModel:UiModel
    {
        
    }
}

namespace Title

{




    public partial class UiTitleParam:UiParam
    {
    }

    public partial class UiTitleView:UiView
    {

            public Img img_bg;
            public Txt txt_;
        public UiTitleView(UiHolder uiHolder):base(uiHolder)
        {

            img_bg = uiHolder.elementTrsLst[0].GetComponent<Img>();
            txt_ = uiHolder.elementTrsLst[1].GetComponent<Txt>();
        }

    }
    public partial class UiTitleCtrl:UiCtrl
    {
        public UiTitleView view;
        public UiTitleModel model;
        public UiTitleParam param;
        public UiDialogMainCtrl parent=>(UiDialogMainCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiTitleParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiTitleView(uiHolder);
            model=new UiTitleModel();


        }

    }
    public partial class UiTitleModel:UiModel
    {
        
    }
}

namespace Profile

{




    public partial class UiProfileParam:UiParam
    {
    }

    public partial class UiProfileView:UiView
    {

            public Img img_bg;
            public Img img_;
        public UiProfileView(UiHolder uiHolder):base(uiHolder)
        {

            img_bg = uiHolder.elementTrsLst[0].GetComponent<Img>();
            img_ = uiHolder.elementTrsLst[1].GetComponent<Img>();
        }

    }
    public partial class UiProfileCtrl:UiCtrl
    {
        public UiProfileView view;
        public UiProfileModel model;
        public UiProfileParam param;
        public UiDialogMainCtrl parent=>(UiDialogMainCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiProfileParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiProfileView(uiHolder);
            model=new UiProfileModel();


        }

    }
    public partial class UiProfileModel:UiModel
    {
        
    }
}

namespace Options

{




    public partial class UiOptionsParam:UiParam
    {
    }

    public partial class UiOptionsView:UiView
    {

            public Btn btn_over;
            public GameObject go_func;
            public Img img_func;
            public Btn btn_menu;
            public Img img_menu;
            public Btn btn_history;
            public Btn btn_hide;
            public Sta sta_skip;
            public Btn btn_skip;
            public Sta sta_autoPlay;
            public Btn btn_autoPlay;
        public UiOptionsView(UiHolder uiHolder):base(uiHolder)
        {

            btn_over = uiHolder.elementTrsLst[0].GetComponent<Btn>();
            go_func = uiHolder.elementTrsLst[1].gameObject;
            img_func = uiHolder.elementTrsLst[2].GetComponent<Img>();
            btn_menu = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            img_menu = uiHolder.elementTrsLst[4].GetComponent<Img>();
            btn_history = uiHolder.elementTrsLst[5].GetComponent<Btn>();
            btn_hide = uiHolder.elementTrsLst[6].GetComponent<Btn>();
            sta_skip = uiHolder.elementTrsLst[7].GetComponent<Sta>();
            btn_skip = uiHolder.elementTrsLst[8].GetComponent<Btn>();
            sta_autoPlay = uiHolder.elementTrsLst[9].GetComponent<Sta>();
            btn_autoPlay = uiHolder.elementTrsLst[10].GetComponent<Btn>();
        }

    }
    public partial class UiOptionsCtrl:UiCtrl
    {
        public UiOptionsView view;
        public UiOptionsModel model;
        public UiOptionsParam param;
        public UiDialogMainCtrl parent=>(UiDialogMainCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiOptionsParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiOptionsView(uiHolder);
            model=new UiOptionsModel();


        }

    }
    public partial class UiOptionsModel:UiModel
    {
        
    }
}

    public partial class UiDialogMainParam:UiParam
    {
    }

    public partial class UiDialogMainView:UiView
    {

            public Btn btn_back;
            public MainText.UiMainTextCtrl page_MainText;
            public Title.UiTitleCtrl page_Title;
            public Profile.UiProfileCtrl page_Profile;
            public Options.UiOptionsCtrl page_Options;
        public UiDialogMainView(UiHolder uiHolder):base(uiHolder)
        {

            btn_back = uiHolder.elementTrsLst[0].GetComponent<Btn>();
            page_MainText = (MainText.UiMainTextCtrl) uiHolder.elementTrsLst[1].GetComponent<UiHolder>().ctrl;
            page_Title = (Title.UiTitleCtrl) uiHolder.elementTrsLst[2].GetComponent<UiHolder>().ctrl;
            page_Profile = (Profile.UiProfileCtrl) uiHolder.elementTrsLst[3].GetComponent<UiHolder>().ctrl;
            page_Options = (Options.UiOptionsCtrl) uiHolder.elementTrsLst[4].GetComponent<UiHolder>().ctrl;
        }

    }
    public partial class UiDialogMainCtrl:UiCtrl
    {
        public UiDialogMainView view;
        public UiDialogMainModel model;
        public UiDialogMainParam param;
        

        public override void SetParam(UiParam param)
        {
            this.param = (UiDialogMainParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiDialogMainView(uiHolder);
            model=new UiDialogMainModel();


            view.page_MainText = new MainText.UiMainTextCtrl();
            view.page_MainText.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
            view.page_Title = new Title.UiTitleCtrl();
            view.page_Title.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
            view.page_Profile = new Profile.UiProfileCtrl();
            view.page_Profile.BindHolderRecursively(uiHolder.subUiHolderLst[2]);
            view.page_Options = new Options.UiOptionsCtrl();
            view.page_Options.BindHolderRecursively(uiHolder.subUiHolderLst[3]);
        }

    }
    public partial class UiDialogMainModel:UiModel
    {
        
    }
}
