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

namespace Ui.ModStory.ModStoryEvent.ModStoryEventConfig
{

    public partial class UiModStoryEventConfigParam
    {
    }
    public partial class UiModStoryEventConfigModel
    {
    }
    public partial class UiModStoryEventConfigCtrl
    {

        public override void OnCreate()
        {

            view.btn_onBeginEvent.onClick.AddListener(() =>
            {
                GameManager.instance.evtCtrl.GetEvents(EventType.Global, CmdTypeDataForm.defaultData, out var sub);
                NotifyManager.instance.AddMultipleChoose(TextManager.instance.GetTxt("onBeginEvent"), false, 2, (lst) =>
                {
                    ConfigForm.DataByUid[1].onBeginEvent = lst[2];
                    Refresh();
                    return true;
                }, sub);
            });
            view.btn_onEndEvent.onClick.AddListener(() =>
            {
                GameManager.instance.evtCtrl.GetEvents(EventType.Global, CmdTypeDataForm.defaultData, out var sub);
                NotifyManager.instance.AddMultipleChoose(TextManager.instance.GetTxt("onEndEvent"), false, 2, (lst) =>
                {
                    ConfigForm.DataByUid[1].onEndEvent = lst[2];
                    Refresh();
                    return true;
                }, sub);
            });

        }
        public override void OnShow()
        {
            Refresh();
        }
        public void Refresh()
        {
            view.txt_onBeginEvent.text = ConfigForm.DataByUid[1].onBeginEvent;
            view.txt_onEndEvent.text = ConfigForm.DataByUid[1].onEndEvent;
        }
    }

}