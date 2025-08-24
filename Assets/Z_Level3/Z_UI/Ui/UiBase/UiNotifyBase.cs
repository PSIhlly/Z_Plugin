
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;
namespace Ui.Notify

{







    public partial class UiTipParam:UiParam
    {
    }

    public partial class UiTipView:UiView
    {

            public Txt txt_;
        public UiTipView(UiHolder uiHolder):base(uiHolder)
        {

            txt_ = uiHolder.elementTrsLst[0].GetComponent<Txt>();
        }

    }
    public partial class UiTipCtrl:UiCtrl
    {
        public UiTipView view;
        public UiTipModel model;
        public UiTipParam param;
        public UiNotifyCtrl parent=>(UiNotifyCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiTipParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiTipView(uiHolder);
            model=new UiTipModel();


        }

    }
    public partial class UiTipModel:UiModel
    {
        
    }






    public partial class UiItemParam:UiParam
    {
    }

    public partial class UiItemView:UiView
    {

            public GameObject go_item;
            public Img img_;
            public Btn btn_;
            public Sta sta_sel;
            public Txt txt_;
        public UiItemView(UiHolder uiHolder):base(uiHolder)
        {

            go_item = uiHolder.elementTrsLst[0].gameObject;
            img_ = uiHolder.elementTrsLst[1].GetComponent<Img>();
            btn_ = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            sta_sel = uiHolder.elementTrsLst[3].GetComponent<Sta>();
            txt_ = uiHolder.elementTrsLst[4].GetComponent<Txt>();
        }

    }
    public partial class UiItemCtrl:UiCtrl
    {
        public UiItemView view;
        public UiItemModel model;
        public UiItemParam param;
        public UiChooseCtrl parent=>(UiChooseCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiItemParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiItemView(uiHolder);
            model=new UiItemModel();


        }

    }
    public partial class UiItemModel:UiModel
    {
        
    }
    public partial class UiChooseParam:UiParam
    {
    }

    public partial class UiChooseView:UiView
    {

            public GameObject go_items;
            public ScrView scr_items;
            public GameObject go_choose;
            public Btn btn_choose;
            public Txt txt_title;
            public GameObject go_close;
            public Btn btn_close;
            public GameObject go_item;
            public UiItemCtrl sub_Item;
        public UiChooseView(UiHolder uiHolder):base(uiHolder)
        {

            go_items = uiHolder.elementTrsLst[0].gameObject;
            scr_items = uiHolder.elementTrsLst[1].GetComponent<ScrView>();
            go_choose = uiHolder.elementTrsLst[2].gameObject;
            btn_choose = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            txt_title = uiHolder.elementTrsLst[4].GetComponent<Txt>();
            go_close = uiHolder.elementTrsLst[5].gameObject;
            btn_close = uiHolder.elementTrsLst[6].GetComponent<Btn>();
            go_item = uiHolder.elementTrsLst[7].gameObject;
            sub_Item = (UiItemCtrl) uiHolder.elementTrsLst[8].GetComponent<UiHolder>().ctrl;
        }

    }
    public partial class UiChooseCtrl:UiCtrl
    {
        public UiChooseView view;
        public UiChooseModel model;
        public UiChooseParam param;
        public UiNotifyCtrl parent=>(UiNotifyCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiChooseParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiChooseView(uiHolder);
            model=new UiChooseModel();


            view.sub_Item = new UiItemCtrl();
            view.sub_Item.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
        }

    }
    public partial class UiChooseModel:UiModel
    {
        
    }









    public partial class UiLabelParam:UiParam
    {
    }

    public partial class UiLabelView:UiView
    {

            public GameObject go_label;
            public Img img_;
            public Btn btn_;
            public Sta sta_sel;
            public Txt txt_;
        public UiLabelView(UiHolder uiHolder):base(uiHolder)
        {

            go_label = uiHolder.elementTrsLst[0].gameObject;
            img_ = uiHolder.elementTrsLst[1].GetComponent<Img>();
            btn_ = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            sta_sel = uiHolder.elementTrsLst[3].GetComponent<Sta>();
            txt_ = uiHolder.elementTrsLst[4].GetComponent<Txt>();
        }

    }
    public partial class UiLabelCtrl:UiCtrl
    {
        public UiLabelView view;
        public UiLabelModel model;
        public UiLabelParam param;
        public UiColumnCtrl parent=>(UiColumnCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiLabelParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiLabelView(uiHolder);
            model=new UiLabelModel();


        }

    }
    public partial class UiLabelModel:UiModel
    {
        
    }
    public partial class UiColumnParam:UiParam
    {
    }

    public partial class UiColumnView:UiView
    {

