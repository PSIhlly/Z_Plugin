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
            var components1 = prm[1].GetOrCreateDictionary();
            var pos = GameManager.PlayerPosToMapPos(new Vector3(components1["x"].num, components1["height"].num, components1["y"].num));
            PlayManager.instance.effectCtrl.FloatingText(prm[0].ToString(), pos);

            return true;
        }
    }
}
