
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;

namespace Ui.ModStoryCharacter

{


namespace ModStoryCharacterArguments

{



    public partial class UiArgParam:UiParam
    {
    }

    public partial class UiArgView:UiView
    {

            public GameObject go_arg;
            public Sta sta_arg;
            public Btn btn_new;
            public Ipt ipt_name;
            public Btn btn_delete;
            public Img img_;
            public Txt txt_new;
            public Txt txt_name;
            public Img img_delete;
            public Txt txt_delete;
        public UiArgView(UiHolder uiHolder):base(uiHolder)
        {

            go_arg = uiHolder.elementTrsLst[0].gameObject;
            sta_arg = uiHolder.elementTrsLst[1].GetComponent<Sta>();
            btn_new = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            ipt_name = uiHolder.elementTrsLst[3].GetComponent<Ipt>();
            btn_delete = uiHolder.elementTrsLst[4].GetComponent<Btn>();
            img_ = uiHolder.elementTrsLst[5].GetComponent<Img>();
            txt_new = uiHolder.elementTrsLst[6].GetComponent<Txt>();
            txt_name = uiHolder.elementTrsLst[7].GetComponent<Txt>();
            img_delete = uiHolder.elementTrsLst[8].GetComponent<Img>();
            txt_delete = uiHolder.elementTrsLst[9].GetComponent<Txt>();
        }

    }
    public partial class UiArgCtrl:UiCtrl
    {
        public UiArgView view;
        public UiArgModel model;
        public UiArgParam param;
        public UiModStoryCharacterArgumentsCtrl parent=>(UiModStoryCharacterArgumentsCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiArgParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiArgView(uiHolder);
            model=new UiArgModel();


        }

    }
    public partial class UiArgModel:UiModel
    {
        
    }
    public partial class UiModStoryCharacterArgumentsParam:UiParam
    {
    }

    public partial class UiModStoryCharacterArgumentsView:UiView
    {

            public GameObject go_items;
            public ScrView scr_items;
            public GameObject go_arg;
            public Sta sta_arg;
            public UiArgCtrl sub_Arg;
        public UiModStoryCharacterArgumentsView(UiHolder uiHolder):base(uiHolder)
        {

            go_items = uiHolder.elementTrsLst[0].gameObject;
            scr_items = uiHolder.elementTrsLst[1].GetComponent<ScrView>();
            go_arg = uiHolder.elementTrsLst[2].gameObject;
            sta_arg = uiHolder.elementTrsLst[3].GetComponent<Sta>();
            sub_Arg = (UiArgCtrl) uiHolder.elementTrsLst[4].GetComponent<UiHolder>().ctrl;
        }

    }
    public partial class UiModStoryCharacterArgumentsCtrl:UiCtrl
    {
        public UiModStoryCharacterArgumentsView view;
        public UiModStoryCharacterArgumentsModel model;
        public UiModStoryCharacterArgumentsParam param;
        public UiModStoryCharacterCtrl parent=>(UiModStoryCharacterCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryCharacterArgumentsParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryCharacterArgumentsView(uiHolder);
            model=new UiModStoryCharacterArgumentsModel();


            view.sub_Arg = new UiArgCtrl();
            view.sub_Arg.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
        }

    }
    public partial class UiModStoryCharacterArgumentsModel:UiModel
    {
        
    }
}

namespace ModStoryCharacterList

{



    public partial class UiItemParam:UiParam
    {
    }

    public partial class UiItemView:UiView
    {

