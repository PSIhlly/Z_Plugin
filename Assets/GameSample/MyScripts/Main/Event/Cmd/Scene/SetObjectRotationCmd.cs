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
            var unit = SceneUnitCommand.Resolve(prm[0].str);
            if (!SceneUnitCommand.CanMove(unit))
                return true;
            float angle = prm[1].num;
            float duration = prm[2].num;
            if (float.IsNaN(angle) || float.IsInfinity(angle)
                || float.IsNaN(duration) || float.IsInfinity(duration))
                return true;
            float targetYaw = Mathf.Repeat(angle, 360f);
            if (duration <= 0)
            {
                SetYaw(unit, targetYaw);
                return true;
            }

            float startYaw = unit.data.euler.y;
            float elapsed = 0;
            // Capture the live instance and scalar arguments, not a reusable UID/shared heap.
            GameManager.instance.evtCtrl.StartTask(asyncTask, () =>
            {
                if (!SceneUnitCommand.IsLive(unit))
                {
                    asyncTask.Complete();
                    return true;
                }
                if (AdvanceRotation(unit, startYaw, targetYaw, duration, ref elapsed,
                    MapUtilController.MovementDeltaTime))
                {
                    asyncTask.Complete();
                    return true;
                }
                return false;
            });

            return false;
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
