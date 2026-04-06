using Form;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using Z_Code.Form;
using Z_DesignStyle;
using Z_Map;
using Z_Text;
using Z_Ui.Dialog;
using Z_Ui.Notify;
using Z_UnitSystem.Form;

namespace Z_Code
{
    public class UseSkillCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new UseSkillCmd());
        }
        public override string GetName() => "UseSkill";
        public override CmdBase GetNew() => new UseSkillCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            var ch = CharacterProductForm.DataByUid.GetDv(GlobalEventHelper.GetId(prm[0].str, GlobalEventHelper.CHARACTER), null);
            if (ch != null)
            {
                var data = SkillProductForm.GetByEvtName(prm[1].str, ch.uid);
                if (data != null)
                {
                    PlayManager.instance.infoCtrl.UseSkill(ch.uid, data.uid,  prm[2].num == 1);
                }
            }




            return true;
        }
    }
}
