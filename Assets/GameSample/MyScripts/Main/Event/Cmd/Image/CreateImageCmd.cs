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
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            asyncTask.res = new BoxDataForm.Data[] { CodeHelper.CreateBoxByNum(PlayManager.instance.assetCtrl.Add(prm[0].str, new Vector2(prm[1].num, prm[2].num))) };
            return true;
        }
    }
}
