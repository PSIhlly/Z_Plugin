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
    public class GetCharacterRotationCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new GetCharacterRotationCmd());
        }
        public override string GetName() => "GetCharacterRotation";
        public override CmdBase GetNew() => new GetCharacterRotationCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            var data = PlayManager.instance.sceneCtrl.GetCharacterUnit(GlobalEventHelper.GetId(prm[0].str, GlobalEventHelper.CHARACTER));
            if (data != null)
            {
                var box = CodeHelper.CreateBoxByNum(data.euler.y);
                asyncTask.res = new BoxDataForm.Data[] { box };
            }
            return true;
        }
    }
}
