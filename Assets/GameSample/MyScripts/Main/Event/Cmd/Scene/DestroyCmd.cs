using System;
using System.Collections.Generic;
using System.Globalization;
using Form;
using UnityEngine;
using Z_Code.Form;
using Z_Map;
using Z_Map.Form;
using Z_UnitSystem.Form;

namespace Z_Code
{
    public class DestroyCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new DestroyCmd());
            RegisterAlias("DestroyCharacter", new DestroyCmd());
            RegisterAlias("DestroyObject", new DestroyCmd());
        }

        public override string GetName() => "Destroy";
        public override CmdBase GetNew() => new DestroyCmd();

        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            if (prm.Length == 0 || string.IsNullOrEmpty(prm[0].str))
                return true;

            // Capture all targets before any removal callback can change the
            // registries, Product references, or another target's identity.
            var targets = Snapshot(prm[0].str);
            var removedCharacterProducts = new List<CharacterProductForm.Data>();
            foreach (var target in targets)
            {
                if (!SceneUnitCommand.IsLive(target.unit, target.data))
                    continue;

                if (target.unit is CharacterUnit character)
                {
                    if (PlayManager.instance.sceneCtrl.RemoveCharacter(character.data)
                        && target.characterProduct != null && !target.preserveCharacterProduct)
                        removedCharacterProducts.Add(target.characterProduct);
                }
                else if (target.unit is ObjectUnit obj)
                    MapManager.instance.RemoveObject(obj.data);
                else if (target.unit is ItemUnit item)
                    MapManager.instance.RemoveItem(item.data);
            }

            // Keep a shared Product available until all selected scene Units
            // have removed their Play lookup entries. Never delete a replacement
            // Product that a removal callback registered with a recycled UID.
            foreach (var product in removedCharacterProducts)
                if (CharacterProductForm.DataByUid.TryGetValue(product.uid, out var registered)
                    && ReferenceEquals(registered, product) && !product.unique)
                    CharacterProductForm.RemoveData(product.uid);
            return true;
        }

        private sealed class Target
        {
            public MapUnit unit;
            public UnitForm.Data data;
            public CharacterProductForm.Data characterProduct;
            public bool preserveCharacterProduct;
        }

        private static List<Target> Snapshot(string reference)
        {
            List<MapUnit> units;
            if (TryGetItemProductUid(reference, out int itemUid))
            {
                units = new List<MapUnit>();
                // An Item handle selects its current scene representations,
                // not its backpack stack, template or stored Product.
                if (ItemProductForm.DataByUid.ContainsKey(itemUid))
                    foreach (var data in ItemUnitForm.DataByUid.Values)
                        if (data?.unit is ItemUnit item && SceneUnitCommand.IsLive(item, data)
                            && item.productInfo.Item1 == itemUid)
                            units.Add(item);
            }
            else
                units = SceneUnitCommand.ResolveAll(reference);

            var result = new List<Target>();
            foreach (var unit in units)
            {
                if (!(unit is CharacterUnit || unit is ObjectUnit || unit is ItemUnit))
                    continue;
                var target = new Target { unit = unit, data = unit.data };
                if (unit is CharacterUnit character)
                {
                    CharacterProductForm.DataByUid.TryGetValue(character.productInfo.Item1, out target.characterProduct);
                    target.preserveCharacterProduct = target.characterProduct?.unique == true;
                }
                result.Add(target);
            }
            return result;
        }

        private static bool TryGetItemProductUid(string reference, out int uid)
        {
            uid = 0;
            string mark = GlobalEventHelper.ITEM;
            return reference.Length > mark.Length * 2
                && reference.StartsWith(mark, StringComparison.Ordinal)
                && reference.EndsWith(mark, StringComparison.Ordinal)
                && int.TryParse(reference.Substring(mark.Length, reference.Length - mark.Length * 2),
                    NumberStyles.None, CultureInfo.InvariantCulture, out uid) && uid > 0;
        }
    }
}
