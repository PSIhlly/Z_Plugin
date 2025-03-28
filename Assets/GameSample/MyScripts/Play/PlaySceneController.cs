using Form;
using Microsoft.Win32;
using System.Collections;
using System.Collections.Generic;
using Ui;
using Ui.ModSceneUnit;
using Ui.ModStory;
using UnityEditor;
using UnityEngine;
using Z_Debug;
using Z_DesignStyle;
using Z_Input;
using Z_Map;
using Z_Map.Form;
using Z_Time;
using Z_Ui;
using Z_UnitSystem;

public interface InternalPlaySceneController
{
    public string folderName { get; }
    public void Begin(MapData dataCtrl, string fileName);
    public void End();
    public void Update();
    public void OnMouse(bool click, Vector3 pos, Vector3 dir);
}
public interface ExternalPlaySceneController
{
    public void SetCamera(float x, float y, float z);

    public void ForceUpdate();
}
public class PlaySceneController : Z_Controller<PlayManager>, InternalPlaySceneController, ExternalPlaySceneController
{
    public PlaySceneController(PlayManager super) : base(super)
    {

    }

    bool enable = false;
    bool waitForActive = false;
    #region internal Var
    private string _folderName;
    public string folderName { get => _folderName; }
    #endregion

    #region extern Var


    #endregion


    public void Begin(MapData dataCtrl, string fileName)
    {
        this._folderName = fileName;
        GameManager.instance.RegisterInputByUgc();
        MapManager.instance.Begin(dataCtrl);
        CameraInstance.instance.Register(Vector3.zero, Z_Math.Graph.ElementwiseMultiply(MapManager.instance.sizeLimit, MapManager.instance.data.mainData.mapUnitSize), 5, 15);
        CameraInstance.instance.tarTrs.position = Z_Math.Graph.ElementwiseMultiply(new Vector3(500, 500, 500), MapManager.instance.data.mainData.mapUnitSize);
        enable = true;
        waitForActive = false;
    }
    public void End()
    {
        enable = false;
        MapManager.instance.End();
        UiManager.instance.CloseAll();
        GameManager.instance.RegisterInputDefault();
        UiManager.instance.ShowUi<UiModStoryCtrl>();

    }
    public T TryGetUnit<T>(RaycastHit[] hits)
    {
        foreach (var hit in hits)
        {
            var tmp = hit.transform.parent.GetComponent<Instance>();
            if (tmp != null && tmp.unit.isVising)
            {
                return (T)(object)tmp.unit;
            }
        }
        return default(T);
    }
    public void OnMouse(bool click, Vector3 pos, Vector3 dir)
    {

        if (waitForActive || !enable)
            return;
        // set z
        pos.z = CameraInstance.instance.cam.nearClipPlane;
        // to world
        Vector3 worldPosition = CameraInstance.instance.cam.ScreenToWorldPoint(pos);
        worldPosition.y = CameraInstance.instance.tarTrs.position.y;
        var hits = Physics.RaycastAll(worldPosition + Vector3.up * 100, Vector3.down);
        var hitPos = MapManager.instance.utilCtrl.RealPos2MapPos(worldPosition);
       
    }
    public void ForceUpdate()
    {

        MapManager.instance.ResetInfo();
        if (waitForActive)
            return;

        waitForActive = true;
        TimeManager.instance.AddNextBigFrameAction(() =>
        {
            waitForActive = false;
        }, _super.gameObject);
    }
    public void Update()
    {
        if (!enable)
            return;

        {
            MapManager.instance.UpdateInfo();
            {
                MapManager.instance.SetPos(CameraInstance.instance.tarTrs.position);
            }
        }


    }
    public void SetCamera(float x, float y, float z)
    {
        CameraInstance.instance.tarTrs.position = new Vector3(x, y, z);
        Z_EventHelper.Invoke(new CameraMoveEvent());
    }


}
