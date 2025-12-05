using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ui.Loading;
using Ui.ModSceneMain;
using Unity.VisualScripting;
using UnityEngine;
using Z_DesignStyle;
using Z_Input;
using Z_Map;
using Z_Map.Analysis;
using Z_Map.Form;
using Z_Ui;
using Z_Ui.Loading;
using Z_UnitSystem;
using static UnityEditor.PlayerSettings;

public class PlayManager : Z_MonoManager<PlayManager>
{
    public PlayData data;
    private string _folderName;

    public bool boxPlay;

    private bool enable;

    #region life

    private InternalPlaySceneController _sceneCtrl;
    public ExternalPlaySceneController sceneCtrl;

    private InternalPlayInfoController _infoCtrl;
    public ExternalPlayInfoController infoCtrl;

    private InternalPlayAssetController _assetCtrl;
    public ExternalPlayAssetController assetCtrl;

    // public ModAssetCtrl assetCtrl;
    public override void Init()
    {
        base.Init();

        var __sceneCtrl = new PlaySceneController(this);
        _sceneCtrl = __sceneCtrl;
        sceneCtrl = __sceneCtrl;

        var __infoCtrl = new PlayInfoController(this);
        _infoCtrl = __infoCtrl;
        infoCtrl = __infoCtrl;

        var __assetCtrl = new PlayAssetController(this);
        _assetCtrl = __assetCtrl;
        assetCtrl = __assetCtrl;
        //  assetCtrl = new ModAssetCtrl(this);
    }


    public void FixedUpdate()
    {
    }
    public void Update()
    {
        if(!enable)
        {
            return;
        }
        _sceneCtrl.Update();
        _infoCtrl.Update();
    }
    public void LateUpdate()
    {
        if (!enable)
        {
            return;
        }
        GameManager.instance.evtCtrl.LateUpdate();
    }
    public async void BeginStory(string storyName, bool boxPlay)
    {
        this._folderName = storyName;
        data = null;
        this.boxPlay = boxPlay;

        LoadingManager.instance.AddLoadItem("playData");
        if (!boxPlay)
        {
            if (!SaveAndLoad.Exist(GetStorySaveProgressFileName()))
            {
                //new , copy to save
                foreach(var data in SceneForm.DataByUid.Values)
                {
                    SaveAndLoad.Copy(GetStoryCoreFolder() + data.uid, GetStorySaveFolder() + data.uid);
                }

                //create play only
                SaveAndLoad.Save(GetStorySaveProgressFileName(), ProgressForm.GetJoByData(GetInitPlayDataByConfig().progress).ToString());
            }

            data = await Task.Run(() =>
            {
                GameManager.instance.saveCtrl.LoadUiItem(GetStorySaveFolder());
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

        LoadingManager.instance.RemoveLoadItem("playData");

        Main2StoryManager.instance.StartLoadScenePlay(GameManager.instance.curConfig.startSceneId);
        _assetCtrl.Begin();

        enable = true;
    }

    public static PlayData GetInitPlayDataByConfig()
    {
        var config = GameManager.instance.curConfig;
        //get instance by proto
        var items = new List<int>();
        foreach (var uid in config.defaultBag)
        {
            var newItem = ItemProductForm.DataByUid[uid].Copy(false);
            newItem.ToProduct();
            items.Add(newItem.uid);
        }
        var characters = new List<int>();
        var charactersActive = new List<int>();


        foreach (var uid in config.defaultTeam)
        {
            var ch = CharacterProductForm.DataByUid[uid];
            if (!ch.unique)
            {
                var newCharacter = ch.Copy(false);
                newCharacter.ToProduct();
                GameManager.instance.characterCtrl.RegisterAnim(newCharacter);

                characters.Add(newCharacter.uid);
                foreach (var uidActive in config.defaultTeamActive)
                {
                    if (uidActive == uid)
                    {
                        charactersActive.Add(newCharacter.uid);
                        break;
                    }
                }
            }
        }
        return new PlayData(new ProgressForm.Data(1, config.startSceneId, config.startpos, config.mainCharacterUid, items, characters, charactersActive, default, Z_Ui.Form.ClipForm.defaultData.Copy(),new Dictionary<int, List<string>>(), false,0));
    }

    public void EndStory()
    {
        _assetCtrl.End();
        enable = false;
    }
    public async void BeginScene(int id)
    {
        _sceneCtrl.Begin(id);
        _infoCtrl.Begin();
    }

    public void EndScene()
    {
        _sceneCtrl.End();
        _infoCtrl.End();
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
        return _folderName + "/Core/cf";
    }

}
