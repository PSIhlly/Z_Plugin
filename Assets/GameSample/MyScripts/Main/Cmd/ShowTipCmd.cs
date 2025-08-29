using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Code.Form;
using Z_Ui.Notify;

namespace Z_Code
{
    public class ShowTipCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new ShowTipCmd());
        }
        public override string GetName() => "ShowTip";
        public override CmdBase GetNew() => new ShowTipCmd();
        protected override BoxDataForm.Data[] ExecuteInternal(BoxDataForm.Data[] prm, InterpretLock localLock)
        {
            if (prm[0].str == null)
            {
                NotifyManager.instance.AddTip(prm[0].num.ToString());
            }
            else
            {
                NotifyManager.instance.AddTip(prm[0].str);
            }
            return null;
        }
    }
}
