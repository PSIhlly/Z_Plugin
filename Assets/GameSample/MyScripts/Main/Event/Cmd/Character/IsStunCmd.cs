using Form;
using UnityEngine;
using Z_Code.Form;

namespace Z_Code
{
    public class IsStunCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new IsStunCmd());
        }

        public override string GetName() => "IsStun";
        public override CmdBase GetNew() => new IsStunCmd();

        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            var character = CharacterProductForm.ResolveReference(prm[0].str);
            var progress = GameManager.instance.curProgress;
            bool stunned = character != null && progress != null
                && character.recoveryTime > progress.seconds;

            asyncTask.res = new[] { CodeHelper.CreateBoxByNum(stunned ? 1 : 0) };
            return true;
        }
    }
}
