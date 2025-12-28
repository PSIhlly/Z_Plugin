
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;
using Z_Texture;using RenderHeads.Media.AVProVideo;
namespace Ui.ModStoryEditorStyleWindow

{







    public partial class UiStyleParam:UiParam
    {
    }

    public partial class UiStyleView:UiView
    {

            public GameObject go_style;
            public Btn btn_;
            public Sta sta_;
            public Txt txt_;
        public UiStyleView(UiHolder uiHolder):base(uiHolder)
        {

            go_style = uiHolder.elementTrsLst[0].gameObject;
            btn_ = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            sta_ = uiHolder.elementTrsLst[2].GetComponent<Sta>();
            txt_ = uiHolder.elementTrsLst[3].GetComponent<Txt>();
        }

    }
    public partial class UiStyleCtrl:UiCtrl
    {
        public UiStyleView view;
        public UiStyleModel model;
        public UiStyleParam param;
        public UiModStoryEditorStyleWindowCtrl parent=>(UiModStoryEditorStyleWindowCtrl)uiHolder.parent.ctrl;

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
    public partial class UiModStoryEditorStyleWindowParam:UiParam
    {
    }

    public partial class UiModStoryEditorStyleWindowView:UiView
    {

            public Btn btn_bbg;
            public Txt txt_desc;
            public GameObject go_style;
            public UiStyleCtrl sub_style;
            public Btn btn_;
            public Sta sta_;
            public Btn btn_close;
        public UiModStoryEditorStyleWindowView(UiHolder uiHolder):base(uiHolder)
        {

            btn_bbg = uiHolder.elementTrsLst[0].GetComponent<Btn>();
            txt_desc = uiHolder.elementTrsLst[1].GetComponent<Txt>();
            go_style = uiHolder.elementTrsLst[2].gameObject;
            sub_style = (UiStyleCtrl) uiHolder.elementTrsLst[3].GetComponent<UiHolder>().ctrl;
            btn_ = uiHolder.elementTrsLst[4].GetComponent<Btn>();
            sta_ = uiHolder.elementTrsLst[5].GetComponent<Sta>();
            btn_close = uiHolder.elementTrsLst[6].GetComponent<Btn>();
        }

    }
    public partial class UiModStoryEditorStyleWindowCtrl:UiCtrl
    {
        public UiModStoryEditorStyleWindowView view;
        public UiModStoryEditorStyleWindowModel model;
        public UiModStoryEditorStyleWindowParam param;
        

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryEditorStyleWindowParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryEditorStyleWindowView(uiHolder);
            model=new UiModStoryEditorStyleWindowModel();


            view.sub_style = new UiStyleCtrl();
            view.sub_style.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
        }

    }
    public partial class UiModStoryEditorStyleWindowModel:UiModel
    {
        
    }
}
