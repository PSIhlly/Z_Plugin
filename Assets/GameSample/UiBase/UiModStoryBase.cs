
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;

namespace Ui
{




    public partial class UiSceneItemParam:UiParam
    {
    }

    public partial class UiSceneItemView:UiView
    {

            public GameObject go_sceneItem;
            public Btn btn_scene;
            public Txt txt_sceneName;
        public UiSceneItemView(UiHolder uiHolder):base(uiHolder)
        {

            go_sceneItem = uiHolder.elementTrsLst[0].gameObject;
            btn_scene = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            txt_sceneName = uiHolder.elementTrsLst[2].GetComponent<Txt>();
        }

    }
    public partial class UiSceneItemCtrl:UiCtrl
    {
        public UiSceneItemView view;
        public UiSceneItemModel model;
        public UiSceneItemParam param;
        public UiModStoryScenePanelCtrl parent=>(UiModStoryScenePanelCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiSceneItemParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiSceneItemView(uiHolder);
            model=new UiSceneItemModel();


        }

    }
    public partial class UiSceneItemModel:UiModel
    {
        
    }
    public partial class UiModStoryScenePanelParam:UiParam
    {
    }

    public partial class UiModStoryScenePanelView:UiView
    {

            public ScrView scr_tt;
            public GameObject go_sceneItem;
            public UiSceneItemCtrl sub_SceneItem;
        public UiModStoryScenePanelView(UiHolder uiHolder):base(uiHolder)
        {

            scr_tt = uiHolder.elementTrsLst[0].GetComponent<ScrView>();
            go_sceneItem = uiHolder.elementTrsLst[1].gameObject;
            sub_SceneItem = (UiSceneItemCtrl) uiHolder.elementTrsLst[2].GetComponent<UiHolder>().ctrl;
        }

    }
    public partial class UiModStoryScenePanelCtrl:UiCtrl
    {
        public UiModStoryScenePanelView view;
        public UiModStoryScenePanelModel model;
        public UiModStoryScenePanelParam param;
        public UiModStoryCtrl parent=>(UiModStoryCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryScenePanelParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryScenePanelView(uiHolder);
            model=new UiModStoryScenePanelModel();


            view.sub_SceneItem = new UiSceneItemCtrl();
            view.sub_SceneItem.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
        }

    }
    public partial class UiModStoryScenePanelModel:UiModel
    {
        
    }



    public partial class UiTextureItemParam:UiParam
    {
    }

    public partial class UiTextureItemView:UiView
    {

            public GameObject go_textureItem;
            public Btn btn_item;
            public Sta sta_item;
            public Img img_;
            public Txt txt_name;
        public UiTextureItemView(UiHolder uiHolder):base(uiHolder)
        {

            go_textureItem = uiHolder.elementTrsLst[0].gameObject;
            btn_item = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            sta_item = uiHolder.elementTrsLst[2].GetComponent<Sta>();
            img_ = uiHolder.elementTrsLst[3].GetComponent<Img>();
            txt_name = uiHolder.elementTrsLst[4].GetComponent<Txt>();
        }

    }
    public partial class UiTextureItemCtrl:UiCtrl
    {
        public UiTextureItemView view;
        public UiTextureItemModel model;
        public UiTextureItemParam param;
        public UiModStoryMaterialPanelTextureCtrl parent=>(UiModStoryMaterialPanelTextureCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiTextureItemParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiTextureItemView(uiHolder);
            model=new UiTextureItemModel();


        }

    }
    public partial class UiTextureItemModel:UiModel
    {
        
    }

    public partial class UiAnimTypeItemParam:UiParam
    {
    }

    public partial class UiAnimTypeItemView:UiView
    {