            public GameObject go_item;
            public Btn btn_item;
            public Sta sta_item;
            public Img img_;
            public Txt txt_name;
        public UiItemView(UiHolder uiHolder):base(uiHolder)
        {

            go_item = uiHolder.elementTrsLst[0].gameObject;
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
        public UiModStoryCharacterListCtrl parent=>(UiModStoryCharacterListCtrl)uiHolder.parent.ctrl;

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
    public partial class UiModStoryCharacterListParam:UiParam
    {
    }

    public partial class UiModStoryCharacterListView:UiView
    {

            public Sta sta_exist;
            public GameObject go_items;
            public ScrView scr_items;
            public Btn btn_avatar;
            public Btn btn_delete;
            public Ipt ipt_name;
            public Btn btn_args;
            public Btn btn_model;
            public GameObject go_item;
            public UiItemCtrl sub_Item;
            public Img img_avatar;
            public Txt txt_avatar;
            public Txt txt_delete;
            public Txt txt_name;
            public Img img_args;
            public Txt txt_args;
            public Img img_model;
            public Txt txt_model;
        public UiModStoryCharacterListView(UiHolder uiHolder):base(uiHolder)
        {

            sta_exist = uiHolder.elementTrsLst[0].GetComponent<Sta>();
            go_items = uiHolder.elementTrsLst[1].gameObject;
            scr_items = uiHolder.elementTrsLst[2].GetComponent<ScrView>();
            btn_avatar = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            btn_delete = uiHolder.elementTrsLst[4].GetComponent<Btn>();
            ipt_name = uiHolder.elementTrsLst[5].GetComponent<Ipt>();
            btn_args = uiHolder.elementTrsLst[6].GetComponent<Btn>();
            btn_model = uiHolder.elementTrsLst[7].GetComponent<Btn>();
            go_item = uiHolder.elementTrsLst[8].gameObject;
            sub_Item = (UiItemCtrl) uiHolder.elementTrsLst[9].GetComponent<UiHolder>().ctrl;
            img_avatar = uiHolder.elementTrsLst[10].GetComponent<Img>();
            txt_avatar = uiHolder.elementTrsLst[11].GetComponent<Txt>();
            txt_delete = uiHolder.elementTrsLst[12].GetComponent<Txt>();
            txt_name = uiHolder.elementTrsLst[13].GetComponent<Txt>();
            img_args = uiHolder.elementTrsLst[14].GetComponent<Img>();
            txt_args = uiHolder.elementTrsLst[15].GetComponent<Txt>();
            img_model = uiHolder.elementTrsLst[16].GetComponent<Img>();
            txt_model = uiHolder.elementTrsLst[17].GetComponent<Txt>();
        }

    }
    public partial class UiModStoryCharacterListCtrl:UiCtrl
    {
        public UiModStoryCharacterListView view;
        public UiModStoryCharacterListModel model;
        public UiModStoryCharacterListParam param;
        public UiModStoryCharacterCtrl parent=>(UiModStoryCharacterCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryCharacterListParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryCharacterListView(uiHolder);
            model=new UiModStoryCharacterListModel();


            view.sub_Item = new UiItemCtrl();
            view.sub_Item.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
        }

    }
    public partial class UiModStoryCharacterListModel:UiModel
    {
        
    }
}

    public partial class UiModStoryCharacterParam:UiParam
    {
    }

    public partial class UiModStoryCharacterView:UiView
    {

            public ModStoryCharacterArguments.UiModStoryCharacterArgumentsCtrl page_ModStoryCharacterArguments;
            public ModStoryCharacterList.UiModStoryCharacterListCtrl page_ModStoryCharacterList;
            public Btn btn_back;
            public Btn btn_args;
            public Sta sta_args;
            public Btn btn_list;
            public Sta sta_list;
            public Img img_args;
            public Txt txt_args;
            public Img img_list;
            public Txt txt_list;
        public UiModStoryCharacterView(UiHolder uiHolder):base(uiHolder)
        {

            page_ModStoryCharacterArguments = (ModStoryCharacterArguments.UiModStoryCharacterArgumentsCtrl) uiHolder.elementTrsLst[0].GetComponent<UiHolder>().ctrl;
            page_ModStoryCharacterList = (ModStoryCharacterList.UiModStoryCharacterListCtrl) uiHolder.elementTrsLst[1].GetComponent<UiHolder>().ctrl;
            btn_back = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            btn_args = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            sta_args = uiHolder.elementTrsLst[4].GetComponent<Sta>();
            btn_list = uiHolder.elementTrsLst[5].GetComponent<Btn>();
            sta_list = uiHolder.elementTrsLst[6].GetComponent<Sta>();
            img_args = uiHolder.elementTrsLst[7].GetComponent<Img>();
            txt_args = uiHolder.elementTrsLst[8].GetComponent<Txt>();
            img_list = uiHolder.elementTrsLst[9].GetComponent<Img>();
            txt_list = uiHolder.elementTrsLst[10].GetComponent<Txt>();
        }

    }
    public partial class UiModStoryCharacterCtrl:UiCtrl
    {
        public UiModStoryCharacterView view;
        public UiModStoryCharacterModel model;
        public UiModStoryCharacterParam param;
        

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryCharacterParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryCharacterView(uiHolder);
            model=new UiModStoryCharacterModel();


            view.page_ModStoryCharacterArguments = new ModStoryCharacterArguments.UiModStoryCharacterArgumentsCtrl();
            view.page_ModStoryCharacterArguments.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
            view.page_ModStoryCharacterList = new ModStoryCharacterList.UiModStoryCharacterListCtrl();
            view.page_ModStoryCharacterList.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
        }

    }
    public partial class UiModStoryCharacterModel:UiModel
    {
        
    }
}
