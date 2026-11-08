using Form;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Ui.ModStoryEventTrigger;
using UnityEngine;
using Z_Code.Form;
using Z_DataSystem;
using Z_DesignStyle;
using Z_Map;
using Z_Map.Form;
using Z_Text;
using Z_Texture;
using Z_Ui;
using Z_Ui.Base;
using Z_Ui.Notify;
using Z_UnitSystem.Form;

namespace Ui.ModStoryEventTriggerWindow
{
    public class TriggerConfig
    {
        public Action<EventTriggerForm.Data> prmChangeAct;
        public Func<string,string> GetNameFunc;
    }
    public partial class UiModStoryEventTriggerWindowParam
    {
        public EventTriggerForm.Data trigger;

        public Dictionary<string, EventTriggerForm.Data> dic;
        public List<TriggerConfig> configs;
        // Unit windows display inherited events first but mutate only their local suffix.
        public Func<EventTriggerForm.Data> getTrigger;
        public Func<int> getInheritedEventCount;
        public Action<Action<EventTriggerForm.Data>> changeTrigger;
        public Func<bool> canDeleteTrigger;
        public Action deleteTrigger;
        public Func<bool> isValid;
    }
    public partial class UiModStoryEventTriggerWindowModel
    {
        public UiModStoryEventTriggerWindowParam prm;
        
    }

    public partial class UiModStoryEventTriggerWindowCtrl:IZ_Listener<EventModifyEvent>
    {
        UiContainer<UiEventCtrl> evtCon;
        UiContainer<UiPrmCtrl> prmCon;
        public override void OnCreate()
        {
            this.Register();
            evtCon = new UiContainer<UiEventCtrl>(this, view.go_event);
            prmCon = new UiContainer<UiPrmCtrl>(this, view.go_prm);
            view.btn_bbg.onClick.AddListener(() =>
            {
                Close();
            });
            view.btn_close.onClick.AddListener(() =>
            {
                Close();
            });
            view.btn_delete.onClick.AddListener(() =>
            {
                if (!CanDeleteTrigger())
                    return;
                if (model.prm.deleteTrigger != null)
                    model.prm.deleteTrigger();
                else
                    model.prm.dic.Remove(model.prm.trigger.name);

                Z_EventHelper.Invoke(new EventModifyEvent());
                Close();
            });
        }
        public override void OnShow()
        {
            model.prm = param;
            Refresh();

        }
        public void Refresh()
        {
            if (model.prm == null)
                return;
            if (model.prm.isValid != null && !model.prm.isValid())
            {
                Close();
                return;
            }
            if (model.prm.getTrigger != null)
                model.prm.trigger = model.prm.getTrigger();
            view.btn_delete.gameObject.SetActive(CanDeleteTrigger());
            bool existParams = model.prm.configs != null && model.prm.configs.Count > 0;
            view.go_params.SetActive(existParams);

            prmCon.Clear();
            if (existParams)
            {
                foreach (var config in model.prm.configs)
                {
                    prmCon.Add(new UiPrmParam() { config = config });
                }
            }
            prmCon.Refresh();

            view.txt_name.oriText = model.prm.trigger.name.Split('$')[0];
            evtCon.Clear();
            for (int i = 0; i < model.prm.trigger.evt.Count; i++)
            {
                evtCon.Add(new UiEventParam() { uid = model.prm.trigger.evt[i], index = i,
                    owner = model.prm });
            }
            
            evtCon.Add(new UiEventParam() { uid = -1, index = -1, owner = model.prm });
            evtCon.Refresh();
            UiManager.Rebuild(gameObject, true);
        }

        public void OnEvent(EventModifyEvent evt)
        {
            if (active)
                Refresh();
        }

        public override void OnHide()
        {
            evtCon.Clear();
            prmCon.Clear();
            model.prm = null;
            param = null;
        }

        internal bool IsCurrent(UiEventParam binding)
        {
            return binding != null && active && ReferenceEquals(binding.owner, model.prm)
                && (model.prm.isValid == null || model.prm.isValid());
        }

        internal int InheritedEventCount => model.prm?.getInheritedEventCount?.Invoke() ?? 0;

