
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
    public partial class UiModStoryParam:UiParam
    {
    }

    public partial class UiModStoryView:UiView
    {

            public Btn btn_back;
            public Btn btn_play;
            public UiModStoryScenePanelCtrl sub_ModStoryScenePanel;
            public Txt txt_title;
            public Btn btn_overview;
            public Sta sta_overview;
            public Btn btn_parameter;
            public Sta sta_parameter;
            public Btn btn_character;
            public Sta sta_character;
            public Btn btn_item;
            public Sta sta_item;
            public Btn btn_mapObject;
            public Sta sta_mapObject;
            public Btn btn_event;
            public Sta sta_event;
            public Btn btn_scene;
            public Sta sta_scene;
            public Txt txt_overview;
            public Txt txt_parameter;
            public Txt txt_character;
            public Txt txt_item;
            public Txt txt_mapObject;
            public Txt txt_event;
            public Txt txt_scene;
        public UiModStoryView(UiHolder uiHolder):base(uiHolder)
        {

            btn_back = uiHolder.elementTrsLst[0].GetComponent<Btn>();
            btn_play = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            sub_ModStoryScenePanel = (UiModStoryScenePanelCtrl) uiHolder.elementTrsLst[2].GetComponent<UiHolder>().ctrl;
            txt_title = uiHolder.elementTrsLst[3].GetComponent<Txt>();
            btn_overview = uiHolder.elementTrsLst[4].GetComponent<Btn>();
            sta_overview = uiHolder.elementTrsLst[5].GetComponent<Sta>();
            btn_parameter = uiHolder.elementTrsLst[6].GetComponent<Btn>();
            sta_parameter = uiHolder.elementTrsLst[7].GetComponent<Sta>();
            btn_character = uiHolder.elementTrsLst[8].GetComponent<Btn>();
            sta_character = uiHolder.elementTrsLst[9].GetComponent<Sta>();
            btn_item = uiHolder.elementTrsLst[10].GetComponent<Btn>();
            sta_item = uiHolder.elementTrsLst[11].GetComponent<Sta>();
            btn_mapObject = uiHolder.elementTrsLst[12].GetComponent<Btn>();
            sta_mapObject = uiHolder.elementTrsLst[13].GetComponent<Sta>();
            btn_event = uiHolder.elementTrsLst[14].GetComponent<Btn>();
            sta_event = uiHolder.elementTrsLst[15].GetComponent<Sta>();
            btn_scene = uiHolder.elementTrsLst[16].GetComponent<Btn>();
            sta_scene = uiHolder.elementTrsLst[17].GetComponent<Sta>();
            txt_overview = uiHolder.elementTrsLst[18].GetComponent<Txt>();
            txt_parameter = uiHolder.elementTrsLst[19].GetComponent<Txt>();
            txt_character = uiHolder.elementTrsLst[20].GetComponent<Txt>();
            txt_item = uiHolder.elementTrsLst[21].GetComponent<Txt>();
            txt_mapObject = uiHolder.elementTrsLst[22].GetComponent<Txt>();
            txt_event = uiHolder.elementTrsLst[23].GetComponent<Txt>();
            txt_scene = uiHolder.elementTrsLst[24].GetComponent<Txt>();
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
        }

    }
    public partial class UiModStoryModel:UiModel
    {
        
    }
}
