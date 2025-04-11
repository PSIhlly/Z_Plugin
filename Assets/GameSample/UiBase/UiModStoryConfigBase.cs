
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;

namespace Ui.ModStoryConfig

{


namespace ModStoryConfigInit

{


    public partial class UiModStoryConfigInitParam:UiParam
    {
    }

    public partial class UiModStoryConfigInitView:UiView
    {

            public Btn btn_mainCharacter;
            public Txt txt_mainCharacter;
        public UiModStoryConfigInitView(UiHolder uiHolder):base(uiHolder)
        {

            btn_mainCharacter = uiHolder.elementTrsLst[0].GetComponent<Btn>();
            txt_mainCharacter = uiHolder.elementTrsLst[1].GetComponent<Txt>();
        }

    }
    public partial class UiModStoryConfigInitCtrl:UiCtrl
    {
        public UiModStoryConfigInitView view;
        public UiModStoryConfigInitModel model;
        public UiModStoryConfigInitParam param;
        public UiModStoryConfigCtrl parent=>(UiModStoryConfigCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryConfigInitParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryConfigInitView(uiHolder);
            model=new UiModStoryConfigInitModel();


        }

    }
    public partial class UiModStoryConfigInitModel:UiModel
    {
        
    }
}

namespace ModStoryConfigGlobal

{


    public partial class UiModStoryConfigGlobalParam:UiParam
    {
    }

    public partial class UiModStoryConfigGlobalView:UiView
    {

            public Btn btn_mainCharacter;
            public Txt txt_mainCharacter;
        public UiModStoryConfigGlobalView(UiHolder uiHolder):base(uiHolder)
        {

            btn_mainCharacter = uiHolder.elementTrsLst[0].GetComponent<Btn>();
            txt_mainCharacter = uiHolder.elementTrsLst[1].GetComponent<Txt>();
        }

    }
    public partial class UiModStoryConfigGlobalCtrl:UiCtrl
    {
        public UiModStoryConfigGlobalView view;
        public UiModStoryConfigGlobalModel model;
        public UiModStoryConfigGlobalParam param;
        public UiModStoryConfigCtrl parent=>(UiModStoryConfigCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryConfigGlobalParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryConfigGlobalView(uiHolder);
            model=new UiModStoryConfigGlobalModel();


        }

    }
    public partial class UiModStoryConfigGlobalModel:UiModel
    {
        
    }
}

    public partial class UiModStoryConfigParam:UiParam
    {
    }

    public partial class UiModStoryConfigView:UiView
    {

            public ModStoryConfigInit.UiModStoryConfigInitCtrl page_ModStoryConfigInit;
            public ModStoryConfigGlobal.UiModStoryConfigGlobalCtrl page_ModStoryConfigGlobal;
            public Btn btn_back;
            public Btn btn_init;
            public Sta sta_init;
            public Btn btn_global;
            public Sta sta_global;
            public Img img_init;
            public Txt txt_init;
            public Img img_global;
            public Txt txt_global;
        public UiModStoryConfigView(UiHolder uiHolder):base(uiHolder)
        {

            page_ModStoryConfigInit = (ModStoryConfigInit.UiModStoryConfigInitCtrl) uiHolder.elementTrsLst[0].GetComponent<UiHolder>().ctrl;
            page_ModStoryConfigGlobal = (ModStoryConfigGlobal.UiModStoryConfigGlobalCtrl) uiHolder.elementTrsLst[1].GetComponent<UiHolder>().ctrl;
            btn_back = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            btn_init = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            sta_init = uiHolder.elementTrsLst[4].GetComponent<Sta>();
            btn_global = uiHolder.elementTrsLst[5].GetComponent<Btn>();
            sta_global = uiHolder.elementTrsLst[6].GetComponent<Sta>();
            img_init = uiHolder.elementTrsLst[7].GetComponent<Img>();
            txt_init = uiHolder.elementTrsLst[8].GetComponent<Txt>();
            img_global = uiHolder.elementTrsLst[9].GetComponent<Img>();
            txt_global = uiHolder.elementTrsLst[10].GetComponent<Txt>();
        }

    }
    public partial class UiModStoryConfigCtrl:UiCtrl
    {
        public UiModStoryConfigView view;
        public UiModStoryConfigModel model;
        public UiModStoryConfigParam param;
        

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryConfigParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryConfigView(uiHolder);
            model=new UiModStoryConfigModel();


            view.page_ModStoryConfigInit = new ModStoryConfigInit.UiModStoryConfigInitCtrl();
            view.page_ModStoryConfigInit.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
            view.page_ModStoryConfigGlobal = new ModStoryConfigGlobal.UiModStoryConfigGlobalCtrl();
            view.page_ModStoryConfigGlobal.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
        }

    }
    public partial class UiModStoryConfigModel:UiModel
    {
        
    }
}
