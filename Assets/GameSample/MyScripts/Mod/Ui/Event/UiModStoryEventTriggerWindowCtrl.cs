using Form;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Ui.ModStoryEventTrigger;
using UnityEngine;
using Z_Code.Form;
using Z_DataSystem;
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
    public partial class UiModStoryEventTriggerWindowParam
    {
        public EventTriggerForm.Data trigger;
        public Action onChange;

    }
    public partial class UiModStoryEventTriggerWindowModel
    {
        public UiModStoryEventTriggerWindowParam prm;
        
    }

    public partial class UiModStoryEventTriggerWindowCtrl
    {
        UiContainer<UiEventCtrl> con;
        public override void OnCreate()
        {
            con = new UiContainer<UiEventCtrl>(view.go_event);
            view.btn_bbg.onClick.AddListener(() =>
            {
                Close();
            });
            view.btn_close.onClick.AddListener(() =>
            {
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
            view.txt_name.text = AssetManager.GetKeyName(model.prm.trigger.name);
            con.Clear();
            foreach(var nm in model.prm.trigger.evt)
            {
                con.Add(new UiEventParam() { nm = nm });
            }
            con.Add(new UiEventParam() { nm = null });
            con.Refresh();

        }
       
    }
    public partial class UiEventParam
    {
        public string nm;
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
                ModManager.instance.assetCtrl.ChooseEvent(parent.model.prm.trigger.name, SceneEventType.All, CmdTypeDataForm.defaultData.name, TextManager.instance.GetTxt(parent.model.prm.trigger.name), (res) =>
                {
                    parent.model.prm.trigger.evt.Add(res.content);
                    parent.model.prm.onChange?.Invoke();
                    parent.Refresh();
                });
            });
            view.btn_delete.onClick.AddListener(() =>
            {
                parent.model.prm.trigger.evt.Remove(model.prm.nm);
                parent.model.prm.onChange?.Invoke();
                parent.Refresh();
            });
            view.btn_edit.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ChooseEvent(parent.model.prm.trigger.name, SceneEventType.All, CmdTypeDataForm.defaultData.name, TextManager.instance.GetTxt(parent.model.prm.trigger.name), (res) =>
                {
                    int pos=parent.model.prm.trigger.evt.IndexOf(model.prm.nm);
                    parent.model.prm.trigger.evt[pos] = res.content;
                    parent.model.prm.onChange?.Invoke();
                    parent.Refresh();
                });
            });
        }
        public override void OnShow()
        {
            model.prm = param;
            Refresh();
        }
        public void Refresh()
        {
            view.sta_.ChangeState(string.IsNullOrEmpty(model.prm.nm) ? 0 : 1);
            view.txt_.text = model.prm.nm;
        }
    }
}
