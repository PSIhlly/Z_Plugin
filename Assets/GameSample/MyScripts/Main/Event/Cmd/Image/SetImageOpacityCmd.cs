using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Code.Form;
using Z_Ui.Dialog;
using Z_Ui.Notify;

namespace Z_Code
{
    public class SetImageOpacityCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new SetImageOpacityCmd());
        }
        public override string GetName() => "SetImageOpacity";
        public override CmdBase GetNew() => new SetImageOpacityCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            PlayManager.instance.assetCtrl.SetOpacity((int)prm[0].num,prm[1].num, prm[2].num);
            return true;
        }
    }
}
