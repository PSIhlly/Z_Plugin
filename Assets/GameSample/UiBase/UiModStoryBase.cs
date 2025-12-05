
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;
using Z_Texture;
using Z_Video;
using UnityEngine.Video;
namespace Ui.ModStory

{




namespace ModStoryOverview

{




    public partial class UiModStoryOverviewParam:UiParam
    {
    }

    public partial class UiModStoryOverviewView:UiView
    {

            public Ipt ipt_name;
            public Ipt ipt_introduction;
            public Btn btn_image;
            public Img img_image;
        public UiModStoryOverviewView(UiHolder uiHolder):base(uiHolder)
        {

            ipt_name = uiHolder.elementTrsLst[0].GetComponent<Ipt>();
            ipt_introduction = uiHolder.elementTrsLst[1].GetComponent<Ipt>();
            btn_image = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            img_image = uiHolder.elementTrsLst[3].GetComponent<Img>();
        }

    }
    public partial class UiModStoryOverviewCtrl:UiCtrl
    {
        public UiModStoryOverviewView view;
        public UiModStoryOverviewModel model;
        public UiModStoryOverviewParam param;
        public UiModStoryCtrl parent=>(UiModStoryCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryOverviewParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryOverviewView(uiHolder);
            model=new UiModStoryOverviewModel();


        }

    }
    public partial class UiModStoryOverviewModel:UiModel
    {
        
    }
}

namespace ModStoryParameter

{




namespace ModStoryCharacterParameter

{







    public partial class UiArgParam:UiParam
    {
    }

    public partial class UiArgView:UiView
    {

            public GameObject go_arg;
            public Sta sta_exist;
            public Btn btn_new;
            public Btn btn_delete;
            public Ipt ipt_name;
            public Ipt ipt_value;
            public Dp dp_;
        public UiArgView(UiHolder uiHolder):base(uiHolder)
        {

            go_arg = uiHolder.elementTrsLst[0].gameObject;
            sta_exist = uiHolder.elementTrsLst[1].GetComponent<Sta>();
            btn_new = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            btn_delete = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            ipt_name = uiHolder.elementTrsLst[4].GetComponent<Ipt>();
            ipt_value = uiHolder.elementTrsLst[5].GetComponent<Ipt>();
            dp_ = uiHolder.elementTrsLst[6].GetComponent<Dp>();
        }

    }
    public partial class UiArgCtrl:UiCtrl
    {
        public UiArgView view;
        public UiArgModel model;
        public UiArgParam param;
        public UiModStoryCharacterParameterCtrl parent=>(UiModStoryCharacterParameterCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiArgParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiArgView(uiHolder);
            model=new UiArgModel();


        }

    }
    public partial class UiArgModel:UiModel
    {
        
    }
    public partial class UiModStoryCharacterParameterParam:UiParam
    {
    }

    public partial class UiModStoryCharacterParameterView:UiView
    {

            public ScrView scr_args;
            public GameObject go_arg;
            public UiArgCtrl sub_Arg;
        public UiModStoryCharacterParameterView(UiHolder uiHolder):base(uiHolder)
        {

            scr_args = uiHolder.elementTrsLst[0].GetComponent<ScrView>();
            go_arg = uiHolder.elementTrsLst[1].gameObject;
            sub_Arg = (UiArgCtrl) uiHolder.elementTrsLst[2].GetComponent<UiHolder>().ctrl;
        }

    }
    public partial class UiModStoryCharacterParameterCtrl:UiCtrl
    {
        public UiModStoryCharacterParameterView view;
        public UiModStoryCharacterParameterModel model;
        public UiModStoryCharacterParameterParam param;
        public UiModStoryParameterCtrl parent=>(UiModStoryParameterCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryCharacterParameterParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryCharacterParameterView(uiHolder);
            model=new UiModStoryCharacterParameterModel();


            view.sub_Arg = new UiArgCtrl();
            view.sub_Arg.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
        }

    }
    public partial class UiModStoryCharacterParameterModel:UiModel
    {
        
    }
}

namespace ModStoryItemParameter

{







    public partial class UiArgParam:UiParam
    {
    }

    public partial class UiArgView:UiView
    {

            public GameObject go_arg;
            public Sta sta_exist;
            public Btn btn_new;
            public Btn btn_delete;
            public Ipt ipt_name;
            public Ipt ipt_value;
            public Dp dp_;
        public UiArgView(UiHolder uiHolder):base(uiHolder)
        {

            go_arg = uiHolder.elementTrsLst[0].gameObject;
            sta_exist = uiHolder.elementTrsLst[1].GetComponent<Sta>();
            btn_new = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            btn_delete = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            ipt_name = uiHolder.elementTrsLst[4].GetComponent<Ipt>();
            ipt_value = uiHolder.elementTrsLst[5].GetComponent<Ipt>();
            dp_ = uiHolder.elementTrsLst[6].GetComponent<Dp>();
        }

    }
    public partial class UiArgCtrl:UiCtrl
    {
        public UiArgView view;
        public UiArgModel model;
        public UiArgParam param;
        public UiModStoryItemParameterCtrl parent=>(UiModStoryItemParameterCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiArgParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiArgView(uiHolder);
            model=new UiArgModel();


        }

    }
    public partial class UiArgModel:UiModel
    {
        
    }
    public partial class UiModStoryItemParameterParam:UiParam
    {
    }

    public partial class UiModStoryItemParameterView:UiView
    {

            public ScrView scr_args;
            public GameObject go_arg;
            public UiArgCtrl sub_Arg;
        public UiModStoryItemParameterView(UiHolder uiHolder):base(uiHolder)
        {

            scr_args = uiHolder.elementTrsLst[0].GetComponent<ScrView>();
            go_arg = uiHolder.elementTrsLst[1].gameObject;
            sub_Arg = (UiArgCtrl) uiHolder.elementTrsLst[2].GetComponent<UiHolder>().ctrl;
        }

    }
    public partial class UiModStoryItemParameterCtrl:UiCtrl
    {
        public UiModStoryItemParameterView view;
        public UiModStoryItemParameterModel model;
        public UiModStoryItemParameterParam param;
        public UiModStoryParameterCtrl parent=>(UiModStoryParameterCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryItemParameterParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryItemParameterView(uiHolder);
            model=new UiModStoryItemParameterModel();


            view.sub_Arg = new UiArgCtrl();
            view.sub_Arg.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
        }

    }
    public partial class UiModStoryItemParameterModel:UiModel
    {
        
    }
}

namespace ModStoryConfig

{




    public partial class UiModStoryConfigParam:UiParam
    {
    }

    public partial class UiModStoryConfigView:UiView
    {

            public Btn btn_mainCharacter;
            public Btn btn_perspective;
            public Txt txt_mainCharacter;
            public Txt txt_perspective;
        public UiModStoryConfigView(UiHolder uiHolder):base(uiHolder)
        {

            btn_mainCharacter = uiHolder.elementTrsLst[0].GetComponent<Btn>();
            btn_perspective = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            txt_mainCharacter = uiHolder.elementTrsLst[2].GetComponent<Txt>();
            txt_perspective = uiHolder.elementTrsLst[3].GetComponent<Txt>();
        }

    }
    public partial class UiModStoryConfigCtrl:UiCtrl
    {
        public UiModStoryConfigView view;
        public UiModStoryConfigModel model;
        public UiModStoryConfigParam param;
        public UiModStoryParameterCtrl parent=>(UiModStoryParameterCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryConfigParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryConfigView(uiHolder);
            model=new UiModStoryConfigModel();


        }

    }
    public partial class UiModStoryConfigModel:UiModel
    {
        
    }
}

    public partial class UiModStoryParameterParam:UiParam
    {
    }

    public partial class UiModStoryParameterView:UiView
    {

            public ModStoryCharacterParameter.UiModStoryCharacterParameterCtrl page_ModStoryCharacterParameter;
            public ModStoryItemParameter.UiModStoryItemParameterCtrl page_ModStoryItemParameter;
            public ModStoryConfig.UiModStoryConfigCtrl page_ModStoryConfig;
            public Btn btn_characterParameter;
            public Sta sta_characterParameter;
            public Btn btn_itemParameter;
            public Sta sta_itemParameter;
            public Btn btn_config;
            public Sta sta_config;
        public UiModStoryParameterView(UiHolder uiHolder):base(uiHolder)
        {

            page_ModStoryCharacterParameter = (ModStoryCharacterParameter.UiModStoryCharacterParameterCtrl) uiHolder.elementTrsLst[0].GetComponent<UiHolder>().ctrl;
            page_ModStoryItemParameter = (ModStoryItemParameter.UiModStoryItemParameterCtrl) uiHolder.elementTrsLst[1].GetComponent<UiHolder>().ctrl;
            page_ModStoryConfig = (ModStoryConfig.UiModStoryConfigCtrl) uiHolder.elementTrsLst[2].GetComponent<UiHolder>().ctrl;
            btn_characterParameter = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            sta_characterParameter = uiHolder.elementTrsLst[4].GetComponent<Sta>();
            btn_itemParameter = uiHolder.elementTrsLst[5].GetComponent<Btn>();
            sta_itemParameter = uiHolder.elementTrsLst[6].GetComponent<Sta>();
            btn_config = uiHolder.elementTrsLst[7].GetComponent<Btn>();
            sta_config = uiHolder.elementTrsLst[8].GetComponent<Sta>();
        }

    }
    public partial class UiModStoryParameterCtrl:UiCtrl
    {
        public UiModStoryParameterView view;
        public UiModStoryParameterModel model;
        public UiModStoryParameterParam param;
        public UiModStoryCtrl parent=>(UiModStoryCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryParameterParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryParameterView(uiHolder);
            model=new UiModStoryParameterModel();


            view.page_ModStoryCharacterParameter = new ModStoryCharacterParameter.UiModStoryCharacterParameterCtrl();
            view.page_ModStoryCharacterParameter.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
            view.page_ModStoryItemParameter = new ModStoryItemParameter.UiModStoryItemParameterCtrl();
            view.page_ModStoryItemParameter.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
            view.page_ModStoryConfig = new ModStoryConfig.UiModStoryConfigCtrl();
            view.page_ModStoryConfig.BindHolderRecursively(uiHolder.subUiHolderLst[2]);
        }

    }
    public partial class UiModStoryParameterModel:UiModel
    {
        
    }
}

namespace ModStoryCharacter

{




namespace ModStoryCharacterList

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
        public UiModStoryCharacterListCtrl parent=>(UiModStoryCharacterListCtrl)uiHolder.parent.ctrl;

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



    public partial class UiBigItemParam:UiParam
    {
    }

    public partial class UiBigItemView:UiView
    {

            public GameObject go_bigItem;
            public Sta sta_bigItem;
            public Sta sta_exist;
            public Btn btn_new;
            public Btn btn_;
            public Sta sta_;
            public Txt txt_;
            public Img img_;
        public UiBigItemView(UiHolder uiHolder):base(uiHolder)
        {

            go_bigItem = uiHolder.elementTrsLst[0].gameObject;
            sta_bigItem = uiHolder.elementTrsLst[1].GetComponent<Sta>();
            sta_exist = uiHolder.elementTrsLst[2].GetComponent<Sta>();
            btn_new = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            btn_ = uiHolder.elementTrsLst[4].GetComponent<Btn>();
            sta_ = uiHolder.elementTrsLst[5].GetComponent<Sta>();
            txt_ = uiHolder.elementTrsLst[6].GetComponent<Txt>();
            img_ = uiHolder.elementTrsLst[7].GetComponent<Img>();
        }

    }
    public partial class UiBigItemCtrl:UiCtrl
    {
        public UiBigItemView view;
        public UiBigItemModel model;
        public UiBigItemParam param;
        public UiModStoryCharacterListCtrl parent=>(UiModStoryCharacterListCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiBigItemParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiBigItemView(uiHolder);
            model=new UiBigItemModel();


        }

    }
    public partial class UiBigItemModel:UiModel
    {
        
    }
    public partial class UiModStoryCharacterListParam:UiParam
    {
    }

    public partial class UiModStoryCharacterListView:UiView
    {

            public ScrView scr_labs;
            public ScrView scr_bigItems;
            public GameObject go_lab;
            public UiLabCtrl sub_Lab;
            public GameObject go_bigItem;
            public Sta sta_bigItem;
            public UiBigItemCtrl sub_BigItem;
        public UiModStoryCharacterListView(UiHolder uiHolder):base(uiHolder)
        {

            scr_labs = uiHolder.elementTrsLst[0].GetComponent<ScrView>();
            scr_bigItems = uiHolder.elementTrsLst[1].GetComponent<ScrView>();
            go_lab = uiHolder.elementTrsLst[2].gameObject;
            sub_Lab = (UiLabCtrl) uiHolder.elementTrsLst[3].GetComponent<UiHolder>().ctrl;
            go_bigItem = uiHolder.elementTrsLst[4].gameObject;
            sta_bigItem = uiHolder.elementTrsLst[5].GetComponent<Sta>();
            sub_BigItem = (UiBigItemCtrl) uiHolder.elementTrsLst[6].GetComponent<UiHolder>().ctrl;
        }

    }
    public partial class UiModStoryCharacterListCtrl:UiCtrl
    {
        public UiModStoryCharacterListView view;
        public UiModStoryCharacterListModel model;
        public UiModStoryCharacterListParam param;
        public UiModStoryCharacterCtrl parent=>(UiModStoryCharacterCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryCharacterListParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryCharacterListView(uiHolder);
            model=new UiModStoryCharacterListModel();


            view.sub_Lab = new UiLabCtrl();
            view.sub_Lab.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
            view.sub_BigItem = new UiBigItemCtrl();
            view.sub_BigItem.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
        }

    }
    public partial class UiModStoryCharacterListModel:UiModel
    {
        
    }
}

namespace ModStoryCharacterUnit

{




namespace ModStoryCharacterUnitOverview

{




    public partial class UiModStoryCharacterUnitOverviewParam:UiParam
    {
    }

    public partial class UiModStoryCharacterUnitOverviewView:UiView
    {

            public Btn btn_delete;
            public Ipt ipt_name;
            public Ipt ipt_label;
            public Btn btn_image;
            public Img img_image;
            public Btn btn_tachie;
            public Img img_tachie;
            public Ipt ipt_desc;
        public UiModStoryCharacterUnitOverviewView(UiHolder uiHolder):base(uiHolder)
        {

            btn_delete = uiHolder.elementTrsLst[0].GetComponent<Btn>();
            ipt_name = uiHolder.elementTrsLst[1].GetComponent<Ipt>();
            ipt_label = uiHolder.elementTrsLst[2].GetComponent<Ipt>();
            btn_image = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            img_image = uiHolder.elementTrsLst[4].GetComponent<Img>();
            btn_tachie = uiHolder.elementTrsLst[5].GetComponent<Btn>();
            img_tachie = uiHolder.elementTrsLst[6].GetComponent<Img>();
            ipt_desc = uiHolder.elementTrsLst[7].GetComponent<Ipt>();
        }

    }
    public partial class UiModStoryCharacterUnitOverviewCtrl:UiCtrl
    {
        public UiModStoryCharacterUnitOverviewView view;
        public UiModStoryCharacterUnitOverviewModel model;
        public UiModStoryCharacterUnitOverviewParam param;
        public UiModStoryCharacterUnitCtrl parent=>(UiModStoryCharacterUnitCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryCharacterUnitOverviewParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryCharacterUnitOverviewView(uiHolder);
            model=new UiModStoryCharacterUnitOverviewModel();


        }

    }
    public partial class UiModStoryCharacterUnitOverviewModel:UiModel
    {
        
    }
}

namespace ModStoryCharacterUnitParameter

{







    public partial class UiArgIptParam:UiParam
    {
    }

    public partial class UiArgIptView:UiView
    {

            public GameObject go_argIpt;
            public Txt txt_;
            public Ipt ipt_min;
            public Ipt ipt_value;
            public Ipt ipt_max;
        public UiArgIptView(UiHolder uiHolder):base(uiHolder)
        {

            go_argIpt = uiHolder.elementTrsLst[0].gameObject;
            txt_ = uiHolder.elementTrsLst[1].GetComponent<Txt>();
            ipt_min = uiHolder.elementTrsLst[2].GetComponent<Ipt>();
            ipt_value = uiHolder.elementTrsLst[3].GetComponent<Ipt>();
            ipt_max = uiHolder.elementTrsLst[4].GetComponent<Ipt>();
        }

    }
    public partial class UiArgIptCtrl:UiCtrl
    {
        public UiArgIptView view;
        public UiArgIptModel model;
        public UiArgIptParam param;
        public UiModStoryCharacterUnitParameterCtrl parent=>(UiModStoryCharacterUnitParameterCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiArgIptParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiArgIptView(uiHolder);
            model=new UiArgIptModel();


        }

    }
    public partial class UiArgIptModel:UiModel
    {
        
    }
    public partial class UiModStoryCharacterUnitParameterParam:UiParam
    {
    }

    public partial class UiModStoryCharacterUnitParameterView:UiView
    {

            public ScrView scr_argIpts;
            public GameObject go_argIpt;
            public UiArgIptCtrl sub_ArgIpt;
        public UiModStoryCharacterUnitParameterView(UiHolder uiHolder):base(uiHolder)
        {

            scr_argIpts = uiHolder.elementTrsLst[0].GetComponent<ScrView>();
            go_argIpt = uiHolder.elementTrsLst[1].gameObject;
            sub_ArgIpt = (UiArgIptCtrl) uiHolder.elementTrsLst[2].GetComponent<UiHolder>().ctrl;
        }

    }
    public partial class UiModStoryCharacterUnitParameterCtrl:UiCtrl
    {
        public UiModStoryCharacterUnitParameterView view;
        public UiModStoryCharacterUnitParameterModel model;
        public UiModStoryCharacterUnitParameterParam param;
        public UiModStoryCharacterUnitCtrl parent=>(UiModStoryCharacterUnitCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryCharacterUnitParameterParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryCharacterUnitParameterView(uiHolder);
            model=new UiModStoryCharacterUnitParameterModel();


            view.sub_ArgIpt = new UiArgIptCtrl();
            view.sub_ArgIpt.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
        }

    }
    public partial class UiModStoryCharacterUnitParameterModel:UiModel
    {
        
    }
}

namespace ModStoryCharacterUnitAppearance

{




namespace ModStoryCharacterUnitAppearanceList

{







