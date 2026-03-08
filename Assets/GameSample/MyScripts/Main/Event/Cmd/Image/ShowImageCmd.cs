using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Code.Form;
using Z_Ui.Dialog;
using Z_Ui.Notify;

namespace Z_Code
{
    public class ShowImageCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new ShowImageCmd());
        }
        public override string GetName() => "ShowImage";
        public override CmdBase GetNew() => new ShowImageCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            var handle = PlayManager.instance.assetCtrl.Add(prm[0].str, new Vector2(prm[1].num, prm[2].num));
            PlayManager.instance.assetCtrl.SetRemoveTime(handle, prm[3].num);
            asyncTask.res = new BoxDataForm.Data[] { CodeHelper.CreateBoxByStr(GlobalEventHelper.GetName(GlobalEventHelper.UIIMAGE,handle.ToString())) };
            return true;
        }
    }
}
