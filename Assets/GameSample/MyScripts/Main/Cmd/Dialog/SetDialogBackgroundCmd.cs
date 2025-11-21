using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Code.Form;
using Z_Ui.Dialog;
using Z_Ui.Notify;

namespace Z_Code
{
    public class SetDialogBackgroundCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new SetDialogBackgroundCmd());
        }
        public override string GetName() => "SetDialogBackground";
        public override CmdBase GetNew() => new SetDialogBackgroundCmd();
        protected override BoxDataForm.Data[] ExecuteInternal(BoxDataForm.Data[] prm, InterpretLock localLock)
        {
            PlayManager.instance.data.progress.dialogCache.mainPictureName = GlobalEventHelper.GetEventAssetTexName(prm[0].str);
            return null;
        }
    }
}
