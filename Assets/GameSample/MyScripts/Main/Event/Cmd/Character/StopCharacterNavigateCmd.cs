using Form;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Z_Code.Form;
using Z_DataSystem;
using Z_Map;
using Z_Map.Form;
using Z_Text;
using Z_Time;
using Z_Ui.Dialog;
using Z_Ui.Notify;
using Z_UnitSystem.Form;

namespace Z_Code
{
    public class StopCharacterNavigateCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new StopCharacterNavigateCmd());
        }
        public override string GetName() => "StopCharacterNavigate";
        public override CmdBase GetNew() => new StopCharacterNavigateCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            if (!CharacterProductForm.TryGetReferenceUid(prm[0].str, out var uid))
                return true;
            foreach (var data in PlayManager.instance.sceneCtrl.GetCharacterUnits(uid))
            {
                var productData = CharacterProductForm.DataByUid[data.unit.productInfo.Item1];
                productData.enableNav = false;
            }
            return true;
        }
    }
}
