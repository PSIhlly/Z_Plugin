using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Form;
using Item;
using Z_Texture;
using Z_Ui;

namespace Ui
{
    public partial class UiStartCtrl
    {
        public override void OnCreate()
        {
            base.OnCreate();
            view.btn_mod.onClick.AddListener(() =>
            {
                UiManager.instance.ShowUi<UiModCtrl>();
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
