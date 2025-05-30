
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;

namespace Ui.Lounge

{


    public partial class UiLoungeParam:UiParam
    {
    }

    public partial class UiLoungeView:UiView
    {

            public Txt txt_title;
            public Btn btn_back;
            public Img img_StfIcon;
            public Img img_SpIcon;
            public Txt txt_StfCnt;
            public Txt txt_SpCnt;
        public UiLoungeView(UiHolder uiHolder):base(uiHolder)
        {

            txt_title = uiHolder.elementTrsLst[0].GetComponent<Txt>();
            btn_back = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            img_StfIcon = uiHolder.elementTrsLst[2].GetComponent<Img>();
            img_SpIcon = uiHolder.elementTrsLst[3].GetComponent<Img>();
            txt_StfCnt = uiHolder.elementTrsLst[4].GetComponent<Txt>();
            txt_SpCnt = uiHolder.elementTrsLst[5].GetComponent<Txt>();
        }

    }
    public partial class UiLoungeCtrl:UiCtrl
    {
        public UiLoungeView view;
        public UiLoungeModel model;
        public UiLoungeParam param;
        

        public override void SetParam(UiParam param)
        {
            this.param = (UiLoungeParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiLoungeView(uiHolder);
            model=new UiLoungeModel();


        }

    }
    public partial class UiLoungeModel:UiModel
    {
        
    }
}
