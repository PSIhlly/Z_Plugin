using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Code.Form;
using Z_Ui.Dialog;
using Z_Ui.Notify;

namespace Z_Code
{
    public class SetImagePosCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new SetImagePosCmd());
        }
        public override string GetName() => "SetImagePos";
        public override CmdBase GetNew() => new SetImagePosCmd();
        protected override BoxDataForm.Data[] ExecuteInternal(BoxDataForm.Data[] prm, InterpretLock localLock)
        {
            PlayManager.instance.assetCtrl.SetPos((int)prm[0].num,new Vector2(prm[1].num, prm[2].num), prm[3].num);
            return null;
        }
    }
}
