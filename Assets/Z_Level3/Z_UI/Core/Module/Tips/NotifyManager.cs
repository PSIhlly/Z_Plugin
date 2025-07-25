using System;
using System.Collections;
using System.Collections.Generic;
using Ui;
using Ui.Notify;
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
    public class ChooseInfo
    {
        public string title;
        public Func<int, bool> func;
        public List<(string,Sprite)> items;
        public bool canClose;
        public int id;
    }
    public class MultipleChooseInfo
    {
        public string title;
        public int labCnt;
        public Func<List<string>, bool> func;
        public Dictionary<string,(Sprite,object)> sub;
        public bool canClose;
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
    public class InputAreaInfo
    {
        public string title;
        public Func<string,bool> func;
        public bool canClose;
        public int id;
    }

    public class NotifyManager : Z_Manager<NotifyManager>
    {
        public static int tipIdCnt;
        public static int chooseIdCnt;
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
        public void AddChoose(string title, bool canClose, Func<int, bool> func, List<(string, Sprite)> items)
        {
            var info = new ChooseInfo()
            {
                title = title,
                items = items,
                func = func,
                canClose= canClose,
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
                    chooseInfo = info
                });
            }
        }
        public void AddMultipleChoose(string title, bool canClose,int labCnt, Func<List<string>, bool> func, Dictionary<string, (Sprite, object)> sub)
        {
            var info = new MultipleChooseInfo()
            {
                labCnt= labCnt,
                title = title,
                sub = sub,
                func = func,
                canClose = canClose,
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
                    multipleChooseInfo = info
                });
            }
        }
        public void AddPopup(string title,string content, bool canClose,List<string>words,List<Func<bool>> funcs)
        {
            var info = new PopupInfo()
            {
                title=title,
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
        public void AddInputArea(string title, bool canClose, Func<string,bool> func)
        {
            var info = new InputAreaInfo()
            {
                title = title,
                canClose = canClose,
                func = func,
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
                    inputAreaInfo = info
                });
            }
        }
        
        public override void Init()
        {
        }
    }
}