        private bool CanDeleteTrigger() => model.prm != null
            && (model.prm.isValid == null || model.prm.isValid())
            && (model.prm.canDeleteTrigger?.Invoke() ?? model.prm.dic != null);

        internal void ChangeEvents(UiEventParam binding, Action<EventTriggerForm.Data, int> change)
        {
            if (!IsCurrent(binding)) return;
            int index = binding.index - InheritedEventCount;
            if (binding.owner.changeTrigger != null)
                binding.owner.changeTrigger(trigger => change(trigger, index));
            else
                change(binding.owner.trigger, index);
        }
    }


    public partial class UiPrmParam
    {
        public TriggerConfig config;
    }
    public partial class UiPrmModel
    {
        public UiPrmParam prm;
    }
    public partial class UiPrmCtrl
    {

        public override void OnCreate()
        {
            view.btn_edit.onClick.AddListener(() =>
            {
                model.prm?.config?.prmChangeAct?.Invoke(parent.model.prm.trigger);
            });
            
        }
        public override void OnShow()
        {
            model.prm = param;
            Refresh();
        }
        public void Refresh()
        {
            view.txt_.text = model.prm.config.GetNameFunc(parent.model.prm.trigger.name);
            // The authored label is inside this Button. Keep it visible for
            // read-only instance parameters and disable only interaction.
            view.btn_edit.gameObject.SetActive(true);
            view.btn_edit.interactable = model.prm.config.prmChangeAct != null;
        }
    }

    public partial class UiEventParam
    {
        public int uid;
        public int index;
        public UiModStoryEventTriggerWindowParam owner;
    }
    public partial class UiEventModel
    {
        public UiEventParam prm;
    }
    public partial class UiEventCtrl
    {

        public override void OnCreate()
        {
            view.btn_add.onClick.AddListener(() =>
            {
                var binding = model.prm;
                if (!parent.IsCurrent(binding) || binding.index != -1)
                    return;
                ModManager.instance.assetCtrl.ChooseEvent(SceneEventType.All, CmdTypeDataForm.defaultData.name, TextManager.instance.GetTxt(binding.owner.trigger.name.Split("$")[0]), (res) =>
                {
                    if (!ReferenceEquals(model.prm, binding) || !parent.IsCurrent(binding))
                        return;
                    parent.ChangeEvents(binding, (trigger, index) => trigger.evt.Add(res.id));
                    Z_EventHelper.Invoke(new EventModifyEvent());
                });
            });
            view.btn_delete.onClick.AddListener(() =>
            {
                if (!CanEdit(model.prm))
                    return;
                parent.ChangeEvents(model.prm, (trigger, index) => trigger.evt.RemoveAt(index));
                Z_EventHelper.Invoke(new EventModifyEvent());
            });
            view.btn_edit.onClick.AddListener(() =>
            {
                var binding = model.prm;
                if (!CanEdit(binding))
                    return;
                ModManager.instance.assetCtrl.ChooseEvent(SceneEventType.All, CmdTypeDataForm.defaultData.name, TextManager.instance.GetTxt(binding.owner.trigger.name.Split("$")[0]), (res) =>
                {
                    if (!ReferenceEquals(model.prm, binding) || !CanEdit(binding))
                        return;
                    parent.ChangeEvents(binding, (trigger, index) => trigger.evt[index] = res.id);
                    Z_EventHelper.Invoke(new EventModifyEvent());
                });
            });
        }
        public override void OnShow()
        {
            model.prm = param;
            Refresh();
        }
        public override void OnHide()
        {
            param = null;
            model.prm = null;
        }
        public void Refresh()
        {
            view.sta_.ChangeState(model.prm.uid==-1 ? 0 : 1);
            view.btn_edit.gameObject.SetActive(CanEdit(model.prm));
            view.btn_delete.gameObject.SetActive(CanEdit(model.prm));
            view.txt_.text = EventProgramDataForm.DataByUid.GetDv(model.prm.uid, EventProgramDataForm.defaultData).name;
        }

        private bool CanEdit(UiEventParam binding)
        {
            return parent.IsCurrent(binding) && binding.index >= parent.InheritedEventCount
                && binding.index < binding.owner.trigger.evt.Count
                && binding.owner.trigger.evt[binding.index] == binding.uid;
        }
    }
}
