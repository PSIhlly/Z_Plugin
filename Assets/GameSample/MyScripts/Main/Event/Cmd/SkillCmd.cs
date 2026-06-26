using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Code.Form;
using Z_Text;
using Z_Ui.Notify;

namespace Z_Code
{
    public class SkillCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new SkillCmd());
        }
        public override void GetUnitChooseCode(Action<string> act, SyntaxNode cur)
        {
            ModManager.instance.assetCtrl.ChooseSkill(TextManager.instance.GetTxt("Choose skill"), (form) =>
            {
                act.Invoke($"\"{GlobalEventHelper.GetName(GlobalEventHelper.SKILL, form.uid.ToString())}\"");
            });
        }
        public override string GetName() => "Skill";
        public override CmdBase GetNew() => new SkillCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            asyncTask.error = "Skill 功能未实现";
            return true;
        }
    }
}
