using Form;
using System.Collections.Generic;
using Ui.ModStory.ModStoryEffect.ModStoryEffectUnit;
using Ui.Start;
using Z_DesignStyle;
using Z_Ui;
using Z_Ui.Loading;
using Z_UnitSystem;

public enum StoryLifeEventType
{
    FirstEnter = 0,
    EverySecond = 1,
    Enter = 3,
    Leave = 2,
}
public class StoryLifeEvent : Z_Event
{
    public StoryLifeEventType type;
}

public enum ParamShowType
{
    Always,
    AlwaysWithPanel,
    AlwaysWithPanelAndScene,
    AlwaysWithPanelAndSceneWithoutPlayer,
    OnlyNotZero,
    Hide
}
public class PlayManager : Z_MonoManager<PlayManager>
{
    private string _folderName;

    public bool boxPlay;

    public bool enable;
    private const string ScenePresentationLoadItem = "playSceneStart";
    private bool scenePresentationPending;

#if UNITY_EDITOR
    [UnityEngine.SerializeField]
    [UnityEngine.Tooltip("在 Play 运行时绘制当前玩家 nav 可通行的 Tile（Scene 视图需开启 Gizmos）。")]
    private bool showWalkableNavTiles = true;
#endif

    #region life

    private InternalPlaySceneController _sceneCtrl;
    public ExternalPlaySceneController sceneCtrl;

    private InternalPlayInfoController _infoCtrl;
    public ExternalPlayInfoController infoCtrl;

    private InternalPlayAssetController _assetCtrl;
    public ExternalPlayAssetController assetCtrl;


    public PlaySceneEffectController effectCtrl;
    public PlayMapController mapCtrl;
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


        effectCtrl = new PlaySceneEffectController(this);
        mapCtrl = new PlayMapController(this);
    }


    public void FixedUpdate()
    {
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!UnityEngine.Application.isPlaying || !enable || !showWalkableNavTiles || sceneCtrl == null)
            return;

        var map = MapManager.instance;
        if (map.enable)
            map.navigationCtrl?.DrawWalkableTilesDebug(sceneCtrl.playerM?.unit);
    }
#endif

    public void Update()
    {
        if (!enable)
        {
            return;
        }
        NotifySceneEntered();
        UpdateScene();
    }

    private void NotifySceneEntered()
    {
        if (GameManager.instance.curProgress.targetScene.Item1 == GameManager.instance.curScene.uid && GameManager.instance.curProgress.sceneId != GameManager.instance.curScene.uid)
        {
            GameManager.instance.curProgress.sceneId = GameManager.instance.curScene.uid;
            if (!GameManager.instance.curScene.notFirstTime)
            {
                GameManager.instance.curScene.notFirstTime = true;
                Z_EventHelper.Invoke(new StoryLifeEvent() { type = StoryLifeEventType.FirstEnter });
            }
            else
            {

                Z_EventHelper.Invoke(new StoryLifeEvent() { type = StoryLifeEventType.Enter });
            }
        }
    }

    private void UpdateScene()
    {
        //lifeEvent
        if (GameManager.instance.curProgress.targetScene.Item1 != GameManager.instance.curScene.uid && GameManager.instance.curProgress.eventState != EventState.Leave)
        {
            GameManager.instance.curProgress.eventState = EventState.Leave;
            Z_EventHelper.Invoke(new StoryLifeEvent() { type = StoryLifeEventType.Leave });
        }


        if (GameManager.instance.curProgress.eventState == EventState.Leave)
        {
            if (EventInterpretDataForm.DataByUid.Count == 0)
            {
                Main2StoryManager.instance.ChangeScene(GameManager.instance.curProgress.targetScene.Item1, () =>
                {
                    GameManager.instance.curProgress.pos = (MapManager.instance.utilCtrl.MapPos2RealPos(GameManager.PlayerPosToMapPos(GameManager.instance.curProgress.targetScene.Item2)));
                });
                GameManager.instance.curProgress.eventState = EventState.Normal;
            }
        }

        _sceneCtrl.Update();
        _infoCtrl.Update();
        effectCtrl.Update();
    }
    public void LateUpdate()
    {
        if (!enable)
        {
            return;
        }
        GameManager.instance.evtCtrl.LateUpdate();
        mapCtrl.FlushUnlockTextures();
        // Entry scripts have now had their normal first pass (including ShowDialog).
        // Keep loading above the HUD until this point even if BeginScene ran after Update.
        CompleteScenePresentation();
    }
    public async void BeginStory(int id, bool boxPlay)
    {
        // Do not update scene state while the initial map is loading asynchronously.
        enable = false;
        this._folderName = Main2StoryManager.GetStoryFolderNameById(id);
        this.boxPlay = boxPlay;

        LoadingManager.instance.AddLoadItem("playData");
        if (!boxPlay)
        {
            if (!SaveAndLoad.Exist(GetStorySaveFolder()))
            {
                GameManager.instance.saveCtrl.LoadCoreStory(id);
                FirstPlayInit();
                GameManager.instance.saveCtrl.SaveSaveStory(id);
            }
            GameManager.instance.saveCtrl.LoadSaveStory(id);
        }
        else
        {
            GameManager.instance.saveCtrl.LoadCoreStory(id);
            FirstPlayInit();
        }

        LoadingManager.instance.RemoveLoadItem("playData");

        var firstEnter = GameManager.instance.curProgress.sceneId == 0;
        Main2StoryManager.instance.StartLoadScenePlay(GameManager.instance.curProgress.targetScene.Item1, () =>
        {
            if (firstEnter)
            {
                GameManager.instance.curProgress.pos = MapManager.instance.utilCtrl.MapPos2RealPos(GameManager.PlayerPosToMapPos(GameManager.instance.curProgress.targetScene.Item2));
            }
        });


        _assetCtrl.Begin();



    }



    public void EndStory()
    {
        CompleteScenePresentation();
        _assetCtrl.End();
        GameManager.instance.evtCtrl.Reset();
        enable = false;
    }
    public void BeginScene(int id)
    {
        scenePresentationPending = true;
        LoadingManager.instance.AddLoadItem(ScenePresentationLoadItem);
        try
        {
            _sceneCtrl.Begin(id);
            _infoCtrl.Begin();
            mapCtrl.Begin();
            effectCtrl.Begin();
            // Position and player creation must finish before lifecycle events/updates run.
            enable = true;
            NotifySceneEntered();
        }
        catch
        {
            enable = false;
            CompleteScenePresentation();
            throw;
        }
    }

    private void CompleteScenePresentation()
    {
        if (!scenePresentationPending)
            return;
        scenePresentationPending = false;
        LoadingManager.instance.RemoveLoadItem(ScenePresentationLoadItem);
    }

    public void EndScene()
    {
        enable = false;
        CompleteScenePresentation();
        effectCtrl.End();
        _sceneCtrl.End();
        _infoCtrl.End();
        mapCtrl.End();
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
            newItem.ToProduct(uid);
            items.Add(newItem.uid);
        }
        progress.bag = items;

        var characters = new List<int>();
        var charactersActive = new List<int>();

        foreach (var ch in CharacterProductForm.DataByUid.Values)
        {
            ch.CheckSkillProduct();
        }


        foreach (var uid in progress.team)
        {
            var ch = CharacterProductForm.DataByUid[uid];
            if (!ch.unique)
            {
                var newCharacter = ch.Copy(false);
                newCharacter.ToProduct(uid);

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
