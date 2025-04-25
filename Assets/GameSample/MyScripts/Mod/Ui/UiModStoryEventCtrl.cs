using Form;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;
using UnityEngine;
using UnityEngine.UI;
using Z_Text;
using Z_Time;
using Z_Ui.Base;
using Z_Ui.Notify;

namespace Ui.ModStoryEvent
{
    public partial class UiModStoryEventParam
    {
        public Action<string> func;
    }
    public partial class UiModStoryEventModel
    {
        public string curLab;
        public string curSubLab;
        public string curItem;
        public Action<string> func;

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
        }
        public override void Close()
        {
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
            view.go_choose.SetActive(model.curItem !=null);
            labelCon.Clear();
            foreach (var name in EventForm.DatasByLab.Keys)
            {
                labelCon.Add(new UiLabelParam()
                {
                    name = name
                });
            }
            labelCon.Refresh();

            subLabelcon.Clear();
            if (model.curLab != null)
            {
                foreach (var name in EventForm.DatasBySublab.Keys)
                {
                    if(EventForm.DatasBySublab[name][0].lab==model.curLab)
                    {
                        subLabelcon.Add(new UiSubLabelParam()
                        {
                            name = name
                        });
                    }
                }
                
            }
            subLabelcon.Refresh();

            con.Clear();
            if (model.curSubLab != null)
            {
                foreach (var name in EventForm.DatasBySublab.Keys)
                {
                    if (EventForm.DatasBySublab[name][0].lab == model.curLab)
                    {
                        con.Add(new UiItemParam()
                        {
                            name = name
                        });
                    }
                }

            }
            con.Refresh();
        }
        public void SetCur(string lab = null, string subLab = null, string item=null)
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
                parent.SetCur(model.name);
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
            view.txt_.text = model.name==""? TextManager.instance.GetTxt("custom") : model.name;
            view.sta_sel.ChangeState(parent.model.curLab == model.name ? 1 : 0);
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
                parent.SetCur(parent.model.curLab, model.name);
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
            view.txt_.text = model.name == "" ? TextManager.instance.GetTxt("custom") : model.name;
            view.sta_sel.ChangeState(parent.model.curItem == model.name ? 1 : 0);
        }
    }


    public partial class UiItemParam
    {
        public EventForm.Data data;
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
                parent.SetCur(parent.model.curLab, model.id);
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
            view.sta_sel.ChangeState(parent.model.curItem == model.id ? 1 : 0);
        }
    }

}
