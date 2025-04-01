using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Form;
using Item;
using Z_Texture;
using Z_Ui.Base;
using Z_Ui;
using Z_Map;

namespace Ui.ModStoryMaterial
{
    public partial class UiModStoryMaterialModel
    {
        public int select;
    
    }
        public partial class UiModStoryMaterialCtrl
    {
        public override void OnCreate()
        {
            view.btn_back.onClick.AddListener(() =>
            {
                Close();
            });
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

        public override void Close()
        {
            GameManager.instance.saveCtrl.SaveMaterial(ModManager.instance.GetStoryCoreFolder());
            base.Close();
        }
        public void Refresh()
        {
            view.sta_texture.ChangeState(model.select == 0 ? 1 : 0);
            view.sta_mask.ChangeState(model.select == 1 ? 1 : 0);
            view.page_ModStoryMaterialTexture.SetActive(model.select == 0);
            view.page_ModStoryMaterialMask.SetActive(model.select == 1);
        }

    }

}

