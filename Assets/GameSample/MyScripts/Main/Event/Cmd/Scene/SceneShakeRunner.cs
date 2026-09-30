using Ui.DialogBg;
using UnityEngine;
using Z_Ui;

public class SceneShakeRunner : MonoBehaviour
{
    private const float CameraAmplitude = 0.08f;
    private const float DialogAmplitude = 10f;

    private bool isRunning;
    private float duration;
    private object sceneOwner;
    private Transform cameraTransform;
    private Vector3 cameraPosition;
    private RectTransform dialogRect;
    private Vector2 dialogPosition;
    private float elapsed;

    public void Begin(float seconds)
    {
        Finish();

        duration = seconds;
        sceneOwner = GameManager.instance.curScene;
        cameraTransform = GetComponent<CameraInstance>().cam.transform;
        cameraPosition = cameraTransform.localPosition;

        var dialog = UiManager.instance.GetUi<UiDialogBgCtrl>();
        if (dialog != null && dialog.active)
        {
            dialogRect = dialog.rect;
            if (dialogRect != null)
                dialogPosition = dialogRect.anchoredPosition;
        }

        elapsed = 0f;
        isRunning = true;
    }

    private void Update()
    {
        if (!isRunning)
            return;

        if (!ReferenceEquals(sceneOwner, GameManager.instance.curScene))
        {
            Finish();
            return;
        }

        elapsed = Mathf.Min(duration, elapsed + Time.deltaTime);
        float phase = elapsed / duration * 4f * Mathf.PI;
        float offset = Mathf.Sin(phase);
        if (cameraTransform != null)
            cameraTransform.localPosition = cameraPosition + Vector3.right * (offset * CameraAmplitude);
        if (dialogRect != null)
            dialogRect.anchoredPosition = dialogPosition + Vector2.right * (offset * DialogAmplitude);

        if (elapsed >= duration)
            Finish();
    }

    private void OnDisable()
    {
        Finish();
    }

    private void Finish()
    {
        if (cameraTransform != null)
            cameraTransform.localPosition = cameraPosition;
        if (dialogRect != null)
            dialogRect.anchoredPosition = dialogPosition;

        isRunning = false;
        sceneOwner = null;
        cameraTransform = null;
        dialogRect = null;
    }
}
