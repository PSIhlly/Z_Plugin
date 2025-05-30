
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;

namespace Ui.PlayData

{


namespace PlayDataBackpack

{



    public partial class UiItemParam:UiParam
    {
    }

    public partial class UiItemView:UiView
    {

            public GameObject go_item;
            public Sta sta_item;
            public Img img_;
            public Btn btn_;
            public Txt txt_amount;
        public UiItemView(UiHolder uiHolder):base(uiHolder)
        {

            go_item = uiHolder.elementTrsLst[0].gameObject;
            sta_item = uiHolder.elementTrsLst[1].GetComponent<Sta>();
            img_ = uiHolder.elementTrsLst[2].GetComponent<Img>();
            btn_ = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            txt_amount = uiHolder.elementTrsLst[4].GetComponent<Txt>();
        }

    }
    public partial class UiItemCtrl:UiCtrl
    {
        public UiItemView view;
        public UiItemModel model;
        public UiItemParam param;
        public UiPlayDataBackpackCtrl parent=>(UiPlayDataBackpackCtrl)uiHolder.parent.ctrl;

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
    public partial class UiPlayDataBackpackParam:UiParam
    {
    }

    public partial class UiPlayDataBackpackView:UiView
    {

            public ScrView scr_;
            public Sta sta_show;
            public Img img_;
            public Btn btn_;
            public Txt txt_desc;
            public Img img_drop;
            public Btn btn_drop;
            public Img img_use;
            public Btn btn_use;
            public Img img_equip;
            public Btn btn_equip;
            public GameObject go_item;
            public Sta sta_item;
            public UiItemCtrl sub_Item;
            public Txt txt_amount;
            public Txt txt_drop;
            public Txt txt_use;
            public Txt txt_equip;
        public UiPlayDataBackpackView(UiHolder uiHolder):base(uiHolder)
        {

            scr_ = uiHolder.elementTrsLst[0].GetComponent<ScrView>();
            sta_show = uiHolder.elementTrsLst[1].GetComponent<Sta>();
            img_ = uiHolder.elementTrsLst[2].GetComponent<Img>();
            btn_ = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            txt_desc = uiHolder.elementTrsLst[4].GetComponent<Txt>();
            img_drop = uiHolder.elementTrsLst[5].GetComponent<Img>();
            btn_drop = uiHolder.elementTrsLst[6].GetComponent<Btn>();
            img_use = uiHolder.elementTrsLst[7].GetComponent<Img>();
            btn_use = uiHolder.elementTrsLst[8].GetComponent<Btn>();
            img_equip = uiHolder.elementTrsLst[9].GetComponent<Img>();
            btn_equip = uiHolder.elementTrsLst[10].GetComponent<Btn>();
            go_item = uiHolder.elementTrsLst[11].gameObject;
            sta_item = uiHolder.elementTrsLst[12].GetComponent<Sta>();
            sub_Item = (UiItemCtrl) uiHolder.elementTrsLst[13].GetComponent<UiHolder>().ctrl;
            txt_amount = uiHolder.elementTrsLst[14].GetComponent<Txt>();
            txt_drop = uiHolder.elementTrsLst[15].GetComponent<Txt>();
            txt_use = uiHolder.elementTrsLst[16].GetComponent<Txt>();
            txt_equip = uiHolder.elementTrsLst[17].GetComponent<Txt>();
        }

    }
    public partial class UiPlayDataBackpackCtrl:UiCtrl
    {
        public UiPlayDataBackpackView view;
        public UiPlayDataBackpackModel model;
        public UiPlayDataBackpackParam param;
        public UiPlayDataCtrl parent=>(UiPlayDataCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiPlayDataBackpackParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiPlayDataBackpackView(uiHolder);
            model=new UiPlayDataBackpackModel();


            view.sub_Item = new UiItemCtrl();
            view.sub_Item.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
        }

    }
    public partial class UiPlayDataBackpackModel:UiModel
    {
        
    }
}

    public partial class UiPlayDataParam:UiParam
    {
    }

    public partial class UiPlayDataView:UiView
    {

            public PlayDataBackpack.UiPlayDataBackpackCtrl page_PlayDataBackpack;
            public Btn btn_back;
            public Btn btn_backpack;
            public Sta sta_backpack;
            public Btn btn_character;
            public Sta sta_character;
            public Img img_backpack;
            public Txt txt_backpack;
            public Img img_character;
            public Txt txt_character;
        public UiPlayDataView(UiHolder uiHolder):base(uiHolder)
        {

            page_PlayDataBackpack = (PlayDataBackpack.UiPlayDataBackpackCtrl) uiHolder.elementTrsLst[0].GetComponent<UiHolder>().ctrl;
            btn_back = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            btn_backpack = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            sta_backpack = uiHolder.elementTrsLst[3].GetComponent<Sta>();
            btn_character = uiHolder.elementTrsLst[4].GetComponent<Btn>();
            sta_character = uiHolder.elementTrsLst[5].GetComponent<Sta>();
            img_backpack = uiHolder.elementTrsLst[6].GetComponent<Img>();
            txt_backpack = uiHolder.elementTrsLst[7].GetComponent<Txt>();
            img_character = uiHolder.elementTrsLst[8].GetComponent<Img>();
            txt_character = uiHolder.elementTrsLst[9].GetComponent<Txt>();
        }

    }
    public partial class UiPlayDataCtrl:UiCtrl
    {
        public UiPlayDataView view;
        public UiPlayDataModel model;
        public UiPlayDataParam param;
        

        public override void SetParam(UiParam param)
        {
            this.param = (UiPlayDataParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiPlayDataView(uiHolder);
            model=new UiPlayDataModel();


            view.page_PlayDataBackpack = new PlayDataBackpack.UiPlayDataBackpackCtrl();
            view.page_PlayDataBackpack.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
        }

    }
    public partial class UiPlayDataModel:UiModel
    {
        
    }
}
