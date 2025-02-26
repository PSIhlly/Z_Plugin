using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Form;
using Item;
using Z_Texture;
using Z_Ui.Base;
using Z_Ui;

namespace Ui
{
    public partial class UiModStoryScenePanelCtrl
    {
        UiScrViewContainer<UiSceneItemCtrl> con;

        public override void OnCreate()
        {
            con = new UiScrViewContainer<UiSceneItemCtrl>(view.go_sceneItem, view.scr_tt);
            
        }


        public override void OnShow()
        {
            Refresh();
        }
        public void Refresh()
        {
            con.Clear();
            for (int i = 0; i < 1; i++)
            {
                con.Add(new UiSceneItemParam()
                {
                    name = "scene1"
                });
            }
            con.Refresh();
        }



    }
    public partial class UiSceneItemParam
    {
        public string name;
    }
    public partial class UiSceneItemCtrl
    {
        public override void OnCreate()
        {
            view.btn_scene.onClick.AddListener(() =>
            {
                Main2SceneManager.instance.StartLoadSceneUgc("scene1");
                UiManager.instance.CloseUi<UiModStoryCtrl>();
            });
        }
        public override void OnShow()
        {
            view.txt_sceneName.text = param.name;
            Refresh();
        }
        public void Refresh()
        {
        }



    }
}

