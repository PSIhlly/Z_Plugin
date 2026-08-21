
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;
using Z_Texture;using RenderHeads.Media.AVProVideo;
namespace Ui.ModAssetSelectWindow

{







    public partial class UiLabParam:UiParam
    {
    }

    public partial class UiLabView:UiView
    {

            public GameObject go_lab;
            public Btn btn_;
            public Sta sta_;
            public Sta sta_state;
            public Txt txt_;
        public UiLabView(UiHolder uiHolder):base(uiHolder)
        {

            go_lab = uiHolder.elementTrsLst[0].gameObject;
            btn_ = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            sta_ = uiHolder.elementTrsLst[2].GetComponent<Sta>();
            sta_state = uiHolder.elementTrsLst[3].GetComponent<Sta>();
            txt_ = uiHolder.elementTrsLst[4].GetComponent<Txt>();
        }

    }
    public partial class UiLabCtrl:UiCtrl
    {
        public UiLabView view;
        public UiLabModel model;
        public UiLabParam param;
        public UiModAssetSelectWindowCtrl parent=>(UiModAssetSelectWindowCtrl)uiHolder.parent.ctrl;

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
        public UiModAssetSelectWindowCtrl parent=>(UiModAssetSelectWindowCtrl)uiHolder.parent.ctrl;

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
    public partial class UiModAssetSelectWindowParam:UiParam
    {
    }

    public partial class UiModAssetSelectWindowView:UiView
    {

            public MediaPlayer mp_;
            public Btn btn_bg;
            public ScrView scr_labs;
            public ScrView scr_items;
            public Sta sta_selected;
            public Btn btn_close;
            public Btn btn_delete;
            public Btn btn_replace;
            public Ipt ipt_labelName;
            public Btn btn_labelDelete;
            public GameObject go_lab;
            public UiLabCtrl sub_lab;
            public GameObject go_item;
            public UiItemCtrl sub_item;
            public Btn btn_;
            public Sta sta_;
            public Btn btn_setLabel;
            public Sta sta_setLabel;
            public Ipt ipt_name;
        public UiModAssetSelectWindowView(UiHolder uiHolder):base(uiHolder)
        {

            mp_ = uiHolder.elementTrsLst[0].GetComponent<MediaPlayer>();
            btn_bg = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            scr_labs = uiHolder.elementTrsLst[2].GetComponent<ScrView>();
            scr_items = uiHolder.elementTrsLst[3].GetComponent<ScrView>();
            sta_selected = uiHolder.elementTrsLst[4].GetComponent<Sta>();
            btn_close = uiHolder.elementTrsLst[5].GetComponent<Btn>();
            btn_delete = uiHolder.elementTrsLst[6].GetComponent<Btn>();
            btn_replace = uiHolder.elementTrsLst[7].GetComponent<Btn>();
            ipt_labelName = uiHolder.elementTrsLst[8].GetComponent<Ipt>();
            btn_labelDelete = uiHolder.elementTrsLst[9].GetComponent<Btn>();
            go_lab = uiHolder.elementTrsLst[10].gameObject;
            sub_lab = (UiLabCtrl) uiHolder.elementTrsLst[11].GetComponent<UiHolder>().ctrl;
            go_item = uiHolder.elementTrsLst[12].gameObject;
            sub_item = (UiItemCtrl) uiHolder.elementTrsLst[13].GetComponent<UiHolder>().ctrl;
            btn_ = uiHolder.elementTrsLst[14].GetComponent<Btn>();
            sta_ = uiHolder.elementTrsLst[15].GetComponent<Sta>();
            btn_setLabel = uiHolder.elementTrsLst[16].GetComponent<Btn>();
            sta_setLabel = uiHolder.elementTrsLst[17].GetComponent<Sta>();
            ipt_name = uiHolder.elementTrsLst[18].GetComponent<Ipt>();
        }

    }
    public partial class UiModAssetSelectWindowCtrl:UiCtrl
    {
        public UiModAssetSelectWindowView view;
        public UiModAssetSelectWindowModel model;
        public UiModAssetSelectWindowParam param;
        

        public override void SetParam(UiParam param)
        {
            this.param = (UiModAssetSelectWindowParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModAssetSelectWindowView(uiHolder);
            model=new UiModAssetSelectWindowModel();


            view.sub_lab = new UiLabCtrl();
            view.sub_lab.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
            view.sub_item = new UiItemCtrl();
            view.sub_item.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
        }

    }
    public partial class UiModAssetSelectWindowModel:UiModel
    {
        
    }
}
