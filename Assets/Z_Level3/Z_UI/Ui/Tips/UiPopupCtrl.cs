using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui;
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
        RectTransform scrollRectTransform;
        float defaultScrollHeight;
        public override void OnCreate()
        {
            con = new UiContainer<UiSelectionCtrl>(this, view.sub_Selection.gameObject);
            scrollRectTransform = view.scr_.GetComponent<RectTransform>();
            defaultScrollHeight = scrollRectTransform.sizeDelta.y;
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

        }
        public void Refresh()
        {
            con.Clear();
            view.scr_.verticalNormalizedPosition = 0;
            view.scr_.enabled = model.info.enableScr;
            view.txt_content.text = model.info.content;
            view.go_close.SetActive(model.info.canClose);
            view.txt_title.text = model.info.title;
            if(model.info.selectionWords!=null)
            {
                for (int i = 0; i < model.info.selectionWords.Count; i++)
                {
                    con.Add(new UiSelectionParam()
                    {
                        name = model.info.selectionWords[i],
                        func = model.info.funcs[i],
                        id = i
                    });
                }
            }
            con.Refresh();
            UiManager.Rebuild(gameObject, true);

            var size = scrollRectTransform.sizeDelta;
            size.y = view.scr_.content.rect.height > 200f
                ? Mathf.Min(view.scr_.content.rect.height + 50f, 500f)
                : defaultScrollHeight;
            scrollRectTransform.sizeDelta = size;

            UiManager.Rebuild(gameObject, true);
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
