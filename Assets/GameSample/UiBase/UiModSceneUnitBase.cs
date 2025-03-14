
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
            public Btn btn_delete;
            public Txt txt_rotateSet;
            public Txt txt_posSet;
            public Btn btn_pos;
            public Img img_close;
            public Txt txt_close;
            public Img img_exit;
            public Txt txt_exit;
            public Ipt ipt_rotateSet;
            public Ipt ipt_posSetX;
            public Ipt ipt_posSetZ;
            public Ipt ipt_posSetY;
            public Img img_pos;
            public Sta sta_pos;
            public Txt txt_pos;
        public UiModSceneUnitView(UiHolder uiHolder):base(uiHolder)
        {

            btn_bbg = uiHolder.elementTrsLst[0].GetComponent<Btn>();
            txt_name = uiHolder.elementTrsLst[1].GetComponent<Txt>();
            btn_close = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            btn_delete = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            txt_rotateSet = uiHolder.elementTrsLst[4].GetComponent<Txt>();
            txt_posSet = uiHolder.elementTrsLst[5].GetComponent<Txt>();
            btn_pos = uiHolder.elementTrsLst[6].GetComponent<Btn>();
            img_close = uiHolder.elementTrsLst[7].GetComponent<Img>();
            txt_close = uiHolder.elementTrsLst[8].GetComponent<Txt>();
            img_exit = uiHolder.elementTrsLst[9].GetComponent<Img>();
            txt_exit = uiHolder.elementTrsLst[10].GetComponent<Txt>();
            ipt_rotateSet = uiHolder.elementTrsLst[11].GetComponent<Ipt>();
            ipt_posSetX = uiHolder.elementTrsLst[12].GetComponent<Ipt>();
            ipt_posSetZ = uiHolder.elementTrsLst[13].GetComponent<Ipt>();
            ipt_posSetY = uiHolder.elementTrsLst[14].GetComponent<Ipt>();
            img_pos = uiHolder.elementTrsLst[15].GetComponent<Img>();
            sta_pos = uiHolder.elementTrsLst[16].GetComponent<Sta>();
            txt_pos = uiHolder.elementTrsLst[17].GetComponent<Txt>();
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
