
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;

namespace Ui.ModStoryItemListArguments

{


namespace ModStoryItemListArgumentsStatic

{


    public partial class UiModStoryItemListArgumentsStaticParam:UiParam
    {
    }

    public partial class UiModStoryItemListArgumentsStaticView:UiView
    {

            public Btn btn_hpArgument;
            public Btn btn_speedArgument;
            public Btn btn_idleAnim;
            public Btn btn_moveAnim;
            public Txt txt_hpArgument;
            public Txt txt_speedArgument;
            public Txt txt_idleAnim;
            public Txt txt_moveAnim;
        public UiModStoryItemListArgumentsStaticView(UiHolder uiHolder):base(uiHolder)
        {

            btn_hpArgument = uiHolder.elementTrsLst[0].GetComponent<Btn>();
            btn_speedArgument = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            btn_idleAnim = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            btn_moveAnim = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            txt_hpArgument = uiHolder.elementTrsLst[4].GetComponent<Txt>();
            txt_speedArgument = uiHolder.elementTrsLst[5].GetComponent<Txt>();
            txt_idleAnim = uiHolder.elementTrsLst[6].GetComponent<Txt>();
            txt_moveAnim = uiHolder.elementTrsLst[7].GetComponent<Txt>();
        }

    }
    public partial class UiModStoryItemListArgumentsStaticCtrl:UiCtrl
    {
        public UiModStoryItemListArgumentsStaticView view;
        public UiModStoryItemListArgumentsStaticModel model;
        public UiModStoryItemListArgumentsStaticParam param;
        public UiModStoryItemListArgumentsCtrl parent=>(UiModStoryItemListArgumentsCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryItemListArgumentsStaticParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryItemListArgumentsStaticView(uiHolder);
            model=new UiModStoryItemListArgumentsStaticModel();


        }

    }
    public partial class UiModStoryItemListArgumentsStaticModel:UiModel
    {
        
    }
}

namespace ModStoryItemListArgumentsCustom

{



    public partial class UiUnitParam:UiParam
    {
    }

    public partial class UiUnitView:UiView
    {

            public GameObject go_unit;
            public Sta sta_unit;
            public Txt txt_name;
            public Txt txt_default;
            public Txt txt_min;
            public Txt txt_max;
            public Ipt ipt_default;
            public Ipt ipt_min;
            public Ipt ipt_max;
        public UiUnitView(UiHolder uiHolder):base(uiHolder)
        {

            go_unit = uiHolder.elementTrsLst[0].gameObject;
            sta_unit = uiHolder.elementTrsLst[1].GetComponent<Sta>();
            txt_name = uiHolder.elementTrsLst[2].GetComponent<Txt>();
            txt_default = uiHolder.elementTrsLst[3].GetComponent<Txt>();
            txt_min = uiHolder.elementTrsLst[4].GetComponent<Txt>();
            txt_max = uiHolder.elementTrsLst[5].GetComponent<Txt>();
            ipt_default = uiHolder.elementTrsLst[6].GetComponent<Ipt>();
            ipt_min = uiHolder.elementTrsLst[7].GetComponent<Ipt>();
            ipt_max = uiHolder.elementTrsLst[8].GetComponent<Ipt>();
        }

    }
    public partial class UiUnitCtrl:UiCtrl
    {
        public UiUnitView view;
        public UiUnitModel model;
        public UiUnitParam param;
        public UiModStoryItemListArgumentsCustomCtrl parent=>(UiModStoryItemListArgumentsCustomCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiUnitParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiUnitView(uiHolder);
            model=new UiUnitModel();


        }

    }
    public partial class UiUnitModel:UiModel
    {
        
    }
    public partial class UiModStoryItemListArgumentsCustomParam:UiParam
    {
    }

    public partial class UiModStoryItemListArgumentsCustomView:UiView
    {

