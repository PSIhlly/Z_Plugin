using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEngine;
using UnityEngine.UI;
using Z_Time;
using Z_Ui.Base;
using Z_Ui.Notify;

namespace Ui.Notify
{
    public partial class UiChooseParam
    {
        public ChooseInfo info;
    }
    public partial class UiChooseModel
    {
        public ChooseInfo info;
        public EntryItem cur;
    }

    public partial class UiChooseCtrl
    {

        UiScrViewContainer<UiItemCtrl> con;
        public override void OnCreate()
        {
            con = new UiScrViewContainer<UiItemCtrl>(this, view.go_item, view.scr_items);
            view.btn_close.onClick.AddListener(() =>
            {
                Close();
            });
            view.btn_choose.onClick.AddListener(() =>
            {
                if(model.info.func(model.cur))
                {
                    Close();
                }
            });
        }
        public override void Close()
        {
            base.Close();
            parent.RemoveChoose(model.info.id);
        }
        public override void OnShow()
        {
            if (param != null)
            {
                model.info = param.info;
                model.cur = param.info.items;
            }
            Refresh();

            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);

        }
        public void Refresh()
        {
            con.Clear();
            view.txt_title.text = model.info.title;
            view.go_close.SetActive(model.info.canClose);
            view.go_choose.SetActive(model.cur.deepth == 1);

            foreach (var item in model.info.items.subs.Values)
            {
                con.Add(new UiItemParam()
                {
                    cur= item
                });
            }
            con.Refresh();
        }
        public void SetCur(EntryItem item)
        {
            model.cur = item;
            Refresh();
        }



    }

    public partial class UiItemParam
    {
        public EntryItem cur;
    }
    public partial class UiItemModel
    {
        public EntryItem cur;
    }
    public partial class UiItemCtrl
    {
        public override void OnCreate()
        {
            view.btn_.onClick.AddListener(() =>
            {
                parent.SetCur(model.cur);
            });
        }
        public override void OnShow()
        {
            if (param != null)
            {
                model.cur = param.cur;
            }
            Refresh();
        }
        public void Refresh()
        {
            view.txt_.text= model.cur.content;
            view.img_.sprite = model.cur.sprite;
            view.sta_sel.ChangeState(parent.model.cur == model.cur ? 1 : 0);
        }
    }

}