    public partial class UiBigItemParam:UiParam
    {
    }

    public partial class UiBigItemView:UiView
    {

            public GameObject go_bigItem;
            public Sta sta_bigItem;
            public Sta sta_exist;
            public Btn btn_new;
            public Btn btn_;
            public Sta sta_;
            public Txt txt_;
            public Img img_;
        public UiBigItemView(UiHolder uiHolder):base(uiHolder)
        {

            go_bigItem = uiHolder.elementTrsLst[0].gameObject;
            sta_bigItem = uiHolder.elementTrsLst[1].GetComponent<Sta>();
            sta_exist = uiHolder.elementTrsLst[2].GetComponent<Sta>();
            btn_new = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            btn_ = uiHolder.elementTrsLst[4].GetComponent<Btn>();
            sta_ = uiHolder.elementTrsLst[5].GetComponent<Sta>();
            txt_ = uiHolder.elementTrsLst[6].GetComponent<Txt>();
            img_ = uiHolder.elementTrsLst[7].GetComponent<Img>();
        }

    }
    public partial class UiBigItemCtrl:UiCtrl
    {
        public UiBigItemView view;
        public UiBigItemModel model;
        public UiBigItemParam param;
        public UiModStoryCharacterUnitAppearanceListCtrl parent=>(UiModStoryCharacterUnitAppearanceListCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiBigItemParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiBigItemView(uiHolder);
            model=new UiBigItemModel();


        }

    }
    public partial class UiBigItemModel:UiModel
    {
        
    }
    public partial class UiModStoryCharacterUnitAppearanceListParam:UiParam
    {
    }

    public partial class UiModStoryCharacterUnitAppearanceListView:UiView
    {

            public ScrView scr_bigItems;
            public GameObject go_bigItem;
            public Sta sta_bigItem;
            public UiBigItemCtrl sub_BigItem;
        public UiModStoryCharacterUnitAppearanceListView(UiHolder uiHolder):base(uiHolder)
        {

            scr_bigItems = uiHolder.elementTrsLst[0].GetComponent<ScrView>();
            go_bigItem = uiHolder.elementTrsLst[1].gameObject;
            sta_bigItem = uiHolder.elementTrsLst[2].GetComponent<Sta>();
            sub_BigItem = (UiBigItemCtrl) uiHolder.elementTrsLst[3].GetComponent<UiHolder>().ctrl;
        }

    }
    public partial class UiModStoryCharacterUnitAppearanceListCtrl:UiCtrl
    {
        public UiModStoryCharacterUnitAppearanceListView view;
        public UiModStoryCharacterUnitAppearanceListModel model;
        public UiModStoryCharacterUnitAppearanceListParam param;
        public UiModStoryCharacterUnitAppearanceCtrl parent=>(UiModStoryCharacterUnitAppearanceCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryCharacterUnitAppearanceListParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryCharacterUnitAppearanceListView(uiHolder);
            model=new UiModStoryCharacterUnitAppearanceListModel();


            view.sub_BigItem = new UiBigItemCtrl();
            view.sub_BigItem.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
        }

    }
    public partial class UiModStoryCharacterUnitAppearanceListModel:UiModel
    {
        
    }
}

namespace ModStoryCharacterUnitAppearanceUnit

{

using Ui.Axis;






    public partial class UiToggleParam:UiParam
    {
    }

    public partial class UiToggleView:UiView
    {

            public GameObject go_toggle;
            public Txt txt_;
            public Btn btn_enablePart;
            public Sta sta_enablePart;
        public UiToggleView(UiHolder uiHolder):base(uiHolder)
        {

            go_toggle = uiHolder.elementTrsLst[0].gameObject;
            txt_ = uiHolder.elementTrsLst[1].GetComponent<Txt>();
            btn_enablePart = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            sta_enablePart = uiHolder.elementTrsLst[3].GetComponent<Sta>();
        }

    }
    public partial class UiToggleCtrl:UiCtrl
    {
        public UiToggleView view;
        public UiToggleModel model;
        public UiToggleParam param;
        public UiModStoryCharacterUnitAppearanceUnitCtrl parent=>(UiModStoryCharacterUnitAppearanceUnitCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiToggleParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiToggleView(uiHolder);
            model=new UiToggleModel();


        }

    }
    public partial class UiToggleModel:UiModel
    {
        
    }



    public partial class UiItemParam:UiParam
    {
    }

    public partial class UiItemView:UiView
    {

            public GameObject go_item;
            public Sta sta_exist;
            public Btn btn_new;
            public Btn btn_;
            public Sta sta_;
            public Img img_;
            public Txt txt_;
        public UiItemView(UiHolder uiHolder):base(uiHolder)
        {

            go_item = uiHolder.elementTrsLst[0].gameObject;
            sta_exist = uiHolder.elementTrsLst[1].GetComponent<Sta>();
            btn_new = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            btn_ = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            sta_ = uiHolder.elementTrsLst[4].GetComponent<Sta>();
            img_ = uiHolder.elementTrsLst[5].GetComponent<Img>();
            txt_ = uiHolder.elementTrsLst[6].GetComponent<Txt>();
        }

    }
    public partial class UiItemCtrl:UiCtrl
    {
        public UiItemView view;
        public UiItemModel model;
        public UiItemParam param;
        public UiModStoryCharacterUnitAppearanceUnitCtrl parent=>(UiModStoryCharacterUnitAppearanceUnitCtrl)uiHolder.parent.ctrl;

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



    public partial class UiEquipPartParam:UiParam
    {
    }

    public partial class UiEquipPartView:UiView
    {

            public GameObject go_equipPart;
            public Btn btn_;
            public Sta sta_;
            public Txt txt_;
        public UiEquipPartView(UiHolder uiHolder):base(uiHolder)
        {

            go_equipPart = uiHolder.elementTrsLst[0].gameObject;
            btn_ = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            sta_ = uiHolder.elementTrsLst[2].GetComponent<Sta>();
            txt_ = uiHolder.elementTrsLst[3].GetComponent<Txt>();
        }

    }
    public partial class UiEquipPartCtrl:UiCtrl
    {
        public UiEquipPartView view;
        public UiEquipPartModel model;
        public UiEquipPartParam param;
        public UiModStoryCharacterUnitAppearanceUnitCtrl parent=>(UiModStoryCharacterUnitAppearanceUnitCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiEquipPartParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiEquipPartView(uiHolder);
            model=new UiEquipPartModel();


        }

    }
    public partial class UiEquipPartModel:UiModel
    {
        
    }



    public partial class UiPartParam:UiParam
    {
    }

    public partial class UiPartView:UiView
    {

            public GameObject go_part;
            public Btn btn_;
            public Sta sta_;
            public Txt txt_;
        public UiPartView(UiHolder uiHolder):base(uiHolder)
        {

            go_part = uiHolder.elementTrsLst[0].gameObject;
            btn_ = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            sta_ = uiHolder.elementTrsLst[2].GetComponent<Sta>();
            txt_ = uiHolder.elementTrsLst[3].GetComponent<Txt>();
        }

    }
    public partial class UiPartCtrl:UiCtrl
    {
        public UiPartView view;
        public UiPartModel model;
        public UiPartParam param;
        public UiModStoryCharacterUnitAppearanceUnitCtrl parent=>(UiModStoryCharacterUnitAppearanceUnitCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiPartParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiPartView(uiHolder);
            model=new UiPartModel();


        }

    }
    public partial class UiPartModel:UiModel
    {
        
    }
    public partial class UiModStoryCharacterUnitAppearanceUnitParam:UiParam
    {
    }

    public partial class UiModStoryCharacterUnitAppearanceUnitView:UiView
    {

            public ScrView scr_enableParts;
            public Btn btn_delete;
            public ScrView scr_items;
            public Sta sta_show;
            public Ipt ipt_name;
            public Ipt ipt_scale;
            public Ipt ipt_interval;
            public Btn btn_deleteTex;
            public Btn btn_resetTex;
            public ScrView scr_equipParts;
            public Sta sta_equip;
            public ScrView scr_parts;
            public GameObject go_toggle;
            public UiToggleCtrl sub_Toggle;
            public GameObject go_item;
            public UiItemCtrl sub_Item;
            public Btn btn_image;
            public RImg rimg_image;
            public RectTransform rtf_image;
            public RectTransform rtf_axis;
            public UiAxisCtrl model_Axis;
            public GameObject go_equipPart;
            public UiEquipPartCtrl sub_EquipPart;
            public Btn btn_itemStyle;
            public Btn btn_plus;
            public Btn btn_minus;
            public GameObject go_part;
            public UiPartCtrl sub_Part;
            public Txt txt_;
            public Txt txt_layer;
        public UiModStoryCharacterUnitAppearanceUnitView(UiHolder uiHolder):base(uiHolder)
        {

            scr_enableParts = uiHolder.elementTrsLst[0].GetComponent<ScrView>();
            btn_delete = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            scr_items = uiHolder.elementTrsLst[2].GetComponent<ScrView>();
            sta_show = uiHolder.elementTrsLst[3].GetComponent<Sta>();
            ipt_name = uiHolder.elementTrsLst[4].GetComponent<Ipt>();
            ipt_scale = uiHolder.elementTrsLst[5].GetComponent<Ipt>();
            ipt_interval = uiHolder.elementTrsLst[6].GetComponent<Ipt>();
            btn_deleteTex = uiHolder.elementTrsLst[7].GetComponent<Btn>();
            btn_resetTex = uiHolder.elementTrsLst[8].GetComponent<Btn>();
            scr_equipParts = uiHolder.elementTrsLst[9].GetComponent<ScrView>();
            sta_equip = uiHolder.elementTrsLst[10].GetComponent<Sta>();
            scr_parts = uiHolder.elementTrsLst[11].GetComponent<ScrView>();
            go_toggle = uiHolder.elementTrsLst[12].gameObject;
            sub_Toggle = (UiToggleCtrl) uiHolder.elementTrsLst[13].GetComponent<UiHolder>().ctrl;
            go_item = uiHolder.elementTrsLst[14].gameObject;
            sub_Item = (UiItemCtrl) uiHolder.elementTrsLst[15].GetComponent<UiHolder>().ctrl;
            btn_image = uiHolder.elementTrsLst[16].GetComponent<Btn>();
            rimg_image = uiHolder.elementTrsLst[17].GetComponent<RImg>();
            rtf_image = uiHolder.elementTrsLst[18].GetComponent<RectTransform>();
            rtf_axis = uiHolder.elementTrsLst[19].GetComponent<RectTransform>();
            model_Axis = (UiAxisCtrl) uiHolder.elementTrsLst[20].GetComponent<UiHolder>().ctrl;
            go_equipPart = uiHolder.elementTrsLst[21].gameObject;
            sub_EquipPart = (UiEquipPartCtrl) uiHolder.elementTrsLst[22].GetComponent<UiHolder>().ctrl;
            btn_itemStyle = uiHolder.elementTrsLst[23].GetComponent<Btn>();
            btn_plus = uiHolder.elementTrsLst[24].GetComponent<Btn>();
            btn_minus = uiHolder.elementTrsLst[25].GetComponent<Btn>();
            go_part = uiHolder.elementTrsLst[26].gameObject;
            sub_Part = (UiPartCtrl) uiHolder.elementTrsLst[27].GetComponent<UiHolder>().ctrl;
            txt_ = uiHolder.elementTrsLst[28].GetComponent<Txt>();
            txt_layer = uiHolder.elementTrsLst[29].GetComponent<Txt>();
        }

    }
    public partial class UiModStoryCharacterUnitAppearanceUnitCtrl:UiCtrl
    {
        public UiModStoryCharacterUnitAppearanceUnitView view;
        public UiModStoryCharacterUnitAppearanceUnitModel model;
        public UiModStoryCharacterUnitAppearanceUnitParam param;
        public UiModStoryCharacterUnitAppearanceCtrl parent=>(UiModStoryCharacterUnitAppearanceCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryCharacterUnitAppearanceUnitParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryCharacterUnitAppearanceUnitView(uiHolder);
            model=new UiModStoryCharacterUnitAppearanceUnitModel();


            view.sub_Toggle = new UiToggleCtrl();
            view.sub_Toggle.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
            view.sub_Item = new UiItemCtrl();
            view.sub_Item.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
            view.model_Axis = new UiAxisCtrl();
            view.model_Axis.BindHolderRecursively(uiHolder.subUiHolderLst[2]);
            view.sub_EquipPart = new UiEquipPartCtrl();
            view.sub_EquipPart.BindHolderRecursively(uiHolder.subUiHolderLst[3]);
            view.sub_Part = new UiPartCtrl();
            view.sub_Part.BindHolderRecursively(uiHolder.subUiHolderLst[4]);
        }

    }
    public partial class UiModStoryCharacterUnitAppearanceUnitModel:UiModel
    {
        
    }
}

    public partial class UiModStoryCharacterUnitAppearanceParam:UiParam
    {
    }

    public partial class UiModStoryCharacterUnitAppearanceView:UiView
    {

            public ModStoryCharacterUnitAppearanceList.UiModStoryCharacterUnitAppearanceListCtrl page_ModStoryCharacterUnitAppearanceList;
            public ModStoryCharacterUnitAppearanceUnit.UiModStoryCharacterUnitAppearanceUnitCtrl page_ModStoryCharacterUnitAppearanceUnit;
        public UiModStoryCharacterUnitAppearanceView(UiHolder uiHolder):base(uiHolder)
        {

            page_ModStoryCharacterUnitAppearanceList = (ModStoryCharacterUnitAppearanceList.UiModStoryCharacterUnitAppearanceListCtrl) uiHolder.elementTrsLst[0].GetComponent<UiHolder>().ctrl;
            page_ModStoryCharacterUnitAppearanceUnit = (ModStoryCharacterUnitAppearanceUnit.UiModStoryCharacterUnitAppearanceUnitCtrl) uiHolder.elementTrsLst[1].GetComponent<UiHolder>().ctrl;
        }

    }
    public partial class UiModStoryCharacterUnitAppearanceCtrl:UiCtrl
    {
        public UiModStoryCharacterUnitAppearanceView view;
        public UiModStoryCharacterUnitAppearanceModel model;
        public UiModStoryCharacterUnitAppearanceParam param;
        public UiModStoryCharacterUnitCtrl parent=>(UiModStoryCharacterUnitCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryCharacterUnitAppearanceParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryCharacterUnitAppearanceView(uiHolder);
            model=new UiModStoryCharacterUnitAppearanceModel();


            view.page_ModStoryCharacterUnitAppearanceList = new ModStoryCharacterUnitAppearanceList.UiModStoryCharacterUnitAppearanceListCtrl();
            view.page_ModStoryCharacterUnitAppearanceList.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
            view.page_ModStoryCharacterUnitAppearanceUnit = new ModStoryCharacterUnitAppearanceUnit.UiModStoryCharacterUnitAppearanceUnitCtrl();
            view.page_ModStoryCharacterUnitAppearanceUnit.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
        }

    }
    public partial class UiModStoryCharacterUnitAppearanceModel:UiModel
    {
        
    }
}

namespace ModStoryCharacterUnitConfig

{




    public partial class UiModStoryCharacterUnitConfigParam:UiParam
    {
    }

    public partial class UiModStoryCharacterUnitConfigView:UiView
    {

            public Btn btn_hpArgument;
            public Btn btn_moveSpeedParameter;
            public Btn btn_idleAnim;
            public Btn btn_moveAnim;
            public Btn btn_unique;
            public Sta sta_unique;
            public Txt txt_hpArgument;
            public Txt txt_moveSpeedParameter;
            public Txt txt_idleAnim;
            public Txt txt_moveAnim;
            public Btn btn_onTouchEvent;
            public Btn btn_onTouchEventTrigger;
            public Btn btn_onLeaveEvent;
            public Btn btn_onLeaveEventTrigger;
            public Btn btn_onShowEvent;
            public Btn btn_onShowEventTrigger;
            public Txt txt_onTouchEvent;
            public Txt txt_onTouchEventTrigger;
            public Txt txt_onLeaveEvent;
            public Txt txt_onLeaveEventTrigger;
            public Txt txt_onShowEvent;
            public Txt txt_onShowEventTrigger;
        public UiModStoryCharacterUnitConfigView(UiHolder uiHolder):base(uiHolder)
        {

            btn_hpArgument = uiHolder.elementTrsLst[0].GetComponent<Btn>();
            btn_moveSpeedParameter = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            btn_idleAnim = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            btn_moveAnim = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            btn_unique = uiHolder.elementTrsLst[4].GetComponent<Btn>();
            sta_unique = uiHolder.elementTrsLst[5].GetComponent<Sta>();
            txt_hpArgument = uiHolder.elementTrsLst[6].GetComponent<Txt>();
            txt_moveSpeedParameter = uiHolder.elementTrsLst[7].GetComponent<Txt>();
            txt_idleAnim = uiHolder.elementTrsLst[8].GetComponent<Txt>();
            txt_moveAnim = uiHolder.elementTrsLst[9].GetComponent<Txt>();
            btn_onTouchEvent = uiHolder.elementTrsLst[10].GetComponent<Btn>();
            btn_onTouchEventTrigger = uiHolder.elementTrsLst[11].GetComponent<Btn>();
            btn_onLeaveEvent = uiHolder.elementTrsLst[12].GetComponent<Btn>();
            btn_onLeaveEventTrigger = uiHolder.elementTrsLst[13].GetComponent<Btn>();
            btn_onShowEvent = uiHolder.elementTrsLst[14].GetComponent<Btn>();
            btn_onShowEventTrigger = uiHolder.elementTrsLst[15].GetComponent<Btn>();
            txt_onTouchEvent = uiHolder.elementTrsLst[16].GetComponent<Txt>();
            txt_onTouchEventTrigger = uiHolder.elementTrsLst[17].GetComponent<Txt>();
            txt_onLeaveEvent = uiHolder.elementTrsLst[18].GetComponent<Txt>();
            txt_onLeaveEventTrigger = uiHolder.elementTrsLst[19].GetComponent<Txt>();
            txt_onShowEvent = uiHolder.elementTrsLst[20].GetComponent<Txt>();
            txt_onShowEventTrigger = uiHolder.elementTrsLst[21].GetComponent<Txt>();
        }

    }
    public partial class UiModStoryCharacterUnitConfigCtrl:UiCtrl
    {
        public UiModStoryCharacterUnitConfigView view;
        public UiModStoryCharacterUnitConfigModel model;
        public UiModStoryCharacterUnitConfigParam param;
        public UiModStoryCharacterUnitCtrl parent=>(UiModStoryCharacterUnitCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryCharacterUnitConfigParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryCharacterUnitConfigView(uiHolder);
            model=new UiModStoryCharacterUnitConfigModel();


        }

    }
    public partial class UiModStoryCharacterUnitConfigModel:UiModel
    {
        
    }
}

