using Form;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Z_Code.Form;
using Z_Text;
using Z_Ui.Dialog;
using Z_Ui.Notify;

namespace Z_Code
{
    public class ShowEffectByCharacterCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new ShowEffectByCharacterCmd());
        }
        public override string GetName() => "ShowEffectByCharacter";
        public override CmdBase GetNew() => new ShowEffectByCharacterCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            var data = PlayManager.instance.sceneCtrl.GetCharacterUnit(GlobalEventHelper.GetId(prm[1].str, GlobalEventHelper.CHARACTER));
            bool ignoreRot = prm[2].num == 1;
            var delta = ShowEffectCmd.DeltaHeight;
            if (data != null)
            {
                PlayManager.instance.effectCtrl.CreatEffect(GlobalEventHelper.GetId(prm[0].str, GlobalEventHelper.EFFECT), data.pos + delta, ignoreRot ? 0 : data.euler.y, (img) =>
                {
                    if(data.unit.ins!=null)
                    {
                        img.oriPos = data.unit.ins.transform.position + delta;
                        if (!ignoreRot)
                        {
                            img.oriRot = data.euler.y;
                        }
                    }

                });
                ShowEffectCmd.Nxt();
            }

            return true;
        }
    }
}
