using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Code.Form;

namespace Z_Code
{
    public class GetValuesCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new GetValuesCmd());
        }
        public override string GetName() => "GetValues";
        public override CmdBase GetNew() => new GetValuesCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            int i = 0;
            var box = CodeHelper.CreateBox();
            foreach (var pair in prm[0].dic)
            {
                box.dic[i.ToString()] = pair.Value.DeepCopy();
                i++;
            }
            asyncTask.res = new BoxDataForm.Data[] { box };
            return true;
        }
    }
}
