using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Code.Form;
using Z_Ui.Dialog;
using Z_Ui.Notify;

namespace Z_Code
{
    public class DeleteImageCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new DeleteImageCmd());
        }
        public override string GetName() => "DeleteImage";
        public override CmdBase GetNew() => new DeleteImageCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            PlayManager.instance.assetCtrl.SetRemoveTime((int)prm[0].num, prm[1].num);
            return true;
        }
    }
}
