using System;
using System.Globalization;
using Form;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Z_Code.Form;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Map;
using Z_Map.Form;
using Z_UnitSystem;

namespace Z_Code
{
    public class GenerateItemCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Init() => Register(new GenerateItemCmd());

        public override string GetName() => "GenerateItem";
        public override CmdBase GetNew() => new GenerateItemCmd();

        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            asyncTask.res = new[] { CodeHelper.CreateBoxByNum(0) };
            if (!TryGetItem(prm[0].str, out var source) || source.model == null
                || !TryReadPosition(prm[1], out var position))
                return true;

            var map = MapManager.instance;
            var game = GameManager.instance;
            var pools = InstancePoolManager.instance;
            position = GameManager.PlayerPosToMapPos(position);
            if (map.data == null || map.utilCtrl == null || map.updateCtrl == null
                || game.utilCtrl == null || pools.pools == null || pools.defaultRoot == null
                || map.utilCtrl.GetPlacementTile(position) == null || !HasModelPrefabs(source))
                return true;

            // The pickup path consumes this Product. Never bind a source template
            // or a caller's backpack Product, nor share its nested model/parameters.
            var product = ItemProductForm.GetDataByJo(ItemProductForm.GetJoByData(source));
            product.uid = -1;
            product.ToProduct(source.protoUid > 0 ? source.protoUid : source.uid);
            if (!ItemProductForm.DataByUid.TryGetValue(product.uid, out var registered)
                || !ReferenceEquals(product, registered))
                return true;

            GameObject prefab = null;
            InstancePool pool = null;
            ItemUnitForm.Data placed = null;
            bool committed = false;
            try
            {
                string prefabName = GlobalDefaultHelper.GetRuntimeMapItemPrefabName(product.uid);
                // Product IDs can be reused after pickup. Retire only an unused
                // scene-owned pool, never a pool still referenced by a map item.
                if (pools.pools.TryGetValue(prefabName, out var previous))
                {
                    foreach (var item in ItemUnitForm.DataByUid.Values)
                        if (item.prefabName == prefabName)
                            return true;
                    if (previous.prefab == null || !previous.prefab.transform.IsChildOf(pools.defaultRoot))
                        return true;
                    pools.Unregister(previous);
                    previous.Destroy();
                    UnityEngine.Object.Destroy(previous.prefab);
                }

                prefab = game.utilCtrl.CombineNewObjectByPrefabs(prefabName, product.model, true, true);
                prefab.transform.SetParent(pools.defaultRoot, false);
                pool = new InstancePool(prefab, pools.defaultRoot);
                pools.Register(pool);
                placed = map.AddItem(product.name, position, prefabName);
                if (placed == null)
                    return true;

                placed.unit.productInfo = (product.uid, -1);
                placed.extra = MapUnit.GetProductInfoString(new JObject(), placed.unit.productInfo);
                placed.minimapIcon = product.minimapIcon;
                // AddItem may already have shown the instance before binding.
                // This refreshes its appearance, without repeating Create/onShow.
                if (placed.unit.ins != null)
                    Z_EventHelper.Invoke(new ItemEvent { type = MapEventType.Show, unit = placed.unit });

                asyncTask.res = new[] { CodeHelper.CreateBoxByStr(GlobalEventHelper.GetSceneObjectReference(placed.unit)) };
                committed = true;
                return true;
            }
            finally
            {
                if (!committed)
                {
                    if (placed != null && ItemUnitForm.DataByUid.TryGetValue(placed.uid, out var currentItem)
                        && ReferenceEquals(currentItem, placed))
                        map.RemoveItem(placed);
                    if (pool != null && pools.pools.TryGetValue(pool.prefab.name, out var currentPool)
                        && ReferenceEquals(currentPool, pool))
                    {
                        pools.Unregister(pool);
                        pool.Destroy();
                    }
                    if (prefab != null)
                        UnityEngine.Object.Destroy(prefab);
                    if (ItemProductForm.DataByUid.TryGetValue(product.uid, out var currentProduct)
                        && ReferenceEquals(currentProduct, product))
                        ItemProductForm.RemoveData(product.uid);
                }
            }
        }

        private static bool TryGetItem(string reference, out ItemProductForm.Data item)
        {
            item = null;
            string mark = GlobalEventHelper.ITEM;
            return !string.IsNullOrEmpty(reference) && reference.Length > mark.Length * 2
                && reference.StartsWith(mark, StringComparison.Ordinal)
                && reference.EndsWith(mark, StringComparison.Ordinal)
                && int.TryParse(reference.Substring(mark.Length, reference.Length - mark.Length * 2),
                    NumberStyles.None, CultureInfo.InvariantCulture, out int uid) && uid > 0
                && ItemProductForm.DataByUid.TryGetValue(uid, out item) && item != null;
        }

        private static bool TryReadPosition(BoxDataForm.Data value, out Vector3 position)
        {
            position = default;
            if (value?.dic == null || !value.dic.TryGetValue("x", out var x)
                || !value.dic.TryGetValue("y", out var y) || !value.dic.TryGetValue("height", out var height)
                || !IsCoordinate(x) || !IsCoordinate(y) || !IsCoordinate(height))
                return false;
            position = new Vector3(x.num, height.num, y.num);
            return true;
        }

        private static bool IsCoordinate(BoxDataForm.Data value) => value != null && value.str == null
            && !float.IsNaN(value.num) && !float.IsInfinity(value.num);

        private static bool HasModelPrefabs(ItemProductForm.Data source)
        {
            if (source.model.subPrefabUnitName == null || source.model.subPrefabUnitName.Count == 0)
                return false;
            foreach (int id in source.model.subPrefabUnitName)
                if (!GameObjectAssetForm.DataById.TryGetValue(id, out var asset) || asset == null || asset.GetGo() == null
                    || asset.GetGo().GetComponentInChildren<Renderer>() == null)
                    return false;
            return true;
        }
    }
}
