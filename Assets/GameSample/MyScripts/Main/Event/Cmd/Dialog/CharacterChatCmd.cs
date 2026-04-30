using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Code.Form;
using Z_Ui.Dialog;
using Z_Ui.Notify;

namespace Z_Code
{
    public class CharacterChatCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new CharacterChatCmd());
        }
        public override string GetName() => "CharacterChat";
        public override CmdBase GetNew() => new CharacterChatCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            asyncTask.interpreter.data.heapTemp.Clear();
            foreach (var p in prm)
                asyncTask.interpreter.data.heapTemp.Add(p.DeepCopy());
            var heapTemp = asyncTask.interpreter.data.heapTemp;
            PlayManager.instance.effectCtrl.ChatText(GlobalEventHelper.GetId(prm[0].str, GlobalEventHelper.CHARACTER), prm[1].str, prm[2].str, prm[3].num);
            return true;
        }
    }
}
