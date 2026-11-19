using Form;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ui.ModStoryEventTrigger;
using Ui.EventChoose;
using Ui.ModStoryEventTriggerWindow;
using UnityEngine;
using Z_Code;
using Z_Code.Form;
using Z_DataSystem;
using Z_Map;
using Z_Map.Form;
using Z_ObjectAnimator.Base;
using Z_ObjectAnimator.Core;
using Z_Text;
using Z_Texture;
using Z_Ui;
using Z_Ui.Base;
using Z_Ui.Notify;
using Z_UnitSystem.Form;

namespace Ui.ModSceneBehaviourUnit
{
    public partial class UiModSceneBehaviourUnitParam
    {
        public UnitForm.Data data;

    }
    public partial class UiModSceneBehaviourUnitModel
    {
        public UnitForm.Data data;
        
    }

    public partial class UiParamParam
    {
        public string key;
        public GameParamForm.Data data;
    }
    public partial class UiParamModel
    {
        public string key;
        public GameParamForm.Data data;
    }

    public partial class UiModSceneBehaviourUnitCtrl
    {
        private UiContainer<UiEventTriggerCtrl> triggers;
        private UiContainer<UiParamCtrl> parameters;
        private Ui.TriggerEventChoose.GlobalVariableTriggerList globalTriggers;
        private UiModStoryEventTriggerWindowCtrl eventWindow;
        private UiModStoryEventTriggerWindowParam openedWindow;

        public override void OnCreate()
        {
            view.btn_bbg.onClick.AddListener(() =>
            {
                Close();
            });
            view.btn_close.onClick.AddListener(() =>
            {
                Close();
            });
            view.btn_delete.onClick.AddListener(DeleteUnit);
            parameters = new UiContainer<UiParamCtrl>(this, view.go_param);
            triggers = new UiContainer<UiEventTriggerCtrl>(this, view.go_trigger);
            globalTriggers = new Ui.TriggerEventChoose.GlobalVariableTriggerList(this, view.go_triggerEventChoose);
        }
        public override void OnShow()
        {
            model.data = param?.data;
            if(!(model.data?.unit is MapUnit))
            {
                Close();
                return;
            }
            Refresh();

        }

        public override void OnHide()
        {
            globalTriggers?.Clear();
            ClearParams();
            foreach (var binding in triggers.paramLst)
                ((UiEventTriggerCtrl)triggers.Get(binding).ctrl).Set(null);
            triggers.Clear();
            model.data = null;
            if (eventWindow?.model?.prm != null && ReferenceEquals(eventWindow.model.prm, openedWindow))
                eventWindow.Close();
            eventWindow = null;
            openedWindow = null;
        }