    public partial class UiModStoryCharacterUnitParam:UiParam
    {
    }

    public partial class UiModStoryCharacterUnitView:UiView
    {

            public Btn btn_back;
            public ModStoryCharacterUnitOverview.UiModStoryCharacterUnitOverviewCtrl page_ModStoryCharacterUnitOverview;
            public ModStoryCharacterUnitParameter.UiModStoryCharacterUnitParameterCtrl page_ModStoryCharacterUnitParameter;
            public ModStoryCharacterUnitAppearance.UiModStoryCharacterUnitAppearanceCtrl page_ModStoryCharacterUnitAppearance;
            public ModStoryCharacterUnitConfig.UiModStoryCharacterUnitConfigCtrl page_ModStoryCharacterUnitConfig;
            public Btn btn_overview;
            public Sta sta_overview;
            public Btn btn_parameter;
            public Sta sta_parameter;
            public Btn btn_appearance;
            public Sta sta_appearance;
            public Btn btn_config;
            public Sta sta_config;
        public UiModStoryCharacterUnitView(UiHolder uiHolder):base(uiHolder)
        {

            btn_back = uiHolder.elementTrsLst[0].GetComponent<Btn>();
            page_ModStoryCharacterUnitOverview = (ModStoryCharacterUnitOverview.UiModStoryCharacterUnitOverviewCtrl) uiHolder.elementTrsLst[1].GetComponent<UiHolder>().ctrl;
            page_ModStoryCharacterUnitParameter = (ModStoryCharacterUnitParameter.UiModStoryCharacterUnitParameterCtrl) uiHolder.elementTrsLst[2].GetComponent<UiHolder>().ctrl;
            page_ModStoryCharacterUnitAppearance = (ModStoryCharacterUnitAppearance.UiModStoryCharacterUnitAppearanceCtrl) uiHolder.elementTrsLst[3].GetComponent<UiHolder>().ctrl;
            page_ModStoryCharacterUnitConfig = (ModStoryCharacterUnitConfig.UiModStoryCharacterUnitConfigCtrl) uiHolder.elementTrsLst[4].GetComponent<UiHolder>().ctrl;
            btn_overview = uiHolder.elementTrsLst[5].GetComponent<Btn>();
            sta_overview = uiHolder.elementTrsLst[6].GetComponent<Sta>();
            btn_parameter = uiHolder.elementTrsLst[7].GetComponent<Btn>();
            sta_parameter = uiHolder.elementTrsLst[8].GetComponent<Sta>();
            btn_appearance = uiHolder.elementTrsLst[9].GetComponent<Btn>();
            sta_appearance = uiHolder.elementTrsLst[10].GetComponent<Sta>();
            btn_config = uiHolder.elementTrsLst[11].GetComponent<Btn>();
            sta_config = uiHolder.elementTrsLst[12].GetComponent<Sta>();
        }

    }
    public partial class UiModStoryCharacterUnitCtrl:UiCtrl
    {
        public UiModStoryCharacterUnitView view;
        public UiModStoryCharacterUnitModel model;
        public UiModStoryCharacterUnitParam param;
        public UiModStoryCharacterCtrl parent=>(UiModStoryCharacterCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryCharacterUnitParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryCharacterUnitView(uiHolder);
            model=new UiModStoryCharacterUnitModel();


            view.page_ModStoryCharacterUnitOverview = new ModStoryCharacterUnitOverview.UiModStoryCharacterUnitOverviewCtrl();
            view.page_ModStoryCharacterUnitOverview.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
            view.page_ModStoryCharacterUnitParameter = new ModStoryCharacterUnitParameter.UiModStoryCharacterUnitParameterCtrl();
            view.page_ModStoryCharacterUnitParameter.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
            view.page_ModStoryCharacterUnitAppearance = new ModStoryCharacterUnitAppearance.UiModStoryCharacterUnitAppearanceCtrl();
            view.page_ModStoryCharacterUnitAppearance.BindHolderRecursively(uiHolder.subUiHolderLst[2]);
            view.page_ModStoryCharacterUnitConfig = new ModStoryCharacterUnitConfig.UiModStoryCharacterUnitConfigCtrl();
            view.page_ModStoryCharacterUnitConfig.BindHolderRecursively(uiHolder.subUiHolderLst[3]);
        }

    }
    public partial class UiModStoryCharacterUnitModel:UiModel
    {
        
    }
}

    public partial class UiModStoryCharacterParam:UiParam
    {
    }

    public partial class UiModStoryCharacterView:UiView
    {

            public ModStoryCharacterList.UiModStoryCharacterListCtrl page_ModStoryCharacterList;
            public ModStoryCharacterUnit.UiModStoryCharacterUnitCtrl page_ModStoryCharacterUnit;
        public UiModStoryCharacterView(UiHolder uiHolder):base(uiHolder)
        {

            page_ModStoryCharacterList = (ModStoryCharacterList.UiModStoryCharacterListCtrl) uiHolder.elementTrsLst[0].GetComponent<UiHolder>().ctrl;
            page_ModStoryCharacterUnit = (ModStoryCharacterUnit.UiModStoryCharacterUnitCtrl) uiHolder.elementTrsLst[1].GetComponent<UiHolder>().ctrl;
        }

    }
    public partial class UiModStoryCharacterCtrl:UiCtrl
    {
        public UiModStoryCharacterView view;
        public UiModStoryCharacterModel model;
        public UiModStoryCharacterParam param;
        public UiModStoryCtrl parent=>(UiModStoryCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryCharacterParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryCharacterView(uiHolder);
            model=new UiModStoryCharacterModel();


            view.page_ModStoryCharacterList = new ModStoryCharacterList.UiModStoryCharacterListCtrl();
            view.page_ModStoryCharacterList.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
            view.page_ModStoryCharacterUnit = new ModStoryCharacterUnit.UiModStoryCharacterUnitCtrl();
            view.page_ModStoryCharacterUnit.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
        }

    }
    public partial class UiModStoryCharacterModel:UiModel
    {
        
    }
}

namespace ModStorySkill

{




namespace ModStorySkillList

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
        public UiModStorySkillListCtrl parent=>(UiModStorySkillListCtrl)uiHolder.parent.ctrl;

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



    public partial class UiBigItemParam:UiParam
    {
    }

    public partial class UiBigItemView:UiView
    {

            public GameObject go_bigItem;
            public Sta sta_bigItem;
            public Sta sta_exist;
            public Btn btn_new;
            public Btn btn_;
            public Sta sta_;
            public Txt txt_;
            public Img img_;
        public UiBigItemView(UiHolder uiHolder):base(uiHolder)
        {

            go_bigItem = uiHolder.elementTrsLst[0].gameObject;
            sta_bigItem = uiHolder.elementTrsLst[1].GetComponent<Sta>();
            sta_exist = uiHolder.elementTrsLst[2].GetComponent<Sta>();
            btn_new = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            btn_ = uiHolder.elementTrsLst[4].GetComponent<Btn>();
            sta_ = uiHolder.elementTrsLst[5].GetComponent<Sta>();
            txt_ = uiHolder.elementTrsLst[6].GetComponent<Txt>();
            img_ = uiHolder.elementTrsLst[7].GetComponent<Img>();
        }

    }
    public partial class UiBigItemCtrl:UiCtrl
    {
        public UiBigItemView view;
        public UiBigItemModel model;
        public UiBigItemParam param;
        public UiModStorySkillListCtrl parent=>(UiModStorySkillListCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiBigItemParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiBigItemView(uiHolder);
            model=new UiBigItemModel();


        }

    }
    public partial class UiBigItemModel:UiModel
    {
        
    }
    public partial class UiModStorySkillListParam:UiParam
    {
    }

    public partial class UiModStorySkillListView:UiView
    {

            public ScrView scr_labs;
            public ScrView scr_bigItems;
            public GameObject go_lab;
            public UiLabCtrl sub_Lab;
            public GameObject go_bigItem;
            public Sta sta_bigItem;
            public UiBigItemCtrl sub_BigItem;
        public UiModStorySkillListView(UiHolder uiHolder):base(uiHolder)
        {

            scr_labs = uiHolder.elementTrsLst[0].GetComponent<ScrView>();
            scr_bigItems = uiHolder.elementTrsLst[1].GetComponent<ScrView>();
            go_lab = uiHolder.elementTrsLst[2].gameObject;
            sub_Lab = (UiLabCtrl) uiHolder.elementTrsLst[3].GetComponent<UiHolder>().ctrl;
            go_bigItem = uiHolder.elementTrsLst[4].gameObject;
            sta_bigItem = uiHolder.elementTrsLst[5].GetComponent<Sta>();
            sub_BigItem = (UiBigItemCtrl) uiHolder.elementTrsLst[6].GetComponent<UiHolder>().ctrl;
        }

    }
    public partial class UiModStorySkillListCtrl:UiCtrl
    {
        public UiModStorySkillListView view;
        public UiModStorySkillListModel model;
        public UiModStorySkillListParam param;
        public UiModStorySkillCtrl parent=>(UiModStorySkillCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStorySkillListParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStorySkillListView(uiHolder);
            model=new UiModStorySkillListModel();


            view.sub_Lab = new UiLabCtrl();
            view.sub_Lab.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
            view.sub_BigItem = new UiBigItemCtrl();
            view.sub_BigItem.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
        }

    }
    public partial class UiModStorySkillListModel:UiModel
    {
        
    }
}

namespace ModStorySkillUnit

{




    public partial class UiModStorySkillUnitParam:UiParam
    {
    }

    public partial class UiModStorySkillUnitView:UiView
    {

            public Btn btn_back;
            public Btn btn_delete;
            public Ipt ipt_name;
            public Ipt ipt_label;
            public Ipt ipt_cd;
            public Btn btn_icon;
            public Img img_icon;
            public Btn btn_triggerCondition;
            public Btn btn_onTriggerEvent;
            public Btn btn_lightAttack;
            public Sta sta_lightAttack;
            public Btn btn_heavyAttack;
            public Sta sta_heavyAttack;
            public Btn btn_e;
            public Sta sta_e;
            public Btn btn_q;
            public Sta sta_q;
            public Txt txt_triggerCondition;
            public Txt txt_onTriggerEvent;
        public UiModStorySkillUnitView(UiHolder uiHolder):base(uiHolder)
        {

            btn_back = uiHolder.elementTrsLst[0].GetComponent<Btn>();
            btn_delete = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            ipt_name = uiHolder.elementTrsLst[2].GetComponent<Ipt>();
            ipt_label = uiHolder.elementTrsLst[3].GetComponent<Ipt>();
            ipt_cd = uiHolder.elementTrsLst[4].GetComponent<Ipt>();
            btn_icon = uiHolder.elementTrsLst[5].GetComponent<Btn>();
            img_icon = uiHolder.elementTrsLst[6].GetComponent<Img>();
            btn_triggerCondition = uiHolder.elementTrsLst[7].GetComponent<Btn>();
            btn_onTriggerEvent = uiHolder.elementTrsLst[8].GetComponent<Btn>();
            btn_lightAttack = uiHolder.elementTrsLst[9].GetComponent<Btn>();
            sta_lightAttack = uiHolder.elementTrsLst[10].GetComponent<Sta>();
            btn_heavyAttack = uiHolder.elementTrsLst[11].GetComponent<Btn>();
            sta_heavyAttack = uiHolder.elementTrsLst[12].GetComponent<Sta>();
            btn_e = uiHolder.elementTrsLst[13].GetComponent<Btn>();
            sta_e = uiHolder.elementTrsLst[14].GetComponent<Sta>();
            btn_q = uiHolder.elementTrsLst[15].GetComponent<Btn>();
            sta_q = uiHolder.elementTrsLst[16].GetComponent<Sta>();
            txt_triggerCondition = uiHolder.elementTrsLst[17].GetComponent<Txt>();
            txt_onTriggerEvent = uiHolder.elementTrsLst[18].GetComponent<Txt>();
        }

    }
    public partial class UiModStorySkillUnitCtrl:UiCtrl
    {
        public UiModStorySkillUnitView view;
        public UiModStorySkillUnitModel model;
        public UiModStorySkillUnitParam param;
        public UiModStorySkillCtrl parent=>(UiModStorySkillCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStorySkillUnitParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStorySkillUnitView(uiHolder);
            model=new UiModStorySkillUnitModel();


        }

    }
    public partial class UiModStorySkillUnitModel:UiModel
    {
        
    }
}

    public partial class UiModStorySkillParam:UiParam
    {
    }

    public partial class UiModStorySkillView:UiView
    {

            public ModStorySkillList.UiModStorySkillListCtrl page_ModStorySkillList;
            public ModStorySkillUnit.UiModStorySkillUnitCtrl page_ModStorySkillUnit;
        public UiModStorySkillView(UiHolder uiHolder):base(uiHolder)
        {

            page_ModStorySkillList = (ModStorySkillList.UiModStorySkillListCtrl) uiHolder.elementTrsLst[0].GetComponent<UiHolder>().ctrl;
            page_ModStorySkillUnit = (ModStorySkillUnit.UiModStorySkillUnitCtrl) uiHolder.elementTrsLst[1].GetComponent<UiHolder>().ctrl;
        }

    }
    public partial class UiModStorySkillCtrl:UiCtrl
    {
        public UiModStorySkillView view;
        public UiModStorySkillModel model;
        public UiModStorySkillParam param;
        public UiModStoryCtrl parent=>(UiModStoryCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStorySkillParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStorySkillView(uiHolder);
            model=new UiModStorySkillModel();


            view.page_ModStorySkillList = new ModStorySkillList.UiModStorySkillListCtrl();
            view.page_ModStorySkillList.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
            view.page_ModStorySkillUnit = new ModStorySkillUnit.UiModStorySkillUnitCtrl();
            view.page_ModStorySkillUnit.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
        }

    }
    public partial class UiModStorySkillModel:UiModel
    {
        
    }
}

namespace ModStoryItem

{




namespace ModStoryItemList

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
        public UiModStoryItemListCtrl parent=>(UiModStoryItemListCtrl)uiHolder.parent.ctrl;

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



    public partial class UiBigItemParam:UiParam
    {
    }

    public partial class UiBigItemView:UiView
    {

            public GameObject go_bigItem;
            public Sta sta_bigItem;
            public Sta sta_exist;
            public Btn btn_new;
            public Btn btn_;
            public Sta sta_;
            public Txt txt_;
            public Img img_;
        public UiBigItemView(UiHolder uiHolder):base(uiHolder)
        {

            go_bigItem = uiHolder.elementTrsLst[0].gameObject;
            sta_bigItem = uiHolder.elementTrsLst[1].GetComponent<Sta>();
            sta_exist = uiHolder.elementTrsLst[2].GetComponent<Sta>();
            btn_new = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            btn_ = uiHolder.elementTrsLst[4].GetComponent<Btn>();
            sta_ = uiHolder.elementTrsLst[5].GetComponent<Sta>();
            txt_ = uiHolder.elementTrsLst[6].GetComponent<Txt>();
            img_ = uiHolder.elementTrsLst[7].GetComponent<Img>();
        }

    }
    public partial class UiBigItemCtrl:UiCtrl
    {
        public UiBigItemView view;
        public UiBigItemModel model;
        public UiBigItemParam param;
        public UiModStoryItemListCtrl parent=>(UiModStoryItemListCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiBigItemParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiBigItemView(uiHolder);
            model=new UiBigItemModel();


        }

    }
    public partial class UiBigItemModel:UiModel
    {
        
    }
    public partial class UiModStoryItemListParam:UiParam
    {
    }

    public partial class UiModStoryItemListView:UiView
    {

