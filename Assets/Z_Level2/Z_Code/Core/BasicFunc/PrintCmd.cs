using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Z_Code
{
    public class PrintCmd : CmdBase
    {
        
        public override string GetName()=>"Print";

        public override int GetPrmCnt() => 1;

        public override int GetRetCnt() => 0;

        protected override Box[] ExecuteInternal(Box[] prm)
        {
            Debug.Log(prm[0].str);
            return null;
        }
    }
}
