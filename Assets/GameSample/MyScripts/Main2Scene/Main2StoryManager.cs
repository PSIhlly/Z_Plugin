using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using Z_Audio;
using Z_Code;
using Z_Code.Form;
using Z_DataSystem;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Input;
using Z_Map;
using Z_Texture;
using Z_Ui.Form;
using Z_Ui.Loading;
using Z_UnitSystem;



public class Main2StoryManager : Z_MonoManager<Main2StoryManager>
{
    #region story
    public void StartLoadStoryUgc(int storyId, EditorStyle defaultStyle)
    {
        string storyFolder = GetStoryFolderNameById(storyId);
        if (!SaveAndLoad.Exist(GetStoryCoreFolder(storyFolder)))
        {
            GameManager.instance.saveCtrl.ResetStory();
            StoryForm.AddData(new StoryForm.Data(storyId, "new" + storyId, "empty", string.Empty, Guid.NewGuid().ToString("N")));
            SceneForm.Clear();
            var sceneData = new SceneForm.Data(1, "scene", GlobalDefaultHelper.DefaultTexId, Vector2.zero, false, false, new Dictionary<string, EventTriggerForm.Data>(),new Dictionary<int, List<string>>(), new Dictionary<int, List<string>>(), false);
            SceneForm.AddData(sceneData);
            CharacterParamForm.Clear();
            CharacterProductForm.Clear();

            ItemParamForm.Clear();
            ItemProductForm.Clear();

            SkillParamForm.Clear();
            SkillProductForm.Clear();

            ModManager.instance.assetCtrl.CreateCharacter(0, "Player");
            var player = CharacterProductForm.DataByNameProtouid[("Player", 0)];
            player.unique = true;
            ProgressForm.Clear();
            var progress = new ProgressForm.Data(1, 0, 0, new Vector3(500, 750, 500), player.uid, new List<int>() { }, new List<int>() { player.uid }, new List<int>() { player.uid }, new Dictionary<string, string>(), CameraMode.Overhead, ClipForm.defaultData.Copy(), 0, defaultStyle, false, false, true, true, true, true, true, GlobalDefaultHelper.DefaultTexId, 0, (sceneData.uid, Vector3.zero), false, EventState.Normal);
            ProgressForm.AddData(progress);
            var data = new GameMapData();

            var dic = GameManager.instance.innerAssetDic;
            data.Init((GameObjectAssetForm.Data)dic["mapground"], (GameObjectAssetForm.Data)dic["mapslope"], (GameObjectAssetForm.Data)dic["mapfloor"], (GameObjectAssetForm.Data)dic["img"], (GameObjectAssetForm.Data)dic["canvas"], (TexAssetForm.Data)dic["defaultTileTexture"]);
            var defaultTextureLabId = LabForm.GetOrCreate("default", nameof(MapTextureForm));
            var defaultObjectLabId = LabForm.GetOrCreate("default", nameof(MapObjectForm));
            MapTextureForm.AddData(new MapTextureForm.Data(1, "grass", GameManager.instance.innerAssetDic["defaultTileTexture"].id, 0, new List<int>() { GameManager.instance.innerAssetDic["defaultTileTexture"].id }, defaultTextureLabId, new Dictionary<string, EventTriggerForm.Data>(), false, new Dictionary<int, int>(), 0, false, new List<int>()));
            var wall = new MapObjectForm.Data(1, "wall", GameManager.instance.innerAssetDic["defaultObjectTexture"].id,
                new MapModelForm.Data(1, new List<int>() { GameManager.instance.innerAssetDic["cube"].id }, new List<Vector3>() { Vector3.zero }, new List<Vector3>() { Vector3.one }, new List<List<int>>() { new List<int>() { GameManager.instance.innerAssetDic["defaultObjectTexture"].id } }, 0, true, 1),
                defaultObjectLabId, true, new Dictionary<string, EventTriggerForm.Data>(), new Dictionary<string, MapObjectParamForm.Data>(),
                GlobalDefaultHelper.DefaultTexId, FaceType.Fixed, new Dictionary<AnimDirecton, List<int>>());
            wall.EnsureDirectionData();
            wall.SyncLegacyAnimClip(AnimDirecton.Fixed);
            MapObjectForm.AddData(wall);

            EventProgramDataForm.Clear();

            var dialogLabId = LabForm.GetOrCreate("enterGame", "dialog", "", nameof(EventProgramDataForm));
            ModManager.instance.assetCtrl.CreateEvent(dialogLabId, "mainDialog");
            var dialogEvt = EventProgramDataForm.DataByUid.Values.FirstOrDefault(d => d.name == "mainDialog");

            sceneData.events["onEnterEvent"] = new EventTriggerForm.Data(-1, "onEnterEvent", new List<int>() { dialogEvt.uid }, default);

            switch (defaultStyle)
            {
                case EditorStyle.Avg:
                case EditorStyle.AvgAdvanced:
                    dialogEvt.ApplyCode(@"ShowDialog(""$i$$i$"",""$i$$i$"",""player"",""hello"");ShowDialog(""$i$$i$"",""$i$$i$"",""player"",""you can edit it in event panel"");GameOver();");
                    break;
                case EditorStyle.Rpg:
                case EditorStyle.RpgAdvanced:
                    dialogEvt.ApplyCode(@"GamePause();ShowDialog(""$i$$i$"",""$i$$i$"",""player"",""hello"");ShowDialog(""$i$$i$"",""$i$$i$"",""player"",""you can edit it in event panel"");");
                    var prmBox = CodeHelper.CreateBoxByNum(100);
                    var hpParamData = new CharacterParamForm.Data(-1, "Hp", 0, "", BoxDataForm.GetJoByData(prmBox).ToString(), "HpMax", ParamShowType.AlwaysWithPanelAndScene);
                    var hpMaxParamData = new CharacterParamForm.Data(-1, "HpMax", 0, "", BoxDataForm.GetJoByData(prmBox).ToString(), "", ParamShowType.AlwaysWithPanelAndScene);
                    prmBox.num = 5;
                    var speedParamData = new CharacterParamForm.Data(-1, "Speed", 0, "", BoxDataForm.GetJoByData(prmBox).ToString(), "", default);
                    CharacterParamForm.AddData(hpParamData);
                    CharacterParamForm.AddData(speedParamData);
                    CharacterParamForm.AddData(hpMaxParamData);
                    player.hpParamName = "Hp";
                    player.speedParamName = "Speed";
                    player.paramDic[player.hpParamName] = hpParamData.Copy();
                    player.paramDic["HpMax"] = hpMaxParamData.Copy();
                    player.paramDic[player.speedParamName] = speedParamData.Copy();
                    break;
            }
            GameManager.instance.saveCtrl.SaveCoreStory(storyId);
            GameManager.instance.saveCtrl.SaveSceneMap(GetStoryCoreFolder(storyFolder) + Main2StoryManager.GetSceneFileNameById(sceneData.uid), data);

        }
        StartLoadStory(storyId);
        ModManager.instance.BeginStory(storyId);
    }
    public void StartLoadStoryPlay(int storyId, bool boxPlay)
    {
        StartLoadStory(storyId);
        PlayManager.instance.BeginStory(storyId, boxPlay);

    }
    private void StartLoadStory(int storyId)
    {
        string storyFolder = GetStoryFolderNameById(storyId);
        StoryTexAssetForm.ClearAuto();
        GameManager.instance.curStory = StoryForm.DataById[storyId];

    }
    public void UnloadStoryUgc()
    {
        ModManager.instance.EndStory();
        UnloadStory();
    }
    public void UnloadStoryPlay()
    {
        PlayManager.instance.EndStory();
        UnloadStory();
    }

