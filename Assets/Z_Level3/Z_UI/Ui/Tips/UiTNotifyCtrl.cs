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
    public partial class UiNotifyParam
    {
        public TipInfo tipInfo;
        public PopupInfo popupInfo;
        public ChooseInfo chooseInfo;
    }
    public partial class UiNotifyModel
    {
        public List<TipInfo> tipInfos = new List<TipInfo>();
        public List<PopupInfo> popupInfos = new List<PopupInfo>();
        public List<ChooseInfo> chooseInfos = new List<ChooseInfo>();

        public int id;
    }
    public partial class UiNotifyCtrl
    {
        UiContainer<UiTipCtrl> tipCon;
        UiContainer<UiChooseCtrl> chooseCon;
        UiContainer<UiPopupCtrl> popupCon;
        public override void OnCreate()
        {
            tipCon = new UiContainer<UiTipCtrl>(view.sub_Tip.gameObject);
            chooseCon = new UiContainer<UiChooseCtrl>(view.sub_Choose.gameObject);
            popupCon = new UiContainer<UiPopupCtrl>(view.sub_Popup.gameObject);


            view.btn_back.onClick.AddListener(() =>
            {

            });
        }
        public override void OnShow()
        {
            if (param != null)
            {
                if (param.tipInfo != null)
                    Add(param.tipInfo);
                if (param.chooseInfo != null)
                    Add(param.chooseInfo);
                if (param.popupInfo != null)
                    Add(param.popupInfo);
            }
        }
        public void Refresh()
        {
            //tip:

            tipCon.Clear();
            if (model.tipInfos.Count > 0)
            {
                var cur = model.tipInfos[model.tipInfos.Count - 1];
                tipCon.Add(new UiTipParam()
                {
                    info = cur
                });
            }
            tipCon.Refresh();

            //choose
            chooseCon.Clear();
            if (model.chooseInfos.Count > 0)
            {
                var cur = model.chooseInfos[model.chooseInfos.Count - 1];
                chooseCon.Add(new UiChooseParam()
                {
                    info = cur
                });
            }
            chooseCon.Refresh();

            //popup:
            popupCon.Clear();
            view.go_block.SetActive(false);
            if (model.popupInfos.Count > 0)
            {
                var cur = model.popupInfos[model.popupInfos.Count - 1];
                popupCon.Add(new UiPopupParam()
                {
                    info = cur
                });
                view.go_block.SetActive(true);

            }
            popupCon.Refresh();

        }

        public void Add(TipInfo info)
        {
            model.tipInfos.Add(info);
            Refresh();
        }
        public void Add(ChooseInfo info)
        {
            model.chooseInfos.Add(info);
            Refresh();
        }
        public void Add(PopupInfo info)
        {
            model.popupInfos.Add(info);
            Refresh();
        }
        public void RemoveTip(int id)
        {
            for (int i = 0; i < model.tipInfos.Count; i++)
            {

                if (model.tipInfos[i].id == id)
                {
                    model.tipInfos.RemoveAt(i);
                    break;
                }
            }
            Refresh();
        }
        public void RemoveChoose(int id)
        {
            for (int i = 0; i < model.chooseInfos.Count; i++)
            {

                if (model.chooseInfos[i].id == id)
                {
                    model.chooseInfos.RemoveAt(i);
                    break;
                }
            }
            Refresh();
        }
        public void RemovePopup(int id)
        {
            for (int i = 0; i < model.popupInfos.Count; i++)
            {
                if (model.popupInfos[i].id == id)
                {
                    model.popupInfos.RemoveAt(i);
                    break;
                }
            }
            Refresh();
        }
    }
    public partial class UiTipModel
    {
        public Timer removeTimer;
        public TipInfo info;
    }
    public partial class UiTipParam
    {
        public TipInfo info;
    }

    public partial class UiTipCtrl
    {
        public override void OnShow()
        {
            if (param != null)
            {
                view.txt_.text = param.info.content;
                model.info = param.info;
            }
            model.removeTimer = TimeManager.instance.StartTimer(param.info.time - Time.time, () =>
            {
                Close();
                return true;
            }, uiHolder);
            LayoutRebuilder.ForceRebuildLayoutImmediate(view.txt_.rectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        }
        public override void Close()
        {
            base.Close();
            parent.RemoveTip(model.info.id);
        }

    }

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

        UiContainer<UiItemCtrl> con;
        public override void OnCreate()
        {
            con = new UiContainer<UiItemCtrl>(view.sub_Item.gameObject);
            view.btn_close.onClick.AddListener(() =>
            {
                Close();
            });
            view.btn_choose.onClick.AddListener(() =>
            {
                model.info.act?.Invoke(model.cur);
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

            for (int i = 0; i < model.info.words.Count; i++)
            {
                con.Add(new UiItemParam()
                {
                    name = model.info.words[i],
                    sprite = model.info.sprites[i],
                    id = model.info.id
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
