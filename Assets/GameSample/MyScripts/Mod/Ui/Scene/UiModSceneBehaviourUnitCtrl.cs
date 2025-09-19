using Form;
using System.Collections;
using System.Collections.Generic;
using Ui.ModStoryEventTrigger;
using UnityEngine;
using Z_DataSystem;
using Z_Map;
using Z_Map.Form;
using Z_ObjectAnimator.Base;
using Z_ObjectAnimator.Core;
using Z_Text;
using Z_Texture;
using Z_Ui;
using Z_Ui.Base;
using Z_Ui.Notify;
using Z_UnitSystem.Form;

namespace Ui.ModSceneBehaviourUnit
{
    public partial class UiModSceneBehaviourUnitParam
    {
        public UnitForm.Data data;

    }
    public partial class UiModSceneBehaviourUnitModel
    {
        public UnitForm.Data data;
        
    }

    public partial class UiModSceneBehaviourUnitCtrl
    {

        public override void OnCreate()
        {
            view.btn_bbg.onClick.AddListener(() =>
            {
                Close();
            });
            view.btn_close.onClick.AddListener(() =>
            {
                Close();
            });
            view.btn_evt.onClick.AddListener(() =>
            {
                var unit = (MapUnit)model.data.unit;
                //TODO:
                /*UiManager.instance.ShowUi<UiModStoryEventTriggerCtrl>(new UiModStoryEventTriggerParam()
                {
                    dic= unit.evtDic,
                    act = (dic) =>
                    {
                        unit.evtDic = dic;
                    }
                });*/
                Close();
            });
        }
        public override void OnShow()
        {
            model.data = param.data;
            if(!(model.data.unit is MapUnit))
            {
                Close();
                return;
            }
            Refresh();

        }
        public void Refresh()
        {
            view.txt_name.text = AssetManager.GetKeyName(model.data.name);

        }
       
    }
}
