
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;
using Z_Texture;using RenderHeads.Media.AVProVideo;
namespace Ui.Story

{







    public partial class UiItemParam:UiParam
    {
    }

    public partial class UiItemView:UiView
    {

            public GameObject go_item;
            public Sta sta_item;
            public Sta sta_exist;
            public Btn btn_new;
            public Btn btn_;
            public Sta sta_;
            public Img img_;
            public Txt txt_;
        public UiItemView(UiHolder uiHolder):base(uiHolder)
        {

            go_item = uiHolder.elementTrsLst[0].gameObject;
            sta_item = uiHolder.elementTrsLst[1].GetComponent<Sta>();
            sta_exist = uiHolder.elementTrsLst[2].GetComponent<Sta>();
            btn_new = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            btn_ = uiHolder.elementTrsLst[4].GetComponent<Btn>();
            sta_ = uiHolder.elementTrsLst[5].GetComponent<Sta>();
            img_ = uiHolder.elementTrsLst[6].GetComponent<Img>();
            txt_ = uiHolder.elementTrsLst[7].GetComponent<Txt>();
        }

    }
    public partial class UiItemCtrl:UiCtrl
    {
        public UiItemView view;
        public UiItemModel model;
        public UiItemParam param;
        public UiStoryCtrl parent=>(UiStoryCtrl)uiHolder.parent.ctrl;

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
    public partial class UiStoryParam:UiParam
    {
    }

    public partial class UiStoryView:UiView
    {

            public Btn btn_back;
            public Txt txt_title;
            public ScrView scr_items;
            public GameObject go_item;
            public Sta sta_item;
            public UiItemCtrl sub_item;
        public UiStoryView(UiHolder uiHolder):base(uiHolder)
        {

            btn_back = uiHolder.elementTrsLst[0].GetComponent<Btn>();
            txt_title = uiHolder.elementTrsLst[1].GetComponent<Txt>();
            scr_items = uiHolder.elementTrsLst[2].GetComponent<ScrView>();
            go_item = uiHolder.elementTrsLst[3].gameObject;
            sta_item = uiHolder.elementTrsLst[4].GetComponent<Sta>();
            sub_item = (UiItemCtrl) uiHolder.elementTrsLst[5].GetComponent<UiHolder>().ctrl;
        }

    }
    public partial class UiStoryCtrl:UiCtrl
    {
        public UiStoryView view;
        public UiStoryModel model;
        public UiStoryParam param;
        

        public override void SetParam(UiParam param)
        {
            this.param = (UiStoryParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiStoryView(uiHolder);
            model=new UiStoryModel();


            view.sub_item = new UiItemCtrl();
            view.sub_item.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
        }

    }
    public partial class UiStoryModel:UiModel
    {
        
    }
}
