using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Audio;
using Z_Code.Form;
using Z_DataSystem;
using Z_DataSystem.Form;
using Z_Ui.Dialog;
using Z_Ui.Notify;

namespace Z_Code
{
    public class ShowAdvancedDialogCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new ShowAdvancedDialogCmd());
        }
        public override string GetName() => "ShowAdvancedDialog";
        public override CmdBase GetNew() => new ShowAdvancedDialogCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            if (VideoAssetForm.DataByName.ContainsKey(prm[5].str))
                AudioManager.instance.BgmPause();
            if (prm[1].str =="$i$$i$")
                prm[1].str = null;
            if (prm[0].str == "$i$$i$")
                prm[0].str = null;

            DialogManager.instance.Begin(prm[2].str, prm[3].str, prm[0].str, prm[5].str, prm[1].str, prm[4].str, () =>
            {
                AudioManager.instance.BgmContinue();
                asyncTask.Complete();
            }, prm[6].num == 1);
            return false;
        }
    }
}
