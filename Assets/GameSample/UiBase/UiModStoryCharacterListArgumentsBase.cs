
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;

namespace Ui.ModStoryCharacterListArguments

{



    public partial class UiUnitParam:UiParam
    {
    }

    public partial class UiUnitView:UiView
    {

            public GameObject go_unit;
            public Sta sta_unit;
            public Txt txt_name;
            public Txt txt_default;
            public Txt txt_min;
            public Txt txt_max;
            public Ipt ipt_default;
            public Ipt ipt_min;
            public Ipt ipt_max;
        public UiUnitView(UiHolder uiHolder):base(uiHolder)
        {

            go_unit = uiHolder.elementTrsLst[0].gameObject;
            sta_unit = uiHolder.elementTrsLst[1].GetComponent<Sta>();
            txt_name = uiHolder.elementTrsLst[2].GetComponent<Txt>();
            txt_default = uiHolder.elementTrsLst[3].GetComponent<Txt>();
            txt_min = uiHolder.elementTrsLst[4].GetComponent<Txt>();
            txt_max = uiHolder.elementTrsLst[5].GetComponent<Txt>();
            ipt_default = uiHolder.elementTrsLst[6].GetComponent<Ipt>();
            ipt_min = uiHolder.elementTrsLst[7].GetComponent<Ipt>();
            ipt_max = uiHolder.elementTrsLst[8].GetComponent<Ipt>();
        }

    }
    public partial class UiUnitCtrl:UiCtrl
    {
        public UiUnitView view;
        public UiUnitModel model;
        public UiUnitParam param;
        public UiModStoryCharacterListArgumentsCtrl parent=>(UiModStoryCharacterListArgumentsCtrl)uiHolder.parent.ctrl;

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
    public partial class UiModStoryCharacterListArgumentsParam:UiParam
    {
    }

    public partial class UiModStoryCharacterListArgumentsView:UiView
    {

            public Btn btn_back;
            public GameObject go_units;
            public ScrView scr_units;
            public GameObject go_unit;
            public Sta sta_unit;
            public UiUnitCtrl sub_Unit;
        public UiModStoryCharacterListArgumentsView(UiHolder uiHolder):base(uiHolder)
        {

            btn_back = uiHolder.elementTrsLst[0].GetComponent<Btn>();
            go_units = uiHolder.elementTrsLst[1].gameObject;
            scr_units = uiHolder.elementTrsLst[2].GetComponent<ScrView>();
            go_unit = uiHolder.elementTrsLst[3].gameObject;
            sta_unit = uiHolder.elementTrsLst[4].GetComponent<Sta>();
            sub_Unit = (UiUnitCtrl) uiHolder.elementTrsLst[5].GetComponent<UiHolder>().ctrl;
        }

    }
    public partial class UiModStoryCharacterListArgumentsCtrl:UiCtrl
    {
        public UiModStoryCharacterListArgumentsView view;
        public UiModStoryCharacterListArgumentsModel model;
        public UiModStoryCharacterListArgumentsParam param;
        

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryCharacterListArgumentsParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryCharacterListArgumentsView(uiHolder);
            model=new UiModStoryCharacterListArgumentsModel();


            view.sub_Unit = new UiUnitCtrl();
            view.sub_Unit.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
        }

    }
    public partial class UiModStoryCharacterListArgumentsModel:UiModel
    {
        
    }
}
