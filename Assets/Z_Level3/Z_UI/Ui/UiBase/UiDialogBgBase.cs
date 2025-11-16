
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;
using Z_Texture;
using Z_Video;
using UnityEngine.Video;
namespace Ui.DialogBg

{




    public partial class UiDialogBgParam:UiParam
    {
    }

    public partial class UiDialogBgView:UiView
    {

            public Img img_;
            public VideoPlayer vp_;
        public UiDialogBgView(UiHolder uiHolder):base(uiHolder)
        {

            img_ = uiHolder.elementTrsLst[0].GetComponent<Img>();
            vp_ = uiHolder.elementTrsLst[1].GetComponent<VideoPlayer>();
        }

    }
    public partial class UiDialogBgCtrl:UiCtrl
    {
        public UiDialogBgView view;
        public UiDialogBgModel model;
        public UiDialogBgParam param;
        

        public override void SetParam(UiParam param)
        {
            this.param = (UiDialogBgParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiDialogBgView(uiHolder);
            model=new UiDialogBgModel();


        }

    }
    public partial class UiDialogBgModel:UiModel
    {
        
    }
}
