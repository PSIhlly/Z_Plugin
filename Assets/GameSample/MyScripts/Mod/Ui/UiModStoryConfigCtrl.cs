using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Form;
using Item;
using Z_Texture;
using Z_Ui.Base;
using Z_Ui;
using Z_Map;

namespace Ui.ModStoryConfig
{
    public partial class UiModStoryConfigModel
    {
        public int select;

    }
    public partial class UiModStoryConfigCtrl
    {
        public override void OnCreate()
        {
            view.btn_back.onClick.AddListener(() =>
            {
                Close();
            });
            view.btn_init.onClick.AddListener(() =>
            {
                model.select = 0;
                Refresh();
            });
            view.btn_global.onClick.AddListener(() =>
            {
                model.select = 1;
                Refresh();
            });
        }
        public override void OnShow()
        {
            Refresh();
        }

        public override void Close()
        {
            GameManager.instance.saveCtrl.SaveConfig(ModManager.instance.GetStoryCoreFolder());
            base.Close();
        }
        public void Refresh()
        {
            view.sta_init.ChangeState(model.select == 0 ? 1 : 0);
            view.sta_global.ChangeState(model.select == 1 ? 1 : 0);
            view.page_ModStoryConfigInit.SetActive(model.select == 0);
            view.page_ModStoryConfigGlobal.SetActive(model.select == 1);
        }
    }

}

