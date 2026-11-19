using Form;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
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
            var components0 = prm[0].GetOrCreateDictionary();
            var components1 = prm[1].GetOrCreateDictionary();
            var inputFrom = new Vector3(components0["x"].num, components0["height"].num, components0["y"].num);
            var inputTo = new Vector3(components1["x"].num, components1["height"].num, components1["y"].num);
            var from = GameManager.PlayerPosToMapPos(inputFrom);
            var to = GameManager.PlayerPosToMapPos(inputTo);
            var radius = prm[2].num;
            bool blocked = MapManager.instance.utilCtrl.HasBlockingCast(from, to, radius);
            asyncTask.res = new[] { CodeHelper.CreateBoxByNum(blocked ? 0 : 1) };

            return true;
        }
    }
}
