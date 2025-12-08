
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;
using Z_Texture;
using Z_Video;
using UnityEngine.Video;
namespace Ui.AnimChoose

{




    public partial class UiAnimChooseParam:UiParam
    {
    }

    public partial class UiAnimChooseView:UiView
    {

            public Txt txt_;
            public Btn btn_;
            public Txt txt_anim;
        public UiAnimChooseView(UiHolder uiHolder):base(uiHolder)
        {

            txt_ = uiHolder.elementTrsLst[0].GetComponent<Txt>();
            btn_ = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            txt_anim = uiHolder.elementTrsLst[2].GetComponent<Txt>();
        }

    }
    public partial class UiAnimChooseCtrl:UiCtrl
    {
        public UiAnimChooseView view;
        public UiAnimChooseModel model;
        public UiAnimChooseParam param;
        

        public override void SetParam(UiParam param)
        {
            this.param = (UiAnimChooseParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiAnimChooseView(uiHolder);
            model=new UiAnimChooseModel();


        }

    }
    public partial class UiAnimChooseModel:UiModel
    {
        
    }
}
