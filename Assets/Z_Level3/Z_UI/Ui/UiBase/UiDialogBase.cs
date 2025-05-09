
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;

namespace Ui.Dialog

{




    public partial class UiHistoryItemParam:UiParam
    {
    }

    public partial class UiHistoryItemView:UiView
    {

            public GameObject go_historyItem;
            public Img img_ProfilePicture;
            public Txt txt_title;
            public Txt txt_mainText;
        public UiHistoryItemView(UiHolder uiHolder):base(uiHolder)
        {

            go_historyItem = uiHolder.elementTrsLst[0].gameObject;
            img_ProfilePicture = uiHolder.elementTrsLst[1].GetComponent<Img>();
            txt_title = uiHolder.elementTrsLst[2].GetComponent<Txt>();
            txt_mainText = uiHolder.elementTrsLst[3].GetComponent<Txt>();
        }

    }
    public partial class UiHistoryItemCtrl:UiCtrl
    {
        public UiHistoryItemView view;
        public UiHistoryItemModel model;
        public UiHistoryItemParam param;
        public UiHistoryCtrl parent=>(UiHistoryCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiHistoryItemParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiHistoryItemView(uiHolder);
            model=new UiHistoryItemModel();


        }

    }
    public partial class UiHistoryItemModel:UiModel
    {
        
    }
    public partial class UiHistoryParam:UiParam
    {
    }

    public partial class UiHistoryView:UiView
    {

            public GameObject go_history;
            public Btn btn_back;
            public ScrView scr_tt;
            public GameObject go_historyItem;
            public UiHistoryItemCtrl sub_HistoryItem;
        public UiHistoryView(UiHolder uiHolder):base(uiHolder)
        {

            go_history = uiHolder.elementTrsLst[0].gameObject;
            btn_back = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            scr_tt = uiHolder.elementTrsLst[2].GetComponent<ScrView>();
            go_historyItem = uiHolder.elementTrsLst[3].gameObject;
            sub_HistoryItem = (UiHistoryItemCtrl) uiHolder.elementTrsLst[4].GetComponent<UiHolder>().ctrl;
        }

    }
    public partial class UiHistoryCtrl:UiCtrl
    {
        public UiHistoryView view;
        public UiHistoryModel model;
        public UiHistoryParam param;
        public UiDialogCtrl parent=>(UiDialogCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiHistoryParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiHistoryView(uiHolder);
            model=new UiHistoryModel();


            view.sub_HistoryItem = new UiHistoryItemCtrl();
            view.sub_HistoryItem.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
        }

    }
    public partial class UiHistoryModel:UiModel
    {
        
    }

    public partial class UiMainPictureParam:UiParam
    {
    }

    public partial class UiMainPictureView:UiView
    {

            public GameObject go_mainPicture;
            public Img img_;
        public UiMainPictureView(UiHolder uiHolder):base(uiHolder)
        {

            go_mainPicture = uiHolder.elementTrsLst[0].gameObject;
            img_ = uiHolder.elementTrsLst[1].GetComponent<Img>();
        }

    }
    public partial class UiMainPictureCtrl:UiCtrl
    {
        public UiMainPictureView view;
        public UiMainPictureModel model;
        public UiMainPictureParam param;
        public UiDialogCtrl parent=>(UiDialogCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiMainPictureParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiMainPictureView(uiHolder);
            model=new UiMainPictureModel();


        }

    }
    public partial class UiMainPictureModel:UiModel
    {
        
    }

    public partial class UiFuncParam:UiParam
    {
    }

    public partial class UiFuncView:UiView
    {

            public GameObject go_func;
            public Sta sta_autoPlay;
            public Btn btn_autoPlay;
            public Sta sta_skip;
            public Btn btn_skip;
            public Btn btn_hide;
            public Btn btn_history;
        public UiFuncView(UiHolder uiHolder):base(uiHolder)
        {

            go_func = uiHolder.elementTrsLst[0].gameObject;
            sta_autoPlay = uiHolder.elementTrsLst[1].GetComponent<Sta>();
            btn_autoPlay = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            sta_skip = uiHolder.elementTrsLst[3].GetComponent<Sta>();
            btn_skip = uiHolder.elementTrsLst[4].GetComponent<Btn>();
            btn_hide = uiHolder.elementTrsLst[5].GetComponent<Btn>();
            btn_history = uiHolder.elementTrsLst[6].GetComponent<Btn>();
        }

    }
    public partial class UiFuncCtrl:UiCtrl
    {
        public UiFuncView view;
        public UiFuncModel model;
        public UiFuncParam param;
        public UiDialogCtrl parent=>(UiDialogCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiFuncParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiFuncView(uiHolder);
            model=new UiFuncModel();


        }

    }
    public partial class UiFuncModel:UiModel
    {
        
    }

    public partial class UiTitleParam:UiParam
    {
    }

    public partial class UiTitleView:UiView
    {

            public GameObject go_title;
            public Txt txt_;
        public UiTitleView(UiHolder uiHolder):base(uiHolder)
        {

            go_title = uiHolder.elementTrsLst[0].gameObject;
            txt_ = uiHolder.elementTrsLst[1].GetComponent<Txt>();
        }

    }
    public partial class UiTitleCtrl:UiCtrl
    {
        public UiTitleView view;
        public UiTitleModel model;
        public UiTitleParam param;
        public UiDialogCtrl parent=>(UiDialogCtrl)uiHolder.parent.ctrl;

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

