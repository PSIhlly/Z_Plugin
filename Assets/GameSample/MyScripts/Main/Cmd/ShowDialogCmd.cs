using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Code.Form;
using Z_Ui.Dialog;
using Z_Ui.Notify;

namespace Z_Code
{
    public class ShowDialogCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new ShowDialogCmd());
        }
        public override string GetName() => "ShowDialog";
        public override CmdBase GetNew() => new ShowDialogCmd();
        protected override BoxDataForm.Data[] ExecuteInternal(BoxDataForm.Data[] prm, InterpretLock localLock)
        {
            DialogManager.instance.Begin(new List<string>() { prm[2].str }, new List<string>() { prm[3].str }, new List<string>() { GlobalEventHelper.GetEventAssetTexName(prm[0].str) }, new List<string>() { GlobalEventHelper.GetEventAssetTexName(prm[1].str) }, () =>
            {
                localLock.Unlock();
            });
            localLock.Lock();
            return null;
        }
    }
}
