
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;
namespace Ui.ModSceneMenu

{




    public partial class UiModSceneMenuParam:UiParam
    {
    }

    public partial class UiModSceneMenuView:UiView
    {

            public Btn btn_bbg;
            public Btn btn_save;
            public Btn btn_back;
            public Btn btn_exit;
        public UiModSceneMenuView(UiHolder uiHolder):base(uiHolder)
        {

            btn_bbg = uiHolder.elementTrsLst[0].GetComponent<Btn>();
            btn_save = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            btn_back = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            btn_exit = uiHolder.elementTrsLst[3].GetComponent<Btn>();
        }

    }
    public partial class UiModSceneMenuCtrl:UiCtrl
    {
        public UiModSceneMenuView view;
        public UiModSceneMenuModel model;
        public UiModSceneMenuParam param;
        

        public override void SetParam(UiParam param)
        {
            this.param = (UiModSceneMenuParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModSceneMenuView(uiHolder);
            model=new UiModSceneMenuModel();


        }

    }
    public partial class UiModSceneMenuModel:UiModel
    {
        
    }
}
