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
using Z_Math;
using Z_Text;
using Z_Time;
using Z_Ui.Dialog;
using Z_Ui.Notify;
using Z_UnitSystem.Form;

namespace Z_Code
{
    public class SetObjectRotationCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new SetObjectRotationCmd());
        }
        public override string GetName() => "SetObjectRotation";
        public override CmdBase GetNew() => new SetObjectRotationCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            var data = UnitForm.DataByUid[GlobalEventHelper.GetId(prm[0].str, GlobalEventHelper.SCENEOBJECT)];

            GameManager.instance.evtCtrl.StartTask(() =>
            {
                var euler = prm[1].num;
                var time = Mathf.Max(0.0001f, prm[2].num);


                if (data != null && data is ObjectUnitForm.Data oData)
                {
                    var step = Mathf.Min(1, Time.deltaTime / time) * (euler - oData.euler.y);
                    var nextEuler = oData.euler.NewSetY(oData.euler.y + step);
                    MapManager.instance.updateCtrl.ApplyMove(
                        oData.unit,
                        oData.pos,
                        nextEuler,
                        true);
                }
                prm[2].num -= Time.deltaTime;
                if (prm[2].num <= 0)
                {
                    asyncTask.Complete();
                    return true;
                }
                return false;
            });


            return true;
        }
    }
}
