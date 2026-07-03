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
        public static Vector3 DeltaHeight = Vector3.up* 0.019f;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new ShowEffectCmd());
        }
        public override string GetName() => "ShowEffect";
        public override CmdBase GetNew() => new ShowEffectCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            PlayManager.instance.effectCtrl.CreatEffect(GlobalEventHelper.GetId(prm[0].str, GlobalEventHelper.EFFECT), MapManager.instance.utilCtrl.MapPos2RealPos(GameManager.PlayerPosToMapPos(new Vector3(prm[1].dic["x"].num, prm[1].dic["height"].num, prm[1].dic["y"].num)) + DeltaHeight), prm[2].num);
            Nxt();
            return true;
        }
        public static void Nxt()
        {
            DeltaHeight.y = (DeltaHeight.y + 0.19f) % 0.73f;
        }
    }
}
