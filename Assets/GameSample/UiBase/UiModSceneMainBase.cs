
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;

namespace Ui.ModSceneMain

{




    public partial class UiToolItemParam:UiParam
    {
    }

    public partial class UiToolItemView:UiView
    {

            public GameObject go_toolItem;
            public Btn btn_tool;
            public Sta sta_tool;
            public Img img_;
            public Txt txt_name;
        public UiToolItemView(UiHolder uiHolder):base(uiHolder)
        {

            go_toolItem = uiHolder.elementTrsLst[0].gameObject;
            btn_tool = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            sta_tool = uiHolder.elementTrsLst[2].GetComponent<Sta>();
            img_ = uiHolder.elementTrsLst[3].GetComponent<Img>();
            txt_name = uiHolder.elementTrsLst[4].GetComponent<Txt>();
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
            public Btn btn_tool;
            public Sta sta_tool;
            public Img img_;
            public Txt txt_name;
        public UiToolTypeItemView(UiHolder uiHolder):base(uiHolder)
        {

            go_toolTypeItem = uiHolder.elementTrsLst[0].gameObject;
            btn_tool = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            sta_tool = uiHolder.elementTrsLst[2].GetComponent<Sta>();
            img_ = uiHolder.elementTrsLst[3].GetComponent<Img>();
            txt_name = uiHolder.elementTrsLst[4].GetComponent<Txt>();
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

            public Btn btn_showTool;
            public GameObject go_toolButtonPos;
            public Txt txt_showBg;
            public GameObject go_tool;
            public ScrView scr_tool;
            public GameObject go_toolContentPos;
            public GameObject go_toolType;
            public ScrView scr_toolType;
            public Btn btn_rotate;
            public Btn btn_pos;
            public Btn btn_cnt;
            public Txt txt_textureLayerSet;
            public Img img_;
            public Txt txt_rotate;
            public Txt txt_rotateSet;
            public Ipt ipt_rotateSet;
            public Img img_pos;
            public Sta sta_pos;
            public Txt txt_pos;
            public Txt txt_posSet;
            public Ipt ipt_posSetX;
            public Ipt ipt_posSetZ;
            public Ipt ipt_posSetY;
            public Img img_cnt;
            public Txt txt_cnt;
            public Txt txt_cntSet;
            public Ipt ipt_cntSetX;
            public Ipt ipt_cntSetY;
            public Btn btn_layer0;
            public Btn btn_layer1;
            public Btn btn_layer2;
            public GameObject go_toolItem;
            public UiToolItemCtrl sub_ToolItem;
            public Img img_layer0;
            public Sta sta_layer0;
            public Txt txt_layer0;
            public Img img_layer1;
            public Sta sta_layer1;
            public Txt txt_layer1;
            public Img img_layer2;
            public Sta sta_layer2;
            public Txt txt_layer2;
            public GameObject go_toolTypeItem;
            public UiToolTypeItemCtrl sub_ToolTypeItem;
        public UiModToolView(UiHolder uiHolder):base(uiHolder)
        {

            btn_showTool = uiHolder.elementTrsLst[0].GetComponent<Btn>();
            go_toolButtonPos = uiHolder.elementTrsLst[1].gameObject;
            txt_showBg = uiHolder.elementTrsLst[2].GetComponent<Txt>();
            go_tool = uiHolder.elementTrsLst[3].gameObject;
            scr_tool = uiHolder.elementTrsLst[4].GetComponent<ScrView>();
            go_toolContentPos = uiHolder.elementTrsLst[5].gameObject;
            go_toolType = uiHolder.elementTrsLst[6].gameObject;
            scr_toolType = uiHolder.elementTrsLst[7].GetComponent<ScrView>();
            btn_rotate = uiHolder.elementTrsLst[8].GetComponent<Btn>();
            btn_pos = uiHolder.elementTrsLst[9].GetComponent<Btn>();
            btn_cnt = uiHolder.elementTrsLst[10].GetComponent<Btn>();
            txt_textureLayerSet = uiHolder.elementTrsLst[11].GetComponent<Txt>();
            img_ = uiHolder.elementTrsLst[12].GetComponent<Img>();
            txt_rotate = uiHolder.elementTrsLst[13].GetComponent<Txt>();
            txt_rotateSet = uiHolder.elementTrsLst[14].GetComponent<Txt>();
            ipt_rotateSet = uiHolder.elementTrsLst[15].GetComponent<Ipt>();
            img_pos = uiHolder.elementTrsLst[16].GetComponent<Img>();
            sta_pos = uiHolder.elementTrsLst[17].GetComponent<Sta>();
            txt_pos = uiHolder.elementTrsLst[18].GetComponent<Txt>();
            txt_posSet = uiHolder.elementTrsLst[19].GetComponent<Txt>();
            ipt_posSetX = uiHolder.elementTrsLst[20].GetComponent<Ipt>();
            ipt_posSetZ = uiHolder.elementTrsLst[21].GetComponent<Ipt>();
            ipt_posSetY = uiHolder.elementTrsLst[22].GetComponent<Ipt>();
            img_cnt = uiHolder.elementTrsLst[23].GetComponent<Img>();
            txt_cnt = uiHolder.elementTrsLst[24].GetComponent<Txt>();
            txt_cntSet = uiHolder.elementTrsLst[25].GetComponent<Txt>();
            ipt_cntSetX = uiHolder.elementTrsLst[26].GetComponent<Ipt>();
            ipt_cntSetY = uiHolder.elementTrsLst[27].GetComponent<Ipt>();
            btn_layer0 = uiHolder.elementTrsLst[28].GetComponent<Btn>();
            btn_layer1 = uiHolder.elementTrsLst[29].GetComponent<Btn>();
            btn_layer2 = uiHolder.elementTrsLst[30].GetComponent<Btn>();
            go_toolItem = uiHolder.elementTrsLst[31].gameObject;
            sub_ToolItem = (UiToolItemCtrl) uiHolder.elementTrsLst[32].GetComponent<UiHolder>().ctrl;
            img_layer0 = uiHolder.elementTrsLst[33].GetComponent<Img>();
            sta_layer0 = uiHolder.elementTrsLst[34].GetComponent<Sta>();
            txt_layer0 = uiHolder.elementTrsLst[35].GetComponent<Txt>();
            img_layer1 = uiHolder.elementTrsLst[36].GetComponent<Img>();
            sta_layer1 = uiHolder.elementTrsLst[37].GetComponent<Sta>();
            txt_layer1 = uiHolder.elementTrsLst[38].GetComponent<Txt>();
            img_layer2 = uiHolder.elementTrsLst[39].GetComponent<Img>();
            sta_layer2 = uiHolder.elementTrsLst[40].GetComponent<Sta>();
            txt_layer2 = uiHolder.elementTrsLst[41].GetComponent<Txt>();
            go_toolTypeItem = uiHolder.elementTrsLst[42].gameObject;
            sub_ToolTypeItem = (UiToolTypeItemCtrl) uiHolder.elementTrsLst[43].GetComponent<UiHolder>().ctrl;
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


            view.sub_ToolItem = new UiToolItemCtrl();
            view.sub_ToolItem.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
            view.sub_ToolTypeItem = new UiToolTypeItemCtrl();
            view.sub_ToolTypeItem.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
        }

    }
    public partial class UiModToolModel:UiModel
    {
        
    }
    public partial class UiModSceneMainParam:UiParam
    {
    }

    public partial class UiModSceneMainView:UiView
    {

            public Btn btn_menu;
            public GameObject go_toolPos;
            public UiModToolCtrl sub_ModTool;
            public Txt txt_viewPos;
            public Ipt ipt_viewPosSetX;
            public Ipt ipt_viewPosSetY;
            public Ipt ipt_viewPosSetZ;
            public Btn btn_view;
            public Btn btn_model;
            public Btn btn_behaviour;
            public Txt txt_menu;
            public Img img_;
            public Txt txt_center;
            public Img img_model;
            public Txt txt_model;
            public Sta sta_selModel;
            public Img img_behaviour;
            public Txt txt_behaviour;
            public Sta sta_selBehaviour;
        public UiModSceneMainView(UiHolder uiHolder):base(uiHolder)
        {

            btn_menu = uiHolder.elementTrsLst[0].GetComponent<Btn>();
            go_toolPos = uiHolder.elementTrsLst[1].gameObject;
            sub_ModTool = (UiModToolCtrl) uiHolder.elementTrsLst[2].GetComponent<UiHolder>().ctrl;
            txt_viewPos = uiHolder.elementTrsLst[3].GetComponent<Txt>();
            ipt_viewPosSetX = uiHolder.elementTrsLst[4].GetComponent<Ipt>();
            ipt_viewPosSetY = uiHolder.elementTrsLst[5].GetComponent<Ipt>();
            ipt_viewPosSetZ = uiHolder.elementTrsLst[6].GetComponent<Ipt>();
            btn_view = uiHolder.elementTrsLst[7].GetComponent<Btn>();
            btn_model = uiHolder.elementTrsLst[8].GetComponent<Btn>();
            btn_behaviour = uiHolder.elementTrsLst[9].GetComponent<Btn>();
            txt_menu = uiHolder.elementTrsLst[10].GetComponent<Txt>();
            img_ = uiHolder.elementTrsLst[11].GetComponent<Img>();
            txt_center = uiHolder.elementTrsLst[12].GetComponent<Txt>();
            img_model = uiHolder.elementTrsLst[13].GetComponent<Img>();
            txt_model = uiHolder.elementTrsLst[14].GetComponent<Txt>();
            sta_selModel = uiHolder.elementTrsLst[15].GetComponent<Sta>();
            img_behaviour = uiHolder.elementTrsLst[16].GetComponent<Img>();
            txt_behaviour = uiHolder.elementTrsLst[17].GetComponent<Txt>();
            sta_selBehaviour = uiHolder.elementTrsLst[18].GetComponent<Sta>();
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


            view.sub_ModTool = new UiModToolCtrl();
            view.sub_ModTool.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
        }

    }
    public partial class UiModSceneMainModel:UiModel
    {
        
    }
}
