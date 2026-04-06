using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Code.Form;

namespace Z_Code
{
    public class GetKeysCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new GetKeysCmd());
        }
        public override string GetName() => "GetKeys";
        public override CmdBase GetNew() => new GetKeysCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            int i = 0;
            var box = CodeHelper.CreateBox();
            foreach (var pair in prm[0].dic)
            {
                box.dic[i.ToString()] = CodeHelper.CreateBoxByStr(pair.Key);
                i++;
            }
            asyncTask.res = new BoxDataForm.Data[] { box };
            return true;
        }
    }
}
