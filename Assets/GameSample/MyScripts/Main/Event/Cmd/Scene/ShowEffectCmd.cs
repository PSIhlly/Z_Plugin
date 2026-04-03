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
    public class ShowEffectCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new ShowEffectCmd());
        }
        public override string GetName() => "ShowEffect";
        public override CmdBase GetNew() => new ShowEffectCmd();
        private float DeltaSpeed=0.0002f;
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            PlayManager.instance.effectCtrl.CreatEffect(GlobalEventHelper.GetId(prm[0].str, GlobalEventHelper.EFFECT), MapManager.instance.utilCtrl.MapPos2RealPos(new Vector3(prm[1].dic["x"].num, prm[1].dic["height"].num+ DeltaSpeed, prm[1].dic["y"].num)), prm[2].num);
            DeltaSpeed = (DeltaSpeed + 0.0002f) % 0.002f;
            return true;
        }
    }
}
