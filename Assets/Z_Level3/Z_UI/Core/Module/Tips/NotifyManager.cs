using System;
using System.Collections;
using System.Collections.Generic;
using Ui;
using Ui.Notify;
using UnityEngine;
using Z_DesignStyle;

namespace Z_Ui.Notify
{
    public class EntryItem
    {
        public string content;
        public Sprite sprite;
        public EntryItem parent;
        public int deepth;
        public int id = 1;
        public Dictionary<string, EntryItem> subs=new Dictionary<string, EntryItem>();
        public void Add(string name,Sprite icon=null,int id=0)
        {
            subs[name] = new EntryItem()
            {
                content = name,
                sprite = icon,
                parent= this,
                deepth= deepth+1,
                id= id
            };
        }
        public bool IsChildOf(EntryItem item)
        {
            var tmp = this;
            while(tmp!=null)
            {
                if (tmp == item)
                    return true;
                tmp = tmp.parent;
            }
            return false;
        }
    }
    public class TipInfo
    {
        public float time;
        public string content;
        public int id;
    }
    public class ChooseInfo
    {
        public string title;
        public Func<EntryItem, bool> func;
        public EntryItem items;
        public bool canClose;
        public int id;
    }
    public class MultipleChooseInfo
    {
        public string title;
        public Func<EntryItem, bool> func;
        public EntryItem item;
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
        public void AddChoose(string title, bool canClose, Func<EntryItem, bool> func, EntryItem items)
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
        public void AddMultipleChoose(string title, bool canClose,Func<EntryItem, bool> func, EntryItem items)
        {
            var info = new MultipleChooseInfo()
            {
                title = title,
                item = items,
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
