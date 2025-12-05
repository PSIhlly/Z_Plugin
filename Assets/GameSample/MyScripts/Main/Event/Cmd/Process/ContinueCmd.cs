using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Code.Form;
using Z_Text;
using Z_Ui.Notify;

namespace Z_Code
{
    public class ContinueCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new ContinueCmd());
        }
        public override string GetName() => "Continue";
        public override CmdBase GetNew() => new ContinueCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            if(GameManager.instance.curProgress.blockProgramUid == asyncTask.interpreter.data.uid)
            {
                GameManager.instance.curProgress.blockProgramUid = 0;
            }
            return true;
        }
    }
}
