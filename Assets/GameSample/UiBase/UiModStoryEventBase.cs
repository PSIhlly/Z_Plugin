
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;

namespace Ui.ModStoryEvent

{



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
        public UiModStoryEventCtrl parent=>(UiModStoryEventCtrl)uiHolder.parent.ctrl;

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

    public partial class UiSubLabelParam:UiParam
    {
    }

    public partial class UiSubLabelView:UiView
    {

            public GameObject go_subLabel;
            public Img img_;
            public Btn btn_;
            public Sta sta_sel;
            public Txt txt_;
        public UiSubLabelView(UiHolder uiHolder):base(uiHolder)
        {

            go_subLabel = uiHolder.elementTrsLst[0].gameObject;
            img_ = uiHolder.elementTrsLst[1].GetComponent<Img>();
            btn_ = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            sta_sel = uiHolder.elementTrsLst[3].GetComponent<Sta>();
            txt_ = uiHolder.elementTrsLst[4].GetComponent<Txt>();
        }

    }
    public partial class UiSubLabelCtrl:UiCtrl
    {
        public UiSubLabelView view;
        public UiSubLabelModel model;
        public UiSubLabelParam param;
        public UiModStoryEventCtrl parent=>(UiModStoryEventCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiSubLabelParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiSubLabelView(uiHolder);
            model=new UiSubLabelModel();


        }

    }
    public partial class UiSubLabelModel:UiModel
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
        public UiModStoryEventCtrl parent=>(UiModStoryEventCtrl)uiHolder.parent.ctrl;

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
    public partial class UiModStoryEventParam:UiParam
    {
    }

    public partial class UiModStoryEventView:UiView
    {

            public GameObject go_labels;
            public ScrView scr_labels;
            public GameObject go_subLabels;
            public ScrView scr_subLabels;
            public GameObject go_items;
            public ScrView scr_items;
            public Sta sta_show;
            public Txt txt_title;
            public GameObject go_close;
            public Btn btn_close;
            public GameObject go_edit;
            public Btn btn_edit;
            public GameObject go_choose;
            public Btn btn_choose;
            public GameObject go_label;
            public UiLabelCtrl sub_Label;
            public GameObject go_subLabel;
            public UiSubLabelCtrl sub_SubLabel;
            public GameObject go_item;
            public UiItemCtrl sub_Item;
            public Txt txt_content;
            public Txt txt_edit;
            public Txt txt_choose;
        public UiModStoryEventView(UiHolder uiHolder):base(uiHolder)
        {

            go_labels = uiHolder.elementTrsLst[0].gameObject;
            scr_labels = uiHolder.elementTrsLst[1].GetComponent<ScrView>();
            go_subLabels = uiHolder.elementTrsLst[2].gameObject;
            scr_subLabels = uiHolder.elementTrsLst[3].GetComponent<ScrView>();
            go_items = uiHolder.elementTrsLst[4].gameObject;
            scr_items = uiHolder.elementTrsLst[5].GetComponent<ScrView>();
            sta_show = uiHolder.elementTrsLst[6].GetComponent<Sta>();
            txt_title = uiHolder.elementTrsLst[7].GetComponent<Txt>();
            go_close = uiHolder.elementTrsLst[8].gameObject;
            btn_close = uiHolder.elementTrsLst[9].GetComponent<Btn>();
            go_edit = uiHolder.elementTrsLst[10].gameObject;
            btn_edit = uiHolder.elementTrsLst[11].GetComponent<Btn>();
            go_choose = uiHolder.elementTrsLst[12].gameObject;
            btn_choose = uiHolder.elementTrsLst[13].GetComponent<Btn>();
            go_label = uiHolder.elementTrsLst[14].gameObject;
            sub_Label = (UiLabelCtrl) uiHolder.elementTrsLst[15].GetComponent<UiHolder>().ctrl;
            go_subLabel = uiHolder.elementTrsLst[16].gameObject;
            sub_SubLabel = (UiSubLabelCtrl) uiHolder.elementTrsLst[17].GetComponent<UiHolder>().ctrl;
            go_item = uiHolder.elementTrsLst[18].gameObject;
            sub_Item = (UiItemCtrl) uiHolder.elementTrsLst[19].GetComponent<UiHolder>().ctrl;
            txt_content = uiHolder.elementTrsLst[20].GetComponent<Txt>();
            txt_edit = uiHolder.elementTrsLst[21].GetComponent<Txt>();
            txt_choose = uiHolder.elementTrsLst[22].GetComponent<Txt>();
        }

    }
    public partial class UiModStoryEventCtrl:UiCtrl
    {
        public UiModStoryEventView view;
        public UiModStoryEventModel model;
        public UiModStoryEventParam param;
        

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryEventParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryEventView(uiHolder);
            model=new UiModStoryEventModel();


            view.sub_Label = new UiLabelCtrl();
            view.sub_Label.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
            view.sub_SubLabel = new UiSubLabelCtrl();
            view.sub_SubLabel.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
            view.sub_Item = new UiItemCtrl();
            view.sub_Item.BindHolderRecursively(uiHolder.subUiHolderLst[2]);
        }

    }
    public partial class UiModStoryEventModel:UiModel
    {
        
    }
}
