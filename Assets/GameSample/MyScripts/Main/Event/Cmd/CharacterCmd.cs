using System;
using System.Globalization;
using UnityEngine;
using Z_Code.Form;
using Z_Text;

namespace Z_Code
{
    public class CharacterCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new CharacterCmd());
        }

        public override void GetUnitChooseCode(Action<string> act, SyntaxNode cur)
        {
            ModManager.instance.assetCtrl.ChooseCharacter(TextManager.instance.GetTxt("Choose character"), data =>
            {
                if (data != null)
                    act?.Invoke($"\"{GlobalEventHelper.GetName(GlobalEventHelper.CHARACTER, data.uid.ToString(CultureInfo.InvariantCulture))}\"");
            });
        }

        public override string GetName() => "Character";
        public override CmdBase GetNew() => new CharacterCmd();

        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            asyncTask.error = "Character 常量不能直接执行";
            return true;
        }
    }
}
