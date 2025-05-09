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
    

    public partial class UiPopupParam
    {
        public PopupInfo info;
    }
    public partial class UiPopupModel
    {
        public PopupInfo info;
    }

    public partial class UiPopupCtrl
    {

        UiContainer<UiSelectionCtrl> con;
        public override void OnCreate()
        {
            con = new UiContainer<UiSelectionCtrl>(view.sub_Selection.gameObject);
            view.btn_close.onClick.AddListener(() =>
            {
                Close();
            });
        }
        public override void Close()
        {
            base.Close();
            parent.RemovePopup(model.info.id);
        }
        public override void OnShow()
        {
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

            view.txt_content.text = model.info.content;
            view.go_close.SetActive(model.info.canClose);
            view.txt_title.text = model.info.title;

            for (int i = 0; i < model.info.selectionWords.Count; i++)
            {
                con.Add(new UiSelectionParam()
                {
                    name = model.info.selectionWords[i],
                    func = model.info.funcs[i],
                    id = i
                });
            }

            con.Refresh();
        }

    }

    public partial class UiSelectionParam
    {
        public Func<bool> func;
        public string name;
        public int id;
    }
    public partial class UiSelectionModel
    {
        public Func<bool> funcs;
        public int id;
    }
    public partial class UiSelectionCtrl
    {
        public override void OnCreate()
        {
            view.btn_.onClick.AddListener(() =>
            {
                if (model.funcs != null)
                {
                    if (model.funcs())
                    {
                        parent.Close();
                    }
                }
            });
        }
        public override void OnShow()
        {
            if (param != null)
            {
                model.funcs = param.func;
                view.txt_.text = param.name;
                model.id = param.id;
            }
        }
    }

}
