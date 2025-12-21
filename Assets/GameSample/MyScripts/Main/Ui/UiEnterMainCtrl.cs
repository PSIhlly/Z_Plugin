using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Form;
using Item;
using Z_Texture;
using Z_Ui;
using Ui.Start;
using Ui.Mod;
using Ui.Lounge;

namespace Ui.EnterMain
{
    public partial class UiEnterMainCtrl
    {
        public override void OnCreate()
        {
            view.btn_lounge.onClick.AddListener(()=>
            {
                UiManager.instance.ShowUi<UiLoungeCtrl>();
                Close();
            });
            view.btn_play.onClick.AddListener(() =>
            {
                UiManager.instance.ShowUi<UiStartCtrl>();
                Close();
            });

            view.btn_mod.onClick.AddListener(() =>
            {
                UiManager.instance.ShowUi<UiModCtrl>();
                Close();
            });
            view.btn_quit.onClick.AddListener(() =>
            {
#if UNITY_EDITOR
#else
                Application.Quit();
#endif
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
