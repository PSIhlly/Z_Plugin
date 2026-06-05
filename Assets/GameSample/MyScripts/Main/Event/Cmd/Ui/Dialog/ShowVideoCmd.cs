using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Audio;
using Z_Code.Form;
using Z_DataSystem.Form;
using Z_Ui.Dialog;
using Z_Ui.Notify;

namespace Z_Code
{
    public class ShowVideoCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new ShowVideoCmd());
        }
        public override string GetName() => "ShowVideo";
        public override CmdBase GetNew() => new ShowVideoCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            asyncTask.interpreter.data.heapTemp.Clear();
            foreach (var p in prm)
                asyncTask.interpreter.data.heapTemp.Add(p.DeepCopy());
            var heapTemp = asyncTask.interpreter.data.heapTemp;

                AudioManager.instance.BgmPause();
            DialogManager.instance.Begin(null,  null , heapTemp[1].str , heapTemp[0].str, "", "", () =>
            {
                AudioManager.instance.BgmContinue();
                asyncTask.Complete();
            });
            return false;
        }
    }
}
