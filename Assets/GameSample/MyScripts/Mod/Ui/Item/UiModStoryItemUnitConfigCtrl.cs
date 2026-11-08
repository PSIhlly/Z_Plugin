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
namespace Ui.ModStory.ModStoryItem.ModStoryItemUnit.ModStoryItemUnitConfig
{

    public partial class UiModStoryItemUnitConfigParam
    {
        public ItemProductForm.Data data;
    }
    public partial class UiModStoryItemUnitConfigModel
    {

        public ItemProductForm.Data data;
    }
    public partial class UiModStoryItemUnitConfigCtrl
    {

        private EventChoose.UiSceneEventChooseList eventChooseList;
        private TriggerEventChoose.GlobalVariableTriggerList globalTriggers;

        public override void OnCreate()
        {
            eventChooseList = new EventChoose.UiSceneEventChooseList(this, view.go_eventChoose);
            globalTriggers = new TriggerEventChoose.GlobalVariableTriggerList(this, view.go_triggerEventChoose);



            view.btn_canEquipped.onClick.AddListener(() =>
            {
                model.data.canEquipe = !model.data.canEquipe;
                Refresh();
            });
            view.btn_isConsume.onClick.AddListener(() =>
            {
                model.data.isConsume = !model.data.isConsume;
                Refresh();
            });

            view.btn_part.onClick.AddListener(() =>
            {
                var items = new EntryItem();
                foreach (EquipPartType part in Enum.GetValues(typeof(EquipPartType)))
                {
                    items.Add(TextManager.instance.GetTxt(part.ToString()), null, (int)part);
                }
                NotifyManager.instance.AddChoose(TextManager.instance.GetTxt("chooseEquipPart"), false, (res) =>
                {
                    model.data.equip = (EquipPartType)res.id;
                    Refresh();
                    return true;
                }, items);
            });
            view.btn_minimapIcon.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ImportItemMinimap(model.data.uid);
                Refresh();
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
            view.go_equipToggle.SetActive(GameManager.instance.curProgress.enableEquip);
            view.sta_canEquipped.ChangeState(model.data.canEquipe? 1 : 0);
            view.sta_equip.ChangeState(model.data.canEquipe ? 1 : 0);
            

            view.sta_isConsume.ChangeState(model.data.isConsume ? 1 : 0);


            view.model_EventChooseUse.Set(new EventChoose.UiEventChooseParam() { dic = model.data.events, key = "onUseEvent" });
            view.model_EventChooseEquip.Set(new EventChoose.UiEventChooseParam() { dic = model.data.events, key = "onEquipEvent" });
            view.model_EventChooseDisequip.Set(new EventChoose.UiEventChooseParam() { dic = model.data.events, key = "onDisequipEvent" });

            eventChooseList.Render(model.data.events, GameManager.instance.curProgress.enableMinimap);
            var data = model.data;
            var events = data.events;
            globalTriggers.Render(events, () => ReferenceEquals(model.data, data) && ReferenceEquals(data.events, events)
                && ItemProductForm.DataByUid.TryGetValue(data.uid, out var current) && ReferenceEquals(current, data));


            view.go_minimap.SetActive(GameManager.instance.curProgress.enableMinimap);
            view.img_minimapIcon.BindTexData(TexAssetForm.DataById.GetDv(model.data.minimapIcon, TexAssetForm.DataById[GlobalDefaultHelper.DefaultStoryTexId]));
            Z_Ui.UiManager.Rebuild(gameObject, true);
        }
    }

}
