using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Code.Form;
using Z_Text;
using Z_Ui.Notify;

namespace Z_Code
{
    public class NewVectorCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new NumCmd());
        }
        public override string GetName() => "NewVector";
        public override CmdBase GetNew() => new NumCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            var box = CodeHelper.CreateBox();
            box.dic["x"] = CodeHelper.CreateBoxByNum(prm[0].num);
            box.dic["y"] = CodeHelper.CreateBoxByNum(prm[1].num);
            box.dic["height"] = CodeHelper.CreateBoxByNum(prm[2].num);
            asyncTask.res = new BoxDataForm.Data[] { box };
            return true;
        }
    }
}
