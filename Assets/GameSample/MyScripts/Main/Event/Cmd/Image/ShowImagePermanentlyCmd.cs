using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Code.Form;
using Z_Ui.Dialog;
using Z_Ui.Notify;

namespace Z_Code
{
    public class ShowImagePermanentlyCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new ShowImagePermanentlyCmd());
        }
        public override string GetName() => "ShowImagePermanently";
        public override CmdBase GetNew() => new ShowImagePermanentlyCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            var handle = PlayManager.instance.assetCtrl.Add(prm[0].str, new Vector2(prm[1].num, prm[2].num));
            PlayManager.instance.assetCtrl.SetRemoveTime(handle, int.MaxValue);
            asyncTask.res = new BoxDataForm.Data[] { CodeHelper.CreateBoxByStr(GlobalEventHelper.GetName(GlobalEventHelper.UIIMAGE, handle.ToString())) };
            return true;
        }
    }
}
