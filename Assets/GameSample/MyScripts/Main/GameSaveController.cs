using Form;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RenderHeads.Media.AVProVideo;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.WSA;
using Z_DataSystem;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Map;
using Z_Texture;
using Z_UnitSystem;

public class GameSaveController : Z_Controller<GameManager>
{
    public string mapTextureFormFileName => "mtf";
    public string mapMaskFormFileName => "mmf";
    public string mapObjectFormFileName => "mof";
    public string storyFormFileName => "sf";
    public string characterParamFormFileName => "cpaf";
    public string skillParamFormFileName => "spaf";

    public string mapObjectParamFormFileName => "opaf";
    public string sceneFormFileName => "saf";
    public string characterProductFormFileName => "cprf";
    public string eventFormFileName => "ef";
    public string itemParamFormFileName => "ipaf";
    public string effectFormFileName => "etf";
    public string skillProductFormFileName => "splf";
    public string itemProductFormFileName => "iprf";
    public string imageUiItemFormFileName => "iuif";

    public string progressFormFileName => "pf";
    public string imageAssetFormFileName => "iaff";
    public string audioAssetFormFileName => "aaff";
    public string videoAssetFormFileName => "vaff";
    public string labFormFileName => "lf";
    public string missionFormFileName => "msf";

    public string assetFolder => "ast/";
    public GameSaveController(GameManager super) : base(super)
    {
    }

    #region package

    public byte[] Package(int storyId)
    {
        Texture2D icon;
        try
        { 
            icon = (Texture2D)AssetManager.instance.texCtrl.CreateDataByBytes(GameManager.instance.curStory.icon.ToArray(),"tmp").GetTex();
        }
        catch (Exception e)
        {
            icon = Texture2D.whiteTexture;
        }
        return TextureHelper.GetPNGWithExtraInfo(icon, System.Text.Encoding.UTF8.GetBytes(SaveAndLoad.Package(Main2StoryManager.GetStoryCoreFolder(storyId))));
    }

    #endregion

    #region unpackage

    public void Unpackage(byte[] content, int storyId)
    {
        SaveAndLoad.Unpackage(System.Text.Encoding.UTF8.GetString(TextureHelper.GetExtraInfoByPNG(content)), Main2StoryManager.GetStoryCoreFolder(storyId));
    }

    #endregion

    #region save
    public void SaveSceneMap(string scenePath)
    {
        if (MapManager.instance.data != null)
        {
            SaveAndLoad.Save(scenePath, JsonConvert.SerializeObject(MapManager.instance.data.GetJsonData()));
        }
    }
    public void SaveSceneMap(string scenePath, MapInfo data)
    {
        SaveAndLoad.Save(scenePath, JsonConvert.SerializeObject(data.GetJsonData()));
    }
    public void SaveSaveStory(int id)
    {
        string path = Main2StoryManager.GetStorySaveFolder(id);

        SaveLab(path);
        SaveMaterial(path);
        SaveObject(path);
        SaveCharacter(path);
        SaveSkill(path);
        SaveItem(path);
        SaveEffect(path);
        SaveMission(path);
        SaveEvent(path, null);
        SaveConfig(path);
        SaveScene(path);

        SaveUiItem(path);
        SaveProgress(path);

        SaveSceneMap(PlayManager.instance.GetSceneCacheFileName());

        CopyMapScene(Main2StoryManager.GetStoryCacheFolder(id), path);
    }
    public void SaveCoreStory(int id)
    {
        SaveOverview(id);

        string path = Main2StoryManager.GetStoryCoreFolder(id);

        SaveLab(path);
        SaveMaterial(path);
        SaveObject(path);
        SaveCharacter(path);
        SaveSkill(path);
        SaveItem(path);
        SaveEffect(path);
        SaveMission(path);
        SaveEvent(path);
        SaveConfig(path);
        SaveScene(path);

        foreach (var data in SceneForm.DataByUid.Values)
        {
            if (!SaveAndLoad.Exist(path + Main2StoryManager.GetSceneFileNameById(data.uid)))
            {
                var mapData = new GameMapData();
                var dic = GameManager.instance.innerAssetDic;
                mapData.Init((GameObjectAssetForm.Data)dic["map"], (GameObjectAssetForm.Data)dic["mapslope"], (GameObjectAssetForm.Data)dic["img"], (GameObjectAssetForm.Data)dic["canvas"],(TexAssetForm.Data) dic["defaultTileTexture"]);
                GameManager.instance.saveCtrl.SaveSceneMap(path + Main2StoryManager.GetSceneFileNameById(data.uid), mapData);
            }
        }

        SaveAndLoad.Delete(Main2StoryManager.GetStorySaveFolder(id));
        SaveAssets(path);
    }
    public void SaveOverview(int id)
    {
        var storyCoreFolder = Main2StoryManager.GetStoryCoreFolder(id);
        SaveAndLoad.Save(storyCoreFolder + "/" + storyFormFileName, StoryForm.GetJoByData(StoryForm.DataById[id]).ToString());

    }

