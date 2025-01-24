
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
        public UiModStoryCtrl parent=>(UiModStoryCtrl)uiHolder.parent.ctrl;

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
    public partial class UiModStoryParam:UiParam
    {
    }

    public partial class UiModStoryView:UiView
    {

            public Txt txt_title;
            public Btn btn_back;
            public ScrView scr_tt;
            public GameObject go_sceneItem;
            public UiSceneItemCtrl sub_SceneItem;
        public UiModStoryView(UiHolder uiHolder):base(uiHolder)
        {

            txt_title = uiHolder.elementTrsLst[0].GetComponent<Txt>();
            btn_back = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            scr_tt = uiHolder.elementTrsLst[2].GetComponent<ScrView>();
            go_sceneItem = uiHolder.elementTrsLst[3].gameObject;
            sub_SceneItem = (UiSceneItemCtrl) uiHolder.elementTrsLst[4].GetComponent<UiHolder>().ctrl;
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


            view.sub_SceneItem = new UiSceneItemCtrl();
            view.sub_SceneItem.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
        }

    }
    public partial class UiModStoryModel:UiModel
    {
        
    }
}
