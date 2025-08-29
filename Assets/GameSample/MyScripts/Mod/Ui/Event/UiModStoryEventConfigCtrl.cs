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
                ModManager.instance.assetCtrl.ChooseEvent(EventType.Global, CmdTypeDataForm.defaultData.name, TextManager.instance.GetTxt("onBeginEvent"), (item) =>
                {
                    GameManager.instance.curConfig.onBeginEvent = item.content;
                    Refresh();
                });
            });
            view.btn_onEndEvent.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ChooseEvent(EventType.Global, CmdTypeDataForm.defaultData.name, TextManager.instance.GetTxt("onEndEvent"), (item) =>
                {
                    GameManager.instance.curConfig.onEndEvent = item.content;
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
            view.txt_onBeginEvent.text = GameManager.instance.curConfig.onBeginEvent;
            view.txt_onEndEvent.text = GameManager.instance.curConfig.onEndEvent;
        }
    }

}