
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;

namespace Ui.ModStoryCharacterListArguments

{


namespace ModStoryCharacterListArgumentsStatic

{


    public partial class UiModStoryCharacterListArgumentsStaticParam:UiParam
    {
    }

    public partial class UiModStoryCharacterListArgumentsStaticView:UiView
    {

        public UiModStoryCharacterListArgumentsStaticView(UiHolder uiHolder):base(uiHolder)
        {

        }

    }
    public partial class UiModStoryCharacterListArgumentsStaticCtrl:UiCtrl
    {
        public UiModStoryCharacterListArgumentsStaticView view;
        public UiModStoryCharacterListArgumentsStaticModel model;
        public UiModStoryCharacterListArgumentsStaticParam param;
        public UiModStoryCharacterListArgumentsCtrl parent=>(UiModStoryCharacterListArgumentsCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryCharacterListArgumentsStaticParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryCharacterListArgumentsStaticView(uiHolder);
            model=new UiModStoryCharacterListArgumentsStaticModel();


        }

    }
    public partial class UiModStoryCharacterListArgumentsStaticModel:UiModel
    {
        
    }
}

namespace ModStoryCharacterListArgumentsCustom

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
        public UiModStoryCharacterListArgumentsCustomCtrl parent=>(UiModStoryCharacterListArgumentsCustomCtrl)uiHolder.parent.ctrl;

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
    public partial class UiModStoryCharacterListArgumentsCustomParam:UiParam
    {
    }

    public partial class UiModStoryCharacterListArgumentsCustomView:UiView
    {

            public GameObject go_units;
            public ScrView scr_units;
            public GameObject go_unit;
            public Sta sta_unit;
            public UiUnitCtrl sub_Unit;
        public UiModStoryCharacterListArgumentsCustomView(UiHolder uiHolder):base(uiHolder)
        {

            go_units = uiHolder.elementTrsLst[0].gameObject;
            scr_units = uiHolder.elementTrsLst[1].GetComponent<ScrView>();
            go_unit = uiHolder.elementTrsLst[2].gameObject;
            sta_unit = uiHolder.elementTrsLst[3].GetComponent<Sta>();
            sub_Unit = (UiUnitCtrl) uiHolder.elementTrsLst[4].GetComponent<UiHolder>().ctrl;
        }

    }
    public partial class UiModStoryCharacterListArgumentsCustomCtrl:UiCtrl
    {
        public UiModStoryCharacterListArgumentsCustomView view;
        public UiModStoryCharacterListArgumentsCustomModel model;
        public UiModStoryCharacterListArgumentsCustomParam param;
        public UiModStoryCharacterListArgumentsCtrl parent=>(UiModStoryCharacterListArgumentsCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryCharacterListArgumentsCustomParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryCharacterListArgumentsCustomView(uiHolder);
            model=new UiModStoryCharacterListArgumentsCustomModel();


            view.sub_Unit = new UiUnitCtrl();
            view.sub_Unit.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
        }

    }
    public partial class UiModStoryCharacterListArgumentsCustomModel:UiModel
    {
        
    }
}

    public partial class UiModStoryCharacterListArgumentsParam:UiParam
    {
    }

    public partial class UiModStoryCharacterListArgumentsView:UiView
    {

            public ModStoryCharacterListArgumentsStatic.UiModStoryCharacterListArgumentsStaticCtrl page_ModStoryCharacterListArgumentsStatic;
            public ModStoryCharacterListArgumentsCustom.UiModStoryCharacterListArgumentsCustomCtrl page_ModStoryCharacterListArgumentsCustom;
            public Btn btn_back;
            public Btn btn_static;
            public Sta sta_static;
            public Btn btn_custom;
            public Sta sta_custom;
            public Img img_init;
            public Img img_global;
        public UiModStoryCharacterListArgumentsView(UiHolder uiHolder):base(uiHolder)
        {

            page_ModStoryCharacterListArgumentsStatic = (ModStoryCharacterListArgumentsStatic.UiModStoryCharacterListArgumentsStaticCtrl) uiHolder.elementTrsLst[0].GetComponent<UiHolder>().ctrl;
            page_ModStoryCharacterListArgumentsCustom = (ModStoryCharacterListArgumentsCustom.UiModStoryCharacterListArgumentsCustomCtrl) uiHolder.elementTrsLst[1].GetComponent<UiHolder>().ctrl;
            btn_back = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            btn_static = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            sta_static = uiHolder.elementTrsLst[4].GetComponent<Sta>();
            btn_custom = uiHolder.elementTrsLst[5].GetComponent<Btn>();
            sta_custom = uiHolder.elementTrsLst[6].GetComponent<Sta>();
            img_init = uiHolder.elementTrsLst[7].GetComponent<Img>();
            img_global = uiHolder.elementTrsLst[8].GetComponent<Img>();
        }

    }
    public partial class UiModStoryCharacterListArgumentsCtrl:UiCtrl
    {
        public UiModStoryCharacterListArgumentsView view;
        public UiModStoryCharacterListArgumentsModel model;
        public UiModStoryCharacterListArgumentsParam param;
        

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryCharacterListArgumentsParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryCharacterListArgumentsView(uiHolder);
            model=new UiModStoryCharacterListArgumentsModel();


            view.page_ModStoryCharacterListArgumentsStatic = new ModStoryCharacterListArgumentsStatic.UiModStoryCharacterListArgumentsStaticCtrl();
            view.page_ModStoryCharacterListArgumentsStatic.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
            view.page_ModStoryCharacterListArgumentsCustom = new ModStoryCharacterListArgumentsCustom.UiModStoryCharacterListArgumentsCustomCtrl();
            view.page_ModStoryCharacterListArgumentsCustom.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
        }

    }
    public partial class UiModStoryCharacterListArgumentsModel:UiModel
    {
        
    }
}
