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
    public partial class UiNotifyParam
    {
        public TipInfo tipInfo;
        public PopupInfo popupInfo;
    }
    public partial class UiNotifyModel
    {
        public List<TipInfo> tipInfos=new List<TipInfo>();
        public List<PopupInfo> popupInfos=new List<PopupInfo>();
        public int id;
    }
    public partial class UiNotifyCtrl
    {
        UiContainer<UiTipCtrl> tipCon;
        UiContainer<UiPopupCtrl> popupCon;
        public override void OnCreate()
        {
            tipCon = new UiContainer<UiTipCtrl>(view.sub_Tip.gameObject);
            popupCon = new UiContainer<UiPopupCtrl>(view.sub_Popup.gameObject);


            view.btn_back.onClick.AddListener(()=>
            {

            });
        }
        public override void OnShow()
        {
            if(param!=null)
            {
                if(param.tipInfo!=null)
                    Add(param.tipInfo);
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
                    var cur = model.tipInfos[model.tipInfos.Count-1];
                    tipCon.Add(new UiTipParam()
                    {
                        info = cur
                    });
                }
                tipCon.Refresh();

            //popup:
            popupCon.Clear();
            view.go_block.SetActive(false);
            if (model.popupInfos.Count > 0)
            {
                    var cur = model.popupInfos[model.popupInfos.Count-1];
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
        public void Add(PopupInfo info)
        {
            model.popupInfos.Add(info);
            Refresh();
        }
        public void RemoveTip(int id)
        {
            for(int i=0;i<model.tipInfos.Count;i++)
            {
                
                if(model.tipInfos[i].id==id)
                {
                    model.tipInfos.RemoveAt(i);
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
            view.btn_.onClick.AddListener(()=>
            {
                if(model.funcs!=null)
                {
                    if (model.funcs())
                    {
                        parent.parent.RemovePopup(model.id);
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
    public partial class UiPopupParam
    {
        public PopupInfo info;
    }

    public partial class UiPopupCtrl
    {

        UiContainer<UiSelectionCtrl> con;
        public override void OnCreate()
        {
            con = new UiContainer<UiSelectionCtrl>(view.sub_Selection.gameObject);
            view.btn_close.onClick.AddListener(()=>
            {
                Close();
            });
        }
        public override void OnShow()
        {
            con.Clear();
            if (param != null)
            { 
                view.txt_content.text = param.info.content;
                view.go_close.SetActive(param.info.canClose);
                view.txt_title.text = param.info.title;

                for(int i=0;i<param.info.selectionWords.Count;i++)
                {
                    con.Add(new UiSelectionParam()
                    {
                        name = param.info.selectionWords[i],
                        func = param.info.funcs[i],
                        id = param.info.id
                    }) ;
                }
                
            }

            con.Refresh();

            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);

        }
        

    }
    public partial class UiTipModel
    {
        public Timer removeTimer;
        public int id;
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
                model.id = param.info.id;
            }
            model.removeTimer = TimeManager.instance.StartTimer(param.info.time - Time.time, () =>
            {
                parent.RemoveTip(model.id);
                return true;
            },uiHolder);

            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        }

    }
}
