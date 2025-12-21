using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Code.Form;
using Z_Ui.Dialog;
using Z_Ui.Notify;

namespace Z_Code
{
    public class SetDialogAudioCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new SetDialogAudioCmd());
        }
        public override string GetName() => "SetDialogAudio";
        public override CmdBase GetNew() => new SetDialogAudioCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            GameManager.instance.curProgress.dialogCache.mainAudioName = prm[0].str;
            return true;
        }
    }
}
