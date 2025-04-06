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
            if (!SaveAndLoad.Exist(GetStorySaveProgressFileName()))
            {
                //new , copy to save
                SaveAndLoad.Copy(GetStoryCoreFolder() + "scene1", GetStorySaveFolder() + "scene1");

                SaveAndLoad.Copy(GetStoryCoreConfigFileName(), GetStorySaveProgressFileName());

                //create play only
                SaveAndLoad.Save(GetStorySaveProgressFileName(), ProgressForm.GetJoByData(GetInitPlayDataByConfig().progress).ToString());
            }

            data = await Task.Run(() =>
            {
                return GameManager.instance.saveCtrl.LoadProgress(GetStorySaveProgressFileName());
            });

        }
        else
        {
            data = await Task.Run(() =>
            {
                return GetInitPlayDataByConfig();
            });
        }


        Main2StoryManager.instance.StartLoadScenePlay("scene1");

    }

    public static PlayData GetInitPlayDataByConfig()
    {
        var config = ConfigForm.DataByUid[1];
        return new PlayData(new ProgressForm.Data(1, config.startSceneId, config.startpos, config.mainCharacterName));
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
    public string GetStorySaveProgressFileName()
    {
        return _folderName + "/Save/progress";
    }
    public string GetStoryCoreConfigFileName()
    {
        return _folderName + "/Core/config";
    }

}
