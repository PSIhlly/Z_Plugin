using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui;
namespace Ui
{
    public partial class UiModSceneMainCtrl
    {
        public override void OnCreate()
        {
            view.btn_showTool.onClick.AddListener(() =>
            {
                //Close();
            });
            view.btn_menu.onClick.AddListener(() =>
            {
                UiManager.instance.ShowUi<UiModSceneMenuCtrl>();
            });
        }
    }
}
