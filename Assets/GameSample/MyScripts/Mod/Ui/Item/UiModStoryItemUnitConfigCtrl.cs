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

        public override void OnCreate()
        {



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

        }
        public override void OnShow()
        {
            model.data = param.data;
            Refresh();
        }
        public void Refresh()
        {
            view.sta_canEquipped.ChangeState(model.data.canEquipe ? 1 : 0);
            view.sta_equip.ChangeState(model.data.canEquipe ? 1 : 0);
            view.sta_isConsume.ChangeState(model.data.isConsume ? 1 : 0);


            view.model_EventChooseUse.Set(new EventChoose.UiEventChooseParam() { dic = model.data.events, key = "onUseEvent" });
            view.model_EventChooseEquip.Set(new EventChoose.UiEventChooseParam() { dic = model.data.events, key = "onEquipEvent" });
            view.model_EventChooseDisequip.Set(new EventChoose.UiEventChooseParam() { dic = model.data.events, key = "onDisequipEvent" });

            view.model_EventChooseCharacterTouch.Set(new EventChoose.UiEventChooseParam() { dic = model.data.events, key = "onCharacterTouchEvent" });
            view.model_EventChooseCharacterLeave.Set(new EventChoose.UiEventChooseParam() { dic = model.data.events, key = "onCharacterLeaveEvent" });
            view.model_EventChooseObjectTouch.Set(new EventChoose.UiEventChooseParam() { dic = model.data.events, key = "onObjectTouchEvent" });
            view.model_EventChooseObjectLeave.Set(new EventChoose.UiEventChooseParam() { dic = model.data.events, key = "onObjectLeaveEvent" });
            view.model_EventChooseShow.Set(new EventChoose.UiEventChooseParam() { dic = model.data.events, key = "onShowEvent" });
            view.model_EventChoosePerSecond.Set(new EventChoose.UiEventChooseParam() { dic = model.data.events, key = "onPerSecondEvent" });
            view.model_EventChooseBoundaryTouch.Set(new EventChoose.UiEventChooseParam() { dic = model.data.events, key = "onBoundaryTouchEvent" });
            view.model_EventChooseInteract.Set(new EventChoose.UiEventChooseParam() { dic = model.data.events, key = "onInteractEvent" });

            view.go_equip.SetActive(GameManager.instance.curProgress.enableEquip);
        }
    }

}