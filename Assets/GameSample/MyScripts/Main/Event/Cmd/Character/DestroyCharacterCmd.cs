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
            var units = new List<Z_Map.Form.CharacterUnitForm.Data>();
            if (CharacterProductForm.TryGetReferenceUid(reference, out var productUid))
                units = PlayManager.instance.sceneCtrl.GetCharacterUnits(productUid);
            else
            {
                const string mark = "$so$";
                if (reference.Length <= mark.Length * 2
                    || !reference.StartsWith(mark, StringComparison.Ordinal)
                    || !reference.EndsWith(mark, StringComparison.Ordinal)
                    || !int.TryParse(reference.Substring(mark.Length, reference.Length - mark.Length * 2),
                        System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var unitUid)
                    || unitUid <= 0)
                    return true;
                if (UnitForm.DataByUid.TryGetValue(unitUid, out var unit)
                    && unit is Z_Map.Form.CharacterUnitForm.Data characterData)
                    units.Add(characterData);
            }
            foreach (var data in units)
            {
                if (!(data?.unit is CharacterUnit character))
                    continue;
                var product = CharacterProductForm.DataByUid.GetDv(character.productInfo.Item1, null);
                if (!PlayManager.instance.sceneCtrl.RemoveCharacter(data))
                    continue;
                // Uniqueness protects the Product, not its instance in this scene.
                if (product != null && !product.unique)
                    CharacterProductForm.RemoveData(product.uid);
            }
            return true;
        }
    }
}
