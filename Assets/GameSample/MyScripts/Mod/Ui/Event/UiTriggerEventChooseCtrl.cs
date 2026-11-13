using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Form;
using Ui.EventCustomTrigger;
using Ui.ModStoryEventTriggerWindow;
using UnityEngine;
using Z_Code.Form;
using Z_DesignStyle;
using Z_Map;
using Z_Text;
using Z_Ui;
using Z_Ui.Base;
using Z_Ui.Notify;

namespace Ui.TriggerEventChoose
{
    // Shared with the existing Scene-level custom-trigger editor.
    public static class GlobalVariableTriggerEditor
    {
        public const string Prefix = "onGlobalVarChangeEvent$";
        public static string Key(int uid) => Prefix + uid.ToString(CultureInfo.InvariantCulture);
        public static string Name(string key)
        {
            if (key == null || !key.StartsWith(Prefix, StringComparison.Ordinal)) return key ?? "";
            var suffix = key.Substring(Prefix.Length);
            if (suffix.Length == 0) return TextManager.instance.GetTxt("Any");
            return int.TryParse(suffix, NumberStyles.None, CultureInfo.InvariantCulture, out int uid)
                && EventGlobalVariableForm.DataByUid.TryGetValue(uid, out var variable) ? variable.name : suffix;
        }

        public static void Choose(Func<bool> valid, Func<string, bool> chosen)
        {
            if (!valid()) return;
            var items = new EntryItem();
            var any = new EntryItem { content = TextManager.instance.GetTxt("Any"), parent = items, deepth = 1, id = 0 };
            items.subs.Add("", any);
            var choices = new Dictionary<EntryItem, EventGlobalVariableForm.Data>();
            foreach (var variable in EventGlobalVariableForm.DataByUid.Values.OrderBy(value => value.uid))
            {
                var item = new EntryItem { content = variable.name, parent = items, deepth = 1, id = variable.uid };
                items.subs.Add(variable.uid.ToString(CultureInfo.InvariantCulture), item);
                choices.Add(item, variable);
            }
            NotifyManager.instance.AddChoose(TextManager.instance.GetTxt("Choose param"), true, selected =>
            {
                if (!valid()) return true;
                if (ReferenceEquals(selected, any)) return chosen(Prefix);
                if (selected == null || !choices.TryGetValue(selected, out var variable)
                    || !EventGlobalVariableForm.DataByUid.TryGetValue(variable.uid, out var current)
                    || !ReferenceEquals(current, variable)) return true;
                return chosen(Key(variable.uid));
            }, items);
        }

        public static UiEventCustomTriggerParam Create(Dictionary<string, EventTriggerForm.Data> events, Func<bool> valid,
            Action<Action> mutate = null, Func<string, bool> canRenameTo = null)
        {
            return new UiEventCustomTriggerParam
            {
                dic = events, defaultKey = Prefix,
                newKeyFunc = () => !valid() ? null : !events.ContainsKey(Prefix) ? Prefix
                    : EventGlobalVariableForm.DataByUid.Values.OrderBy(value => value.uid)
                        .Select(value => Key(value.uid)).FirstOrDefault(key => !events.ContainsKey(key)),
                configs = new List<TriggerConfig> { new TriggerConfig
                {
                    GetNameFunc = Name,
                    prmChangeAct = trigger =>
                    {
                        var oldKey = trigger?.name;
                        bool IsCurrent() => valid() && oldKey != null && trigger.name == oldKey
                            && events.TryGetValue(oldKey, out var current) && ReferenceEquals(current, trigger);
                        Choose(IsCurrent, key =>
                        {
                            if (canRenameTo != null && !canRenameTo(key)) return false;
                            if (events.TryGetValue(key, out var other) && !ReferenceEquals(other, trigger)) return false;
                            if (key == oldKey) return true;
                            void Rename() { events.Remove(oldKey); trigger.name = key; events.Add(key, trigger); }
                            if (mutate == null) Rename(); else mutate(Rename);
                            Z_EventHelper.Invoke(new EventModifyEvent());
                            return true;
                        });
                    }
                } }
            };
        }
    }