    public void SaveLab(string storyFolder)
    {
        SaveAndLoad.Save(storyFolder + "/" + labFormFileName, LabForm.GetJaByDatas().ToString());
    }

    public void SaveMaterial(string storyCoreFolder)
    {

        SaveAndLoad.Save(storyCoreFolder + "/" + mapTextureFormFileName, MapTextureForm.GetJaByDatas().ToString());

        SaveAndLoad.Save(storyCoreFolder + "/" + mapMaskFormFileName, MapMaskForm.GetJaByDatas().ToString());


    }
    public void SaveObject(string storyCoreFolder)
    {
        SaveAndLoad.Save(storyCoreFolder + "/" + mapObjectParamFormFileName, MapObjectParamForm.GetJaByDatas().ToString());

        SaveAndLoad.Save(storyCoreFolder + "/" + mapObjectFormFileName, MapObjectForm.GetJaByDatas().ToString());

    }

    public void SaveCharacter(string storyCoreFolder)
    {
        SaveAndLoad.Save(storyCoreFolder + "/" + characterParamFormFileName, CharacterParamForm.GetJaByDatas().ToString());

        SaveAndLoad.Save(storyCoreFolder + "/" + characterProductFormFileName, CharacterProductForm.GetJaByDatas().ToString());

    }
    public void SaveSkill(string storyCoreFolder)
    {
        SaveAndLoad.Save(storyCoreFolder + "/" + skillParamFormFileName, SkillParamForm.GetJaByDatas().ToString());
        SaveAndLoad.Save(storyCoreFolder + "/" + skillProductFormFileName, SkillProductForm.GetJaByDatas().ToString());

    }
    public void SaveItem(string storyCoreFolder)
    {
        SaveAndLoad.Save(storyCoreFolder + "/" + itemParamFormFileName, ItemParamForm.GetJaByDatas().ToString());
        SaveAndLoad.Save(storyCoreFolder + "/" + itemProductFormFileName, ItemProductForm.GetJaByDatas().ToString());
    }
    public void SaveEffect(string storyCoreFolder)
    {
        SaveAndLoad.Save(storyCoreFolder + "/" + effectFormFileName, EffectForm.GetJaByDatas().ToString());

    }

    public void SaveMission(string storyCoreFolder)
    {
        SaveAndLoad.Save(storyCoreFolder + "/" + missionFormFileName, MissionForm.GetJaByDatas().ToString());
    }

    public void SaveEvent(string storyCoreFolder, EventProgramDataForm.Data data = null)
    {
        SaveAndLoad.Save(storyCoreFolder + "/" + eventFormFileName, EventProgramDataForm.GetJaByDatas().ToString());



    }

    public void SaveConfig(string storyFolder)
    {
        SaveAndLoad.Save(storyFolder + "/" + progressFormFileName, ProgressForm.GetJaByDatas().ToString());
    }

