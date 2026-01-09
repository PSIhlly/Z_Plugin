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
    public class MoveObjectAbsoluteCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new MoveObjectAbsoluteCmd());
        }
        public override string GetName() => "MoveObjectAbsolute";
        public override CmdBase GetNew() => new MoveObjectAbsoluteCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            var data = UnitForm.DataByUid[GlobalEventHelper.GetId(prm[0].str, GlobalEventHelper.SCENEOBJECT)];
            GameManager.instance.evtCtrl.StartTask(() =>
            {
                var pos = MapManager.instance.utilCtrl.MapPos2RealPos(new Vector3(prm[1].num, prm[3].num, prm[2].num));
                var time = prm[4].num;
                var oldPos = data.pos;
                var step = Mathf.Min(1,Time.deltaTime / time) * (pos - oldPos)+ oldPos;


                if (time <= 0)
                {
                    asyncTask.Complete();
                    return true;
                }
                else
                {
                    if (data.unit is ObjectUnit o)
                    {
                        o.Move(step);
                    }
                }
                prm[4].num -= Time.deltaTime;
                return false;
            });

            return false;
        }
    }
}