            public ScrView scr_labs;
            public ScrView scr_bigItems;
            public GameObject go_lab;
            public UiLabCtrl sub_Lab;
            public GameObject go_bigItem;
            public Sta sta_bigItem;
            public UiBigItemCtrl sub_BigItem;
        public UiModStoryItemListView(UiHolder uiHolder):base(uiHolder)
        {

            scr_labs = uiHolder.elementTrsLst[0].GetComponent<ScrView>();
            scr_bigItems = uiHolder.elementTrsLst[1].GetComponent<ScrView>();
            go_lab = uiHolder.elementTrsLst[2].gameObject;
            sub_Lab = (UiLabCtrl) uiHolder.elementTrsLst[3].GetComponent<UiHolder>().ctrl;
            go_bigItem = uiHolder.elementTrsLst[4].gameObject;
            sta_bigItem = uiHolder.elementTrsLst[5].GetComponent<Sta>();
            sub_BigItem = (UiBigItemCtrl) uiHolder.elementTrsLst[6].GetComponent<UiHolder>().ctrl;
        }

    }
    public partial class UiModStoryItemListCtrl:UiCtrl
    {
        public UiModStoryItemListView view;
        public UiModStoryItemListModel model;
        public UiModStoryItemListParam param;
        public UiModStoryItemCtrl parent=>(UiModStoryItemCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryItemListParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryItemListView(uiHolder);
            model=new UiModStoryItemListModel();


            view.sub_Lab = new UiLabCtrl();
            view.sub_Lab.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
            view.sub_BigItem = new UiBigItemCtrl();
            view.sub_BigItem.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
        }

    }
    public partial class UiModStoryItemListModel:UiModel
    {
        
    }
}

namespace ModStoryItemUnit

{




namespace ModStoryItemUnitOverview

{




    public partial class UiModStoryItemUnitOverviewParam:UiParam
    {
    }

    public partial class UiModStoryItemUnitOverviewView:UiView
    {

            public Ipt ipt_desc;
            public Btn btn_delete;
            public Ipt ipt_name;
            public Ipt ipt_label;
            public Btn btn_image;
            public Img img_image;
        public UiModStoryItemUnitOverviewView(UiHolder uiHolder):base(uiHolder)
        {

            ipt_desc = uiHolder.elementTrsLst[0].GetComponent<Ipt>();
            btn_delete = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            ipt_name = uiHolder.elementTrsLst[2].GetComponent<Ipt>();
            ipt_label = uiHolder.elementTrsLst[3].GetComponent<Ipt>();
            btn_image = uiHolder.elementTrsLst[4].GetComponent<Btn>();
            img_image = uiHolder.elementTrsLst[5].GetComponent<Img>();
        }

    }
    public partial class UiModStoryItemUnitOverviewCtrl:UiCtrl
    {
        public UiModStoryItemUnitOverviewView view;
        public UiModStoryItemUnitOverviewModel model;
        public UiModStoryItemUnitOverviewParam param;
        public UiModStoryItemUnitCtrl parent=>(UiModStoryItemUnitCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryItemUnitOverviewParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryItemUnitOverviewView(uiHolder);
            model=new UiModStoryItemUnitOverviewModel();


        }

    }
    public partial class UiModStoryItemUnitOverviewModel:UiModel
    {
        
    }
}

namespace ModStoryItemUnitParameter

{







    public partial class UiArgIptParam:UiParam
    {
    }

    public partial class UiArgIptView:UiView
    {

            public GameObject go_argIpt;
            public Txt txt_;
            public Ipt ipt_min;
            public Ipt ipt_value;
            public Ipt ipt_max;
        public UiArgIptView(UiHolder uiHolder):base(uiHolder)
        {

            go_argIpt = uiHolder.elementTrsLst[0].gameObject;
            txt_ = uiHolder.elementTrsLst[1].GetComponent<Txt>();
            ipt_min = uiHolder.elementTrsLst[2].GetComponent<Ipt>();
            ipt_value = uiHolder.elementTrsLst[3].GetComponent<Ipt>();
            ipt_max = uiHolder.elementTrsLst[4].GetComponent<Ipt>();
        }

    }
    public partial class UiArgIptCtrl:UiCtrl
    {
        public UiArgIptView view;
        public UiArgIptModel model;
        public UiArgIptParam param;
        public UiModStoryItemUnitParameterCtrl parent=>(UiModStoryItemUnitParameterCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiArgIptParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiArgIptView(uiHolder);
            model=new UiArgIptModel();


        }

    }
    public partial class UiArgIptModel:UiModel
    {
        
    }
    public partial class UiModStoryItemUnitParameterParam:UiParam
    {
    }

    public partial class UiModStoryItemUnitParameterView:UiView
    {

            public ScrView scr_argIpts;
            public GameObject go_argIpt;
            public UiArgIptCtrl sub_ArgIpt;
        public UiModStoryItemUnitParameterView(UiHolder uiHolder):base(uiHolder)
        {

            scr_argIpts = uiHolder.elementTrsLst[0].GetComponent<ScrView>();
            go_argIpt = uiHolder.elementTrsLst[1].gameObject;
            sub_ArgIpt = (UiArgIptCtrl) uiHolder.elementTrsLst[2].GetComponent<UiHolder>().ctrl;
        }

    }
    public partial class UiModStoryItemUnitParameterCtrl:UiCtrl
    {
        public UiModStoryItemUnitParameterView view;
        public UiModStoryItemUnitParameterModel model;
        public UiModStoryItemUnitParameterParam param;
        public UiModStoryItemUnitCtrl parent=>(UiModStoryItemUnitCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryItemUnitParameterParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryItemUnitParameterView(uiHolder);
            model=new UiModStoryItemUnitParameterModel();


            view.sub_ArgIpt = new UiArgIptCtrl();
            view.sub_ArgIpt.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
        }

    }
    public partial class UiModStoryItemUnitParameterModel:UiModel
    {
        
    }
}

namespace ModStoryItemUnitAppearance

{

using Ui.Axis;






    public partial class UiStyleParam:UiParam
    {
    }

    public partial class UiStyleView:UiView
    {

            public GameObject go_style;
            public Sta sta_exist;
            public Btn btn_new;
            public Btn btn_;
            public Sta sta_;
            public Txt txt_;
            public Img img_;
        public UiStyleView(UiHolder uiHolder):base(uiHolder)
        {

            go_style = uiHolder.elementTrsLst[0].gameObject;
            sta_exist = uiHolder.elementTrsLst[1].GetComponent<Sta>();
            btn_new = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            btn_ = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            sta_ = uiHolder.elementTrsLst[4].GetComponent<Sta>();
            txt_ = uiHolder.elementTrsLst[5].GetComponent<Txt>();
            img_ = uiHolder.elementTrsLst[6].GetComponent<Img>();
        }

    }
    public partial class UiStyleCtrl:UiCtrl
    {
        public UiStyleView view;
        public UiStyleModel model;
        public UiStyleParam param;
        public UiModStoryItemUnitAppearanceCtrl parent=>(UiModStoryItemUnitAppearanceCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiStyleParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiStyleView(uiHolder);
            model=new UiStyleModel();


        }

    }
    public partial class UiStyleModel:UiModel
    {
        
    }



    public partial class UiItemParam:UiParam
    {
    }

    public partial class UiItemView:UiView
    {

            public GameObject go_item;
            public Sta sta_exist;
            public Btn btn_new;
            public Btn btn_;
            public Sta sta_;
            public Img img_;
            public Txt txt_;
        public UiItemView(UiHolder uiHolder):base(uiHolder)
        {

            go_item = uiHolder.elementTrsLst[0].gameObject;
            sta_exist = uiHolder.elementTrsLst[1].GetComponent<Sta>();
            btn_new = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            btn_ = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            sta_ = uiHolder.elementTrsLst[4].GetComponent<Sta>();
            img_ = uiHolder.elementTrsLst[5].GetComponent<Img>();
            txt_ = uiHolder.elementTrsLst[6].GetComponent<Txt>();
        }

    }
    public partial class UiItemCtrl:UiCtrl
    {
        public UiItemView view;
        public UiItemModel model;
        public UiItemParam param;
        public UiModStoryItemUnitAppearanceCtrl parent=>(UiModStoryItemUnitAppearanceCtrl)uiHolder.parent.ctrl;

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
    public partial class UiModStoryItemUnitAppearanceParam:UiParam
    {
    }

    public partial class UiModStoryItemUnitAppearanceView:UiView
    {

            public ScrView scr_styles;
            public Sta sta_show;
            public ScrView scr_items;
            public Btn btn_delete;
            public GameObject go_style;
            public UiStyleCtrl sub_Style;
            public Ipt ipt_width;
            public Ipt ipt_length;
            public Ipt ipt_height;
            public Btn btn_reset;
            public Btn btn_model;
            public Btn btn_image;
            public RImg rimg_image;
            public RectTransform rtf_image;
            public RectTransform rtf_axis;
            public UiAxisCtrl model_Axis;
            public GameObject go_item;
            public UiItemCtrl sub_Item;
            public Txt txt_model;
        public UiModStoryItemUnitAppearanceView(UiHolder uiHolder):base(uiHolder)
        {

            scr_styles = uiHolder.elementTrsLst[0].GetComponent<ScrView>();
            sta_show = uiHolder.elementTrsLst[1].GetComponent<Sta>();
            scr_items = uiHolder.elementTrsLst[2].GetComponent<ScrView>();
            btn_delete = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            go_style = uiHolder.elementTrsLst[4].gameObject;
            sub_Style = (UiStyleCtrl) uiHolder.elementTrsLst[5].GetComponent<UiHolder>().ctrl;
            ipt_width = uiHolder.elementTrsLst[6].GetComponent<Ipt>();
            ipt_length = uiHolder.elementTrsLst[7].GetComponent<Ipt>();
            ipt_height = uiHolder.elementTrsLst[8].GetComponent<Ipt>();
            btn_reset = uiHolder.elementTrsLst[9].GetComponent<Btn>();
            btn_model = uiHolder.elementTrsLst[10].GetComponent<Btn>();
            btn_image = uiHolder.elementTrsLst[11].GetComponent<Btn>();
            rimg_image = uiHolder.elementTrsLst[12].GetComponent<RImg>();
            rtf_image = uiHolder.elementTrsLst[13].GetComponent<RectTransform>();
            rtf_axis = uiHolder.elementTrsLst[14].GetComponent<RectTransform>();
            model_Axis = (UiAxisCtrl) uiHolder.elementTrsLst[15].GetComponent<UiHolder>().ctrl;
            go_item = uiHolder.elementTrsLst[16].gameObject;
            sub_Item = (UiItemCtrl) uiHolder.elementTrsLst[17].GetComponent<UiHolder>().ctrl;
            txt_model = uiHolder.elementTrsLst[18].GetComponent<Txt>();
        }

    }
    public partial class UiModStoryItemUnitAppearanceCtrl:UiCtrl
    {
        public UiModStoryItemUnitAppearanceView view;
        public UiModStoryItemUnitAppearanceModel model;
        public UiModStoryItemUnitAppearanceParam param;
        public UiModStoryItemUnitCtrl parent=>(UiModStoryItemUnitCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryItemUnitAppearanceParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryItemUnitAppearanceView(uiHolder);
            model=new UiModStoryItemUnitAppearanceModel();


            view.sub_Style = new UiStyleCtrl();
            view.sub_Style.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
            view.model_Axis = new UiAxisCtrl();
            view.model_Axis.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
            view.sub_Item = new UiItemCtrl();
            view.sub_Item.BindHolderRecursively(uiHolder.subUiHolderLst[2]);
        }

    }
    public partial class UiModStoryItemUnitAppearanceModel:UiModel
    {
        
    }
}

namespace ModStoryItemUnitConfig

{




    public partial class UiModStoryItemUnitConfigParam:UiParam
    {
    }

    public partial class UiModStoryItemUnitConfigView:UiView
    {

            public Sta sta_equip;
            public Btn btn_onDisequipEvent;
            public Btn btn_onEquipEvent;
            public Btn btn_part;
            public Btn btn_canEquipped;
            public Sta sta_canEquipped;
            public Ipt ipt_price;
            public Btn btn_onUseEvent;
            public Txt txt_onDisequipEvent;
            public Txt txt_onEquipEvent;
            public Txt txt_part;
            public Txt txt_onUseEvent;
            public Btn btn_onTouchEvent;
            public Btn btn_onTouchEventTrigger;
            public Btn btn_onLeaveEvent;
            public Btn btn_onLeaveEventTrigger;
            public Btn btn_onShowEvent;
            public Btn btn_onShowEventTrigger;
            public Txt txt_onTouchEvent;
            public Txt txt_onTouchEventTrigger;
            public Txt txt_onLeaveEvent;
            public Txt txt_onLeaveEventTrigger;
            public Txt txt_onShowEvent;
            public Txt txt_onShowEventTrigger;
        public UiModStoryItemUnitConfigView(UiHolder uiHolder):base(uiHolder)
        {

            sta_equip = uiHolder.elementTrsLst[0].GetComponent<Sta>();
            btn_onDisequipEvent = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            btn_onEquipEvent = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            btn_part = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            btn_canEquipped = uiHolder.elementTrsLst[4].GetComponent<Btn>();
            sta_canEquipped = uiHolder.elementTrsLst[5].GetComponent<Sta>();
            ipt_price = uiHolder.elementTrsLst[6].GetComponent<Ipt>();
            btn_onUseEvent = uiHolder.elementTrsLst[7].GetComponent<Btn>();
            txt_onDisequipEvent = uiHolder.elementTrsLst[8].GetComponent<Txt>();
            txt_onEquipEvent = uiHolder.elementTrsLst[9].GetComponent<Txt>();
            txt_part = uiHolder.elementTrsLst[10].GetComponent<Txt>();
            txt_onUseEvent = uiHolder.elementTrsLst[11].GetComponent<Txt>();
            btn_onTouchEvent = uiHolder.elementTrsLst[12].GetComponent<Btn>();
            btn_onTouchEventTrigger = uiHolder.elementTrsLst[13].GetComponent<Btn>();
            btn_onLeaveEvent = uiHolder.elementTrsLst[14].GetComponent<Btn>();
            btn_onLeaveEventTrigger = uiHolder.elementTrsLst[15].GetComponent<Btn>();
            btn_onShowEvent = uiHolder.elementTrsLst[16].GetComponent<Btn>();
            btn_onShowEventTrigger = uiHolder.elementTrsLst[17].GetComponent<Btn>();
            txt_onTouchEvent = uiHolder.elementTrsLst[18].GetComponent<Txt>();
            txt_onTouchEventTrigger = uiHolder.elementTrsLst[19].GetComponent<Txt>();
            txt_onLeaveEvent = uiHolder.elementTrsLst[20].GetComponent<Txt>();
            txt_onLeaveEventTrigger = uiHolder.elementTrsLst[21].GetComponent<Txt>();
            txt_onShowEvent = uiHolder.elementTrsLst[22].GetComponent<Txt>();
            txt_onShowEventTrigger = uiHolder.elementTrsLst[23].GetComponent<Txt>();
        }

    }
    public partial class UiModStoryItemUnitConfigCtrl:UiCtrl
    {
        public UiModStoryItemUnitConfigView view;
        public UiModStoryItemUnitConfigModel model;
        public UiModStoryItemUnitConfigParam param;
        public UiModStoryItemUnitCtrl parent=>(UiModStoryItemUnitCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryItemUnitConfigParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryItemUnitConfigView(uiHolder);
            model=new UiModStoryItemUnitConfigModel();


        }

    }
    public partial class UiModStoryItemUnitConfigModel:UiModel
    {
        
    }
}

    public partial class UiModStoryItemUnitParam:UiParam
    {
    }

    public partial class UiModStoryItemUnitView:UiView
    {

            public Btn btn_back;
            public ModStoryItemUnitOverview.UiModStoryItemUnitOverviewCtrl page_ModStoryItemUnitOverview;
            public ModStoryItemUnitParameter.UiModStoryItemUnitParameterCtrl page_ModStoryItemUnitParameter;
            public ModStoryItemUnitAppearance.UiModStoryItemUnitAppearanceCtrl page_ModStoryItemUnitAppearance;
            public ModStoryItemUnitConfig.UiModStoryItemUnitConfigCtrl page_ModStoryItemUnitConfig;
            public Btn btn_overview;
            public Sta sta_overview;
            public Btn btn_parameter;
            public Sta sta_parameter;
            public Btn btn_appearance;
            public Sta sta_appearance;
            public Btn btn_config;
            public Sta sta_config;
        public UiModStoryItemUnitView(UiHolder uiHolder):base(uiHolder)
        {

            btn_back = uiHolder.elementTrsLst[0].GetComponent<Btn>();
            page_ModStoryItemUnitOverview = (ModStoryItemUnitOverview.UiModStoryItemUnitOverviewCtrl) uiHolder.elementTrsLst[1].GetComponent<UiHolder>().ctrl;
            page_ModStoryItemUnitParameter = (ModStoryItemUnitParameter.UiModStoryItemUnitParameterCtrl) uiHolder.elementTrsLst[2].GetComponent<UiHolder>().ctrl;
            page_ModStoryItemUnitAppearance = (ModStoryItemUnitAppearance.UiModStoryItemUnitAppearanceCtrl) uiHolder.elementTrsLst[3].GetComponent<UiHolder>().ctrl;
            page_ModStoryItemUnitConfig = (ModStoryItemUnitConfig.UiModStoryItemUnitConfigCtrl) uiHolder.elementTrsLst[4].GetComponent<UiHolder>().ctrl;
            btn_overview = uiHolder.elementTrsLst[5].GetComponent<Btn>();
            sta_overview = uiHolder.elementTrsLst[6].GetComponent<Sta>();
            btn_parameter = uiHolder.elementTrsLst[7].GetComponent<Btn>();
            sta_parameter = uiHolder.elementTrsLst[8].GetComponent<Sta>();
            btn_appearance = uiHolder.elementTrsLst[9].GetComponent<Btn>();
            sta_appearance = uiHolder.elementTrsLst[10].GetComponent<Sta>();
            btn_config = uiHolder.elementTrsLst[11].GetComponent<Btn>();
            sta_config = uiHolder.elementTrsLst[12].GetComponent<Sta>();
        }

    }
    public partial class UiModStoryItemUnitCtrl:UiCtrl
    {
        public UiModStoryItemUnitView view;
        public UiModStoryItemUnitModel model;
        public UiModStoryItemUnitParam param;
        public UiModStoryItemCtrl parent=>(UiModStoryItemCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryItemUnitParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryItemUnitView(uiHolder);
            model=new UiModStoryItemUnitModel();


            view.page_ModStoryItemUnitOverview = new ModStoryItemUnitOverview.UiModStoryItemUnitOverviewCtrl();
            view.page_ModStoryItemUnitOverview.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
            view.page_ModStoryItemUnitParameter = new ModStoryItemUnitParameter.UiModStoryItemUnitParameterCtrl();
            view.page_ModStoryItemUnitParameter.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
            view.page_ModStoryItemUnitAppearance = new ModStoryItemUnitAppearance.UiModStoryItemUnitAppearanceCtrl();
            view.page_ModStoryItemUnitAppearance.BindHolderRecursively(uiHolder.subUiHolderLst[2]);
            view.page_ModStoryItemUnitConfig = new ModStoryItemUnitConfig.UiModStoryItemUnitConfigCtrl();
            view.page_ModStoryItemUnitConfig.BindHolderRecursively(uiHolder.subUiHolderLst[3]);
        }

    }
    public partial class UiModStoryItemUnitModel:UiModel
    {
        
    }
}

