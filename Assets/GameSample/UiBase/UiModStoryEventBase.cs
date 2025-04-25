
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

    public partial class UiSubLabelParam:UiParam
    {
    }

    public partial class UiSubLabelView:UiView
    {

            public GameObject go_item;
            public Img img_;
            public Btn btn_;
            public Sta sta_sel;
            public Txt txt_;
        public UiSubLabelView(UiHolder uiHolder):base(uiHolder)
        {

            go_item = uiHolder.elementTrsLst[0].gameObject;
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

    public partial class UiCmdParam:UiParam
    {
    }

    public partial class UiCmdView:UiView
    {

            public GameObject go_cmd;
            public Img img_;
            public Btn btn_;
            public Txt txt_;
        public UiCmdView(UiHolder uiHolder):base(uiHolder)
        {

            go_cmd = uiHolder.elementTrsLst[0].gameObject;
            img_ = uiHolder.elementTrsLst[1].GetComponent<Img>();
            btn_ = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            txt_ = uiHolder.elementTrsLst[3].GetComponent<Txt>();
        }

    }
    public partial class UiCmdCtrl:UiCtrl
    {
        public UiCmdView view;
        public UiCmdModel model;
        public UiCmdParam param;
        public UiModStoryEventCtrl parent=>(UiModStoryEventCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiCmdParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiCmdView(uiHolder);
            model=new UiCmdModel();


        }

    }
    public partial class UiCmdModel:UiModel
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
            public GameObject go_choose;
            public Btn btn_choose;
            public Txt txt_title;
            public GameObject go_close;
            public Btn btn_close;
            public Txt txt_choose;
            public GameObject go_cmds;
            public ScrView scr_cmds;
            public Ipt ipt_name;
            public Ipt ipt_label;
            public Ipt ipt_subLabel;
            public GameObject go_label;
            public UiLabelCtrl sub_Label;
            public GameObject go_subLabel;
            public UiSubLabelCtrl sub_SubLabel;
            public GameObject go_item;
            public UiSubLabelCtrl sub_SubLabel;
            public Txt txt_name;
            public Txt txt_label;
            public Txt txt_subLabel;
            public GameObject go_cmd;
            public UiCmdCtrl sub_Cmd;
        public UiModStoryEventView(UiHolder uiHolder):base(uiHolder)
        {

            go_labels = uiHolder.elementTrsLst[0].gameObject;
            scr_labels = uiHolder.elementTrsLst[1].GetComponent<ScrView>();
            go_subLabels = uiHolder.elementTrsLst[2].gameObject;
            scr_subLabels = uiHolder.elementTrsLst[3].GetComponent<ScrView>();
            go_items = uiHolder.elementTrsLst[4].gameObject;
            scr_items = uiHolder.elementTrsLst[5].GetComponent<ScrView>();
            sta_show = uiHolder.elementTrsLst[6].GetComponent<Sta>();
            go_choose = uiHolder.elementTrsLst[7].gameObject;
            btn_choose = uiHolder.elementTrsLst[8].GetComponent<Btn>();
            txt_title = uiHolder.elementTrsLst[9].GetComponent<Txt>();
            go_close = uiHolder.elementTrsLst[10].gameObject;
            btn_close = uiHolder.elementTrsLst[11].GetComponent<Btn>();
            txt_choose = uiHolder.elementTrsLst[12].GetComponent<Txt>();
            go_cmds = uiHolder.elementTrsLst[13].gameObject;
            scr_cmds = uiHolder.elementTrsLst[14].GetComponent<ScrView>();
            ipt_name = uiHolder.elementTrsLst[15].GetComponent<Ipt>();
            ipt_label = uiHolder.elementTrsLst[16].GetComponent<Ipt>();
            ipt_subLabel = uiHolder.elementTrsLst[17].GetComponent<Ipt>();
            go_label = uiHolder.elementTrsLst[18].gameObject;
            sub_Label = (UiLabelCtrl) uiHolder.elementTrsLst[19].GetComponent<UiHolder>().ctrl;
            go_subLabel = uiHolder.elementTrsLst[20].gameObject;
            sub_SubLabel = (UiSubLabelCtrl) uiHolder.elementTrsLst[21].GetComponent<UiHolder>().ctrl;
            go_item = uiHolder.elementTrsLst[22].gameObject;
            sub_SubLabel = (UiSubLabelCtrl) uiHolder.elementTrsLst[23].GetComponent<UiHolder>().ctrl;
            txt_name = uiHolder.elementTrsLst[24].GetComponent<Txt>();
            txt_label = uiHolder.elementTrsLst[25].GetComponent<Txt>();
            txt_subLabel = uiHolder.elementTrsLst[26].GetComponent<Txt>();
            go_cmd = uiHolder.elementTrsLst[27].gameObject;
            sub_Cmd = (UiCmdCtrl) uiHolder.elementTrsLst[28].GetComponent<UiHolder>().ctrl;
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
            view.sub_SubLabel = new UiSubLabelCtrl();
            view.sub_SubLabel.BindHolderRecursively(uiHolder.subUiHolderLst[2]);
            view.sub_Cmd = new UiCmdCtrl();
            view.sub_Cmd.BindHolderRecursively(uiHolder.subUiHolderLst[3]);
        }

    }
    public partial class UiModStoryEventModel:UiModel
    {
        
    }
}
