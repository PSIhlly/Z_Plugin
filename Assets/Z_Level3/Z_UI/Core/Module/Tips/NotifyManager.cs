using System;
using System.Collections;
using System.Collections.Generic;
using Ui;
using UnityEngine;
using Z_DesignStyle;

namespace Z_Ui.Notify
{
    public class TipInfo
    {
        public float time;
        public string content;
        public int id;
    }
    public class PopupInfo
    {
        public string title;
        public string content;
        public List<Func<bool>> funcs;
        public List<string> selectionWords;
        public bool canClose;
        public int id;
    }

    public class NotifyManager : Z_Manager<NotifyManager>
    {
        public static int tipIdCnt;
        public static int popupIdCnt;
        public void AddTip(string content,float time=2)
       {
            var info = new TipInfo()
            {
                content = content,
                time = Time.time + time,
                id = tipIdCnt++
            };
            var ctrl = UiManager.instance.GetUi<UiNotifyCtrl>();
            if(ctrl!=null&&ctrl.isActive)
            {
                ctrl.Add(info);
            }else
            {
                UiManager.instance.ShowUi<UiNotifyCtrl>(new UiNotifyParam()
                {
                    tipInfo= info
                }) ;
            }
       }
        public void AddPopup(string content, bool canClose,List<string>words,List<Func<bool>> funcs)
        {
            var info = new PopupInfo()
            {
                content = content,
                canClose=canClose,
                selectionWords = words,
                funcs = funcs,
                id = popupIdCnt++
            };
            var ctrl = UiManager.instance.GetUi<UiNotifyCtrl>();
            if (ctrl != null && ctrl.isActive)
            {
                ctrl.Add(info);
            }
            else
            {
                UiManager.instance.ShowUi<UiNotifyCtrl>(new UiNotifyParam()
                {
                    popupInfo = info
                });
            }
        }

        public override void Init()
        {
        }
    }
}