    public void SaveProgress(string storyFolder)
    {
        SaveAndLoad.Save(storyFolder + "/" + progressFormFileName, ProgressForm.GetJaByDatas().ToString());
    }
    public void SaveUiItem(string storySaveFolder)
    {
        SaveAndLoad.Save(storySaveFolder + "/" + imageUiItemFormFileName, ImageUiItemForm.GetJaByDatas().ToString());
    }
    public void SaveScene(string storyCoreFolder)
    {
        SaveAndLoad.Save(storyCoreFolder + "/" + sceneFormFileName, SceneForm.GetJaByDatas().ToString());

    }
    public void SaveAssets(string storyCoreFolder)
    {

        SaveAndLoad.Save(storyCoreFolder + "/" + imageAssetFormFileName, StoryTexAssetForm.GetJaByDatas().ToString());
        SaveAndLoad.Save(storyCoreFolder + "/" + audioAssetFormFileName, StoryAudioAssetForm.GetJaByDatas().ToString());
        SaveAndLoad.Save(storyCoreFolder + "/" + videoAssetFormFileName, StoryVideoAssetForm.GetJaByDatas().ToString());

        foreach (var data in StoryTexAssetForm.DataById.Values)
        {
            SaveStoryTex(data.id, storyCoreFolder);
        }
        foreach (var data in StoryAudioAssetForm.DataById.Values)
        {
            SaveStoryAudio(data.id, storyCoreFolder);
        }
        foreach (var data in StoryVideoAssetForm.DataById.Values)
        {
            SaveStoryVideo(data.id, storyCoreFolder);
        }
        SaveAndLoad.EachFile(storyCoreFolder + assetFolder, (name) =>
        {
            var id1 = AssetManager.instance.texCtrl.GetId(name);
            var id2 = AssetManager.instance.audioCtrl.GetId(name);
            var id3 = AssetManager.instance.videoCtrl.GetId(name);
            if (!StoryTexAssetForm.DataById.ContainsKey(id1) && !StoryAudioAssetForm.DataById.ContainsKey(id2) && !StoryVideoAssetForm.DataById.ContainsKey(id3))
            {
                SaveAndLoad.Delete(storyCoreFolder + assetFolder + name);
            }
        });
    }
    #region util
    private void SaveTex(int texId, string path)
    {
        var data = TexAssetForm.DataById.GetDv(texId, null);
        if (data != null && data.bytes != null && !GlobalDefaultHelper.IsInnerAssetName(texId))
        {

            path = path + assetFolder + texId;
            if (!SaveAndLoad.Exist(path))
            {
                SaveAndLoad.Save(path, data.bytes);
            }

        }
    }
    private void SaveStoryTex(int texId, string path)
    {
        var data = TexAssetForm.DataById.GetDv(texId, null);
        if (data != null && data.bytes != null && !GlobalDefaultHelper.IsInnerAssetName(texId))
        {
            var tex = StoryTexAssetForm.DataById[texId];
            path = path + assetFolder + AssetManager.instance.texCtrl.GetName(texId);
            if (!SaveAndLoad.Exist(path))
            {
                SaveAndLoad.Save(path, data.bytes);
            }
        }
    }
    private void SaveStoryVideo(int videoId, string path)
    {
        var data = VideoAssetForm.DataById.GetDv(videoId, null);
        if (data != null && !GlobalDefaultHelper.IsInnerAssetName(videoId))
        {
            var tex = VideoAssetForm.DataById[videoId];
            path = path + assetFolder + AssetManager.instance.videoCtrl.GetName(videoId);
            if (!SaveAndLoad.Exist(path))
            {
                SaveAndLoad.Copy(data.path, path);
            }
        }
    }
    private void SaveStoryAudio(int audioId, string path)
    {
        var data = AudioAssetForm.DataById.GetDv(audioId, null);
        if (data != null && !GlobalDefaultHelper.IsInnerAssetName(audioId))
        {
            var tex = AudioAssetForm.DataById[audioId];
            path = path + assetFolder + AssetManager.instance.audioCtrl.GetName(audioId);
            if (!SaveAndLoad.Exist(path))
            {
                SaveAndLoad.Copy(data.path, path);
            }
        }
    }
    #endregion
    #endregion

    #region load
    public void LoadCoreStory(int id)
    {
        ResetStory();
        var folder = Main2StoryManager.GetStoryCoreFolder(id);
        var assetFolder = Main2StoryManager.GetStoryAssetFolder(id);
        LoadLab(folder);
        LoadScene(folder);
        LoadMaterial(folder);
        LoadObject(folder);
        LoadCharacter(folder);
        LoadSkill(folder);

        LoadItem(folder);
        LoadEffect(folder);
        LoadMission(folder);

        LoadEvent(folder);
        LoadProgress(folder);

        DeleteCache(id);
        CopyMapScene(folder, Main2StoryManager.GetStoryCacheFolder(id));
        LoadAsset(assetFolder);
        RepairMissingCharacterTextureReferences();
    }
    public void ResetStory()
    {
        LoadScene(null);
        LoadMaterial(null);
        LoadObject(null);
        LoadCharacter(null);
        LoadSkill(null);

        LoadItem(null);
        LoadEffect(null);
        LoadMission(null);

        LoadEvent(null);
        LoadProgress(null);
        LoadUiItem(null);
        LoadAsset(null);
        LoadLab(null);
    }
    public void LoadSaveStory(int id)
    {
        ResetStory();
        var folder = Main2StoryManager.GetStorySaveFolder(id);

        var assetFolder = Main2StoryManager.GetStoryAssetFolder(id);
        LoadLab(folder);
        LoadScene(folder);
        LoadMaterial(folder);
        LoadObject(folder);
        LoadCharacter(folder);
        LoadSkill(folder);

        LoadItem(folder);
        LoadEffect(folder);
        LoadMission(folder);

        LoadEvent(folder);
        LoadProgress(folder);
        //manage Scene
        DeleteCache(id);
        if (LackMapScene(folder))
        {
            CopyMapScene(Main2StoryManager.GetStoryCoreFolder(id), folder);
        }
        CopyMapScene(folder, Main2StoryManager.GetStoryCacheFolder(id));
        //
        LoadUiItem(folder);
        LoadAsset(assetFolder);
        RepairMissingCharacterTextureReferences();

    }

