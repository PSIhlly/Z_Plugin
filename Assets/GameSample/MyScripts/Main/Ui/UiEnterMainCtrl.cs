using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Form;
using Item;
using Z_Texture;
using Z_Ui;

namespace Ui
{
    public partial class UiEnterMainCtrl
    {
        public override void OnCreate()
        {
            view.btn_warRoom.onClick.AddListener(()=>
            {
                UiManager.instance.ShowUi<UiWarRoomCtrl>();
                Close();
            });
            view.btn_start.onClick.AddListener(() =>
            {
                UiManager.instance.ShowUi<UiStartCtrl>();
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
