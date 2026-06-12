using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ui.ModStoryEventTriggerWindow;
using UnityEngine.Video;
using Z_Code.Form;
using Z_DesignStyle;
using Z_Text;
using Z_Texture;
using Z_Ui;
using Z_Ui.Base;
 

namespace Ui.EventChoose
{

    public partial class UiEventChooseParam
    {
        public string key;
        public Dictionary<string, EventTriggerForm.Data> dic;
    }
    public partial class UiEventChooseModel
    {
        public UiEventChooseParam prm;
    }
    public partial class UiEventChooseCtrl:IZ_Listener<EventModifyEvent>
    {

        public override void OnCreate()
        {
            this.Register();
            view.btn_onEvent.onClick.AddListener(() =>
            {
                if (!model.prm.dic.ContainsKey(model.prm.key))
                    model.prm.dic[model.prm.key] = GameEventController.CreateTrigger(model.prm.key);

                UiManager.instance.ShowUi<UiModStoryEventTriggerWindowCtrl>(new UiModStoryEventTriggerWindowParam() { trigger = model.prm.dic[model.prm.key], dic= model.prm.dic });
            });
            view.btn_onEventTrigger.onClick.AddListener(() =>
            {
                if (!model.prm.dic.ContainsKey(model.prm.key))
                    model.prm.dic[model.prm.key] = GameEventController.CreateTrigger(model.prm.key);

                ModManager.instance.assetCtrl.ChooseEventTriggerType((item) =>
                {
                    model.prm.dic[model.prm.key].type = item;
                    Refresh();
                });
            });

        }
        public override void OnShow()
        {
        }
        public void Set(UiEventChooseParam prm)
        {
            model.prm = prm;
            Refresh();
        }
        public void Refresh()
        {
            if(model.prm==null)
            {
                return;
            }
            view.txt_name.oriText = model.prm.key;
            var evt = model.prm.dic.GetDv(model.prm.key, EventTriggerForm.defaultData).evt;
            view.txt_onEvent.text = $"{EventProgramDataForm.DataByUid.GetDv(evt.GetDv(0,0), EventProgramDataForm.defaultData).name}({evt.Count})";

            view.txt_onEventTrigger.oriText = model.prm.dic.GetDv(model.prm.key, EventTriggerForm.defaultData).type.ToString();
        }

        public void OnEvent(EventModifyEvent evt)
        {
            Refresh();
        }
    }

}