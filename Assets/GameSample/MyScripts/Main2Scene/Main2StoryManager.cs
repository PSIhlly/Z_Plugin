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
            var progress = new ProgressForm.Data(1, 0, 0, new Vector3(500, 500, 500), player.uid, new List<int>() { }, new List<int>() { player.uid }, new List<int>() { player.uid }, new Dictionary<string, string>(), CameraMode.Overhead, ClipForm.defaultData.Copy(), 0, defaultStyle, false, false, true, true, true, true, true, GlobalDefaultHelper.DefaultTexId, 0, (sceneData.uid, Vector3.zero), false, EventState.Normal, false, -1f, true, true);
            ProgressForm.AddData(progress);
            var data = new GameMapData();

            var dic = GameManager.instance.innerAssetDic;
            data.Init((GameObjectAssetForm.Data)dic["mapground"], (GameObjectAssetForm.Data)dic["mapslope"], (GameObjectAssetForm.Data)dic["mapfloor"], (GameObjectAssetForm.Data)dic["img"], (GameObjectAssetForm.Data)dic["canvas"], (TexAssetForm.Data)dic["defaultTileTexture"]);
            var defaultTextureLabId = LabForm.GetOrCreate("default", nameof(MapTextureForm));
            var defaultObjectLabId = LabForm.GetOrCreate("default", nameof(ObjectProductForm));
            MapTextureForm.AddData(new MapTextureForm.Data(1, "grass", GameManager.instance.innerAssetDic["defaultTileTexture"].id, 0, new List<int>() { GameManager.instance.innerAssetDic["defaultTileTexture"].id }, defaultTextureLabId, new Dictionary<string, EventTriggerForm.Data>(), false, new Dictionary<int, int>(), 0, false, new List<int>(), false, new Dictionary<int, int>()));
            var wall = new ObjectProductForm.Data(-1, "wall", defaultObjectLabId, 0, GameManager.instance.innerAssetDic["defaultObjectTexture"].id,
                new MapModelForm.Data(1, new List<int>() { GameManager.instance.innerAssetDic["cube"].id }, new List<Vector3>() { Vector3.zero }, new List<Vector3>() { Vector3.one }, new List<List<int>>() { new List<int>() { GameManager.instance.innerAssetDic["defaultObjectTexture"].id } }, 0, true, 1),
                true, new Dictionary<string, EventTriggerForm.Data>(), new Dictionary<string, MapObjectParamForm.Data>(),
                GlobalDefaultHelper.DefaultTexId, FaceType.Fixed, new Dictionary<AnimDirecton, List<int>>(), false, false, true);
            wall.EnsureDirectionData();
            wall.SyncLegacyAnimClip(AnimDirecton.Fixed);
            ObjectProductForm.AddData(wall);

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
    private readonly Dictionary<int, List<int>> objectWangTileGameTexIds = new Dictionary<int, List<int>>();

    private void CreateWangTileTextures()
    {
        DestroyWangTileTextures();

        foreach (MapTextureForm.Data mapTexture in MapTextureForm.DataById.Values)
        {
            mapTexture.WangTileDic = new Dictionary<int, int>();
            mapTexture.FrontWangTileDic = new Dictionary<int, int>();
            if (mapTexture.isWangTile)
                CreateWangTileTextureVariants(mapTexture, mapTexture.texs, mapTexture.WangTileDic, false);
            if (mapTexture.enableFrontPart && mapTexture.frontIsWangTile)
                CreateWangTileTextureVariants(mapTexture, mapTexture.frontPartTexs, mapTexture.FrontWangTileDic, true);
        }

        foreach (ObjectProductForm.Data mapObject in ObjectProductForm.DataByUid.Values.Where(data => data.protoUid == 0))
            CreateObjectWangTileTextures(mapObject);
    }

    private void CreateObjectWangTileTextures(ObjectProductForm.Data mapObject)
    {
        if (!mapObject.isWangTile)
            return;

        mapObject.EnsureDirectionData();
        if (mapObject.faceType == FaceType.FourDirection)
        {
            foreach (AnimDirecton direction in new[]
                     { AnimDirecton.Up, AnimDirecton.Down, AnimDirecton.Left, AnimDirecton.Right })
                CreateObjectWangTileTextureVariants(mapObject, direction);
        }
        else
        {
            CreateObjectWangTileTextureVariants(mapObject, AnimDirecton.Fixed);
        }
    }

    public void RefreshObjectWangTileTextures(ObjectProductForm.Data mapObject)
    {
        if (mapObject == null || GameManager.instance.curScene == null)
            return;

        if (ObjectProductForm.DataByUid.TryGetValue(ObjectProductForm.GetTemplateUid(mapObject), out var prototype))
            mapObject = prototype;

        GameMapController.ClearObjectWangTileAnimationTextures(ObjectProductForm.GetTemplateUid(mapObject));
        if (objectWangTileGameTexIds.TryGetValue(ObjectProductForm.GetTemplateUid(mapObject), out List<int> ids))
        {
            DestroyGeneratedWangTileTextures(ids);
            var obsolete = new HashSet<int>(ids);
            wangTileGameTexIds.RemoveAll(obsolete.Contains);
            objectWangTileGameTexIds.Remove(ObjectProductForm.GetTemplateUid(mapObject));
        }

        CreateObjectWangTileTextures(mapObject);
        GameManager.instance.mapCtrl.RefreshObjectWangTileAppearances(ObjectProductForm.GetTemplateUid(mapObject));
    }

    private void CreateObjectWangTileTextureVariants(ObjectProductForm.Data mapObject, AnimDirecton direction)
    {
        CreateWangTileTextureVariants($"ObjectWangTile_{ObjectProductForm.GetTemplateUid(mapObject)}_{direction}",
            mapObject.GetAnimClip(direction),
            (mask, textureId) => GameMapController.RegisterObjectWangTileAnimationTexture(
                ObjectProductForm.GetTemplateUid(mapObject), direction, mask, textureId),
            textureId =>
            {
                if (!objectWangTileGameTexIds.TryGetValue(ObjectProductForm.GetTemplateUid(mapObject), out List<int> ids))
                {
                    ids = new List<int>();
                    objectWangTileGameTexIds.Add(ObjectProductForm.GetTemplateUid(mapObject), ids);
                }
                ids.Add(textureId);
            });
    }

    private void CreateWangTileTextureVariants(MapTextureForm.Data mapTexture, List<int> textures,
        Dictionary<int, int> variants, bool frontPart)
    {
        string partName = frontPart ? "FrontWangTile" : "WangTile";
        CreateWangTileTextureVariants($"{partName}_{mapTexture.id}", textures, (mask, textureId) =>
        {
            if (!variants.ContainsKey(mask))
                variants[mask] = textureId;
            GameMapController.RegisterWangTileAnimationTexture(mapTexture.id, frontPart, mask, textureId);
        });
    }

    private void CreateWangTileTextureVariants(string variantName, List<int> textures,
        Action<int, int> registerVariant, Action<int> registerGeneratedTexture = null)
    {
        if (textures == null || textures.Count == 0)
            return;

        for (int frameIndex = 0; frameIndex < textures.Count; frameIndex++)
        {
            TexAssetForm.Data sourceData = TexAssetForm.DataById.GetDv(textures[frameIndex], null);
            var animationFrames = sourceData?.GetAnimationFrames();
            if (animationFrames != null && animationFrames.Count > 0)
            {
                Dictionary<int, List<AnimatedFrameData>> animatedVariants;
                try { animatedVariants = CreateAnimatedWangTileFrames(animationFrames); }
                catch (Exception exception)
                {
                    Debug.LogError($"Failed to create {variantName} frame {frameIndex}: {exception.Message}");
                    continue;
                }
                var idsByTexture = new Dictionary<Texture2D, int>();
                foreach (var pair in animatedVariants)
                {
                    var firstTexture = pair.Value[0].texture;
                    if (!idsByTexture.TryGetValue(firstTexture, out int texId))
                    {
                        var generatedData = new TexAssetForm.Data(-1,
                            $"{variantName}_{frameIndex}_{idsByTexture.Count}", string.Empty,
                            null, string.Empty, firstTexture, LabForm.NoneId);
                        GameManager.instance.saveCtrl.AddGameTex(ref generatedData);
                        // AddGameTex wraps the row in its concrete subtype; attach runtime frames afterwards.
                        generatedData.SetRuntimeAnimationFrames(pair.Value);
                        texId = generatedData.id;
                        idsByTexture.Add(firstTexture, texId);
                        wangTileGameTexIds.Add(texId);
                        registerGeneratedTexture?.Invoke(texId);
                    }
                    registerVariant(pair.Key, texId);
                }
                continue;
            }
            if (!(sourceData?.GetTex() is Texture2D sourceTexture))
            {
                Debug.LogError($"{variantName} frame {frameIndex} has no Texture2D source.");
                continue;
            }

            Dictionary<int, Sprite> spritesByMask = null;
            try
            {
                spritesByMask = TileHelper.GetAutoTileSprites(sourceTexture);
                Dictionary<Sprite, int> texIdBySprite = new Dictionary<Sprite, int>();

                foreach (KeyValuePair<int, Sprite> pair in spritesByMask)
                {
                    if (!texIdBySprite.TryGetValue(pair.Value, out int texId))
                    {
                        TexAssetForm.Data generatedData = new TexAssetForm.Data(
                            -1,
                            $"{variantName}_{frameIndex}_{texIdBySprite.Count}",
                            string.Empty,
                            null,
                            string.Empty,
                            pair.Value.texture,
                            LabForm.NoneId);
                        GameManager.instance.saveCtrl.AddGameTex(ref generatedData);
                        texId = generatedData.id;
                        texIdBySprite.Add(pair.Value, texId);
                        wangTileGameTexIds.Add(texId);
                        registerGeneratedTexture?.Invoke(texId);
                    }

                    registerVariant(pair.Key, texId);
                }
            }
            catch (Exception exception)
            {
                Debug.LogError($"Failed to create {variantName} frame {frameIndex}: {exception.Message}");
            }
            finally
            {
                if (spritesByMask != null)
                    foreach (Sprite sprite in new HashSet<Sprite>(spritesByMask.Values))
                        Destroy(sprite);
            }
        }
    }

    internal static Dictionary<int, List<AnimatedFrameData>> CreateAnimatedWangTileFrames(
        IReadOnlyList<AnimatedFrameData> frames)
    {
        var result = new Dictionary<int, List<AnimatedFrameData>>(256);
        try
        {
            foreach (var frame in frames)
            {
                var sprites = TileHelper.GetAutoTileSprites(frame.texture);
                try
                {
                    foreach (var pair in sprites)
                    {
                        if (!result.TryGetValue(pair.Key, out var clip))
                        {
                            clip = new List<AnimatedFrameData>(frames.Count);
                            result.Add(pair.Key, clip);
                        }
                        clip.Add(new AnimatedFrameData { texture = pair.Value.texture, delaySeconds = frame.delaySeconds });
                    }
                }
                finally
                {
                    foreach (var sprite in new HashSet<Sprite>(sprites.Values))
                        Destroy(sprite);
                }
            }
            return result;
        }
        catch
        {
            var textures = new HashSet<Texture>();
            foreach (var clip in result.Values)
                foreach (var frame in clip)
                    if (textures.Add(frame.texture)) Destroy(frame.texture);
            throw;
        }
    }

    private static void DestroyGeneratedWangTileTextures(IEnumerable<int> ids)
    {
        var destroyed = new HashSet<Texture>();
        foreach (int texId in ids)
        {
            if (!GameTexAssetForm.DataById.TryGetValue(texId, out var data))
                continue;
            var frames = data.GetAnimationFrames();
            if (frames != null)
                foreach (var frame in frames)
                    if (destroyed.Add(frame.texture)) Destroy(frame.texture);
            if (data.asset is Texture texture && destroyed.Add(texture))
                Destroy(texture);
            GameTexAssetForm.RemoveData(texId);
        }
    }

    private void DestroyWangTileTextures()
    {
        GameMapController.ClearWangTileAnimationTextures();
        DestroyGeneratedWangTileTextures(wangTileGameTexIds);
        wangTileGameTexIds.Clear();
        objectWangTileGameTexIds.Clear();

        foreach (MapTextureForm.Data mapTexture in MapTextureForm.DataById.Values)
        {
            mapTexture.WangTileDic?.Clear();
            mapTexture.FrontWangTileDic?.Clear();
        }
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
    public async Task StartLoadSceneUgc(int sceneId)
    {
        DynamicGlobalSettings.playing = false;
        bool ok = await StartLoadScene(ModManager.instance.GetStoryCoreFolder(), sceneId);
        //ugc模式不搞碰撞缩放
        foreach (var form in ObjectProductForm.DataByUid.Values.Where(data => data.protoUid == 0))
        {
            var name = GlobalDefaultHelper.GetRuntimeMapObjectPrefabName(form.uid);
            var prefab = InstancePoolManager.instance.GetPrefab(name);
            if (prefab != null)
            {
                foreach (var col in prefab.GetComponentsInChildren<Collider>())
                {
                    col.transform.localScale = Vector3.one;
                }
            }
        }
        foreach (var product in ObjectProductForm.DataByUid.Values.Where(data => data.protoUid == 0))
            if (product.boundsCollision || !product.centerCollider)
                GameManager.instance.mapCtrl.RefreshObjectWangTileAppearances(product.uid);
        ModManager.instance.BeginScene(sceneId);
    }
    public async void StartLoadScenePlay(int sceneId, Action onMapComplete = null)
    {
        DynamicGlobalSettings.playing = true;
        bool ok = await StartLoadScene(PlayManager.instance.GetStoryCacheFolder(), sceneId, onMapComplete);


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
        try
        {
            LoadingManager.instance.SetProgress("scene", 0.05f);
            MapInfo data = await Task.Run(() =>
            {
                return GameManager.instance.saveCtrl.LoadSceneMap(storyFolder + "/" + GetSceneFileNameById(id));
            });
            LoadingManager.instance.SetProgress("scene", 0.4f);

            data.mainData.viewSize = new Vector3Int((int)(InputManager.instance.screenWorldSize.x / 2) + 4 + 1, 5, (int)(InputManager.instance.screenWorldSize.y / 2) + 4 + 1);

            MapManager.instance.Begin(data);
            LoadingManager.instance.SetProgress("scene", 0.6f);
            // All visual candidate columns must exist before resolving WangTile
            // collision masks, including off-screen Objects loaded from the save.
            // 这一段按 Object 数量推进 0.6 -> 0.95 的进度。
            var wangTileProducts = ObjectProductForm.DataByUid.Values
                .Where(product => product.protoUid == 0 && (product.boundsCollision || !product.centerCollider))
                .ToList();
            for (int i = 0; i < wangTileProducts.Count; i++)
            {
                GameManager.instance.mapCtrl.RefreshObjectWangTileAppearances(wangTileProducts[i].uid);
                if ((i & 7) == 0 || i == wangTileProducts.Count - 1)
                    LoadingManager.instance.SetProgress("scene", 0.6f + 0.35f * (i + 1) / wangTileProducts.Count);
            }
            LoadingManager.instance.SetProgress("scene", 0.95f);
            onMapComplete?.Invoke();
            LoadingManager.instance.SetProgress("scene", 1f);

            return true;
        }
        finally
        {
            LoadingManager.instance.RemoveLoadItem("scene");
        }
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
