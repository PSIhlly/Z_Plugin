using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Code.Form;
using Z_Text;
using Z_Ui.Notify;

namespace Z_Code
{
    public class PauseCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            var cmd = new PauseCmd();
            Register(cmd);
            RegisterAlias("GamePause", cmd);
        }
        public override string GetName() => "Pause";
        public override CmdBase GetNew() => new PauseCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            GameManager.instance.curProgress.blockProgramUid = asyncTask.interpreter.data.rootUid == 0 ? asyncTask.interpreter.data.uid : asyncTask.interpreter.data.rootUid;
            return true;
        }
    }
}
