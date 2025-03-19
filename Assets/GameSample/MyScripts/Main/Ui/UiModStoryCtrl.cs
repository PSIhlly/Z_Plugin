using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Form;
using Item;
using Z_Texture;
using Z_Ui.Base;
using Z_Ui;
using Z_DataSystem;
using Ui.Mod;

namespace Ui.ModStory
{
    public partial class UiModStoryModel
    {
        public UiCtrl curUi;
    }
    public partial class UiModStoryCtrl
    {

        public override void OnCreate()
        {
            view.btn_back.onClick.AddListener(() =>
            {
                Main2StoryManager.instance.UnloadStoryUgc();
                UiManager.instance.ShowUi<UiModCtrl>();
                Close();
            });

            view.btn_module.onClick.AddListener(() =>
            {
                model.curUi = view.sub_ModStoryModulePanel;
                Refresh();
            });
            view.btn_scene.onClick.AddListener(() =>
            {
                model.curUi = view.sub_ModStoryScenePanel;
                Refresh();
            });
        }



        public override void OnShow()
        {
            model.curUi = view.sub_ModStoryScenePanel;
            Refresh();
        }
        public void Refresh()
        {
            view.sta_scene.ChangeState(model.curUi == view.sub_ModStoryScenePanel ? 1 : 0);
            view.sub_ModStoryScenePanel.gameObject.SetActive(model.curUi == view.sub_ModStoryScenePanel);

            view.sta_module.ChangeState(model.curUi == view.sub_ModStoryModulePanel ? 1 : 0);
            view.sub_ModStoryModulePanel.gameObject.SetActive(model.curUi == view.sub_ModStoryModulePanel);
        }

    }

}
