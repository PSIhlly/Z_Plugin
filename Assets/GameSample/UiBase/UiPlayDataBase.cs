
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;
using Z_Texture;using RenderHeads.Media.AVProVideo;
namespace Ui.PlayData

{




namespace PlayDataBackpack

{







    public partial class UiLabParam:UiParam
    {
    }

    public partial class UiLabView:UiView
    {

            public GameObject go_lab;
            public Btn btn_;
            public Sta sta_;
            public Sta sta_valid;
            public Txt txt_;
        public UiLabView(UiHolder uiHolder):base(uiHolder)
        {

            go_lab = uiHolder.elementTrsLst[0].gameObject;
            btn_ = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            sta_ = uiHolder.elementTrsLst[2].GetComponent<Sta>();
            sta_valid = uiHolder.elementTrsLst[3].GetComponent<Sta>();
            txt_ = uiHolder.elementTrsLst[4].GetComponent<Txt>();
        }

    }
    public partial class UiLabCtrl:UiCtrl
    {
        public UiLabView view;
        public UiLabModel model;
        public UiLabParam param;
        public UiPlayDataBackpackCtrl parent=>(UiPlayDataBackpackCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiLabParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiLabView(uiHolder);
            model=new UiLabModel();


        }

    }
    public partial class UiLabModel:UiModel
    {
        
    }



    public partial class UiGameItemParam:UiParam
    {
    }

    public partial class UiGameItemView:UiView
    {

            public GameObject go_gameItem;
            public Sta sta_exist;
            public Btn btn_new;
            public Btn btn_;
            public Sta sta_;
            public Txt txt_count;
            public Img img_;
            public Txt txt_;
        public UiGameItemView(UiHolder uiHolder):base(uiHolder)
        {

            go_gameItem = uiHolder.elementTrsLst[0].gameObject;
            sta_exist = uiHolder.elementTrsLst[1].GetComponent<Sta>();
            btn_new = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            btn_ = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            sta_ = uiHolder.elementTrsLst[4].GetComponent<Sta>();
            txt_count = uiHolder.elementTrsLst[5].GetComponent<Txt>();
            img_ = uiHolder.elementTrsLst[6].GetComponent<Img>();
            txt_ = uiHolder.elementTrsLst[7].GetComponent<Txt>();
        }

    }
    public partial class UiGameItemCtrl:UiCtrl
    {
        public UiGameItemView view;
        public UiGameItemModel model;
        public UiGameItemParam param;
        public UiPlayDataBackpackCtrl parent=>(UiPlayDataBackpackCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiGameItemParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiGameItemView(uiHolder);
            model=new UiGameItemModel();


        }

    }
    public partial class UiGameItemModel:UiModel
    {
        
    }



    public partial class UiGameArgsParam:UiParam
    {
    }

    public partial class UiGameArgsView:UiView
    {

            public GameObject go_gameArgs;
            public Sta sta_gameArgs;
            public Txt txt_;
        public UiGameArgsView(UiHolder uiHolder):base(uiHolder)
        {

            go_gameArgs = uiHolder.elementTrsLst[0].gameObject;
            sta_gameArgs = uiHolder.elementTrsLst[1].GetComponent<Sta>();
            txt_ = uiHolder.elementTrsLst[2].GetComponent<Txt>();
        }

    }
    public partial class UiGameArgsCtrl:UiCtrl
    {
        public UiGameArgsView view;
        public UiGameArgsModel model;
        public UiGameArgsParam param;
        public UiPlayDataBackpackCtrl parent=>(UiPlayDataBackpackCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiGameArgsParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiGameArgsView(uiHolder);
            model=new UiGameArgsModel();


        }

    }
    public partial class UiGameArgsModel:UiModel
    {
        
    }
    public partial class UiPlayDataBackpackParam:UiParam
    {
    }

    public partial class UiPlayDataBackpackView:UiView
    {