            public GameObject go_animTypeItem;
            public Btn btn_item;
            public Sta sta_item;
            public Img img_;
            public Txt txt_name;
        public UiAnimTypeItemView(UiHolder uiHolder):base(uiHolder)
        {

            go_animTypeItem = uiHolder.elementTrsLst[0].gameObject;
            btn_item = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            sta_item = uiHolder.elementTrsLst[2].GetComponent<Sta>();
            img_ = uiHolder.elementTrsLst[3].GetComponent<Img>();
            txt_name = uiHolder.elementTrsLst[4].GetComponent<Txt>();
        }

    }
    public partial class UiAnimTypeItemCtrl:UiCtrl
    {
        public UiAnimTypeItemView view;
        public UiAnimTypeItemModel model;
        public UiAnimTypeItemParam param;
        public UiModStoryMaterialPanelTextureCtrl parent=>(UiModStoryMaterialPanelTextureCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiAnimTypeItemParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiAnimTypeItemView(uiHolder);
            model=new UiAnimTypeItemModel();


        }

    }
    public partial class UiAnimTypeItemModel:UiModel
    {
        
    }
    public partial class UiModStoryMaterialPanelTextureParam:UiParam
    {
    }

    public partial class UiModStoryMaterialPanelTextureView:UiView
    {

            public GameObject go_items;
            public ScrView scr_items;
            public Img img_tex;
            public Btn btn_play;
            public Ipt ipt_intervalSet;
            public Btn btn_replace;
            public Btn btn_delete;
            public Ipt ipt_name;
            public GameObject go_anims;
            public ScrView scr_anims;
            public Txt txt_play;
            public Txt txt_intervalSet;
            public Txt txt_replace;
            public Txt txt_delete;
            public Txt txt_name;
            public GameObject go_textureItem;
            public UiTextureItemCtrl sub_TextureItem;
            public GameObject go_animTypeItem;
            public UiAnimTypeItemCtrl sub_AnimTypeItem;
        public UiModStoryMaterialPanelTextureView(UiHolder uiHolder):base(uiHolder)
        {

            go_items = uiHolder.elementTrsLst[0].gameObject;
            scr_items = uiHolder.elementTrsLst[1].GetComponent<ScrView>();
            img_tex = uiHolder.elementTrsLst[2].GetComponent<Img>();
            btn_play = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            ipt_intervalSet = uiHolder.elementTrsLst[4].GetComponent<Ipt>();
            btn_replace = uiHolder.elementTrsLst[5].GetComponent<Btn>();
            btn_delete = uiHolder.elementTrsLst[6].GetComponent<Btn>();
            ipt_name = uiHolder.elementTrsLst[7].GetComponent<Ipt>();
            go_anims = uiHolder.elementTrsLst[8].gameObject;
            scr_anims = uiHolder.elementTrsLst[9].GetComponent<ScrView>();
            txt_play = uiHolder.elementTrsLst[10].GetComponent<Txt>();
            txt_intervalSet = uiHolder.elementTrsLst[11].GetComponent<Txt>();
            txt_replace = uiHolder.elementTrsLst[12].GetComponent<Txt>();
            txt_delete = uiHolder.elementTrsLst[13].GetComponent<Txt>();
            txt_name = uiHolder.elementTrsLst[14].GetComponent<Txt>();
            go_textureItem = uiHolder.elementTrsLst[15].gameObject;
            sub_TextureItem = (UiTextureItemCtrl) uiHolder.elementTrsLst[16].GetComponent<UiHolder>().ctrl;
            go_animTypeItem = uiHolder.elementTrsLst[17].gameObject;
            sub_AnimTypeItem = (UiAnimTypeItemCtrl) uiHolder.elementTrsLst[18].GetComponent<UiHolder>().ctrl;
        }

    }
    public partial class UiModStoryMaterialPanelTextureCtrl:UiCtrl
    {
        public UiModStoryMaterialPanelTextureView view;
        public UiModStoryMaterialPanelTextureModel model;
        public UiModStoryMaterialPanelTextureParam param;
        public UiModStoryMaterialPanelCtrl parent=>(UiModStoryMaterialPanelCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryMaterialPanelTextureParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryMaterialPanelTextureView(uiHolder);
            model=new UiModStoryMaterialPanelTextureModel();


            view.sub_TextureItem = new UiTextureItemCtrl();
            view.sub_TextureItem.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
            view.sub_AnimTypeItem = new UiAnimTypeItemCtrl();
            view.sub_AnimTypeItem.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
        }

    }
    public partial class UiModStoryMaterialPanelTextureModel:UiModel
    {
        
    }


