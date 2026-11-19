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
        public static Vector3 Delta = (Vector3.up+Vector3.back)* 0.0011f;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new ShowEffectCmd());
        }
        public override string GetName() => "ShowEffect";
        public override CmdBase GetNew() => new ShowEffectCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            var components1 = prm[1].GetOrCreateDictionary();
            PlayManager.instance.effectCtrl.CreatEffect(GlobalEventHelper.GetId(prm[0].str, GlobalEventHelper.EFFECT), GameManager.PlayerPosToMapPos(new Vector3(components1["x"].num, components1["height"].num, components1["y"].num)) + Delta, prm[2].num);
            Nxt();
            return true;
        }
        public static void Nxt()
        {
            Delta.y = (Delta.y + 0.0011f) % 0.73f;
        }
    }
}
