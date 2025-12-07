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
    public class MoveObjectCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new MoveObjectCmd());
        }
        public override string GetName() => "MoveObject";
        public override CmdBase GetNew() => new MoveObjectCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            var unit = UnitForm.DataByUid[(int)prm[0].num].unit;
            GameManager.instance.evtCtrl.StartTask(() =>
            {
                var relaPos = new Vector3(prm[1].num, prm[3].num, prm[2].num);
                var time = prm[4].num;
                var step = Mathf.Min(1,Time.deltaTime / time) * relaPos;
                prm[1].num -= step.x;
                prm[2].num -= step.z;
                prm[3].num -= step.y;
                if (time <= 0)
                {
                    asyncTask.Complete();
                    return true;
                }
                else
                {
                    if (unit is ObjectUnit o)
                    {
                        o.Move(step);
                    }
                    else if (unit is ItemUnit i)
                    {
                        
                    }
                }
                prm[4].num -= Time.deltaTime;
                return false;
            });

            return false;
        }
    }
}
