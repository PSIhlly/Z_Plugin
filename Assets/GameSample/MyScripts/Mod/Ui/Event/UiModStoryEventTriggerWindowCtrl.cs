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
    public class TriggerConfig
    {
        public string prmName;
        public Action<Dictionary<string, EventTriggerForm.Data>> prmChangeAct;
        public Func<string,string> GetNameFunc;
    }
    public partial class UiModStoryEventTriggerWindowParam
    {
        public EventTriggerForm.Data trigger;
        public Action onHide;

        public Dictionary<string, EventTriggerForm.Data> dic;
        public List<TriggerConfig> configs;
    }
    public partial class UiModStoryEventTriggerWindowModel
    {
        public UiModStoryEventTriggerWindowParam prm;
        
    }

    public partial class UiModStoryEventTriggerWindowCtrl
    {
        UiContainer<UiEventCtrl> evtCon;
        UiContainer<UiPrmCtrl> prmCon;
        public override void OnCreate()
        {
            evtCon = new UiContainer<UiEventCtrl>(view.go_event);
            prmCon = new UiContainer<UiPrmCtrl>(view.go_prm);
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
                model.prm.dic.Remove(model.prm.trigger.name);
                Close();
            });
        }
        public override void OnShow()
        {
            model.prm = param;
            Refresh();

        }
        public override void OnHide()
        {
            model.prm.onHide?.Invoke();
        }
        public void Refresh()
        {
            bool existParams = model.prm.configs != null && model.prm.configs.Count > 0;
            view.go_prm.SetActive(existParams);

            prmCon.Clear();
            if (existParams)
            {
                foreach (var config in model.prm.configs)
                {
                    evtCon.Add(new UiPrmParam() { config = config });
                }
            }
            prmCon.Refresh();

            view.txt_name.text = AssetManager.GetKeyName(model.prm.trigger.name);
            evtCon.Clear();
            foreach(var nm in model.prm.trigger.evt)
            {
                evtCon.Add(new UiEventParam() { nm = nm });
            }
            evtCon.Add(new UiEventParam() { nm = null });
            evtCon.Refresh();

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
                model.prm.config.prmChangeAct(parent.model.prm.dic);
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
                ModManager.instance.assetCtrl.ChooseEvent(SceneEventType.All, CmdTypeDataForm.defaultData.name, TextManager.instance.GetTxt(parent.model.prm.trigger.name), (res) =>
                {
                    parent.model.prm.trigger.evt.Add(res.content);
                    parent.model.prm.onHide?.Invoke();
                    parent.Refresh();
                });
            });
            view.btn_delete.onClick.AddListener(() =>
            {
                parent.model.prm.trigger.evt.Remove(model.prm.nm);
                parent.model.prm.onHide?.Invoke();
                parent.Refresh();
            });
            view.btn_edit.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ChooseEvent(SceneEventType.All, CmdTypeDataForm.defaultData.name, TextManager.instance.GetTxt(parent.model.prm.trigger.name), (res) =>
                {
                    int pos=parent.model.prm.trigger.evt.IndexOf(model.prm.nm);
                    parent.model.prm.trigger.evt[pos] = res.content;
                    parent.model.prm.onHide?.Invoke();
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
