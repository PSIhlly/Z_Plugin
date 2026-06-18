using Form;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using Z_Code.Form;
using Z_DesignStyle;
using Z_Map;
using Z_Map.Analysis;
using Z_Text;
using Z_Ui.Dialog;
using Z_Ui.Notify;
using Z_UnitSystem.Form;

namespace Z_Code
{
    public class GetVectorLengthCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new GetVectorLengthCmd());
        }
        public override string GetName() => "GetVectorLength";
        public override CmdBase GetNew() => new GetVectorLengthCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            var a = MapManager.instance.utilCtrl.MapPos2RealPos(GameManager.PlayerPosToMapPos(new Vector3(prm[0].dic["x"].num, prm[0].dic["height"].num, prm[0].dic["y"].num)));
            asyncTask.res = new BoxDataForm.Data[] { CodeHelper.CreateBoxByNum(a.magnitude) }; 
            return true;
        }
    }
}