    public partial class UiModStoryItemParam:UiParam
    {
    }

    public partial class UiModStoryItemView:UiView
    {

            public ModStoryItemList.UiModStoryItemListCtrl page_ModStoryItemList;
            public ModStoryItemUnit.UiModStoryItemUnitCtrl page_ModStoryItemUnit;
        public UiModStoryItemView(UiHolder uiHolder):base(uiHolder)
        {

            page_ModStoryItemList = (ModStoryItemList.UiModStoryItemListCtrl) uiHolder.elementTrsLst[0].GetComponent<UiHolder>().ctrl;
            page_ModStoryItemUnit = (ModStoryItemUnit.UiModStoryItemUnitCtrl) uiHolder.elementTrsLst[1].GetComponent<UiHolder>().ctrl;
        }

    }
    public partial class UiModStoryItemCtrl:UiCtrl
    {
        public UiModStoryItemView view;
        public UiModStoryItemModel model;
        public UiModStoryItemParam param;
        public UiModStoryCtrl parent=>(UiModStoryCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryItemParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryItemView(uiHolder);
            model=new UiModStoryItemModel();


            view.page_ModStoryItemList = new ModStoryItemList.UiModStoryItemListCtrl();
            view.page_ModStoryItemList.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
            view.page_ModStoryItemUnit = new ModStoryItemUnit.UiModStoryItemUnitCtrl();
            view.page_ModStoryItemUnit.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
        }

    }
    public partial class UiModStoryItemModel:UiModel
    {
        
    }
}

namespace ModStoryMapObject

{




namespace ModStoryMapObjectType

{




    public partial class UiModStoryMapObjectTypeParam:UiParam
    {
    }

    public partial class UiModStoryMapObjectTypeView:UiView
    {

            public Btn btn_texture;
            public Btn btn_mask;
            public Btn btn_object;
        public UiModStoryMapObjectTypeView(UiHolder uiHolder):base(uiHolder)
        {

            btn_texture = uiHolder.elementTrsLst[0].GetComponent<Btn>();
            btn_mask = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            btn_object = uiHolder.elementTrsLst[2].GetComponent<Btn>();
        }

    }
    public partial class UiModStoryMapObjectTypeCtrl:UiCtrl
    {
        public UiModStoryMapObjectTypeView view;
        public UiModStoryMapObjectTypeModel model;
        public UiModStoryMapObjectTypeParam param;
        public UiModStoryMapObjectCtrl parent=>(UiModStoryMapObjectCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryMapObjectTypeParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryMapObjectTypeView(uiHolder);
            model=new UiModStoryMapObjectTypeModel();


        }

    }
    public partial class UiModStoryMapObjectTypeModel:UiModel
    {
        
    }
}

namespace ModStoryMapObjectList

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
        public UiModStoryMapObjectListCtrl parent=>(UiModStoryMapObjectListCtrl)uiHolder.parent.ctrl;

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



    public partial class UiBigItemParam:UiParam
    {
    }

    public partial class UiBigItemView:UiView
    {

            public GameObject go_bigItem;
            public Sta sta_bigItem;
            public Sta sta_exist;
            public Btn btn_new;
            public Btn btn_;
            public Sta sta_;
            public Txt txt_;
            public Img img_;
        public UiBigItemView(UiHolder uiHolder):base(uiHolder)
        {

            go_bigItem = uiHolder.elementTrsLst[0].gameObject;
            sta_bigItem = uiHolder.elementTrsLst[1].GetComponent<Sta>();
            sta_exist = uiHolder.elementTrsLst[2].GetComponent<Sta>();
            btn_new = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            btn_ = uiHolder.elementTrsLst[4].GetComponent<Btn>();
            sta_ = uiHolder.elementTrsLst[5].GetComponent<Sta>();
            txt_ = uiHolder.elementTrsLst[6].GetComponent<Txt>();
            img_ = uiHolder.elementTrsLst[7].GetComponent<Img>();
        }

    }
    public partial class UiBigItemCtrl:UiCtrl
    {
        public UiBigItemView view;
        public UiBigItemModel model;
        public UiBigItemParam param;
        public UiModStoryMapObjectListCtrl parent=>(UiModStoryMapObjectListCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiBigItemParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiBigItemView(uiHolder);
            model=new UiBigItemModel();


        }

    }
    public partial class UiBigItemModel:UiModel
    {
        
    }
    public partial class UiModStoryMapObjectListParam:UiParam
    {
    }

    public partial class UiModStoryMapObjectListView:UiView
    {

            public ScrView scr_labs;
            public Btn btn_back;
            public ScrView scr_bigItems;
            public GameObject go_lab;
            public UiLabCtrl sub_Lab;
            public GameObject go_bigItem;
            public Sta sta_bigItem;
            public UiBigItemCtrl sub_BigItem;
        public UiModStoryMapObjectListView(UiHolder uiHolder):base(uiHolder)
        {

            scr_labs = uiHolder.elementTrsLst[0].GetComponent<ScrView>();
            btn_back = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            scr_bigItems = uiHolder.elementTrsLst[2].GetComponent<ScrView>();
            go_lab = uiHolder.elementTrsLst[3].gameObject;
            sub_Lab = (UiLabCtrl) uiHolder.elementTrsLst[4].GetComponent<UiHolder>().ctrl;
            go_bigItem = uiHolder.elementTrsLst[5].gameObject;
            sta_bigItem = uiHolder.elementTrsLst[6].GetComponent<Sta>();
            sub_BigItem = (UiBigItemCtrl) uiHolder.elementTrsLst[7].GetComponent<UiHolder>().ctrl;
        }

    }
    public partial class UiModStoryMapObjectListCtrl:UiCtrl
    {
        public UiModStoryMapObjectListView view;
        public UiModStoryMapObjectListModel model;
        public UiModStoryMapObjectListParam param;
        public UiModStoryMapObjectCtrl parent=>(UiModStoryMapObjectCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryMapObjectListParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryMapObjectListView(uiHolder);
            model=new UiModStoryMapObjectListModel();


            view.sub_Lab = new UiLabCtrl();
            view.sub_Lab.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
            view.sub_BigItem = new UiBigItemCtrl();
            view.sub_BigItem.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
        }

    }
    public partial class UiModStoryMapObjectListModel:UiModel
    {
        
    }
}

namespace ModStoryMapObjectTexture

{




namespace ModStoryMapObjectTextureAppearance

{







    public partial class UiItemParam:UiParam
    {
    }

    public partial class UiItemView:UiView
    {

            public GameObject go_item;
            public Sta sta_exist;
            public Btn btn_new;
            public Btn btn_;
            public Sta sta_;
            public Img img_;
            public Txt txt_;
        public UiItemView(UiHolder uiHolder):base(uiHolder)
        {

            go_item = uiHolder.elementTrsLst[0].gameObject;
            sta_exist = uiHolder.elementTrsLst[1].GetComponent<Sta>();
            btn_new = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            btn_ = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            sta_ = uiHolder.elementTrsLst[4].GetComponent<Sta>();
            img_ = uiHolder.elementTrsLst[5].GetComponent<Img>();
            txt_ = uiHolder.elementTrsLst[6].GetComponent<Txt>();
        }

    }
    public partial class UiItemCtrl:UiCtrl
    {
        public UiItemView view;
        public UiItemModel model;
        public UiItemParam param;
        public UiModStoryMapObjectTextureAppearanceCtrl parent=>(UiModStoryMapObjectTextureAppearanceCtrl)uiHolder.parent.ctrl;

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
    public partial class UiModStoryMapObjectTextureAppearanceParam:UiParam
    {
    }

    public partial class UiModStoryMapObjectTextureAppearanceView:UiView
    {

            public Btn btn_delete;
            public ScrView scr_items;
            public Sta sta_show;
            public Ipt ipt_name;
            public Ipt ipt_label;
            public Ipt ipt_interval;
            public Btn btn_new;
            public Btn btn_deleteTex;
            public GameObject go_item;
            public UiItemCtrl sub_Item;
            public Btn btn_image;
            public Img img_image;
        public UiModStoryMapObjectTextureAppearanceView(UiHolder uiHolder):base(uiHolder)
        {

            btn_delete = uiHolder.elementTrsLst[0].GetComponent<Btn>();
            scr_items = uiHolder.elementTrsLst[1].GetComponent<ScrView>();
            sta_show = uiHolder.elementTrsLst[2].GetComponent<Sta>();
            ipt_name = uiHolder.elementTrsLst[3].GetComponent<Ipt>();
            ipt_label = uiHolder.elementTrsLst[4].GetComponent<Ipt>();
            ipt_interval = uiHolder.elementTrsLst[5].GetComponent<Ipt>();
            btn_new = uiHolder.elementTrsLst[6].GetComponent<Btn>();
            btn_deleteTex = uiHolder.elementTrsLst[7].GetComponent<Btn>();
            go_item = uiHolder.elementTrsLst[8].gameObject;
            sub_Item = (UiItemCtrl) uiHolder.elementTrsLst[9].GetComponent<UiHolder>().ctrl;
            btn_image = uiHolder.elementTrsLst[10].GetComponent<Btn>();
            img_image = uiHolder.elementTrsLst[11].GetComponent<Img>();
        }

    }
    public partial class UiModStoryMapObjectTextureAppearanceCtrl:UiCtrl
    {
        public UiModStoryMapObjectTextureAppearanceView view;
        public UiModStoryMapObjectTextureAppearanceModel model;
        public UiModStoryMapObjectTextureAppearanceParam param;
        public UiModStoryMapObjectTextureCtrl parent=>(UiModStoryMapObjectTextureCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryMapObjectTextureAppearanceParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryMapObjectTextureAppearanceView(uiHolder);
            model=new UiModStoryMapObjectTextureAppearanceModel();


            view.sub_Item = new UiItemCtrl();
            view.sub_Item.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
        }

    }
    public partial class UiModStoryMapObjectTextureAppearanceModel:UiModel
    {
        
    }
}

namespace ModStoryMapObjectTextureConfig

{




    public partial class UiModStoryMapObjectTextureConfigParam:UiParam
    {
    }

    public partial class UiModStoryMapObjectTextureConfigView:UiView
    {

            public Btn btn_onTouchEvent;
            public Btn btn_onTouchEventTrigger;
            public Btn btn_onLeaveEvent;
            public Btn btn_onLeaveEventTrigger;
            public Btn btn_onShowEvent;
            public Btn btn_onShowEventTrigger;
            public Txt txt_onTouchEvent;
            public Txt txt_onTouchEventTrigger;
            public Txt txt_onLeaveEvent;
            public Txt txt_onLeaveEventTrigger;
            public Txt txt_onShowEvent;
            public Txt txt_onShowEventTrigger;
        public UiModStoryMapObjectTextureConfigView(UiHolder uiHolder):base(uiHolder)
        {

            btn_onTouchEvent = uiHolder.elementTrsLst[0].GetComponent<Btn>();
            btn_onTouchEventTrigger = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            btn_onLeaveEvent = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            btn_onLeaveEventTrigger = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            btn_onShowEvent = uiHolder.elementTrsLst[4].GetComponent<Btn>();
            btn_onShowEventTrigger = uiHolder.elementTrsLst[5].GetComponent<Btn>();
            txt_onTouchEvent = uiHolder.elementTrsLst[6].GetComponent<Txt>();
            txt_onTouchEventTrigger = uiHolder.elementTrsLst[7].GetComponent<Txt>();
            txt_onLeaveEvent = uiHolder.elementTrsLst[8].GetComponent<Txt>();
            txt_onLeaveEventTrigger = uiHolder.elementTrsLst[9].GetComponent<Txt>();
            txt_onShowEvent = uiHolder.elementTrsLst[10].GetComponent<Txt>();
            txt_onShowEventTrigger = uiHolder.elementTrsLst[11].GetComponent<Txt>();
        }

    }
    public partial class UiModStoryMapObjectTextureConfigCtrl:UiCtrl
    {
        public UiModStoryMapObjectTextureConfigView view;
        public UiModStoryMapObjectTextureConfigModel model;
        public UiModStoryMapObjectTextureConfigParam param;
        public UiModStoryMapObjectTextureCtrl parent=>(UiModStoryMapObjectTextureCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryMapObjectTextureConfigParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryMapObjectTextureConfigView(uiHolder);
            model=new UiModStoryMapObjectTextureConfigModel();


        }

    }
    public partial class UiModStoryMapObjectTextureConfigModel:UiModel
    {
        
    }
}

    public partial class UiModStoryMapObjectTextureParam:UiParam
    {
    }

    public partial class UiModStoryMapObjectTextureView:UiView
    {

            public ModStoryMapObjectTextureAppearance.UiModStoryMapObjectTextureAppearanceCtrl page_ModStoryMapObjectTextureAppearance;
            public ModStoryMapObjectTextureConfig.UiModStoryMapObjectTextureConfigCtrl page_ModStoryMapObjectTextureConfig;
            public Btn btn_back;
            public Btn btn_appearance;
            public Sta sta_appearance;
            public Btn btn_config;
            public Sta sta_config;
        public UiModStoryMapObjectTextureView(UiHolder uiHolder):base(uiHolder)
        {

            page_ModStoryMapObjectTextureAppearance = (ModStoryMapObjectTextureAppearance.UiModStoryMapObjectTextureAppearanceCtrl) uiHolder.elementTrsLst[0].GetComponent<UiHolder>().ctrl;
            page_ModStoryMapObjectTextureConfig = (ModStoryMapObjectTextureConfig.UiModStoryMapObjectTextureConfigCtrl) uiHolder.elementTrsLst[1].GetComponent<UiHolder>().ctrl;
            btn_back = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            btn_appearance = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            sta_appearance = uiHolder.elementTrsLst[4].GetComponent<Sta>();
            btn_config = uiHolder.elementTrsLst[5].GetComponent<Btn>();
            sta_config = uiHolder.elementTrsLst[6].GetComponent<Sta>();
        }

    }
    public partial class UiModStoryMapObjectTextureCtrl:UiCtrl
    {
        public UiModStoryMapObjectTextureView view;
        public UiModStoryMapObjectTextureModel model;
        public UiModStoryMapObjectTextureParam param;
        public UiModStoryMapObjectCtrl parent=>(UiModStoryMapObjectCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryMapObjectTextureParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryMapObjectTextureView(uiHolder);
            model=new UiModStoryMapObjectTextureModel();


            view.page_ModStoryMapObjectTextureAppearance = new ModStoryMapObjectTextureAppearance.UiModStoryMapObjectTextureAppearanceCtrl();
            view.page_ModStoryMapObjectTextureAppearance.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
            view.page_ModStoryMapObjectTextureConfig = new ModStoryMapObjectTextureConfig.UiModStoryMapObjectTextureConfigCtrl();
            view.page_ModStoryMapObjectTextureConfig.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
        }

    }
    public partial class UiModStoryMapObjectTextureModel:UiModel
    {
        
    }
}

namespace ModStoryMapObjectMask

{







    public partial class UiItemParam:UiParam
    {
    }

    public partial class UiItemView:UiView
    {

            public GameObject go_item;
            public Sta sta_exist;
            public Btn btn_new;
            public Btn btn_;
            public Sta sta_;
            public Img img_;
            public Txt txt_;
        public UiItemView(UiHolder uiHolder):base(uiHolder)
        {

            go_item = uiHolder.elementTrsLst[0].gameObject;
            sta_exist = uiHolder.elementTrsLst[1].GetComponent<Sta>();
            btn_new = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            btn_ = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            sta_ = uiHolder.elementTrsLst[4].GetComponent<Sta>();
            img_ = uiHolder.elementTrsLst[5].GetComponent<Img>();
            txt_ = uiHolder.elementTrsLst[6].GetComponent<Txt>();
        }

    }
    public partial class UiItemCtrl:UiCtrl
    {
        public UiItemView view;
        public UiItemModel model;
        public UiItemParam param;
        public UiModStoryMapObjectMaskCtrl parent=>(UiModStoryMapObjectMaskCtrl)uiHolder.parent.ctrl;

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
    public partial class UiModStoryMapObjectMaskParam:UiParam
    {
    }

    public partial class UiModStoryMapObjectMaskView:UiView
    {