            public ScrView scr_labs;
            public ScrView scr_gameItems;
            public Sta sta_show;
            public Sta sta_showArea;
            public Img img_;
            public Btn btn_;
            public Txt txt_desc;
            public ScrView scr_gameArgs;
            public Txt txt_name;
            public Txt txt_amount;
            public GameObject go_lab;
            public UiLabCtrl sub_lab;
            public GameObject go_gameItem;
            public UiGameItemCtrl sub_gameItem;
            public Btn btn_drop;
            public Btn btn_use;
            public Btn btn_equip;
            public Btn btn_unequip;
            public GameObject go_gameArgs;
            public Sta sta_gameArgs;
            public UiGameArgsCtrl sub_gameArgs;
        public UiPlayDataBackpackView(UiHolder uiHolder):base(uiHolder)
        {

            scr_labs = uiHolder.elementTrsLst[0].GetComponent<ScrView>();
            scr_gameItems = uiHolder.elementTrsLst[1].GetComponent<ScrView>();
            sta_show = uiHolder.elementTrsLst[2].GetComponent<Sta>();
            sta_showArea = uiHolder.elementTrsLst[3].GetComponent<Sta>();
            img_ = uiHolder.elementTrsLst[4].GetComponent<Img>();
            btn_ = uiHolder.elementTrsLst[5].GetComponent<Btn>();
            txt_desc = uiHolder.elementTrsLst[6].GetComponent<Txt>();
            scr_gameArgs = uiHolder.elementTrsLst[7].GetComponent<ScrView>();
            txt_name = uiHolder.elementTrsLst[8].GetComponent<Txt>();
            txt_amount = uiHolder.elementTrsLst[9].GetComponent<Txt>();
            go_lab = uiHolder.elementTrsLst[10].gameObject;
            sub_lab = (UiLabCtrl) uiHolder.elementTrsLst[11].GetComponent<UiHolder>().ctrl;
            go_gameItem = uiHolder.elementTrsLst[12].gameObject;
            sub_gameItem = (UiGameItemCtrl) uiHolder.elementTrsLst[13].GetComponent<UiHolder>().ctrl;
            btn_drop = uiHolder.elementTrsLst[14].GetComponent<Btn>();
            btn_use = uiHolder.elementTrsLst[15].GetComponent<Btn>();
            btn_equip = uiHolder.elementTrsLst[16].GetComponent<Btn>();
            btn_unequip = uiHolder.elementTrsLst[17].GetComponent<Btn>();
            go_gameArgs = uiHolder.elementTrsLst[18].gameObject;
            sta_gameArgs = uiHolder.elementTrsLst[19].GetComponent<Sta>();
            sub_gameArgs = (UiGameArgsCtrl) uiHolder.elementTrsLst[20].GetComponent<UiHolder>().ctrl;
        }

    }
    public partial class UiPlayDataBackpackCtrl:UiCtrl
    {
        public UiPlayDataBackpackView view;
        public UiPlayDataBackpackModel model;
        public UiPlayDataBackpackParam param;
        public UiPlayDataCtrl parent=>(UiPlayDataCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiPlayDataBackpackParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiPlayDataBackpackView(uiHolder);
            model=new UiPlayDataBackpackModel();


            view.sub_lab = new UiLabCtrl();
            view.sub_lab.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
            view.sub_gameItem = new UiGameItemCtrl();
            view.sub_gameItem.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
            view.sub_gameArgs = new UiGameArgsCtrl();
            view.sub_gameArgs.BindHolderRecursively(uiHolder.subUiHolderLst[2]);
        }

    }
    public partial class UiPlayDataBackpackModel:UiModel
    {
        
    }
}

namespace PlayDataCharacter

{




namespace PlayDataCharacterData

{







    public partial class UiGameArgsParam:UiParam
    {
    }

    public partial class UiGameArgsView:UiView
    {

