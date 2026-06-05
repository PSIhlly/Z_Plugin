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
    public void Update()
    {
        if (!enable)
        {
            return;
        }
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

        Main2StoryManager.instance.StartLoadScenePlay(GameManager.instance.curProgress.targetScene.Item1, () =>
        {
            if (GameManager.instance.curProgress.sceneId == 0)
            {
                GameManager.instance.curProgress.pos = MapManager.instance.utilCtrl.MapPos2RealPos(GameManager.PlayerPosToMapPos(GameManager.instance.curProgress.targetScene.Item2));
            }
        });


        _assetCtrl.Begin();

        effectCtrl.Begin();


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
        mapCtrl.Begin();
    }

    public void EndScene()
    {
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
