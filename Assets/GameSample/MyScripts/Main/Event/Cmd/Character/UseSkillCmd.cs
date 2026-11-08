using Form;
using System.Globalization;
using UnityEngine;
using Z_Code.Form;

namespace Z_Code
{
    public class UseSkillCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new UseSkillCmd());
        }
        public override string GetName() => "UseSkill";
        public override CmdBase GetNew() => new UseSkillCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            CharacterSkillCommand.Use(prm[0].str, prm[1].str, null, ignoreCd: true);
            return true;
        }
    }

    public abstract class UseCharacterSlotSkillCmd : CmdBase
    {
        protected abstract SkillType Slot { get; }

        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            CharacterSkillCommand.Use(prm[0].str, null, Slot, prm[1].num == 1f);
            return true;
        }
    }

    public sealed class UseCharacterLightAttackCmd : UseCharacterSlotSkillCmd
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Init() => Register(new UseCharacterLightAttackCmd());
        public override string GetName() => "UseCharacterLightAttack";
        public override CmdBase GetNew() => new UseCharacterLightAttackCmd();
        protected override SkillType Slot => SkillType.LightAttack;
    }

    public sealed class UseCharacterHeavyAttackCmd : UseCharacterSlotSkillCmd
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Init() => Register(new UseCharacterHeavyAttackCmd());
        public override string GetName() => "UseCharacterHeavyAttack";
        public override CmdBase GetNew() => new UseCharacterHeavyAttackCmd();
        protected override SkillType Slot => SkillType.HeavyAttack;
    }

    public sealed class UseCharacterESkillCmd : UseCharacterSlotSkillCmd
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Init() => Register(new UseCharacterESkillCmd());
        public override string GetName() => "UseCharacterESkill";
        public override CmdBase GetNew() => new UseCharacterESkillCmd();
        protected override SkillType Slot => SkillType.E;
    }

    public sealed class UseCharacterQSkillCmd : UseCharacterSlotSkillCmd
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Init() => Register(new UseCharacterQSkillCmd());
        public override string GetName() => "UseCharacterQSkill";
        public override CmdBase GetNew() => new UseCharacterQSkillCmd();
        protected override SkillType Slot => SkillType.Q;
    }

    internal static class CharacterSkillCommand
    {
        public static void Use(string character, string reference, SkillType? slot, bool ignoreCd)
        {
            var info = PlayManager.instance.infoCtrl;
            if (info == null || GameManager.instance.curProgress == null)
                return;
            // Skill events dispatch synchronously. A previous target's callback
            // must not redirect a later target to a replacement using its UID.
            foreach (var data in CharacterProductForm.ResolveReferences(character))
            {
                if (!CharacterProductForm.DataByUid.TryGetValue(data.uid, out var current)
                    || !ReferenceEquals(current, data))
                    continue;
                SkillProductForm.Data skill;
                if (slot.HasValue)
                {
                    if (data.skill == null || !data.skill.TryGetValue(slot.Value, out int uid)
                        || !SkillProductForm.DataByUid.TryGetValue(uid, out skill) || skill == null)
                        continue;
                }
                else
                {
                    skill = ResolveSpecified(data, reference);
                    if (skill == null)
                        continue;
                }
                info.UseSkill(data.uid, skill.uid, ignoreCd);
            }
        }

        private static SkillProductForm.Data ResolveSpecified(CharacterProductForm.Data character, string reference)
        {
            const string marker = "$sk$";
            if (string.IsNullOrEmpty(reference) || !GlobalEventHelper.IsAsset(reference, marker)
                || !int.TryParse(reference.Substring(marker.Length, reference.Length - marker.Length * 2),
                    NumberStyles.Integer, CultureInfo.InvariantCulture, out int uid) || uid == 0)
                return null;
            if (character.skill != null)
                return SkillProductForm.GetByEvtName(reference, character.uid);
            return uid > 0 && SkillProductForm.DataByUid.TryGetValue(uid, out var skill) ? skill : null;
        }
    }
}
