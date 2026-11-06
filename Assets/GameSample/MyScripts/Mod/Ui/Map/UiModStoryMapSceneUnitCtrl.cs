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
using Z_Ui.Loading;
using Ui.ModStoryEventTriggerWindow;
using System.Globalization;

namespace Ui.ModStory.ModStoryMap.ModStoryMapScene.ModStoryMapSceneUnit
{

    public partial class UiModStoryMapSceneUnitParam
    {
        public SceneForm.Data data;
    }
    public partial class UiModStoryMapSceneUnitModel
    {
        public SceneForm.Data data;
        public bool startingScene;
    }
    public partial class UiModStoryMapSceneUnitCtrl : IZ_Listener<AssetEvent>, IZ_Listener<EventModifyEvent>
    {
        private const string GlobalVarTriggerPrefix = "onGlobalVarChangeEvent$";

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
            view.btn_edit.onClick.AddListener(async () =>
            {
                if (model.startingScene)
                    return;

                model.startingScene = true;
                int storyId = GameManager.instance.curStory.id;
                int sceneId = model.data.uid;
                try
                {
                    await LoadingManager.instance.RunAsync("modSceneEdit", async () =>
                    {
                        var loading = LoadingManager.instance;
                        string work = "modSceneSave:" + Guid.NewGuid().ToString("N");
                        loading.AddLoadItem(work, 1);
                        try
                        {
                            loading.RunStep(work, "Save authored Story", () => GameManager.instance.saveCtrl.SaveCoreStory(storyId));
                            await Main2StoryManager.instance.StartLoadSceneUgc(sceneId);
                            UiManager.instance.CloseUi<UiModStoryCtrl>();
                        }
                        finally { loading.RemoveLoadItem(work); }
                    });
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
                finally
                {
                    model.startingScene = false;
                }
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
            view.btn_hideInLargeMap.transform.parent.gameObject.SetActive(GameManager.instance.curProgress.enableLargeMap);
            RefreshEvents();
        }

        public void RefreshEvents()
        {
            view.model_EventChooseEnter.Set(new EventChoose.UiEventChooseParam() { dic = dic, key = "onEnterEvent" });
            view.model_EventChoosePerSecond.Set(new EventChoose.UiEventChooseParam() { dic = dic, key = "onPerSecondEvent" });
            view.model_EventChoosePerSecond.Set(new EventChoose.UiEventChooseParam() { dic = dic, key = "onLeaveEvent" });

            view.model_EventChooseCharacterParamChange.Set(new EventChoose.UiEventChooseParam() { dic = dic, key = "onCharacterParamChangeEvent" });
            view.model_EventCustomTriggerGlobalVar.Set(CreateGlobalVarTriggerParam());
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
                         ModManager.instance.assetCtrl.ChooseCharacter(TextManager.instance.GetTxt("Choose character"), (data) =>
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
                         ModManager.instance.assetCtrl.ChooseItem(TextManager.instance.GetTxt("Choose item"), (data) =>
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
                         ModManager.instance.assetCtrl.ChooseItem(TextManager.instance.GetTxt("Choose item"), (data) =>
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

        private UiEventCustomTriggerParam CreateGlobalVarTriggerParam()
        {
            var scene = model.data;
            var events = scene?.events;
            return new UiEventCustomTriggerParam
            {
                dic = events,
                defaultKey = GlobalVarTriggerPrefix,
                newKeyFunc = () =>
                {
                    if (!IsCurrentGlobalVarScene(scene, events))
                        return null;
                    if (!events.ContainsKey(GlobalVarTriggerPrefix))
                        return GlobalVarTriggerPrefix;
                    foreach (var variable in EventGlobalVariableForm.DataByUid.Values.OrderBy(data => data.uid))
                    {
                        var key = GlobalVarTriggerKey(variable.uid);
                        if (!events.ContainsKey(key))
                            return key;
                    }
                    return null;
                },
                configs = new List<TriggerConfig>
                {
                    new TriggerConfig
                    {
                        GetNameFunc = GetGlobalVarTriggerName,
                        prmChangeAct = trigger => ChooseGlobalVarTrigger(scene, events, trigger)
                    }
                }
            };
        }

        private static string GlobalVarTriggerKey(int uid) =>
            GlobalVarTriggerPrefix + uid.ToString(CultureInfo.InvariantCulture);

        private static string GetGlobalVarTriggerName(string key)
        {
            if (key == null || !key.StartsWith(GlobalVarTriggerPrefix, StringComparison.Ordinal))
                return key ?? string.Empty;
            var suffix = key.Substring(GlobalVarTriggerPrefix.Length);
            if (suffix.Length == 0)
                return TextManager.instance.GetTxt("Any");
            return int.TryParse(suffix, NumberStyles.None, CultureInfo.InvariantCulture, out var uid)
                && EventGlobalVariableForm.DataByUid.TryGetValue(uid, out var variable)
                ? variable.name : suffix;
        }

        private bool IsCurrentGlobalVarScene(SceneForm.Data scene, Dictionary<string, EventTriggerForm.Data> events) =>
            scene != null && events != null && ReferenceEquals(model.data, scene)
            && ReferenceEquals(scene.events, events)
            && SceneForm.DataByUid.TryGetValue(scene.uid, out var current) && ReferenceEquals(current, scene);

        private void ChooseGlobalVarTrigger(SceneForm.Data scene,
            Dictionary<string, EventTriggerForm.Data> events, EventTriggerForm.Data trigger)
        {
            var oldKey = trigger?.name;
            if (!IsCurrentGlobalVarScene(scene, events) || oldKey == null
                || !events.TryGetValue(oldKey, out var current) || !ReferenceEquals(current, trigger))
                return;

            var items = new EntryItem();
            var any = new EntryItem { content = TextManager.instance.GetTxt("Any"), parent = items, deepth = 1, id = 0 };
            items.subs.Add(string.Empty, any);
            var choices = new Dictionary<EntryItem, EventGlobalVariableForm.Data>();
            foreach (var variable in EventGlobalVariableForm.DataByUid.Values.OrderBy(data => data.uid))
            {
                var item = new EntryItem { content = variable.name, parent = items, deepth = 1, id = variable.uid };
                // Internal keys are identities, so a user-authored name matching Any stays selectable.
                items.subs.Add(variable.uid.ToString(CultureInfo.InvariantCulture), item);
                choices.Add(item, variable);
            }

            NotifyManager.instance.AddChoose(TextManager.instance.GetTxt("Choose param"), true, selected =>
            {
                if (!IsCurrentGlobalVarScene(scene, events) || trigger.name != oldKey
                    || !events.TryGetValue(oldKey, out var original) || !ReferenceEquals(original, trigger))
                    return true;

                var key = GlobalVarTriggerPrefix;
                if (!ReferenceEquals(selected, any))
                {
                    if (selected == null || !choices.TryGetValue(selected, out var variable)
                        || !EventGlobalVariableForm.DataByUid.TryGetValue(variable.uid, out var registered)
                        || !ReferenceEquals(registered, variable))
                        return true;
                    key = GlobalVarTriggerKey(variable.uid);
                }
                if (events.TryGetValue(key, out var duplicate) && !ReferenceEquals(duplicate, trigger))
                    return false;
                if (key == oldKey)
                    return true;

                events.Remove(oldKey);
                trigger.name = key;
                events.Add(key, trigger);
                Z_EventHelper.Invoke(new EventModifyEvent());
                return true;
            }, items);
        }

        public void OnEvent(EventModifyEvent evt)
        {
            RefreshEvents();
        }
    }
}
