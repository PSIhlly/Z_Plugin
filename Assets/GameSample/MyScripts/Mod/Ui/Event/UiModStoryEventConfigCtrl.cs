using Form;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ui.EventCustomTrigger;
using Ui.ModStoryEventTriggerWindow;
using UnityEngine;
using Z_Code.Form;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Map.Analysis;
using Z_String;
using Z_Text;
using Z_Texture;
using Z_Ui;
using Z_Ui.Base;
using Z_Ui.Notify;

namespace Ui.ModStory.ModStoryEvent.ModStoryEventConfig
{

    public partial class UiModStoryEventConfigParam
    {
    }
    public partial class UiModStoryEventConfigModel
    {
    }
    public partial class UiModStoryEventConfigCtrl : IZ_Listener<EventModifyEvent>
    {

        Dictionary<string, EventTriggerForm.Data> dic => GameManager.instance.curProgress.events;
        public override void OnCreate()
        {
            this.Register();
        }



        public override void OnShow()
        {
            Refresh();
        }
        public void Refresh()
        {
            view.model_EventChooseBegin.Set(new EventChoose.UiEventChooseParam() { dic = dic, key = "onBeginEvent" });
            view.model_EventChoosePerSecond.Set(new EventChoose.UiEventChooseParam() { dic = dic, key = "onPerSecondEvent" });

            view.model_EventChooseCharacterParamChange.Set(new EventChoose.UiEventChooseParam() { dic = dic, key = "onCharacterParamChangeEvent" });
            view.model_EventCustomTriggerCharacterParam.Set(new UiEventCustomTriggerParam() { dic = dic, defaultKey = "onCharacterParamChangeEvent$$", configs = new List<TriggerConfig>() {
                new TriggerConfig()
                {
                    GetNameFunc = (raw) => {
                        var splits = raw.Split("$");
                        return splits[1] == "" ? TextManager.instance.GetTxt("Any character") : CharacterProductForm.DataByUid.GetDv(int.Parse(splits[1]),CharacterProductForm.defaultData).name;
                        },
                     prmChangeAct = (trigger) =>
                     {
                         var oldkey=trigger.name;
                         var any=new EntryItem(){ content = TextManager.instance.GetTxt("Any")};
                         ModManager.instance.assetCtrl.ChooseCharacter("Choose character", (data) =>
                         {
                             dic.Remove(oldkey);
                             var splits = trigger.name.Split("$");
                             
                             trigger.name = splits[0]+"$"+(data==null?"":data.uid)+"$"+splits[2];
                             dic[trigger.name]=trigger;
                             Z_EventHelper.Invoke(new EventModifyEvent());
                         },any);
                     }
                },
                new TriggerConfig()
                {
                    GetNameFunc = (raw) => {
                        var splits = raw.Split("$");
                        return splits[2] == "" ? TextManager.instance.GetTxt("Any param") : splits[2];
                        },
                     prmChangeAct = (trigger) =>
                     {
                         var oldkey=trigger.name;
                         var any=new EntryItem(){ content = TextManager.instance.GetTxt("Any")};
                         ModManager.instance.assetCtrl.ChooseCharacterParam("Choose param", (res) =>
                         {
                             dic.Remove(oldkey);
                             var splits = trigger.name.Split("$");
                             trigger.name = splits[0]+"$"+splits[1]+"$"+(any==res?"":res.content);
                             dic[trigger.name]=trigger;
                             Z_EventHelper.Invoke(new EventModifyEvent());
                         },any);
                     }
                }
            }
            });

            view.model_EventChooseGainItem.Set(new EventChoose.UiEventChooseParam() { dic = dic, key = "onGainItemEvent" });
            view.model_EventCustomTriggerGainItem.Set(new UiEventCustomTriggerParam()
            {
                dic = dic,
                defaultKey = "onGainItemEvent$",
                configs = new List<TriggerConfig>() {
                new TriggerConfig()
                {
                    GetNameFunc = (raw) => {
                        var splits = raw.Split("$");
                        return splits[1] == "" ? TextManager.instance.GetTxt("Any item") : ItemProductForm.DataByUid.GetDv(int.Parse(splits[1]),ItemProductForm.defaultData).name;
                        },
                     prmChangeAct = (trigger) =>
                     {
                         var oldkey=trigger.name;
                         var any=new EntryItem(){ content = TextManager.instance.GetTxt("Any")};
                         ModManager.instance.assetCtrl.ChooseItem("Choose item", (data) =>
                         {
                             dic.Remove(oldkey);
                             var splits = trigger.name.Split("$");
                             trigger.name = splits[0]+"$"+data.uid;
                             dic[trigger.name]=trigger;
                             Z_EventHelper.Invoke(new EventModifyEvent());
                         },any);
                     }
                }
            }
            });
            view.model_EventChooseLostItem.Set(new EventChoose.UiEventChooseParam() { dic = dic, key = "onLostItemEvent" });
            view.model_EventCustomTriggerLostItem.Set(new UiEventCustomTriggerParam()
            {
                dic = dic,
                defaultKey = "onLostItemEvent$",
                configs = new List<TriggerConfig>() {
                new TriggerConfig()
                {
                    GetNameFunc = (raw) => {
                        var splits = raw.Split("$");
                        return splits[1] == "" ? TextManager.instance.GetTxt("Any item") : ItemProductForm.DataByUid.GetDv(int.Parse(splits[1]),ItemProductForm.defaultData).name;
                        },
                     prmChangeAct = (trigger) =>
                     {
                         var oldkey=trigger.name;
                         var any=new EntryItem(){ content = TextManager.instance.GetTxt("Any")};
                         ModManager.instance.assetCtrl.ChooseItem("Choose item", (data) =>
                         {
                             dic.Remove(oldkey);
                             var splits = trigger.name.Split("$");
                             trigger.name = splits[0]+"$"+data.uid;
                             dic[trigger.name]=trigger;
                             Z_EventHelper.Invoke(new EventModifyEvent());
                         },any);
                     }
                }
            }
            });
        }
        public void OnEvent(EventModifyEvent evt)
        {
            Refresh();
        }
    }

}