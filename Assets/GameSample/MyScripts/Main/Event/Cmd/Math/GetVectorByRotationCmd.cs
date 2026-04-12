using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Z_Code.Form;
using Z_Text;
using Z_Ui.Notify;

namespace Z_Code
{
    public class GetVectorByRotationCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new GetVectorByRotationCmd());
        }
        public override string GetName() => "GetVectorByRotation";
        public override CmdBase GetNew() => new GetVectorByRotationCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            var rot = prm[0].num;
            var box = CodeHelper.CreateBox();
            var forward = Vector3.forward;
            var rotated = Quaternion.Euler(0, rot, 0) * forward;
            box.dic["x"] = CodeHelper.CreateBoxByNum(rotated.x);
            box.dic["height"] = CodeHelper.CreateBoxByNum(rotated.y);
            box.dic["y"] = CodeHelper.CreateBoxByNum(rotated.z);

            asyncTask.res = new BoxDataForm.Data[] {box };

            return true;
        }
    }
}
