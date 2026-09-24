
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;
using Z_Texture;using RenderHeads.Media.AVProVideo;
namespace Ui.ModStoryEventEditWindow

{










    public partial class UiDeepthParam:UiParam
    {
    }

    public partial class UiDeepthView:UiView
    {

            public GameObject go_deepth;
        public UiDeepthView(UiHolder uiHolder):base(uiHolder)
        {

            go_deepth = uiHolder.elementTrsLst[0].gameObject;
        }

    }
    public partial class UiDeepthCtrl:UiCtrl
    {
        public UiDeepthView view;
        public UiDeepthModel model;
        public UiDeepthParam param;
        public UiItemCtrl parent=>(UiItemCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiDeepthParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiDeepthView(uiHolder);
            model=new UiDeepthModel();


        }

    }
    public partial class UiDeepthModel:UiModel
    {
        
    }



    public partial class UiUnitParam:UiParam
    {
    }

    public partial class UiUnitView:UiView
    {

            public GameObject go_unit;
            public RectTransform rtf_unit;
            public Btn btn_;
            public Sta sta_;
            public GameObject go_image;
            public RectTransform rtf_root;
            public Img img_;
            public Txt txt_;
        public UiUnitView(UiHolder uiHolder):base(uiHolder)
        {

            go_unit = uiHolder.elementTrsLst[0].gameObject;
            rtf_unit = uiHolder.elementTrsLst[1].GetComponent<RectTransform>();
            btn_ = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            sta_ = uiHolder.elementTrsLst[3].GetComponent<Sta>();
            go_image = uiHolder.elementTrsLst[4].gameObject;
            rtf_root = uiHolder.elementTrsLst[5].GetComponent<RectTransform>();
            img_ = uiHolder.elementTrsLst[6].GetComponent<Img>();
            txt_ = uiHolder.elementTrsLst[7].GetComponent<Txt>();
        }

    }
    public partial class UiUnitCtrl:UiCtrl
    {
        public UiUnitView view;
        public UiUnitModel model;
        public UiUnitParam param;
        public UiItemCtrl parent=>(UiItemCtrl)uiHolder.parent.ctrl;

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
    public partial class UiItemParam:UiParam
    {
    }

    public partial class UiItemView:UiView
    {

            public GameObject go_item;
            public GameObject go_deepth;
            public UiDeepthCtrl sub_deepth;
            public Sta sta_isEmpty;
            public RectTransform rtf_unitRoot;
            public Txt txt_new;
            public GameObject go_unit;
            public RectTransform rtf_unit;
            public UiUnitCtrl sub_unit;
        public UiItemView(UiHolder uiHolder):base(uiHolder)
        {

            go_item = uiHolder.elementTrsLst[0].gameObject;
            go_deepth = uiHolder.elementTrsLst[1].gameObject;
            sub_deepth = (UiDeepthCtrl) uiHolder.elementTrsLst[2].GetComponent<UiHolder>().ctrl;
            sta_isEmpty = uiHolder.elementTrsLst[3].GetComponent<Sta>();
            rtf_unitRoot = uiHolder.elementTrsLst[4].GetComponent<RectTransform>();
            txt_new = uiHolder.elementTrsLst[5].GetComponent<Txt>();
            go_unit = uiHolder.elementTrsLst[6].gameObject;
            rtf_unit = uiHolder.elementTrsLst[7].GetComponent<RectTransform>();
            sub_unit = (UiUnitCtrl) uiHolder.elementTrsLst[8].GetComponent<UiHolder>().ctrl;
        }

    }
    public partial class UiItemCtrl:UiCtrl
    {
        public UiItemView view;
        public UiItemModel model;
        public UiItemParam param;
        public UiModStoryEventEditWindowCtrl parent=>(UiModStoryEventEditWindowCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiItemParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiItemView(uiHolder);
            model=new UiItemModel();


            view.sub_deepth = new UiDeepthCtrl();
            view.sub_deepth.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
            view.sub_unit = new UiUnitCtrl();
            view.sub_unit.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
        }

    }
    public partial class UiItemModel:UiModel
    {
        
    }



    public partial class UiLineParam:UiParam
    {
    }

    public partial class UiLineView:UiView
    {

