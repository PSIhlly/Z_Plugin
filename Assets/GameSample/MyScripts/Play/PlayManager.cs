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

public class PlayManager : Z_MonoManager<PlayManager>
{
    public PlayData data;
    private string _folderName;

    #region life

    private InternalPlaySceneController _sceneCtrl;
    public ExternalPlaySceneController sceneCtrl;
    // public ModAssetCtrl assetCtrl;
    public override void Init()
    {
        base.Init();
        var __sceneCtrl = new PlaySceneController(this);
        _sceneCtrl = __sceneCtrl;
        sceneCtrl = __sceneCtrl;

        //  assetCtrl = new ModAssetCtrl(this);
    }
    public void OnMouse(bool click, Vector3 pos, Vector3 dir)
    {
        _sceneCtrl.OnMouse(click, pos, dir);
    }
    public void Update()
    {
        _sceneCtrl.Update();
    }
    public async void BeginStory(string storyName, bool ignoreSave)
    {
        this._folderName = storyName;
        PlayData data = null;
        if (!ignoreSave)
        {
            if (SaveAndLoad.Exist(GetStoryPlayDataFileName()))
            {
                data = await Task.Run(() =>
                {
                    return GameManager.instance.saveCtrl.LoadPlayData(GetStoryPlayDataFileName());
                });
            }else
            {
                //copy to save
            }
            
        }

        data = await Task.Run(() =>
        {
            var data = new PlayData();
            return data;
        });

    }
    public void EndStory()
    {

    }
    public async void BeginScene(string fileName)
    {
        _sceneCtrl.Begin(fileName);
    }

    public void EndScene()
    {
        _sceneCtrl.End();
    }

    #endregion
    public string GetSceneFileName()
    {
        return _folderName + "/Save/" + _sceneCtrl.fileName;
    }

    public string GetStorySaveFolder()
    {
        return _folderName + "/Save/";
    }
    public string GetStoryPlayDataFileName()
    {
        return _folderName + "/Save/playData";
    }
}
