using Form;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using Z_Code.Form;
using Z_DesignStyle;
using Z_Map;
using Z_Map.Form;
using Z_Text;
using Z_Ui.Dialog;
using Z_Ui.Notify;
using Z_UnitSystem.Form;

namespace Z_Code
{
    public class CompleteMissionCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new CompleteMissionCmd());
        }
        public override string GetName() => "CompleteMission";
        public override CmdBase GetNew() => new CompleteMissionCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            var data = MissionForm.DataByName.GetDv(prm[0].str, null);
            if (data != null &&!data.done)
            {
                data.done = true;
                Z_EventHelper.Invoke(new MissionEvent { type = MissionEventType.Done, data = data });
            }
            return true;
        }
    }
}