    public partial class UiMaskItemParam:UiParam
    {
    }

    public partial class UiMaskItemView:UiView
    {

            public GameObject go_maskItem;
            public Btn btn_item;
            public Sta sta_item;
            public Img img_;
            public Txt txt_name;
        public UiMaskItemView(UiHolder uiHolder):base(uiHolder)
        {

            go_maskItem = uiHolder.elementTrsLst[0].gameObject;
            btn_item = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            sta_item = uiHolder.elementTrsLst[2].GetComponent<Sta>();
            img_ = uiHolder.elementTrsLst[3].GetComponent<Img>();
            txt_name = uiHolder.elementTrsLst[4].GetComponent<Txt>();
        }

    }
    public partial class UiMaskItemCtrl:UiCtrl
    {
        public UiMaskItemView view;
        public UiMaskItemModel model;
        public UiMaskItemParam param;
        public UiModStoryMaterialPanelMaskCtrl parent=>(UiModStoryMaterialPanelMaskCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiMaskItemParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiMaskItemView(uiHolder);
            model=new UiMaskItemModel();


        }

    }
    public partial class UiMaskItemModel:UiModel
    {
        
    }

    public partial class UiMaskTypeItemParam:UiParam
    {
    }

    public partial class UiMaskTypeItemView:UiView
    {

            public GameObject go_maskTypeItem;
            public Btn btn_item;
            public Sta sta_item;
            public Img img_;
            public Txt txt_name;
        public UiMaskTypeItemView(UiHolder uiHolder):base(uiHolder)
        {

            go_maskTypeItem = uiHolder.elementTrsLst[0].gameObject;
            btn_item = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            sta_item = uiHolder.elementTrsLst[2].GetComponent<Sta>();
            img_ = uiHolder.elementTrsLst[3].GetComponent<Img>();
            txt_name = uiHolder.elementTrsLst[4].GetComponent<Txt>();
        }

    }
    public partial class UiMaskTypeItemCtrl:UiCtrl
    {
        public UiMaskTypeItemView view;
        public UiMaskTypeItemModel model;
        public UiMaskTypeItemParam param;
        public UiModStoryMaterialPanelMaskCtrl parent=>(UiModStoryMaterialPanelMaskCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiMaskTypeItemParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiMaskTypeItemView(uiHolder);
            model=new UiMaskTypeItemModel();


        }

    }
    public partial class UiMaskTypeItemModel:UiModel
    {
        
    }
    public partial class UiModStoryMaterialPanelMaskParam:UiParam
    {
    }

    public partial class UiModStoryMaterialPanelMaskView:UiView
    {

