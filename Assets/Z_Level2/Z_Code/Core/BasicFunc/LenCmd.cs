using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Code.Form;

namespace Z_Code
{
    public class LenCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new LenCmd());
        }
        public override string GetName() => "Len";
        public override CmdBase GetNew() => new LenCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            int i = 0;
            for (; i < prm[0].dic.Count; i++)
            {
                if (!prm[0].dic.ContainsKey(i.ToString()))
                {
                    break;
                }
            }
            asyncTask.res = new BoxDataForm.Data[] { CodeHelper.CreateBoxByNum(i) };
            return true;
        }
    }
}