    public partial class UiMainTextParam:UiParam
    {
    }

    public partial class UiMainTextView:UiView
    {

            public GameObject go_mainText;
            public Txt txt_;
            public Btn btn_over;
        public UiMainTextView(UiHolder uiHolder):base(uiHolder)
        {

            go_mainText = uiHolder.elementTrsLst[0].gameObject;
            txt_ = uiHolder.elementTrsLst[1].GetComponent<Txt>();
            btn_over = uiHolder.elementTrsLst[2].GetComponent<Btn>();
        }

    }
    public partial class UiMainTextCtrl:UiCtrl
    {
        public UiMainTextView view;
        public UiMainTextModel model;
        public UiMainTextParam param;
        public UiDialogCtrl parent=>(UiDialogCtrl)uiHolder.parent.ctrl;

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

    public partial class UiProfilePictureParam:UiParam
    {
    }

    public partial class UiProfilePictureView:UiView
    {

            public GameObject go_ProfilePicture;
            public Img img_;
        public UiProfilePictureView(UiHolder uiHolder):base(uiHolder)
        {

            go_ProfilePicture = uiHolder.elementTrsLst[0].gameObject;
            img_ = uiHolder.elementTrsLst[1].GetComponent<Img>();
        }

    }
    public partial class UiProfilePictureCtrl:UiCtrl
    {
        public UiProfilePictureView view;
        public UiProfilePictureModel model;
        public UiProfilePictureParam param;
        public UiDialogCtrl parent=>(UiDialogCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiProfilePictureParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiProfilePictureView(uiHolder);
            model=new UiProfilePictureModel();


        }

    }
    public partial class UiProfilePictureModel:UiModel
    {
        
    }
    public partial class UiDialogParam:UiParam
    {
    }

    public partial class UiDialogView:UiView
    {

            public GameObject go_normal;
            public Btn btn_;
            public GameObject go_history;
            public UiHistoryCtrl sub_History;
            public GameObject go_mainPicture;
            public UiMainPictureCtrl sub_MainPicture;
            public GameObject go_op;
            public Btn btn_back;
            public GameObject go_func;
            public UiFuncCtrl sub_Func;
            public GameObject go_title;
            public UiTitleCtrl sub_Title;
            public GameObject go_mainText;
            public UiMainTextCtrl sub_MainText;
            public GameObject go_ProfilePicture;
            public UiProfilePictureCtrl sub_ProfilePicture;
        public UiDialogView(UiHolder uiHolder):base(uiHolder)
        {

            go_normal = uiHolder.elementTrsLst[0].gameObject;
            btn_ = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            go_history = uiHolder.elementTrsLst[2].gameObject;
            sub_History = (UiHistoryCtrl) uiHolder.elementTrsLst[3].GetComponent<UiHolder>().ctrl;
            go_mainPicture = uiHolder.elementTrsLst[4].gameObject;
            sub_MainPicture = (UiMainPictureCtrl) uiHolder.elementTrsLst[5].GetComponent<UiHolder>().ctrl;
            go_op = uiHolder.elementTrsLst[6].gameObject;
            btn_back = uiHolder.elementTrsLst[7].GetComponent<Btn>();
            go_func = uiHolder.elementTrsLst[8].gameObject;
            sub_Func = (UiFuncCtrl) uiHolder.elementTrsLst[9].GetComponent<UiHolder>().ctrl;
            go_title = uiHolder.elementTrsLst[10].gameObject;
            sub_Title = (UiTitleCtrl) uiHolder.elementTrsLst[11].GetComponent<UiHolder>().ctrl;
            go_mainText = uiHolder.elementTrsLst[12].gameObject;
            sub_MainText = (UiMainTextCtrl) uiHolder.elementTrsLst[13].GetComponent<UiHolder>().ctrl;
            go_ProfilePicture = uiHolder.elementTrsLst[14].gameObject;
            sub_ProfilePicture = (UiProfilePictureCtrl) uiHolder.elementTrsLst[15].GetComponent<UiHolder>().ctrl;
        }

    }
    public partial class UiDialogCtrl:UiCtrl
    {
        public UiDialogView view;
        public UiDialogModel model;
        public UiDialogParam param;
        

        public override void SetParam(UiParam param)
        {
            this.param = (UiDialogParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiDialogView(uiHolder);
            model=new UiDialogModel();


            view.sub_History = new UiHistoryCtrl();
            view.sub_History.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
            view.sub_MainPicture = new UiMainPictureCtrl();
            view.sub_MainPicture.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
            view.sub_Func = new UiFuncCtrl();
            view.sub_Func.BindHolderRecursively(uiHolder.subUiHolderLst[2]);
            view.sub_Title = new UiTitleCtrl();
            view.sub_Title.BindHolderRecursively(uiHolder.subUiHolderLst[3]);
            view.sub_MainText = new UiMainTextCtrl();
            view.sub_MainText.BindHolderRecursively(uiHolder.subUiHolderLst[4]);
            view.sub_ProfilePicture = new UiProfilePictureCtrl();
            view.sub_ProfilePicture.BindHolderRecursively(uiHolder.subUiHolderLst[5]);
        }

    }
    public partial class UiDialogModel:UiModel
    {
        
    }
}
