using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Code.Form;
using Z_Text;
using Z_Ui.Dialog;
using Z_Ui.Notify;

namespace Z_Code
{
    public class ShowEffectCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new ShowEffectCmd());
        }
        public override string GetName() => "ShowEffect";
        public override void GetUnitChooseCode(Action<string> act)
        {
            ModManager.instance.assetCtrl.ChooseEffectr(TextManager.instance.GetTxt("Choose effect"),(form) => 
            {
                act.Invoke($"{form.uid}");
            });
        }
        public override CmdBase GetNew() => new ShowEffectCmd();
        protected override BoxDataForm.Data[] ExecuteInternal(BoxDataForm.Data[] prm, InterpretLock localLock)
        {
            GameManager.instance.effectCtrl.CreatEffect((int)prm[0].num, new Vector3(prm[1].num, prm[2].num, prm[3].num), prm[4].num);
            return null;
        }
    }
}
