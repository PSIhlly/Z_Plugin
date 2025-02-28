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
    public partial class UiModStoryMaterialPanelModel
    {
        public int select;
    
    }
        public partial class UiModStoryMaterialPanelCtrl
    {
        public override void OnCreate()
        {
            view.btn_mask.onClick.AddListener(() =>
            {
                model.select = 1;
                Refresh();
            });
            view.btn_texture.onClick.AddListener(() =>
            {
                model.select = 0;
                Refresh();
            });
        }
        public override void OnShow()
        {
            Refresh();
        }
        public void Refresh()
        {
            view.sta_texture.ChangeState(model.select == 0 ? 1 : 0);
            view.sta_mask.ChangeState(model.select == 1 ? 1 : 0);

            view.sub_ModStoryMaterialPanelTexture.SetActive(model.select == 0);
            view.sub_ModStoryMaterialPanelMask.SetActive(model.select == 1);
        }


    }
}

