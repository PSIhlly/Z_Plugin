using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Z_Code
{
    public class PrintCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Register()
        {
            Register(new PrintCmd());
        }
        public override string GetName()=> "Print";

        public override int GetPrmCnt() => 1;

        public override int GetRetCnt() => 0;
        public override CmdBase GetNew()=>new PrintCmd();
        protected override Box[] ExecuteInternal(Box[] prm)
        {
            if(prm[0].str==null)
            {
                Debug.Log(prm[0].num);
            }
            else
            {
                Debug.Log(prm[0].str);
            }
            return null;
        }
    }
}
