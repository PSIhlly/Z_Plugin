
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;
namespace Ui.ModSceneUnit

{




    public partial class UiModSceneUnitParam:UiParam
    {
    }

    public partial class UiModSceneUnitView:UiView
    {

            public Btn btn_bbg;
            public Txt txt_name;
            public Btn btn_close;
            public Btn btn_aligh;
            public Btn btn_delete;
            public Ipt ipt_rotateSet;
            public Ipt ipt_posSetX;
            public Ipt ipt_posSetZ;
            public Ipt ipt_posSetY;
        public UiModSceneUnitView(UiHolder uiHolder):base(uiHolder)
        {

            btn_bbg = uiHolder.elementTrsLst[0].GetComponent<Btn>();
            txt_name = uiHolder.elementTrsLst[1].GetComponent<Txt>();
            btn_close = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            btn_aligh = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            btn_delete = uiHolder.elementTrsLst[4].GetComponent<Btn>();
            ipt_rotateSet = uiHolder.elementTrsLst[5].GetComponent<Ipt>();
            ipt_posSetX = uiHolder.elementTrsLst[6].GetComponent<Ipt>();
            ipt_posSetZ = uiHolder.elementTrsLst[7].GetComponent<Ipt>();
            ipt_posSetY = uiHolder.elementTrsLst[8].GetComponent<Ipt>();
        }

    }
    public partial class UiModSceneUnitCtrl:UiCtrl
    {
        public UiModSceneUnitView view;
        public UiModSceneUnitModel model;
        public UiModSceneUnitParam param;
        

        public override void SetParam(UiParam param)
        {
            this.param = (UiModSceneUnitParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModSceneUnitView(uiHolder);
            model=new UiModSceneUnitModel();


        }

    }
    public partial class UiModSceneUnitModel:UiModel
    {
        
    }
}
