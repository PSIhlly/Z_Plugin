using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.ConstrainedExecution;
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
        public EntryItem sel;
        public int deepth;
    }

    public partial class UiMultipleChooseCtrl
    {

        UiContainer<UiColumnCtrl> colCon;
        UiScrViewContainer<UiSubItemCtrl> itemCon;
        public override void OnCreate()
        {
            colCon = new UiContainer<UiColumnCtrl>(this, view.go_column);
            itemCon = new UiScrViewContainer<UiSubItemCtrl>(this, view.go_subItem, view.scr_subItems);
            view.btn_close.onClick.AddListener(() =>
            {
                Close();
            });
            view.btn_choose.onClick.AddListener(() =>
            {
                if(model.info.func(model.sel))
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
            if (param != null)
            {
                model.info = param.info;
                model.sel = model.info.defaultItem==null? model.info.item: model.info.defaultItem;
            }
            if(model.sel==null|| model.info.item.subs.Count==0)
            {
                Debug.LogError("No Option");
                Close();
                return;
            }
            model.deepth = GetDeepth(model.info.item);
            Refresh();


        }
        public void Refresh()
        {
            
            view.txt_title.text = model.info.title;
            view.go_close.SetActive(model.info.canClose);
            view.go_choose.SetActive( model.deepth == model.sel.deepth);
            
            colCon.Clear();
            
            for(int i=0;i< model.deepth-1; i++)
            {
                colCon.Add(new UiColumnParam()
                {
                    deepth=i,
                     cur= GetTarItem(model.sel,i)
                });
            }
            colCon.Refresh();

            itemCon.Clear();
            if (model.deepth-1 <= model.sel.deepth)
            {
                var cur=GetTarItem(model.sel, model.deepth - 1);
                foreach (var sub in cur.subs.Values)
                {
                    itemCon.Add(new UiSubItemParam()
                    {
                        cur = sub
                    });
                }
            }
            itemCon.Refresh();
            TimeManager.instance.AddCurLateUpdateAction(() =>
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
            },gameObject);
        }
        public int GetDeepth(EntryItem item)
        {
            int max = 0;
            foreach (var sub in item.subs.Values)
            {
                max = Math.Max(max, GetDeepth(sub) + 1);
            }
            return max;
        }
        public EntryItem GetTarItem(EntryItem item,int deepth)
        {
            var tmp = item;
            if (tmp.deepth < deepth)
            {
                return null;
            }
            while (tmp.deepth>deepth)
            {
                tmp = tmp.parent;
            }
            return tmp;
        }

    }
    public partial class UiColumnParam
    {
        public int deepth;
        public EntryItem cur;
    }
    public partial class UiColumnModel
    {
        public int deepth;
        public EntryItem cur;
    }
    public partial class UiColumnCtrl
    {
        UiScrViewContainer<UiLabelCtrl> lableCon;
        public override void OnCreate()
        {
            lableCon = new UiScrViewContainer<UiLabelCtrl>(this, view.go_label, view.scr_labels);
            rect.SetSiblingIndex(rect.parent.childCount-2);
        }

        public override void OnShow()
        {
            model.cur = param.cur;
            model.deepth=param.deepth;

            Refresh();

        }
        public void Refresh()
        {

            lableCon.Clear();
            if(model.cur!=null)
            {
                foreach (var sub in model.cur.subs.Values)
                {
                    lableCon.Add(new UiLabelParam()
                    {
                        cur = sub,
                        deepth = model.deepth
                    });
                }
            }
            lableCon.Refresh();

        }
       
    }



    public partial class UiLabelParam
    {
        public EntryItem cur;
        public int deepth;
    }
    public partial class UiLabelModel
    {
        public EntryItem cur;
        public int deepth;
    }
    public partial class UiLabelCtrl
    {
        public override void OnCreate()
        {
            view.btn_.onClick.AddListener(() =>
            {
                
                parent.parent.model.sel=model.cur;
                
                parent.parent.Refresh();
            });
        }
        public override void OnShow()
        {
            if (param != null)
            {
                model.cur=param.cur;
                model.deepth = param.deepth;
            }
            Refresh();
        }
        public void Refresh()
        {
            view.txt_.text = model.cur.content;
            view.img_.sprite = model.cur.sprite;
            view.sta_sel.ChangeState(parent.parent.model.sel.IsChildOf(model.cur)? 1:0);
        }
    }


    public partial class UiSubItemParam
    {
        public EntryItem cur;
    }
    public partial class UiSubItemModel
    {

        public EntryItem cur;
    }
    public partial class UiSubItemCtrl
    {
        public override void OnCreate()
        {
            view.btn_.onClick.AddListener(() =>
            {
                parent.model.sel=model.cur;
                parent.Refresh();
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
            view.sta_sel.ChangeState(parent.model.sel.IsChildOf(model.cur) ? 1 : 0);
        }
    }

}
