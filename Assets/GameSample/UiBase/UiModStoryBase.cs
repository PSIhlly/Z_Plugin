
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

    public partial class UiModStoryMaterialPanelParam:UiParam
    {
    }

    public partial class UiModStoryMaterialPanelView:UiView
    {

        public UiModStoryMaterialPanelView(UiHolder uiHolder):base(uiHolder)
        {

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