    public void LoadOverview()
    {
        StoryForm.Clear();
        string[] allDirectories = Directory.GetDirectories(SaveAndLoad.perPath);
        foreach (string dir in allDirectories)
        {
            var coreFolder = Main2StoryManager.GetStoryCoreFolder(dir);
            if (int.TryParse(Path.GetFileName(dir), out int id) && SaveAndLoad.Exist(coreFolder + storyFormFileName))
            {
                var form = StoryForm.GetDataByJo(JObject.Parse(SaveAndLoad.Load<string>(coreFolder + storyFormFileName)));
                form.id = id;//矫正
                StoryForm.AddData(form);
            }
        }
    }

    public void LoadLab(string folder)
    {
        LabForm.ClearAuto();
        if (folder == null)
            return;

        var pathForm = folder + labFormFileName;
        if (SaveAndLoad.Exist(pathForm))
        {
            foreach (var form in LabForm.GetDatasByJa(JArray.Parse(SaveAndLoad.Load<string>(pathForm))))
            {
                LabForm.AddData(form);
            }
        }
    }

    private static void MigrateFlatLabIds(JArray forms, string belong, params string[] legacyKeys)
    {
        foreach (JObject form in forms)
        {
            if (TryNormalizeCurrentLabId(form, belong))
                continue;

            form["labId"] = LabForm.NoneId;
            foreach (var legacyKey in legacyKeys)
            {
                if (!form.TryGetValue(legacyKey, out var legacy) || legacy.Type == JTokenType.Null)
                    continue;

                if (legacy.Type == JTokenType.Integer)
                    form["labId"] = LabForm.GetOrCreateForBelong(legacy.Value<int>(), belong);
                else
                    form["labId"] = LabForm.GetOrCreate(legacy.Value<string>(), belong);
                break;
            }

        }
    }

    private static void MigrateHierarchicalLabIds(JArray forms, string belong, string lv1Key, string lv2Key)
    {
        foreach (JObject form in forms)
        {
            if (TryNormalizeCurrentLabId(form, belong))
                continue;

            var hasLv1 = form.TryGetValue(lv1Key, out var lv1Token) && lv1Token.Type != JTokenType.Null;
            var hasLv2 = form.TryGetValue(lv2Key, out var lv2Token) && lv2Token.Type != JTokenType.Null;
            if (!hasLv1 && !hasLv2)
            {
                form["labId"] = LabForm.NoneId;
                continue;
            }

            form["labId"] = LabForm.GetOrCreate(
                hasLv1 ? lv1Token.Value<string>() : string.Empty,
                hasLv2 ? lv2Token.Value<string>() : string.Empty,
                string.Empty,
                belong);
        }
    }

    private static bool TryNormalizeCurrentLabId(JObject form, string belong)
    {
        if (!form.TryGetValue("labId", out var current) || current.Type != JTokenType.Integer)
            return false;

        var labId = current.Value<int>();
        if (labId == LabForm.NoneId)
            return true;
        if (!LabForm.TryGetData(labId, out _))
            return false;

        form["labId"] = LabForm.GetOrCreateForBelong(labId, belong);
        return true;
    }

