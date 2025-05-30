
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;

namespace Ui.Start

{


    public partial class UiStartParam:UiParam
    {
    }

    public partial class UiStartView:UiView
    {

            public Txt txt_title;
            public Btn btn_ori;
            public Btn btn_mod;
            public Btn btn_back;
        public UiStartView(UiHolder uiHolder):base(uiHolder)
        {

            txt_title = uiHolder.elementTrsLst[0].GetComponent<Txt>();
            btn_ori = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            btn_mod = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            btn_back = uiHolder.elementTrsLst[3].GetComponent<Btn>();
        }

    }
    public partial class UiStartCtrl:UiCtrl
    {
        public UiStartView view;
        public UiStartModel model;
        public UiStartParam param;
        

        public override void SetParam(UiParam param)
        {
            this.param = (UiStartParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiStartView(uiHolder);
            model=new UiStartModel();


        }

    }
    public partial class UiStartModel:UiModel
    {
        
    }
}
