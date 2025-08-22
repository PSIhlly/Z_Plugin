using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
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
        public List<string> sel;
    }

    public partial class UiMultipleChooseCtrl
    {

        UiContainer<UiColumnCtrl> colCon;
        UiScrViewContainer<UiSubItemCtrl> itemCon;
        public override void OnCreate()
        {
            model.sel = new List<string>();
            colCon = new UiContainer<UiColumnCtrl>(view.go_column);
            itemCon = new UiScrViewContainer<UiSubItemCtrl>(view.go_subItem, view.scr_subItems);
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
            model.sel.Clear();

            if (param != null)
            {
                model.info = param.info;
            }
            Refresh();


        }
        public void Refresh()
        {
            
            view.txt_title.text = model.info.title;
            view.go_close.SetActive(model.info.canClose);
            view.go_choose.SetActive(model.sel.Count==model.info.labCnt+1);
            
            colCon.Clear();
            for (int i = model.info.labCnt-1; i >=0 ; i--)
            {
                GetInfo(i,out var sub);
                colCon.Add(new UiColumnParam()
                {
                    id=i,
                     sub= sub
                });
            }
            colCon.Refresh();

            itemCon.Clear();
            if (model.sel.Count >= model.info.labCnt)
            {
                GetInfo(model.info.labCnt, out var sub);
                foreach (var kv in sub)
                {
                    itemCon.Add(new UiSubItemParam()
                    {
                        name = kv.Key,
                        sprite = kv.Value.Item1
                    });
                }
            }
            itemCon.Refresh();

            TimeManager.instance.StartTimer(0.001f, 0, () =>
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
                return true;
            }, uiHolder);
        }

        public void GetInfo(int id,out Dictionary<string, (Sprite,object)> sub)
        {
            if(id > model.sel.Count)
            {
                sub = new Dictionary<string, (Sprite, object)>();
                return;
            }
            sub = model.info.sub;
            for (int i=0;i<id;i++)
            {
                sub = sub[model.sel[i]].Item2 as Dictionary<string, (Sprite, object)>;
            }
        }

    }
    public partial class UiColumnParam
    {
        public int id;
        public Dictionary<string, (Sprite, object)> sub;
    }
    public partial class UiColumnModel
    {
        public int id;
        public Dictionary<string, (Sprite, object)> sub;
    }
    public partial class UiColumnCtrl
    {
        UiScrViewContainer<UiLabelCtrl> lableCon;
        public override void OnCreate()
        {
            lableCon = new UiScrViewContainer<UiLabelCtrl>(view.go_label, view.scr_labels);
        }

        public override void OnShow()
        {
            model.sub = param.sub;
            model.id=param.id;

            Refresh();

            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);

        }
        public void Refresh()
        {

            lableCon.Clear();
            foreach (var kv in model.sub)
            {
                lableCon.Add(new UiLabelParam()
                {
                    name = kv.Key,
                    sprite = kv.Value.Item1,
                    columnId = model.id
                });
            }
            lableCon.Refresh();

        }
    }



    public partial class UiLabelParam
    {
        public Sprite sprite;
        public string name;
        public int id;
        public int columnId;
    }
    public partial class UiLabelModel
    {
        public Sprite sprite;
        public string name;
        public int id;
        public int columnId;
    }
    public partial class UiLabelCtrl
    {
        public override void OnCreate()
        {
            view.btn_.onClick.AddListener(() =>
            {
                while (parent.parent.model.sel.Count > model.columnId )
                {
                    parent.parent.model.sel.RemoveAt(model.columnId);
                }
                parent.parent.model.sel.Add(model.name);
                
                parent.parent.Refresh();
            });
        }
        public override void OnShow()
        {
            if (param != null)
            {
                model.sprite = param.sprite;
                model.name = param.name;
                model.id = param.id;
                model.columnId=param.columnId;
            }
            Refresh();
        }
        public void Refresh()
        {
            view.txt_.text = model.name;
            view.img_.sprite = model.sprite;
            view.sta_sel.ChangeState(parent.parent.model.sel.Count> model.columnId&& parent.parent.model.sel[model.columnId]==model.name?1:0);
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
    }
    public partial class UiSubItemCtrl
    {
        public override void OnCreate()
        {
            view.btn_.onClick.AddListener(() =>
            {
                while (parent.model.sel.Count > parent.model.info.labCnt)
                {
                    parent.model.sel.RemoveAt(parent.model.info.labCnt);
                }
                parent.model.sel.Add(model.name);
                parent.Refresh();
            });
        }
        public override void OnShow()
        {
            if (param != null)
            {
                model.sprite = param.sprite;
                model.name = param.name;
            }
            Refresh();
        }
        public void Refresh()
        {
            view.txt_.text= model.name;
            view.img_.sprite = model.sprite;
            view.sta_sel.ChangeState(parent.model.sel.Count > parent.model.info.labCnt && parent.model.sel[parent.model.info.labCnt] == model.name ? 1 : 0);
        }
    }

}