    public void LoadMaterial(string folder)
    {
        MapTextureForm.ClearAuto();
        MapMaskForm.ClearAuto();
        var pathForm = folder + mapTextureFormFileName;
        if (SaveAndLoad.Exist(pathForm))
        {
            var forms = JArray.Parse(SaveAndLoad.Load<string>(pathForm));
            MigrateFlatLabIds(forms, nameof(MapTextureForm), "label");
            foreach (var form in MapTextureForm.GetDatasByJa(forms))
            {
                MapTextureForm.AddData(form);
            }
        }



        pathForm = folder + mapMaskFormFileName;
        if (SaveAndLoad.Exist(pathForm))
        {
            var forms = JArray.Parse(SaveAndLoad.Load<string>(pathForm));
            MigrateFlatLabIds(forms, nameof(MapMaskForm), "label");
            foreach (var form in MapMaskForm.GetDatasByJa(forms))
            {
                MapMaskForm.AddData(form);
            }
        }

    }
    public void LoadObject(string folder)
    {
        var pathForm = folder + mapObjectFormFileName;
        MapObjectForm.ClearAuto();
        if (SaveAndLoad.Exist(pathForm))
        {
            var forms = JArray.Parse(SaveAndLoad.Load<string>(pathForm));
            MigrateFlatLabIds(forms, nameof(MapObjectForm), "label");
            foreach (var form in MapObjectForm.GetDatasByJa(forms))
            {
                MapObjectForm.AddData(form);
            }
        }

    }

    public void LoadCharacter(string folder)
    {
        var pathForm = folder + characterParamFormFileName;
        CharacterParamForm.ClearAuto();
        if (SaveAndLoad.Exist(pathForm))
        {
            foreach (var form in CharacterParamForm.GetDatasByJa(JArray.Parse(SaveAndLoad.Load<string>(pathForm))))
            {
                CharacterParamForm.AddData(form);
            }
        }

        pathForm = folder + characterProductFormFileName;
        CharacterProductForm.ClearAuto();
        if (SaveAndLoad.Exist(pathForm))
        {
            var forms = JArray.Parse(SaveAndLoad.Load<string>(pathForm));
            MigrateFlatLabIds(forms, nameof(CharacterProductForm), "label");
            foreach (var form in CharacterProductForm.GetDatasByJa(forms))
            {
                CharacterProductForm.AddData(form);
            }
        }

        foreach (var data in CharacterProductForm.DataByUid.Values)
        {
            var icon = data.avatarTex;

            foreach (var anim in data.animDic.Values)
            {
                foreach (BodyPartType part in Enum.GetValues(typeof(BodyPartType)))
                {
                    foreach (AnimDirecton dir in Enum.GetValues(typeof(AnimDirecton)))
                    {
                        if (!anim.animClip.ContainsKey(dir))
                        {
                            anim.animClip[dir] = new List<CharacterAnimClipForm.Data>();
                        }


                    }
                }
            }
        }
    }
    public void LoadSkill(string folder)
    {
        var pathForm = folder + skillParamFormFileName;
        SkillParamForm.ClearAuto();
        if (SaveAndLoad.Exist(pathForm))
        {
            foreach (var form in SkillParamForm.GetDatasByJa(JArray.Parse(SaveAndLoad.Load<string>(pathForm))))
            {
                SkillParamForm.AddData(form);
            }
        }

        pathForm = folder + skillProductFormFileName;
        SkillProductForm.ClearAuto();
        if (SaveAndLoad.Exist(pathForm))
        {
            var forms = JArray.Parse(SaveAndLoad.Load<string>(pathForm));
            MigrateFlatLabIds(forms, nameof(SkillProductForm), "label");
            foreach (var form in SkillProductForm.GetDatasByJa(forms))
            {
                SkillProductForm.AddData(form);
            }
        }


    }
    public void LoadItem(string folder)
    {
        var pathForm = folder + itemParamFormFileName;
        ItemParamForm.ClearAuto();
        if (SaveAndLoad.Exist(pathForm))
        {
            foreach (var form in ItemParamForm.GetDatasByJa(JArray.Parse(SaveAndLoad.Load<string>(pathForm))))
            {
                ItemParamForm.AddData(form);
            }
        }

        pathForm = folder + itemProductFormFileName;
        ItemProductForm.ClearAuto();
        if (SaveAndLoad.Exist(pathForm))
        {
            var forms = JArray.Parse(SaveAndLoad.Load<string>(pathForm));
            MigrateFlatLabIds(forms, nameof(ItemProductForm), "label");
            foreach (var form in ItemProductForm.GetDatasByJa(forms))
            {
                ItemProductForm.AddData(form);
            }
        }


    }

    public void LoadEffect(string folder)
    {
        var pathForm = folder + effectFormFileName;
        EffectForm.ClearAuto();
        if (SaveAndLoad.Exist(pathForm))
        {
            var forms = JArray.Parse(SaveAndLoad.Load<string>(pathForm));
            MigrateFlatLabIds(forms, nameof(EffectForm), "label");
            foreach (var form in EffectForm.GetDatasByJa(forms))
            {
                EffectForm.AddData(form);
            }
        }
    }