            public GameObject go_gameArgs;
            public Sta sta_gameArgs;
            public Txt txt_;
        public UiGameArgsView(UiHolder uiHolder):base(uiHolder)
        {

            go_gameArgs = uiHolder.elementTrsLst[0].gameObject;
            sta_gameArgs = uiHolder.elementTrsLst[1].GetComponent<Sta>();
            txt_ = uiHolder.elementTrsLst[2].GetComponent<Txt>();
        }

    }
    public partial class UiGameArgsCtrl:UiCtrl
    {
        public UiGameArgsView view;
        public UiGameArgsModel model;
        public UiGameArgsParam param;
        public UiPlayDataCharacterDataCtrl parent=>(UiPlayDataCharacterDataCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiGameArgsParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiGameArgsView(uiHolder);
            model=new UiGameArgsModel();


        }

    }
    public partial class UiGameArgsModel:UiModel
    {
        
    }
    public partial class UiPlayDataCharacterDataParam:UiParam
    {
    }

    public partial class UiPlayDataCharacterDataView:UiView
    {

            public ScrView scr_gameArgs;
            public Txt txt_desc;
            public Txt txt_name;
            public Img img_tachie;
            public GameObject go_gameArgs;
            public Sta sta_gameArgs;
            public UiGameArgsCtrl sub_gameArgs;
        public UiPlayDataCharacterDataView(UiHolder uiHolder):base(uiHolder)
        {

            scr_gameArgs = uiHolder.elementTrsLst[0].GetComponent<ScrView>();
            txt_desc = uiHolder.elementTrsLst[1].GetComponent<Txt>();
            txt_name = uiHolder.elementTrsLst[2].GetComponent<Txt>();
            img_tachie = uiHolder.elementTrsLst[3].GetComponent<Img>();
            go_gameArgs = uiHolder.elementTrsLst[4].gameObject;
            sta_gameArgs = uiHolder.elementTrsLst[5].GetComponent<Sta>();
            sub_gameArgs = (UiGameArgsCtrl) uiHolder.elementTrsLst[6].GetComponent<UiHolder>().ctrl;
        }

    }
    public partial class UiPlayDataCharacterDataCtrl:UiCtrl
    {
        public UiPlayDataCharacterDataView view;
        public UiPlayDataCharacterDataModel model;
        public UiPlayDataCharacterDataParam param;
        public UiPlayDataCharacterCtrl parent=>(UiPlayDataCharacterCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiPlayDataCharacterDataParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiPlayDataCharacterDataView(uiHolder);
            model=new UiPlayDataCharacterDataModel();


            view.sub_gameArgs = new UiGameArgsCtrl();
            view.sub_gameArgs.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
        }

    }
    public partial class UiPlayDataCharacterDataModel:UiModel
    {
        
    }
}

namespace PlayDataCharacterEquip

{







    public partial class UiGameEquipParam:UiParam
    {
    }

    public partial class UiGameEquipView:UiView
    {

            public GameObject go_gameEquip;
            public Sta sta_exist;
            public Txt txt_;
            public Btn btn_;
            public Sta sta_;
            public Img img_;
        public UiGameEquipView(UiHolder uiHolder):base(uiHolder)
        {

            go_gameEquip = uiHolder.elementTrsLst[0].gameObject;
            sta_exist = uiHolder.elementTrsLst[1].GetComponent<Sta>();
            txt_ = uiHolder.elementTrsLst[2].GetComponent<Txt>();
            btn_ = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            sta_ = uiHolder.elementTrsLst[4].GetComponent<Sta>();
            img_ = uiHolder.elementTrsLst[5].GetComponent<Img>();
        }

    }
    public partial class UiGameEquipCtrl:UiCtrl
    {
        public UiGameEquipView view;
        public UiGameEquipModel model;
        public UiGameEquipParam param;
        public UiPlayDataCharacterEquipCtrl parent=>(UiPlayDataCharacterEquipCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiGameEquipParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiGameEquipView(uiHolder);
            model=new UiGameEquipModel();


        }

    }
    public partial class UiGameEquipModel:UiModel
    {
        
    }



