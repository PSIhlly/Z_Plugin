
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;

namespace Ui.ModStoryCharacter
{




    public partial class UiArgParam:UiParam
    {
    }

    public partial class UiArgView:UiView
    {

            public GameObject go_arg;
            public Sta sta_arg;
            public Btn btn_new;
            public Ipt ipt_name;
            public Btn btn_delete;
            public Img img_;
            public Txt txt_new;
            public Txt txt_name;
            public Img img_delete;
            public Txt txt_delete;
        public UiArgView(UiHolder uiHolder):base(uiHolder)
        {

            go_arg = uiHolder.elementTrsLst[0].gameObject;
            sta_arg = uiHolder.elementTrsLst[1].GetComponent<Sta>();
            btn_new = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            ipt_name = uiHolder.elementTrsLst[3].GetComponent<Ipt>();
            btn_delete = uiHolder.elementTrsLst[4].GetComponent<Btn>();
            img_ = uiHolder.elementTrsLst[5].GetComponent<Img>();
            txt_new = uiHolder.elementTrsLst[6].GetComponent<Txt>();
            txt_name = uiHolder.elementTrsLst[7].GetComponent<Txt>();
            img_delete = uiHolder.elementTrsLst[8].GetComponent<Img>();
            txt_delete = uiHolder.elementTrsLst[9].GetComponent<Txt>();
        }

    }
    public partial class UiArgCtrl:UiCtrl
    {
        public UiArgView view;
        public UiArgModel model;
        public UiArgParam param;
        public UiModStoryCharacterArgumentsCtrl parent=>(UiModStoryCharacterArgumentsCtrl)uiHolder.parent.ctrl;

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
    public partial class UiModStoryCharacterArgumentsParam:UiParam
    {
    }

    public partial class UiModStoryCharacterArgumentsView:UiView
    {

            public GameObject go_items;
            public ScrView scr_items;
            public GameObject go_arg;
            public Sta sta_arg;
            public UiArgCtrl sub_Arg;
        public UiModStoryCharacterArgumentsView(UiHolder uiHolder):base(uiHolder)
        {

            go_items = uiHolder.elementTrsLst[0].gameObject;
            scr_items = uiHolder.elementTrsLst[1].GetComponent<ScrView>();
            go_arg = uiHolder.elementTrsLst[2].gameObject;
            sta_arg = uiHolder.elementTrsLst[3].GetComponent<Sta>();
            sub_Arg = (UiArgCtrl) uiHolder.elementTrsLst[4].GetComponent<UiHolder>().ctrl;
        }

    }
    public partial class UiModStoryCharacterArgumentsCtrl:UiCtrl
    {
        public UiModStoryCharacterArgumentsView view;
        public UiModStoryCharacterArgumentsModel model;
        public UiModStoryCharacterArgumentsParam param;
        public UiModStoryCharacterCtrl parent=>(UiModStoryCharacterCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryCharacterArgumentsParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryCharacterArgumentsView(uiHolder);
            model=new UiModStoryCharacterArgumentsModel();


            view.sub_Arg = new UiArgCtrl();
            view.sub_Arg.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
        }

    }
    public partial class UiModStoryCharacterArgumentsModel:UiModel
    {
        
    }


    public partial class UiMaskItemParam:UiParam
    {
    }

    public partial class UiMaskItemView:UiView
    {

            public GameObject go_maskItem;
            public Btn btn_item;
            public Sta sta_item;
            public Img img_;
            public Txt txt_name;
        public UiMaskItemView(UiHolder uiHolder):base(uiHolder)
        {

            go_maskItem = uiHolder.elementTrsLst[0].gameObject;
            btn_item = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            sta_item = uiHolder.elementTrsLst[2].GetComponent<Sta>();
            img_ = uiHolder.elementTrsLst[3].GetComponent<Img>();
            txt_name = uiHolder.elementTrsLst[4].GetComponent<Txt>();
        }

    }
    public partial class UiMaskItemCtrl:UiCtrl
    {
        public UiMaskItemView view;
        public UiMaskItemModel model;
        public UiMaskItemParam param;
        public UiModStoryCharacterListCtrl parent=>(UiModStoryCharacterListCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiMaskItemParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiMaskItemView(uiHolder);
            model=new UiMaskItemModel();


        }

    }
    public partial class UiMaskItemModel:UiModel
    {
        
    }

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
        public UiModStoryCharacterListCtrl parent=>(UiModStoryCharacterListCtrl)uiHolder.parent.ctrl;

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
    public partial class UiModStoryCharacterListParam:UiParam
    {
    }

    public partial class UiModStoryCharacterListView:UiView
    {

