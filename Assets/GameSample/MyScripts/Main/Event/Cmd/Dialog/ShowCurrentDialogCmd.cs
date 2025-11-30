using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Code.Form;
using Z_Ui.Dialog;
using Z_Ui.Notify;

namespace Z_Code
{
    public class ShowCurrentDialogCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new ShowCurrentDialogCmd());
        }
        public override string GetName() => "ShowCurrentDialog";
        public override CmdBase GetNew() => new ShowCurrentDialogCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            var cache = GameManager.instance.curProgress.dialogCache;
            DialogManager.instance.Begin(cache,() =>
            {
                asyncTask.Complete();
            }, prm[0].num > 0);
            return false;
        }
    }
}
