using Form;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.GPUDriven;
using Z_Code.Form;
using Z_DataSystem;
using Z_Map;
using Z_Map.Analysis;
using Z_Map.Form;
using Z_Text;
using Z_Time;
using Z_Ui.Dialog;
using Z_Ui.Notify;
using Z_UnitSystem.Form;

namespace Z_Code
{
    public class IsUnobstructedCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new IsUnobstructedCmd());
        }
        public override string GetName() => "IsUnobstructed";
        public override CmdBase GetNew() => new IsUnobstructedCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            var from = MapManager.instance.utilCtrl.MapPos2RealPos(new Vector3(prm[0].dic["x"].num, prm[0].dic["height"].num, prm[0].dic["y"].num))+Vector3.up*0.5f;
            var to = MapManager.instance.utilCtrl.MapPos2RealPos(new Vector3(prm[1].dic["x"].num, prm[1].dic["height"].num, prm[1].dic["y"].num)) + Vector3.up * 0.5f;
            var radius = prm[2].num;
            var res = MapManager.instance.utilCtrl.CaptureCast(from, to, radius);
            res = MapManager.instance.utilCtrl.CaptureCast(from, to, radius);
            asyncTask.res = new BoxDataForm.Data[] { CodeHelper.CreateBoxByNum(1) };
            foreach (var u in res)
            {
                if (u is ObjectUnit obj && obj.data.isObstacle)
                {
                    asyncTask.res[0].num = 0;
                    break;
                }
            }

            return true;
        }
    }
}
