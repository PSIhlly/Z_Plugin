using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Form;
using Item;
using Z_Texture;
using Z_Ui.Base;
using Z_Ui;
using Ui.ModStoryMaterial;
using Ui.ModStoryObject;
using Ui.ModStoryCharacter;
using Ui.ModStoryConfig;
using Ui.ModStoryEvent;

namespace Ui.ModStory
{
    public partial class UiModStoryModulePanelCtrl
    {

        public override void OnCreate()
        {
            view.btn_material.onClick.AddListener(() =>
            {
                UiManager.instance.ShowUi<UiModStoryMaterialCtrl>();

            });
            view.btn_item.onClick.AddListener(() =>
            {
                UiManager.instance.ShowUi<UiModStoryObjectCtrl>();

            });
            view.btn_character.onClick.AddListener(() =>
            {
                UiManager.instance.ShowUi<UiModStoryCharacterCtrl>();

            });
            view.btn_config.onClick.AddListener(() =>
            {
                UiManager.instance.ShowUi<UiModStoryConfigCtrl>();

            });
            view.btn_event.onClick.AddListener(() =>
            {
                UiManager.instance.ShowUi<UiModStoryEventCtrl>();

            });
        }


        public override void OnShow()
        {
            Refresh();
        }
        public void Refresh()
        {
           


        }



    }
   
}

