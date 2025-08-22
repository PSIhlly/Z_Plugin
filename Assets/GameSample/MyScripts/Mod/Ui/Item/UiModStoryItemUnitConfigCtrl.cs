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

           
            view.btn_onTouchEvent.onClick.AddListener(() =>
            {
                GameManager.instance.evtCtrl.GetEvents(EventType.Item, CmdTypeDataForm.defaultData, out var sub);
                NotifyManager.instance.AddMultipleChoose(TextManager.instance.GetTxt("onTouchEvent"), false, 2, (lst) =>
                {
                    model.data.onTouchEvent = lst[2];
                    Refresh();
                    return true;
                }, sub);
            });
            view.btn_onLeaveEvent.onClick.AddListener(() =>
            {
                GameManager.instance.evtCtrl.GetEvents(EventType.Item, CmdTypeDataForm.defaultData, out var sub);
                NotifyManager.instance.AddMultipleChoose(TextManager.instance.GetTxt("onLeaveEvent"), false, 2, (lst) =>
                {
                    model.data.onLeaveEvent = lst[2];
                    Refresh();
                    return true;
                }, sub);
            });
            view.btn_onShowEvent.onClick.AddListener(() =>
            {
                GameManager.instance.evtCtrl.GetEvents(EventType.Item, CmdTypeDataForm.defaultData, out var sub);
                NotifyManager.instance.AddMultipleChoose(TextManager.instance.GetTxt("onShowEvent"), false, 2, (lst) =>
                {
                    model.data.onShowEvent = lst[2];
                    Refresh();
                    return true;
                }, sub);
            });
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
            view.btn_onUseEvent.onClick.AddListener(() =>
            {
                GameManager.instance.evtCtrl.GetEvents(EventType.Item, CmdTypeDataForm.defaultData, out var sub);
                NotifyManager.instance.AddMultipleChoose(TextManager.instance.GetTxt("onUseEvent"), false, 2, (lst) =>
                {
                    model.data.onUseEvent = lst[2];
                    Refresh();
                    return true;
                }, sub);
            });
            view.btn_onEquipEvent.onClick.AddListener(() =>
            {
                GameManager.instance.evtCtrl.GetEvents(EventType.Item, CmdTypeDataForm.defaultData, out var sub);
                NotifyManager.instance.AddMultipleChoose(TextManager.instance.GetTxt("onEquipEvent"), false, 2, (lst) =>
                {
                    model.data.onEquipEvent = lst[2];
                    Refresh();
                    return true;
                }, sub);
            });
            view.btn_onDisequipEvent.onClick.AddListener(() =>
            {
                GameManager.instance.evtCtrl.GetEvents(EventType.Item, CmdTypeDataForm.defaultData, out var sub);
                NotifyManager.instance.AddMultipleChoose(TextManager.instance.GetTxt("onDisequipEvent"), false, 2, (lst) =>
                {
                    model.data.onDisequipEvent = lst[2];
                    Refresh();
                    return true;
                }, sub);
            });
            view.btn_onTouchEvent.onClick.AddListener(() =>
            {
                GameManager.instance.evtCtrl.GetEvents(EventType.Item, CmdTypeDataForm.defaultData, out var sub);
                NotifyManager.instance.AddMultipleChoose(TextManager.instance.GetTxt("onTouchEvent"), false, 2, (lst) =>
                {
                    model.data.onTouchEvent = lst[2];
                    Refresh();
                    return true;
                }, sub);
            });
            view.btn_onLeaveEvent.onClick.AddListener(() =>
            {
                GameManager.instance.evtCtrl.GetEvents(EventType.Item, CmdTypeDataForm.defaultData, out var sub);
                NotifyManager.instance.AddMultipleChoose(TextManager.instance.GetTxt("onLeaveEvent"), false, 2, (lst) =>
                {
                    model.data.onLeaveEvent = lst[2];
                    Refresh();
                    return true;
                }, sub);
            });
            view.btn_onShowEvent.onClick.AddListener(() =>
            {
                GameManager.instance.evtCtrl.GetEvents(EventType.Item, CmdTypeDataForm.defaultData, out var sub);
                NotifyManager.instance.AddMultipleChoose(TextManager.instance.GetTxt("onShowEvent"), false, 2, (lst) =>
                {
                    model.data.onShowEvent = lst[2];
                    Refresh();
                    return true;
                }, sub);
            });
            view.btn_part.onClick.AddListener(() =>
            {
                var lst = new List<(string, Sprite)>();
                foreach (EquipPartType part in Enum.GetValues(typeof(EquipPartType)))
                {
                    lst.Add((TextManager.instance.GetTxt(part.ToString()), null));
                }
                NotifyManager.instance.AddChoose(TextManager.instance.GetTxt("chooseEquipPart"), false, (res) =>
                {
                    model.data.equip = (EquipPartType)res;
                    Refresh();
                    return true;
                }, lst);
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
            view.txt_onUseEvent.text = model.data.onUseEvent;
            view.txt_onEquipEvent.text = model.data.onEquipEvent;
            view.txt_onDisequipEvent.text = model.data.onDisequipEvent;
            view.ipt_price.Set(model.data.price.ToString());
            view.txt_onTouchEvent.text = model.data.onTouchEvent;
            view.txt_onLeaveEvent.text = model.data.onLeaveEvent;
            view.txt_onShowEvent.text = model.data.onShowEvent;
        }
    }

}