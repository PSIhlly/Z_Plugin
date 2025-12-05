using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Code.Form;
using Z_Text;
using Z_Ui.Notify;

namespace Z_Code
{
    public class LoadCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new LoadCmd());
        }
        public override string GetName() => "Load";
        public override CmdBase GetNew() => new LoadCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            int curId = GameManager.instance.curStory.id;
            bool box = PlayManager.instance.boxPlay;
            Main2StoryManager.instance.UnloadScenePlay();
            Main2StoryManager.instance.UnloadStoryPlay();
            Main2StoryManager.instance.StartLoadStoryPlay(curId, box);
            return true;
        }
    }
}
