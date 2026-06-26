using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Audio;
using Z_Code.Form;
using Z_DataSystem;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Ui.Notify;

namespace Z_Code
{
    public class PlayAudioCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new PlayAudioCmd());
        }
        public override string GetName() => "PlayAudio";
        public override CmdBase GetNew() => new PlayAudioCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            var data = AudioAssetForm.DataById.GetDv(AssetManager.instance.audioCtrl.GetId(prm[0].str), null);
            if (data != null)
            {
                AudioManager.instance.Play(data.path);
            }
            else
            {
                asyncTask.error = "PlayAudio 找不到音频资源: " + prm[0].str;
            }

            return true;
        }
    }
}
