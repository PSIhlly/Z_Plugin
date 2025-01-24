
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;

namespace Ui
{


    public partial class UiLoadingParam:UiParam
    {
    }

    public partial class UiLoadingView:UiView
    {

            public Txt txt_title;
            public Btn btn_back;
        public UiLoadingView(UiHolder uiHolder):base(uiHolder)
        {

            txt_title = uiHolder.elementTrsLst[0].GetComponent<Txt>();
            btn_back = uiHolder.elementTrsLst[1].GetComponent<Btn>();
        }

    }
    public partial class UiLoadingCtrl:UiCtrl
    {
        public UiLoadingView view;
        public UiLoadingModel model;
        public UiLoadingParam param;
        

        public override void SetParam(UiParam param)
        {
            this.param = (UiLoadingParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiLoadingView(uiHolder);
            model=new UiLoadingModel();


        }

    }
    public partial class UiLoadingModel:UiModel
    {
        
    }
}
