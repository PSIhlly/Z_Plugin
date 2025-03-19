
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;

namespace Ui.ModStoryObject
{



    public partial class UiItemParam:UiParam
    {
    }

    public partial class UiItemView:UiView
    {

            public GameObject go_Item;
            public Btn btn_item;
            public Sta sta_item;
            public Img img_;
            public Txt txt_name;
        public UiItemView(UiHolder uiHolder):base(uiHolder)
        {

            go_Item = uiHolder.elementTrsLst[0].gameObject;
            btn_item = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            sta_item = uiHolder.elementTrsLst[2].GetComponent<Sta>();
            img_ = uiHolder.elementTrsLst[3].GetComponent<Img>();
            txt_name = uiHolder.elementTrsLst[4].GetComponent<Txt>();
        }

    }
    public partial class UiItemCtrl:UiCtrl
    {
        public UiItemView view;
        public UiItemModel model;
        public UiItemParam param;
        public UiModStoryObjectCtrl parent=>(UiModStoryObjectCtrl)uiHolder.parent.ctrl;

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
            public Sta sta_unit;
            public Btn btn_new;
            public Btn btn_tex;
            public Btn btn_prefab;
            public Txt txt_pos;
            public Txt txt_scale;
            public Btn btn_delete;
            public Img img_tex;
            public Img img_;
            public Txt txt_name;
            public Ipt ipt_posX;
            public Ipt ipt_posY;
            public Ipt ipt_posZ;
            public Ipt ipt_scaleX;
            public Ipt ipt_scaleY;
            public Ipt ipt_scaleZ;
        public UiUnitView(UiHolder uiHolder):base(uiHolder)
        {

            go_unit = uiHolder.elementTrsLst[0].gameObject;
            sta_unit = uiHolder.elementTrsLst[1].GetComponent<Sta>();
            btn_new = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            btn_tex = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            btn_prefab = uiHolder.elementTrsLst[4].GetComponent<Btn>();
            txt_pos = uiHolder.elementTrsLst[5].GetComponent<Txt>();
            txt_scale = uiHolder.elementTrsLst[6].GetComponent<Txt>();
            btn_delete = uiHolder.elementTrsLst[7].GetComponent<Btn>();
            img_tex = uiHolder.elementTrsLst[8].GetComponent<Img>();
            img_ = uiHolder.elementTrsLst[9].GetComponent<Img>();
            txt_name = uiHolder.elementTrsLst[10].GetComponent<Txt>();
            ipt_posX = uiHolder.elementTrsLst[11].GetComponent<Ipt>();
            ipt_posY = uiHolder.elementTrsLst[12].GetComponent<Ipt>();
            ipt_posZ = uiHolder.elementTrsLst[13].GetComponent<Ipt>();
            ipt_scaleX = uiHolder.elementTrsLst[14].GetComponent<Ipt>();
            ipt_scaleY = uiHolder.elementTrsLst[15].GetComponent<Ipt>();
            ipt_scaleZ = uiHolder.elementTrsLst[16].GetComponent<Ipt>();
        }

    }
    public partial class UiUnitCtrl:UiCtrl
    {
        public UiUnitView view;
        public UiUnitModel model;
        public UiUnitParam param;
        public UiModStoryObjectCtrl parent=>(UiModStoryObjectCtrl)uiHolder.parent.ctrl;

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
    public partial class UiModStoryObjectParam:UiParam
    {
    }

    public partial class UiModStoryObjectView:UiView
    {

            public Btn btn_back;
            public Btn btn_item;
            public Sta sta_item;
            public Sta sta_exist;
            public Img img_texture;
            public Txt txt_texture;
            public GameObject go_items;
            public ScrView scr_items;
            public Btn btn_delete;
            public Ipt ipt_name;
            public GameObject go_Item;
            public UiItemCtrl sub_Item;
            public Txt txt_delete;
            public Txt txt_name;
            public GameObject go_units;
            public ScrView scr_units;
            public GameObject go_unit;
            public Sta sta_unit;
            public UiUnitCtrl sub_Unit;
        public UiModStoryObjectView(UiHolder uiHolder):base(uiHolder)
        {

            btn_back = uiHolder.elementTrsLst[0].GetComponent<Btn>();
            btn_item = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            sta_item = uiHolder.elementTrsLst[2].GetComponent<Sta>();
            sta_exist = uiHolder.elementTrsLst[3].GetComponent<Sta>();
            img_texture = uiHolder.elementTrsLst[4].GetComponent<Img>();
            txt_texture = uiHolder.elementTrsLst[5].GetComponent<Txt>();
            go_items = uiHolder.elementTrsLst[6].gameObject;
            scr_items = uiHolder.elementTrsLst[7].GetComponent<ScrView>();
            btn_delete = uiHolder.elementTrsLst[8].GetComponent<Btn>();
            ipt_name = uiHolder.elementTrsLst[9].GetComponent<Ipt>();
            go_Item = uiHolder.elementTrsLst[10].gameObject;
            sub_Item = (UiItemCtrl) uiHolder.elementTrsLst[11].GetComponent<UiHolder>().ctrl;
            txt_delete = uiHolder.elementTrsLst[12].GetComponent<Txt>();
            txt_name = uiHolder.elementTrsLst[13].GetComponent<Txt>();
            go_units = uiHolder.elementTrsLst[14].gameObject;
            scr_units = uiHolder.elementTrsLst[15].GetComponent<ScrView>();
            go_unit = uiHolder.elementTrsLst[16].gameObject;
            sta_unit = uiHolder.elementTrsLst[17].GetComponent<Sta>();
            sub_Unit = (UiUnitCtrl) uiHolder.elementTrsLst[18].GetComponent<UiHolder>().ctrl;
        }

    }
    public partial class UiModStoryObjectCtrl:UiCtrl
    {
        public UiModStoryObjectView view;
        public UiModStoryObjectModel model;
        public UiModStoryObjectParam param;
        

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryObjectParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryObjectView(uiHolder);
            model=new UiModStoryObjectModel();


            view.sub_Item = new UiItemCtrl();
            view.sub_Item.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
            view.sub_Unit = new UiUnitCtrl();
            view.sub_Unit.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
        }

    }
    public partial class UiModStoryObjectModel:UiModel
    {
        
    }
}