    public void LoadMission(string folder)
    {
        MissionForm.ClearAuto();
        if (folder == null)
            return;

        var pathForm = folder + missionFormFileName;
        if (SaveAndLoad.Exist(pathForm))
        {
            var forms = JArray.Parse(SaveAndLoad.Load<string>(pathForm));
            MigrateFlatLabIds(forms, nameof(MissionForm), "label", "lab");
            foreach (var form in MissionForm.GetDatasByJa(forms))
            {
                MissionForm.AddData(form);
            }
        }
    }

    public void LoadAsset(string folder)
    {
        var path = "";
        path = folder + imageAssetFormFileName;
        StoryTexAssetForm.ClearAuto();
        StoryAudioAssetForm.ClearAuto();
        StoryVideoAssetForm.ClearAuto();
        if (SaveAndLoad.Exist(path))
        {
            var forms = JArray.Parse(SaveAndLoad.Load<string>(path));
            MigrateFlatLabIds(forms, nameof(StoryTexAssetForm), "lab");
            foreach (var form in StoryTexAssetForm.GetDatasByJa(forms))
            {
                if (form != null)
                {
                    StoryTexAssetForm.AddData(form);
                    form.path = SaveAndLoad.GetRealPath(folder + assetFolder + AssetManager.instance.texCtrl.GetName(form.id));
                }
            }
        }
        path = folder + audioAssetFormFileName;
        if (SaveAndLoad.Exist(path))
        {
            var forms = JArray.Parse(SaveAndLoad.Load<string>(path));
            MigrateFlatLabIds(forms, nameof(StoryAudioAssetForm), "lab");
            foreach (var form in StoryAudioAssetForm.GetDatasByJa(forms))
            {
                StoryAudioAssetForm.AddData(form);
                form.path = SaveAndLoad.GetRealPath(folder + assetFolder + AssetManager.instance.audioCtrl.GetName(form.id));
            }
        }
        path = folder + videoAssetFormFileName;
        if (SaveAndLoad.Exist(path))
        {
            var forms = JArray.Parse(SaveAndLoad.Load<string>(path));
            MigrateFlatLabIds(forms, nameof(StoryVideoAssetForm), "lab");
            foreach (var form in StoryVideoAssetForm.GetDatasByJa(forms))
            {
                StoryVideoAssetForm.AddData(form);
                form.path = SaveAndLoad.GetRealPath(folder + assetFolder + AssetManager.instance.videoCtrl.GetName(form.id));
            }
        }

    }

    public void LoadEvent(string folder)
    {
        var pathForm = folder + eventFormFileName;

        EventProgramDataForm.ClearAuto();
        if (SaveAndLoad.Exist(pathForm))
        {
            var forms = JArray.Parse(SaveAndLoad.Load<string>(pathForm));
            MigrateHierarchicalLabIds(forms, nameof(EventProgramDataForm), "category", "type");
            foreach (var form in EventProgramDataForm.GetDatasByJa(forms))
            {
                EventProgramDataForm.AddData(form);
            }
        }


    }
    public MapInfo LoadSceneMap(string scenePath)
    {
        var mapData = new GameMapData();
        mapData.Init(SaveAndLoad.Load<string>(scenePath));
        return mapData;
    }
    public void LoadProgress(string folder)
    {
        var pathForm = folder + progressFormFileName;

        if (SaveAndLoad.Exist(pathForm))
        {
            foreach (var form in ProgressForm.GetDatasByJa(JArray.Parse(SaveAndLoad.Load<string>(pathForm))))
            {
                ProgressForm.AddData(form);
            }
            DynamicGlobalSettings.cameraMode = GameManager.instance.curProgress.cameraMode;
        }
    }


    public void LoadUiItem(string folder)
    {
        var pathForm = folder + imageUiItemFormFileName;
        ImageUiItemForm.Clear();
        if (SaveAndLoad.Exist(pathForm))
        {
            foreach (var form in ImageUiItemForm.GetDatasByJa(JArray.Parse(SaveAndLoad.Load<string>(pathForm))))
            {
                ImageUiItemForm.AddData(form);
            }
        }
    }

