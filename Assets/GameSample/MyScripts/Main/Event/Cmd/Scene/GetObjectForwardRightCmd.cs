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
    public class GetObjectRightVectorCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new GetObjectRightVectorCmd());
        }
        public override string GetName() => "GetObjectRightVector";
        public override CmdBase GetNew() => new GetObjectRightVectorCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            var data = UnitForm.DataByUid.GetDv(GlobalEventHelper.GetId(prm[0].str, GlobalEventHelper.SCENEOBJECT), null);
            if (data != null)
            {
                var box = CodeHelper.CreateBox();
                var right = Quaternion.Euler(data.euler) * Vector3.right;
                box.dic["x"] = CodeHelper.CreateBoxByNum(right.x);
                box.dic["height"] = CodeHelper.CreateBoxByNum(right.y);
                box.dic["y"] = CodeHelper.CreateBoxByNum(right.z);
                asyncTask.res = new BoxDataForm.Data[] { box };
            }
            return true;
        }
    }
}
