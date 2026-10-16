using UnityEngine;
using Z_Code.Form;
using Z_Map;

namespace Z_Code
{
    public class MoveCmd : CmdBase
    {
        protected virtual bool Relative => false;
        protected virtual bool UsesDuration => false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new MoveCmd());
            Register(new MoveRelativeCmd());
            // Stored events keep the former duration argument; the new names use speed.
            RegisterAlias("MoveCharacter", new MoveCharacterCmd());
            RegisterAlias("MoveObject", new MoveObjectCmd());
            RegisterAlias("MoveCharacterRelative", new MoveCharacterRelativeCmd());
            RegisterAlias("MoveObjectRelative", new MoveObjectRelativeCmd());
        }

        public override string GetName() => "Move";
        public override CmdBase GetNew() => new MoveCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            var unit = SceneUnitCommand.Resolve(prm[0].str);
            if (!SceneUnitCommand.CanMove(unit)) return true;
            var vector = SceneUnitCommand.ReadVector(prm[1]);
            var target = Relative ? unit.data.pos + MapManager.instance.utilCtrl.MapPos2RealPos(vector)
                : SceneUnitCommand.WorldPosition(vector);
            float distance = MapManager.instance.utilCtrl.RealPos2MapPos(target - unit.data.pos).magnitude;
            float argument = prm[2].num;
            if (float.IsNaN(argument) || float.IsInfinity(argument) || distance <= .00001f)
                return true;
            if (UsesDuration && argument <= 0)
            {
                SceneUnitCommand.Move(unit, target - unit.data.pos);
                return true;
            }
            if (argument <= 0) return true;
            float speed = UsesDuration ? distance / argument : argument;
            float remainingTime = UsesDuration ? argument : distance / speed;
            // Capture this instance, not a reusable UID or the interpreter's shared heapTemp.
            GameManager.instance.evtCtrl.StartTask(asyncTask, () =>
            {
                if (!SceneUnitCommand.IsLive(unit))
                {
                    asyncTask.Complete();
                    return true;
                }
                if (AdvanceMove(unit, target, speed, ref remainingTime, MapUtilController.MovementDeltaTime))
                {
                    // A blocked move ends after its allotted travel time, as before.
                    asyncTask.Complete();
                    return true;
                }
                return false;
            });
            return false;
        }

        internal static bool AdvanceMove(MapUnit unit, Vector3 target, float speed, ref float remainingTime, float deltaTime)
        {
            float delta = Mathf.Min(MapUtilController.ClampMovementDeltaTime(deltaTime), remainingTime);
            var offset = MapManager.instance.utilCtrl.RealPos2MapPos(target - unit.data.pos);
            var step = Vector3.ClampMagnitude(offset, speed * delta);
            SceneUnitCommand.Move(unit, MapManager.instance.utilCtrl.MapPos2RealPos(step));
            remainingTime -= delta;
            return remainingTime <= .000001f || MapManager.instance.utilCtrl.RealPos2MapPos(
                target - unit.data.pos).sqrMagnitude <= .0000000001f;
        }
    }

    public class MoveRelativeCmd : MoveCmd
    {
        protected override bool Relative => true;
        public override string GetName() => "MoveRelative";
        public override CmdBase GetNew() => new MoveRelativeCmd();
    }
}
