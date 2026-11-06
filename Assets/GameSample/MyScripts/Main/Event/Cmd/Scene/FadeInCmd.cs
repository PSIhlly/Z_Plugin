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
            UiManager.instance.ShowUi<UiPlayEffectCtrl>(new UiPlayEffectParam { fadeOut = false });
            return true;
        }
    }
}
