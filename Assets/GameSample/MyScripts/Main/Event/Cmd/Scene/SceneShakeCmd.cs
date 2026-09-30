using UnityEngine;
using Z_Code.Form;

namespace Z_Code
{
    public class SceneShakeCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Init()
        {
            Register(new SceneShakeCmd());
        }

        public override string GetName() => "SceneShake";
        public override CmdBase GetNew() => new SceneShakeCmd();

        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            float duration = prm[0].str == string.Empty ? 1f : prm[0].num;
            if (duration <= 0f || float.IsNaN(duration) || float.IsInfinity(duration))
                return true;

            var cameraInstance = CameraInstance.instance;
            if (cameraInstance == null || cameraInstance.cam == null)
                return true;

            var runner = cameraInstance.GetComponent<SceneShakeRunner>();
            if (runner == null)
                runner = cameraInstance.gameObject.AddComponent<SceneShakeRunner>();

            runner.Begin(duration);
            return true;
        }
    }
}