            public Btn btn_back;
            public Btn btn_delete;
            public ScrView scr_items;
            public Sta sta_show;
            public Ipt ipt_name;
            public Ipt ipt_label;
            public GameObject go_mask0;
            public GameObject go_mask1;
            public GameObject go_mask2;
            public GameObject go_mask3;
            public GameObject go_mask4;
            public GameObject go_mask5;
            public GameObject go_item;
            public UiItemCtrl sub_Item;
            public Btn btn_image;
            public Img img_image;
        public UiModStoryMapObjectMaskView(UiHolder uiHolder):base(uiHolder)
        {

            btn_back = uiHolder.elementTrsLst[0].GetComponent<Btn>();
            btn_delete = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            scr_items = uiHolder.elementTrsLst[2].GetComponent<ScrView>();
            sta_show = uiHolder.elementTrsLst[3].GetComponent<Sta>();
            ipt_name = uiHolder.elementTrsLst[4].GetComponent<Ipt>();
            ipt_label = uiHolder.elementTrsLst[5].GetComponent<Ipt>();
            go_mask0 = uiHolder.elementTrsLst[6].gameObject;
            go_mask1 = uiHolder.elementTrsLst[7].gameObject;
            go_mask2 = uiHolder.elementTrsLst[8].gameObject;
            go_mask3 = uiHolder.elementTrsLst[9].gameObject;
            go_mask4 = uiHolder.elementTrsLst[10].gameObject;
            go_mask5 = uiHolder.elementTrsLst[11].gameObject;
            go_item = uiHolder.elementTrsLst[12].gameObject;
            sub_Item = (UiItemCtrl) uiHolder.elementTrsLst[13].GetComponent<UiHolder>().ctrl;
            btn_image = uiHolder.elementTrsLst[14].GetComponent<Btn>();
            img_image = uiHolder.elementTrsLst[15].GetComponent<Img>();
        }

    }
    public partial class UiModStoryMapObjectMaskCtrl:UiCtrl
    {
        public UiModStoryMapObjectMaskView view;
        public UiModStoryMapObjectMaskModel model;
        public UiModStoryMapObjectMaskParam param;
        public UiModStoryMapObjectCtrl parent=>(UiModStoryMapObjectCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryMapObjectMaskParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryMapObjectMaskView(uiHolder);
            model=new UiModStoryMapObjectMaskModel();


            view.sub_Item = new UiItemCtrl();
            view.sub_Item.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
        }

    }
    public partial class UiModStoryMapObjectMaskModel:UiModel
    {
        
    }
}

namespace ModStoryMapObjectObject

{




namespace ModStoryMapObjectObjectAppearance

{

using Ui.Axis;






    public partial class UiItemParam:UiParam
    {
    }

    public partial class UiItemView:UiView
    {

            public GameObject go_item;
            public Sta sta_exist;
            public Btn btn_new;
            public Btn btn_;
            public Sta sta_;
            public Img img_;
            public Txt txt_;
        public UiItemView(UiHolder uiHolder):base(uiHolder)
        {

            go_item = uiHolder.elementTrsLst[0].gameObject;
            sta_exist = uiHolder.elementTrsLst[1].GetComponent<Sta>();
            btn_new = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            btn_ = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            sta_ = uiHolder.elementTrsLst[4].GetComponent<Sta>();
            img_ = uiHolder.elementTrsLst[5].GetComponent<Img>();
            txt_ = uiHolder.elementTrsLst[6].GetComponent<Txt>();
        }

    }
    public partial class UiItemCtrl:UiCtrl
    {
        public UiItemView view;
        public UiItemModel model;
        public UiItemParam param;
        public UiModStoryMapObjectObjectAppearanceCtrl parent=>(UiModStoryMapObjectObjectAppearanceCtrl)uiHolder.parent.ctrl;

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
    public partial class UiModStoryMapObjectObjectAppearanceParam:UiParam
    {
    }

    public partial class UiModStoryMapObjectObjectAppearanceView:UiView
    {

            public Btn btn_delete;
            public Sta sta_show;
            public ScrView scr_items;
            public Ipt ipt_name;
            public Btn btn_deleteUnit;
            public Ipt ipt_width;
            public Ipt ipt_length;
            public Ipt ipt_height;
            public Btn btn_reset;
            public Btn btn_model;
            public Btn btn_image;
            public RImg rimg_image;
            public RectTransform rtf_image;
            public RectTransform rtf_axis;
            public UiAxisCtrl model_Axis;
            public GameObject go_item;
            public UiItemCtrl sub_Item;
            public Txt txt_model;
        public UiModStoryMapObjectObjectAppearanceView(UiHolder uiHolder):base(uiHolder)
        {

            btn_delete = uiHolder.elementTrsLst[0].GetComponent<Btn>();
            sta_show = uiHolder.elementTrsLst[1].GetComponent<Sta>();
            scr_items = uiHolder.elementTrsLst[2].GetComponent<ScrView>();
            ipt_name = uiHolder.elementTrsLst[3].GetComponent<Ipt>();
            btn_deleteUnit = uiHolder.elementTrsLst[4].GetComponent<Btn>();
            ipt_width = uiHolder.elementTrsLst[5].GetComponent<Ipt>();
            ipt_length = uiHolder.elementTrsLst[6].GetComponent<Ipt>();
            ipt_height = uiHolder.elementTrsLst[7].GetComponent<Ipt>();
            btn_reset = uiHolder.elementTrsLst[8].GetComponent<Btn>();
            btn_model = uiHolder.elementTrsLst[9].GetComponent<Btn>();
            btn_image = uiHolder.elementTrsLst[10].GetComponent<Btn>();
            rimg_image = uiHolder.elementTrsLst[11].GetComponent<RImg>();
            rtf_image = uiHolder.elementTrsLst[12].GetComponent<RectTransform>();
            rtf_axis = uiHolder.elementTrsLst[13].GetComponent<RectTransform>();
            model_Axis = (UiAxisCtrl) uiHolder.elementTrsLst[14].GetComponent<UiHolder>().ctrl;
            go_item = uiHolder.elementTrsLst[15].gameObject;
            sub_Item = (UiItemCtrl) uiHolder.elementTrsLst[16].GetComponent<UiHolder>().ctrl;
            txt_model = uiHolder.elementTrsLst[17].GetComponent<Txt>();
        }

    }
    public partial class UiModStoryMapObjectObjectAppearanceCtrl:UiCtrl
    {
        public UiModStoryMapObjectObjectAppearanceView view;
        public UiModStoryMapObjectObjectAppearanceModel model;
        public UiModStoryMapObjectObjectAppearanceParam param;
        public UiModStoryMapObjectObjectCtrl parent=>(UiModStoryMapObjectObjectCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryMapObjectObjectAppearanceParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryMapObjectObjectAppearanceView(uiHolder);
            model=new UiModStoryMapObjectObjectAppearanceModel();


            view.model_Axis = new UiAxisCtrl();
            view.model_Axis.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
            view.sub_Item = new UiItemCtrl();
            view.sub_Item.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
        }

    }
    public partial class UiModStoryMapObjectObjectAppearanceModel:UiModel
    {
        
    }
}

namespace ModStoryMapObjectObjectConfig

{




    public partial class UiModStoryMapObjectObjectConfigParam:UiParam
    {
    }

    public partial class UiModStoryMapObjectObjectConfigView:UiView
    {

            public Btn btn_fixed;
            public Sta sta_fixed;
            public Btn btn_onTouchEvent;
            public Btn btn_onTouchEventTrigger;
            public Btn btn_onLeaveEvent;
            public Btn btn_onLeaveEventTrigger;
            public Btn btn_onShowEvent;
            public Btn btn_onShowEventTrigger;
            public Txt txt_onTouchEvent;
            public Txt txt_onTouchEventTrigger;
            public Txt txt_onLeaveEvent;
            public Txt txt_onLeaveEventTrigger;
            public Txt txt_onShowEvent;
            public Txt txt_onShowEventTrigger;
        public UiModStoryMapObjectObjectConfigView(UiHolder uiHolder):base(uiHolder)
        {

            btn_fixed = uiHolder.elementTrsLst[0].GetComponent<Btn>();
            sta_fixed = uiHolder.elementTrsLst[1].GetComponent<Sta>();
            btn_onTouchEvent = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            btn_onTouchEventTrigger = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            btn_onLeaveEvent = uiHolder.elementTrsLst[4].GetComponent<Btn>();
            btn_onLeaveEventTrigger = uiHolder.elementTrsLst[5].GetComponent<Btn>();
            btn_onShowEvent = uiHolder.elementTrsLst[6].GetComponent<Btn>();
            btn_onShowEventTrigger = uiHolder.elementTrsLst[7].GetComponent<Btn>();
            txt_onTouchEvent = uiHolder.elementTrsLst[8].GetComponent<Txt>();
            txt_onTouchEventTrigger = uiHolder.elementTrsLst[9].GetComponent<Txt>();
            txt_onLeaveEvent = uiHolder.elementTrsLst[10].GetComponent<Txt>();
            txt_onLeaveEventTrigger = uiHolder.elementTrsLst[11].GetComponent<Txt>();
            txt_onShowEvent = uiHolder.elementTrsLst[12].GetComponent<Txt>();
            txt_onShowEventTrigger = uiHolder.elementTrsLst[13].GetComponent<Txt>();
        }

    }
    public partial class UiModStoryMapObjectObjectConfigCtrl:UiCtrl
    {
        public UiModStoryMapObjectObjectConfigView view;
        public UiModStoryMapObjectObjectConfigModel model;
        public UiModStoryMapObjectObjectConfigParam param;
        public UiModStoryMapObjectObjectCtrl parent=>(UiModStoryMapObjectObjectCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryMapObjectObjectConfigParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryMapObjectObjectConfigView(uiHolder);
            model=new UiModStoryMapObjectObjectConfigModel();


        }

    }
    public partial class UiModStoryMapObjectObjectConfigModel:UiModel
    {
        
    }
}

    public partial class UiModStoryMapObjectObjectParam:UiParam
    {
    }

    public partial class UiModStoryMapObjectObjectView:UiView
    {

            public ModStoryMapObjectObjectAppearance.UiModStoryMapObjectObjectAppearanceCtrl page_ModStoryMapObjectObjectAppearance;
            public ModStoryMapObjectObjectConfig.UiModStoryMapObjectObjectConfigCtrl page_ModStoryMapObjectObjectConfig;
            public Btn btn_back;
            public Btn btn_appearance;
            public Sta sta_appearance;
            public Btn btn_config;
            public Sta sta_config;
        public UiModStoryMapObjectObjectView(UiHolder uiHolder):base(uiHolder)
        {

            page_ModStoryMapObjectObjectAppearance = (ModStoryMapObjectObjectAppearance.UiModStoryMapObjectObjectAppearanceCtrl) uiHolder.elementTrsLst[0].GetComponent<UiHolder>().ctrl;
            page_ModStoryMapObjectObjectConfig = (ModStoryMapObjectObjectConfig.UiModStoryMapObjectObjectConfigCtrl) uiHolder.elementTrsLst[1].GetComponent<UiHolder>().ctrl;
            btn_back = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            btn_appearance = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            sta_appearance = uiHolder.elementTrsLst[4].GetComponent<Sta>();
            btn_config = uiHolder.elementTrsLst[5].GetComponent<Btn>();
            sta_config = uiHolder.elementTrsLst[6].GetComponent<Sta>();
        }

    }
    public partial class UiModStoryMapObjectObjectCtrl:UiCtrl
    {
        public UiModStoryMapObjectObjectView view;
        public UiModStoryMapObjectObjectModel model;
        public UiModStoryMapObjectObjectParam param;
        public UiModStoryMapObjectCtrl parent=>(UiModStoryMapObjectCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryMapObjectObjectParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryMapObjectObjectView(uiHolder);
            model=new UiModStoryMapObjectObjectModel();


            view.page_ModStoryMapObjectObjectAppearance = new ModStoryMapObjectObjectAppearance.UiModStoryMapObjectObjectAppearanceCtrl();
            view.page_ModStoryMapObjectObjectAppearance.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
            view.page_ModStoryMapObjectObjectConfig = new ModStoryMapObjectObjectConfig.UiModStoryMapObjectObjectConfigCtrl();
            view.page_ModStoryMapObjectObjectConfig.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
        }

    }
    public partial class UiModStoryMapObjectObjectModel:UiModel
    {
        
    }
}

    public partial class UiModStoryMapObjectParam:UiParam
    {
    }

    public partial class UiModStoryMapObjectView:UiView
    {

            public ModStoryMapObjectType.UiModStoryMapObjectTypeCtrl page_ModStoryMapObjectType;
            public ModStoryMapObjectList.UiModStoryMapObjectListCtrl page_ModStoryMapObjectList;
            public ModStoryMapObjectTexture.UiModStoryMapObjectTextureCtrl page_ModStoryMapObjectTexture;
            public ModStoryMapObjectMask.UiModStoryMapObjectMaskCtrl page_ModStoryMapObjectMask;
            public ModStoryMapObjectObject.UiModStoryMapObjectObjectCtrl page_ModStoryMapObjectObject;
        public UiModStoryMapObjectView(UiHolder uiHolder):base(uiHolder)
        {

            page_ModStoryMapObjectType = (ModStoryMapObjectType.UiModStoryMapObjectTypeCtrl) uiHolder.elementTrsLst[0].GetComponent<UiHolder>().ctrl;
            page_ModStoryMapObjectList = (ModStoryMapObjectList.UiModStoryMapObjectListCtrl) uiHolder.elementTrsLst[1].GetComponent<UiHolder>().ctrl;
            page_ModStoryMapObjectTexture = (ModStoryMapObjectTexture.UiModStoryMapObjectTextureCtrl) uiHolder.elementTrsLst[2].GetComponent<UiHolder>().ctrl;
            page_ModStoryMapObjectMask = (ModStoryMapObjectMask.UiModStoryMapObjectMaskCtrl) uiHolder.elementTrsLst[3].GetComponent<UiHolder>().ctrl;
            page_ModStoryMapObjectObject = (ModStoryMapObjectObject.UiModStoryMapObjectObjectCtrl) uiHolder.elementTrsLst[4].GetComponent<UiHolder>().ctrl;
        }

    }
    public partial class UiModStoryMapObjectCtrl:UiCtrl
    {
        public UiModStoryMapObjectView view;
        public UiModStoryMapObjectModel model;
        public UiModStoryMapObjectParam param;
        public UiModStoryCtrl parent=>(UiModStoryCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryMapObjectParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryMapObjectView(uiHolder);
            model=new UiModStoryMapObjectModel();


            view.page_ModStoryMapObjectType = new ModStoryMapObjectType.UiModStoryMapObjectTypeCtrl();
            view.page_ModStoryMapObjectType.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
            view.page_ModStoryMapObjectList = new ModStoryMapObjectList.UiModStoryMapObjectListCtrl();
            view.page_ModStoryMapObjectList.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
            view.page_ModStoryMapObjectTexture = new ModStoryMapObjectTexture.UiModStoryMapObjectTextureCtrl();
            view.page_ModStoryMapObjectTexture.BindHolderRecursively(uiHolder.subUiHolderLst[2]);
            view.page_ModStoryMapObjectMask = new ModStoryMapObjectMask.UiModStoryMapObjectMaskCtrl();
            view.page_ModStoryMapObjectMask.BindHolderRecursively(uiHolder.subUiHolderLst[3]);
            view.page_ModStoryMapObjectObject = new ModStoryMapObjectObject.UiModStoryMapObjectObjectCtrl();
            view.page_ModStoryMapObjectObject.BindHolderRecursively(uiHolder.subUiHolderLst[4]);
        }

    }
    public partial class UiModStoryMapObjectModel:UiModel
    {
        
    }
}

namespace ModStoryEffect

{




namespace ModStoryEffectList

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
        public UiModStoryEffectListCtrl parent=>(UiModStoryEffectListCtrl)uiHolder.parent.ctrl;

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



    public partial class UiBigItemParam:UiParam
    {
    }

    public partial class UiBigItemView:UiView
    {

            public GameObject go_bigItem;
            public Sta sta_bigItem;
            public Sta sta_exist;
            public Btn btn_new;
            public Btn btn_;
            public Sta sta_;
            public Txt txt_;
            public Img img_;
        public UiBigItemView(UiHolder uiHolder):base(uiHolder)
        {

            go_bigItem = uiHolder.elementTrsLst[0].gameObject;
            sta_bigItem = uiHolder.elementTrsLst[1].GetComponent<Sta>();
            sta_exist = uiHolder.elementTrsLst[2].GetComponent<Sta>();
            btn_new = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            btn_ = uiHolder.elementTrsLst[4].GetComponent<Btn>();
            sta_ = uiHolder.elementTrsLst[5].GetComponent<Sta>();
            txt_ = uiHolder.elementTrsLst[6].GetComponent<Txt>();
            img_ = uiHolder.elementTrsLst[7].GetComponent<Img>();
        }

    }
    public partial class UiBigItemCtrl:UiCtrl
    {
        public UiBigItemView view;
        public UiBigItemModel model;
        public UiBigItemParam param;
        public UiModStoryEffectListCtrl parent=>(UiModStoryEffectListCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiBigItemParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiBigItemView(uiHolder);
            model=new UiBigItemModel();


        }

    }
    public partial class UiBigItemModel:UiModel
    {
        
    }
    public partial class UiModStoryEffectListParam:UiParam
    {
    }

    public partial class UiModStoryEffectListView:UiView
    {

