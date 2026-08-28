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

    public partial class UiTipModel
    {
        public Timer removeTimer;
        public TipInfo info;
    }
    public partial class UiTipParam
    {
        public TipInfo info;
    }

    public partial class UiTipCtrl
    {
        public override void OnShow()
        {
            if (param != null)
            {
                view.txt_.text = param.info.content;
                model.info = param.info;
            }
            TimeManager.instance.CancelTimer(model.removeTimer);
            model.removeTimer = TimeManager.instance.StartTimer(param.info.time - Time.time, 0, () =>
            {
                parent.RemoveTip(model.info.id);
                return true;
            }, uiHolder);

            UiManager.Rebuild(gameObject,true);
        }

    }

}
