using UnityEngine;
using Form;

namespace Z_Map
{
    public partial class ObjectUnit
    {
        public override bool TryGetFirstPartTransform(out Vector3 position, out Vector3 scale)
        {
            var product = data.unit.product;
            if (product == null || product.model?.subPrefabUnitName == null
                || product.model.subPrefabUnitName.Count == 0)
                return base.TryGetFirstPartTransform(out position, out scale);
            var direction = product.GetAnimDirection(data.euler.y);
            position = product.GetModelPosition(direction);
            scale = product.GetModelSize(direction);
            return true;
        }
    }
}
