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


            view.btn_onTouchEvent.onClick.AddListener(() =>
            {
                GameManager.instance.evtCtrl.GetEvents(EventType.Object, CmdTypeDataForm.defaultData, out var sub);
                NotifyManager.instance.AddMultipleChoose(TextManager.instance.GetTxt("onTouchEvent"), false, 2, (lst) =>
                {
                    model.data.onTouchEvent = lst[2];
                    return true;
                }, sub);
            });
            view.btn_onLeaveEvent.onClick.AddListener(() =>
            {
                GameManager.instance.evtCtrl.GetEvents(EventType.Object, CmdTypeDataForm.defaultData, out var sub);
                NotifyManager.instance.AddMultipleChoose(TextManager.instance.GetTxt("onLeaveEvent"), false, 2, (lst) =>
                {
                    model.data.onLeaveEvent = lst[2];
                    return true;
                }, sub);
            });
            view.btn_onShowEvent.onClick.AddListener(() =>
            {
                GameManager.instance.evtCtrl.GetEvents(EventType.Object, CmdTypeDataForm.defaultData, out var sub);
                NotifyManager.instance.AddMultipleChoose(TextManager.instance.GetTxt("onShowEvent"), false, 2, (lst) =>
                {
                    model.data.onShowEvent = lst[2];
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
            view.txt_onTouchEvent.text = model.data.onTouchEvent;
            view.txt_onLeaveEvent.text = model.data.onLeaveEvent;
            view.txt_onShowEvent.text = model.data.onShowEvent;
        }
    }

}