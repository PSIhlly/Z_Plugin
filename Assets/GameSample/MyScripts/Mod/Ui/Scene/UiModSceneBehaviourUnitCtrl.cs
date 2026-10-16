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
            // The new prefab button is not in the generated View yet. Resolve it
            // once here; regenerating UiBase later does not add another listener.
            var deleteButton = System.Array.Find(gameObject.GetComponentsInChildren<Btn>(true),
                button => button.name == "btn_delete");
            if (deleteButton != null)
                deleteButton.onClick.AddListener(DeleteUnit);
            else
                Debug.LogError("ModSceneBehaviourUnit is missing btn_delete.", gameObject);

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
            model.data = param?.data;
            if(!(model.data?.unit is MapUnit))
            {
                Close();
                return;
            }
            Refresh();

        }
        private void DeleteUnit()
        {
            var data = model.data;
            if (!(data?.unit is MapUnit))
            {
                Close();
                return;
            }

            using (ModManager.instance.sceneCtrl.BeginOperation())
            {
                ModManager.instance.sceneCtrl.Track(data);
                if (data is ItemUnitForm.Data itemData)
                    MapManager.instance.RemoveItem(itemData);
                else if (data is ObjectUnitForm.Data objectData)
                    MapManager.instance.RemoveObject(objectData);
                else if (data is CharacterUnitForm.Data characterData)
                    MapManager.instance.RemoveCharacter(characterData);
                else if (data is TileUnitForm.Data tileData)
                    MapManager.instance.RemoveTile(tileData);
            }
            model.data = null;
            Close();
        }
        public void Refresh()
        {
            view.txt_name.text =model.data.name;

        }
       
    }
}
