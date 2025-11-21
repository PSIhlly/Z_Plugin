using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Code.Form;
using Z_Ui.Dialog;
using Z_Ui.Notify;

namespace Z_Code
{
    public class SetDialogAvatarCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new SetDialogAvatarCmd());
        }
        public override string GetName() => "SetDialogAvatar";
        public override CmdBase GetNew() => new SetDialogAvatarCmd();
        protected override BoxDataForm.Data[] ExecuteInternal(BoxDataForm.Data[] prm, InterpretLock localLock)
        {
            PlayManager.instance.data.progress.dialogCache.profilePictureName = GlobalEventHelper.GetEventAssetTexName(prm[0].str);
            return null;
        }
    }
}
