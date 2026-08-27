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

            view.btn_collision.onClick.AddListener(() =>
            {
                model.data.collision = !model.data.collision;
                Refresh();
            });
            view.btn_minimapIcon.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ImportObjectMinimap(model.data.id);
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
            view.sta_collision.ChangeState(model.data.collision?1:0);

            view.model_EventChooseCharacterTouch.Set(new EventChoose.UiEventChooseParam() { dic = model.data.events, key = "onCharacterTouchEvent" });
            view.model_EventChooseCharacterLeave.Set(new EventChoose.UiEventChooseParam() { dic = model.data.events, key = "onCharacterLeaveEvent" });
            view.model_EventChooseObjectTouch.Set(new EventChoose.UiEventChooseParam() { dic = model.data.events, key = "onObjectTouchEvent" });
            view.model_EventChooseObjectLeave.Set(new EventChoose.UiEventChooseParam() { dic = model.data.events, key = "onObjectLeaveEvent" });
            view.model_EventChooseTileTouch.Set(new EventChoose.UiEventChooseParam() { dic = model.data.events, key = "onTileTouchEvent" });
            view.model_EventChooseShow.Set(new EventChoose.UiEventChooseParam() { dic = model.data.events, key = "onShowEvent" });
            view.model_EventChoosePerSecond.Set(new EventChoose.UiEventChooseParam() { dic = model.data.events, key = "onPerSecondEvent" });
            view.model_EventChooseBoundaryTouch.Set(new EventChoose.UiEventChooseParam() { dic = model.data.events, key = "onBoundaryTouchEvent" });
            view.model_EventChooseInteract.Set(new EventChoose.UiEventChooseParam() { dic = model.data.events, key = "onInteractEvent" });

            view.model_EventChooseClickMinimap.Set(new EventChoose.UiEventChooseParam() { dic = model.data.events, key = "onClickMinimapEvent" });
            view.model_EventChooseClickMinimap.SetShow(GameManager.instance.curProgress.enableMinimap);

            view.model_EventChooseLeaveScene.Set(new EventChoose.UiEventChooseParam() { dic = model.data.events, key = "onLeaveSceneEvent" });

            view.go_minimap.SetActive(GameManager.instance.curProgress.enableMinimap);
            view.img_minimapIcon.BindTexData(TexAssetForm.DataById.GetDv(model.data.minimapIcon, TexAssetForm.DataById[GlobalDefaultHelper.DefaultStoryTexId]));

        }
    }

}
