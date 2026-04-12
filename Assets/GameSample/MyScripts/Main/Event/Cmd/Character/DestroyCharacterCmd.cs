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
using Z_Text;
using Z_Ui.Dialog;
using Z_Ui.Notify;
using Z_UnitSystem.Form;

namespace Z_Code
{
    public class DestroyCharacterCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new DestroyCharacterCmd());
        }
        public override string GetName() => "DestroyCharacter";
        public override CmdBase GetNew() => new DestroyCharacterCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            var data = PlayManager.instance.sceneCtrl.GetCharacterUnit(GlobalEventHelper.GetId(prm[0].str, GlobalEventHelper.CHARACTER));
            if (data != null)
            {
                var unit = data.unit;
                if (unit is CharacterUnit c)
                {
                    var form = CharacterProductForm.DataByUid.GetDv(c.productInfo.Item1, null);
                    if (form != null && !form.unique)
                    {
                        CharacterProductForm.RemoveData(form.uid);
                        MapManager.instance.RemoveCharacter(c.data);
                    }

                }
            }
            return true;
        }
    }
}
