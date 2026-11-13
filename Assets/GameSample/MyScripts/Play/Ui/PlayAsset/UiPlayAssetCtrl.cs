using Form;
using System.Linq;
using UnityEngine;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Math;
using Z_Time;
using Z_Ui.Base;



namespace Ui.PlayAsset
{

    public partial class UiPlayAssetParam
    {

    }
    public partial class UiPlayAssetModel
    {
        public int visitId;
    }
    public partial class UiPlayAssetCtrl : IZ_Listener<PlayAssetEvent>
    {

        UiContainer<UiImageCtrl> imageCon;
        public override void OnCreate()
        {
            imageCon = new UiContainer<UiImageCtrl>(this, view.go_image);
        }


        public override void OnShow()
        {
            Z_EventHelper.Register(this);
            Refresh();
        }
        public override void OnHide()
        {
            Z_EventHelper.Unregister(this);
        }
        public void Refresh()
        {
            if (imageCon == null)
                return;
            imageCon.Clear();
            foreach (var data in ImageUiItemForm.DataByUid.Values.OrderBy(data => data.uid))
            {
                imageCon.Add(new UiImageParam()
                {
                    data = data
                });
            }
            imageCon.Refresh();

        }
        public void OnEvent(PlayAssetEvent evt)
        {
            if (active)
                Refresh();
        }

    }

    public partial class UiImageParam
    {
        public ImageUiItemForm.Data data;
    }
    public partial class UiImageModel
    {
        public UiImageParam prm;
    }
    public partial class UiImageCtrl
    {
        private float baseTextFontSize;
        private bool textFontSizeCaptured;

        public float CurrentOpacity => TryGetData(out var data)
            ? data.isText ? view.txt_.color.a : view.img_.color.a : 1;

        public override void OnCreate()
        {
            CaptureTextFontSize();
        }
        public override void OnShow()
        {
            ReleaseData();
            if (param?.data == null || !ImageUiItemForm.DataByUid.TryGetValue(param.data.uid, out var registered)
                || !ReferenceEquals(registered, param.data))
            {
                ClearGraphics();
                if (gameObject != null)
                    gameObject.SetActive(false);
                return;
            }
            model.prm = param;
            model.prm.data.ctrl = this;
            CaptureTextFontSize();
            var data = model.prm.data;
            if (data.isText)
            {
                view.img_.BindTexDataOrHide(null);
                float scale = float.IsNaN(data.textScale) || float.IsInfinity(data.textScale)
                    ? 0 : Mathf.Max(0, data.textScale);
                view.txt_.fontSize = baseTextFontSize * scale;
                view.txt_.text = data.text ?? string.Empty;
                view.txt_.enabled = true;
                rect.sizeDelta = string.IsNullOrEmpty(data.text) || scale == 0 ? Vector2.zero
                    : view.txt_.GetPreferredValues(view.txt_.text, float.PositiveInfinity, float.PositiveInfinity);
            }
            else
            {
                view.txt_.text = string.Empty;
                view.txt_.enabled = false;
                rect.sizeDelta = data.size;
                if (!TexAssetForm.DataById.TryGetValue(data.tex, out var texture) || texture == null)
                    TexAssetForm.DataById.TryGetValue(GlobalDefaultHelper.DefaultEventTexId, out texture);
                view.img_.BindTexDataOrHide(texture);
            }

            Refresh();
        }
        public override void OnHide()
        {
            ReleaseData();
            ClearGraphics();
        }
        public override void OnUpdate()
        {
            if (!TryGetData(out var data))
                return;
            float deltaTime = TimeManager.GetDeltaTime();
            data.posProgress += deltaTime;
            data.eulerProgress += deltaTime;
            data.opacityProgress += deltaTime;
            if (data.removeTime < int.MaxValue)
            {
                data.removeTime -= deltaTime;
                if (data.removeTime <= 0)
                {
                    // Removing refreshes/reuses the shared container synchronously.
                    PlayManager.instance.assetCtrl?.Remove(data.uid);
                    return;
                }
            }
            Refresh();
        }
        public void Refresh()
        {
            if (!TryGetData(out var data))
                return;

            if (data.posTime <= 0)
            {
                rect.position = data.tarPos;
            }
            else
            {
                rect.position = Vector3.Lerp(data.oldPos, data.tarPos, data.posProgress / data.posTime);
            }
            if (data.eulerTime <= 0)
            {
                rect.eulerAngles = rect.eulerAngles.NewSetZ(data.tarEuler);
            }
            else
            {
                rect.eulerAngles = rect.eulerAngles.NewSetZ(Mathf.Lerp(data.oldEuler, data.tarEuler, data.eulerProgress / data.eulerTime));
            }

            float opacity = data.opacityTime <= 0 ? data.tarOpacity
                : Mathf.Lerp(data.oldOpacity, data.tarOpacity, data.opacityProgress / data.opacityTime);
            if (data.isText)
                view.txt_.color = view.txt_.color.NewSetA(opacity);
            else
                view.img_.color = view.img_.color.NewSetA(opacity);
        }

        private bool TryGetData(out ImageUiItemForm.Data data)
        {
            data = model?.prm?.data;
            return data != null && ReferenceEquals(data.ctrl, this)
                && ImageUiItemForm.DataByUid.TryGetValue(data.uid, out var registered)
                && ReferenceEquals(data, registered);
        }

        private void ReleaseData()
        {
            var data = model?.prm?.data;
            if (data != null && ReferenceEquals(data.ctrl, this))
                data.ctrl = null;
            if (model != null)
                model.prm = null;
        }

        private void ClearGraphics()
        {
            if (view?.img_ != null)
                view.img_.BindTexDataOrHide(null);
            if (view?.txt_ != null)
            {
                view.txt_.text = string.Empty;
                view.txt_.enabled = false;
            }
        }

        private void CaptureTextFontSize()
        {
            if (!textFontSizeCaptured && view?.txt_ != null)
            {
                baseTextFontSize = view.txt_.fontSize;
                textFontSizeCaptured = true;
            }
        }
    }


}
