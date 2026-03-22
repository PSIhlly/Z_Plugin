using Form;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using Z_Code.Form;
using Z_DesignStyle;
using Z_Map;
using Z_Map.Form;
using Z_Text;
using Z_Ui.Dialog;
using Z_Ui.Notify;
using Z_UnitSystem.Form;

namespace Z_Code
{
    public class IsCharacterActiveCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new IsCharacterActiveCmd());
        }
        public override string GetName() => "IsCharacterActive";
        public override CmdBase GetNew() => new IsCharacterActiveCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            var data = CharacterProductForm.DataByUid.GetDv(GlobalEventHelper.GetId(prm[0].str, GlobalEventHelper.CHARACTER), null);

            var box = CodeHelper.CreateBoxByNum(data != null ? 1 : 0);
            asyncTask.res = new BoxDataForm.Data[] { box };
            return true;
        }
    }
}
