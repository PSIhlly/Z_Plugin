
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;
namespace Ui.PlaySceneMenu

{




    public partial class UiPlaySceneMenuParam:UiParam
    {
    }

    public partial class UiPlaySceneMenuView:UiView
    {

            public Btn btn_bbg;
            public Btn btn_save;
            public Btn btn_back;
            public Btn btn_exit;
        public UiPlaySceneMenuView(UiHolder uiHolder):base(uiHolder)
        {

            btn_bbg = uiHolder.elementTrsLst[0].GetComponent<Btn>();
            btn_save = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            btn_back = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            btn_exit = uiHolder.elementTrsLst[3].GetComponent<Btn>();
        }

    }
    public partial class UiPlaySceneMenuCtrl:UiCtrl
    {
        public UiPlaySceneMenuView view;
        public UiPlaySceneMenuModel model;
        public UiPlaySceneMenuParam param;
        

        public override void SetParam(UiParam param)
        {
            this.param = (UiPlaySceneMenuParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiPlaySceneMenuView(uiHolder);
            model=new UiPlaySceneMenuModel();


        }

    }
    public partial class UiPlaySceneMenuModel:UiModel
    {
        
    }
}