    public partial class UiGameArgsParam:UiParam
    {
    }

    public partial class UiGameArgsView:UiView
    {

            public GameObject go_gameArgs;
            public Sta sta_gameArgs;
            public Txt txt_;
        public UiGameArgsView(UiHolder uiHolder):base(uiHolder)
        {

            go_gameArgs = uiHolder.elementTrsLst[0].gameObject;
            sta_gameArgs = uiHolder.elementTrsLst[1].GetComponent<Sta>();
            txt_ = uiHolder.elementTrsLst[2].GetComponent<Txt>();
        }

    }
    public partial class UiGameArgsCtrl:UiCtrl
    {
        public UiGameArgsView view;
        public UiGameArgsModel model;
        public UiGameArgsParam param;
        public UiPlayDataCharacterEquipCtrl parent=>(UiPlayDataCharacterEquipCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiGameArgsParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiGameArgsView(uiHolder);
            model=new UiGameArgsModel();


        }

    }
    public partial class UiGameArgsModel:UiModel
    {
        
    }
    public partial class UiPlayDataCharacterEquipParam:UiParam
    {
    }

    public partial class UiPlayDataCharacterEquipView:UiView
    {

            public ScrView scr_gameEquip;
            public Sta sta_show;
            public Txt txt_;
            public Txt txt_part;
            public Txt txt_name;
            public ScrView scr_gameArgs;
            public GameObject go_gameEquip;
            public UiGameEquipCtrl sub_gameEquip;
            public Btn btn_unequip;
            public Btn btn_equip;
            public GameObject go_gameArgs;
            public Sta sta_gameArgs;
            public UiGameArgsCtrl sub_gameArgs;
        public UiPlayDataCharacterEquipView(UiHolder uiHolder):base(uiHolder)
        {

            scr_gameEquip = uiHolder.elementTrsLst[0].GetComponent<ScrView>();
            sta_show = uiHolder.elementTrsLst[1].GetComponent<Sta>();
            txt_ = uiHolder.elementTrsLst[2].GetComponent<Txt>();
            txt_part = uiHolder.elementTrsLst[3].GetComponent<Txt>();
            txt_name = uiHolder.elementTrsLst[4].GetComponent<Txt>();
            scr_gameArgs = uiHolder.elementTrsLst[5].GetComponent<ScrView>();
            go_gameEquip = uiHolder.elementTrsLst[6].gameObject;
            sub_gameEquip = (UiGameEquipCtrl) uiHolder.elementTrsLst[7].GetComponent<UiHolder>().ctrl;
            btn_unequip = uiHolder.elementTrsLst[8].GetComponent<Btn>();
            btn_equip = uiHolder.elementTrsLst[9].GetComponent<Btn>();
            go_gameArgs = uiHolder.elementTrsLst[10].gameObject;
            sta_gameArgs = uiHolder.elementTrsLst[11].GetComponent<Sta>();
            sub_gameArgs = (UiGameArgsCtrl) uiHolder.elementTrsLst[12].GetComponent<UiHolder>().ctrl;
        }

    }
    public partial class UiPlayDataCharacterEquipCtrl:UiCtrl
    {
        public UiPlayDataCharacterEquipView view;
        public UiPlayDataCharacterEquipModel model;
        public UiPlayDataCharacterEquipParam param;
        public UiPlayDataCharacterCtrl parent=>(UiPlayDataCharacterCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiPlayDataCharacterEquipParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiPlayDataCharacterEquipView(uiHolder);
            model=new UiPlayDataCharacterEquipModel();


            view.sub_gameEquip = new UiGameEquipCtrl();
            view.sub_gameEquip.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
            view.sub_gameArgs = new UiGameArgsCtrl();
            view.sub_gameArgs.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
        }

    }
    public partial class UiPlayDataCharacterEquipModel:UiModel
    {
        
    }
}

namespace PlayDataCharacterSkill

{




    public partial class UiPlayDataCharacterSkillParam:UiParam
    {
    }

