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
using Z_Time;
namespace Ui.ModStory.ModStoryMapObject.ModStoryMapObjectObject.ModStoryMapObjectObjectConfig
{

    public partial class UiModStoryMapObjectObjectConfigParam
    {
        public ObjectProductForm.Data data;
    }
    public partial class UiModStoryMapObjectObjectConfigModel
    {

        public ObjectProductForm.Data data;

    }
    public partial class UiModStoryMapObjectObjectConfigCtrl
    {

        private EventChoose.UiSceneEventChooseList eventChooseList;
        private TriggerEventChoose.GlobalVariableTriggerList globalTriggers;

        public override void OnCreate()
        {
            eventChooseList = new EventChoose.UiSceneEventChooseList(this, view.go_eventChoose);
            globalTriggers = new TriggerEventChoose.GlobalVariableTriggerList(this, view.go_triggerEventChoose);

            view.btn_faceType.onClick.AddListener(() =>
            {
                var items = new EntryItem();
                foreach (FaceType faceType in Enum.GetValues(typeof(FaceType)))
                {
                    items.Add(TextManager.instance.GetTxt(faceType.ToString()), null, (int)faceType);
                }
                NotifyManager.instance.AddChoose(TextManager.instance.GetTxt("Choose faceType"),
                    true, item =>
                    {
                        model.data.EnsureDirectionData();
                        model.data.faceType = (FaceType)item.id;
                        model.data.SyncLegacyAnimClip(model.data.GetDefaultAnimDirection());
                        model.data.SyncLegacyModelTransform(model.data.GetDefaultAnimDirection());
                        if (model.data.isWangTile || model.data.boundsCollision)
                            Main2StoryManager.instance.RefreshObjectWangTileTextures(model.data);
                        Refresh();
                        return true;
                    }, items);
            });

            view.btn_collision.onClick.AddListener(() =>
            {
                model.data.collision = !model.data.collision;
                Refresh();
                if (MapManager.instance.data != null)
                    GameManager.instance.mapCtrl.RefreshObjectWangTileAppearances(model.data.uid);
            });
            view.btn_boundsCollision.onClick.AddListener(() =>
            {
                model.data.boundsCollision = !model.data.boundsCollision;
                Refresh();
            });
            view.btn_centerCollider.onClick.AddListener(() =>
            {
                model.data.centerCollider = !model.data.centerCollider;
                Refresh();
                // ModStory retains scene rows after unloading the map. Update
                // the config immediately; only refresh spatial indexes in a loaded map.
                if (MapManager.instance.data != null)
                    GameManager.instance.mapCtrl.RefreshObjectWangTileAppearances(model.data.uid);
            });
            view.ipt_colliderScale.onFinishInput += s =>
            {
                model.data.model.colliderScale = StringHelper.ToFloat(s, 1, true);
                Refresh();
            };
            view.btn_isWangTile.onClick.AddListener(() =>
            {
                model.data.isWangTile = !model.data.isWangTile;
                Main2StoryManager.instance.RefreshObjectWangTileTextures(model.data);
                Refresh();
            });
            view.btn_minimapIcon.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ImportObjectMinimap(model.data.uid);
                Refresh();
            });
        }
        public override void OnShow()
        {
            model.data = param.data;
            model.data.EnsureDirectionData();
            Refresh();
        }
        public override void OnHide()
        {
            eventChooseList?.Clear();
            globalTriggers?.Clear();
        }

        public void Refresh()
        {
            view.txt_faceType.oriText = model.data.faceType.ToString();
            view.sta_collision.ChangeState(model.data.collision?1:0);
            view.sta_boundsCollision.ChangeState(model.data.boundsCollision ? 1 : 0);
            view.sta_centerCollider.ChangeState(model.data.centerCollider ? 1 : 0);
            view.ipt_colliderScale.Set(model.data.model.colliderScale.ToString("0.##"));
            view.sta_isWangTile.ChangeState(model.data.isWangTile ? 1 : 0);

            eventChooseList.Render(model.data.events, GameManager.instance.curProgress.enableMinimap);
            var data = model.data;
            var events = data.events;
            globalTriggers.Render(events, () => ReferenceEquals(model.data, data) && ReferenceEquals(data.events, events)
                && ObjectProductForm.DataByUid.TryGetValue(data.uid, out var current) && ReferenceEquals(current, data));

            view.go_minimap.SetActive(GameManager.instance.curProgress.enableMinimap);
            view.img_minimapIcon.BindTexData(TexAssetForm.DataById.GetDv(model.data.minimapIcon, TexAssetForm.DataById[GlobalDefaultHelper.DefaultStoryTexId]));

            Z_Ui.UiManager.Rebuild(gameObject, true);

        }
    }

}
