using Ui.PlayEffect;
using UnityEngine;
using Z_Code.Form;
using Z_Ui;

namespace Z_Code
{
    public class FadeInCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Init()
        {
            Register(new FadeInCmd());
        }

        public override string GetName() => "FadeIn";
        public override CmdBase GetNew() => new FadeInCmd();

        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            UiManager.instance.ShowUi<UiPlayEffectCtrl>(new UiPlayEffectParam
            {
                fadeOut = false,
                duration = FadeCommandDuration.Read(prm)
            });
            return true;
        }
    }

    internal static class FadeCommandDuration
    {
        internal static float Read(BoxDataForm.Data[] arguments)
        {
            // Old sources/bytecode and omitted Cmd arguments keep the one-second default.
            if (arguments == null || arguments.Length == 0 || arguments[0] == null
                || arguments[0].str == string.Empty)
                return 1f;
            return arguments[0].num;
        }
    }
}
