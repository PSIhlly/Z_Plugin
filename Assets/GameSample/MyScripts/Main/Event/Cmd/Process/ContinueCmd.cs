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
            var cmd = new ContinueCmd();
            Register(cmd);
            RegisterAlias("GameContinue", cmd);
        }
        public override string GetName() => "Continue";
        public override CmdBase GetNew() => new ContinueCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            if(GameManager.instance.curProgress.blockProgramUid ==(asyncTask.interpreter.data.rootUid==0? asyncTask.interpreter.data.uid: asyncTask.interpreter.data.rootUid))
            {
                GameManager.instance.curProgress.blockProgramUid = 0;
            }
            return true;
        }
    }
}
