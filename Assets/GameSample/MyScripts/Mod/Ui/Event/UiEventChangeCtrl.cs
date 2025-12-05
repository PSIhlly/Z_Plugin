using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Video;
using Z_Code.Form;
using Z_DesignStyle;
using Z_Text;
using Z_Texture;
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
                ModManager.instance.assetCtrl.ChooseEvent(model.prm.key, SceneEventType.All, CmdTypeDataForm.defaultData.name, TextManager.instance.GetTxt(model.prm.key), (res) =>
                {
                    model.prm.dic[model.prm.key] = GameEventController.CreateTrigger(model.prm.key, res.content, default);
                    Refresh();
                });
            });
            view.btn_onEventTrigger.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ChooseEventTriggerType((item) =>
                {
                    model.prm.dic[model.prm.key].type = item;
                    Refresh();
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
            view.txt_onEvent.text = model.prm.dic.GetDv(model.prm.key, EventTriggerForm.defaultData).evt;

            view.txt_onEventTrigger.oriText = model.prm.dic.GetDv(model.prm.key, EventTriggerForm.defaultData).type.ToString();
        }
    }

}