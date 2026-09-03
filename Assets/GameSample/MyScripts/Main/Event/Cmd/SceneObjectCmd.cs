using System;
using System.Collections;
using System.Collections.Generic;
using Z_Code.Form;
using Z_Text;
using UnityEngine;
namespace Z_Code
{
    public class SceneObjectProtoCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new SceneObjectProtoCmd());
        }

        public override void GetUnitChooseCode(Action<string> act, SyntaxNode cur)
        {
            ModManager.instance.assetCtrl.ChooseSceneObject(TextManager.instance.GetTxt("Choose scene object"), (data) =>
            {
                if (data != null)
                    act?.Invoke($"\"{GlobalEventHelper.GetName(GlobalEventHelper.SCENEOBJECTPROTO, data.id.ToString())}\"");
            });
        }

        public override string GetName() => "SceneObjectProto";
        public override CmdBase GetNew() => new SceneObjectProtoCmd();

        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            asyncTask.error = "SceneObjectProto 常量不能直接执行";
            return true;
        }
    }
}
