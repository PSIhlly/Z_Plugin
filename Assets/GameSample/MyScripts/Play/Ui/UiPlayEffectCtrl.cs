using UnityEngine;

namespace Ui.PlayEffect
{
    public partial class UiPlayEffectParam
    {
        public bool fadeOut;
        public float duration = 1f;
    }

    public partial class UiPlayEffectModel
    {
        public bool fadeOut;
        public bool fading;
        public double startedAt;
        public float duration;
        public bool waitingForFirstRender;
    }

    public partial class UiPlayEffectCtrl
    {
        public override void OnShow()
        {
            CancelFirstRenderWait();
            model.fadeOut = param?.fadeOut ?? false;
            float duration = param?.duration ?? 1f;
            model.duration = float.IsNaN(duration) || float.IsInfinity(duration) ? 0f : Mathf.Max(0f, duration);
            model.startedAt = Time.unscaledTimeAsDouble;
            model.fading = true;
            // This image is a solid-color overlay, never localized artwork.
            view.img_.cn = null;
            view.img_.en = null;
            view.img_.gameObject.SetActive(true);
            view.img_.enabled = true;
            view.img_.BindTexData(null);
            view.img_.overrideSprite = null;
            // CanvasRenderer modulation/culling may survive a previous hidden UI.
            // A standalone FadeOut must start with a drawable black graphic.
            view.img_.canvasRenderer.SetColor(Color.white);
            view.img_.canvasRenderer.cull = false;
            SetAlpha(model.fadeOut ? 1f : 0f);
            view.img_.SetAllDirty();
            if (model.duration <= 0f)
            {
                Complete();
                return;
            }

            // Loading can stall between Show and the first canvas submission.
            // Do not consume the fade before its initial black/clear frame exists.
            model.waitingForFirstRender = true;
            Canvas.willRenderCanvases += StartAfterFirstRender;
        }

        public override void OnUpdate()
        {
            if (!model.fading || model.waitingForFirstRender)
                return;

            float progress = Mathf.Clamp01((float)((Time.unscaledTimeAsDouble - model.startedAt) / model.duration));
            SetAlpha(model.fadeOut ? 1f - progress : progress);
            if (progress >= 1f)
                Complete();
        }

        public override void OnHide()
        {
            model.fading = false;
            CancelFirstRenderWait();
        }

        private void StartAfterFirstRender()
        {
            if (!active || !model.fading || !model.waitingForFirstRender)
            {
                CancelFirstRenderWait();
                return;
            }
            var canvas = view.img_.canvas;
            if (!view.img_.isActiveAndEnabled || canvas == null || !canvas.isActiveAndEnabled)
                return;
            // Never mutate graphics/layout inside a canvas rebuild callback.
            model.startedAt = Time.unscaledTimeAsDouble;
            CancelFirstRenderWait();
        }

        private void Complete()
        {
            SetAlpha(model.fadeOut ? 0f : 1f);
            model.fading = false;
            CancelFirstRenderWait();
            Close();
        }

        private void CancelFirstRenderWait()
        {
            Canvas.willRenderCanvases -= StartAfterFirstRender;
            if (model != null)
                model.waitingForFirstRender = false;
        }

        private void SetAlpha(float alpha)
        {
            view.img_.color = new Color(0f, 0f, 0f, alpha);
        }
    }
}