            public GameObject go_units;
            public ScrView scr_units;
            public GameObject go_unit;
            public Sta sta_unit;
            public UiUnitCtrl sub_Unit;
        public UiModStoryItemListArgumentsCustomView(UiHolder uiHolder):base(uiHolder)
        {

            go_units = uiHolder.elementTrsLst[0].gameObject;
            scr_units = uiHolder.elementTrsLst[1].GetComponent<ScrView>();
            go_unit = uiHolder.elementTrsLst[2].gameObject;
            sta_unit = uiHolder.elementTrsLst[3].GetComponent<Sta>();
            sub_Unit = (UiUnitCtrl) uiHolder.elementTrsLst[4].GetComponent<UiHolder>().ctrl;
        }

    }
    public partial class UiModStoryItemListArgumentsCustomCtrl:UiCtrl
    {
        public UiModStoryItemListArgumentsCustomView view;
        public UiModStoryItemListArgumentsCustomModel model;
        public UiModStoryItemListArgumentsCustomParam param;
        public UiModStoryItemListArgumentsCtrl parent=>(UiModStoryItemListArgumentsCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryItemListArgumentsCustomParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryItemListArgumentsCustomView(uiHolder);
            model=new UiModStoryItemListArgumentsCustomModel();


            view.sub_Unit = new UiUnitCtrl();
            view.sub_Unit.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
        }

    }
    public partial class UiModStoryItemListArgumentsCustomModel:UiModel
    {
        
    }
}

    public partial class UiModStoryItemListArgumentsParam:UiParam
    {
    }

    public partial class UiModStoryItemListArgumentsView:UiView
    {

            public ModStoryItemListArgumentsStatic.UiModStoryItemListArgumentsStaticCtrl page_ModStoryItemListArgumentsStatic;
            public ModStoryItemListArgumentsCustom.UiModStoryItemListArgumentsCustomCtrl page_ModStoryItemListArgumentsCustom;
            public Btn btn_back;
            public Btn btn_static;
            public Sta sta_static;
            public Btn btn_custom;
            public Sta sta_custom;
            public Img img_init;
            public Img img_global;
        public UiModStoryItemListArgumentsView(UiHolder uiHolder):base(uiHolder)
        {

            page_ModStoryItemListArgumentsStatic = (ModStoryItemListArgumentsStatic.UiModStoryItemListArgumentsStaticCtrl) uiHolder.elementTrsLst[0].GetComponent<UiHolder>().ctrl;
            page_ModStoryItemListArgumentsCustom = (ModStoryItemListArgumentsCustom.UiModStoryItemListArgumentsCustomCtrl) uiHolder.elementTrsLst[1].GetComponent<UiHolder>().ctrl;
            btn_back = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            btn_static = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            sta_static = uiHolder.elementTrsLst[4].GetComponent<Sta>();
            btn_custom = uiHolder.elementTrsLst[5].GetComponent<Btn>();
            sta_custom = uiHolder.elementTrsLst[6].GetComponent<Sta>();
            img_init = uiHolder.elementTrsLst[7].GetComponent<Img>();
            img_global = uiHolder.elementTrsLst[8].GetComponent<Img>();
        }

    }
    public partial class UiModStoryItemListArgumentsCtrl:UiCtrl
    {
        public UiModStoryItemListArgumentsView view;
        public UiModStoryItemListArgumentsModel model;
        public UiModStoryItemListArgumentsParam param;
        

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryItemListArgumentsParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryItemListArgumentsView(uiHolder);
            model=new UiModStoryItemListArgumentsModel();


            view.page_ModStoryItemListArgumentsStatic = new ModStoryItemListArgumentsStatic.UiModStoryItemListArgumentsStaticCtrl();
            view.page_ModStoryItemListArgumentsStatic.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
            view.page_ModStoryItemListArgumentsCustom = new ModStoryItemListArgumentsCustom.UiModStoryItemListArgumentsCustomCtrl();
            view.page_ModStoryItemListArgumentsCustom.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
        }

    }
    public partial class UiModStoryItemListArgumentsModel:UiModel
    {
        
    }
}
