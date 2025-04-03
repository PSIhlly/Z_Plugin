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

    public bool boxPlay;

    

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
    public async void BeginStory(string storyName, bool boxPlay)
    {
        this._folderName = storyName;
        PlayData data = null; 
        this.boxPlay = boxPlay;
        if (!boxPlay)
        {
            if (!SaveAndLoad.Exist(GetStorySavePlayDataFileName()))
            {
                //new , copy to save
                SaveAndLoad.Copy(GetStoryCoreFolder() + "scene1", GetStorySaveFolder() + "scene1");

                SaveAndLoad.Copy(GetStoryCorePlayDataFileName(), GetStorySavePlayDataFileName());

            }

            data = await Task.Run(() =>
            {
                return GameManager.instance.saveCtrl.LoadPlayData(GetStorySavePlayDataFileName());
            });

        }
        else
        {
            data = await Task.Run(() =>
            {
                return GameManager.instance.saveCtrl.LoadPlayData(GetStoryCorePlayDataFileName());
            });
        }


        Main2StoryManager.instance.StartLoadScenePlay("scene1");

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
    public string GetSceneSaveFileName()
    {
        return _folderName + "/Save/" + _sceneCtrl.fileName;
    }
    public string GetStoryCoreFolder()
    {
        return _folderName + "/Core/";
    }

    public string GetStorySaveFolder()
    {
        return _folderName + "/Save/";
    }
    public string GetStorySavePlayDataFileName()
    {
        return _folderName + "/Save/playData";
    }
    public string GetStoryCorePlayDataFileName()
    {
        return _folderName + "/Core/playData";
    }

}
