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
            if(PlayManager.instance.data.progress.blockProgramUid == asyncTask.interpreter.data.uid)
            {
                PlayManager.instance.data.progress.blockProgramUid = 0;
            }
            return true;
        }
    }
}
