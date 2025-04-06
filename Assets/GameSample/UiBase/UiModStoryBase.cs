
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;

namespace Ui.ModStory

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

    public partial class UiModStoryModulePanelParam:UiParam
    {
    }

    public partial class UiModStoryModulePanelView:UiView
    {

            public Btn btn_material;
            public Sta sta_material;
            public Btn btn_item;
            public Sta sta_item;
            public Btn btn_character;
            public Sta sta_character;
            public Btn btn_config;
            public Sta sta_config;
            public Txt txt_material;
            public Txt txt_item;
            public Txt txt_character;
            public Txt txt_config;
        public UiModStoryModulePanelView(UiHolder uiHolder):base(uiHolder)
        {

            btn_material = uiHolder.elementTrsLst[0].GetComponent<Btn>();
            sta_material = uiHolder.elementTrsLst[1].GetComponent<Sta>();
            btn_item = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            sta_item = uiHolder.elementTrsLst[3].GetComponent<Sta>();
            btn_character = uiHolder.elementTrsLst[4].GetComponent<Btn>();
            sta_character = uiHolder.elementTrsLst[5].GetComponent<Sta>();
            btn_config = uiHolder.elementTrsLst[6].GetComponent<Btn>();
            sta_config = uiHolder.elementTrsLst[7].GetComponent<Sta>();
            txt_material = uiHolder.elementTrsLst[8].GetComponent<Txt>();
            txt_item = uiHolder.elementTrsLst[9].GetComponent<Txt>();
            txt_character = uiHolder.elementTrsLst[10].GetComponent<Txt>();
            txt_config = uiHolder.elementTrsLst[11].GetComponent<Txt>();
        }

    }
    public partial class UiModStoryModulePanelCtrl:UiCtrl
    {
        public UiModStoryModulePanelView view;
        public UiModStoryModulePanelModel model;
        public UiModStoryModulePanelParam param;
        public UiModStoryCtrl parent=>(UiModStoryCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryModulePanelParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryModulePanelView(uiHolder);
            model=new UiModStoryModulePanelModel();


        }

    }
    public partial class UiModStoryModulePanelModel:UiModel
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
            public UiModStoryModulePanelCtrl sub_ModStoryModulePanel;
            public Btn btn_play;
            public Btn btn_scene;
            public Sta sta_scene;
            public Btn btn_module;
            public Sta sta_module;
            public Txt txt_scene;
            public Txt txt_module;
        public UiModStoryView(UiHolder uiHolder):base(uiHolder)
        {

            txt_title = uiHolder.elementTrsLst[0].GetComponent<Txt>();
            btn_back = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            sub_ModStoryScenePanel = (UiModStoryScenePanelCtrl) uiHolder.elementTrsLst[2].GetComponent<UiHolder>().ctrl;
            sub_ModStoryModulePanel = (UiModStoryModulePanelCtrl) uiHolder.elementTrsLst[3].GetComponent<UiHolder>().ctrl;
            btn_play = uiHolder.elementTrsLst[4].GetComponent<Btn>();
            btn_scene = uiHolder.elementTrsLst[5].GetComponent<Btn>();
            sta_scene = uiHolder.elementTrsLst[6].GetComponent<Sta>();
            btn_module = uiHolder.elementTrsLst[7].GetComponent<Btn>();
            sta_module = uiHolder.elementTrsLst[8].GetComponent<Sta>();
            txt_scene = uiHolder.elementTrsLst[9].GetComponent<Txt>();
            txt_module = uiHolder.elementTrsLst[10].GetComponent<Txt>();
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
            view.sub_ModStoryModulePanel = new UiModStoryModulePanelCtrl();
            view.sub_ModStoryModulePanel.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
        }

    }
    public partial class UiModStoryModel:UiModel
    {
        
    }
}
