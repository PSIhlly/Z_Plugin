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
    public class IsMissionCompleteCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new IsMissionCompleteCmd());
        }
        public override string GetName() => "IsMissionComplete";
        public override CmdBase GetNew() => new IsMissionCompleteCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            var data = MissionForm.DataByName.GetDv(prm[0].str, null);
            var box = CodeHelper.CreateBoxByNum(data != null&&data.done ? 1 : 0);
            asyncTask.res = new BoxDataForm.Data[] { box };
            return true;
        }
    }
}
