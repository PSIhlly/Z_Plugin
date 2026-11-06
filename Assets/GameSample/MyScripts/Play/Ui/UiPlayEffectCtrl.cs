using UnityEngine;

namespace Ui.PlayEffect
{
    public partial class UiPlayEffectParam
    {
        public bool fadeOut;
    }

    public partial class UiPlayEffectModel
    {
        public bool fadeOut;
        public bool fading;
        public double startedAt;
    }

    public partial class UiPlayEffectCtrl
    {
        private const double FadeSeconds = 1.0;

        public override void OnShow()
        {
            model.fadeOut = param?.fadeOut ?? false;
            model.startedAt = Time.unscaledTimeAsDouble;
            model.fading = true;
            view.img_.BindTexData(null);
            view.img_.overrideSprite = null;
            view.img_.enabled = true;
            SetAlpha(model.fadeOut ? 1f : 0f);
        }

        public override void OnUpdate()
        {
            if (!model.fading)
                return;

            float progress = Mathf.Clamp01((float)((Time.unscaledTimeAsDouble - model.startedAt) / FadeSeconds));
            SetAlpha(model.fadeOut ? 1f - progress : progress);
            if (progress >= 1f)
            {
                model.fading = false;
                Close();
            }
        }

        public override void OnHide()
        {
            model.fading = false;
        }

        private void SetAlpha(float alpha)
        {
            view.img_.color = new Color(0f, 0f, 0f, alpha);
        }
    }
}
