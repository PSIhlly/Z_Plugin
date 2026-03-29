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
    public class SetSkillParameterCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new SetSkillParameterCmd());
        }
        public override string GetName() => "SetSkillParameter";
        public override CmdBase GetNew() => new SetSkillParameterCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            PlayManager.instance.infoCtrl.ChangeSkillParam(GlobalEventHelper.GetId(prm[0].str, GlobalEventHelper.SKILL), prm[1].str, prm[2]);

            return true;
        }
    }
}
