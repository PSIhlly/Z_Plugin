
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;

namespace Ui.PlaySceneMain

{


    public partial class UiPlaySceneMainParam:UiParam
    {
    }

    public partial class UiPlaySceneMainView:UiView
    {

            public Btn btn_menu;
            public Txt txt_menu;
        public UiPlaySceneMainView(UiHolder uiHolder):base(uiHolder)
        {

            btn_menu = uiHolder.elementTrsLst[0].GetComponent<Btn>();
            txt_menu = uiHolder.elementTrsLst[1].GetComponent<Txt>();
        }

    }
    public partial class UiPlaySceneMainCtrl:UiCtrl
    {
        public UiPlaySceneMainView view;
        public UiPlaySceneMainModel model;
        public UiPlaySceneMainParam param;
        

        public override void SetParam(UiParam param)
        {
            this.param = (UiPlaySceneMainParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiPlaySceneMainView(uiHolder);
            model=new UiPlaySceneMainModel();


        }

    }
    public partial class UiPlaySceneMainModel:UiModel
    {
        
    }
}
