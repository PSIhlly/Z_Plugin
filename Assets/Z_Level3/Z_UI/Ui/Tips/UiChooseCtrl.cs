using System;
using System.Collections;
using System.Collections.Generic;
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
        public int cur;
    }

    public partial class UiChooseCtrl
    {

        UiScrViewContainer<UiItemCtrl> con;
        public override void OnCreate()
        {
            con = new UiScrViewContainer<UiItemCtrl>(view.go_item,view.scr_items);
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
            model.cur = -1;

            if (param != null)
            {
                model.info = param.info;
            }
            Refresh();

            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);

        }
        public void Refresh()
        {
            con.Clear();
            view.txt_title.text = model.info.title;
            view.go_close.SetActive(model.info.canClose);
            view.go_choose.SetActive(model.cur!=-1);

            for (int i = 0; i < model.info.items.Count; i++)
            {
                con.Add(new UiItemParam()
                {
                    name = model.info.items[i].Item1,
                    sprite = model.info.items[i].Item2,
                    id = i
                });
            }
            con.Refresh();
        }
        public void SetCur(int id)
        {
            model.cur = id;
            Refresh();
        }



    }

    public partial class UiItemParam
    {
        public Sprite sprite;
        public string name;
        public int id;
    }
    public partial class UiItemModel
    {
        public Sprite sprite;
        public string name;
        public int id;
    }
    public partial class UiItemCtrl
    {
        public override void OnCreate()
        {
            view.btn_.onClick.AddListener(() =>
            {
                parent.SetCur(model.id);
            });
        }
        public override void OnShow()
        {
            if (param != null)
            {
                model.sprite = param.sprite;
                model.name = param.name;
                model.id = param.id;
            }
            Refresh();
        }
        public void Refresh()
        {
            view.txt_.text= model.name;
            view.img_.sprite = model.sprite;
            view.sta_sel.ChangeState(parent.model.cur == model.id ? 1 : 0);
        }
    }

}