    public partial class UiPlayDataCharacterSkillView:UiView
    {

        public UiPlayDataCharacterSkillView(UiHolder uiHolder):base(uiHolder)
        {

        }

    }
    public partial class UiPlayDataCharacterSkillCtrl:UiCtrl
    {
        public UiPlayDataCharacterSkillView view;
        public UiPlayDataCharacterSkillModel model;
        public UiPlayDataCharacterSkillParam param;
        public UiPlayDataCharacterCtrl parent=>(UiPlayDataCharacterCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiPlayDataCharacterSkillParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiPlayDataCharacterSkillView(uiHolder);
            model=new UiPlayDataCharacterSkillModel();


        }

    }
    public partial class UiPlayDataCharacterSkillModel:UiModel
    {
        
    }
}




    public partial class UiGameItemParam:UiParam
    {
    }

    public partial class UiGameItemView:UiView
    {

            public GameObject go_gameItem;
            public Sta sta_exist;
            public Btn btn_new;
            public Btn btn_;
            public Sta sta_;
            public Txt txt_count;
            public Img img_;
            public Txt txt_;
        public UiGameItemView(UiHolder uiHolder):base(uiHolder)
        {

            go_gameItem = uiHolder.elementTrsLst[0].gameObject;
            sta_exist = uiHolder.elementTrsLst[1].GetComponent<Sta>();
            btn_new = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            btn_ = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            sta_ = uiHolder.elementTrsLst[4].GetComponent<Sta>();
            txt_count = uiHolder.elementTrsLst[5].GetComponent<Txt>();
            img_ = uiHolder.elementTrsLst[6].GetComponent<Img>();
            txt_ = uiHolder.elementTrsLst[7].GetComponent<Txt>();
        }

    }
    public partial class UiGameItemCtrl:UiCtrl
    {
        public UiGameItemView view;
        public UiGameItemModel model;
        public UiGameItemParam param;
        public UiPlayDataCharacterCtrl parent=>(UiPlayDataCharacterCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiGameItemParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiGameItemView(uiHolder);
            model=new UiGameItemModel();


        }

    }
    public partial class UiGameItemModel:UiModel
    {
        
    }
    public partial class UiPlayDataCharacterParam:UiParam
    {
    }

    public partial class UiPlayDataCharacterView:UiView
    {

