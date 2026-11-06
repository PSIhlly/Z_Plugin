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
            int? selectedAssetId = cur?.desc?.type == CodeType.Str
                ? AssetManager.instance.texCtrl.GetId(cur.desc.code) : (int?)null;
            ModManager.instance.assetCtrl.ImportImage((form) =>
            {
                act.Invoke("\""+AssetManager.instance.texCtrl.GetName(form.id)+ "\"");
            }, selectedAssetId);
        }
        public override string GetName() => "Image";
        public override CmdBase GetNew() => new ImageCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            asyncTask.error = "Image 功能未实现";
            return true;
        }
    }
}