            public Sta sta_exist;
            public GameObject go_items;
            public ScrView scr_items;
            public Img img_tex;
            public Btn btn_replace;
            public Btn btn_delete;
            public Ipt ipt_name;
            public GameObject go_units;
            public ScrView scr_units;
            public GameObject go_maskItem;
            public UiMaskItemCtrl sub_MaskItem;
            public Txt txt_replace;
            public Txt txt_delete;
            public Txt txt_name;
            public GameObject go_unit;
            public Sta sta_unit;
            public UiUnitCtrl sub_Unit;
        public UiModStoryCharacterListView(UiHolder uiHolder):base(uiHolder)
        {

            sta_exist = uiHolder.elementTrsLst[0].GetComponent<Sta>();
            go_items = uiHolder.elementTrsLst[1].gameObject;
            scr_items = uiHolder.elementTrsLst[2].GetComponent<ScrView>();
            img_tex = uiHolder.elementTrsLst[3].GetComponent<Img>();
            btn_replace = uiHolder.elementTrsLst[4].GetComponent<Btn>();
            btn_delete = uiHolder.elementTrsLst[5].GetComponent<Btn>();
            ipt_name = uiHolder.elementTrsLst[6].GetComponent<Ipt>();
            go_units = uiHolder.elementTrsLst[7].gameObject;
            scr_units = uiHolder.elementTrsLst[8].GetComponent<ScrView>();
            go_maskItem = uiHolder.elementTrsLst[9].gameObject;
            sub_MaskItem = (UiMaskItemCtrl) uiHolder.elementTrsLst[10].GetComponent<UiHolder>().ctrl;
            txt_replace = uiHolder.elementTrsLst[11].GetComponent<Txt>();
            txt_delete = uiHolder.elementTrsLst[12].GetComponent<Txt>();
            txt_name = uiHolder.elementTrsLst[13].GetComponent<Txt>();
            go_unit = uiHolder.elementTrsLst[14].gameObject;
            sta_unit = uiHolder.elementTrsLst[15].GetComponent<Sta>();
            sub_Unit = (UiUnitCtrl) uiHolder.elementTrsLst[16].GetComponent<UiHolder>().ctrl;
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


            view.sub_MaskItem = new UiMaskItemCtrl();
            view.sub_MaskItem.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
            view.sub_Unit = new UiUnitCtrl();
            view.sub_Unit.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
        }

    }
    public partial class UiModStoryCharacterListModel:UiModel
    {
        
    }
    public partial class UiModStoryCharacterParam:UiParam
    {
    }

    public partial class UiModStoryCharacterView:UiView
    {

            public UiModStoryCharacterArgumentsCtrl sub_ModStoryCharacterArguments;
            public UiModStoryCharacterListCtrl sub_ModStoryCharacterList;
            public Btn btn_back;
            public Btn btn_args;
            public Sta sta_args;
            public Btn btn_list;
            public Sta sta_list;
            public Img img_args;
            public Txt txt_args;
            public Img img_list;
            public Txt txt_list;
        public UiModStoryCharacterView(UiHolder uiHolder):base(uiHolder)
        {

            sub_ModStoryCharacterArguments = (UiModStoryCharacterArgumentsCtrl) uiHolder.elementTrsLst[0].GetComponent<UiHolder>().ctrl;
            sub_ModStoryCharacterList = (UiModStoryCharacterListCtrl) uiHolder.elementTrsLst[1].GetComponent<UiHolder>().ctrl;
            btn_back = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            btn_args = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            sta_args = uiHolder.elementTrsLst[4].GetComponent<Sta>();
            btn_list = uiHolder.elementTrsLst[5].GetComponent<Btn>();
            sta_list = uiHolder.elementTrsLst[6].GetComponent<Sta>();
            img_args = uiHolder.elementTrsLst[7].GetComponent<Img>();
            txt_args = uiHolder.elementTrsLst[8].GetComponent<Txt>();
            img_list = uiHolder.elementTrsLst[9].GetComponent<Img>();
            txt_list = uiHolder.elementTrsLst[10].GetComponent<Txt>();
        }

    }
    public partial class UiModStoryCharacterCtrl:UiCtrl
    {
        public UiModStoryCharacterView view;
        public UiModStoryCharacterModel model;
        public UiModStoryCharacterParam param;
        

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryCharacterParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryCharacterView(uiHolder);
            model=new UiModStoryCharacterModel();


            view.sub_ModStoryCharacterArguments = new UiModStoryCharacterArgumentsCtrl();
            view.sub_ModStoryCharacterArguments.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
            view.sub_ModStoryCharacterList = new UiModStoryCharacterListCtrl();
            view.sub_ModStoryCharacterList.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
        }

    }
    public partial class UiModStoryCharacterModel:UiModel
    {
        
    }
}
