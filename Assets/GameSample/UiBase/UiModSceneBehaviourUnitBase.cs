
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
            public Img img_;
            public Txt txt_name;
            public Btn btn_close;
            public Btn btn_evt;
        public UiModSceneBehaviourUnitView(UiHolder uiHolder):base(uiHolder)
        {

            btn_bbg = uiHolder.elementTrsLst[0].GetComponent<Btn>();
            img_ = uiHolder.elementTrsLst[1].GetComponent<Img>();
            txt_name = uiHolder.elementTrsLst[2].GetComponent<Txt>();
            btn_close = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            btn_evt = uiHolder.elementTrsLst[4].GetComponent<Btn>();
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
