
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;

namespace Ui
{



    public partial class UiToolItemParam:UiParam
    {
    }

    public partial class UiToolItemView:UiView
    {

            public GameObject go_toolItem;
            public Btn btn_tool;
            public Img img_;
            public Txt txt_name;
        public UiToolItemView(UiHolder uiHolder):base(uiHolder)
        {

            go_toolItem = uiHolder.elementTrsLst[0].gameObject;
            btn_tool = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            img_ = uiHolder.elementTrsLst[2].GetComponent<Img>();
            txt_name = uiHolder.elementTrsLst[3].GetComponent<Txt>();
        }

    }
    public partial class UiToolItemCtrl:UiCtrl
    {
        public UiToolItemView view;
        public UiToolItemModel model;
        public UiToolItemParam param;
        public UiModSceneMainCtrl parent=>(UiModSceneMainCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiToolItemParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiToolItemView(uiHolder);
            model=new UiToolItemModel();


        }

    }
    public partial class UiToolItemModel:UiModel
    {
        
    }
    public partial class UiModSceneMainParam:UiParam
    {
    }

    public partial class UiModSceneMainView:UiView
    {

            public Btn btn_menu;
            public GameObject go_toolBg;
            public Txt txt_menu;
            public Btn btn_showTool;
            public GameObject go_tool;
            public ScrView scr_tool;
            public Txt txt_showBg;
            public GameObject go_toolItem;
            public UiToolItemCtrl sub_ToolItem;
        public UiModSceneMainView(UiHolder uiHolder):base(uiHolder)
        {

            btn_menu = uiHolder.elementTrsLst[0].GetComponent<Btn>();
            go_toolBg = uiHolder.elementTrsLst[1].gameObject;
            txt_menu = uiHolder.elementTrsLst[2].GetComponent<Txt>();
            btn_showTool = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            go_tool = uiHolder.elementTrsLst[4].gameObject;
            scr_tool = uiHolder.elementTrsLst[5].GetComponent<ScrView>();
            txt_showBg = uiHolder.elementTrsLst[6].GetComponent<Txt>();
            go_toolItem = uiHolder.elementTrsLst[7].gameObject;
            sub_ToolItem = (UiToolItemCtrl) uiHolder.elementTrsLst[8].GetComponent<UiHolder>().ctrl;
        }

    }
    public partial class UiModSceneMainCtrl:UiCtrl
    {
        public UiModSceneMainView view;
        public UiModSceneMainModel model;
        public UiModSceneMainParam param;
        

        public override void SetParam(UiParam param)
        {
            this.param = (UiModSceneMainParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModSceneMainView(uiHolder);
            model=new UiModSceneMainModel();


            view.sub_ToolItem = new UiToolItemCtrl();
            view.sub_ToolItem.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
        }

    }
    public partial class UiModSceneMainModel:UiModel
    {
        
    }
}
