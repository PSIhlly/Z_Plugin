using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Form;
using Item;
using Z_Texture;
using Z_Ui.Base;
using Z_Ui;
using Ui.ModStoryMaterial;
using Ui.ModStoryItem;

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
                UiManager.instance.ShowUi<UiModStoryItemCtrl>();

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

