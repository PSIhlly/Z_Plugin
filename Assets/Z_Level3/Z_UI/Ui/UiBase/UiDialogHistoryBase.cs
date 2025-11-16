
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;
namespace Ui.DialogHistory

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
        public UiDialogHistoryCtrl parent=>(UiDialogHistoryCtrl)uiHolder.parent.ctrl;

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
    public partial class UiDialogHistoryParam:UiParam
    {
    }

    public partial class UiDialogHistoryView:UiView
    {

            public Btn btn_back;
            public ScrView scr_tt;
            public GameObject go_historyItem;
            public UiHistoryItemCtrl sub_HistoryItem;
        public UiDialogHistoryView(UiHolder uiHolder):base(uiHolder)
        {

            btn_back = uiHolder.elementTrsLst[0].GetComponent<Btn>();
            scr_tt = uiHolder.elementTrsLst[1].GetComponent<ScrView>();
            go_historyItem = uiHolder.elementTrsLst[2].gameObject;
            sub_HistoryItem = (UiHistoryItemCtrl) uiHolder.elementTrsLst[3].GetComponent<UiHolder>().ctrl;
        }

    }
    public partial class UiDialogHistoryCtrl:UiCtrl
    {
        public UiDialogHistoryView view;
        public UiDialogHistoryModel model;
        public UiDialogHistoryParam param;
        

        public override void SetParam(UiParam param)
        {
            this.param = (UiDialogHistoryParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiDialogHistoryView(uiHolder);
            model=new UiDialogHistoryModel();


            view.sub_HistoryItem = new UiHistoryItemCtrl();
            view.sub_HistoryItem.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
        }

    }
    public partial class UiDialogHistoryModel:UiModel
    {
        
    }
}
