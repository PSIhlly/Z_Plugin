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
                var key = "onTouchEvent";
                ModManager.instance.assetCtrl.ChooseEvent(model.data.events, key, EventType.Object, CmdTypeDataForm.defaultData.name, TextManager.instance.GetTxt(key), (item) =>
                {
                    Refresh();
                });               
            });
            view.btn_onLeaveEvent.onClick.AddListener(() =>
            {
                var key = "onLeaveEvent";
                ModManager.instance.assetCtrl.ChooseEvent(model.data.events, key, EventType.Object, CmdTypeDataForm.defaultData.name, TextManager.instance.GetTxt(key), (item) =>
                {
                    Refresh();
                });
            });
            view.btn_onShowEvent.onClick.AddListener(() =>
            {
                var key = "onShowEvent";
                ModManager.instance.assetCtrl.ChooseEvent(model.data.events, key, EventType.Object, CmdTypeDataForm.defaultData.name, TextManager.instance.GetTxt(key), (item) =>
                {
                    Refresh();
                });
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
            view.txt_onTouchEvent.text = model.data.events.GetDv("onTouchEvent", EventTriggerForm.defaultData).evt;
            view.txt_onLeaveEvent.text = model.data.events.GetDv("onLeaveEvent", EventTriggerForm.defaultData).evt;
            view.txt_onShowEvent.text = model.data.events.GetDv("onShowEvent", EventTriggerForm.defaultData).evt;
        }
    }

}