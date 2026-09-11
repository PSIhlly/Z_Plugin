using Form;
using UnityEngine;
using Z_Code.Form;

namespace Z_Code
{
    public class GetCharacterNameCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new GetCharacterNameCmd());
        }

        public override string GetName() => "GetCharacterName";
        public override CmdBase GetNew() => new GetCharacterNameCmd();

        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            int characterUid = GlobalEventHelper.GetId(prm[0].str, GlobalEventHelper.CHARACTER);
            string characterName = CharacterProductForm.DataByUid.TryGetValue(characterUid, out var character)
                ? character.name
                : string.Empty;

            asyncTask.res = new[] { CodeHelper.CreateBoxByStr(characterName) };
            return true;
        }
    }
}