            public Ipt ipt_name;
            public GameObject go_items;
            public ScrView scr_items;
            public Txt txt_usageExample;
            public Img img_tex;
            public Btn btn_replace;
            public Img img_1;
            public Img img_2;
            public Img img_3;
            public Img img_4;
            public Img img_5;
            public Img img_6;
            public Img img_7;
            public Img img_8;
            public Img img_9;
            public Btn btn_delete;
            public GameObject go_masks;
            public ScrView scr_masks;
            public Txt txt_name;
            public Txt txt_replace;
            public Txt txt_delete;
            public GameObject go_maskItem;
            public UiMaskItemCtrl sub_MaskItem;
            public GameObject go_maskTypeItem;
            public UiMaskTypeItemCtrl sub_MaskTypeItem;
        public UiModStoryMaterialPanelMaskView(UiHolder uiHolder):base(uiHolder)
        {

            ipt_name = uiHolder.elementTrsLst[0].GetComponent<Ipt>();
            go_items = uiHolder.elementTrsLst[1].gameObject;
            scr_items = uiHolder.elementTrsLst[2].GetComponent<ScrView>();
            txt_usageExample = uiHolder.elementTrsLst[3].GetComponent<Txt>();
            img_tex = uiHolder.elementTrsLst[4].GetComponent<Img>();
            btn_replace = uiHolder.elementTrsLst[5].GetComponent<Btn>();
            img_1 = uiHolder.elementTrsLst[6].GetComponent<Img>();
            img_2 = uiHolder.elementTrsLst[7].GetComponent<Img>();
            img_3 = uiHolder.elementTrsLst[8].GetComponent<Img>();
            img_4 = uiHolder.elementTrsLst[9].GetComponent<Img>();
            img_5 = uiHolder.elementTrsLst[10].GetComponent<Img>();
            img_6 = uiHolder.elementTrsLst[11].GetComponent<Img>();
            img_7 = uiHolder.elementTrsLst[12].GetComponent<Img>();
            img_8 = uiHolder.elementTrsLst[13].GetComponent<Img>();
            img_9 = uiHolder.elementTrsLst[14].GetComponent<Img>();
            btn_delete = uiHolder.elementTrsLst[15].GetComponent<Btn>();
            go_masks = uiHolder.elementTrsLst[16].gameObject;
            scr_masks = uiHolder.elementTrsLst[17].GetComponent<ScrView>();
            txt_name = uiHolder.elementTrsLst[18].GetComponent<Txt>();
            txt_replace = uiHolder.elementTrsLst[19].GetComponent<Txt>();
            txt_delete = uiHolder.elementTrsLst[20].GetComponent<Txt>();
            go_maskItem = uiHolder.elementTrsLst[21].gameObject;
            sub_MaskItem = (UiMaskItemCtrl) uiHolder.elementTrsLst[22].GetComponent<UiHolder>().ctrl;
            go_maskTypeItem = uiHolder.elementTrsLst[23].gameObject;
            sub_MaskTypeItem = (UiMaskTypeItemCtrl) uiHolder.elementTrsLst[24].GetComponent<UiHolder>().ctrl;
        }

    }
    public partial class UiModStoryMaterialPanelMaskCtrl:UiCtrl
    {
        public UiModStoryMaterialPanelMaskView view;
        public UiModStoryMaterialPanelMaskModel model;
        public UiModStoryMaterialPanelMaskParam param;
        public UiModStoryMaterialPanelCtrl parent=>(UiModStoryMaterialPanelCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryMaterialPanelMaskParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryMaterialPanelMaskView(uiHolder);
            model=new UiModStoryMaterialPanelMaskModel();


            view.sub_MaskItem = new UiMaskItemCtrl();
            view.sub_MaskItem.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
            view.sub_MaskTypeItem = new UiMaskTypeItemCtrl();
            view.sub_MaskTypeItem.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
        }

    }
    public partial class UiModStoryMaterialPanelMaskModel:UiModel
    {
        
    }
    public partial class UiModStoryMaterialPanelParam:UiParam
    {
    }

    public partial class UiModStoryMaterialPanelView:UiView
    {

