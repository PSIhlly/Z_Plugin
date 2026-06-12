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
    public class ImageCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new ImageCmd());
        }
        public override void GetUnitChooseCode(Action<string> act, SyntaxNode cur)
        {
            ModManager.instance.assetCtrl.ImportImage((form) =>
            {
                act.Invoke("\""+AssetManager.instance.texCtrl.GetName(form.id)+ "\"");
            });
        }
        public override string GetName() => "Image";
        public override CmdBase GetNew() => new ImageCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            return true;
        }
    }
}
