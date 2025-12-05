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
            view.ipt_price.onFinishInput+=(s) =>
            {
                model.data.price= StringHelper.ToInt(s,0);
                Refresh();
            };
           
            view.btn_part.onClick.AddListener(() =>
            {
                var items = new EntryItem();
                foreach (EquipPartType part in Enum.GetValues(typeof(EquipPartType)))
                {
                    items.Add(TextManager.instance.GetTxt(part.ToString()));
                }
                NotifyManager.instance.AddChoose(TextManager.instance.GetTxt("chooseEquipPart"), false, (res) =>
                {
                    model.data.equip = (EquipPartType)Enum.Parse(typeof(EquipPartType),res.content);
                    Refresh();
                    return true;
                }, items);
            });
           
        }
        public override void OnShow()
        {
            model.data=param.data;
            Refresh();
        }
        public void Refresh()
        {
            view.sta_canEquipped.ChangeState(model.data.canEquipe?1:0);
            view.ipt_price.Set(model.data.price.ToString());

            view.txt_onUseEvent.text = model.data.events.GetDv("onUseEvent", EventTriggerForm.defaultData).evt;
            view.txt_onEquipEvent.text = model.data.events.GetDv("onEquipEvent", EventTriggerForm.defaultData).evt;
            view.txt_onDisequipEvent.text = model.data.events.GetDv("onDisequipEvent", EventTriggerForm.defaultData).evt;
            view.txt_onTouchEvent.text = model.data.events.GetDv("onTouchEvent", EventTriggerForm.defaultData).evt;
            view.txt_onLeaveEvent.text = model.data.events.GetDv("onLeaveEvent", EventTriggerForm.defaultData).evt;
            view.txt_onShowEvent.text = model.data.events.GetDv("onShowEvent", EventTriggerForm.defaultData).evt;


        }
    }

}