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
            asyncTask.interpreter.data.heapTemp.Clear();
            foreach (var p in prm)
                asyncTask.interpreter.data.heapTemp.Add(p.DeepCopy());
            var heapTemp = asyncTask.interpreter.data.heapTemp;
            GameManager.instance.evtCtrl.StartTask(() =>
            {
                var data = UnitForm.DataByUid.GetDv(GlobalEventHelper.GetId(heapTemp[0].str, GlobalEventHelper.SCENEOBJECT), null);
                if (data != null)
                {
                    var pos = MapManager.instance.utilCtrl.MapPos2RealPos(new Vector3(heapTemp[1].dic["x"].num, heapTemp[1].dic["height"].num, heapTemp[1].dic["y"].num));
                    var time = Mathf.Max(0.0001f, heapTemp[2].num);
                    var oldPos = data.pos;
                    var step = Mathf.Min(1, Time.deltaTime / time) * (pos - oldPos) + oldPos;



                    {
                        if (data.unit is ObjectUnit o)
                        {
                            o.Move(step);
                        }
                    }

                }
                heapTemp[2].num -= Time.deltaTime;
                if (heapTemp[2].num <= 0)
                {
                    asyncTask.Complete();
                    return true;
                }
                return false;
            });

            return false;
        }
    }
}
