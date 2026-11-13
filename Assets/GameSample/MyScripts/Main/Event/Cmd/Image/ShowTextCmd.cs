using System;
using System.Globalization;
using UnityEngine;
using Z_Code.Form;

namespace Z_Code
{
    public class ShowTextCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Init() => Register(new ShowTextCmd());

        public override string GetName() => "ShowText";
        public override CmdBase GetNew() => new ShowTextCmd();

        public override void GetUnitChooseCode(Action<string> act, SyntaxNode cur)
        {
            var data = global::Form.GameCmdDataForm.DataByName[GetName()];
            string code = data.defaultCode.Trim();
            if (!string.IsNullOrEmpty(data.allowAsVoid) && code.StartsWith(data.allowAsVoid, StringComparison.Ordinal))
                code = code.Substring(data.allowAsVoid.Length);
            act?.Invoke(code.TrimEnd().TrimEnd(';').TrimEnd());
        }

        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            var assets = PlayManager.instance.assetCtrl;
            int handle = assets.AddText(prm[0].str ?? string.Empty, prm[1].num);
            assets.SetPos(handle, Vector2.one * 0.5f, 0);
            assets.SetRemoveTime(handle, prm[2].num);
            asyncTask.res = new[] { CodeHelper.CreateBoxByStr(GlobalEventHelper.GetName(
                GlobalEventHelper.UIIMAGE, handle.ToString(CultureInfo.InvariantCulture))) };
            return true;
        }
    }
}
