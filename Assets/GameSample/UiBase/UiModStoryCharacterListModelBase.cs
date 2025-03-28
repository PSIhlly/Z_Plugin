
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;

namespace Ui.ModStoryCharacterListModel

{



    public partial class UiItemParam:UiParam
    {
    }

    public partial class UiItemView:UiView
    {

            public GameObject go_item;
            public Btn btn_item;
            public Sta sta_item;
            public Txt txt_name;
        public UiItemView(UiHolder uiHolder):base(uiHolder)
        {

            go_item = uiHolder.elementTrsLst[0].gameObject;
            btn_item = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            sta_item = uiHolder.elementTrsLst[2].GetComponent<Sta>();
            txt_name = uiHolder.elementTrsLst[3].GetComponent<Txt>();
        }

    }
    public partial class UiItemCtrl:UiCtrl
    {
        public UiItemView view;
        public UiItemModel model;
        public UiItemParam param;
        public UiModStoryCharacterListModelCtrl parent=>(UiModStoryCharacterListModelCtrl)uiHolder.parent.ctrl;

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

    public partial class UiUnitParam:UiParam
    {
    }

    public partial class UiUnitView:UiView
    {

            public GameObject go_unit;
            public Btn btn_unit;
            public Sta sta_unit;
            public Img img_;
            public Txt txt_name;
        public UiUnitView(UiHolder uiHolder):base(uiHolder)
        {

            go_unit = uiHolder.elementTrsLst[0].gameObject;
            btn_unit = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            sta_unit = uiHolder.elementTrsLst[2].GetComponent<Sta>();
            img_ = uiHolder.elementTrsLst[3].GetComponent<Img>();
            txt_name = uiHolder.elementTrsLst[4].GetComponent<Txt>();
        }

    }
    public partial class UiUnitCtrl:UiCtrl
    {
        public UiUnitView view;
        public UiUnitModel model;
        public UiUnitParam param;
        public UiModStoryCharacterListModelCtrl parent=>(UiModStoryCharacterListModelCtrl)uiHolder.parent.ctrl;

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
    public partial class UiModStoryCharacterListModelParam:UiParam
    {
    }

    public partial class UiModStoryCharacterListModelView:UiView
    {

            public Btn btn_back;
            public Sta sta_show;
            public GameObject go_items;
            public ScrView scr_items;
            public Ipt ipt_name;
            public Btn btn_up;
            public Sta sta_up;
            public Btn btn_down;
            public Sta sta_down;
            public Btn btn_delete;
            public Btn btn_play;
            public Ipt ipt_interval;
            public Ipt ipt_scale;
            public Sta sta_innerId;
            public GameObject go_units;
            public ScrView scr_units;
            public Txt txt_name;
            public Txt txt_interval;
            public Txt txt_scale;
            public GameObject go_item;
            public UiItemCtrl sub_Item;
            public Btn btn_leftHand;
            public Sta sta_leftHand;
            public Btn btn_rightHand;
            public Sta sta_rightHand;
            public Btn btn_head;
            public Sta sta_head;
            public Btn btn_replace;
            public Btn btn_deleteId;
            public GameObject go_unit;
            public UiUnitCtrl sub_Unit;
        public UiModStoryCharacterListModelView(UiHolder uiHolder):base(uiHolder)
        {

            btn_back = uiHolder.elementTrsLst[0].GetComponent<Btn>();
            sta_show = uiHolder.elementTrsLst[1].GetComponent<Sta>();
            go_items = uiHolder.elementTrsLst[2].gameObject;
            scr_items = uiHolder.elementTrsLst[3].GetComponent<ScrView>();
            ipt_name = uiHolder.elementTrsLst[4].GetComponent<Ipt>();
            btn_up = uiHolder.elementTrsLst[5].GetComponent<Btn>();
            sta_up = uiHolder.elementTrsLst[6].GetComponent<Sta>();
            btn_down = uiHolder.elementTrsLst[7].GetComponent<Btn>();
            sta_down = uiHolder.elementTrsLst[8].GetComponent<Sta>();
            btn_delete = uiHolder.elementTrsLst[9].GetComponent<Btn>();
            btn_play = uiHolder.elementTrsLst[10].GetComponent<Btn>();
            ipt_interval = uiHolder.elementTrsLst[11].GetComponent<Ipt>();
            ipt_scale = uiHolder.elementTrsLst[12].GetComponent<Ipt>();
            sta_innerId = uiHolder.elementTrsLst[13].GetComponent<Sta>();
            go_units = uiHolder.elementTrsLst[14].gameObject;
            scr_units = uiHolder.elementTrsLst[15].GetComponent<ScrView>();
            txt_name = uiHolder.elementTrsLst[16].GetComponent<Txt>();
            txt_interval = uiHolder.elementTrsLst[17].GetComponent<Txt>();
            txt_scale = uiHolder.elementTrsLst[18].GetComponent<Txt>();
            go_item = uiHolder.elementTrsLst[19].gameObject;
            sub_Item = (UiItemCtrl) uiHolder.elementTrsLst[20].GetComponent<UiHolder>().ctrl;
            btn_leftHand = uiHolder.elementTrsLst[21].GetComponent<Btn>();
            sta_leftHand = uiHolder.elementTrsLst[22].GetComponent<Sta>();
            btn_rightHand = uiHolder.elementTrsLst[23].GetComponent<Btn>();
            sta_rightHand = uiHolder.elementTrsLst[24].GetComponent<Sta>();
            btn_head = uiHolder.elementTrsLst[25].GetComponent<Btn>();
            sta_head = uiHolder.elementTrsLst[26].GetComponent<Sta>();
            btn_replace = uiHolder.elementTrsLst[27].GetComponent<Btn>();
            btn_deleteId = uiHolder.elementTrsLst[28].GetComponent<Btn>();
            go_unit = uiHolder.elementTrsLst[29].gameObject;
            sub_Unit = (UiUnitCtrl) uiHolder.elementTrsLst[30].GetComponent<UiHolder>().ctrl;
        }

    }
    public partial class UiModStoryCharacterListModelCtrl:UiCtrl
    {
        public UiModStoryCharacterListModelView view;
        public UiModStoryCharacterListModelModel model;
        public UiModStoryCharacterListModelParam param;
        

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryCharacterListModelParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryCharacterListModelView(uiHolder);
            model=new UiModStoryCharacterListModelModel();


            view.sub_Item = new UiItemCtrl();
            view.sub_Item.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
            view.sub_Unit = new UiUnitCtrl();
            view.sub_Unit.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
        }

    }
    public partial class UiModStoryCharacterListModelModel:UiModel
    {
        
    }
}
