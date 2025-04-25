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
    public partial class UiMultipleChooseParam
    {
        public MultipleChooseInfo info;
    }
    public partial class UiMultipleChooseModel
    {
        public MultipleChooseInfo info;
        public int curLab;
        public int curItem;
    }

    public partial class UiMultipleChooseCtrl
    {

        UiScrViewContainer<UiLabelCtrl> lableCon;
        UiScrViewContainer<UiSubItemCtrl> con;
        public override void OnCreate()
        {
            lableCon = new UiScrViewContainer<UiLabelCtrl>(view.go_label,view.scr_labels);
            con = new UiScrViewContainer<UiSubItemCtrl>(view.go_item,view.scr_items);
            view.btn_close.onClick.AddListener(() =>
            {
                Close();
            });
            view.btn_choose.onClick.AddListener(() =>
            {
                if(model.info.func((model.curLab, model.curItem)))
                {
                    Close();
                }
            });
        }
        public override void Close()
        {
            base.Close();
            parent.RemoveMultipleChoose(model.info.id);
        }
        public override void OnShow()
        {
            model.curLab = -1;
            model.curItem = -1;

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
            view.go_close.SetActive(model.info.canClose);
            view.go_choose.SetActive(model.curItem != -1&&model.curLab!=-1);
            
            lableCon.Clear();
            for (int i = 0; i < model.info.words.Count; i++)
            {
                lableCon.Add(new UiSubItemParam()
                {
                    name = model.info.words[i].Item1,
                    sprite = model.info.sprites[i].Item1,
                    id = i
                });
            }
            lableCon.Refresh();

            con.Clear();
            if (model.curLab!=-1)
            {
                for (int i = 0; i < model.info.words[model.curLab].Item2.Count; i++)
                {
                    con.Add(new UiSubItemParam()
                    {
                        name = model.info.words[model.curLab].Item2[i],
                        sprite = model.info.sprites[model.curLab].Item2[i],
                        id = i
                    });
                }
            }
            con.Refresh();
        }
        public void SetCur(int lab,int item)
        {
            model.curLab = lab;
            model.curItem = item;
            Refresh();
        }



    }
    public partial class UiLabelParam
    {
        public Sprite sprite;
        public string name;
        public int id;
    }
    public partial class UiLabelModel
    {
        public Sprite sprite;
        public string name;
        public int id;
    }
    public partial class UiLabelCtrl
    {
        public override void OnCreate()
        {
            view.btn_.onClick.AddListener(() =>
            {
                parent.SetCur(model.id,-1);
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
            view.txt_.text = model.name;
            view.img_.sprite = model.sprite;
            view.sta_sel.ChangeState(parent.model.curLab == model.id ? 1 : 0);
        }
    }


    public partial class UiSubItemParam
    {
        public Sprite sprite;
        public string name;
        public int id;
    }
    public partial class UiSubItemModel
    {
        public Sprite sprite;
        public string name;
        public int id;
    }
    public partial class UiSubItemCtrl
    {
        public override void OnCreate()
        {
            view.btn_.onClick.AddListener(() =>
            {
                parent.SetCur(parent.model.curLab,model.id);
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
            view.sta_sel.ChangeState(parent.model.curItem == model.id ? 1 : 0);
        }
    }

}