    public partial class UiTriggerEventChooseParam
    {
        public Func<IEnumerable<string>> getKeys;
        public Func<bool> isValid;
        public Action<string> open;
        public Action rebuild;
    }
    public partial class UiTriggerEventChooseModel { public UiTriggerEventChooseParam prm; }
    public partial class UiTriggerEventChooseCtrl : IZ_Listener<EventModifyEvent>
    {
        private UiContainer<UiTriggerCtrl> rows;
        public override void OnCreate()
        {
            this.Register();
            rows = new UiContainer<UiTriggerCtrl>(this, view.go_trigger);
        }
        public override void OnShow() { if (param != null) Set(param); }
        public void Set(UiTriggerEventChooseParam binding)
        {
            param = binding; model.prm = binding;
            if (rows != null) Refresh();
        }
        public override void OnHide() => Set(null);
        public void Refresh()
        {
            foreach (var binding in rows.paramLst) ((UiTriggerCtrl)rows.Get(binding).ctrl).Set(null);
            rows.Clear();
            var settings = model.prm;
            if (settings == null || !settings.isValid())
            {
                UiManager.Rebuild(gameObject, true);
                settings?.rebuild?.Invoke();
                return;
            }
            foreach (var key in settings.getKeys().Where(key => key.StartsWith(GlobalVariableTriggerEditor.Prefix,
                         StringComparison.Ordinal)).Distinct().OrderBy(key => key, StringComparer.Ordinal))
                rows.Add(new UiTriggerParam { key = key, owner = settings });
            rows.Add(new UiTriggerParam { owner = settings });
            rows.Refresh();
            UiManager.Rebuild(gameObject.transform.parent==null?gameObject:gameObject.transform.parent.gameObject, true);
            settings.rebuild?.Invoke();
        }
        internal void Open(UiTriggerParam row)
        {
            if (!active || row == null || !ReferenceEquals(model.prm, row.owner) || !row.owner.isValid()) return;
            var settings = row.owner;
            if (row.key != null) settings.open(row.key);
            // The chooser outlives this pooled row. A same-owner layout refresh
            // must not cancel it; the binding's owner/map lifetime remains authoritative.
            else GlobalVariableTriggerEditor.Choose(settings.isValid, key => { settings.open(key); return true; });
        }
        public void OnEvent(EventModifyEvent evt) { if (active) Refresh(); }
    }
    public partial class UiTriggerParam { public string key; public UiTriggerEventChooseParam owner; }
    public partial class UiTriggerModel { public UiTriggerParam prm; }
    public partial class UiTriggerCtrl
    {
        public override void OnCreate() => view.btn_.onClick.AddListener(() => parent.Open(model.prm));
        public override void OnShow() => Set(param);
        public override void OnHide() => Set(null);
        public void Set(UiTriggerParam binding)
        {
            param = binding; model.prm = binding;
            if (binding == null) return;
            view.sta_.ChangeState(binding.key == null ? 0 : 1);
            view.txt_.text = binding.key == null ? "" : TextManager.instance.GetTxt("onGlobalVarChangeEvent")
                + "-" + GlobalVariableTriggerEditor.Name(binding.key);
        }
    }

    // Render the authored Model as a pooled template, shared by all five owners.
    public sealed class GlobalVariableTriggerList
    {
        private readonly UiCtrl owner;
        private readonly UiContainer<UiTriggerEventChooseCtrl> rows;
        private UiModStoryEventTriggerWindowCtrl window;
        private UiModStoryEventTriggerWindowParam opened;
        private object binding;
        private object bindingSource;
        public GlobalVariableTriggerList(UiCtrl owner, GameObject template)
        { this.owner = owner; rows = new UiContainer<UiTriggerEventChooseCtrl>(owner, template); }
        public void Render(Dictionary<string, EventTriggerForm.Data> events, Func<bool> valid)
        {
            if (events == null) { Clear(); return; }
            var token = Prepare(events);
            bool IsCurrent() => ReferenceEquals(binding, token) && owner.active && valid();
            var settings = GlobalVariableTriggerEditor.Create(events, IsCurrent);
            Add(() => events.Keys, IsCurrent, key =>
            {
                if (!events.TryGetValue(key, out var trigger)) events[key] = trigger = GameEventController.CreateTrigger(key);
                opened = new UiModStoryEventTriggerWindowParam { trigger = trigger, dic = events,
                    configs = settings.configs, isValid = IsCurrent };
                window = UiManager.instance.ShowUi<UiModStoryEventTriggerWindowCtrl>(opened)?.ctrl as UiModStoryEventTriggerWindowCtrl;
                Z_EventHelper.Invoke(new EventModifyEvent());
            });
        }
        public void Render(MapUnit unit, Func<bool> valid, Action<string> open)
        {
            var token = Prepare(unit);
            Add(() => (unit.GetPrototypeEvents()?.Keys.AsEnumerable() ?? Enumerable.Empty<string>()).Concat(unit.evtDic.Keys),
                () => ReferenceEquals(binding, token) && owner.active && valid(), open);
        }
        private void Add(Func<IEnumerable<string>> keys, Func<bool> valid, Action<string> open)
        {
            rows.Add(new UiTriggerEventChooseParam { getKeys = keys, isValid = valid, open = open,
                rebuild = () => UiManager.Rebuild(owner.gameObject, true) });
            rows.Refresh();
            UiManager.Rebuild(owner.gameObject, true);
        }
        public void Clear()
        {
            binding = null;
            bindingSource = null;
            ClearRows();
            if (window?.model?.prm != null && ReferenceEquals(window.model.prm, opened)) window.Close();
            window = null; opened = null;
        }
        private object Prepare(object source)
        {
            if (!ReferenceEquals(bindingSource, source)) Clear();
            else ClearRows();
            bindingSource = source;
            return binding ?? (binding = new object());
        }
        private void ClearRows()
        {
            foreach (var param in rows.paramLst) ((UiTriggerEventChooseCtrl)rows.Get(param).ctrl).Set(null);
            rows.Clear();
        }
    }
}
