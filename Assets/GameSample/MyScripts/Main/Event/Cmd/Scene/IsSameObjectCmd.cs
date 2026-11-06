using UnityEngine;
using Z_Code.Form;

namespace Z_Code
{
    public class IsSameObjectCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new IsSameObjectCmd());
        }

        public override string GetName() => "IsSameObject";
        public override CmdBase GetNew() => new IsSameObjectCmd();

        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            var first = GlobalEventHelper.ResolveSceneObjectTemplate(prm[0].str);
            var second = GlobalEventHelper.ResolveSceneObjectTemplate(prm[1].str);
            bool same = first != null && second != null && first.protoUid > 0
                && (first.protoUid == second.uid || first.protoUid == second.protoUid);
            asyncTask.res = new[] { CodeHelper.CreateBoxByNum(same ? 1f : 0f) };
            return true;
        }
    }
}
