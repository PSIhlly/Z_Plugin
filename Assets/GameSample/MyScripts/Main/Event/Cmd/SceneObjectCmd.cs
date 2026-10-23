using System;
using System.Collections;
using System.Collections.Generic;
using Z_Code.Form;
using Z_Text;
using UnityEngine;
namespace Z_Code
{
    public class SceneObjectCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new SceneObjectCmd());
            RegisterAlias("SceneObjectProto", new SceneObjectCmd()); // Old authored method-name compatibility.
        }

        public override void GetUnitChooseCode(Action<string> act, SyntaxNode cur)
        {
            ModManager.instance.assetCtrl.ChooseSceneObject(TextManager.instance.GetTxt("Choose scene object"), (data) =>
            {
                if (data != null)
                    act?.Invoke($"\"{GlobalEventHelper.GetSceneObjectProductReference(data.uid)}\"");
            });
        }

        public override string GetName() => "SceneObject";
        public override CmdBase GetNew() => new SceneObjectCmd();

        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            asyncTask.error = "SceneObject 常量不能直接执行";
            return true;
        }
    }

    public class SceneObjectProtoCmd : SceneObjectCmd
    {
        public override string GetName() => "SceneObjectProto";
        public override CmdBase GetNew() => new SceneObjectProtoCmd();
    }
}