    private readonly List<int> wangTileGameTexIds = new List<int>();

    private void CreateWangTileTextures()
    {
        DestroyWangTileTextures();

        foreach (MapTextureForm.Data mapTexture in MapTextureForm.DataById.Values)
        {
            mapTexture.WangTileDic = new Dictionary<int, int>();
            if (!mapTexture.isWangTile || mapTexture.texs == null || mapTexture.texs.Count == 0)
                continue;

            TexAssetForm.Data sourceData = TexAssetForm.DataById.GetDv(mapTexture.texs[0], null);
            if (!(sourceData?.GetTex() is Texture2D sourceTexture))
            {
                Debug.LogError($"WangTile '{mapTexture.name}' has no Texture2D source.");
                continue;
            }

            try
            {
                Dictionary<int, Sprite> spritesByMask = TileHelper.GetAutoTileSprites(sourceTexture);
                Dictionary<Sprite, int> texIdBySprite = new Dictionary<Sprite, int>();

                foreach (KeyValuePair<int, Sprite> pair in spritesByMask)
                {
                    if (!texIdBySprite.TryGetValue(pair.Value, out int texId))
                    {
                        TexAssetForm.Data generatedData = new TexAssetForm.Data(
                            -1,
                            $"WangTile_{mapTexture.id}_{texIdBySprite.Count}",
                            string.Empty,
                            null,
                            string.Empty,
                            pair.Value.texture,
                            LabForm.NoneId);
                        GameManager.instance.saveCtrl.AddGameTex(ref generatedData);
                        texId = generatedData.id;
                        texIdBySprite.Add(pair.Value, texId);
                        wangTileGameTexIds.Add(texId);
                    }

                    mapTexture.WangTileDic[pair.Key] = texId;
                }

                foreach (Sprite sprite in texIdBySprite.Keys)
                    Destroy(sprite);
            }
            catch (Exception exception)
            {
                Debug.LogError($"Failed to create WangTile '{mapTexture.name}': {exception.Message}");
                mapTexture.WangTileDic.Clear();
            }
        }
    }

