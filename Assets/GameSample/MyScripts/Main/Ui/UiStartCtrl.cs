using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Form;
using Item;
using Z_Texture;
using Z_Ui;
using Ui.Mod;
using Ui.EnterMain;
using Ui.Story;

namespace Ui.Start
{
    public partial class UiStartCtrl
    {
        public override void OnCreate()
        {
            base.OnCreate();
            view.btn_ori.onClick.AddListener(() =>
            {
                UiManager.instance.ShowUi<UiStoryCtrl>(new UiStoryParam()
                {
                    datas = new List<StoryForm.Data>(StoryForm.DataById.Values)
                });
            });
            view.btn_mod.onClick.AddListener(() =>
            {
                UiManager.instance.ShowUi<UiStoryCtrl>(new UiStoryParam()
                {
                    datas = new List<StoryForm.Data>(StoryForm.DataById.Values)
                });
                Close();
            });
            view.btn_back.onClick.AddListener(() =>
            {
                UiManager.instance.ShowUi<UiEnterMainCtrl>();
                Close();
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