            public GameObject go_column;
            public GameObject go_labels;
            public ScrView scr_labels;
            public GameObject go_label;
            public UiLabelCtrl sub_Label;
        public UiColumnView(UiHolder uiHolder):base(uiHolder)
        {

            go_column = uiHolder.elementTrsLst[0].gameObject;
            go_labels = uiHolder.elementTrsLst[1].gameObject;
            scr_labels = uiHolder.elementTrsLst[2].GetComponent<ScrView>();
            go_label = uiHolder.elementTrsLst[3].gameObject;
            sub_Label = (UiLabelCtrl) uiHolder.elementTrsLst[4].GetComponent<UiHolder>().ctrl;
        }

    }
    public partial class UiColumnCtrl:UiCtrl
    {
        public UiColumnView view;
        public UiColumnModel model;
        public UiColumnParam param;
        public UiMultipleChooseCtrl parent=>(UiMultipleChooseCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiColumnParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiColumnView(uiHolder);
            model=new UiColumnModel();


            view.sub_Label = new UiLabelCtrl();
            view.sub_Label.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
        }

    }
    public partial class UiColumnModel:UiModel
    {
        
    }



    public partial class UiSubItemParam:UiParam
    {
    }

    public partial class UiSubItemView:UiView
    {

            public GameObject go_subItem;
            public Img img_;
            public Btn btn_;
            public Sta sta_sel;
            public Txt txt_;
        public UiSubItemView(UiHolder uiHolder):base(uiHolder)
        {

            go_subItem = uiHolder.elementTrsLst[0].gameObject;
            img_ = uiHolder.elementTrsLst[1].GetComponent<Img>();
            btn_ = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            sta_sel = uiHolder.elementTrsLst[3].GetComponent<Sta>();
            txt_ = uiHolder.elementTrsLst[4].GetComponent<Txt>();
        }

    }
    public partial class UiSubItemCtrl:UiCtrl
    {
        public UiSubItemView view;
        public UiSubItemModel model;
        public UiSubItemParam param;
        public UiMultipleChooseCtrl parent=>(UiMultipleChooseCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiSubItemParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiSubItemView(uiHolder);
            model=new UiSubItemModel();


        }

    }
    public partial class UiSubItemModel:UiModel
    {
        
    }
    public partial class UiMultipleChooseParam:UiParam
    {
    }

    public partial class UiMultipleChooseView:UiView
    {

            public RectTransform rtf_root;
            public Txt txt_title;
            public GameObject go_close;
            public Btn btn_close;
            public GameObject go_column;
            public UiColumnCtrl sub_Column;
            public GameObject go_choose;
            public Btn btn_choose;
            public GameObject go_subItems;
            public ScrView scr_subItems;
            public GameObject go_subItem;
            public UiSubItemCtrl sub_SubItem;
        public UiMultipleChooseView(UiHolder uiHolder):base(uiHolder)
        {

            rtf_root = uiHolder.elementTrsLst[0].GetComponent<RectTransform>();
            txt_title = uiHolder.elementTrsLst[1].GetComponent<Txt>();
            go_close = uiHolder.elementTrsLst[2].gameObject;
            btn_close = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            go_column = uiHolder.elementTrsLst[4].gameObject;
            sub_Column = (UiColumnCtrl) uiHolder.elementTrsLst[5].GetComponent<UiHolder>().ctrl;
            go_choose = uiHolder.elementTrsLst[6].gameObject;
            btn_choose = uiHolder.elementTrsLst[7].GetComponent<Btn>();
            go_subItems = uiHolder.elementTrsLst[8].gameObject;
            scr_subItems = uiHolder.elementTrsLst[9].GetComponent<ScrView>();
            go_subItem = uiHolder.elementTrsLst[10].gameObject;
            sub_SubItem = (UiSubItemCtrl) uiHolder.elementTrsLst[11].GetComponent<UiHolder>().ctrl;
        }

    }
    public partial class UiMultipleChooseCtrl:UiCtrl
    {
        public UiMultipleChooseView view;
        public UiMultipleChooseModel model;
        public UiMultipleChooseParam param;
        public UiNotifyCtrl parent=>(UiNotifyCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiMultipleChooseParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiMultipleChooseView(uiHolder);
            model=new UiMultipleChooseModel();


            view.sub_Column = new UiColumnCtrl();
            view.sub_Column.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
            view.sub_SubItem = new UiSubItemCtrl();
            view.sub_SubItem.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
        }

    }
    public partial class UiMultipleChooseModel:UiModel
    {
        
    }






    public partial class UiSelectionParam:UiParam
    {
    }

    public partial class UiSelectionView:UiView
    {

            public Btn btn_;
            public Txt txt_;
        public UiSelectionView(UiHolder uiHolder):base(uiHolder)
        {

            btn_ = uiHolder.elementTrsLst[0].GetComponent<Btn>();
            txt_ = uiHolder.elementTrsLst[1].GetComponent<Txt>();
        }

    }
    public partial class UiSelectionCtrl:UiCtrl
    {
        public UiSelectionView view;
        public UiSelectionModel model;
        public UiSelectionParam param;
        public UiPopupCtrl parent=>(UiPopupCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiSelectionParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiSelectionView(uiHolder);
            model=new UiSelectionModel();


        }

    }
    public partial class UiSelectionModel:UiModel
    {
        
    }
    public partial class UiPopupParam:UiParam
    {
    }

