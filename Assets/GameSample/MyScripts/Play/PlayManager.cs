using Form;
using System.Collections.Generic;
using Ui.Start;
using Z_DesignStyle;
using Z_Ui;
using Z_Ui.Loading;
using Z_UnitSystem;
public enum ParamShowType
{
    Always,
    AlwaysWithPanel,
    OnlyNotZero,
    Hide
}
public class PlayManager : Z_MonoManager<PlayManager>
{
    private string _folderName;

    public bool boxPlay;

    public bool enable;

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
        if (!enable)
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
    public async void BeginStory(int id, bool boxPlay)
    {
        this._folderName = Main2StoryManager.GetStoryFolderNameById(id);
        this.boxPlay = boxPlay;

        LoadingManager.instance.AddLoadItem("playData");
        if (!boxPlay)
        {
            if (!SaveAndLoad.Exist(GetStorySaveFolder()))
            {
                GameManager.instance.saveCtrl.LoadCoreStory(id);
                FirstPlayInit();
            }
            GameManager.instance.saveCtrl.LoadSaveStory(id);
        }
        else
        {
            GameManager.instance.saveCtrl.LoadCoreStory(id);
            FirstPlayInit();
        }

        LoadingManager.instance.RemoveLoadItem("playData");

        Main2StoryManager.instance.StartLoadScenePlay(GameManager.instance.curProgress.sceneId);
        _assetCtrl.Begin();

        enable = true;
    }



    public void EndStory()
    {
        _assetCtrl.End();
        GameManager.instance.evtCtrl.Reset();
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
    public static void FirstPlayInit()
    {
        var progress = GameManager.instance.curProgress;
        //get instance by proto
        var items = new List<int>();
        foreach (var uid in progress.bag)
        {
            var newItem = ItemProductForm.DataByUid[uid].Copy(false);
            newItem.ToProduct();
            items.Add(newItem.uid);
        }
        var characters = new List<int>();
        var charactersActive = new List<int>();


        foreach (var uid in progress.team)
        {
            var ch = CharacterProductForm.DataByUid[uid];
            if (!ch.unique)
            {
                var newCharacter = ch.Copy(false);
                newCharacter.ToProduct();

                characters.Add(newCharacter.uid);
                foreach (var uidActive in progress.teamActive)
                {
                    if (uidActive == uid)
                    {
                        charactersActive.Add(newCharacter.uid);
                        break;
                    }
                }
            }
        }
    }
    public void Exit()
    {
        int curId = GameManager.instance.curStory.id;

        Main2StoryManager.instance.UnloadScenePlay();
        Main2StoryManager.instance.UnloadStoryPlay();
        if (instance.boxPlay)
        {
            Main2StoryManager.instance.StartLoadStoryUgc(curId, default);
        }
        else
        {
            UiManager.instance.ShowUi<UiStartCtrl>();
        }

    }

    public string GetSceneCacheFileName()
    {
        return GetStoryCacheFolder() + _sceneCtrl.fileName;
    }
    public string GetSceneSaveFileName()
    {
        return _folderName + "/Save/" + _sceneCtrl.fileName;
    }
    public string GetStoryCoreFolder()
    {
        return _folderName + "/Core/";
    }
    public string GetStoryCacheFolder()
    {
        return _folderName + "/Cache/";
    }
    public string GetStorySaveFolder()
    {
        return _folderName + "/Save/";
    }
    public string GetStorySaveProgressFileName()
    {
        return _folderName + "/Save/" + GameManager.instance.saveCtrl.progressFormFileName;
    }

}
