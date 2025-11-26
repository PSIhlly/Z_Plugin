using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Code.Form;
using Z_Text;
using Z_Ui.Notify;

namespace Z_Code
{
    public class SetLocalVarCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new SetLocalVarCmd());
        }
        public override void GetUnitChooseCode(Action<string> act)
        {
            NotifyManager.instance.AddInputArea(TextManager.instance.GetTxt("input local variable name"), true, (res) =>
            {
                act?.Invoke($"{res} = 0;");
                return true;
            });
        }
        public override string GetName() => "SetLocalVar";
        public override CmdBase GetNew() => new SetLocalVarCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            return true;
        }
    }
}