            public ScrView scr_gameCharacters;
            public PlayDataCharacterData.UiPlayDataCharacterDataCtrl page_PlayDataCharacterData;
            public PlayDataCharacterEquip.UiPlayDataCharacterEquipCtrl page_PlayDataCharacterEquip;
            public PlayDataCharacterSkill.UiPlayDataCharacterSkillCtrl page_PlayDataCharacterSkill;
            public GameObject go_gameItem;
            public UiGameItemCtrl sub_gameItem;
            public Btn btn_data;
            public Sta sta_data;
            public Btn btn_equip;
            public Sta sta_equip;
            public Btn btn_skill;
            public Sta sta_skill;
        public UiPlayDataCharacterView(UiHolder uiHolder):base(uiHolder)
        {

            scr_gameCharacters = uiHolder.elementTrsLst[0].GetComponent<ScrView>();
            page_PlayDataCharacterData = (PlayDataCharacterData.UiPlayDataCharacterDataCtrl) uiHolder.elementTrsLst[1].GetComponent<UiHolder>().ctrl;
            page_PlayDataCharacterEquip = (PlayDataCharacterEquip.UiPlayDataCharacterEquipCtrl) uiHolder.elementTrsLst[2].GetComponent<UiHolder>().ctrl;
            page_PlayDataCharacterSkill = (PlayDataCharacterSkill.UiPlayDataCharacterSkillCtrl) uiHolder.elementTrsLst[3].GetComponent<UiHolder>().ctrl;
            go_gameItem = uiHolder.elementTrsLst[4].gameObject;
            sub_gameItem = (UiGameItemCtrl) uiHolder.elementTrsLst[5].GetComponent<UiHolder>().ctrl;
            btn_data = uiHolder.elementTrsLst[6].GetComponent<Btn>();
            sta_data = uiHolder.elementTrsLst[7].GetComponent<Sta>();
            btn_equip = uiHolder.elementTrsLst[8].GetComponent<Btn>();
            sta_equip = uiHolder.elementTrsLst[9].GetComponent<Sta>();
            btn_skill = uiHolder.elementTrsLst[10].GetComponent<Btn>();
            sta_skill = uiHolder.elementTrsLst[11].GetComponent<Sta>();
        }

    }
    public partial class UiPlayDataCharacterCtrl:UiCtrl
    {
        public UiPlayDataCharacterView view;
        public UiPlayDataCharacterModel model;
        public UiPlayDataCharacterParam param;
        public UiPlayDataCtrl parent=>(UiPlayDataCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiPlayDataCharacterParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiPlayDataCharacterView(uiHolder);
            model=new UiPlayDataCharacterModel();


            view.page_PlayDataCharacterData = new PlayDataCharacterData.UiPlayDataCharacterDataCtrl();
            view.page_PlayDataCharacterData.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
            view.page_PlayDataCharacterEquip = new PlayDataCharacterEquip.UiPlayDataCharacterEquipCtrl();
            view.page_PlayDataCharacterEquip.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
            view.page_PlayDataCharacterSkill = new PlayDataCharacterSkill.UiPlayDataCharacterSkillCtrl();
            view.page_PlayDataCharacterSkill.BindHolderRecursively(uiHolder.subUiHolderLst[2]);
            view.sub_gameItem = new UiGameItemCtrl();
            view.sub_gameItem.BindHolderRecursively(uiHolder.subUiHolderLst[3]);
        }

    }
    public partial class UiPlayDataCharacterModel:UiModel
    {
        
    }
}

    public partial class UiPlayDataParam:UiParam
    {
    }

    public partial class UiPlayDataView:UiView
    {

            public Btn btn_back;
            public PlayDataBackpack.UiPlayDataBackpackCtrl page_PlayDataBackpack;
            public PlayDataCharacter.UiPlayDataCharacterCtrl page_PlayDataCharacter;
            public Btn btn_backpack;
            public Sta sta_backpack;
            public Btn btn_character;
            public Sta sta_character;
        public UiPlayDataView(UiHolder uiHolder):base(uiHolder)
        {

            btn_back = uiHolder.elementTrsLst[0].GetComponent<Btn>();
            page_PlayDataBackpack = (PlayDataBackpack.UiPlayDataBackpackCtrl) uiHolder.elementTrsLst[1].GetComponent<UiHolder>().ctrl;
            page_PlayDataCharacter = (PlayDataCharacter.UiPlayDataCharacterCtrl) uiHolder.elementTrsLst[2].GetComponent<UiHolder>().ctrl;
            btn_backpack = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            sta_backpack = uiHolder.elementTrsLst[4].GetComponent<Sta>();
            btn_character = uiHolder.elementTrsLst[5].GetComponent<Btn>();
            sta_character = uiHolder.elementTrsLst[6].GetComponent<Sta>();
        }

    }
    public partial class UiPlayDataCtrl:UiCtrl
    {
        public UiPlayDataView view;
        public UiPlayDataModel model;
        public UiPlayDataParam param;
        

        public override void SetParam(UiParam param)
        {
            this.param = (UiPlayDataParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiPlayDataView(uiHolder);
            model=new UiPlayDataModel();


            view.page_PlayDataBackpack = new PlayDataBackpack.UiPlayDataBackpackCtrl();
            view.page_PlayDataBackpack.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
            view.page_PlayDataCharacter = new PlayDataCharacter.UiPlayDataCharacterCtrl();
            view.page_PlayDataCharacter.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
        }

    }
    public partial class UiPlayDataModel:UiModel
    {
        
    }
}
