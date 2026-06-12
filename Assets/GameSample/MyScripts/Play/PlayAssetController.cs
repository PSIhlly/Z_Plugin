using Form;
using Microsoft.Win32;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using Ui;
using Ui.ModSceneUnit;
using Ui.PlayAsset;
using Ui.PlaySceneMain;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.Text;
using Z_DataSystem;
using Z_DataSystem.Form;
using Z_Debug;
using Z_DesignStyle;
using Z_Input;
using Z_Map;
using Z_Map.Form;
using Z_Math;
using Z_Time;
using Z_Ui;
using Z_UnitSystem;

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
            size = new Vector2(tex.width * scale, tex.height * scale);
        }
        var data = new ImageUiItemForm.Data(-1, texId, size, Vector2.one * 0.5f, 0, 0, Vector2.one * 0.5f, 0, 0, 0, 0, 0, 0, 0, 0, 999999);
        var id = ImageUiItemForm.AddData(data);
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
        var rect = UiManager.instance.GetUi<UiPlayAssetCtrl>().rect;
        if (data != null && rect != null)
        {

            data.tarPos = Graph.GetRealPos(normalizedTar, rect);
            data.posTime = time;
            data.posProgress = 0;
            if (data.ctrl == null)
            {
                if (time == 0)
                {
                    data.oldPos = data.tarPos;
                }
            }
            else
            {
                if (time == 0)
                {
                    data.ctrl.rect.position = data.tarPos;
                }
                data.oldPos = data.ctrl.rect.position;
            }

            Z_EventHelper.Invoke(new PlayAssetEvent());
        }
    }
    public void SetEuler(int id, float tar, float time)
    {
        var data = ImageUiItemForm.DataByUid.GetDv(id, null);
        if (data != null)
        {
            data.tarEuler = tar;
            data.eulerTime = time;
            data.eulerProgress = 0;
            data.oldEuler = data.ctrl == null ? 0 : data.ctrl.rect.eulerAngles.z;
        }
        Z_EventHelper.Invoke(new PlayAssetEvent());
    }
    public void SetOpacity(int id, float tar, float time)
    {
        var data = ImageUiItemForm.DataByUid.GetDv(id, null);
        if (data != null)
        {
            data.tarOpacity = tar;
            data.opacityTime = time;
            data.opacityProgress = 0;
            data.oldOpacity = data.ctrl == null ? 1 : data.ctrl.view.img_.color.a;
        }
        Z_EventHelper.Invoke(new PlayAssetEvent());
    }
    public void SetRemoveTime(int id, float time)
    {
        var data = ImageUiItemForm.DataByUid.GetDv(id, null);
        if (data != null)
        {
            data.removeTime = time;
        }
        Z_EventHelper.Invoke(new PlayAssetEvent());
    }





}
