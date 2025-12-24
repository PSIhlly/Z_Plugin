using Form;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ui.EventChoose;
using Ui.ModStoryEventTriggerWindow;
using Unity.VisualScripting.FullSerializer;
using UnityEngine.Video;
using Z_Code.Form;
using Z_DesignStyle;
using Z_Text;
using Z_Texture;
using Z_Ui;
using Z_Ui.Base;
using Z_Video;

namespace Ui.EventCustomTrigger
{

    public partial class UiEventCustomTriggerParam
    {
        public string defaultKey;
        public Dictionary<string, EventTriggerForm.Data> dic;
        public List<TriggerConfig> configs;
    }
    public partial class UiEventCustomTriggerModel
    {
        public UiEventCustomTriggerParam prm;
    }
    public partial class UiEventCustomTriggerCtrl : IZ_Listener<EventModifyEvent>
    {
        UiContainer<UiTriggerCtrl> con;

        public override void OnCreate()
        {
            this.Register();
            con = new UiContainer<UiTriggerCtrl>(view.go_trigger);
        }
        public override void OnShow()
        {
        }
        public void Set(UiEventCustomTriggerParam prm)
        {
            model.prm = prm;
            Refresh();
        }
        public void Refresh()
        {
            con.Clear();
            foreach (var trigger in model.prm.dic.Values)
            {
                if (trigger.name.StartsWith(model.prm.defaultKey.Split("$")[0] + "$"))
                    con.Add(new UiTriggerParam() { data = trigger });
            }
            con.Add(new UiTriggerParam() { data = null });
            con.Refresh();
        }

        public void OnEvent(EventModifyEvent evt)
        {
            Refresh();
        }
    }


    public partial class UiTriggerParam
    {
        public EventTriggerForm.Data data;
    }
    public partial class UiTriggerModel
    {
        public UiTriggerParam prm;
    }
    public partial class UiTriggerCtrl
    {

        public override void OnCreate()
        {
            view.btn_.onClick.AddListener(() =>
            {
                if (model.prm.data != null)
                {
                    UiManager.instance.ShowUi<UiModStoryEventTriggerWindowCtrl>(new UiModStoryEventTriggerWindowParam()
                    {
                        trigger = model.prm.data,
                        dic = parent.model.prm.dic,
                        configs = parent.model.prm.configs,
                    });
                }
                else
                {
                    parent.model.prm.dic.Add(parent.model.prm.defaultKey, GameEventController.CreateTrigger(parent.model.prm.defaultKey));
                    parent.Refresh();
                }
            });


        }
        public override void OnShow()
        {
            model.prm = param;
            Refresh();
        }

        public void Refresh()
        {

            view.sta_.ChangeState(model.prm.data == null ? 0 : 1);
            if (model.prm.data != null)
            {
                var text = $"{TextManager.instance.GetTxt(model.prm.data.name.Split("$")[0])}";
                foreach (var config in parent.model.prm.configs)
                {
                    text += "-"+config.GetNameFunc(model.prm.data.name);
                }
                view.txt_.text = text;
            }

        }
    }
}