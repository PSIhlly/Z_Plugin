
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;

namespace Ui.ModSceneBehaviourUnit

{


    public partial class UiModSceneBehaviourUnitParam:UiParam
    {
    }

    public partial class UiModSceneBehaviourUnitView:UiView
    {

            public Btn btn_bbg;
            public Txt txt_name;
            public Btn btn_close;
            public Btn btn_evt;
            public Img img_close;
            public Txt txt_close;
            public Img img_evt;
            public Txt txt_evt;
        public UiModSceneBehaviourUnitView(UiHolder uiHolder):base(uiHolder)
        {

            btn_bbg = uiHolder.elementTrsLst[0].GetComponent<Btn>();
            txt_name = uiHolder.elementTrsLst[1].GetComponent<Txt>();
            btn_close = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            btn_evt = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            img_close = uiHolder.elementTrsLst[4].GetComponent<Img>();
            txt_close = uiHolder.elementTrsLst[5].GetComponent<Txt>();
            img_evt = uiHolder.elementTrsLst[6].GetComponent<Img>();
            txt_evt = uiHolder.elementTrsLst[7].GetComponent<Txt>();
        }

    }
    public partial class UiModSceneBehaviourUnitCtrl:UiCtrl
    {
        public UiModSceneBehaviourUnitView view;
        public UiModSceneBehaviourUnitModel model;
        public UiModSceneBehaviourUnitParam param;
        

        public override void SetParam(UiParam param)
        {
            this.param = (UiModSceneBehaviourUnitParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModSceneBehaviourUnitView(uiHolder);
            model=new UiModSceneBehaviourUnitModel();


        }

    }
    public partial class UiModSceneBehaviourUnitModel:UiModel
    {
        
    }
}
