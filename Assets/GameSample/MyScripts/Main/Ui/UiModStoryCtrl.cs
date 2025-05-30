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
            view.btn_play.onClick.AddListener(() =>
            {
                Main2StoryManager.instance.UnloadStoryUgc();
                Main2StoryManager.instance.StartLoadStoryPlay(ModManager.instance.GetFolderName(), true);
                Close();
            });


            view.btn_overview.onClick.AddListener(() =>
            {
                //model.curUi = ;
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
            //model.curUi = view.sub;
            Refresh();
        }
        public void Refresh()
        {

        }

    }

}
