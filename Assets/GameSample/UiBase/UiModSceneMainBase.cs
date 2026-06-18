
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;
using Z_Texture;using RenderHeads.Media.AVProVideo;
namespace Ui.ModSceneMain

{




namespace ModTool

{







    public partial class UiLabParam:UiParam
    {
    }

    public partial class UiLabView:UiView
    {

            public GameObject go_lab;
            public Btn btn_;
            public Sta sta_;
            public Sta sta_valid;
            public Txt txt_;
        public UiLabView(UiHolder uiHolder):base(uiHolder)
        {

            go_lab = uiHolder.elementTrsLst[0].gameObject;
            btn_ = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            sta_ = uiHolder.elementTrsLst[2].GetComponent<Sta>();
            sta_valid = uiHolder.elementTrsLst[3].GetComponent<Sta>();
            txt_ = uiHolder.elementTrsLst[4].GetComponent<Txt>();
        }

    }
    public partial class UiLabCtrl:UiCtrl
    {
        public UiLabView view;
        public UiLabModel model;
        public UiLabParam param;
        public UiModToolCtrl parent=>(UiModToolCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiLabParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiLabView(uiHolder);
            model=new UiLabModel();


        }

    }
    public partial class UiLabModel:UiModel
    {
        
    }



    public partial class UiToolItemParam:UiParam
    {
    }

    public partial class UiToolItemView:UiView
    {

            public GameObject go_toolItem;
            public Sta sta_exist;
            public Btn btn_new;
            public Btn btn_;
            public Sta sta_;
            public Img img_;
            public Txt txt_;
        public UiToolItemView(UiHolder uiHolder):base(uiHolder)
        {

            go_toolItem = uiHolder.elementTrsLst[0].gameObject;
            sta_exist = uiHolder.elementTrsLst[1].GetComponent<Sta>();
            btn_new = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            btn_ = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            sta_ = uiHolder.elementTrsLst[4].GetComponent<Sta>();
            img_ = uiHolder.elementTrsLst[5].GetComponent<Img>();
            txt_ = uiHolder.elementTrsLst[6].GetComponent<Txt>();
        }

    }
    public partial class UiToolItemCtrl:UiCtrl
    {
        public UiToolItemView view;
        public UiToolItemModel model;
        public UiToolItemParam param;
        public UiModToolCtrl parent=>(UiModToolCtrl)uiHolder.parent.ctrl;

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



    public partial class UiToolTypeItemParam:UiParam
    {
    }

    public partial class UiToolTypeItemView:UiView
    {

            public GameObject go_toolTypeItem;
            public Sta sta_exist;
            public Btn btn_new;
            public Btn btn_;
            public Sta sta_;
            public Img img_;
            public Txt txt_;
        public UiToolTypeItemView(UiHolder uiHolder):base(uiHolder)
        {

            go_toolTypeItem = uiHolder.elementTrsLst[0].gameObject;
            sta_exist = uiHolder.elementTrsLst[1].GetComponent<Sta>();
            btn_new = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            btn_ = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            sta_ = uiHolder.elementTrsLst[4].GetComponent<Sta>();
            img_ = uiHolder.elementTrsLst[5].GetComponent<Img>();
            txt_ = uiHolder.elementTrsLst[6].GetComponent<Txt>();
        }

    }
    public partial class UiToolTypeItemCtrl:UiCtrl
    {
        public UiToolTypeItemView view;
        public UiToolTypeItemModel model;
        public UiToolTypeItemParam param;
        public UiModToolCtrl parent=>(UiModToolCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiToolTypeItemParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiToolTypeItemView(uiHolder);
            model=new UiToolTypeItemModel();


        }

    }
    public partial class UiToolTypeItemModel:UiModel
    {
        
    }
    public partial class UiModToolParam:UiParam
    {
    }

    public partial class UiModToolView:UiView
    {

