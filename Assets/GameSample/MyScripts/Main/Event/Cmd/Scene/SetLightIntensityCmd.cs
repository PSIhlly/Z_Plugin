using UnityEngine;
using Z_Code.Form;

namespace Z_Code
{
    /// <summary>
    /// 设置全局灯光(CameraInstance.globalLight)强度，并把结果记录进 ProgressForm.lightIntensity，
    /// 使该设置随存档保留、切换场景后依然生效。<c>-1</c> 表示未记录，沿用相机视角的默认强度。
    /// </summary>
    public class SetLightIntensityCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Init()
        {
            Register(new SetLightIntensityCmd());
        }

        public override string GetName() => "SetLightIntensity";
        public override CmdBase GetNew() => new SetLightIntensityCmd();

        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            if (prm[0].str == string.Empty)
                return true;

            float intensity = prm[0].num;
            if (float.IsNaN(intensity) || float.IsInfinity(intensity))
                return true;

            intensity = Mathf.Max(0f, intensity);

            var cameraInstance = CameraInstance.instance;
            if (cameraInstance == null || cameraInstance.globalLight == null)
                return true;

            cameraInstance.globalLight.intensity = intensity;

            var gameManager = GameManager.instance;
            var progress = gameManager == null ? null : gameManager.curProgress;
            if (progress != null)
                progress.lightIntensity = intensity;

            return true;
        }
    }
}
