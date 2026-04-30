
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
            public Btn btn_perspective;
            public Txt txt_perspective;
        public UiModStoryConfigInitView(UiHolder uiHolder):base(uiHolder)
        {

            btn_mainCharacter = uiHolder.elementTrsLst[0].GetComponent<Btn>();
            txt_mainCharacter = uiHolder.elementTrsLst[1].GetComponent<Txt>();
            btn_perspective = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            txt_perspective = uiHolder.elementTrsLst[3].GetComponent<Txt>();
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
            public Btn btn_enableEquip;
            public Sta sta_enableEquip;
            public Btn btn_enableSkill;
            public Sta sta_enableSkill;
            public GameObject go_teamItem;
            public ScrView scr_team;
            public GameObject go_activeTeamItem;
            public ScrView scr_activeTeam;
            public GameObject go_bagItem;
            public ScrView scr_bag;
        public UiModStoryConfigGlobalView(UiHolder uiHolder):base(uiHolder)
        {

            btn_mainCharacter = uiHolder.elementTrsLst[0].GetComponent<Btn>();
            txt_mainCharacter = uiHolder.elementTrsLst[1].GetComponent<Txt>();
            btn_enableEquip = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            sta_enableEquip = uiHolder.elementTrsLst[3].GetComponent<Sta>();
            btn_enableSkill = uiHolder.elementTrsLst[4].GetComponent<Btn>();
            sta_enableSkill = uiHolder.elementTrsLst[5].GetComponent<Sta>();
            go_teamItem = uiHolder.elementTrsLst[6].gameObject;
            scr_team = uiHolder.elementTrsLst[7].GetComponent<ScrView>();
            go_activeTeamItem = uiHolder.elementTrsLst[8].gameObject;
            scr_activeTeam = uiHolder.elementTrsLst[9].GetComponent<ScrView>();
            go_bagItem = uiHolder.elementTrsLst[10].gameObject;
            scr_bag = uiHolder.elementTrsLst[11].GetComponent<ScrView>();
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

namespace ModStoryConfigTeamItem
{
    public partial class UiTeamItemParam:UiParam
    {
    }

    public partial class UiTeamItemView:UiView
    {
            public GameObject go_exist;
            public Sta sta_exist;
            public Btn btn_new;
            public Btn btn_;
            public Img img_;
            public Txt txt_;
        public UiTeamItemView(UiHolder uiHolder):base(uiHolder)
        {
            go_exist = uiHolder.elementTrsLst[0].gameObject;
            sta_exist = uiHolder.elementTrsLst[1].GetComponent<Sta>();
            btn_new = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            btn_ = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            img_ = uiHolder.elementTrsLst[4].GetComponent<Img>();
            txt_ = uiHolder.elementTrsLst[5].GetComponent<Txt>();
        }
    }
    public partial class UiTeamItemCtrl:UiCtrl
    {
        public UiTeamItemView view;
        public UiTeamItemModel model;
        public UiTeamItemParam param;
        public UiModStoryConfigCtrl parent=>(UiModStoryConfigCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiTeamItemParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {
            base.BindHolderRecursively(uiHolder);
            view = new UiTeamItemView(uiHolder);
            model=new UiTeamItemModel();
        }
    }
    public partial class UiTeamItemModel:UiModel
    {
    }
}

namespace ModStoryConfigActiveTeamItem
{
    public partial class UiActiveTeamItemParam:UiParam
    {
    }

    public partial class UiActiveTeamItemView:UiView
    {
            public GameObject go_exist;
            public Sta sta_exist;
            public Btn btn_new;
            public Btn btn_;
            public Img img_;
            public Txt txt_;
        public UiActiveTeamItemView(UiHolder uiHolder):base(uiHolder)
        {
            go_exist = uiHolder.elementTrsLst[0].gameObject;
            sta_exist = uiHolder.elementTrsLst[1].GetComponent<Sta>();
            btn_new = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            btn_ = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            img_ = uiHolder.elementTrsLst[4].GetComponent<Img>();
            txt_ = uiHolder.elementTrsLst[5].GetComponent<Txt>();
        }
    }
    public partial class UiActiveTeamItemCtrl:UiCtrl
    {
        public UiActiveTeamItemView view;
        public UiActiveTeamItemModel model;
        public UiActiveTeamItemParam param;
        public UiModStoryConfigCtrl parent=>(UiModStoryConfigCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiActiveTeamItemParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {
            base.BindHolderRecursively(uiHolder);
            view = new UiActiveTeamItemView(uiHolder);
            model=new UiActiveTeamItemModel();
        }
    }
    public partial class UiActiveTeamItemModel:UiModel
    {
    }
}

namespace ModStoryConfigBagItem
{
    public partial class UiBagItemParam:UiParam
    {
    }

    public partial class UiBagItemView:UiView
    {
            public GameObject go_exist;
            public Sta sta_exist;
            public Btn btn_new;
            public Btn btn_;
            public Img img_;
            public Txt txt_;
        public UiBagItemView(UiHolder uiHolder):base(uiHolder)
        {
            go_exist = uiHolder.elementTrsLst[0].gameObject;
            sta_exist = uiHolder.elementTrsLst[1].GetComponent<Sta>();
            btn_new = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            btn_ = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            img_ = uiHolder.elementTrsLst[4].GetComponent<Img>();
            txt_ = uiHolder.elementTrsLst[5].GetComponent<Txt>();
        }
    }
    public partial class UiBagItemCtrl:UiCtrl
    {
        public UiBagItemView view;
        public UiBagItemModel model;
        public UiBagItemParam param;
        public UiModStoryConfigCtrl parent=>(UiModStoryConfigCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiBagItemParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {
            base.BindHolderRecursively(uiHolder);
            view = new UiBagItemView(uiHolder);
            model=new UiBagItemModel();
        }
    }
    public partial class UiBagItemModel:UiModel
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
