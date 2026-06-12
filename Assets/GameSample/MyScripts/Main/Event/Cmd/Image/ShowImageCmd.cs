using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Code.Form;
using Z_DataSystem;
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
            var handle = PlayManager.instance.assetCtrl.Add(AssetManager.instance.texCtrl.GetId(prm[0].str), prm[1].num);
            PlayManager.instance.assetCtrl.SetPos(handle, Vector2.one * 0.5f,0);
            
            PlayManager.instance.assetCtrl.SetRemoveTime(handle, prm[2].num);
            asyncTask.res = new BoxDataForm.Data[] { CodeHelper.CreateBoxByStr(GlobalEventHelper.GetName(GlobalEventHelper.UIIMAGE,handle.ToString())) };
            return true;
        }
    }
}
