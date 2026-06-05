using Form;
using System;
using System.Collections;
using System.Collections.Generic;
using Ui.TeamSelectWindow;
using Unity.VisualScripting;
using UnityEngine;
using Z_Code.Form;
using Z_DataSystem;
using Z_DesignStyle;
using Z_Map;
using Z_Map.Form;
using Z_Math;
using Z_Text;
using Z_Time;
using Z_Ui;
using Z_Ui.Dialog;
using Z_Ui.Notify;
using Z_UnitSystem.Form;

namespace Z_Code
{
    public class ShowTeamerSelectCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new ShowTeamerSelectCmd());
        }
        public override string GetName() => "ShowTeamerSelect";
        public override CmdBase GetNew() => new ShowTeamerSelectCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            UiManager.instance.ShowUi<UiTeamSelectWindowCtrl>();
            GameManager.instance.evtCtrl.StartTask(() =>
            {
                if (UiManager.instance.GetUi<UiTeamSelectWindowCtrl>() == null || !UiManager.instance.GetUi<UiTeamSelectWindowCtrl>().active)
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
