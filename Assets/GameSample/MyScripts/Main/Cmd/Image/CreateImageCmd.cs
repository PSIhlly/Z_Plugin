using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Code.Form;
using Z_Ui.Dialog;
using Z_Ui.Notify;

namespace Z_Code
{
    public class CreateImageCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new CreateImageCmd());
        }
        public override string GetName() => "CreateImage";
        public override CmdBase GetNew() => new CreateImageCmd();
        protected override BoxDataForm.Data[] ExecuteInternal(BoxDataForm.Data[] prm, InterpretLock localLock)
        {
            return new BoxDataForm.Data[] { new BoxDataForm.Data(-1,"","", PlayManager.instance.assetCtrl.Add(GlobalEventHelper.GetEventAssetTexName(prm[0].str), new Vector2(prm[1].num, prm[2].num))) };
        }
    }
}
