using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Code.Form;
using Z_Text;
using Z_Ui.Notify;

namespace Z_Code
{
    public class TextCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new TextCmd());
        }
        public override void GetUnitChooseCode(Action<string> act, SyntaxNode cur)
        {
            NotifyManager.instance.AddInputArea(TextManager.instance.GetTxt("input value"), true, (res) =>
            {
                act?.Invoke($"\"{res}\"");
                return true;
            }, cur.desc.code);
        }
        public override string GetName() => "Text";
        public override CmdBase GetNew() => new TextCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            asyncTask.error = "Text 功能未实现";
            return true;
        }
    }
}
