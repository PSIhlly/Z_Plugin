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

            var cache = GameManager.instance.curProgress.dialogCache;
            cache.mainAudioName = "";
            cache.title = prm[2].str;
            cache.mainText = prm[3].str;
            cache.mainPictureName = prm[0].str;
            cache.mainVideoName = "";
            cache.mainPictureName = prm[1].str;
            cache.mainAudioName = "";
            DialogManager.instance.Begin(prm[2].str,  prm[3].str , prm[0].str ,"", prm[1].str,"", () =>
            {
                asyncTask.Complete();
            });
            return false;
        }
    }
}
