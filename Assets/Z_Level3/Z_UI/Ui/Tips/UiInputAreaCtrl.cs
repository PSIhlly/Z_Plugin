using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Z_Time;
using Z_Ui;
using Z_Ui.Base;
using Z_Ui.Notify;

namespace Ui.Notify
{


    public partial class UiInputAreaParam
    {
        public InputAreaInfo info;
    }
    public partial class UiInputAreaModel
    {
        public InputAreaInfo info;
    }

    public partial class UiInputAreaCtrl
    {

        public override void OnCreate()
        {
            view.btn_close.onClick.AddListener(() =>
            {
                Close();
            });
            view.btn_.onClick.AddListener(() =>
            {
                if (model.info.func(view.ipt_.text))
                {
                    Close();
                }
            });
            view.ipt_.onValueChanged.AddListener((v) =>
            {
                Refresh();
            });
        }
        public override void Close()
        {
            base.Close();
            parent.RemoveInputArea(model.info.id);
        }
        public override void OnShow()
        {
            if (param != null)
            {
                model.info = param.info;
            }
            view.ipt_.Set(param.info.defaultText);
            
            Refresh();

        }
        public void Refresh()
        {
            view.txt_title.text = model.info.title;
            UiManager.Rebuild(gameObject, true);
        }

    }



}
