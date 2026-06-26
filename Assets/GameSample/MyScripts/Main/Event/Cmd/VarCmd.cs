using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Code.Form;
using Z_Text;
using Z_Ui.Notify;

namespace Z_Code
{
    public class LocalVarCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new LocalVarCmd());
        }
        public override void GetUnitChooseCode(Action<string> act, SyntaxNode cur)
        {
            NotifyManager.instance.AddInputArea(TextManager.instance.GetTxt("input local variable name"), true, (res) =>
            {
                act?.Invoke($"{res}");
                return true;
            }, cur.desc.code);
        }
        public override string GetName() => "LocalVar";
        public override CmdBase GetNew() => new LocalVarCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            asyncTask.error = "LocalVar 功能未实现";
            return true;
        }
    }
}
