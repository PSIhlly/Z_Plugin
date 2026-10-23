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
using Z_Text;
using Z_Ui.Dialog;
using Z_Ui.Notify;
using Z_UnitSystem.Form;

namespace Z_Code
{
    public class GetObjectRotationCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new GetObjectRotationCmd());
        }
        public override string GetName() => "GetObjectRotation";
        public override CmdBase GetNew() => new GetObjectRotationCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            var data = GlobalEventHelper.ResolveSceneObjectUnit(prm[0].str)?.data;
            asyncTask.res = new[] { CodeHelper.CreateBoxByNum(0) };
            if (data != null)
            {
                var box = CodeHelper.CreateBoxByNum(data.euler.y);
                asyncTask.res = new BoxDataForm.Data[] { box };
            }
            return true;
        }
    }
}
