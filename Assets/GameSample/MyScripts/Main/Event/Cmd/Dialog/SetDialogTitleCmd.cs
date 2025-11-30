using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Code.Form;
using Z_Ui.Dialog;
using Z_Ui.Notify;

namespace Z_Code
{
    public class SetDialogTitleCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new SetDialogTitleCmd());
        }
        public override string GetName() => "SetDialogTitle";
        public override CmdBase GetNew() => new SetDialogTitleCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            GameManager.instance.curProgress.dialogCache.title = prm[0].str;
            return true;
        }
    }
}
