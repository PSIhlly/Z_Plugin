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
    public class SetCharacterPositionRelativeCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new SetCharacterPositionRelativeCmd());
        }
        public override string GetName() => "SetCharacterPositionRelative";
        public override CmdBase GetNew() => new SetCharacterPositionRelativeCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            var productData = CharacterProductForm.DataByUid[GlobalEventHelper.GetId(prm[0].str, GlobalEventHelper.CHARACTER)];
            var data = PlayManager.instance.sceneCtrl.GetCharacterUnit(productData.uid);

            var pos = data.pos + MapManager.instance.utilCtrl.MapPos2RealPos(new Vector3(prm[1].num, prm[3].num, prm[2].num));

            if (data.unit is CharacterUnit o)
            {
                MapManager.instance.updateCtrl.ApplyMove(data.unit, pos, data.euler, true);
            }

            return true;
        }
    }
}
