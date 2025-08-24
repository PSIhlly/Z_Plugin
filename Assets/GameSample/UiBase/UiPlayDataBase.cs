
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;
namespace Ui.PlayData

{




namespace PlayDataBackpack

{







    public partial class UiGameItemParam:UiParam
    {
    }

    public partial class UiGameItemView:UiView
    {

            public GameObject go_gameItem;
            public Sta sta_gameItem;
            public Sta sta_exist;
            public Btn btn_new;
            public Btn btn_;
            public Sta sta_;
            public Txt txt_count;
            public Img img_;
            public Txt txt_;
        public UiGameItemView(UiHolder uiHolder):base(uiHolder)
        {

            go_gameItem = uiHolder.elementTrsLst[0].gameObject;
            sta_gameItem = uiHolder.elementTrsLst[1].GetComponent<Sta>();
            sta_exist = uiHolder.elementTrsLst[2].GetComponent<Sta>();
            btn_new = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            btn_ = uiHolder.elementTrsLst[4].GetComponent<Btn>();
            sta_ = uiHolder.elementTrsLst[5].GetComponent<Sta>();
            txt_count = uiHolder.elementTrsLst[6].GetComponent<Txt>();
            img_ = uiHolder.elementTrsLst[7].GetComponent<Img>();
            txt_ = uiHolder.elementTrsLst[8].GetComponent<Txt>();
        }

    }
    public partial class UiGameItemCtrl:UiCtrl
    {
        public UiGameItemView view;
        public UiGameItemModel model;
        public UiGameItemParam param;
        public UiPlayDataBackpackCtrl parent=>(UiPlayDataBackpackCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiGameItemParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiGameItemView(uiHolder);
            model=new UiGameItemModel();


        }

    }
    public partial class UiGameItemModel:UiModel
    {
        
    }
    public partial class UiPlayDataBackpackParam:UiParam
    {
    }

    public partial class UiPlayDataBackpackView:UiView
    {

            public ScrView scr_gameItems;
            public Sta sta_show;
            public Img img_;
            public Btn btn_;
            public Txt txt_desc;
            public GameObject go_gameItem;
            public Sta sta_gameItem;
            public UiGameItemCtrl sub_GameItem;
            public Btn btn_drop;
            public Btn btn_equip;
            public Btn btn_use;
        public UiPlayDataBackpackView(UiHolder uiHolder):base(uiHolder)
        {

            scr_gameItems = uiHolder.elementTrsLst[0].GetComponent<ScrView>();
            sta_show = uiHolder.elementTrsLst[1].GetComponent<Sta>();
            img_ = uiHolder.elementTrsLst[2].GetComponent<Img>();
            btn_ = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            txt_desc = uiHolder.elementTrsLst[4].GetComponent<Txt>();
            go_gameItem = uiHolder.elementTrsLst[5].gameObject;
            sta_gameItem = uiHolder.elementTrsLst[6].GetComponent<Sta>();
            sub_GameItem = (UiGameItemCtrl) uiHolder.elementTrsLst[7].GetComponent<UiHolder>().ctrl;
            btn_drop = uiHolder.elementTrsLst[8].GetComponent<Btn>();
            btn_equip = uiHolder.elementTrsLst[9].GetComponent<Btn>();
            btn_use = uiHolder.elementTrsLst[10].GetComponent<Btn>();
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


            view.sub_GameItem = new UiGameItemCtrl();
            view.sub_GameItem.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
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

            public Btn btn_back;
            public PlayDataBackpack.UiPlayDataBackpackCtrl page_PlayDataBackpack;
            public Btn btn_backpack;
            public Sta sta_backpack;
            public Btn btn_character;
            public Sta sta_character;
        public UiPlayDataView(UiHolder uiHolder):base(uiHolder)
        {

            btn_back = uiHolder.elementTrsLst[0].GetComponent<Btn>();
            page_PlayDataBackpack = (PlayDataBackpack.UiPlayDataBackpackCtrl) uiHolder.elementTrsLst[1].GetComponent<UiHolder>().ctrl;
            btn_backpack = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            sta_backpack = uiHolder.elementTrsLst[3].GetComponent<Sta>();
            btn_character = uiHolder.elementTrsLst[4].GetComponent<Btn>();
            sta_character = uiHolder.elementTrsLst[5].GetComponent<Sta>();
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
