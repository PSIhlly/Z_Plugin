using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Code.Form;
using Z_Ui.Dialog;
using Z_Ui.Notify;

namespace Z_Code
{
    public class ShowDialogCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new ShowDialogCmd());
        }
        public override string GetName() => "ShowDialog";
        public override CmdBase GetNew() => new ShowDialogCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            asyncTask.interpreter.data.heapTemp.Clear();
            foreach (var p in prm)
                asyncTask.interpreter.data.heapTemp.Add(p.DeepCopy());
            var heapTemp = asyncTask.interpreter.data.heapTemp;

            DialogManager.instance.Begin(heapTemp[2].str,  heapTemp[3].str , heapTemp[0].str ,"", heapTemp[1].str,"", () =>
            {
                asyncTask.Complete();
            });
            return false;
        }
    }
}
