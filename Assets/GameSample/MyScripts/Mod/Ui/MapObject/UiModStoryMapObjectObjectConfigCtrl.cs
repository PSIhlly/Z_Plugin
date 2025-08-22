using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;
using UnityEngine;
using Z_Text;
using Z_Ui.Notify;
using Z_Code.Form;
using Z_String;
using Z_DataSystem.Form;

namespace Ui.ModStory.ModStoryMapObject.ModStoryMapObjectObject.ModStoryMapObjectObjectConfig
{

    public partial class UiModStoryMapObjectObjectConfigParam
    {
        public MapObjectForm.Data data;
    }
    public partial class UiModStoryMapObjectObjectConfigModel
    {

        public MapObjectForm.Data data;

    }
    public partial class UiModStoryMapObjectObjectConfigCtrl
    {

        public override void OnCreate()
        {

            view.btn_onShowEvent.onClick.AddListener(() =>
            {
                model.data.isFixed = !model.data.isFixed;
                Refresh();
            });
            view.btn_onTouchEvent.onClick.AddListener(() =>
            {
                GameManager.instance.evtCtrl.GetEvents(EventType.Object, CmdTypeDataForm.defaultData, out var sub);
                NotifyManager.instance.AddMultipleChoose(TextManager.instance.GetTxt("onTouchEvent"), false, 2, (lst) =>
                {
                    model.data.onTouchEvent = lst[2];
                    Refresh();
                    return true;
                }, sub);
            });
            view.btn_onLeaveEvent.onClick.AddListener(() =>
            {
                GameManager.instance.evtCtrl.GetEvents(EventType.Object, CmdTypeDataForm.defaultData, out var sub);
                NotifyManager.instance.AddMultipleChoose(TextManager.instance.GetTxt("onLeaveEvent"), false, 2, (lst) =>
                {
                    model.data.onLeaveEvent = lst[2];
                    Refresh();
                    return true;
                }, sub);
            });
            view.btn_onShowEvent.onClick.AddListener(() =>
            {
                GameManager.instance.evtCtrl.GetEvents(EventType.Object, CmdTypeDataForm.defaultData, out var sub);
                NotifyManager.instance.AddMultipleChoose(TextManager.instance.GetTxt("onShowEvent"), false, 2, (lst) =>
                {
                    model.data.onShowEvent = lst[2];
                    Refresh();
                    return true;
                }, sub);
            });
            view.btn_fixed.onClick.AddListener(() =>
            {
                model.data.isFixed = !model.data.isFixed;
                Refresh();
            });

        }
        public override void OnShow()
        {
            model.data = param.data;
            Refresh();
        }
        public void Refresh()
        {
            view.sta_fixed.ChangeState(model.data.isFixed?1:0);
            view.txt_onTouchEvent.text = model.data.onTouchEvent;
            view.txt_onLeaveEvent.text = model.data.onLeaveEvent;
            view.txt_onShowEvent.text = model.data.onShowEvent;
        }
    }

}