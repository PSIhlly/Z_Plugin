using Form;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using Z_Code.Form;
using Z_Map;
using Z_Text;
using Z_Ui.Dialog;
using Z_Ui.Notify;
using Z_UnitSystem.Form;

namespace Z_Code
{
    public class GetCurrentCharacterCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new GetCurrentCharacterCmd());
        }
        public override string GetName() => "GetCurrentCharacter";
        public override CmdBase GetNew() => new GetCurrentCharacterCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            var player = PlayManager.instance.sceneCtrl.playerG;
            asyncTask.res = new BoxDataForm.Data[] { CodeHelper.CreateBoxByStr(player == null ? string.Empty : GlobalEventHelper.GetName(GlobalEventHelper.CHARACTER, player.uid.ToString())) };
            return true;
        }
    }
}
