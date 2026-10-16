using System;
using UnityEngine;
using Z_Code.Form;
using Z_Map;
using Z_UnitSystem.Form;

namespace Z_Code
{
    internal static class SceneUnitCommand
    {
        public static MapUnit Resolve(string reference)
        {
            if (string.IsNullOrEmpty(reference))
                return null;
            UnitForm.Data data = null;
            if (TryGetId(reference, GlobalEventHelper.SCENEOBJECT, out var uid))
                UnitForm.DataByUid.TryGetValue(uid, out data);
            else if (TryGetId(reference, GlobalEventHelper.CHARACTER, out var productUid))
                data = PlayManager.instance.sceneCtrl?.GetCharacterUnit(productUid);
            return data?.unit is MapUnit unit && IsLive(unit) ? unit : null;
        }

        private static bool TryGetId(string reference, string mark, out int uid)
        {
            uid = 0;
            return reference.Length > mark.Length * 2
                && reference.StartsWith(mark, StringComparison.Ordinal)
                && reference.EndsWith(mark, StringComparison.Ordinal)
                && int.TryParse(reference.Substring(mark.Length, reference.Length - mark.Length * 2), out uid)
                && uid > 0;
        }

        public static bool IsLive(MapUnit unit)
        {
            return unit != null && UnitForm.DataByUid.TryGetValue(unit.data.uid, out var registered)
                && ReferenceEquals(registered, unit.data) && ReferenceEquals(registered.unit, unit);
        }

        public static bool CanMove(MapUnit unit) => unit is CharacterUnit || unit is ObjectUnit;

        public static Vector3 ReadVector(BoxDataForm.Data value)
        {
            if (value?.dic == null || !value.dic.TryGetValue("x", out var x)
                || !value.dic.TryGetValue("height", out var height) || !value.dic.TryGetValue("y", out var y)
                || x == null || height == null || y == null)
                throw new InvalidOperationException("位置需要 x、y、height 三项");
            return new Vector3(x.num, height.num, y.num);
        }

        public static BoxDataForm.Data BoxVector(Vector3 vector)
        {
            var box = CodeHelper.CreateBox();
            box.dic["x"] = CodeHelper.CreateBoxByNum(vector.x);
            box.dic["height"] = CodeHelper.CreateBoxByNum(vector.y);
            box.dic["y"] = CodeHelper.CreateBoxByNum(vector.z);
            return box;
        }

        public static Vector3 WorldPosition(Vector3 position) =>
            MapManager.instance.utilCtrl.MapPos2RealPos(GameManager.PlayerPosToMapPos(position));

        public static void Move(MapUnit unit, Vector3 step)
        {
            // Unit.Move consumes a displacement; never multiply by Character Speed.
            if (unit is CharacterUnit character) character.Move(step);
            else if (unit is ObjectUnit obj) obj.Move(step);
        }
    }
}
