
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;
using Z_Texture;using RenderHeads.Media.AVProVideo;
namespace Ui.PlayEffect

{




    public partial class UiPlayEffectParam:UiParam
    {
    }

    public partial class UiPlayEffectView:UiView
    {

            public Img img_;
        public UiPlayEffectView(UiHolder uiHolder):base(uiHolder)
        {

            img_ = uiHolder.elementTrsLst[0].GetComponent<Img>();
        }

    }
    public partial class UiPlayEffectCtrl:UiCtrl
    {
        public UiPlayEffectView view;
        public UiPlayEffectModel model;
        public UiPlayEffectParam param;
        

        public override void SetParam(UiParam param)
        {
            this.param = (UiPlayEffectParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiPlayEffectView(uiHolder);
            model=new UiPlayEffectModel();


        }

    }
    public partial class UiPlayEffectModel:UiModel
    {
        
    }
}
