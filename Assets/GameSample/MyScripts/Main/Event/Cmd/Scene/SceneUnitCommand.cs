using System;
using System.Collections.Generic;
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
            var units = ResolveAll(reference);
            return units.Count == 0 ? null : units[0];
        }

        public static List<MapUnit> ResolveAll(string reference)
        {
            var result = new List<MapUnit>();
            if (string.IsNullOrEmpty(reference))
                return result;
            if (TryGetId(reference, GlobalEventHelper.CHARACTER, out var productUid))
            {
                var characters = PlayManager.instance.sceneCtrl?.GetCharacterUnits(productUid);
                if (characters != null)
                    foreach (var data in characters)
                        if (data?.unit is MapUnit unit && IsLive(unit, data))
                            result.Add(unit);
            }
            else
            {
                foreach (var unit in GlobalEventHelper.ResolveSceneObjectUnits(reference))
                    if (IsLive(unit))
                        result.Add(unit);
            }
            return result;
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
            => unit != null && IsLive(unit, unit.data);

        public static bool IsLive(MapUnit unit, UnitForm.Data originalData)
        {
            return unit != null && originalData != null && ReferenceEquals(unit.data, originalData)
                && UnitForm.DataByUid.TryGetValue(originalData.uid, out var registered)
                && ReferenceEquals(registered, originalData) && ReferenceEquals(registered.unit, unit);
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
