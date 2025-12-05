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
namespace Ui.ModStory.ModStoryMapObject.ModStoryMapObjectTexture.ModStoryMapObjectTextureConfig
{

    public partial class UiModStoryMapObjectTextureConfigParam
    {
        public MapTextureForm.Data data;
    }
    public partial class UiModStoryMapObjectTextureConfigModel
    {

        public MapTextureForm.Data data;

    }
    public partial class UiModStoryMapObjectTextureConfigCtrl
    {

        public override void OnCreate()
        {



        }
        public override void OnShow()
        {
            model.data = param.data;
            Refresh();
        }
        public void Refresh()
        {
            view.txt_onTouchEvent.text = model.data.events.GetDv("onTouchEvent", EventTriggerForm.defaultData).evt;
            view.txt_onLeaveEvent.text = model.data.events.GetDv("onLeaveEvent", EventTriggerForm.defaultData).evt;
            view.txt_onShowEvent.text = model.data.events.GetDv("onShowEvent", EventTriggerForm.defaultData).evt;
        }
    }

}