    public void LoadScene(string folder)
    {
        var pathForm = folder + sceneFormFileName;
        SceneForm.ClearAuto();
        if (SaveAndLoad.Exist(pathForm))
        {
            foreach (var form in SceneForm.GetDatasByJa(JArray.Parse(SaveAndLoad.Load<string>(pathForm))))
            {
                SceneForm.AddData(form);
            }
        }
    }

    public void LoadLocalModStory()
    {

    }
    #region util


    public void RepairMissingCharacterTextureReferences()
    {
        const int missingPartTextureId = int.MinValue;
        var missingReferences = new List<(Dictionary<BodyPartType, int> partTex, BodyPartType part, int missingId)>();

        foreach (var character in CharacterProductForm.DataByUid.Values)
        {
            if (character.animDic == null)
                continue;

            foreach (var anim in character.animDic.Values)
            {
                if (anim?.animClip == null)
                    continue;

                foreach (var clips in anim.animClip.Values)
                {
                    if (clips == null)
                        continue;

                    foreach (var clip in clips)
                    {
                        if (clip == null)
                            continue;
                        if (clip.partTex == null)
                            clip.partTex = new Dictionary<BodyPartType, int>();

                        foreach (BodyPartType part in Enum.GetValues(typeof(BodyPartType)))
                        {
                            var hasTextureId = clip.partTex.TryGetValue(part, out var textureId);
                            if (!hasTextureId || !TexAssetForm.DataById.ContainsKey(textureId))
                            {
                                missingReferences.Add((
                                    clip.partTex,
                                    part,
                                    hasTextureId ? textureId : missingPartTextureId));
                            }
                        }
                    }
                }
            }
        }

        if (missingReferences.Count == 0)
            return;

        var replacements = new Dictionary<int, int>();
        var emptyTextureBytes = TextureHelper.GetTextureByte(TextureHelper.transparentTexture);
        foreach (var missingReference in missingReferences)
        {
            if (!replacements.TryGetValue(missingReference.missingId, out var replacementId))
            {
                var missingName = missingReference.missingId == missingPartTextureId
                    ? "missing_texture"
                    : $"missing_texture_{missingReference.missingId}";
                var rawData = AssetManager.instance.texCtrl.CreateDataByBytes(emptyTextureBytes, missingName);
                rawData.labId = LabForm.NoneId;
                var replacement = AddStoryTexInternal(rawData);
                replacementId = replacement.id;
                replacements[missingReference.missingId] = replacementId;
            }

            missingReference.partTex[missingReference.part] = replacementId;
        }
    }

    private StoryTexAssetForm.Data AddStoryTexInternal(TexAssetForm.Data rawData)
    {
        var data = new StoryTexAssetForm.Data(rawData);
        if (StoryTexAssetForm.DataById.ContainsKey(data.id))
        {
            var oldData = StoryTexAssetForm.DataById[data.id];
            StoryTexAssetForm.RemoveData(oldData.id);
        }
        data.labId = LabForm.GetOrCreateForBelong(data.labId, nameof(StoryTexAssetForm));
        StoryTexAssetForm.AddData(data);
        return data;
    }

    public void AddStoryTex(ref TexAssetForm.Data rawData)
    {
        var data = AddStoryTexInternal(rawData);
        rawData = data;

        Z_EventHelper.Invoke(new AssetEvent()
        {
            importAssetName = data.name
        });

    }
    public void AddGameTex(ref TexAssetForm.Data rawData)
    {
        GameTexAssetForm.Data data = new GameTexAssetForm.Data(rawData);
        if (GameTexAssetForm.DataById.ContainsKey(data.id))
        {
            var oldData = GameTexAssetForm.DataById[data.id];
            GameTexAssetForm.RemoveData(oldData.id);
        }
        data.labId = LabForm.GetOrCreateForBelong(data.labId, nameof(GameTexAssetForm));
        GameTexAssetForm.AddData(data);

        Z_EventHelper.Invoke(new AssetEvent()
        {
            importAssetName = data.name
        });
        rawData = data; 
    }

    public void AddStoryAudio(AudioAssetForm.Data rawData)
    {
        StoryAudioAssetForm.Data data = new StoryAudioAssetForm.Data(rawData);
        if (StoryAudioAssetForm.DataById.ContainsKey(data.id))
        {
            var oldData = StoryAudioAssetForm.DataById[data.id];
            StoryAudioAssetForm.RemoveData(oldData.id);
        }
        data.labId = LabForm.GetOrCreateForBelong(data.labId, nameof(StoryAudioAssetForm));
        StoryAudioAssetForm.AddData(data);

        Z_EventHelper.Invoke(new AssetEvent()
        {
            importAssetName = data.name
        });
    }

