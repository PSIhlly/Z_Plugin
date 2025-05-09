using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;
using UnityEngine;
using UnityEngine.UI;
using Z_Time;
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
               if(model.info.func(view.ipt_.text))
                {
                    Close();
                }
            });
        }
        public override void Close()
        {
            base.Close();
            parent.RemoveInputArea(model.info.id);
        }
        public override void OnShow()
        {
            view.ipt_.Set("");
            if (param != null)
            {
                model.info = param.info;
            }
            Refresh();
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);

        }
        public void Refresh()
        {
            view.txt_title.text = model.info.title;
        }

    }

    

}
