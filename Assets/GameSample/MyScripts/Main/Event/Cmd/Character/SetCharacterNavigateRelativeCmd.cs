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
    public class SetCharacterNavigateRelativeCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new SetCharacterNavigateRelativeCmd());
        }
        public override string GetName() => "SetCharacterNavigateRelative";
        public override CmdBase GetNew() => new SetCharacterNavigateRelativeCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            if (!CharacterProductForm.TryGetReferenceUid(prm[0].str, out var uid))
                return true;
            foreach (var data in PlayManager.instance.sceneCtrl.GetCharacterUnits(uid))
            {
                var productData = CharacterProductForm.DataByUid[data.unit.productInfo.Item1];
                data.speed = SetCharacterNavigateCmd.GetNavigationSpeed(productData, data.speed);
                data.pathDis = 999;
                data.alertDis = 999;
                data.destination = data.pos + MapManager.instance.utilCtrl.MapPos2RealPos(new Vector3(prm[1].dic["x"].num, prm[1].dic["height"].num, prm[1].dic["y"].num));

                productData.enableNav = true;
            }

            return true;
        }
    }
}
