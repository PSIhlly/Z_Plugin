using UnityEngine;
using Z_Code.Form;
using Z_Map;
using Z_Time;
using Z_UnitSystem.Form;

namespace Z_Code
{
    public class MoveCameraCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Init() => Register(new MoveCameraCmd());

        public override string GetName() => "MoveCamera";
        public override CmdBase GetNew() => new MoveCameraCmd();
        protected virtual bool relative => true;

        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            var map = MapManager.instance.data;
            var scene = GameManager.instance.curScene;
            if (map == null || scene == null)
                return true;
            var vector = SceneUnitCommand.ReadVector(prm[0]);
            float duration = prm[1].num;
            if (!IsFinite(vector) || !IsFinite(duration))
                return true;

            var camera = CameraInstance.instance;
            if (camera == null || !camera.isActiveAndEnabled || camera.cam == null)
                return true;
            var start = camera.ClampPosition(camera.tarTrs.position);
            // Only absolute coordinates carry the player-facing origin offset.
            var destination = relative
                ? start + vector
                : GameManager.PlayerPosToMapPos(vector);
            if (!IsFinite(destination))
                return true;
            var target = camera.ClampPosition(destination);
            uint version = camera.BeginScriptedMove();
            camera.SetScriptedPosition(version, start);
            if (duration <= 0f)
            {
                camera.SetScriptedPosition(version, target);
                return true;
            }

            float elapsed = 0f;
            int lastUpdateFrame = -1;
            GameManager.instance.evtCtrl.StartTask(asyncTask, () =>
            {
                if (camera == null || !camera.HasScriptedControl(version)
                    || !ReferenceEquals(scene, GameManager.instance.curScene)
                    || !ReferenceEquals(map, MapManager.instance.data))
                {
                    asyncTask.Complete();
                    return true;
                }
                // StartTask polls immediately and the scheduler can poll again
                // in the same frame. Consume that frame's bounded delta once.
                if (lastUpdateFrame == Time.frameCount)
                    return false;
                lastUpdateFrame = Time.frameCount;
                if (!AdvanceMove(camera, version, start, target, duration, ref elapsed,
                        TimeManager.GetDeltaTime()))
                    return false;
                asyncTask.Complete();
                return true;
            });
            return false;
        }

        internal static bool AdvanceMove(CameraInstance camera, uint version, Vector3 start,
            Vector3 target, float duration, ref float elapsed, float deltaTime)
        {
            if (camera == null || !camera.HasScriptedControl(version))
                return true;
            elapsed = Mathf.Min(duration, elapsed + TimeManager.ClampDeltaTime(deltaTime));
            bool complete = elapsed >= duration;
            camera.SetScriptedPosition(version, complete ? target : Vector3.Lerp(start, target, elapsed / duration));
            return complete;
        }

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        private static bool IsFinite(Vector3 value) => IsFinite(value.x) && IsFinite(value.y) && IsFinite(value.z);
    }

    public class MoveCameraToCmd : MoveCameraCmd
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Init() => Register(new MoveCameraToCmd());

        public override string GetName() => "MoveCameraTo";
        public override CmdBase GetNew() => new MoveCameraToCmd();
        protected override bool relative => false;
    }

    public class RestoreCameraCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Init() => Register(new RestoreCameraCmd());

        public override string GetName() => "RestoreCamera";
        public override CmdBase GetNew() => new RestoreCameraCmd();

        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            var camera = CameraInstance.instance;
            camera.RestorePlayerFollow();
            var scene = PlayManager.instance.sceneCtrl;
            var player = scene?.playerM;
            // Resolve the current player now, not the player present when a pan
            // began. A missing player leaves the camera still until one exists.
            if (player != null && UnitForm.DataByUid.TryGetValue(player.uid, out var registered)
                && ReferenceEquals(registered, player))
                scene.SetCamera(player.pos.x, player.pos.y, player.pos.z);
            return true;
        }
    }
}
