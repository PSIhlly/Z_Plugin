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
            string characterReference = prm[0].str ?? string.Empty;
            int characterUid = 0;
            if (GlobalEventHelper.IsAsset(characterReference, GlobalEventHelper.CHARACTER))
            {
                int.TryParse(characterReference.Split(GlobalEventHelper.CHARACTER)[1], out characterUid);
            }

            string characterName = CharacterProductForm.DataByUid.TryGetValue(characterUid, out var character) && character != null
                ? character.name ?? string.Empty
                : string.Empty;

            asyncTask.res = new[] { CodeHelper.CreateBoxByStr(characterName) };
            return true;
        }
    }
}
