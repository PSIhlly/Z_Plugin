
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;

namespace Ui.Axis

{


    public partial class UiAxisParam:UiParam
    {
    }

    public partial class UiAxisView:UiView
    {

            public RectTransform rtf_axis;
            public Btn btn_x;
            public Btn btn_y;
            public Btn btn_rot;
        public UiAxisView(UiHolder uiHolder):base(uiHolder)
        {

            rtf_axis = uiHolder.elementTrsLst[0].GetComponent<RectTransform>();
            btn_x = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            btn_y = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            btn_rot = uiHolder.elementTrsLst[3].GetComponent<Btn>();
        }

    }
    public partial class UiAxisCtrl:UiCtrl
    {
        public UiAxisView view;
        public UiAxisModel model;
        public UiAxisParam param;
        

        public override void SetParam(UiParam param)
        {
            this.param = (UiAxisParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiAxisView(uiHolder);
            model=new UiAxisModel();


        }

    }
    public partial class UiAxisModel:UiModel
    {
        
    }
}
