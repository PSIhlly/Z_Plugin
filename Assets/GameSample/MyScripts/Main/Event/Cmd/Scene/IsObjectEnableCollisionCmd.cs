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
    public class IsObjectEnableCollisionCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new IsObjectEnableCollisionCmd());
        }
        public override string GetName() => "IsObjectEnableCollision";
        public override CmdBase GetNew() => new IsObjectEnableCollisionCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            var unit = GlobalEventHelper.ResolveSceneObjectUnit(prm[0].str);
            if (unit is ObjectUnit obj)
            {
                var box = CodeHelper.CreateBoxByNum(obj.data.isObstacle?1:0);
                asyncTask.res = new BoxDataForm.Data[] { box };
            }else
            {
                asyncTask.res = new BoxDataForm.Data[] { CodeHelper.CreateBoxByNum(0) };
            }
            return true;
        }
    }
}
