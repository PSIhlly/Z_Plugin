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
    public class GetLookAtRotationCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new GetLookAtRotationCmd());
        }
        public override string GetName() => "GetLookAtRotation";
        public override CmdBase GetNew() => new GetLookAtRotationCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            // The command returns a yaw, so height must not influence the facing.
            // Convert only the delta: the player-space origin offset cancels out.
            var delta = new Vector3(
                prm[1].dic["x"].num - prm[0].dic["x"].num,
                0f,
                prm[1].dic["y"].num - prm[0].dic["y"].num);
            delta = MapManager.instance.utilCtrl.MapPos2RealPos(delta);
            float yaw = delta.x == 0f && delta.z == 0f
                ? 0f
                : Mathf.Repeat(Mathf.Atan2(delta.x, delta.z) * Mathf.Rad2Deg, 360f);
            asyncTask.res = new[] { CodeHelper.CreateBoxByNum(yaw) };
            return true;
        }
    }
}
