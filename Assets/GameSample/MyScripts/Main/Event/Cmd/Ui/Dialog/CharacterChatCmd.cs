using UnityEngine;
using Z_Code.Form;
using Z_DataSystem;
using Z_Map;

namespace Z_Code
{
    public class CharacterChatCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new CharacterChatCmd());
        }
        public override string GetName() => "CharacterChat";
        public override CmdBase GetNew() => new CharacterChatCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            var units = SceneUnitCommand.ResolveAll(prm[0].str);
            string text = prm[1].str;
            int image = AssetManager.instance.texCtrl.GetId(prm[2].str);
            float seconds = prm[3].num;
            foreach (var unit in units)
                if (unit is CharacterUnit character && SceneUnitCommand.IsLive(character))
                    PlayManager.instance.effectCtrl.ChatText(character.data, text, image, seconds);
            return true;
        }
    }
}
