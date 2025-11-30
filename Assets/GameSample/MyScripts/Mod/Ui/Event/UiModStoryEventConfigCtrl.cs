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
using Z_DesignStyle;

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
                var key = "onBeginEvent";
                ModManager.instance.assetCtrl.ChooseEvent(SceneEventType.Global, CmdTypeDataForm.defaultData.name, TextManager.instance.GetTxt(key), (item) =>
                {
                    GameManager.instance.curProgress.events[key] = GameEventController.CreateTrigger(key, item.content); 
                    Refresh();
                });
            });
            view.btn_onEndEvent.onClick.AddListener(() =>
            {
                var key = "onEndEvent";
                ModManager.instance.assetCtrl.ChooseEvent(SceneEventType.Global, CmdTypeDataForm.defaultData.name, TextManager.instance.GetTxt(key), (item) =>
                {
                    GameManager.instance.curProgress.events[key] = GameEventController.CreateTrigger(key, item.content);
                    Refresh();
                });
            });

        }
        public override void OnShow()
        {
            Refresh();
        }
        public void Refresh()
        {
            view.txt_onBeginEvent.text = GameManager.instance.curProgress.events.GetDv("onBeginEvent", EventTriggerForm.defaultData).evt;
            view.txt_onEndEvent.text = GameManager.instance.curProgress.events.GetDv("onEndEvent", EventTriggerForm.defaultData).evt;
        }
    }

}