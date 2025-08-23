
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;
namespace Ui.ModStoryEventEditWindow

{







    public partial class UiItemParam:UiParam
    {
    }

    public partial class UiItemView:UiView
    {

            public GameObject go_item;
            public Btn btn_;
            public Sta sta_;
            public Sta sta_isEmpty;
            public Txt txt_;
        public UiItemView(UiHolder uiHolder):base(uiHolder)
        {

            go_item = uiHolder.elementTrsLst[0].gameObject;
            btn_ = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            sta_ = uiHolder.elementTrsLst[2].GetComponent<Sta>();
            sta_isEmpty = uiHolder.elementTrsLst[3].GetComponent<Sta>();
            txt_ = uiHolder.elementTrsLst[4].GetComponent<Txt>();
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
            public RectTransform rtf_unit;
            public Btn btn_;
            public Sta sta_;
            public Img img_;
            public RectTransform rtf_root;
            public Txt txt_;
        public UiUnitView(UiHolder uiHolder):base(uiHolder)
        {

            go_unit = uiHolder.elementTrsLst[0].gameObject;
            rtf_unit = uiHolder.elementTrsLst[1].GetComponent<RectTransform>();
            btn_ = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            sta_ = uiHolder.elementTrsLst[3].GetComponent<Sta>();
            img_ = uiHolder.elementTrsLst[4].GetComponent<Img>();
            rtf_root = uiHolder.elementTrsLst[5].GetComponent<RectTransform>();
            txt_ = uiHolder.elementTrsLst[6].GetComponent<Txt>();
        }

    }
    public partial class UiUnitCtrl:UiCtrl
    {
        public UiUnitView view;
        public UiUnitModel model;
        public UiUnitParam param;
        public UiModStoryEventEditWindowCtrl parent=>(UiModStoryEventEditWindowCtrl)uiHolder.parent.ctrl;

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
    public partial class UiModStoryEventEditWindowParam:UiParam
    {
    }

    public partial class UiModStoryEventEditWindowView:UiView
    {

            public Sta sta_switchModPanel;
            public Txt txt_title;
            public GameObject go_close;
            public Btn btn_close;
            public Btn btn_apply;
            public GameObject go_items;
            public ScrView scr_items;
            public Ipt ipt_code;
            public Btn btn_switchMod;
            public Sta sta_switchMod;
            public Ipt ipt_name;
            public Ipt ipt_category;
            public Ipt ipt_type;
            public RectTransform rtf_itemRoot;
            public RectTransform rtf_unitRoot;
            public GameObject go_item;
            public UiItemCtrl sub_Item;
            public GameObject go_unit;
            public RectTransform rtf_unit;
            public UiUnitCtrl sub_Unit;
        public UiModStoryEventEditWindowView(UiHolder uiHolder):base(uiHolder)
        {

            sta_switchModPanel = uiHolder.elementTrsLst[0].GetComponent<Sta>();
            txt_title = uiHolder.elementTrsLst[1].GetComponent<Txt>();
            go_close = uiHolder.elementTrsLst[2].gameObject;
            btn_close = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            btn_apply = uiHolder.elementTrsLst[4].GetComponent<Btn>();
            go_items = uiHolder.elementTrsLst[5].gameObject;
            scr_items = uiHolder.elementTrsLst[6].GetComponent<ScrView>();
            ipt_code = uiHolder.elementTrsLst[7].GetComponent<Ipt>();
            btn_switchMod = uiHolder.elementTrsLst[8].GetComponent<Btn>();
            sta_switchMod = uiHolder.elementTrsLst[9].GetComponent<Sta>();
            ipt_name = uiHolder.elementTrsLst[10].GetComponent<Ipt>();
            ipt_category = uiHolder.elementTrsLst[11].GetComponent<Ipt>();
            ipt_type = uiHolder.elementTrsLst[12].GetComponent<Ipt>();
            rtf_itemRoot = uiHolder.elementTrsLst[13].GetComponent<RectTransform>();
            rtf_unitRoot = uiHolder.elementTrsLst[14].GetComponent<RectTransform>();
            go_item = uiHolder.elementTrsLst[15].gameObject;
            sub_Item = (UiItemCtrl) uiHolder.elementTrsLst[16].GetComponent<UiHolder>().ctrl;
            go_unit = uiHolder.elementTrsLst[17].gameObject;
            rtf_unit = uiHolder.elementTrsLst[18].GetComponent<RectTransform>();
            sub_Unit = (UiUnitCtrl) uiHolder.elementTrsLst[19].GetComponent<UiHolder>().ctrl;
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


            view.sub_Item = new UiItemCtrl();
            view.sub_Item.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
            view.sub_Unit = new UiUnitCtrl();
            view.sub_Unit.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
        }

    }
    public partial class UiModStoryEventEditWindowModel:UiModel
    {
        
    }
}