            public UiModStoryMaterialPanelTextureCtrl sub_ModStoryMaterialPanelTexture;
            public UiModStoryMaterialPanelMaskCtrl sub_ModStoryMaterialPanelMask;
            public Btn btn_texture;
            public Sta sta_texture;
            public Btn btn_mask;
            public Sta sta_mask;
            public Img img_texture;
            public Txt txt_texture;
            public Img img_mask;
            public Txt txt_mask;
        public UiModStoryMaterialPanelView(UiHolder uiHolder):base(uiHolder)
        {

            sub_ModStoryMaterialPanelTexture = (UiModStoryMaterialPanelTextureCtrl) uiHolder.elementTrsLst[0].GetComponent<UiHolder>().ctrl;
            sub_ModStoryMaterialPanelMask = (UiModStoryMaterialPanelMaskCtrl) uiHolder.elementTrsLst[1].GetComponent<UiHolder>().ctrl;
            btn_texture = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            sta_texture = uiHolder.elementTrsLst[3].GetComponent<Sta>();
            btn_mask = uiHolder.elementTrsLst[4].GetComponent<Btn>();
            sta_mask = uiHolder.elementTrsLst[5].GetComponent<Sta>();
            img_texture = uiHolder.elementTrsLst[6].GetComponent<Img>();
            txt_texture = uiHolder.elementTrsLst[7].GetComponent<Txt>();
            img_mask = uiHolder.elementTrsLst[8].GetComponent<Img>();
            txt_mask = uiHolder.elementTrsLst[9].GetComponent<Txt>();
        }

    }
    public partial class UiModStoryMaterialPanelCtrl:UiCtrl
    {
        public UiModStoryMaterialPanelView view;
        public UiModStoryMaterialPanelModel model;
        public UiModStoryMaterialPanelParam param;
        public UiModStoryCtrl parent=>(UiModStoryCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryMaterialPanelParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryMaterialPanelView(uiHolder);
            model=new UiModStoryMaterialPanelModel();


            view.sub_ModStoryMaterialPanelTexture = new UiModStoryMaterialPanelTextureCtrl();
            view.sub_ModStoryMaterialPanelTexture.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
            view.sub_ModStoryMaterialPanelMask = new UiModStoryMaterialPanelMaskCtrl();
            view.sub_ModStoryMaterialPanelMask.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
        }

    }
    public partial class UiModStoryMaterialPanelModel:UiModel
    {
        
    }
    public partial class UiModStoryParam:UiParam
    {
    }

    public partial class UiModStoryView:UiView
    {

            public Txt txt_title;
            public Btn btn_back;
            public UiModStoryScenePanelCtrl sub_ModStoryScenePanel;
            public UiModStoryMaterialPanelCtrl sub_ModStoryMaterialPanel;
            public Btn btn_scene;
            public Sta sta_scene;
            public Btn btn_material;
            public Sta sta_material;
            public Txt txt_scene;
            public Txt txt_material;
        public UiModStoryView(UiHolder uiHolder):base(uiHolder)
        {

            txt_title = uiHolder.elementTrsLst[0].GetComponent<Txt>();
            btn_back = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            sub_ModStoryScenePanel = (UiModStoryScenePanelCtrl) uiHolder.elementTrsLst[2].GetComponent<UiHolder>().ctrl;
            sub_ModStoryMaterialPanel = (UiModStoryMaterialPanelCtrl) uiHolder.elementTrsLst[3].GetComponent<UiHolder>().ctrl;
            btn_scene = uiHolder.elementTrsLst[4].GetComponent<Btn>();
            sta_scene = uiHolder.elementTrsLst[5].GetComponent<Sta>();
            btn_material = uiHolder.elementTrsLst[6].GetComponent<Btn>();
            sta_material = uiHolder.elementTrsLst[7].GetComponent<Sta>();
            txt_scene = uiHolder.elementTrsLst[8].GetComponent<Txt>();
            txt_material = uiHolder.elementTrsLst[9].GetComponent<Txt>();
        }

    }
    public partial class UiModStoryCtrl:UiCtrl
    {
        public UiModStoryView view;
        public UiModStoryModel model;
        public UiModStoryParam param;
        

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryView(uiHolder);
            model=new UiModStoryModel();


            view.sub_ModStoryScenePanel = new UiModStoryScenePanelCtrl();
            view.sub_ModStoryScenePanel.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
            view.sub_ModStoryMaterialPanel = new UiModStoryMaterialPanelCtrl();
            view.sub_ModStoryMaterialPanel.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
        }

    }
    public partial class UiModStoryModel:UiModel
    {
        
    }
}