            public ScrView scr_labs;
            public ScrView scr_bigItems;
            public GameObject go_lab;
            public UiLabCtrl sub_Lab;
            public GameObject go_bigItem;
            public Sta sta_bigItem;
            public UiBigItemCtrl sub_BigItem;
        public UiModStoryEffectListView(UiHolder uiHolder):base(uiHolder)
        {

            scr_labs = uiHolder.elementTrsLst[0].GetComponent<ScrView>();
            scr_bigItems = uiHolder.elementTrsLst[1].GetComponent<ScrView>();
            go_lab = uiHolder.elementTrsLst[2].gameObject;
            sub_Lab = (UiLabCtrl) uiHolder.elementTrsLst[3].GetComponent<UiHolder>().ctrl;
            go_bigItem = uiHolder.elementTrsLst[4].gameObject;
            sta_bigItem = uiHolder.elementTrsLst[5].GetComponent<Sta>();
            sub_BigItem = (UiBigItemCtrl) uiHolder.elementTrsLst[6].GetComponent<UiHolder>().ctrl;
        }

    }
    public partial class UiModStoryEffectListCtrl:UiCtrl
    {
        public UiModStoryEffectListView view;
        public UiModStoryEffectListModel model;
        public UiModStoryEffectListParam param;
        public UiModStoryEffectCtrl parent=>(UiModStoryEffectCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryEffectListParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryEffectListView(uiHolder);
            model=new UiModStoryEffectListModel();


            view.sub_Lab = new UiLabCtrl();
            view.sub_Lab.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
            view.sub_BigItem = new UiBigItemCtrl();
            view.sub_BigItem.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
        }

    }
    public partial class UiModStoryEffectListModel:UiModel
    {
        
    }
}

namespace ModStoryEffectUnit

{







    public partial class UiEffectParam:UiParam
    {
    }

    public partial class UiEffectView:UiView
    {

            public GameObject go_effect;
            public Sta sta_exist;
            public Btn btn_new;
            public Btn btn_delete;
            public Ipt ipt_rotate;
            public Ipt ipt_opacity;
            public Ipt ipt_sustain;
            public Btn btn_transparence;
            public Sta sta_transparence;
            public Ipt ipt_posSetX;
            public Ipt ipt_posSetZ;
            public Ipt ipt_posSetY;
            public Ipt ipt_scaleSetX;
            public Ipt ipt_scaleSetZ;
            public Ipt ipt_scaleSetY;
        public UiEffectView(UiHolder uiHolder):base(uiHolder)
        {

            go_effect = uiHolder.elementTrsLst[0].gameObject;
            sta_exist = uiHolder.elementTrsLst[1].GetComponent<Sta>();
            btn_new = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            btn_delete = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            ipt_rotate = uiHolder.elementTrsLst[4].GetComponent<Ipt>();
            ipt_opacity = uiHolder.elementTrsLst[5].GetComponent<Ipt>();
            ipt_sustain = uiHolder.elementTrsLst[6].GetComponent<Ipt>();
            btn_transparence = uiHolder.elementTrsLst[7].GetComponent<Btn>();
            sta_transparence = uiHolder.elementTrsLst[8].GetComponent<Sta>();
            ipt_posSetX = uiHolder.elementTrsLst[9].GetComponent<Ipt>();
            ipt_posSetZ = uiHolder.elementTrsLst[10].GetComponent<Ipt>();
            ipt_posSetY = uiHolder.elementTrsLst[11].GetComponent<Ipt>();
            ipt_scaleSetX = uiHolder.elementTrsLst[12].GetComponent<Ipt>();
            ipt_scaleSetZ = uiHolder.elementTrsLst[13].GetComponent<Ipt>();
            ipt_scaleSetY = uiHolder.elementTrsLst[14].GetComponent<Ipt>();
        }

    }
    public partial class UiEffectCtrl:UiCtrl
    {
        public UiEffectView view;
        public UiEffectModel model;
        public UiEffectParam param;
        public UiModStoryEffectUnitCtrl parent=>(UiModStoryEffectUnitCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiEffectParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiEffectView(uiHolder);
            model=new UiEffectModel();


        }

    }
    public partial class UiEffectModel:UiModel
    {
        
    }
    public partial class UiModStoryEffectUnitParam:UiParam
    {
    }

    public partial class UiModStoryEffectUnitView:UiView
    {

            public Btn btn_back;
            public Btn btn_delete;
            public ScrView scr_effects;
            public Ipt ipt_name;
            public Ipt ipt_label;
            public Btn btn_image;
            public Img img_image;
            public GameObject go_effect;
            public UiEffectCtrl sub_Effect;
        public UiModStoryEffectUnitView(UiHolder uiHolder):base(uiHolder)
        {

            btn_back = uiHolder.elementTrsLst[0].GetComponent<Btn>();
            btn_delete = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            scr_effects = uiHolder.elementTrsLst[2].GetComponent<ScrView>();
            ipt_name = uiHolder.elementTrsLst[3].GetComponent<Ipt>();
            ipt_label = uiHolder.elementTrsLst[4].GetComponent<Ipt>();
            btn_image = uiHolder.elementTrsLst[5].GetComponent<Btn>();
            img_image = uiHolder.elementTrsLst[6].GetComponent<Img>();
            go_effect = uiHolder.elementTrsLst[7].gameObject;
            sub_Effect = (UiEffectCtrl) uiHolder.elementTrsLst[8].GetComponent<UiHolder>().ctrl;
        }

    }
    public partial class UiModStoryEffectUnitCtrl:UiCtrl
    {
        public UiModStoryEffectUnitView view;
        public UiModStoryEffectUnitModel model;
        public UiModStoryEffectUnitParam param;
        public UiModStoryEffectCtrl parent=>(UiModStoryEffectCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryEffectUnitParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryEffectUnitView(uiHolder);
            model=new UiModStoryEffectUnitModel();


            view.sub_Effect = new UiEffectCtrl();
            view.sub_Effect.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
        }

    }
    public partial class UiModStoryEffectUnitModel:UiModel
    {
        
    }
}

    public partial class UiModStoryEffectParam:UiParam
    {
    }

    public partial class UiModStoryEffectView:UiView
    {

            public ModStoryEffectList.UiModStoryEffectListCtrl page_ModStoryEffectList;
            public ModStoryEffectUnit.UiModStoryEffectUnitCtrl page_ModStoryEffectUnit;
        public UiModStoryEffectView(UiHolder uiHolder):base(uiHolder)
        {

            page_ModStoryEffectList = (ModStoryEffectList.UiModStoryEffectListCtrl) uiHolder.elementTrsLst[0].GetComponent<UiHolder>().ctrl;
            page_ModStoryEffectUnit = (ModStoryEffectUnit.UiModStoryEffectUnitCtrl) uiHolder.elementTrsLst[1].GetComponent<UiHolder>().ctrl;
        }

    }
    public partial class UiModStoryEffectCtrl:UiCtrl
    {
        public UiModStoryEffectView view;
        public UiModStoryEffectModel model;
        public UiModStoryEffectParam param;
        public UiModStoryCtrl parent=>(UiModStoryCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryEffectParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryEffectView(uiHolder);
            model=new UiModStoryEffectModel();


            view.page_ModStoryEffectList = new ModStoryEffectList.UiModStoryEffectListCtrl();
            view.page_ModStoryEffectList.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
            view.page_ModStoryEffectUnit = new ModStoryEffectUnit.UiModStoryEffectUnitCtrl();
            view.page_ModStoryEffectUnit.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
        }

    }
    public partial class UiModStoryEffectModel:UiModel
    {
        
    }
}

namespace ModStoryEvent

{




namespace ModStoryEventCustom

{







    public partial class UiCategoryParam:UiParam
    {
    }

    public partial class UiCategoryView:UiView
    {

            public GameObject go_category;
            public Btn btn_;
            public Sta sta_;
            public Sta sta_valid;
            public Txt txt_;
        public UiCategoryView(UiHolder uiHolder):base(uiHolder)
        {

            go_category = uiHolder.elementTrsLst[0].gameObject;
            btn_ = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            sta_ = uiHolder.elementTrsLst[2].GetComponent<Sta>();
            sta_valid = uiHolder.elementTrsLst[3].GetComponent<Sta>();
            txt_ = uiHolder.elementTrsLst[4].GetComponent<Txt>();
        }

    }
    public partial class UiCategoryCtrl:UiCtrl
    {
        public UiCategoryView view;
        public UiCategoryModel model;
        public UiCategoryParam param;
        public UiModStoryEventCustomCtrl parent=>(UiModStoryEventCustomCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiCategoryParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiCategoryView(uiHolder);
            model=new UiCategoryModel();


        }

    }
    public partial class UiCategoryModel:UiModel
    {
        
    }



    public partial class UiTypeParam:UiParam
    {
    }

    public partial class UiTypeView:UiView
    {

            public GameObject go_type;
            public Btn btn_;
            public Sta sta_;
            public Sta sta_valid;
            public Txt txt_;
        public UiTypeView(UiHolder uiHolder):base(uiHolder)
        {

            go_type = uiHolder.elementTrsLst[0].gameObject;
            btn_ = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            sta_ = uiHolder.elementTrsLst[2].GetComponent<Sta>();
            sta_valid = uiHolder.elementTrsLst[3].GetComponent<Sta>();
            txt_ = uiHolder.elementTrsLst[4].GetComponent<Txt>();
        }

    }
    public partial class UiTypeCtrl:UiCtrl
    {
        public UiTypeView view;
        public UiTypeModel model;
        public UiTypeParam param;
        public UiModStoryEventCustomCtrl parent=>(UiModStoryEventCustomCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiTypeParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiTypeView(uiHolder);
            model=new UiTypeModel();


        }

    }
    public partial class UiTypeModel:UiModel
    {
        
    }



    public partial class UiItemParam:UiParam
    {
    }

    public partial class UiItemView:UiView
    {

            public GameObject go_item;
            public Btn btn_;
            public Sta sta_;
            public Sta sta_valid;
            public Txt txt_;
        public UiItemView(UiHolder uiHolder):base(uiHolder)
        {

            go_item = uiHolder.elementTrsLst[0].gameObject;
            btn_ = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            sta_ = uiHolder.elementTrsLst[2].GetComponent<Sta>();
            sta_valid = uiHolder.elementTrsLst[3].GetComponent<Sta>();
            txt_ = uiHolder.elementTrsLst[4].GetComponent<Txt>();
        }

    }
    public partial class UiItemCtrl:UiCtrl
    {
        public UiItemView view;
        public UiItemModel model;
        public UiItemParam param;
        public UiModStoryEventCustomCtrl parent=>(UiModStoryEventCustomCtrl)uiHolder.parent.ctrl;

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
    public partial class UiModStoryEventCustomParam:UiParam
    {
    }

    public partial class UiModStoryEventCustomView:UiView
    {

            public ScrView scr_categorys;
            public GameObject go_show;
            public ScrView scr_types;
            public ScrView scr_items;
            public Txt txt_name;
            public Txt txt_desc;
            public Btn btn_edit;
            public Sta sta_edit;
            public Btn btn_delete;
            public Sta sta_delete;
            public GameObject go_category;
            public UiCategoryCtrl sub_Category;
            public GameObject go_type;
            public UiTypeCtrl sub_Type;
            public GameObject go_item;
            public UiItemCtrl sub_Item;
        public UiModStoryEventCustomView(UiHolder uiHolder):base(uiHolder)
        {

            scr_categorys = uiHolder.elementTrsLst[0].GetComponent<ScrView>();
            go_show = uiHolder.elementTrsLst[1].gameObject;
            scr_types = uiHolder.elementTrsLst[2].GetComponent<ScrView>();
            scr_items = uiHolder.elementTrsLst[3].GetComponent<ScrView>();
            txt_name = uiHolder.elementTrsLst[4].GetComponent<Txt>();
            txt_desc = uiHolder.elementTrsLst[5].GetComponent<Txt>();
            btn_edit = uiHolder.elementTrsLst[6].GetComponent<Btn>();
            sta_edit = uiHolder.elementTrsLst[7].GetComponent<Sta>();
            btn_delete = uiHolder.elementTrsLst[8].GetComponent<Btn>();
            sta_delete = uiHolder.elementTrsLst[9].GetComponent<Sta>();
            go_category = uiHolder.elementTrsLst[10].gameObject;
            sub_Category = (UiCategoryCtrl) uiHolder.elementTrsLst[11].GetComponent<UiHolder>().ctrl;
            go_type = uiHolder.elementTrsLst[12].gameObject;
            sub_Type = (UiTypeCtrl) uiHolder.elementTrsLst[13].GetComponent<UiHolder>().ctrl;
            go_item = uiHolder.elementTrsLst[14].gameObject;
            sub_Item = (UiItemCtrl) uiHolder.elementTrsLst[15].GetComponent<UiHolder>().ctrl;
        }

    }
    public partial class UiModStoryEventCustomCtrl:UiCtrl
    {
        public UiModStoryEventCustomView view;
        public UiModStoryEventCustomModel model;
        public UiModStoryEventCustomParam param;
        public UiModStoryEventCtrl parent=>(UiModStoryEventCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryEventCustomParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryEventCustomView(uiHolder);
            model=new UiModStoryEventCustomModel();


            view.sub_Category = new UiCategoryCtrl();
            view.sub_Category.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
            view.sub_Type = new UiTypeCtrl();
            view.sub_Type.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
            view.sub_Item = new UiItemCtrl();
            view.sub_Item.BindHolderRecursively(uiHolder.subUiHolderLst[2]);
        }

    }
    public partial class UiModStoryEventCustomModel:UiModel
    {
        
    }
}

namespace ModStoryEventConfig

{




    public partial class UiModStoryEventConfigParam:UiParam
    {
    }

    public partial class UiModStoryEventConfigView:UiView
    {

            public Btn btn_onBeginEvent;
            public Btn btn_onEndEvent;
            public Txt txt_onBeginEvent;
            public Txt txt_onEndEvent;
        public UiModStoryEventConfigView(UiHolder uiHolder):base(uiHolder)
        {

            btn_onBeginEvent = uiHolder.elementTrsLst[0].GetComponent<Btn>();
            btn_onEndEvent = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            txt_onBeginEvent = uiHolder.elementTrsLst[2].GetComponent<Txt>();
            txt_onEndEvent = uiHolder.elementTrsLst[3].GetComponent<Txt>();
        }

    }
    public partial class UiModStoryEventConfigCtrl:UiCtrl
    {
        public UiModStoryEventConfigView view;
        public UiModStoryEventConfigModel model;
        public UiModStoryEventConfigParam param;
        public UiModStoryEventCtrl parent=>(UiModStoryEventCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryEventConfigParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryEventConfigView(uiHolder);
            model=new UiModStoryEventConfigModel();


        }

    }
    public partial class UiModStoryEventConfigModel:UiModel
    {
        
    }
}

    public partial class UiModStoryEventParam:UiParam
    {
    }

    public partial class UiModStoryEventView:UiView
    {

            public ModStoryEventCustom.UiModStoryEventCustomCtrl page_ModStoryEventCustom;
            public ModStoryEventConfig.UiModStoryEventConfigCtrl page_ModStoryEventConfig;
            public Btn btn_customEvent;
            public Sta sta_customEvent;
            public Btn btn_globalEvent;
            public Sta sta_globalEvent;
        public UiModStoryEventView(UiHolder uiHolder):base(uiHolder)
        {

            page_ModStoryEventCustom = (ModStoryEventCustom.UiModStoryEventCustomCtrl) uiHolder.elementTrsLst[0].GetComponent<UiHolder>().ctrl;
            page_ModStoryEventConfig = (ModStoryEventConfig.UiModStoryEventConfigCtrl) uiHolder.elementTrsLst[1].GetComponent<UiHolder>().ctrl;
            btn_customEvent = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            sta_customEvent = uiHolder.elementTrsLst[3].GetComponent<Sta>();
            btn_globalEvent = uiHolder.elementTrsLst[4].GetComponent<Btn>();
            sta_globalEvent = uiHolder.elementTrsLst[5].GetComponent<Sta>();
        }

    }
    public partial class UiModStoryEventCtrl:UiCtrl
    {
        public UiModStoryEventView view;
        public UiModStoryEventModel model;
        public UiModStoryEventParam param;
        public UiModStoryCtrl parent=>(UiModStoryCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryEventParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryEventView(uiHolder);
            model=new UiModStoryEventModel();


            view.page_ModStoryEventCustom = new ModStoryEventCustom.UiModStoryEventCustomCtrl();
            view.page_ModStoryEventCustom.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
            view.page_ModStoryEventConfig = new ModStoryEventConfig.UiModStoryEventConfigCtrl();
            view.page_ModStoryEventConfig.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
        }

    }
    public partial class UiModStoryEventModel:UiModel
    {
        
    }
}

namespace ModStoryMap

{




namespace ModStoryMapMap

{

using Ui.Axis;






    public partial class UiMapSceneParam:UiParam
    {
    }

    public partial class UiMapSceneView:UiView
    {

            public GameObject go_mapScene;
            public Btn btn_import;
            public Img img_;
            public Txt txt_;
        public UiMapSceneView(UiHolder uiHolder):base(uiHolder)
        {

            go_mapScene = uiHolder.elementTrsLst[0].gameObject;
            btn_import = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            img_ = uiHolder.elementTrsLst[2].GetComponent<Img>();
            txt_ = uiHolder.elementTrsLst[3].GetComponent<Txt>();
        }

    }
    public partial class UiMapSceneCtrl:UiCtrl
    {
        public UiMapSceneView view;
        public UiMapSceneModel model;
        public UiMapSceneParam param;
        public UiModStoryMapMapCtrl parent=>(UiModStoryMapMapCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiMapSceneParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiMapSceneView(uiHolder);
            model=new UiMapSceneModel();


        }

    }
    public partial class UiMapSceneModel:UiModel
    {
        
    }
    public partial class UiModStoryMapMapParam:UiParam
    {
    }

    public partial class UiModStoryMapMapView:UiView
    {

