
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;
using Z_Texture;
 
using UnityEngine.Video;
namespace Ui.ParamShow

{




    public partial class UiParamShowParam:UiParam
    {
    }

    public partial class UiParamShowView:UiView
    {

            public Sld sld_;
            public Txt txt_ ;
            public Img img_;
        public UiParamShowView(UiHolder uiHolder):base(uiHolder)
        {

            sld_ = uiHolder.elementTrsLst[0].GetComponent<Sld>();
            txt_  = uiHolder.elementTrsLst[1].GetComponent<Txt>();
            img_ = uiHolder.elementTrsLst[2].GetComponent<Img>();
        }

    }
    public partial class UiParamShowCtrl:UiCtrl
    {
        public UiParamShowView view;
        public UiParamShowModel model;
        public UiParamShowParam param;
        

        public override void SetParam(UiParam param)
        {
            this.param = (UiParamShowParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiParamShowView(uiHolder);
            model=new UiParamShowModel();


        }

    }
    public partial class UiParamShowModel:UiModel
    {
        
    }
}
