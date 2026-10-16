using Form;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Z_Code.Form;
using Z_DataSystem;
using Z_DesignStyle;
using Z_Map;
using Z_Text;
using Z_Ui.Dialog;
using Z_Ui.Notify;
using Z_UnitSystem.Form;

namespace Z_Code
{
    public class DestroyCharacterCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new DestroyCharacterCmd());
        }
        public override string GetName() => "DestroyCharacter";
        public override CmdBase GetNew() => new DestroyCharacterCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            var reference = prm[0].str;
            if (string.IsNullOrEmpty(reference))
                return true;

            // Interaction/BoundaryTouch pass the scene instance; other character
            // events and commands pass the owning Character Product.
            var data = GlobalEventHelper.IsAsset(reference, GlobalEventHelper.SCENEOBJECT)
                ? UnitForm.DataByUid.GetDv(GlobalEventHelper.GetId(reference, GlobalEventHelper.SCENEOBJECT), null)
                    as Z_Map.Form.CharacterUnitForm.Data
                : PlayManager.instance.sceneCtrl.GetCharacterUnit(GlobalEventHelper.GetId(reference, GlobalEventHelper.CHARACTER));
            if (data?.unit is CharacterUnit character)
            {
                var product = CharacterProductForm.DataByUid.GetDv(character.productInfo.Item1, null);
                if (!PlayManager.instance.sceneCtrl.RemoveCharacter(data))
                    return true;
                // Uniqueness protects the Product, not its instance in this scene.
                if (product != null && !product.unique)
                    CharacterProductForm.RemoveData(product.uid);
            }
            return true;
        }
    }
}
