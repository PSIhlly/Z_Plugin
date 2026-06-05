
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;
using Z_Texture;using RenderHeads.Media.AVProVideo;
namespace Ui.PlayDataBackpack

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
        public UiPlayDataBackpackCtrl parent=>(UiPlayDataBackpackCtrl)uiHolder.parent.ctrl;

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



    public partial class UiGameItemParam:UiParam
    {
    }

    public partial class UiGameItemView:UiView
    {

            public GameObject go_gameItem;
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
            sta_exist = uiHolder.elementTrsLst[1].GetComponent<Sta>();
            btn_new = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            btn_ = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            sta_ = uiHolder.elementTrsLst[4].GetComponent<Sta>();
            txt_count = uiHolder.elementTrsLst[5].GetComponent<Txt>();
            img_ = uiHolder.elementTrsLst[6].GetComponent<Img>();
            txt_ = uiHolder.elementTrsLst[7].GetComponent<Txt>();
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



    public partial class UiGameArgsParam:UiParam
    {
    }

    public partial class UiGameArgsView:UiView
    {

            public GameObject go_gameArgs;
            public Sta sta_gameArgs;
            public Txt txt_;
        public UiGameArgsView(UiHolder uiHolder):base(uiHolder)
        {

            go_gameArgs = uiHolder.elementTrsLst[0].gameObject;
            sta_gameArgs = uiHolder.elementTrsLst[1].GetComponent<Sta>();
            txt_ = uiHolder.elementTrsLst[2].GetComponent<Txt>();
        }

    }
    public partial class UiGameArgsCtrl:UiCtrl
    {
        public UiGameArgsView view;
        public UiGameArgsModel model;
        public UiGameArgsParam param;
        public UiPlayDataBackpackCtrl parent=>(UiPlayDataBackpackCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiGameArgsParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiGameArgsView(uiHolder);
            model=new UiGameArgsModel();


        }

    }
    public partial class UiGameArgsModel:UiModel
    {
        
    }
    public partial class UiPlayDataBackpackParam:UiParam
    {
    }

    public partial class UiPlayDataBackpackView:UiView
    {

            public ScrView scr_labs;
            public ScrView scr_gameItems;
            public Sta sta_show;
            public Btn btn_back;
            public Sta sta_showArea;
            public Img img_;
            public Btn btn_;
            public Txt txt_desc;
            public ScrView scr_gameArgs;
            public Txt txt_name;
            public Txt txt_amount;
            public GameObject go_lab;
            public UiLabCtrl sub_lab;
            public GameObject go_gameItem;
            public UiGameItemCtrl sub_gameItem;
            public Btn btn_drop;
            public Btn btn_use;
            public Btn btn_equip;
            public Btn btn_unequip;
            public GameObject go_gameArgs;
            public Sta sta_gameArgs;
            public UiGameArgsCtrl sub_gameArgs;
        public UiPlayDataBackpackView(UiHolder uiHolder):base(uiHolder)
        {

            scr_labs = uiHolder.elementTrsLst[0].GetComponent<ScrView>();
            scr_gameItems = uiHolder.elementTrsLst[1].GetComponent<ScrView>();
            sta_show = uiHolder.elementTrsLst[2].GetComponent<Sta>();
            btn_back = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            sta_showArea = uiHolder.elementTrsLst[4].GetComponent<Sta>();
            img_ = uiHolder.elementTrsLst[5].GetComponent<Img>();
            btn_ = uiHolder.elementTrsLst[6].GetComponent<Btn>();
            txt_desc = uiHolder.elementTrsLst[7].GetComponent<Txt>();
            scr_gameArgs = uiHolder.elementTrsLst[8].GetComponent<ScrView>();
            txt_name = uiHolder.elementTrsLst[9].GetComponent<Txt>();
            txt_amount = uiHolder.elementTrsLst[10].GetComponent<Txt>();
            go_lab = uiHolder.elementTrsLst[11].gameObject;
            sub_lab = (UiLabCtrl) uiHolder.elementTrsLst[12].GetComponent<UiHolder>().ctrl;
            go_gameItem = uiHolder.elementTrsLst[13].gameObject;
            sub_gameItem = (UiGameItemCtrl) uiHolder.elementTrsLst[14].GetComponent<UiHolder>().ctrl;
            btn_drop = uiHolder.elementTrsLst[15].GetComponent<Btn>();
            btn_use = uiHolder.elementTrsLst[16].GetComponent<Btn>();
            btn_equip = uiHolder.elementTrsLst[17].GetComponent<Btn>();
            btn_unequip = uiHolder.elementTrsLst[18].GetComponent<Btn>();
            go_gameArgs = uiHolder.elementTrsLst[19].gameObject;
            sta_gameArgs = uiHolder.elementTrsLst[20].GetComponent<Sta>();
            sub_gameArgs = (UiGameArgsCtrl) uiHolder.elementTrsLst[21].GetComponent<UiHolder>().ctrl;
        }

    }
    public partial class UiPlayDataBackpackCtrl:UiCtrl
    {
        public UiPlayDataBackpackView view;
        public UiPlayDataBackpackModel model;
        public UiPlayDataBackpackParam param;
        

        public override void SetParam(UiParam param)
        {
            this.param = (UiPlayDataBackpackParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiPlayDataBackpackView(uiHolder);
            model=new UiPlayDataBackpackModel();


            view.sub_lab = new UiLabCtrl();
            view.sub_lab.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
            view.sub_gameItem = new UiGameItemCtrl();
            view.sub_gameItem.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
            view.sub_gameArgs = new UiGameArgsCtrl();
            view.sub_gameArgs.BindHolderRecursively(uiHolder.subUiHolderLst[2]);
        }

    }
    public partial class UiPlayDataBackpackModel:UiModel
    {
        
    }
}
