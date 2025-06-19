/*using System.Collections;
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
                model.curUi = view.page_ModStoryOverview;
                Refresh();
            });
            view.btn_parameter.onClick.AddListener(() =>
            {
                model.curUi = view.page_ModStoryParameter;
                Refresh();
            });
            view.btn_character.onClick.AddListener(() =>
            {
                model.curUi = view.page_ModStoryCharacter;
                Refresh();
            }); 
            view.btn_item.onClick.AddListener(() =>
            {
                model.curUi = view.page_ModStoryItem;
                Refresh();
            });
            view.btn_mapObject.onClick.AddListener(() =>
            {
                model.curUi = view.page_ModStoryMapObject;
                Refresh();
            });
            view.btn_event.onClick.AddListener(() =>
            {
                model.curUi = view.page_ModStoryEvent;
                Refresh();
            });
            view.btn_map.onClick.AddListener(() =>
            {
                model.curUi = view.page_ModStoryMap;
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
            view.page_ModStoryOverview.SetActive(model.curUi == view.page_ModStoryOverview);
            view.page_ModStoryParameter.SetActive(model.curUi == view.page_ModStoryParameter);
            view.page_ModStoryCharacter.SetActive(model.curUi == view.page_ModStoryCharacter);
            view.page_ModStoryItem.SetActive(model.curUi == view.page_ModStoryItem);
            view.page_ModStoryMapObject.SetActive(model.curUi == view.page_ModStoryMapObject);
            view.page_ModStoryMap.SetActive(model.curUi == view.page_ModStoryMap);
        }

    }

}
*/