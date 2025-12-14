using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Code.Form;
using Z_Text;
using Z_Time;
using Z_Ui.Notify;

namespace Z_Code
{
    public class GameOverCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new GameOverCmd());
        }
        public override string GetName() => "GameOver";
        public override CmdBase GetNew() => new GameOverCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            TimeManager.instance.AddCurLateUpdateWithoutCheckAction(() => { PlayManager.instance.Exit();});
            return true;
        }
    }
}
