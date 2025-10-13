
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;
namespace Ui.ZCodeEntry

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
        public UiZCodeEntryCtrl parent=>(UiZCodeEntryCtrl)uiHolder.parent.ctrl;

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
        public UiZCodeEntryCtrl parent=>(UiZCodeEntryCtrl)uiHolder.parent.ctrl;

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
    public partial class UiZCodeEntryParam:UiParam
    {
    }

    public partial class UiZCodeEntryView:UiView
    {

            public GameObject go_items;
            public ScrView scr_items;
            public Btn btn_run;
            public Btn btn_toEntry;
            public Btn btn_toCode;
            public Ipt ipt_code;
            public RectTransform rtf_itemRoot;
            public GameObject go_item;
            public UiItemCtrl sub_Item;
            public RectTransform rtf_unitRoot;
            public GameObject go_unit;
            public RectTransform rtf_unit;
            public UiUnitCtrl sub_Unit;
        public UiZCodeEntryView(UiHolder uiHolder):base(uiHolder)
        {

            go_items = uiHolder.elementTrsLst[0].gameObject;
            scr_items = uiHolder.elementTrsLst[1].GetComponent<ScrView>();
            btn_run = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            btn_toEntry = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            btn_toCode = uiHolder.elementTrsLst[4].GetComponent<Btn>();
            ipt_code = uiHolder.elementTrsLst[5].GetComponent<Ipt>();
            rtf_itemRoot = uiHolder.elementTrsLst[6].GetComponent<RectTransform>();
            go_item = uiHolder.elementTrsLst[7].gameObject;
            sub_Item = (UiItemCtrl) uiHolder.elementTrsLst[8].GetComponent<UiHolder>().ctrl;
            rtf_unitRoot = uiHolder.elementTrsLst[9].GetComponent<RectTransform>();
            go_unit = uiHolder.elementTrsLst[10].gameObject;
            rtf_unit = uiHolder.elementTrsLst[11].GetComponent<RectTransform>();
            sub_Unit = (UiUnitCtrl) uiHolder.elementTrsLst[12].GetComponent<UiHolder>().ctrl;
        }

    }
    public partial class UiZCodeEntryCtrl:UiCtrl
    {
        public UiZCodeEntryView view;
        public UiZCodeEntryModel model;
        public UiZCodeEntryParam param;
        

        public override void SetParam(UiParam param)
        {
            this.param = (UiZCodeEntryParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiZCodeEntryView(uiHolder);
            model=new UiZCodeEntryModel();


            view.sub_Item = new UiItemCtrl();
            view.sub_Item.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
            view.sub_Unit = new UiUnitCtrl();
            view.sub_Unit.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
        }

    }
    public partial class UiZCodeEntryModel:UiModel
    {
        
    }
}
