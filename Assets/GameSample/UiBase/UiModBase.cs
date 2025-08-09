
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;
namespace Ui.Mod

{







    public partial class UiStoryItemParam:UiParam
    {
    }

    public partial class UiStoryItemView:UiView
    {

            public GameObject go_storyItem;
            public Btn btn_mod;
            public Sta sta_;
            public Img img_;
            public Txt txt_;
        public UiStoryItemView(UiHolder uiHolder):base(uiHolder)
        {

            go_storyItem = uiHolder.elementTrsLst[0].gameObject;
            btn_mod = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            sta_ = uiHolder.elementTrsLst[2].GetComponent<Sta>();
            img_ = uiHolder.elementTrsLst[3].GetComponent<Img>();
            txt_ = uiHolder.elementTrsLst[4].GetComponent<Txt>();
        }

    }
    public partial class UiStoryItemCtrl:UiCtrl
    {
        public UiStoryItemView view;
        public UiStoryItemModel model;
        public UiStoryItemParam param;
        public UiModCtrl parent=>(UiModCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiStoryItemParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiStoryItemView(uiHolder);
            model=new UiStoryItemModel();


        }

    }
    public partial class UiStoryItemModel:UiModel
    {
        
    }
    public partial class UiModParam:UiParam
    {
    }

    public partial class UiModView:UiView
    {

            public Btn btn_back;
            public Txt txt_title;
            public ScrView scr_tt;
            public GameObject go_storyItem;
            public UiStoryItemCtrl sub_StoryItem;
        public UiModView(UiHolder uiHolder):base(uiHolder)
        {

            btn_back = uiHolder.elementTrsLst[0].GetComponent<Btn>();
            txt_title = uiHolder.elementTrsLst[1].GetComponent<Txt>();
            scr_tt = uiHolder.elementTrsLst[2].GetComponent<ScrView>();
            go_storyItem = uiHolder.elementTrsLst[3].gameObject;
            sub_StoryItem = (UiStoryItemCtrl) uiHolder.elementTrsLst[4].GetComponent<UiHolder>().ctrl;
        }

    }
    public partial class UiModCtrl:UiCtrl
    {
        public UiModView view;
        public UiModModel model;
        public UiModParam param;
        

        public override void SetParam(UiParam param)
        {
            this.param = (UiModParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModView(uiHolder);
            model=new UiModModel();


            view.sub_StoryItem = new UiStoryItemCtrl();
            view.sub_StoryItem.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
        }

    }
    public partial class UiModModel:UiModel
    {
        
    }
}