    public void AddStoryVideo(VideoAssetForm.Data rawData)
    {
        StoryVideoAssetForm.Data data = new StoryVideoAssetForm.Data(rawData);
        if (StoryVideoAssetForm.DataById.ContainsKey(data.id))
        {
            var oldData = StoryVideoAssetForm.DataById[data.id];
            StoryVideoAssetForm.RemoveData(oldData.id);
        }
        data.labId = LabForm.GetOrCreateForBelong(data.labId, nameof(StoryVideoAssetForm));
        StoryVideoAssetForm.AddData(data);

        Z_EventHelper.Invoke(new AssetEvent()
        {
            importAssetName = data.name
        });
    }
    #endregion
    #endregion

    #region copy
    public void CopyMapScene(string from, string to)
    {
        foreach (var data in SceneForm.DataByUid.Values)
        {
            SaveAndLoad.Copy(from + Main2StoryManager.GetSceneFileNameById(data.uid), to + Main2StoryManager.GetSceneFileNameById(data.uid));
        }
    }

    #endregion

    #region check
    public bool LackMapScene(string folder)
    {
        foreach (var data in SceneForm.DataByUid.Values)
        {
            if (!SaveAndLoad.Exist(folder + Main2StoryManager.GetSceneFileNameById(data.uid)))
            {
                return true;
            }
        }
        return false;
    }
    #endregion

    #region del
    public void DeleteCache(int id)
    {
        SaveAndLoad.Delete(Main2StoryManager.GetStoryCacheFolder(id));
    }
    public void DeleteSceneMap(string storyCoreFolder, int id)
    {

        SaveAndLoad.Delete(storyCoreFolder + "/" + id);
    }

    #endregion

    public void ResetPrefabPool()
    {
        InstancePoolManager.instance.Clear();
        foreach (Transform child in InstancePoolManager.instance.defaultRoot)
        {
            GameObject.Destroy(child.gameObject); // 销毁子物体
        }
        foreach (var form in GameObjectAssetForm.DataById.Values)
        {
            if (form.name.StartsWith(GlobalDefaultHelper.GetInternalPrefabName("")))
            {
                InstancePoolManager.instance.AddPool(form.GetGo());
            }
        }
        foreach (var form in MapObjectForm.DataById.Values)
        {
            var obj = _super.utilCtrl.CombineNewObjectByPrefabs(GlobalDefaultHelper.GetRuntimeMapObjectPrefabName(form.id), form.model, true);
            obj.transform.parent = InstancePoolManager.instance.defaultRoot;
            InstancePoolManager.instance.AddPool(obj);
        }

        //item生成mapItem
        MapItemForm.Clear();
        if (ItemProductForm.DatasByProtouid.ContainsKey(0))
        {
            foreach (var itemData in ItemProductForm.DatasByProtouid[0])
            {
                var labId = LabForm.GetOrCreateForBelong(itemData.labId, nameof(MapItemForm));
                MapItemForm.AddData(new MapItemForm.Data(-1, itemData.name, itemData.iconTexName, itemData.model, labId, itemData.uid));
            }
        }

        //character生成mapCharacter
        MapCharacterForm.Clear();
        if (CharacterProductForm.DatasByProtouid.ContainsKey(0))
        {
            foreach (var characterData in CharacterProductForm.DatasByProtouid[0])
            {
                var labId = LabForm.GetOrCreateForBelong(characterData.labId, nameof(MapCharacterForm));
                MapCharacterForm.AddData(new MapCharacterForm.Data(-1, characterData.name, characterData.avatarTex, labId, characterData.uid));
            }
        }

        foreach (var form in ItemProductForm.DataByUid.Values)
        {
            var obj = _super.utilCtrl.CombineNewObjectByPrefabs(GlobalDefaultHelper.GetRuntimeMapItemPrefabName(form.uid), form.model, true, true);
            obj.transform.parent = InstancePoolManager.instance.defaultRoot;
            InstancePoolManager.instance.AddPool(obj);
        }

        var character = _super.utilCtrl.CombineNewCharacterByPrefabs(GlobalDefaultHelper.GetRuntimePrefabName("character"), new List<int>() { GlobalDefaultHelper.DefaultTexId, GlobalDefaultHelper.DefaultTexId, -1 }, true);
        character.transform.parent = InstancePoolManager.instance.defaultRoot;
        InstancePoolManager.instance.AddPool(character);

    }


}
