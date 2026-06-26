using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Code.Form;
using Z_DataSystem;
using Z_Text;
using Z_Ui.Notify;

namespace Z_Code
{
    public class VideoCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new VideoCmd());
        }
        public override void GetUnitChooseCode(Action<string> act, SyntaxNode cur)
        {
            ModManager.instance.assetCtrl.ImportVideo((form) =>
            {
                act.Invoke("\""+AssetManager.instance.videoCtrl.GetName(form.id)+ "\"");
            });
        }
        public override string GetName() => "Video";
        public override CmdBase GetNew() => new VideoCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            asyncTask.error = "Video 功能未实现";
            return true;
        }
    }
}
