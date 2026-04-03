using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Code.Form;
using Z_Text;
using Z_Ui.Notify;

namespace Z_Code
{
    public class EffectCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new EffectCmd());
        }
        public override void GetUnitChooseCode(Action<string> act, SyntaxNode cur)
        {
            ModManager.instance.assetCtrl.ChooseEffect(TextManager.instance.GetTxt("Choose effect"), (form) =>
            {
                act.Invoke($"\"{GlobalEventHelper.GetName(GlobalEventHelper.EFFECT, form.uid.ToString())}\"");
            });
        }
        public override string GetName() => "Effect";
        public override CmdBase GetNew() => new EffectCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            return true;
        }
    }
}
