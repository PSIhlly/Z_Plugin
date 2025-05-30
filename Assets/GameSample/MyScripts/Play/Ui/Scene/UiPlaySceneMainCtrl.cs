using Form;
using System.Collections;
using System.Collections.Generic;
using Ui.ModSceneMenu;
using Ui.PlayData;
using Ui.PlaySceneMenu;
using UnityEngine;
using Z_Map;
using Z_ObjectAnimator.Base;
using Z_ObjectAnimator.Core;
using Z_Texture;
using Z_Ui;
using Z_Ui.Base;

namespace Ui.PlaySceneMain
{

    public partial class UiPlaySceneMainCtrl
    {
        public override void OnCreate()
        {
            view.btn_menu.onClick.AddListener(() =>
            {
                UiManager.instance.ShowUi<UiPlaySceneMenuCtrl>();
            });
            view.btn_data.onClick.AddListener(() =>
            {
                UiManager.instance.ShowUi<UiPlayDataCtrl>();
            });

        }


        public void Refresh()
        {
        }
    }


}
