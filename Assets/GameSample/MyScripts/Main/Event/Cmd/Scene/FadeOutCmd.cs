using Ui.PlayEffect;
using UnityEngine;
using Z_Code.Form;
using Z_Ui;

namespace Z_Code
{
    public class FadeOutCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Init()
        {
            Register(new FadeOutCmd());
        }

        public override string GetName() => "FadeOut";
        public override CmdBase GetNew() => new FadeOutCmd();

        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            UiManager.instance.ShowUi<UiPlayEffectCtrl>(new UiPlayEffectParam { fadeOut = true });
            return true;
        }
    }
}
