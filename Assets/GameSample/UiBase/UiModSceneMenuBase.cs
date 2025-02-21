
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;

namespace Ui
{


    public partial class UiModSceneMenuParam:UiParam
    {
    }

    public partial class UiModSceneMenuView:UiView
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
        public UiModSceneMenuView(UiHolder uiHolder):base(uiHolder)
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
