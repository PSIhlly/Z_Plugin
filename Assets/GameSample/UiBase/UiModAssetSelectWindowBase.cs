
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;
using Z_Texture;using RenderHeads.Media.AVProVideo;
namespace Ui.ModAssetSelectWindow

{







    public partial class UiItemParam:UiParam
    {
    }

    public partial class UiItemView:UiView
    {

            public GameObject go_item;
            public Sta sta_item;
            public Btn btn_;
            public Sta sta_;
            public Img img_;
            public Txt txt_;
        public UiItemView(UiHolder uiHolder):base(uiHolder)
        {

            go_item = uiHolder.elementTrsLst[0].gameObject;
            sta_item = uiHolder.elementTrsLst[1].GetComponent<Sta>();
            btn_ = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            sta_ = uiHolder.elementTrsLst[3].GetComponent<Sta>();
            img_ = uiHolder.elementTrsLst[4].GetComponent<Img>();
            txt_ = uiHolder.elementTrsLst[5].GetComponent<Txt>();
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

            public Btn btn_bbg;
            public Txt txt_externPath;
            public ScrView scr_bigItems;
            public Btn btn_;
            public Sta sta_;
            public Btn btn_close;
            public Btn btn_extern;
            public Sta sta_extern;
            public Btn btn_internal;
            public Sta sta_internal;
            public Btn btn_import;
            public Sta sta_import;
            public GameObject go_item;
            public Sta sta_item;
            public UiItemCtrl sub_item;
        public UiModAssetSelectWindowView(UiHolder uiHolder):base(uiHolder)
        {

            btn_bbg = uiHolder.elementTrsLst[0].GetComponent<Btn>();
            txt_externPath = uiHolder.elementTrsLst[1].GetComponent<Txt>();
            scr_bigItems = uiHolder.elementTrsLst[2].GetComponent<ScrView>();
            btn_ = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            sta_ = uiHolder.elementTrsLst[4].GetComponent<Sta>();
            btn_close = uiHolder.elementTrsLst[5].GetComponent<Btn>();
            btn_extern = uiHolder.elementTrsLst[6].GetComponent<Btn>();
            sta_extern = uiHolder.elementTrsLst[7].GetComponent<Sta>();
            btn_internal = uiHolder.elementTrsLst[8].GetComponent<Btn>();
            sta_internal = uiHolder.elementTrsLst[9].GetComponent<Sta>();
            btn_import = uiHolder.elementTrsLst[10].GetComponent<Btn>();
            sta_import = uiHolder.elementTrsLst[11].GetComponent<Sta>();
            go_item = uiHolder.elementTrsLst[12].gameObject;
            sta_item = uiHolder.elementTrsLst[13].GetComponent<Sta>();
            sub_item = (UiItemCtrl) uiHolder.elementTrsLst[14].GetComponent<UiHolder>().ctrl;
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


            view.sub_item = new UiItemCtrl();
            view.sub_item.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
        }

    }
    public partial class UiModAssetSelectWindowModel:UiModel
    {
        
    }
}
