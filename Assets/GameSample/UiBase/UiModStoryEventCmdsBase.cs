
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;

namespace Ui.ModStoryEventCmds

{



    public partial class UiCmdParam:UiParam
    {
    }

    public partial class UiCmdView:UiView
    {

            public GameObject go_cmd;
            public Img img_add;
            public Btn btn_add;
            public Img img_del;
            public Btn btn_del;
            public Img img_name;
            public Btn btn_name;
            public Txt txt_add;
            public Txt txt_del;
            public Txt txt_name;
        public UiCmdView(UiHolder uiHolder):base(uiHolder)
        {

            go_cmd = uiHolder.elementTrsLst[0].gameObject;
            img_add = uiHolder.elementTrsLst[1].GetComponent<Img>();
            btn_add = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            img_del = uiHolder.elementTrsLst[3].GetComponent<Img>();
            btn_del = uiHolder.elementTrsLst[4].GetComponent<Btn>();
            img_name = uiHolder.elementTrsLst[5].GetComponent<Img>();
            btn_name = uiHolder.elementTrsLst[6].GetComponent<Btn>();
            txt_add = uiHolder.elementTrsLst[7].GetComponent<Txt>();
            txt_del = uiHolder.elementTrsLst[8].GetComponent<Txt>();
            txt_name = uiHolder.elementTrsLst[9].GetComponent<Txt>();
        }

    }
    public partial class UiCmdCtrl:UiCtrl
    {
        public UiCmdView view;
        public UiCmdModel model;
        public UiCmdParam param;
        public UiModStoryEventCmdsCtrl parent=>(UiModStoryEventCmdsCtrl)uiHolder.parent.ctrl;

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
    public partial class UiModStoryEventCmdsParam:UiParam
    {
    }

    public partial class UiModStoryEventCmdsView:UiView
    {

            public Ipt ipt_name;
            public Ipt ipt_label;
            public Ipt ipt_subLabel;
            public GameObject go_cmds;
            public ScrView scr_cmds;
            public Txt txt_title;
            public GameObject go_close;
            public Btn btn_close;
            public Txt txt_name;
            public Txt txt_label;
            public Txt txt_subLabel;
            public GameObject go_cmd;
            public UiCmdCtrl sub_Cmd;
        public UiModStoryEventCmdsView(UiHolder uiHolder):base(uiHolder)
        {

            ipt_name = uiHolder.elementTrsLst[0].GetComponent<Ipt>();
            ipt_label = uiHolder.elementTrsLst[1].GetComponent<Ipt>();
            ipt_subLabel = uiHolder.elementTrsLst[2].GetComponent<Ipt>();
            go_cmds = uiHolder.elementTrsLst[3].gameObject;
            scr_cmds = uiHolder.elementTrsLst[4].GetComponent<ScrView>();
            txt_title = uiHolder.elementTrsLst[5].GetComponent<Txt>();
            go_close = uiHolder.elementTrsLst[6].gameObject;
            btn_close = uiHolder.elementTrsLst[7].GetComponent<Btn>();
            txt_name = uiHolder.elementTrsLst[8].GetComponent<Txt>();
            txt_label = uiHolder.elementTrsLst[9].GetComponent<Txt>();
            txt_subLabel = uiHolder.elementTrsLst[10].GetComponent<Txt>();
            go_cmd = uiHolder.elementTrsLst[11].gameObject;
            sub_Cmd = (UiCmdCtrl) uiHolder.elementTrsLst[12].GetComponent<UiHolder>().ctrl;
        }

    }
    public partial class UiModStoryEventCmdsCtrl:UiCtrl
    {
        public UiModStoryEventCmdsView view;
        public UiModStoryEventCmdsModel model;
        public UiModStoryEventCmdsParam param;
        

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryEventCmdsParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryEventCmdsView(uiHolder);
            model=new UiModStoryEventCmdsModel();


            view.sub_Cmd = new UiCmdCtrl();
            view.sub_Cmd.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
        }

    }
    public partial class UiModStoryEventCmdsModel:UiModel
    {
        
    }
}