            public Img img_map;
            public RectTransform rtf_axis;
            public UiAxisCtrl model_Axis;
            public GameObject go_mapScene;
            public UiMapSceneCtrl sub_MapScene;
            public Btn btn_import;
        public UiModStoryMapMapView(UiHolder uiHolder):base(uiHolder)
        {

            img_map = uiHolder.elementTrsLst[0].GetComponent<Img>();
            rtf_axis = uiHolder.elementTrsLst[1].GetComponent<RectTransform>();
            model_Axis = (UiAxisCtrl) uiHolder.elementTrsLst[2].GetComponent<UiHolder>().ctrl;
            go_mapScene = uiHolder.elementTrsLst[3].gameObject;
            sub_MapScene = (UiMapSceneCtrl) uiHolder.elementTrsLst[4].GetComponent<UiHolder>().ctrl;
            btn_import = uiHolder.elementTrsLst[5].GetComponent<Btn>();
        }

    }
    public partial class UiModStoryMapMapCtrl:UiCtrl
    {
        public UiModStoryMapMapView view;
        public UiModStoryMapMapModel model;
        public UiModStoryMapMapParam param;
        public UiModStoryMapCtrl parent=>(UiModStoryMapCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryMapMapParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryMapMapView(uiHolder);
            model=new UiModStoryMapMapModel();


            view.model_Axis = new UiAxisCtrl();
            view.model_Axis.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
            view.sub_MapScene = new UiMapSceneCtrl();
            view.sub_MapScene.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
        }

    }
    public partial class UiModStoryMapMapModel:UiModel
    {
        
    }
}

namespace ModStoryMapScene

{




namespace ModStoryMapSceneList

{







    public partial class UiBigItemParam:UiParam
    {
    }

    public partial class UiBigItemView:UiView
    {

            public GameObject go_bigItem;
            public Sta sta_bigItem;
            public Sta sta_exist;
            public Btn btn_new;
            public Btn btn_;
            public Sta sta_;
            public Txt txt_;
            public Img img_;
        public UiBigItemView(UiHolder uiHolder):base(uiHolder)
        {

            go_bigItem = uiHolder.elementTrsLst[0].gameObject;
            sta_bigItem = uiHolder.elementTrsLst[1].GetComponent<Sta>();
            sta_exist = uiHolder.elementTrsLst[2].GetComponent<Sta>();
            btn_new = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            btn_ = uiHolder.elementTrsLst[4].GetComponent<Btn>();
            sta_ = uiHolder.elementTrsLst[5].GetComponent<Sta>();
            txt_ = uiHolder.elementTrsLst[6].GetComponent<Txt>();
            img_ = uiHolder.elementTrsLst[7].GetComponent<Img>();
        }

    }
    public partial class UiBigItemCtrl:UiCtrl
    {
        public UiBigItemView view;
        public UiBigItemModel model;
        public UiBigItemParam param;
        public UiModStoryMapSceneListCtrl parent=>(UiModStoryMapSceneListCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiBigItemParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiBigItemView(uiHolder);
            model=new UiBigItemModel();


        }

    }
    public partial class UiBigItemModel:UiModel
    {
        
    }
    public partial class UiModStoryMapSceneListParam:UiParam
    {
    }

    public partial class UiModStoryMapSceneListView:UiView
    {

            public ScrView scr_bigItems;
            public GameObject go_bigItem;
            public Sta sta_bigItem;
            public UiBigItemCtrl sub_BigItem;
        public UiModStoryMapSceneListView(UiHolder uiHolder):base(uiHolder)
        {

            scr_bigItems = uiHolder.elementTrsLst[0].GetComponent<ScrView>();
            go_bigItem = uiHolder.elementTrsLst[1].gameObject;
            sta_bigItem = uiHolder.elementTrsLst[2].GetComponent<Sta>();
            sub_BigItem = (UiBigItemCtrl) uiHolder.elementTrsLst[3].GetComponent<UiHolder>().ctrl;
        }

    }
    public partial class UiModStoryMapSceneListCtrl:UiCtrl
    {
        public UiModStoryMapSceneListView view;
        public UiModStoryMapSceneListModel model;
        public UiModStoryMapSceneListParam param;
        public UiModStoryMapSceneCtrl parent=>(UiModStoryMapSceneCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryMapSceneListParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryMapSceneListView(uiHolder);
            model=new UiModStoryMapSceneListModel();


            view.sub_BigItem = new UiBigItemCtrl();
            view.sub_BigItem.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
        }

    }
    public partial class UiModStoryMapSceneListModel:UiModel
    {
        
    }
}

namespace ModStoryMapSceneUnit

{




    public partial class UiModStoryMapSceneUnitParam:UiParam
    {
    }

    public partial class UiModStoryMapSceneUnitView:UiView
    {

            public Btn btn_delete;
            public Ipt ipt_name;
            public Btn btn_map;
            public Img img_map;
            public Btn btn_edit;
        public UiModStoryMapSceneUnitView(UiHolder uiHolder):base(uiHolder)
        {

            btn_delete = uiHolder.elementTrsLst[0].GetComponent<Btn>();
            ipt_name = uiHolder.elementTrsLst[1].GetComponent<Ipt>();
            btn_map = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            img_map = uiHolder.elementTrsLst[3].GetComponent<Img>();
            btn_edit = uiHolder.elementTrsLst[4].GetComponent<Btn>();
        }

    }
    public partial class UiModStoryMapSceneUnitCtrl:UiCtrl
    {
        public UiModStoryMapSceneUnitView view;
        public UiModStoryMapSceneUnitModel model;
        public UiModStoryMapSceneUnitParam param;
        public UiModStoryMapSceneCtrl parent=>(UiModStoryMapSceneCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryMapSceneUnitParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryMapSceneUnitView(uiHolder);
            model=new UiModStoryMapSceneUnitModel();


        }

    }
    public partial class UiModStoryMapSceneUnitModel:UiModel
    {
        
    }
}

    public partial class UiModStoryMapSceneParam:UiParam
    {
    }

    public partial class UiModStoryMapSceneView:UiView
    {

            public ModStoryMapSceneList.UiModStoryMapSceneListCtrl page_ModStoryMapSceneList;
            public ModStoryMapSceneUnit.UiModStoryMapSceneUnitCtrl page_ModStoryMapSceneUnit;
        public UiModStoryMapSceneView(UiHolder uiHolder):base(uiHolder)
        {

            page_ModStoryMapSceneList = (ModStoryMapSceneList.UiModStoryMapSceneListCtrl) uiHolder.elementTrsLst[0].GetComponent<UiHolder>().ctrl;
            page_ModStoryMapSceneUnit = (ModStoryMapSceneUnit.UiModStoryMapSceneUnitCtrl) uiHolder.elementTrsLst[1].GetComponent<UiHolder>().ctrl;
        }

    }
    public partial class UiModStoryMapSceneCtrl:UiCtrl
    {
        public UiModStoryMapSceneView view;
        public UiModStoryMapSceneModel model;
        public UiModStoryMapSceneParam param;
        public UiModStoryMapCtrl parent=>(UiModStoryMapCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryMapSceneParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryMapSceneView(uiHolder);
            model=new UiModStoryMapSceneModel();


            view.page_ModStoryMapSceneList = new ModStoryMapSceneList.UiModStoryMapSceneListCtrl();
            view.page_ModStoryMapSceneList.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
            view.page_ModStoryMapSceneUnit = new ModStoryMapSceneUnit.UiModStoryMapSceneUnitCtrl();
            view.page_ModStoryMapSceneUnit.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
        }

    }
    public partial class UiModStoryMapSceneModel:UiModel
    {
        
    }
}

namespace ModStoryMapConfig

{




    public partial class UiModStoryMapConfigParam:UiParam
    {
    }

    public partial class UiModStoryMapConfigView:UiView
    {

        public UiModStoryMapConfigView(UiHolder uiHolder):base(uiHolder)
        {

        }

    }
    public partial class UiModStoryMapConfigCtrl:UiCtrl
    {
        public UiModStoryMapConfigView view;
        public UiModStoryMapConfigModel model;
        public UiModStoryMapConfigParam param;
        public UiModStoryMapCtrl parent=>(UiModStoryMapCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryMapConfigParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryMapConfigView(uiHolder);
            model=new UiModStoryMapConfigModel();


        }

    }
    public partial class UiModStoryMapConfigModel:UiModel
    {
        
    }
}

    public partial class UiModStoryMapParam:UiParam
    {
    }

    public partial class UiModStoryMapView:UiView
    {

            public ModStoryMapMap.UiModStoryMapMapCtrl page_ModStoryMapMap;
            public ModStoryMapScene.UiModStoryMapSceneCtrl page_ModStoryMapScene;
            public ModStoryMapConfig.UiModStoryMapConfigCtrl page_ModStoryMapConfig;
            public Btn btn_map;
            public Sta sta_map;
            public Btn btn_scene;
            public Sta sta_scene;
            public Btn btn_config;
            public Sta sta_config;
        public UiModStoryMapView(UiHolder uiHolder):base(uiHolder)
        {

            page_ModStoryMapMap = (ModStoryMapMap.UiModStoryMapMapCtrl) uiHolder.elementTrsLst[0].GetComponent<UiHolder>().ctrl;
            page_ModStoryMapScene = (ModStoryMapScene.UiModStoryMapSceneCtrl) uiHolder.elementTrsLst[1].GetComponent<UiHolder>().ctrl;
            page_ModStoryMapConfig = (ModStoryMapConfig.UiModStoryMapConfigCtrl) uiHolder.elementTrsLst[2].GetComponent<UiHolder>().ctrl;
            btn_map = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            sta_map = uiHolder.elementTrsLst[4].GetComponent<Sta>();
            btn_scene = uiHolder.elementTrsLst[5].GetComponent<Btn>();
            sta_scene = uiHolder.elementTrsLst[6].GetComponent<Sta>();
            btn_config = uiHolder.elementTrsLst[7].GetComponent<Btn>();
            sta_config = uiHolder.elementTrsLst[8].GetComponent<Sta>();
        }

    }
    public partial class UiModStoryMapCtrl:UiCtrl
    {
        public UiModStoryMapView view;
        public UiModStoryMapModel model;
        public UiModStoryMapParam param;
        public UiModStoryCtrl parent=>(UiModStoryCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryMapParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryMapView(uiHolder);
            model=new UiModStoryMapModel();


            view.page_ModStoryMapMap = new ModStoryMapMap.UiModStoryMapMapCtrl();
            view.page_ModStoryMapMap.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
            view.page_ModStoryMapScene = new ModStoryMapScene.UiModStoryMapSceneCtrl();
            view.page_ModStoryMapScene.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
            view.page_ModStoryMapConfig = new ModStoryMapConfig.UiModStoryMapConfigCtrl();
            view.page_ModStoryMapConfig.BindHolderRecursively(uiHolder.subUiHolderLst[2]);
        }

    }
    public partial class UiModStoryMapModel:UiModel
    {
        
    }
}

    public partial class UiModStoryParam:UiParam
    {
    }

    public partial class UiModStoryView:UiView
    {

            public Btn btn_back;
            public Btn btn_play;
            public Btn btn_save;
            public ModStoryOverview.UiModStoryOverviewCtrl page_ModStoryOverview;
            public ModStoryParameter.UiModStoryParameterCtrl page_ModStoryParameter;
            public ModStoryCharacter.UiModStoryCharacterCtrl page_ModStoryCharacter;
            public ModStorySkill.UiModStorySkillCtrl page_ModStorySkill;
            public ModStoryItem.UiModStoryItemCtrl page_ModStoryItem;
            public ModStoryMapObject.UiModStoryMapObjectCtrl page_ModStoryMapObject;
            public ModStoryEffect.UiModStoryEffectCtrl page_ModStoryEffect;
            public ModStoryEvent.UiModStoryEventCtrl page_ModStoryEvent;
            public ModStoryMap.UiModStoryMapCtrl page_ModStoryMap;
            public Txt txt_title;
            public Btn btn_overview;
            public Sta sta_overview;
            public Btn btn_parameter;
            public Sta sta_parameter;
            public Btn btn_character;
            public Sta sta_character;
            public Btn btn_skill;
            public Sta sta_skill;
            public Btn btn_item;
            public Sta sta_item;
            public Btn btn_mapObject;
            public Sta sta_mapObject;
            public Btn btn_effect;
            public Sta sta_effect;
            public Btn btn_event;
            public Sta sta_event;
            public Btn btn_map;
            public Sta sta_map;
            public Txt txt_overview;
        public UiModStoryView(UiHolder uiHolder):base(uiHolder)
        {

            btn_back = uiHolder.elementTrsLst[0].GetComponent<Btn>();
            btn_play = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            btn_save = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            page_ModStoryOverview = (ModStoryOverview.UiModStoryOverviewCtrl) uiHolder.elementTrsLst[3].GetComponent<UiHolder>().ctrl;
            page_ModStoryParameter = (ModStoryParameter.UiModStoryParameterCtrl) uiHolder.elementTrsLst[4].GetComponent<UiHolder>().ctrl;
            page_ModStoryCharacter = (ModStoryCharacter.UiModStoryCharacterCtrl) uiHolder.elementTrsLst[5].GetComponent<UiHolder>().ctrl;
            page_ModStorySkill = (ModStorySkill.UiModStorySkillCtrl) uiHolder.elementTrsLst[6].GetComponent<UiHolder>().ctrl;
            page_ModStoryItem = (ModStoryItem.UiModStoryItemCtrl) uiHolder.elementTrsLst[7].GetComponent<UiHolder>().ctrl;
            page_ModStoryMapObject = (ModStoryMapObject.UiModStoryMapObjectCtrl) uiHolder.elementTrsLst[8].GetComponent<UiHolder>().ctrl;
            page_ModStoryEffect = (ModStoryEffect.UiModStoryEffectCtrl) uiHolder.elementTrsLst[9].GetComponent<UiHolder>().ctrl;
            page_ModStoryEvent = (ModStoryEvent.UiModStoryEventCtrl) uiHolder.elementTrsLst[10].GetComponent<UiHolder>().ctrl;
            page_ModStoryMap = (ModStoryMap.UiModStoryMapCtrl) uiHolder.elementTrsLst[11].GetComponent<UiHolder>().ctrl;
            txt_title = uiHolder.elementTrsLst[12].GetComponent<Txt>();
            btn_overview = uiHolder.elementTrsLst[13].GetComponent<Btn>();
            sta_overview = uiHolder.elementTrsLst[14].GetComponent<Sta>();
            btn_parameter = uiHolder.elementTrsLst[15].GetComponent<Btn>();
            sta_parameter = uiHolder.elementTrsLst[16].GetComponent<Sta>();
            btn_character = uiHolder.elementTrsLst[17].GetComponent<Btn>();
            sta_character = uiHolder.elementTrsLst[18].GetComponent<Sta>();
            btn_skill = uiHolder.elementTrsLst[19].GetComponent<Btn>();
            sta_skill = uiHolder.elementTrsLst[20].GetComponent<Sta>();
            btn_item = uiHolder.elementTrsLst[21].GetComponent<Btn>();
            sta_item = uiHolder.elementTrsLst[22].GetComponent<Sta>();
            btn_mapObject = uiHolder.elementTrsLst[23].GetComponent<Btn>();
            sta_mapObject = uiHolder.elementTrsLst[24].GetComponent<Sta>();
            btn_effect = uiHolder.elementTrsLst[25].GetComponent<Btn>();
            sta_effect = uiHolder.elementTrsLst[26].GetComponent<Sta>();
            btn_event = uiHolder.elementTrsLst[27].GetComponent<Btn>();
            sta_event = uiHolder.elementTrsLst[28].GetComponent<Sta>();
            btn_map = uiHolder.elementTrsLst[29].GetComponent<Btn>();
            sta_map = uiHolder.elementTrsLst[30].GetComponent<Sta>();
            txt_overview = uiHolder.elementTrsLst[31].GetComponent<Txt>();
        }

    }
    public partial class UiModStoryCtrl:UiCtrl
    {
        public UiModStoryView view;
        public UiModStoryModel model;
        public UiModStoryParam param;
        

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryView(uiHolder);
            model=new UiModStoryModel();


            view.page_ModStoryOverview = new ModStoryOverview.UiModStoryOverviewCtrl();
            view.page_ModStoryOverview.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
            view.page_ModStoryParameter = new ModStoryParameter.UiModStoryParameterCtrl();
            view.page_ModStoryParameter.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
            view.page_ModStoryCharacter = new ModStoryCharacter.UiModStoryCharacterCtrl();
            view.page_ModStoryCharacter.BindHolderRecursively(uiHolder.subUiHolderLst[2]);
            view.page_ModStorySkill = new ModStorySkill.UiModStorySkillCtrl();
            view.page_ModStorySkill.BindHolderRecursively(uiHolder.subUiHolderLst[3]);
            view.page_ModStoryItem = new ModStoryItem.UiModStoryItemCtrl();
            view.page_ModStoryItem.BindHolderRecursively(uiHolder.subUiHolderLst[4]);
            view.page_ModStoryMapObject = new ModStoryMapObject.UiModStoryMapObjectCtrl();
            view.page_ModStoryMapObject.BindHolderRecursively(uiHolder.subUiHolderLst[5]);
            view.page_ModStoryEffect = new ModStoryEffect.UiModStoryEffectCtrl();
            view.page_ModStoryEffect.BindHolderRecursively(uiHolder.subUiHolderLst[6]);
            view.page_ModStoryEvent = new ModStoryEvent.UiModStoryEventCtrl();
            view.page_ModStoryEvent.BindHolderRecursively(uiHolder.subUiHolderLst[7]);
            view.page_ModStoryMap = new ModStoryMap.UiModStoryMapCtrl();
            view.page_ModStoryMap.BindHolderRecursively(uiHolder.subUiHolderLst[8]);
        }

    }
    public partial class UiModStoryModel:UiModel
    {
        
    }
}
