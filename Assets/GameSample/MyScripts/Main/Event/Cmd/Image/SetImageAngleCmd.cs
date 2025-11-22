using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Code.Form;
using Z_Ui.Dialog;
using Z_Ui.Notify;

namespace Z_Code
{
    public class SetImageAngleCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new SetImageAngleCmd());
        }
        public override string GetName() => "SetImageAngle";
        public override CmdBase GetNew() => new SetImageAngleCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            PlayManager.instance.assetCtrl.SetEuler((int)prm[0].num, prm[1].num, prm[2].num);
            return true;
        }
    }
}
