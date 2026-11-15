using System.Collections.Generic;
using Form;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Z_Code.Form;
using Z_Map;
using Z_Map.Form;

namespace Z_Code
{
    public class GenerateCharacterCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Init() => Register(new GenerateCharacterCmd());

        public override string GetName() => "GenerateCharacter";
        public override CmdBase GetNew() => new GenerateCharacterCmd();

        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            asyncTask.res = new[] { CodeHelper.CreateBoxByNum(0) };
            if (!CharacterProductForm.TryGetReferenceUid(prm[0].str, out int uid)
                || !CharacterProductForm.DataByUid.TryGetValue(uid, out var source) || source == null
                || !TryPosition(prm[1], out var position)) return true;

            var map = MapManager.instance;
            position = GameManager.PlayerPosToMapPos(position);
            if (map.data == null || map.utilCtrl == null || map.updateCtrl == null
                || map.utilCtrl.GetPlacementTile(position) == null) return true;
            if (source.unique)
                foreach (var unit in CharacterUnitForm.DataByUid.Values)
                    if (unit?.unit != null && unit.unit.productInfo.Item1 == source.uid)
                        return true;

            CharacterProductForm.Data product = source;
            var createdSkills = new List<int>();
            bool committed = false;
            try
            {
                if (!source.unique)
                {
                    product = source.Copy(false);
                    product.skill = new Dictionary<SkillType, int>();
                    product.ToProduct(source.protoUid > 0 ? source.protoUid : source.uid);
                    if (source.skill != null)
                        foreach (var pair in source.skill)
                        {
                            if (!SkillProductForm.DataByUid.TryGetValue(pair.Value, out var skill) || skill == null)
                                continue;
                            var copy = skill.Copy(false);
                            copy.ToProduct(skill.protoUid > 0 ? skill.protoUid : skill.uid);
                            createdSkills.Add(copy.uid);
                            copy.characterUid = product.uid;
                            product.skill[pair.Key] = copy.uid;
                        }
                }
                var extra = MapUnit.GetProductInfoString(new JObject(), (product.uid, -1));
                var placed = map.AddCharacter(product.name, position, GlobalDefaultHelper.GetRuntimePrefabName("character"),
                    false, extra, GameMapData.GetCharacterProductSize(product));
                if (placed == null) return true;
                placed.minimapIcon = product.minimapIcon;
                asyncTask.res = new[] { CodeHelper.CreateBoxByStr(GlobalEventHelper.GetName(GlobalEventHelper.CHARACTER, product.uid.ToString())) };
                committed = true;
                return true;
            }
            finally
            {
                if (!committed && !ReferenceEquals(product, source))
                {
                    foreach (int skill in createdSkills) SkillProductForm.RemoveData(skill);
                    if (CharacterProductForm.DataByUid.TryGetValue(product.uid, out var registered)
                        && ReferenceEquals(registered, product)) CharacterProductForm.RemoveData(product.uid);
                }
            }
        }

        private static bool TryPosition(BoxDataForm.Data value, out Vector3 position)
        {
            position = default;
            if (value?.dic == null || !value.dic.TryGetValue("x", out var x)
                || !value.dic.TryGetValue("y", out var y) || !value.dic.TryGetValue("height", out var height)
                || !IsNumber(x) || !IsNumber(y) || !IsNumber(height)) return false;
            position = new Vector3(x.num, height.num, y.num);
            return true;
        }

        private static bool IsNumber(BoxDataForm.Data value) => value != null && value.str == null
            && !float.IsNaN(value.num) && !float.IsInfinity(value.num);
    }
}
