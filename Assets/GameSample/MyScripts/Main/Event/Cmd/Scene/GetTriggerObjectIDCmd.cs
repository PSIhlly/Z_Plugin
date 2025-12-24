using Form;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using Z_Code.Form;
using Z_Map;
using Z_Map.Form;
using Z_Text;
using Z_Ui.Dialog;
using Z_Ui.Notify;
using Z_UnitSystem.Form;

namespace Z_Code
{
    public class GetTriggerObjectIDCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new GetTriggerObjectIDCmd());
        }
        public override string GetName() => "GetTriggerObjectID";
        public override CmdBase GetNew() => new GetTriggerObjectIDCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            asyncTask.res = new BoxDataForm.Data[] { CodeHelper.CreateBoxByNum(((EventInterpretDataForm.Data)asyncTask.interpreter.data).args[1].num) };
            return true;
        }
    }
}
