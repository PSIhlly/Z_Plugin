
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
            public Txt txt_title;
            public Btn btn_save;
            public Btn btn_back;
            public Btn btn_exit;
            public Img img_save;
            public Txt txt_save;
            public Img img_back;
            public Txt txt_back;
            public Img img_exit;
            public Txt txt_exit;
        public UiPlaySceneMenuView(UiHolder uiHolder):base(uiHolder)
        {

            btn_bbg = uiHolder.elementTrsLst[0].GetComponent<Btn>();
            txt_title = uiHolder.elementTrsLst[1].GetComponent<Txt>();
            btn_save = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            btn_back = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            btn_exit = uiHolder.elementTrsLst[4].GetComponent<Btn>();
            img_save = uiHolder.elementTrsLst[5].GetComponent<Img>();
            txt_save = uiHolder.elementTrsLst[6].GetComponent<Txt>();
            img_back = uiHolder.elementTrsLst[7].GetComponent<Img>();
            txt_back = uiHolder.elementTrsLst[8].GetComponent<Txt>();
            img_exit = uiHolder.elementTrsLst[9].GetComponent<Img>();
            txt_exit = uiHolder.elementTrsLst[10].GetComponent<Txt>();
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
