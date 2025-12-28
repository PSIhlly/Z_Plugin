
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;
using Z_Texture;using RenderHeads.Media.AVProVideo;
namespace Ui.DialogBg

{




    public partial class UiDialogBgParam:UiParam
    {
    }

    public partial class UiDialogBgView:UiView
    {

            public Img img_;
            public MediaPlayer mp_;
        public UiDialogBgView(UiHolder uiHolder):base(uiHolder)
        {

            img_ = uiHolder.elementTrsLst[0].GetComponent<Img>();
            mp_ = uiHolder.elementTrsLst[1].GetComponent<MediaPlayer>();
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
