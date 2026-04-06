using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Code.Form;
using Z_Ui.Notify;

namespace Z_Code
{
    public class FloatingTextCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new FloatingTextCmd());
        }
        public override string GetName() => "FloatingText";
        public override CmdBase GetNew() => new FloatingTextCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            var pos = MapManager.instance.utilCtrl.MapPos2RealPos(new Vector3(prm[1].dic["x"].num, prm[1].dic["height"].num, prm[1].dic["y"].num));
            if (prm[0].str == null)
            {
                PlayManager.instance.effectCtrl.FloatingText(prm[0].num.ToString(), pos);
            }
            else
            {
                PlayManager.instance.effectCtrl.FloatingText(prm[0].str, pos);
            }

            return true;
        }
    }
}