            public GameObject go_toolButtonPos;
            public Btn btn_showTool;
            public ScrView scr_labs;
            public ScrView scr_tool;
            public GameObject go_toolContentPos;
            public ScrView scr_toolType;
            public GameObject go_layer;
            public GameObject go_lab;
            public UiLabCtrl sub_lab;
            public GameObject go_toolItem;
            public UiToolItemCtrl sub_toolItem;
            public Btn btn_layer0;
            public Sta sta_layer0;
            public Btn btn_layer1;
            public Sta sta_layer1;
            public Btn btn_layer2;
            public Sta sta_layer2;
            public Ipt ipt_rotateSet;
            public Btn btn_rotate;
            public Ipt ipt_posSetX;
            public Ipt ipt_posSetZ;
            public Ipt ipt_posSetY;
            public Btn btn_align;
            public Sta sta_align;
            public Btn btn_resetCount;
            public Ipt ipt_cntSetX;
            public Ipt ipt_cntSetY;
            public GameObject go_toolTypeItem;
            public UiToolTypeItemCtrl sub_toolTypeItem;
        public UiModToolView(UiHolder uiHolder):base(uiHolder)
        {

            go_toolButtonPos = uiHolder.elementTrsLst[0].gameObject;
            btn_showTool = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            scr_labs = uiHolder.elementTrsLst[2].GetComponent<ScrView>();
            scr_tool = uiHolder.elementTrsLst[3].GetComponent<ScrView>();
            go_toolContentPos = uiHolder.elementTrsLst[4].gameObject;
            scr_toolType = uiHolder.elementTrsLst[5].GetComponent<ScrView>();
            go_layer = uiHolder.elementTrsLst[6].gameObject;
            go_lab = uiHolder.elementTrsLst[7].gameObject;
            sub_lab = (UiLabCtrl) uiHolder.elementTrsLst[8].GetComponent<UiHolder>().ctrl;
            go_toolItem = uiHolder.elementTrsLst[9].gameObject;
            sub_toolItem = (UiToolItemCtrl) uiHolder.elementTrsLst[10].GetComponent<UiHolder>().ctrl;
            btn_layer0 = uiHolder.elementTrsLst[11].GetComponent<Btn>();
            sta_layer0 = uiHolder.elementTrsLst[12].GetComponent<Sta>();
            btn_layer1 = uiHolder.elementTrsLst[13].GetComponent<Btn>();
            sta_layer1 = uiHolder.elementTrsLst[14].GetComponent<Sta>();
            btn_layer2 = uiHolder.elementTrsLst[15].GetComponent<Btn>();
            sta_layer2 = uiHolder.elementTrsLst[16].GetComponent<Sta>();
            ipt_rotateSet = uiHolder.elementTrsLst[17].GetComponent<Ipt>();
            btn_rotate = uiHolder.elementTrsLst[18].GetComponent<Btn>();
            ipt_posSetX = uiHolder.elementTrsLst[19].GetComponent<Ipt>();
            ipt_posSetZ = uiHolder.elementTrsLst[20].GetComponent<Ipt>();
            ipt_posSetY = uiHolder.elementTrsLst[21].GetComponent<Ipt>();
            btn_align = uiHolder.elementTrsLst[22].GetComponent<Btn>();
            sta_align = uiHolder.elementTrsLst[23].GetComponent<Sta>();
            btn_resetCount = uiHolder.elementTrsLst[24].GetComponent<Btn>();
            ipt_cntSetX = uiHolder.elementTrsLst[25].GetComponent<Ipt>();
            ipt_cntSetY = uiHolder.elementTrsLst[26].GetComponent<Ipt>();
            go_toolTypeItem = uiHolder.elementTrsLst[27].gameObject;
            sub_toolTypeItem = (UiToolTypeItemCtrl) uiHolder.elementTrsLst[28].GetComponent<UiHolder>().ctrl;
        }

    }
    public partial class UiModToolCtrl:UiCtrl
    {
        public UiModToolView view;
        public UiModToolModel model;
        public UiModToolParam param;
        public UiModSceneMainCtrl parent=>(UiModSceneMainCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiModToolParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModToolView(uiHolder);
            model=new UiModToolModel();


            view.sub_lab = new UiLabCtrl();
            view.sub_lab.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
            view.sub_toolItem = new UiToolItemCtrl();
            view.sub_toolItem.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
            view.sub_toolTypeItem = new UiToolTypeItemCtrl();
            view.sub_toolTypeItem.BindHolderRecursively(uiHolder.subUiHolderLst[2]);
        }

    }
    public partial class UiModToolModel:UiModel
    {
        
    }
}

    public partial class UiModSceneMainParam:UiParam
    {
    }

    public partial class UiModSceneMainView:UiView
    {

            public Btn btn_menu;
            public GameObject go_toolPos;
            public ModTool.UiModToolCtrl page_ModTool;
            public Btn btn_view;
            public Btn btn_mapObject;
            public Sta sta_mapObject;
            public Btn btn_event;
            public Sta sta_event;
            public Btn btn_layer0;
            public Sta sta_layer0;
            public Btn btn_layer1;
            public Sta sta_layer1;
            public Btn btn_layer2;
            public Sta sta_layer2;
            public Ipt ipt_viewPosSetX;
            public Ipt ipt_viewPosSetZ;
            public Ipt ipt_viewPosSetY;
        public UiModSceneMainView(UiHolder uiHolder):base(uiHolder)
        {

            btn_menu = uiHolder.elementTrsLst[0].GetComponent<Btn>();
            go_toolPos = uiHolder.elementTrsLst[1].gameObject;
            page_ModTool = (ModTool.UiModToolCtrl) uiHolder.elementTrsLst[2].GetComponent<UiHolder>().ctrl;
            btn_view = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            btn_mapObject = uiHolder.elementTrsLst[4].GetComponent<Btn>();
            sta_mapObject = uiHolder.elementTrsLst[5].GetComponent<Sta>();
            btn_event = uiHolder.elementTrsLst[6].GetComponent<Btn>();
            sta_event = uiHolder.elementTrsLst[7].GetComponent<Sta>();
            btn_layer0 = uiHolder.elementTrsLst[8].GetComponent<Btn>();
            sta_layer0 = uiHolder.elementTrsLst[9].GetComponent<Sta>();
            btn_layer1 = uiHolder.elementTrsLst[10].GetComponent<Btn>();
            sta_layer1 = uiHolder.elementTrsLst[11].GetComponent<Sta>();
            btn_layer2 = uiHolder.elementTrsLst[12].GetComponent<Btn>();
            sta_layer2 = uiHolder.elementTrsLst[13].GetComponent<Sta>();
            ipt_viewPosSetX = uiHolder.elementTrsLst[14].GetComponent<Ipt>();
            ipt_viewPosSetZ = uiHolder.elementTrsLst[15].GetComponent<Ipt>();
            ipt_viewPosSetY = uiHolder.elementTrsLst[16].GetComponent<Ipt>();
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


            view.page_ModTool = new ModTool.UiModToolCtrl();
            view.page_ModTool.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
        }

    }
    public partial class UiModSceneMainModel:UiModel
    {
        
    }
}
