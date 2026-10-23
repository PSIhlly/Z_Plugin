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
    public class SetCharacterParameterCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new SetCharacterParameterCmd());
        }
        public override string GetName() => "SetCharacterParameter";
        public override CmdBase GetNew() => new SetCharacterParameterCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            foreach (var product in CharacterProductForm.ResolveReferences(prm[0].str))
                if (product.paramDic != null && product.paramDic.TryGetValue(prm[1].str, out var parameter)
                    && parameter != null)
                    PlayManager.instance.infoCtrl.ChangeCharacterParam(product.uid, prm[1].str, prm[2]);

            return true;
        }
    }
}
