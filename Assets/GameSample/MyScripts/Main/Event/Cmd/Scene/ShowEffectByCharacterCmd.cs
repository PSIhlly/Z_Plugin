using UnityEngine;
using Z_Code.Form;
using Z_Map;

namespace Z_Code
{
    public class ShowEffectByCharacterCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new ShowEffectByCharacterCmd());
        }
        public override string GetName() => "ShowEffectByCharacter";
        public override CmdBase GetNew() => new ShowEffectByCharacterCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            var units = SceneUnitCommand.ResolveAll(prm[1].str);
            bool ignoreRot = prm[2].num == 1;
            foreach (var unit in units)
            {
                if (!(unit is CharacterUnit) || !SceneUnitCommand.IsLive(unit)) continue;
                var data = unit.data;
                var delta = ShowEffectCmd.Delta;
                PlayManager.instance.effectCtrl.CreatEffect(GlobalEventHelper.GetId(prm[0].str, GlobalEventHelper.EFFECT), data.pos + delta, ignoreRot ? 0 : data.euler.y, (img) =>
                {
                    if(SceneUnitCommand.IsLive(unit, data) && unit.ins != null)
                    {
                        img.oriPos = unit.ins.transform.position + delta;
                        if (!ignoreRot)
                        {
                            img.oriRot = data.euler.y;
                        }
                    }

                });
                ShowEffectCmd.Nxt();
            }

            return true;
        }
    }
}
