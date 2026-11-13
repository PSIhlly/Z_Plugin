using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ui.ModStoryEventTriggerWindow;
using UnityEngine;
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
                if (model.prm?.dic == null)
                    return;
                if (!model.prm.dic.ContainsKey(model.prm.key))
                    model.prm.dic[model.prm.key] = GameEventController.CreateTrigger(model.prm.key);

                UiManager.instance.ShowUi<UiModStoryEventTriggerWindowCtrl>(new UiModStoryEventTriggerWindowParam() { trigger = model.prm.dic[model.prm.key], dic= model.prm.dic });
            });
            view.btn_onEventTrigger.onClick.AddListener(() =>
            {
                var binding = model.prm;
                if (binding?.dic == null)
                    return;
                if (!binding.dic.ContainsKey(binding.key))
                    binding.dic[binding.key] = GameEventController.CreateTrigger(binding.key);
                var trigger = binding.dic[binding.key];

                ModManager.instance.assetCtrl.ChooseEventTriggerType((item) =>
                {
                    if (!ReferenceEquals(model.prm, binding)
                        || !binding.dic.TryGetValue(binding.key, out var current)
                        || !ReferenceEquals(current, trigger))
                        return;
                    trigger.type = item;
                    Refresh();
                });
            });

        }
        public override void OnShow()
        {
            // Embedded Model controls may be shown before their owner calls Set.
            if (param != null)
                Set(param);
        }
        public void Set(UiEventChooseParam prm)
        {
            param = prm;
            model.prm = prm;
            Refresh();
        }
        public void Refresh()
        {
            if(model.prm?.dic==null)
            {
                return;
            }
            view.txt_name.oriText = model.prm.key;
            var evt = model.prm.dic.GetDv(model.prm.key, EventTriggerForm.defaultData).evt;
            view.txt_onEvent.text = $"{EventProgramDataForm.DataByUid.GetDv(evt.GetDv(0,0), EventProgramDataForm.defaultData).name}({evt.Count})";

            view.txt_onEventTrigger.oriText = model.prm.dic.GetDv(model.prm.key, EventTriggerForm.defaultData).type.ToString();

            UiManager.Rebuild(gameObject, true);
        }

        public void OnEvent(EventModifyEvent evt)
        {
            if (active)
                Refresh();
        }
    }

    // Shared by the Character, Item, Object and MapTexture configuration pages.
    public sealed class UiSceneEventChooseList
    {
        private static readonly string[] keys =
        {
            "onCharacterTouchEvent",
            "onCharacterLeaveEvent",
            "onObjectTouchEvent",
            "onObjectLeaveEvent",
            "onTileTouchEvent",
            "onShowEvent",
            "onPerSecondEvent",
            "onBoundaryTouchEvent",
            "onInteractEvent",
            "onClickMinimapEvent",
            "onLeaveSceneEvent",
            "onDestroyEvent"
        };

        public static IReadOnlyList<string> TriggerKeys { get; } = Array.AsReadOnly(keys);

        private readonly UiContainer<UiEventChooseCtrl> rows;

        public UiSceneEventChooseList(UiCtrl owner, GameObject template)
        {
            rows = new UiContainer<UiEventChooseCtrl>(owner, template);
        }

        public void Render(Dictionary<string, EventTriggerForm.Data> events, bool enableMinimap)
        {
            Clear();
            if (events == null)
                return;
            foreach (var key in keys)
            {
                if (key == "onClickMinimapEvent" && !enableMinimap)
                    continue;
                rows.Add(new UiEventChooseParam { dic = events, key = key });
            }
            rows.Refresh();
        }

        public void Clear()
        {
            // Release the previous Product's dictionary, including on unused pooled rows.
            foreach (var parameter in rows.paramLst)
                ((UiEventChooseCtrl)rows.Get(parameter).ctrl).Set(null);
            rows.Clear();
        }
    }

}
