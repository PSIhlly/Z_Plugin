using Form;
using Microsoft.Win32;
using System.Collections;
using System.Collections.Generic;
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
    public int Add(string texName, Vector2 size);
    public void Remove(int id);
    public void SetPos(int id, Vector2 tar, float time);
    public void SetEuler(int id, float tar, float time);
    public void SetOpacity(int id, float tar, float time);
    public void SetRemoveTime(int id, float time);
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

    public int Add(string texName,Vector2 size)
    {
        var data = new ImageUiItemForm.Data(-1, texName,size, Vector2.one * 0.5f, 0, 0, Vector2.one * 0.5f, 0, 0, 0, 0, 0, 0, 0, 0,999999);
        return ImageUiItemForm.AddData(data);
    }
    public void Remove(int id)
    {
        ImageUiItemForm.RemoveData(id);
    }
    public void SetPos(int id, Vector2 tar, float time)
    {
        var data = ImageUiItemForm.DataByUid.GetDv(id, null);
        if (data != null)
        {
            data.tarPos = tar;
            data.posTime = time;
            data.posProgress = 0;
            data.oldPos = data.ctrl == null ?Vector2.one*0.5f: data.ctrl.rect.position;
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
            data.oldEuler = data.ctrl == null ? 0: data.ctrl.rect.eulerAngles.z;
        }
    }
    public void SetOpacity(int id, float tar, float time)
    {
        var data = ImageUiItemForm.DataByUid.GetDv(id, null);
        if (data != null)
        {
            data.tarOpacity = tar;
            data.opacityTime = time;
            data.opacityProgress = 0;
            data.oldOpacity = data.ctrl == null ? 1 : data.ctrl.view.img_image.color.a;
        }
    }
    public void SetRemoveTime(int id, float time)
    {
        var data = ImageUiItemForm.DataByUid.GetDv(id, null);
        if (data != null)
        {
            data.removeTime = time;
        }
    }
    
}