    public partial class UiPopupView:UiView
    {

            public Txt txt_content;
            public Txt txt_title;
            public GameObject go_close;
            public Btn btn_close;
            public UiSelectionCtrl sub_Selection;
            public Txt txt_;
        public UiPopupView(UiHolder uiHolder):base(uiHolder)
        {

            txt_content = uiHolder.elementTrsLst[0].GetComponent<Txt>();
            txt_title = uiHolder.elementTrsLst[1].GetComponent<Txt>();
            go_close = uiHolder.elementTrsLst[2].gameObject;
            btn_close = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            sub_Selection = (UiSelectionCtrl) uiHolder.elementTrsLst[4].GetComponent<UiHolder>().ctrl;
            txt_ = uiHolder.elementTrsLst[5].GetComponent<Txt>();
        }

    }
    public partial class UiPopupCtrl:UiCtrl
    {
        public UiPopupView view;
        public UiPopupModel model;
        public UiPopupParam param;
        public UiNotifyCtrl parent=>(UiNotifyCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiPopupParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiPopupView(uiHolder);
            model=new UiPopupModel();


            view.sub_Selection = new UiSelectionCtrl();
            view.sub_Selection.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
        }

    }
    public partial class UiPopupModel:UiModel
    {
        
    }



    public partial class UiInputAreaParam:UiParam
    {
    }

    public partial class UiInputAreaView:UiView
    {

            public Ipt ipt_;
            public Btn btn_;
            public Txt txt_title;
            public GameObject go_close;
            public Btn btn_close;
            public Txt txt_;
        public UiInputAreaView(UiHolder uiHolder):base(uiHolder)
        {

            ipt_ = uiHolder.elementTrsLst[0].GetComponent<Ipt>();
            btn_ = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            txt_title = uiHolder.elementTrsLst[2].GetComponent<Txt>();
            go_close = uiHolder.elementTrsLst[3].gameObject;
            btn_close = uiHolder.elementTrsLst[4].GetComponent<Btn>();
            txt_ = uiHolder.elementTrsLst[5].GetComponent<Txt>();
        }

    }
    public partial class UiInputAreaCtrl:UiCtrl
    {
        public UiInputAreaView view;
        public UiInputAreaModel model;
        public UiInputAreaParam param;
        public UiNotifyCtrl parent=>(UiNotifyCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiInputAreaParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiInputAreaView(uiHolder);
            model=new UiInputAreaModel();


        }

    }
    public partial class UiInputAreaModel:UiModel
    {
        
    }
    public partial class UiNotifyParam:UiParam
    {
    }

    public partial class UiNotifyView:UiView
    {

            public UiTipCtrl sub_Tip;
            public Btn btn_back;
            public GameObject go_block;
            public Btn btn_block;
            public UiChooseCtrl sub_Choose;
            public UiMultipleChooseCtrl sub_MultipleChoose;
            public UiPopupCtrl sub_Popup;
            public UiInputAreaCtrl sub_InputArea;
        public UiNotifyView(UiHolder uiHolder):base(uiHolder)
        {

            sub_Tip = (UiTipCtrl) uiHolder.elementTrsLst[0].GetComponent<UiHolder>().ctrl;
            btn_back = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            go_block = uiHolder.elementTrsLst[2].gameObject;
            btn_block = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            sub_Choose = (UiChooseCtrl) uiHolder.elementTrsLst[4].GetComponent<UiHolder>().ctrl;
            sub_MultipleChoose = (UiMultipleChooseCtrl) uiHolder.elementTrsLst[5].GetComponent<UiHolder>().ctrl;
            sub_Popup = (UiPopupCtrl) uiHolder.elementTrsLst[6].GetComponent<UiHolder>().ctrl;
            sub_InputArea = (UiInputAreaCtrl) uiHolder.elementTrsLst[7].GetComponent<UiHolder>().ctrl;
        }

    }
    public partial class UiNotifyCtrl:UiCtrl
    {
        public UiNotifyView view;
        public UiNotifyModel model;
        public UiNotifyParam param;
        

        public override void SetParam(UiParam param)
        {
            this.param = (UiNotifyParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiNotifyView(uiHolder);
            model=new UiNotifyModel();


            view.sub_Tip = new UiTipCtrl();
            view.sub_Tip.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
            view.sub_Choose = new UiChooseCtrl();
            view.sub_Choose.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
            view.sub_MultipleChoose = new UiMultipleChooseCtrl();
            view.sub_MultipleChoose.BindHolderRecursively(uiHolder.subUiHolderLst[2]);
            view.sub_Popup = new UiPopupCtrl();
            view.sub_Popup.BindHolderRecursively(uiHolder.subUiHolderLst[3]);
            view.sub_InputArea = new UiInputAreaCtrl();
            view.sub_InputArea.BindHolderRecursively(uiHolder.subUiHolderLst[4]);
        }

    }
    public partial class UiNotifyModel:UiModel
    {
        
    }
}
