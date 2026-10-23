using System.Collections.Generic;
using UnityEngine;
using Z_Code.Form;
using Z_Map;

namespace Z_Code
{
    public class SetRotationCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new SetRotationCmd());
            RegisterAlias("SetCharacterRotation", new SetRotationCmd());
            RegisterAlias("SetObjectRotation", new SetRotationCmd());
        }
        public override string GetName() => "SetRotation";
        public override CmdBase GetNew() => new SetRotationCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            var units = SceneUnitCommand.ResolveAll(prm[0].str);
            var rotations = new List<PendingRotation>();
            foreach (var unit in units)
                if (SceneUnitCommand.CanMove(unit))
                    rotations.Add(new PendingRotation { unit = unit, data = unit.data, startYaw = unit.data.euler.y });
            if (rotations.Count == 0) return true;
            float angle = prm[1].num;
            float duration = prm[2].num;
            if (float.IsNaN(angle) || float.IsInfinity(angle)
                || float.IsNaN(duration) || float.IsInfinity(duration))
                return true;
            float targetYaw = Mathf.Repeat(angle, 360f);
            if (duration <= 0)
            {
                foreach (var rotation in rotations)
                    if (SceneUnitCommand.IsLive(rotation.unit, rotation.data))
                        SetYaw(rotation.unit, targetYaw);
                return true;
            }

            // Each target owns its original identity/start angle for the whole transition.
            GameManager.instance.evtCtrl.StartTask(asyncTask, () =>
            {
                bool complete = true;
                float delta = MapUtilController.MovementDeltaTime;
                foreach (var rotation in rotations)
                {
                    if (rotation.finished) continue;
                    rotation.finished = !SceneUnitCommand.IsLive(rotation.unit, rotation.data)
                        || AdvanceRotation(rotation.unit, rotation.startYaw, targetYaw, duration,
                            ref rotation.elapsed, delta);
                    if (!rotation.finished) complete = false;
                }
                if (complete)
                {
                    asyncTask.Complete();
                    return true;
                }
                return false;
            });

            return false;
        }

        private sealed class PendingRotation
        {
            public MapUnit unit;
            public Z_UnitSystem.Form.UnitForm.Data data;
            public float startYaw;
            public float elapsed;
            public bool finished;
        }

        internal static bool AdvanceRotation(MapUnit unit, float startYaw, float targetYaw,
            float duration, ref float elapsed, float deltaTime)
        {
            elapsed = Mathf.Min(duration, elapsed + MapUtilController.ClampMovementDeltaTime(deltaTime));
            bool finished = elapsed >= duration;
            SetYaw(unit, finished ? targetYaw : Mathf.LerpAngle(startYaw, targetYaw, elapsed / duration));
            return finished;
        }

        private static void SetYaw(MapUnit unit, float yaw)
        {
            var euler = unit.data.euler;
            euler.y = Mathf.Repeat(yaw, 360f);
            if (unit is CharacterUnit character)
            {
                character.forceEuler = euler;
                unit.data.euler = euler;
                if (unit.ins != null)
                    unit.ins.transform.eulerAngles = euler;
                MapManager.instance.updateCtrl.RefreshCharacterOverlap(character);
            }
            else if (unit is ObjectUnit obj)
            {
                MapManager.instance.updateCtrl.ApplyMove(obj, obj.data.pos, euler, teleport: true);
                Z_EventHelper.Invoke(new ObjectEvent { type = MapEventType.Move, unit = obj });
            }
        }
    }

    // Source compatibility only; registration and metadata use the shared Scene command.
    public class SetObjectRotationCmd : SetRotationCmd { }
}
