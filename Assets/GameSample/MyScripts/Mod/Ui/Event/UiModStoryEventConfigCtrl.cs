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


        }
        public override void OnShow()
        {
            Refresh();
        }
        public void Refresh()
        {
            view.model_EventChooseBegin.Set(new EventChoose.UiEventChooseParam() { dic = GameManager.instance.curProgress.events, key = "onBeginEvent" });
        }
    }

}