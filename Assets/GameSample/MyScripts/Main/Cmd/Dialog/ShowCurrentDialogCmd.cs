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
        protected override BoxDataForm.Data[] ExecuteInternal(BoxDataForm.Data[] prm, InterpretLock localLock)
        {
            var cache = PlayManager.instance.data.progress.dialogCache;
            DialogManager.instance.Begin(cache,() =>
            {
                localLock.Unlock();
            }, prm[0].num > 0);
            localLock.Lock();
            return null;
        }
    }
}