            public GameObject go_line;
            public Sta sta_;
            public Btn btn_;
        public UiLineView(UiHolder uiHolder):base(uiHolder)
        {

            go_line = uiHolder.elementTrsLst[0].gameObject;
            sta_ = uiHolder.elementTrsLst[1].GetComponent<Sta>();
            btn_ = uiHolder.elementTrsLst[2].GetComponent<Btn>();
        }

    }
    public partial class UiLineCtrl:UiCtrl
    {
        public UiLineView view;
        public UiLineModel model;
        public UiLineParam param;
        public UiModStoryEventEditWindowCtrl parent=>(UiModStoryEventEditWindowCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiLineParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiLineView(uiHolder);
            model=new UiLineModel();


        }

    }
    public partial class UiLineModel:UiModel
    {
        
    }
    public partial class UiModStoryEventEditWindowParam:UiParam
    {
    }

    public partial class UiModStoryEventEditWindowView:UiView
    {

            public Sta sta_switchModPanel;
            public Txt txt_title;
            public GameObject go_close;
            public Btn btn_close;
            public GameObject go_items;
            public ScrView scr_items;
            public Btn btn_switchMod;
            public Sta sta_switchMod;
            public Btn btn_category;
            public Btn btn_type;
            public Ipt ipt_name;
            public Sta sta_item;
            public Btn btn_apply;
            public Txt txt_category;
            public Txt txt_type;
            public RectTransform rtf_itemRoot;
            public Ipt ipt_code;
            public Sta sta_unit;
            public GameObject go_item;
            public UiItemCtrl sub_item;
            public Btn btn_insert;
            public Btn btn_del;
            public GameObject go_line;
            public UiLineCtrl sub_line;
            public Btn btn_edit;
        public UiModStoryEventEditWindowView(UiHolder uiHolder):base(uiHolder)
        {

            sta_switchModPanel = uiHolder.elementTrsLst[0].GetComponent<Sta>();
            txt_title = uiHolder.elementTrsLst[1].GetComponent<Txt>();
            go_close = uiHolder.elementTrsLst[2].gameObject;
            btn_close = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            go_items = uiHolder.elementTrsLst[4].gameObject;
            scr_items = uiHolder.elementTrsLst[5].GetComponent<ScrView>();
            btn_switchMod = uiHolder.elementTrsLst[6].GetComponent<Btn>();
            sta_switchMod = uiHolder.elementTrsLst[7].GetComponent<Sta>();
            btn_category = uiHolder.elementTrsLst[8].GetComponent<Btn>();
            btn_type = uiHolder.elementTrsLst[9].GetComponent<Btn>();
            ipt_name = uiHolder.elementTrsLst[10].GetComponent<Ipt>();
            sta_item = uiHolder.elementTrsLst[11].GetComponent<Sta>();
            btn_apply = uiHolder.elementTrsLst[12].GetComponent<Btn>();
            txt_category = uiHolder.elementTrsLst[13].GetComponent<Txt>();
            txt_type = uiHolder.elementTrsLst[14].GetComponent<Txt>();
            rtf_itemRoot = uiHolder.elementTrsLst[15].GetComponent<RectTransform>();
            ipt_code = uiHolder.elementTrsLst[16].GetComponent<Ipt>();
            sta_unit = uiHolder.elementTrsLst[17].GetComponent<Sta>();
            go_item = uiHolder.elementTrsLst[18].gameObject;
            sub_item = (UiItemCtrl) uiHolder.elementTrsLst[19].GetComponent<UiHolder>().ctrl;
            btn_insert = uiHolder.elementTrsLst[20].GetComponent<Btn>();
            btn_del = uiHolder.elementTrsLst[21].GetComponent<Btn>();
            go_line = uiHolder.elementTrsLst[22].gameObject;
            sub_line = (UiLineCtrl) uiHolder.elementTrsLst[23].GetComponent<UiHolder>().ctrl;
            btn_edit = uiHolder.elementTrsLst[24].GetComponent<Btn>();
        }

    }
    public partial class UiModStoryEventEditWindowCtrl:UiCtrl
    {
        public UiModStoryEventEditWindowView view;
        public UiModStoryEventEditWindowModel model;
        public UiModStoryEventEditWindowParam param;
        

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryEventEditWindowParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryEventEditWindowView(uiHolder);
            model=new UiModStoryEventEditWindowModel();


            view.sub_item = new UiItemCtrl();
            view.sub_item.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
            view.sub_line = new UiLineCtrl();
            view.sub_line.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
        }

    }
    public partial class UiModStoryEventEditWindowModel:UiModel
    {
        
    }
}
