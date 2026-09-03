using System;
using System.Collections;
using System.Collections.Generic;
using Z_Code.Form;
using Z_Text;
using UnityEngine;
namespace Z_Code
{
    public class ItemCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new ItemCmd());
        }

        public override void GetUnitChooseCode(Action<string> act, SyntaxNode cur)
        {
            ModManager.instance.assetCtrl.ChooseItem(TextManager.instance.GetTxt("Choose item"), (data) =>
            {
                if (data != null)
                    act?.Invoke($"\"{GlobalEventHelper.GetName(GlobalEventHelper.ITEM, data.uid.ToString())}\"");
            });
        }

        public override string GetName() => "Item";
        public override CmdBase GetNew() => new ItemCmd();

        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            asyncTask.error = "Item 常量不能直接执行";
            return true;
        }
    }
}