        internal void OpenTrigger(string key)
        {
            if (!(model.data?.unit is MapUnit unit))
                return;
            var data = model.data;
            var map = MapManager.instance.data;
            var scene = ModManager.instance.sceneCtrl;
            var events = unit.evtDic;
            events.TryGetValue(key, out var local);
            bool IsValid() => ReferenceEquals(model.data, data)
                && ReferenceEquals(MapManager.instance.data, map)
                && UnitForm.DataByUid.TryGetValue(data.uid, out var current)
                && ReferenceEquals(current, data) && ReferenceEquals(unit.evtDic, events);
            void Mutate(System.Action change)
            {
                if (!IsValid()) return;
                using (scene.BeginOperation())
                {
                    scene.Track(data);
                    change();
                    key = local?.name ?? key;
                    unit.evtDic = events;
                }
            }
            void EnsureLocal()
            {
                if (!events.TryGetValue(key, out local))
                    events[key] = local = GameEventController.CreateTrigger(key);
            }
            bool isGlobal = key.StartsWith(Ui.TriggerEventChoose.GlobalVariableTriggerEditor.Prefix, System.StringComparison.Ordinal);
            bool created = isGlobal && local == null && unit.GetPrototypeEvt(key) == null;
            // An explicit global-variable choice creates an editable local binding.
            // Merely viewing inherited/fixed triggers must not copy them into extra.
            if (created) Mutate(EnsureLocal);
            var configs = isGlobal
                ? Ui.TriggerEventChoose.GlobalVariableTriggerEditor.Create(events, IsValid, Mutate,
                    candidate => unit.GetPrototypeEvt(candidate) == null).configs
                : null;
            if (isGlobal && unit.GetPrototypeEvt(key) != null)
                configs[0].prmChangeAct = null;
            openedWindow = new UiModStoryEventTriggerWindowParam
            {
                trigger = unit.GetEvt(key) ?? local ?? GameEventController.CreateTrigger(key),
                getTrigger = () => unit.GetEvt(key) ?? local ?? GameEventController.CreateTrigger(key),
                getInheritedEventCount = () => unit.GetPrototypeEvt(key)?.evt?.Count ?? 0,
                configs = configs,
                isValid = IsValid,
                canDeleteTrigger = () => events.ContainsKey(key),
                deleteTrigger = () => Mutate(() => { events.Remove(key); local = null; }),
                changeTrigger = change => Mutate(() =>
                {
                    EnsureLocal();
                    change(local);
                })
            };
            eventWindow = UiManager.instance.ShowUi<UiModStoryEventTriggerWindowCtrl>(openedWindow)?.ctrl
                as UiModStoryEventTriggerWindowCtrl;
            if (created) Z_EventHelper.Invoke(new EventModifyEvent());
        }
        private void DeleteUnit()
        {
            var data = model.data;
            if (!(data?.unit is MapUnit))
            {
                Close();
                return;
            }

            using (ModManager.instance.sceneCtrl.BeginOperation())
            {
                ModManager.instance.sceneCtrl.Track(data);
                if (data is ItemUnitForm.Data itemData)
                    MapManager.instance.RemoveItem(itemData);
                else if (data is ObjectUnitForm.Data objectData)
                    MapManager.instance.RemoveObject(objectData);
                else if (data is CharacterUnitForm.Data characterData)
                    MapManager.instance.RemoveCharacter(characterData);
                else if (data is TileUnitForm.Data tileData)
                    MapManager.instance.RemoveTile(tileData);
            }
            model.data = null;
            Close();
        }
        public void Refresh()
        {
            view.txt_name.text =model.data.name;
            RefreshParams();
            foreach (var binding in triggers.paramLst)
                ((UiEventTriggerCtrl)triggers.Get(binding).ctrl).Set(null);
            triggers.Clear();
            foreach (var key in UiSceneEventChooseList.TriggerKeys)
                triggers.Add(new UiEventTriggerParam { key = key });
            triggers.Refresh();
            var data = model.data;
            var map = MapManager.instance.data;
            globalTriggers.Render((MapUnit)data.unit, () => ReferenceEquals(model.data, data)
                && ReferenceEquals(MapManager.instance.data, map)
                && UnitForm.DataByUid.TryGetValue(data.uid, out var current) && ReferenceEquals(current, data), OpenTrigger);
            UiManager.Rebuild(gameObject, true);
        }
        // 参数默认显示原型（product）的值，改过的参数才作为 override 存在场景单位上。
        private void RefreshParams()
        {
            ClearParams();
            var info = (model.data?.unit as MapUnit)?.paramInfo;
            if (info != null)
                foreach (var pair in info.OrderBy(pair => pair.Value?.uid ?? 0).ThenBy(pair => pair.Key))
                    parameters.Add(new UiParamParam { key = pair.Key, data = pair.Value });
            parameters.Refresh();
        }
        private void ClearParams()
        {
            foreach (var binding in parameters.paramLst)
                ((UiParamCtrl)parameters.Get(binding).ctrl).Set(null);
            parameters.Clear();
        }
        internal void SetParamValue(string key, GameParamForm.Data value)
        {
            if (!(model.data?.unit is MapUnit unit) || string.IsNullOrEmpty(key) || value == null)
                return;
            var data = model.data;
            var map = MapManager.instance.data;
            if (!ReferenceEquals(MapManager.instance.data, map)
                || !UnitForm.DataByUid.TryGetValue(data.uid, out var current) || !ReferenceEquals(current, data))
                return;
            var scene = ModManager.instance.sceneCtrl;
            using (scene?.BeginOperation())
            {
                scene?.Track(data);
                unit.SetParamOverride(key, value);
            }
            Refresh();
        }
       
    }

    public partial class UiParamCtrl
    {
        public override void OnCreate()
        {
            view.btn_value.onClick.AddListener(() =>
            {
                var data = model.data;
                if (data == null)
                    return;
                ModManager.instance.assetCtrl.ChooseBasicCmd(data.GetValue(), () =>
                {
                    if (ReferenceEquals(model.data, data))
                        parent.SetParamValue(model.key, data);
                });
            });
        }
        public override void OnShow() => Set(param);

        public void Set(UiParamParam binding)
        {
            param = binding;
            model.key = binding?.key;
            model.data = binding?.data;
            if (model.data != null)
                Refresh();
        }
        private void Refresh()
        {
            view.txt_.text = model.data.name;
            view.txt_value.text = model.data.GetValue().GetBoxContent();
        }
    }

    public partial class UiEventTriggerParam
    {
        public string key;
    }

    public partial class UiEventTriggerModel
    {
        public UiEventTriggerParam prm;
    }

    public partial class UiEventTriggerCtrl
    {
        public override void OnCreate()
        {
            view.btn_.onClick.AddListener(() =>
            {
                if (model.prm != null)
                    parent.OpenTrigger(model.prm.key);
            });
        }

        public override void OnShow() => Set(param);

        public void Set(UiEventTriggerParam binding)
        {
            param = binding;
            model.prm = binding;
            if (binding != null)
                view.txt_.oriText = binding.key;
        }
    }
}
