using System.Collections.Generic;
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
            var units = SceneUnitCommand.ResolveAll(prm[0].str);
            if (!units.Exists(SceneUnitCommand.CanMove)) return true;
            var vector = SceneUnitCommand.ReadVector(prm[1]);
            float argument = prm[2].num;
            if (float.IsNaN(argument) || float.IsInfinity(argument) || (!UsesDuration && argument <= 0))
                return true;
            var offsetOrPosition = Relative ? MapManager.instance.utilCtrl.MapPos2RealPos(vector)
                : SceneUnitCommand.WorldPosition(vector);
            var moves = new List<PendingMove>();
            foreach (var unit in units)
            {
                if (!SceneUnitCommand.CanMove(unit)) continue;
                var target = Relative ? unit.data.pos + offsetOrPosition : offsetOrPosition;
                float distance = MapManager.instance.utilCtrl.RealPos2MapPos(target - unit.data.pos).magnitude;
                if (distance <= .00001f) continue;
                moves.Add(new PendingMove
                {
                    unit = unit, data = unit.data, target = target,
                    speed = UsesDuration && argument > 0 ? distance / argument : argument,
                    remainingTime = UsesDuration ? argument : distance / argument
                });
            }
            if (moves.Count == 0) return true;
            if (UsesDuration && argument <= 0)
            {
                foreach (var move in moves)
                    if (SceneUnitCommand.IsLive(move.unit, move.data))
                        SceneUnitCommand.Move(move.unit, move.target - move.unit.data.pos);
                return true;
            }
            // Capture all original identities/targets before the first movement callback runs.
            GameManager.instance.evtCtrl.StartTask(asyncTask, () =>
            {
                bool complete = true;
                float delta = MapUtilController.MovementDeltaTime;
                foreach (var move in moves)
                {
                    if (move.finished) continue;
                    move.finished = !SceneUnitCommand.IsLive(move.unit, move.data)
                        || AdvanceMove(move.unit, move.target, move.speed, ref move.remainingTime, delta);
                    if (!move.finished) complete = false;
                }
                if (complete)
                {
                    // A blocked move ends after its allotted travel time, as before.
                    asyncTask.Complete();
                    return true;
                }
                return false;
            });
            return false;
        }

        private sealed class PendingMove
        {
            public MapUnit unit;
            public Z_UnitSystem.Form.UnitForm.Data data;
            public Vector3 target;
            public float speed;
            public float remainingTime;
            public bool finished;
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
