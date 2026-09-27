using System;
using System.Globalization;
using UnityEngine;
using Z_Code.Form;
using Z_Text;

namespace Z_Code
{
    public class MissionCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new MissionCmd());
        }

        public override string GetName() => "Mission";
        public override CmdBase GetNew() => new MissionCmd();

        public override void GetUnitChooseCode(Action<string> act, SyntaxNode cur)
        {
            ModManager.instance.assetCtrl.ChooseMission(TextManager.instance.GetTxt("mission"), data =>
                act?.Invoke($"\"{GlobalEventHelper.GetName(GlobalEventHelper.MISSION, data.id.ToString(CultureInfo.InvariantCulture))}\""));
        }

        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            asyncTask.res = new[] { CodeHelper.CreateBoxByStr(GlobalEventHelper.GetName(GlobalEventHelper.MISSION)) };
            return true;
        }
    }
}
