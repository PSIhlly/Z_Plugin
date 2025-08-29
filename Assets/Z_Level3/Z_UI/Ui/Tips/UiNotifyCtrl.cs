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
        public ChooseInfo chooseInfo;
        public MultipleChooseInfo multipleChooseInfo;
        public InputAreaInfo inputAreaInfo;
    }
    public partial class UiNotifyModel
    {
        public List<TipInfo> tipInfos = new List<TipInfo>();
        public List<PopupInfo> popupInfos = new List<PopupInfo>();
        public List<ChooseInfo> chooseInfos = new List<ChooseInfo>();
        public List<MultipleChooseInfo> multipleChooseInfos = new List<MultipleChooseInfo>();
        public List<InputAreaInfo> inputAreaInfos = new List<InputAreaInfo>();
        
        public int id;
    }
    public partial class UiNotifyCtrl
    {
        UiContainer<UiTipCtrl> tipCon;
        UiContainer<UiChooseCtrl> chooseCon;
        UiContainer<UiMultipleChooseCtrl> multipleChooseCon;
        UiContainer<UiPopupCtrl> popupCon;
        UiContainer<UiInputAreaCtrl> inputAreaCon;
        public override void OnCreate()
        {
            tipCon = new UiContainer<UiTipCtrl>(view.sub_Tip.gameObject);
            chooseCon = new UiContainer<UiChooseCtrl>(view.sub_Choose.gameObject);
            popupCon = new UiContainer<UiPopupCtrl>(view.sub_Popup.gameObject);
            multipleChooseCon = new UiContainer<UiMultipleChooseCtrl>(view.sub_MultipleChoose.gameObject);
            inputAreaCon = new UiContainer<UiInputAreaCtrl>(view.sub_InputArea.gameObject);
            

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
                if (param.multipleChooseInfo != null)
                    Add(param.multipleChooseInfo);
                if (param.popupInfo != null)
                    Add(param.popupInfo);
                if (param.inputAreaInfo != null)
                    Add(param.inputAreaInfo);
                
            }
        }
        public void Refresh()
        {
            view.btn_back.gameObject.SetActive(false);
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
            Debug.Log("Ë¢ÁË");
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

            //multipleChoose
            multipleChooseCon.Clear();
            if (model.multipleChooseInfos.Count > 0)
            {
                var cur = model.multipleChooseInfos[model.multipleChooseInfos.Count - 1];
                multipleChooseCon.Add(new UiMultipleChooseParam()
                {
                    info = cur
                });
            }
            multipleChooseCon.Refresh();

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

            //InputArea:
            inputAreaCon.Clear();
            view.go_block.SetActive(false);
            if (model.inputAreaInfos.Count > 0)
            {
                var cur = model.inputAreaInfos[model.inputAreaInfos.Count - 1];
                inputAreaCon.Add(new UiInputAreaParam()
                {
                    info = cur
                });
                view.go_block.SetActive(true);

            }
            inputAreaCon.Refresh();
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
        public void Add(MultipleChooseInfo info)
        {
            model.multipleChooseInfos.Add(info);
            Refresh();
        }
        public void Add(PopupInfo info)
        {
            model.popupInfos.Add(info);
            Refresh();
        }
        public void Add(InputAreaInfo info)
        {
            model.inputAreaInfos.Add(info);
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
            Debug.Log("½áÊøÁË" + model.tipInfos.Count);
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
        public void RemoveMultipleChoose(int id)
        {
            for (int i = 0; i < model.multipleChooseInfos.Count; i++)
            {

                if (model.multipleChooseInfos[i].id == id)
                {
                    model.multipleChooseInfos.RemoveAt(i);
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
        public void RemoveInputArea(int id)
        {
            for (int i = 0; i < model.inputAreaInfos.Count; i++)
            {
                if (model.inputAreaInfos[i].id == id)
                {
                    model.inputAreaInfos.RemoveAt(i);
                    break;
                }
            }
            Refresh();
        }
    }
  
}