    private void DestroyWangTileTextures()
    {
        foreach (int texId in wangTileGameTexIds)
        {
            if (!GameTexAssetForm.DataById.TryGetValue(texId, out GameTexAssetForm.Data data))
                continue;

            if (data.asset is Texture texture)
                Destroy(texture);
            GameTexAssetForm.RemoveData(texId);
        }
        wangTileGameTexIds.Clear();

        foreach (MapTextureForm.Data mapTexture in MapTextureForm.DataById.Values)
            mapTexture.WangTileDic?.Clear();
    }
    public void UnloadStory()
    {
        //clean auto content
        var idLst = new List<int>(MapBaseForm.DataById.Keys);
        foreach (var id in idLst)
        {
            if (id <= MapBaseForm.autoIdCnt)
            {
                MapBaseForm.RemoveData(id);
            }
        }
        GameManager.instance.saveCtrl.ResetStory();
        ProgressForm.Clear();
        GameManager.instance.curStory = null;

        AudioManager.instance.BgmStreaming(GlobalSettings.BGM_FILE_NAME);
    }

    public void DeleteStory(int id)
    {
        SaveAndLoad.Delete(GetStoryFolderNameById(id));
        StoryForm.RemoveData(id);
    }
    #endregion

    #region scene
    public async void StartLoadSceneUgc(int sceneId)
    {
        bool ok = await StartLoadScene(ModManager.instance.GetStoryCoreFolder(), sceneId);
        //ugc模式不搞碰撞缩放
        foreach (var form in MapObjectForm.DataById.Values)
        {
            var name = GlobalDefaultHelper.GetRuntimeMapObjectPrefabName(form.id);
            var prefab = InstancePoolManager.instance.GetPrefab(name);
            if (prefab != null)
            {
                foreach (var col in prefab.GetComponentsInChildren<Collider>())
                {
                    col.transform.localScale = Vector3.one;
                }
            }
        }
        DynamicGlobalSettings.playing = false;
        ModManager.instance.BeginScene(sceneId);
    }
    public async void StartLoadScenePlay(int sceneId, Action onMapComplete = null)
    {
        bool ok = await StartLoadScene(PlayManager.instance.GetStoryCacheFolder(), sceneId, onMapComplete);
        DynamicGlobalSettings.playing = true;


        foreach (var textureData in MapTextureForm.DataById.Values)
        {

        }
        PlayManager.instance.BeginScene(sceneId);
    }

    private async Task<bool> StartLoadScene(string storyFolder, int id, Action onMapComplete = null)
    {
        if (!SaveAndLoad.Exist(storyFolder + "/" + GetSceneFileNameById(id)))
        {
            Debug.LogError(storyFolder + "/" + GetSceneFileNameById(id) + " scene file not exist");
        }
        GameManager.instance.curScene = SceneForm.DataByUid[id];

        GameManager.instance.saveCtrl.ResetPrefabPool();

        CreateWangTileTextures();
        LoadingManager.instance.AddLoadItem("scene");
        MapInfo data;

        data = await Task.Run(() =>
        {
            return GameManager.instance.saveCtrl.LoadSceneMap(storyFolder + "/" + GetSceneFileNameById(id));
        });

        LoadingManager.instance.RemoveLoadItem("scene");

        /* {
             //new
             data = await Task.Run(() =>
             {
                 var data = new GameMapData();
                 data.Init();
                 return data;
             });
         }*/
        data.mainData.viewSize = new Vector3Int((int)(InputManager.instance.screenWorldSize.x / 2) + 4, 5, (int)(InputManager.instance.screenWorldSize.y / 2) + 4);

        MapManager.instance.Begin(data);
        onMapComplete?.Invoke();



        return true;
    }
    public void UnloadSceneUgc()
    {
        UnloadScene();
        ModManager.instance.EndScene();
    }
    public void UnloadScenePlay()
    {

        GameManager.instance.saveCtrl.SaveSceneMap(PlayManager.instance.GetSceneCacheFileName());
        UnloadScene();
        PlayManager.instance.EndScene();
    }
    public void UnloadScene()
    {
        GameManager.instance.mapCtrl.Reset();
        GameManager.instance.characterCtrl.Reset();
        GameManager.instance.curScene = null;
        DestroyWangTileTextures();
    }
    public void ChangeScene(int sceneId, Action oncomplete = null)
    {
        UnloadScenePlay();

        StartLoadScenePlay(sceneId, oncomplete);
    }
    #endregion

    #region util
    public static string GetStoryFolderNameById(int id)
    {
        return id.ToString();
    }
    public static string GetSceneFileNameById(int id)
    {
        return id.ToString();
    }
    public static string GetStoryCoreFolder(int id)
    {
        return GetStoryFolderNameById(id) + "/Core/";
    }
    public static string GetStoryAssetFolder(int id)
    {
        return GetStoryCoreFolder(id);
    }
    public static string GetStorySaveFolder(int id)
    {
        return GetStoryFolderNameById(id) + "/Save/";
    }
    public static string GetStoryCacheFolder(int id)
    {
        return GetStoryFolderNameById(id) + "/Cache/";
    }
    public static string GetStoryCoreFolder(string folderName)
    {
        return folderName + "/Core/";
    }
    public static string GetStorySaveFolder(string folderName)
    {
        return folderName + "/Save/";
    }
    public static string GetStoryCacheFolder(string folderName)
    {
        return folderName + "/Cache/";
    }
    #endregion
}
