using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Code.Form;
using Z_Ui.Dialog;
using Z_Ui.Notify;

namespace Z_Code
{
    public class SetDialogContentCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new SetDialogContentCmd());
        }
        public override string GetName() => "SetDialogContent";
        public override CmdBase GetNew() => new SetDialogContentCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            GameManager.instance.curProgress.dialogCache.mainText = prm[0].str;
            return true;
        }
    }
}
