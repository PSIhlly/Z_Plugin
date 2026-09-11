using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Code.Form;
using Z_DataSystem;
using Z_DataSystem.Form;
using Z_Ui.Dialog;
using Z_Ui.Notify;

namespace Z_Code
{
    public class ShowDialogCmd : CmdBase
    {
        private const int AudioParameterIndex = 4;
        private const int FirstIllustrationParameterIndex = 5;
        private const int IllustrationCount = 5;

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
            var handles = new List<int>();
            for (int illustrationIndex = 0; illustrationIndex < IllustrationCount; illustrationIndex++)
            {
                int parameterIndex = FirstIllustrationParameterIndex + illustrationIndex;
                int texId = AssetManager.instance.texCtrl.GetId(GetParameter(heapTemp, parameterIndex));
                if (TexAssetForm.DataById.ContainsKey(texId))
                {
                    int handle = PlayManager.instance.assetCtrl.Add(texId, 1);
                    handles.Add(handle);
                    PlayManager.instance.assetCtrl.SetRemoveTime(handle, int.MaxValue);
                    PlayManager.instance.assetCtrl.SetPos(handle, new Vector2(1 / 6f * (illustrationIndex + 1), 0), 0);
                }
            }

            DialogManager.instance.Begin(GetParameter(heapTemp, 2), GetParameter(heapTemp, 3), AssetManager.instance.texCtrl.GetId(GetParameter(heapTemp, 0)), 0, AssetManager.instance.texCtrl.GetId(GetParameter(heapTemp, 1)), AssetManager.instance.audioCtrl.GetId(GetParameter(heapTemp, AudioParameterIndex)), () =>
            {
                foreach (int handle in handles)
                {
                    PlayManager.instance.assetCtrl.Remove(handle);
                }
                asyncTask.Complete();

            });
            return false;
        }

        private static string GetParameter(List<BoxDataForm.Data> parameters, int index)
        {
            return index >= 0 && index < parameters.Count && parameters[index] != null
                ? parameters[index].str
                : string.Empty;
        }
    }
}
