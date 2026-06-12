using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Code.Form;
using Z_DataSystem;
using Z_Ui.Dialog;
using Z_Ui.Notify;

namespace Z_Code
{
    public class ShowDialogWithIllustrationCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new ShowDialogWithIllustrationCmd());
        }
        public override string GetName() => "ShowDialogWithIllustration";
        public override CmdBase GetNew() => new ShowDialogWithIllustrationCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            asyncTask.interpreter.data.heapTemp.Clear();
            foreach (var p in prm)
                asyncTask.interpreter.data.heapTemp.Add(p.DeepCopy());
            var heapTemp = asyncTask.interpreter.data.heapTemp;

            var handle = PlayManager.instance.assetCtrl.Add(AssetManager.instance.texCtrl.GetId(prm[4].str), prm[7].num);
            PlayManager.instance.assetCtrl.SetRemoveTime(handle, int.MaxValue);
            PlayManager.instance.assetCtrl.SetPos(handle,new Vector2(prm[5].num, prm[6].num), 0);

            DialogManager.instance.Begin(heapTemp[2].str,  heapTemp[3].str , AssetManager.instance.texCtrl.GetId(heapTemp[0].str) ,0, AssetManager.instance.texCtrl.GetId(heapTemp[1].str),0, () =>
            {
                PlayManager.instance.assetCtrl.Remove(handle);
                asyncTask.Complete();

            });
            return false;
        }
    }
}
