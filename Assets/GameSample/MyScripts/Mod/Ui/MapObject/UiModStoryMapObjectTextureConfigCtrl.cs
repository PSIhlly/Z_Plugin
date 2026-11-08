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
using Z_Ui;
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

        private EventChoose.UiSceneEventChooseList eventChooseList;
        private TriggerEventChoose.GlobalVariableTriggerList globalTriggers;

        public override void OnCreate()
        {
            eventChooseList = new EventChoose.UiSceneEventChooseList(this, view.go_eventChoose);
            globalTriggers = new TriggerEventChoose.GlobalVariableTriggerList(this, view.go_triggerEventChoose);
            view.btn_isWangTile.onClick.AddListener(() =>
            {
                model.data.isWangTile = !model.data.isWangTile;
                Refresh();
            });
            view.btn_enableFrontPart.onClick.AddListener(() =>
            {
                model.data.enableFrontPart = !model.data.enableFrontPart;
                Refresh();
            });
            view.btn_frontIsWangTile.onClick.AddListener(() =>
            {
                model.data.frontIsWangTile = !model.data.frontIsWangTile;
                Refresh();
            });
            view.btn_passType.onClick.AddListener(() =>
            {
                var items = new EntryItem();
                items.Add("Unrestricted", null, 0);
                foreach (var passType in PassTypeForm.DataById.Values.OrderBy(data => data.id))
                    items.Add(passType.name, null, passType.id);

                NotifyManager.instance.AddChoose("Choose passType", true, item =>
                {
                    model.data.passType = item.id;
                    Refresh();
                    return true;
                }, items);
            });
        }
        public override void OnShow()
        {
            model.data = param.data;
            Refresh();
        }
        public override void OnHide()
        {
            eventChooseList?.Clear();
            globalTriggers?.Clear();
        }

        public void Refresh()
        {
            view.sta_isWangTile.ChangeState(model.data.isWangTile ? 1 : 0);
            view.sta_enableFrontPart.ChangeState(model.data.enableFrontPart ? 1 : 0);
            view.sta_frontIsWangTile.ChangeState(model.data.frontIsWangTile ? 1 : 0);
            if (model.data.passType != 0 && !PassTypeForm.DataById.ContainsKey(model.data.passType))
                model.data.passType = 0;
            var passTypeText = view.btn_passType.GetComponentInChildren<Txt>(true);
            if (passTypeText != null)
            {
                passTypeText.text = model.data.passType == 0
                    ? "Unrestricted"
                    : PassTypeForm.DataById[model.data.passType].name;
            }
            eventChooseList.Render(model.data.events, GameManager.instance.curProgress.enableMinimap);
            var data = model.data;
            var events = data.events;
            globalTriggers.Render(events, () => ReferenceEquals(model.data, data) && ReferenceEquals(data.events, events)
                && MapTextureForm.DataById.TryGetValue(data.id, out var current) && ReferenceEquals(current, data));

            UiManager.Rebuild(gameObject, true);
        }
    }

}
