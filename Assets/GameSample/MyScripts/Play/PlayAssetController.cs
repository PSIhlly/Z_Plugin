using Form;
using Ui.PlayAsset;
using UnityEngine;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Math;
using Z_Ui;

namespace Form
{
    public partial class ImageUiItemForm
    {
        public partial class Data
        {
            public UiImageCtrl ctrl;
        }
    }
}


public interface InternalPlayAssetController
{
    public void Begin();
    public void End();
}
public interface ExternalPlayAssetController
{
    public int Add(int texId, float scale);
    public int AddText(string text, float scale);
    public void Remove(int id);
    public void SetPos(int id, Vector2 tar, float time);
    public void SetEuler(int id, float tar, float time);
    public void SetOpacity(int id, float tar, float time);
    public void SetRemoveTime(int id, float time);
}
public class PlayAssetEvent : Z_Event
{

}
public class PlayAssetController : Z_Controller<PlayManager>, InternalPlayAssetController, ExternalPlayAssetController
{
    public PlayAssetController(PlayManager super) : base(super)
    {
    }


    #region internal Var

    #endregion

    #region extern Var

    #endregion

    public void Begin()
    {
        UiManager.instance.ShowUi<UiPlayAssetCtrl>();
    }
    public void End()
    {
        UiManager.instance.CloseUi<UiPlayAssetCtrl>();
    }

    public int Add(int texId, float scale)
    {
        var texData = TexAssetForm.DataById.GetDv(texId, null);
        Vector2 size = Vector2.zero;
        if (texData != null)
        {
            var tex = texData.GetTex();
            if (tex != null)
                size = new Vector2(tex.width * scale, tex.height * scale);
        }
        var data = CreateData();
        data.tex = texId;
        data.size = size;
        return AddData(data);
    }
    public int AddText(string text, float scale)
    {
        var data = CreateData();
        data.isText = true;
        data.text = text ?? string.Empty;
        data.textScale = float.IsNaN(scale) || float.IsInfinity(scale) ? 0 : Mathf.Max(0, scale);
        return AddData(data);
    }
    private static ImageUiItemForm.Data CreateData()
    {
        var data = ImageUiItemForm.defaultData;
        data.uid = -1;
        data.oldPos = data.tarPos = Vector2.one * 0.5f;
        data.oldOpacity = data.tarOpacity = 1;
        return data;
    }
    private static int AddData(ImageUiItemForm.Data data)
    {
        var id = ImageUiItemForm.AddData(data);
        if (id > 0)
            Z_EventHelper.Invoke(new PlayAssetEvent());
        return id;
    }
    public void Remove(int id)
    {
        var data = ImageUiItemForm.DataByUid.GetDv(id, null);
        if (data != null)
        {
            data.ctrl = null;
            ImageUiItemForm.RemoveData(id);
        }
        Z_EventHelper.Invoke(new PlayAssetEvent());
    }
    public void SetPos(int id, Vector2 normalizedTar, float time)
    {
        var data = ImageUiItemForm.DataByUid.GetDv(id, null);
        if (data == null)
            return;
        var rect = UiManager.instance.GetUi<UiPlayAssetCtrl>()?.rect;
        if (rect != null)
        {
            var oldPos = data.ctrl == null ? Vector2.Lerp(data.oldPos, data.tarPos,
                data.posTime <= 0 ? 1 : data.posProgress / data.posTime) : (Vector2)data.ctrl.rect.position;
            data.tarPos = Graph.GetRealPos(normalizedTar, rect);
            data.posTime = Duration(time);
            data.posProgress = 0;
            data.oldPos = data.posTime == 0 ? data.tarPos : oldPos;

            Z_EventHelper.Invoke(new PlayAssetEvent());
        }
    }
    public void SetEuler(int id, float tar, float time)
    {
        var data = ImageUiItemForm.DataByUid.GetDv(id, null);
        if (data != null)
        {
            float oldEuler = data.ctrl == null ? Mathf.Lerp(data.oldEuler, data.tarEuler,
                data.eulerTime <= 0 ? 1 : data.eulerProgress / data.eulerTime) : data.ctrl.rect.eulerAngles.z;
            data.tarEuler = tar;
            data.eulerTime = Duration(time);
            data.eulerProgress = 0;
            data.oldEuler = oldEuler;
        }
        Z_EventHelper.Invoke(new PlayAssetEvent());
    }
    public void SetOpacity(int id, float tar, float time)
    {
        var data = ImageUiItemForm.DataByUid.GetDv(id, null);
        if (data != null)
        {
            float oldOpacity = data.ctrl == null ? Mathf.Lerp(data.oldOpacity, data.tarOpacity,
                data.opacityTime <= 0 ? 1 : data.opacityProgress / data.opacityTime) : data.ctrl.CurrentOpacity;
            data.tarOpacity = tar;
            data.opacityTime = Duration(time);
            data.opacityProgress = 0;
            data.oldOpacity = oldOpacity;
        }
        Z_EventHelper.Invoke(new PlayAssetEvent());
    }
    public void SetRemoveTime(int id, float time)
    {
        var data = ImageUiItemForm.DataByUid.GetDv(id, null);
        if (data != null)
        {
            data.removeTime = float.IsNaN(time) ? 0 : time;
        }
        Z_EventHelper.Invoke(new PlayAssetEvent());
    }
    private static float Duration(float time) => float.IsNaN(time) ? 0 : Mathf.Max(0, time);
}
