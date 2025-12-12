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
using Z_Video;

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
    public partial class UiEventChooseCtrl
    {

        public override void OnCreate()
        {

            view.btn_onEvent.onClick.AddListener(() =>
            {
                if (!model.prm.dic.ContainsKey(model.prm.key))
                    model.prm.dic[model.prm.key] = GameEventController.CreateTrigger(model.prm.key);

                UiManager.instance.ShowUi<UiModStoryEventTriggerWindowCtrl>(new UiModStoryEventTriggerWindowParam() { trigger = model.prm.dic[model.prm.key], onChange = Refresh });
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
            view.txt_name.oriText = model.prm.key;
            var evt = model.prm.dic.GetDv(model.prm.key, EventTriggerForm.defaultData).evt;
            view.txt_onEvent.text = $"{evt.GetDv(0, "")}({evt.Count})";

            view.txt_onEventTrigger.oriText = model.prm.dic.GetDv(model.prm.key, EventTriggerForm.defaultData).type.ToString();
        }
    }

}