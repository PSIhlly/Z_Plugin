using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Code.Form;
using Z_Ui.Dialog;
using Z_Ui.Form;
using Z_Ui.Notify;

namespace Z_Code
{
    public class ResetDialogCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new ResetDialogCmd());
        }
        public override string GetName() => "ResetDialog";
        public override CmdBase GetNew() => new ResetDialogCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            PlayManager.instance.data.progress.dialogCache = ClipForm.defaultData.Copy();
            return true;
        }
    }
}
