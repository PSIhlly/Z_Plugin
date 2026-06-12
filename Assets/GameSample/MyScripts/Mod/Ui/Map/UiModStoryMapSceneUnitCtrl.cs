using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;
using Z_DataSystem;
using UnityEngine;
using Z_Ui;
using Z_DataSystem.Form;
using Ui.EventCustomTrigger;
using Z_Code.Form;
using Z_DesignStyle;
using Z_String;
using Z_Text;
using Z_Ui.Notify;
using Ui.ModStoryEventTriggerWindow;

namespace Ui.ModStory.ModStoryMap.ModStoryMapScene.ModStoryMapSceneUnit
{

    public partial class UiModStoryMapSceneUnitParam
    {
        public SceneForm.Data data;
    }
    public partial class UiModStoryMapSceneUnitModel
    {
        public SceneForm.Data data;
    }
    public partial class UiModStoryMapSceneUnitCtrl : IZ_Listener<AssetEvent>, IZ_Listener<EventModifyEvent>
    {

        Dictionary<string, EventTriggerForm.Data> dic => model.data.events;
        public override void OnCreate()
        {
            view.btn_map.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ImportSceneMiniMap(model.data.name);
            });
            view.ipt_name.onFinishInput += (s) =>
            {
                model.data.name = s;
                Refresh();
            };
            view.btn_delete.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.DeleteScene(model.data.uid);
                parent.SelPage(0);
            });
            view.btn_edit.onClick.AddListener(() =>
            {
                Main2StoryManager.instance.StartLoadSceneUgc(model.data.uid);
                UiManager.instance.CloseUi<UiModStoryCtrl>();
            });
            view.btn_hideInLargeMap.onClick.AddListener(() => {
                model.data.hideInLargeMap = !model.data.hideInLargeMap;
                Refresh();
            });
        }

        public void OnEvent(AssetEvent evt)
        {
            if (active)
                Refresh();
        }

        public override void OnShow()
        {
            this.Register < AssetEvent > ();
            this.Register <EventModifyEvent> ();
            model.data = param.data;
            Refresh();
        }
        public override void Close()
        {
            base.Close();
            this.Unregister<AssetEvent>();
            this.Unregister<EventModifyEvent>();
        }
        public void Refresh()
        {
            view.img_map.BindTexData(TexAssetForm.DataById[model.data.miniMap]);
            view.ipt_name.Set(model.data.name);
            view.sta_hideInLargeMap.ChangeState(model.data.hideInLargeMap?1:0);
            RefreshEvents();
        }

        public void RefreshEvents()
        {
            view.model_EventChooseEnter.Set(new EventChoose.UiEventChooseParam() { dic = dic, key = "onEnterEvent" });
            view.model_EventChoosePerSecond.Set(new EventChoose.UiEventChooseParam() { dic = dic, key = "onPerSecondEvent" });
            view.model_EventChoosePerSecond.Set(new EventChoose.UiEventChooseParam() { dic = dic, key = "onLeaveEvent" });

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
            RefreshEvents();
        }
    }
}
