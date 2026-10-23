using UnityEngine;
using Z_Code.Form;
using Z_Map;

namespace Z_Code
{
    public class GetPositionCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new GetPositionCmd());
            RegisterAlias("GetCharacterPosition", new GetPositionCmd());
            RegisterAlias("GetObjectPosition", new GetPositionCmd());
            Register(new GetPositionRelativeCmd());
            RegisterAlias("GetCharacterPositionRelative", new GetPositionRelativeCmd());
            RegisterAlias("GetObjectPositionRelative", new GetPositionRelativeCmd());
            Register(new SetPositionCmd());
            RegisterAlias("SetCharacterPosition", new SetPositionCmd());
            RegisterAlias("SetObjectPosition", new SetPositionCmd());
            Register(new SetPositionRelativeCmd());
            RegisterAlias("SetCharacterPositionRelative", new SetPositionRelativeCmd());
            RegisterAlias("SetObjectPositionRelative", new SetPositionRelativeCmd());
        }

        public override string GetName() => "GetPosition";
        public override CmdBase GetNew() => new GetPositionCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            var unit = SceneUnitCommand.Resolve(prm[0].str);
            var position = unit == null ? Vector3.zero : GameManager.MapPosToPlayerPos(
                MapManager.instance.utilCtrl.RealPos2MapPos(unit.data.pos));
            asyncTask.res = new[] { SceneUnitCommand.BoxVector(position) };
            return true;
        }
    }

    public class GetPositionRelativeCmd : CmdBase
    {
        public override string GetName() => "GetPositionRelative";
        public override CmdBase GetNew() => new GetPositionRelativeCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            var target = SceneUnitCommand.Resolve(prm[0].str);
            var origin = SceneUnitCommand.Resolve(prm[1].str);
            var offset = target == null || origin == null ? Vector3.zero :
                MapManager.instance.utilCtrl.RealPos2MapPos(target.data.pos - origin.data.pos);
            asyncTask.res = new[] { SceneUnitCommand.BoxVector(offset) };
            return true;
        }
    }

    public class SetPositionCmd : CmdBase
    {
        protected virtual bool Relative => false;
        public override string GetName() => "SetPosition";
        public override CmdBase GetNew() => new SetPositionCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            var units = SceneUnitCommand.ResolveAll(prm[0].str);
            if (!units.Exists(SceneUnitCommand.CanMove)) return true;
            var vector = SceneUnitCommand.ReadVector(prm[1]);
            var offsetOrPosition = Relative ? MapManager.instance.utilCtrl.MapPos2RealPos(vector)
                : SceneUnitCommand.WorldPosition(vector);
            var data = new Z_UnitSystem.Form.UnitForm.Data[units.Count];
            var positions = new Vector3[units.Count];
            for (int i = 0; i < units.Count; i++)
            {
                data[i] = units[i].data;
                positions[i] = Relative ? units[i].data.pos + offsetOrPosition : offsetOrPosition;
            }
            for (int i = 0; i < units.Count; i++)
                if (SceneUnitCommand.CanMove(units[i]) && SceneUnitCommand.IsLive(units[i], data[i]))
                    MapManager.instance.updateCtrl.ApplyMove(units[i], positions[i], units[i].data.euler, teleport: true);
            return true;
        }
    }

    public class SetPositionRelativeCmd : SetPositionCmd
    {
        protected override bool Relative => true;
        public override string GetName() => "SetPositionRelative";
        public override CmdBase GetNew() => new SetPositionRelativeCmd();
    }
}
