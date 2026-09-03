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
        public void Refresh()
        {
            view.sta_isWangTile.ChangeState(model.data.isWangTile ? 1 : 0);
            view.sta_enableFrontPart.ChangeState(model.data.enableFrontPart ? 1 : 0);
            if (model.data.passType != 0 && !PassTypeForm.DataById.ContainsKey(model.data.passType))
                model.data.passType = 0;
            var passTypeText = view.btn_passType.GetComponentInChildren<Txt>(true);
            if (passTypeText != null)
            {
                passTypeText.text = model.data.passType == 0
                    ? "Unrestricted"
                    : PassTypeForm.DataById[model.data.passType].name;
            }
            view.model_EventChooseCharacterTouch.Set(new EventChoose.UiEventChooseParam() { dic = model.data.events, key = "onCharacterTouchEvent" });
            view.model_EventChooseCharacterLeave.Set(new EventChoose.UiEventChooseParam() { dic = model.data.events, key = "onCharacterLeaveEvent" });
            view.model_EventChooseObjectTouch.Set(new EventChoose.UiEventChooseParam() { dic = model.data.events, key = "onObjectTouchEvent" });
            view.model_EventChooseObjectLeave.Set(new EventChoose.UiEventChooseParam() { dic = model.data.events, key = "onObjectLeaveEvent" });
            view.model_EventChooseTileTouch.Set(new EventChoose.UiEventChooseParam() { dic = model.data.events, key = "onTileTouchEvent" });
            view.model_EventChooseShow.Set(new EventChoose.UiEventChooseParam() { dic = model.data.events, key = "onShowEvent" });
            view.model_EventChoosePerSecond.Set(new EventChoose.UiEventChooseParam() { dic = model.data.events, key = "onPerSecondEvent" });
            view.model_EventChooseInteract.Set(new EventChoose.UiEventChooseParam() { dic = model.data.events, key = "onInteractEvent" });
            view.model_EventChooseLeaveScene.Set(new EventChoose.UiEventChooseParam() { dic = model.data.events, key = "onLeaveSceneEvent" });

        }
    }

}
