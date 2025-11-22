using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Code.Form;
using Z_Ui.Dialog;
using Z_Ui.Notify;

namespace Z_Code
{
    public class CloseCurrentDialogCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new CloseCurrentDialogCmd());
        }
        public override string GetName() => "CloseCurrentDialog";
        public override CmdBase GetNew() => new CloseCurrentDialogCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            DialogManager.instance.End();
            return true;
        }
    }
}
