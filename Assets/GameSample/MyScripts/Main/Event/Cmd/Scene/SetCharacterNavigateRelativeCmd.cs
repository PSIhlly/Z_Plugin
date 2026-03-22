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
            var productData = CharacterProductForm.DataByUid[GlobalEventHelper.GetId(prm[0].str, GlobalEventHelper.CHARACTER)];
            var data = PlayManager.instance.sceneCtrl.GetCharacterUnit(productData.uid);
            if (data != null)
            {
                data.speed = productData.paramDic[productData.speedParamName].GetValue().num;
                data.pathDis = 999;
                data.alertDis = 999;
                data.destination = data.pos + MapManager.instance.utilCtrl.MapPos2RealPos(new Vector3(prm[1].dic["x"].num, prm[1].dic["height"].num, prm[1].dic["y"].num));
                data.navEnabled = true;
            }

            return true;
        }
    }
}
