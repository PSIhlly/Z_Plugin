using Form;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;
using Ui.ModStoryEventCmds;
using UnityEngine;
using UnityEngine.UI;
using Z_Text;
using Z_Time;
using Z_Ui;
using Z_Ui.Base;
using Z_Ui.Notify;

namespace Ui.ModStoryEvent
{
    public partial class UiModStoryEventParam
    {
        public Action<EventForm.Data> func;
    }
    public partial class UiModStoryEventModel
    {
        public string curLab;
        public string curSubLab;
        public EventForm.Data curItem;
        public Action<EventForm.Data> func;

    }

    public partial class UiModStoryEventCtrl
    {

        UiScrViewContainer<UiLabelCtrl> labelCon;
        UiScrViewContainer<UiSubLabelCtrl> subLabelcon;
        UiScrViewContainer<UiItemCtrl> con;
        public override void OnCreate()
        {
            labelCon = new UiScrViewContainer<UiLabelCtrl>(view.go_label, view.scr_labels);
            subLabelcon = new UiScrViewContainer<UiSubLabelCtrl>(view.go_subLabel, view.scr_subLabels);
            con = new UiScrViewContainer<UiItemCtrl>(view.go_item, view.scr_items);
            view.btn_close.onClick.AddListener(() =>
            {
                Close();
            });
            view.btn_choose.onClick.AddListener(() =>
            {
                model.func(model.curItem);
                Close();
            });
            view.btn_edit.onClick.AddListener(() =>
            {
                UiManager.instance.ShowUi<UiModStoryEventCmdsCtrl>(new UiModStoryEventCmdsParam()
                {
                    data = model.curItem,
                    onClose = () =>
                     {
                         SetCur(model.curItem.lab, model.curItem.subLab, model.curItem);
                         Refresh();
                     }
                });
            });
        }
        public override void Close()
        {
            GameManager.instance.saveCtrl.SaveEvent(ModManager.instance.GetStoryCoreFolder());
            base.Close();
        }
        public override void OnShow()
        {
            model.curLab = null;
            model.curItem = null;

            if (param != null)
            {
                model.func = param.func;
            }
            Refresh();

            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);

        }
        public void Refresh()
        {
            view.go_choose.SetActive(model.curItem != null && model.func != null);
            view.sta_show.ChangeState(model.curItem != null ? 1 : 0);
            labelCon.Clear();
            foreach (var name in EventForm.DatasByLab.Keys)
            {
                labelCon.Add(new UiLabelParam()
                {
                    name = name
                });
            }
            labelCon.Add(new UiLabelParam()
            {
                name = null
            });
            labelCon.Refresh();

            subLabelcon.Clear();
            if (model.curLab != null)
            {
                foreach (var lst in EventForm.DatasByLab.Values)
                {
                    subLabelcon.Add(new UiSubLabelParam()
                    {
                        name = lst[0].subLab
                    });
                }
                subLabelcon.Add(new UiSubLabelParam()
                {
                    name = null
                });
            }

            subLabelcon.Refresh();

            con.Clear();
            if (model.curSubLab != null)
            {
                foreach (var data in EventForm.DatasByLabSublab[(model.curLab, model.curSubLab)])
                {
                    con.Add(new UiItemParam()
                    {
                        data = data
                    });
                }
                con.Add(new UiItemParam()
                {
                    data = null
                });
            }
            con.Refresh();

            if (model.curItem != null)
            {
                view.txt_content.text = model.curItem.name;
            }
        }
        public void SetCur(string lab = null, string subLab = null, EventForm.Data item = null)
        {
            model.curLab = lab;
            model.curSubLab = subLab;
            model.curItem = item;
            Refresh();
        }



    }
    public partial class UiLabelParam
    {
        public string name;
    }
    public partial class UiLabelModel
    {
        public string name;
    }
    public partial class UiLabelCtrl
    {
        public override void OnCreate()
        {
            view.btn_.onClick.AddListener(() =>
            {
                if (model.name == null)
                {
                    ModManager.instance.assetCtrl.CreateEvent("", "", "");
                    parent.Refresh();
                }
                else
                {
                    parent.SetCur(model.name);
                }
            });
        }
        public override void OnShow()
        {
            if (param != null)
            {
                model.name = param.name;
            }
            Refresh();
        }
        public void Refresh()
        {
            if (model.name == null)
            {

                view.txt_.text = TextManager.instance.GetTxt("new");
            }
            else {
                view.txt_.text = model.name == "" ? TextManager.instance.GetTxt("custom") : model.name;
            view.sta_sel.ChangeState(model.name != null && parent.model.curLab == model.name ? 1 : 0);
            }
        }
    }


    public partial class UiSubLabelParam
    {
        public string name;
    }
    public partial class UiSubLabelModel
    {
        public string name;
    }
    public partial class UiSubLabelCtrl
    {
        public override void OnCreate()
        {
            view.btn_.onClick.AddListener(() =>
            {
                if (model.name == null)
                {
                    ModManager.instance.assetCtrl.CreateEvent("", parent.model.curLab, "");
                    parent.Refresh();
                }
                else
                {
                    parent.SetCur(parent.model.curLab, model.name);

                }
            });
        }
        public override void OnShow()
        {
            if (param != null)
            {
                model.name = param.name;
            }
            Refresh();
        }
        public void Refresh()
        {
            if(model.name==null)
            {
                view.txt_.text = TextManager.instance.GetTxt("new");
            }
            else
            {
                view.txt_.text = model.name == "" ? TextManager.instance.GetTxt("custom") : model.name;
                view.sta_sel.ChangeState(model.name != null && parent.model.curSubLab == model.name ? 1 : 0);

            }
        }
    }


    public partial class UiItemParam
    {
        public EventForm.Data data;
    }
    public partial class UiItemModel
    {
        public EventForm.Data data;

    }
    public partial class UiItemCtrl
    {
        public override void OnCreate()
        {
            view.btn_.onClick.AddListener(() =>
            {
                if (model.data == null)
                {
                    ModManager.instance.assetCtrl.CreateEvent("", parent.model.curLab, parent.model.curSubLab);
                    parent.Refresh();
                }
                else
                {
                    parent.SetCur(parent.model.curLab, parent.model.curSubLab, model.data);
                }
            });
        }
        public override void OnShow()
        {
            if (param != null)
            {
                model.data = param.data;
            }
            Refresh();
        }
        public void Refresh()
        {
            if (model.data == null)
            {
                view.txt_.text = TextManager.instance.GetTxt("new");
            }
            else
            {
                view.txt_.text = model.data == null ? TextManager.instance.GetTxt("custom") : model.data.name;
                view.sta_sel.ChangeState(model.data != null && parent.model.curItem == model.data ? 1 : 0);
            }
        }
    }

}
