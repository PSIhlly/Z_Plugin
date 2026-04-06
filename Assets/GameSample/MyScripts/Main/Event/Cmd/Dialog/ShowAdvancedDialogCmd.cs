using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Audio;
using Z_Code.Form;
using Z_DataSystem;
using Z_DataSystem.Form;
using Z_Ui.Dialog;
using Z_Ui.Notify;

namespace Z_Code
{
    public class ShowAdvancedDialogCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new ShowAdvancedDialogCmd());
        }
        public override string GetName() => "ShowAdvancedDialog";
        public override CmdBase GetNew() => new ShowAdvancedDialogCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            asyncTask.interpreter.data.heapTemp.Clear();
            foreach (var p in prm)
                asyncTask.interpreter.data.heapTemp.Add(p.DeepCopy());
            var heapTemp = asyncTask.interpreter.data.heapTemp;
            if (VideoAssetForm.DataByName.ContainsKey(heapTemp[5].str))
                AudioManager.instance.BgmPause();
            if (heapTemp[1].str =="$i$$i$")
                heapTemp[1].str = null;
            if (heapTemp[0].str == "$i$$i$")
                heapTemp[0].str = null;

            DialogManager.instance.Begin(heapTemp[2].str, heapTemp[3].str, heapTemp[0].str, heapTemp[5].str, heapTemp[1].str, heapTemp[4].str, () =>
            {
                AudioManager.instance.BgmContinue();
                asyncTask.Complete();
            }, heapTemp[6].num == 1);
            return false;
        }
    }
}
