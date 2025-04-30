
using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;
using Z_Ui;
using Ui.ModStoryEvent;

namespace Ui.ModStoryEventTrigger
{

    public partial class UiModStoryEventTriggerParam
    {
        public EventType type;
        public Dictionary<string, EventForm.Data> dic;
        public Action<Dictionary<string,EventForm.Data>> act;

    }
    public partial class UiModStoryEventTriggerModel
    {
        public EventType type;
        public Dictionary<string, EventForm.Data> dic;
        public Action<Dictionary<string, EventForm.Data>> act;
    }
    public partial class UiModStoryEventTriggerCtrl
    {

        UiScrViewContainer<UiTriggerCtrl> triggerCon;
        public override void OnCreate()
        {

            view.btn_close.onClick.AddListener(() =>
            {
                Close();
            });
            triggerCon = new UiScrViewContainer<UiTriggerCtrl>(view.go_trigger, view.scr_triggers);

        }
        public override void Close()
        {
            model.act(model.dic);
        }
        public override void OnShow()
        {
            if(param!=null)
            {
                model.act = param.act;
                model.type = param.type;
                model.dic = param.dic;
            }
            Refresh();
        }
        public void Refresh()
        {

            view.txt_title.text = "";
            triggerCon.Clear();
            foreach(var key in model.dic.Keys)
            {
                triggerCon.Add(new UiTriggerParam()
                {
                    name=key
                });
            }
            triggerCon.Refresh();
        }
    }


    public partial class UiTriggerParam
    {
        public string name;
    }
    public partial class UiTriggerModel
    {
        public string name;

    }
    public partial class UiTriggerCtrl
    {
        public override void OnCreate()
        {

            view.btn_.onClick.AddListener(() =>
            {
                UiManager.instance.ShowUi<UiModStoryEventCtrl>(new UiModStoryEventParam()
                {
                    func = (data) =>
                    {
                        parent.model.dic[model.name] = data;
                        parent.Refresh();
                    }
                });
            });

        }
        public override void OnShow()
        {
            model.name = param.name;
            Refresh();
        }
        public void Refresh()
        {
            view.txt_triggerName.text = model.name;
            view.txt_eventName.text = parent.model.dic[model.name].name;
        }
    }


}