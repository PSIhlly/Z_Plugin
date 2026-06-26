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
    public class AudioCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new AudioCmd());
        }
        public override void GetUnitChooseCode(Action<string> act, SyntaxNode cur)
        {
            ModManager.instance.assetCtrl.ImportAudio((form) =>
            {
                act.Invoke("\""+AssetManager.instance.audioCtrl.GetName(form.id)+ "\"");
            });
        }
        public override string GetName() => "Audio";
        public override CmdBase GetNew() => new AudioCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            asyncTask.error = "Audio 功能未实现";
            return true;
        }
    }
}
