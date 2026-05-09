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
    public class AddMissionCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new AddMissionCmd());
        }
        public override string GetName() => "AddMission";
        public override CmdBase GetNew() => new AddMissionCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            var data = MissionForm.DataByName.GetDv(prm[0].str, null);
            if(data!=null&&!data.received)
            {
                data.received = true;
                Z_EventHelper.Invoke(new MissionEvent { type = MissionEventType.Add, data = data });
            }
            return true;
        }
    }
}
