using Form;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Z_Code.Form;
using Z_DataSystem;
using Z_DesignStyle;
using Z_Map;
using Z_Map.Form;
using Z_Math;
using Z_Text;
using Z_Time;
using Z_Ui.Dialog;
using Z_Ui.Notify;
using Z_UnitSystem.Form;

namespace Z_Code
{
    public class CharacterStunCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new CharacterStunCmd());
        }
        public override string GetName() => "CharacterStun";
        public override CmdBase GetNew() => new CharacterStunCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            var productData = CharacterProductForm.DataByUid.GetDv(GlobalEventHelper.GetId(prm[0].str, GlobalEventHelper.CHARACTER), null);
            if (productData != null)
            {
                productData.recoveryTime = Math.Max(productData.recoveryTime, GameManager.instance.curProgress.seconds + prm[1].num);
            }
            return true;
        }
    }
}
