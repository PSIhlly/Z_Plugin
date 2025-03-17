using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ui.Loading;
using Ui.ModSceneMain;
using UnityEngine;
using Z_DesignStyle;
using Z_Map;
using Z_Ui;
using Z_UnitSystem;

public class ModManager : Z_MonoManager<ModManager>
{

    private string _folderName;

    #region life

    private InternalModSceneController _sceneCtrl;
    public ExternalModSceneController sceneCtrl;
    public ModAssetCtrl assetCtrl;
    public override void Init()
    {
        base.Init();
        var __sceneCtrl = new ModSceneController(this);
        _sceneCtrl = __sceneCtrl;
        sceneCtrl = __sceneCtrl;

        assetCtrl = new ModAssetCtrl(this);
    }
    public void OnMouse(bool click, Vector3 pos, Vector3 dir)
    {
        _sceneCtrl.OnMouse(click,pos,dir);
    }
    public void Update()
    {
        _sceneCtrl.Update();
    }
    public void BeginStory(string storyName)
    {
        this._folderName = storyName;
    }
    public void EndStory()
    {

    }
    public async void BeginScene(string fileName)
    {
        UiManager.instance.ShowUi<UiLoadingCtrl>();
        MapData data;

        if (SaveAndLoad.Exist(fileName))
        {
            data = await Task.Run(() =>
            {
                return new MapData(SaveAndLoad.Load(fileName));
            });
        }
        else
        {
            //new
            data = await Task.Run(() =>
            {
                return new MapData();
            });
        }

        _sceneCtrl.Begin(data, fileName);

        Z_EventHelper.Invoke(new LoadingEvent()
        {
            state = LoadingState.Done
        });
        UiManager.instance.CloseUi<UiLoadingCtrl>();
        UiManager.instance.ShowUi<UiModSceneMainCtrl>();
    }

    public void EndScene()
    {
        _sceneCtrl.End();
    }

    #endregion
    public string GetSceneFileName()
    {
        return Application.persistentDataPath + "/" + _folderName + "/" + _sceneCtrl.folderName+"/scene";
    }
    public string GetSceneFolder()
    {
        return Application.persistentDataPath + "/" + _folderName + "/" + _sceneCtrl.folderName + "/";
    }
    public string GetStoryFolder()
    {
        return Application.persistentDataPath+"/"+_folderName+"/";
    }
}
