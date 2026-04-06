using System;
using System.Collections.Generic;
using System.IO;
using Form;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

    public string assetFolder => "ast/";
    public GameSaveController(GameManager super) : base(super)
    {
    }

    #region package

    public void Package(int storyId)
    {
        var icon = Texture2D.whiteTexture;
        if (TexAssetForm.DataByName.ContainsKey(GameManager.instance.curStory.icon))
        {
            icon = (Texture2D)TexAssetForm.DataByName[GameManager.instance.curStory.icon].GetTex();
        }
        File.WriteAllBytes(AssetManager.externPatn + storyId + ".png", TextureHelper.GetPNGWithExtraInfo(icon, System.Text.Encoding.UTF8.GetBytes(SaveAndLoad.Package(Main2StoryManager.GetStoryCoreFolder(storyId)))));
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

        SaveMaterial(path, true);
        SaveObject(path, true);
        SaveCharacter(path, true);
        SaveSkill(path, true);
        SaveItem(path, true);
        SaveEffect(path, true);
        SaveEvent(path, null, true);
        SaveConfig(path, true);
        SaveScene(path, true);

        SaveUiItem(path);
        SaveProgress(path);

        SaveSceneMap(PlayManager.instance.GetSceneCacheFileName());

        CopyMapScene(Main2StoryManager.GetStoryCacheFolder(id), path);
    }
    public void SaveCoreStory(int id)
    {
        SaveOverview(id);

        string path = Main2StoryManager.GetStoryCoreFolder(id);

        SaveMaterial(path);
        SaveObject(path);
        SaveCharacter(path);
        SaveSkill(path);
        SaveItem(path);
        SaveEffect(path);
        SaveEvent(path);
        SaveConfig(path);
        SaveScene(path);
        SaveAndLoad.Delete(Main2StoryManager.GetStorySaveFolder(id));
    }
    public void SaveOverview(int id)
    {
        var storyCoreFolder = Main2StoryManager.GetStoryCoreFolder(id);
        SaveAndLoad.Save(storyCoreFolder + "/" + storyFormFileName, StoryForm.GetJoByData(StoryForm.DataById[id]).ToString());
        foreach (var data in StoryForm.DataById.Values)
        {
            var nm = data.icon;
            SaveTex(nm, storyCoreFolder);

        }
    }

    public void SaveMaterial(string storyCoreFolder, bool noImage = false)
    {

        SaveAndLoad.Save(storyCoreFolder + "/" + mapTextureFormFileName, MapTextureForm.GetJaByDatas().ToString());

        SaveAndLoad.Save(storyCoreFolder + "/" + mapMaskFormFileName, MapMaskForm.GetJaByDatas().ToString());
        if (!noImage)
        {
            foreach (var data in MapTextureForm.DataById.Values)
            {
                for (int i = 0; i < data.texsName.Count; i++)
                {
                    var nm = data.texsName[i];
                    SaveStoryTex(nm, storyCoreFolder);
                }
            }

            foreach (var data in MapMaskForm.DataById.Values)
            {
                for (int i = 0; i < Enum.GetValues(typeof(AlphaTexBasic6)).Length; i++)
                {
                    var nm = data.texsName[i];
                    SaveStoryTex(nm, storyCoreFolder);
                }
            }
        }

    }
    public void SaveObject(string storyCoreFolder, bool noImage = false)
    {
        SaveAndLoad.Save(storyCoreFolder + "/" + mapObjectParamFormFileName, MapObjectParamForm.GetJaByDatas().ToString());

        SaveAndLoad.Save(storyCoreFolder + "/" + mapObjectFormFileName, MapObjectForm.GetJaByDatas().ToString());
        if (!noImage)
            foreach (var data in MapObjectForm.DataById.Values)
            {
                for (int i = 0; i < data.model.subUnitTexsName.Count; i++)
                {
                    var nm = data.model.subUnitTexsName[i];
                    SaveStoryTex(nm, storyCoreFolder);
                }
            }
    }

    public void SaveCharacter(string storyCoreFolder, bool noImage = false)
    {
        SaveAndLoad.Save(storyCoreFolder + "/" + characterParamFormFileName, CharacterParamForm.GetJaByDatas().ToString());

        SaveAndLoad.Save(storyCoreFolder + "/" + characterProductFormFileName, CharacterProductForm.GetJaByDatas().ToString());
        if (!noImage)
            foreach (var data in CharacterProductForm.DataByUid.Values)
            {
                var icon = data.avatarTexName;
                SaveStoryTex(icon, storyCoreFolder);
                SaveStoryTex(data.tachie, storyCoreFolder);
                foreach (var anim in data.animDic.Values)
                {

                    foreach (BodyPartType part in Enum.GetValues(typeof(BodyPartType)))
                    {
                        foreach (AnimDirecton dir in Enum.GetValues(typeof(AnimDirecton)))
                        {
                            for (int i = 0; i < anim.animClip[dir].Count; i++)
                            {
                                if (anim.animClip[dir][i].partTex.ContainsKey(part))
                                {
                                    var nm = anim.animClip[dir][i].partTex[part];
                                    SaveStoryTex(nm, storyCoreFolder);
                                }
                            }
                        }
                    }

                }
            }
    }
    public void SaveSkill(string storyCoreFolder, bool noImage = false)
    {
        SaveAndLoad.Save(storyCoreFolder + "/" + skillParamFormFileName, SkillParamForm.GetJaByDatas().ToString());
        SaveAndLoad.Save(storyCoreFolder + "/" + skillProductFormFileName, SkillProductForm.GetJaByDatas().ToString());
        if (!noImage)
            foreach (var data in SkillProductForm.DataByUid.Values)
            {
                SaveStoryTex(data.icon, storyCoreFolder);
            }


    }
    public void SaveItem(string storyCoreFolder, bool noImage = false)
    {
        SaveAndLoad.Save(storyCoreFolder + "/" + itemParamFormFileName, ItemParamForm.GetJaByDatas().ToString());

        SaveAndLoad.Save(storyCoreFolder + "/" + itemProductFormFileName, ItemProductForm.GetJaByDatas().ToString());
        if (!noImage)
            foreach (var data in ItemProductForm.DataByUid.Values)
            {
                var icon = data.iconTexName;

                SaveStoryTex(icon, storyCoreFolder);
                foreach (var nm in data.model.subUnitTexsName)
                {
                    SaveStoryTex(nm, storyCoreFolder);
                }
            }
    }
    public void SaveEffect(string storyCoreFolder, bool noImage = false)
    {
        SaveAndLoad.Save(storyCoreFolder + "/" + effectFormFileName, EffectForm.GetJaByDatas().ToString());
        if (!noImage)
            foreach (var data in EffectForm.DataByUid.Values)
            {
                foreach (var clip in data.clips)
                {
                    SaveStoryTex(clip[0].tex, storyCoreFolder);
                }
            }
    }

    public void SaveEvent(string storyCoreFolder, EventProgramDataForm.Data data = null, bool noImage = false)
    {
        SaveAndLoad.Save(storyCoreFolder + "/" + eventFormFileName, EventProgramDataForm.GetJaByDatas().ToString());

        SaveStoryTex("$i$$i$", storyCoreFolder);
        if (!noImage)
        {
            Action<EventProgramDataForm.Data> act = (data) =>
            {
                var imgs = data.code.Split(AssetDefines.IMAGE_MARK);//获取常量图片
                for (int i = 1; i < imgs.Length; i += 2)
                {
                    SaveStoryTex(AssetManager.instance.texCtrl.GetName(imgs[i]), storyCoreFolder);
                }
                var videos = data.code.Split(AssetDefines.VIDEO_MARK);//获取常量图片
                for (int i = 1; i < videos.Length; i += 2)
                {
                    SaveStoryVideo(AssetManager.instance.videoCtrl.GetName(videos[i]), storyCoreFolder);
                }
                var audios = data.code.Split(AssetDefines.AUDIO_MARK);//获取常量图片
                for (int i = 1; i < audios.Length; i += 2)
                {
                    SaveStoryAudio(AssetManager.instance.audioCtrl.GetName(audios[i]), storyCoreFolder);
                }
            };
            if (data != null)
            {
                act.Invoke(data);
            }
            else
            {
                foreach (var curData in EventProgramDataForm.DataByUid.Values)
                {
                    act.Invoke(curData);
                }
            }
        }


    }

    public void SaveConfig(string storyFolder, bool noImage = false)
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
    public void SaveScene(string storyCoreFolder, bool noImage = false)
    {
        SaveAndLoad.Save(storyCoreFolder + "/" + sceneFormFileName, SceneForm.GetJaByDatas().ToString());
        if (!noImage)
            foreach (var data in SceneForm.DataByUid.Values)
            {
                var icon = data.miniMap;
                SaveStoryTex(icon, storyCoreFolder);
            }
    }
    #region util
    private void SaveTex(string texName, string path)
    {
        var data = TexAssetForm.DataByName.GetDk(texName, null);
        if (data != null && data.bytes != null && !GlobalNameHelper.IsInnerAssetName(texName))
        {

            path = path + assetFolder + texName;
            if (!SaveAndLoad.Exist(path))
            {
                SaveAndLoad.Save(path, data.bytes);
            }

        }
    }
    private void SaveStoryTex(string texName, string path)
    {
        var data = TexAssetForm.DataByName.GetDv(texName, null);
        if (data != null && data.bytes != null && !GlobalNameHelper.IsInnerAssetName(texName))
        {
            var tex = StoryTexAssetForm.DataByName[texName];
            path = path + assetFolder + texName;
            if (!SaveAndLoad.Exist(path))
            {
                SaveAndLoad.Save(path, data.bytes);
            }
        }
    }
    private void SaveStoryVideo(string videoName, string path)
    {
        var data = VideoAssetForm.DataByName.GetDv(videoName, null);
        if (data != null && !GlobalNameHelper.IsInnerAssetName(videoName))
        {
            var tex = VideoAssetForm.DataByName[videoName];
            path = path + assetFolder + videoName;
            if (!SaveAndLoad.Exist(path))
            {
                SaveAndLoad.Copy(data.path, path);
            }
        }
    }
    private void SaveStoryAudio(string audioName, string path)
    {
        var data = AudioAssetForm.DataByName.GetDv(audioName, null);
        if (data != null && !GlobalNameHelper.IsInnerAssetName(audioName))
        {
            var tex = AudioAssetForm.DataByName[audioName];
            path = path + assetFolder + audioName;
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
        LoadScene(folder, assetFolder);
        LoadMaterial(folder, assetFolder);
        LoadObject(folder, assetFolder);
        LoadCharacter(folder, assetFolder);
        LoadSkill(folder, assetFolder);

        LoadItem(folder, assetFolder);
        LoadEffect(folder, assetFolder);

        LoadEvent(folder, assetFolder);
        LoadProgress(folder, assetFolder);

        DeleteCache(id);
        CopyMapScene(folder, Main2StoryManager.GetStoryCacheFolder(id));
    }
    public void ResetStory()
    {
        LoadScene(null, null);
        LoadMaterial(null, null);
        LoadObject(null, null);
        LoadCharacter(null, null);
        LoadSkill(null, null);

        LoadItem(null, null);
        LoadEffect(null, null);

        LoadEvent(null, null);
        LoadProgress(null, null);
        LoadUiItem(null);
    }
    public void LoadSaveStory(int id)
    {
        ResetStory();
        var folder = Main2StoryManager.GetStorySaveFolder(id);

        var assetFolder = Main2StoryManager.GetStoryAssetFolder(id);
        LoadScene(folder, assetFolder);
        LoadMaterial(folder, assetFolder);
        LoadObject(folder, assetFolder);
        LoadCharacter(folder, assetFolder);
        LoadSkill(folder, assetFolder);

        LoadItem(folder, assetFolder);
        LoadEffect(folder, assetFolder);

        LoadEvent(folder, assetFolder);
        LoadProgress(folder, assetFolder);
        //manage Scene
        DeleteCache(id);
        if (LackMapScene(folder))
        {
            CopyMapScene(Main2StoryManager.GetStoryCoreFolder(id), folder);
        }
        CopyMapScene(folder, Main2StoryManager.GetStoryCacheFolder(id));
        //
        LoadUiItem(folder);

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
                var nm = form.icon;
                LoadTex(nm, coreFolder);
            }
        }
    }
    public void LoadMaterial(string folder, string assetFolder)
    {
        var pathForm = folder + "/" + mapTextureFormFileName;
        if (SaveAndLoad.Exist(pathForm))
        {
            foreach (var form in MapTextureForm.GetDatasByJa(JArray.Parse(SaveAndLoad.Load<string>(pathForm))))
            {
                MapTextureForm.AddData(form);
            }
        }

        foreach (var data in MapTextureForm.DataById.Values)
        {
            for (int i = 0; i < data.texsName.Count; i++)
            {
                var nm = data.texsName[i];
                LoadStoryTex(nm, assetFolder);

            }
        }

        pathForm = folder + "/" + mapMaskFormFileName;
        if (SaveAndLoad.Exist(pathForm))
        {
            foreach (var form in MapMaskForm.GetDatasByJa(JArray.Parse(SaveAndLoad.Load<string>(pathForm))))
            {
                MapMaskForm.AddData(form);
            }
        }
        foreach (var data in MapMaskForm.DataById.Values)
        {
            for (int i = 0; i < Enum.GetValues(typeof(AlphaTexBasic6)).Length; i++)
            {
                var nm = data.texsName[i];
                LoadStoryTex(nm, assetFolder);
            }
        }
    }
    public void LoadObject(string folder, string assetFolder)
    {
        var pathForm = folder + "/" + mapObjectFormFileName;
        MapObjectForm.ClearAuto();
        if (SaveAndLoad.Exist(pathForm))
        {

            foreach (var form in MapObjectForm.GetDatasByJa(JArray.Parse(SaveAndLoad.Load<string>(pathForm))))
            {
                MapObjectForm.AddData(form);
            }
        }
        foreach (var data in MapObjectForm.DataById.Values)
        {
            for (int i = 0; i < data.model.subUnitTexsName.Count; i++)
            {
                var nm = data.model.subUnitTexsName[i];
                LoadStoryTex(nm, assetFolder);

            }
        }
    }

    public void LoadCharacter(string folder, string assetFolder)
    {
        var pathForm = folder + "/" + characterParamFormFileName;
        CharacterParamForm.ClearAuto();
        if (SaveAndLoad.Exist(pathForm))
        {
            foreach (var form in CharacterParamForm.GetDatasByJa(JArray.Parse(SaveAndLoad.Load<string>(pathForm))))
            {
                CharacterParamForm.AddData(form);
            }
        }

        pathForm = folder + "/" + characterProductFormFileName;
        CharacterProductForm.ClearAuto();
        if (SaveAndLoad.Exist(pathForm))
        {
            foreach (var form in CharacterProductForm.GetDatasByJa(JArray.Parse(SaveAndLoad.Load<string>(pathForm))))
            {
                CharacterProductForm.AddData(form);
            }
        }

        foreach (var data in CharacterProductForm.DataByUid.Values)
        {
            var icon = data.avatarTexName;
            LoadStoryTex(icon, assetFolder);
            LoadStoryTex(data.tachie, assetFolder);

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
                        for (int i = 0; i < anim.animClip[dir].Count; i++)
                        {
                            if (anim.animClip[dir][i].partTex.ContainsKey(part))
                            {
                                var nm = anim.animClip[dir][i].partTex[part];
                                LoadStoryTex(nm, assetFolder);
                            }
                        }

                    }
                }
            }
        }
    }
    public void LoadSkill(string folder, string assetFolder)
    {
        var pathForm = folder + "/" + skillParamFormFileName;
        SkillParamForm.ClearAuto();
        if (SaveAndLoad.Exist(pathForm))
        {
            foreach (var form in SkillParamForm.GetDatasByJa(JArray.Parse(SaveAndLoad.Load<string>(pathForm))))
            {
                SkillParamForm.AddData(form);
            }
        }

        pathForm = folder + "/" + skillProductFormFileName;
        SkillProductForm.ClearAuto();
        if (SaveAndLoad.Exist(pathForm))
        {
            foreach (var form in SkillProductForm.GetDatasByJa(JArray.Parse(SaveAndLoad.Load<string>(pathForm))))
            {
                SkillProductForm.AddData(form);
            }
        }

        foreach (var data in SkillProductForm.DataByUid.Values)
        {
            var icon = data.icon;
            LoadStoryTex(icon, assetFolder);
        }
    }
    public void LoadItem(string folder, string assetFolder)
    {
        var pathForm = folder + "/" + itemParamFormFileName;
        ItemParamForm.ClearAuto();
        if (SaveAndLoad.Exist(pathForm))
        {
            foreach (var form in ItemParamForm.GetDatasByJa(JArray.Parse(SaveAndLoad.Load<string>(pathForm))))
            {
                ItemParamForm.AddData(form);
            }
        }

        pathForm = folder + "/" + itemProductFormFileName;
        ItemProductForm.ClearAuto();
        if (SaveAndLoad.Exist(pathForm))
        {
            foreach (var form in ItemProductForm.GetDatasByJa(JArray.Parse(SaveAndLoad.Load<string>(pathForm))))
            {
                ItemProductForm.AddData(form);
            }
        }

        foreach (var data in ItemProductForm.DataByUid.Values)
        {
            var icon = data.iconTexName;
            LoadStoryTex(icon, assetFolder);

            foreach (var nm in data.model.subUnitTexsName)
            {
                LoadStoryTex(nm, assetFolder);
            }
        }
    }

    public void LoadEffect(string folder, string assetFolder)
    {
        var pathForm = folder + "/" + effectFormFileName;
        EffectForm.ClearAuto();
        if (SaveAndLoad.Exist(pathForm))
        {
            foreach (var form in EffectForm.GetDatasByJa(JArray.Parse(SaveAndLoad.Load<string>(pathForm))))
            {
                EffectForm.AddData(form);
            }
        }

        foreach (var data in EffectForm.DataByUid.Values)
        {
            foreach (var clip in data.clips)
            {
                LoadStoryTex(clip[0].tex, assetFolder);
            }
        }
    }

    public void LoadEvent(string folder, string assetFolder)
    {
        var pathForm = folder + "/" + eventFormFileName;

        LoadStoryTex("$i$$i$", assetFolder);
        EventProgramDataForm.ClearAuto();
        if (SaveAndLoad.Exist(pathForm))
        {
            foreach (var form in EventProgramDataForm.GetDatasByJa(JArray.Parse(SaveAndLoad.Load<string>(pathForm))))
            {
                EventProgramDataForm.AddData(form);
                var imgs = form.code.Split(AssetDefines.IMAGE_MARK);//获取常量图片
                for (int i = 1; i < imgs.Length; i += 2)
                {
                    LoadStoryTex(AssetManager.instance.texCtrl.GetName(imgs[i]), assetFolder);
                }
                var audios = form.code.Split(AssetDefines.AUDIO_MARK);//获取常量图片
                for (int i = 1; i < audios.Length; i += 2)
                {
                    LoadStoryAudio(AssetManager.instance.audioCtrl.GetName(audios[i]), assetFolder);
                }
                var videos = form.code.Split(AssetDefines.VIDEO_MARK);//获取常量图片
                for (int i = 1; i < videos.Length; i += 2)
                {
                    LoadStoryVideo(AssetManager.instance.videoCtrl.GetName(videos[i]), assetFolder);
                }
            }
        }


    }
    public MapInfo LoadSceneMap(string scenePath)
    {
        var mapData = new GameMapData();
        mapData.Init(SaveAndLoad.Load<string>(scenePath));
        return mapData;
    }
    public void LoadProgress(string folder, string assetFolder)
    {
        var pathForm = folder + "/" + progressFormFileName;

        if (SaveAndLoad.Exist(pathForm))
        {
            foreach (var form in ProgressForm.GetDatasByJa(JArray.Parse(SaveAndLoad.Load<string>(pathForm))))
            {
                ProgressForm.AddData(form);
            }
        }
    }


    public void LoadUiItem(string folder)
    {
        var pathForm = folder + "/" + imageUiItemFormFileName;
        ImageUiItemForm.Clear();
        if (SaveAndLoad.Exist(pathForm))
        {
            foreach (var form in ImageUiItemForm.GetDatasByJa(JArray.Parse(SaveAndLoad.Load<string>(pathForm))))
            {
                ImageUiItemForm.AddData(form);
            }
        }
    }

    public void LoadScene(string folder, string assetFolder)
    {
        var pathForm = folder + "/" + sceneFormFileName;
        SceneForm.ClearAuto();
        if (SaveAndLoad.Exist(pathForm))
        {
            foreach (var form in SceneForm.GetDatasByJa(JArray.Parse(SaveAndLoad.Load<string>(pathForm))))
            {
                SceneForm.AddData(form);
            }
        }

        foreach (var data in SceneForm.DataByUid.Values)
        {
            var nm = data.miniMap;
            LoadStoryTex(nm, assetFolder);
        }
    }

    public void LoadLocalModStory()
    {

    }
    #region util
    public void LoadStoryTex(string texName, string path)
    {
        if (!string.IsNullOrEmpty(texName) && SaveAndLoad.Exist(path) && !StoryTexAssetForm.DataByName.ContainsKey(texName) && !GlobalNameHelper.IsInnerAssetName(texName))
        {
            path = path + assetFolder + texName;
            if (SaveAndLoad.Exist(path))
            {
                AddStoryTex(AssetManager.instance.texCtrl.CreateDataByPath(path, texName));
            }
            else if (!GameTexAssetForm.DataByName.ContainsKey(texName))
            {
                Debug.LogError(texName + "贴图丢失！");
                AddStoryTex(AssetManager.instance.texCtrl.CreateDataByTex(TextureHelper.transparentTexture, texName));
            }
        }
    }

    public void AddStoryTex(TexAssetForm.Data rawData)
    {
        StoryTexAssetForm.Data data = new StoryTexAssetForm.Data(rawData);
        if (StoryTexAssetForm.DataByName.ContainsKey(data.name))
        {
            var oldData = StoryTexAssetForm.DataByName[data.name];
            StoryTexAssetForm.RemoveData(oldData.id);
        }
        StoryTexAssetForm.AddData(data);

        Z_EventHelper.Invoke(new AssetEvent()
        {
            importAssetName = data.name
        });
    }
    public void AddGameTex(TexAssetForm.Data rawData)
    {
        GameTexAssetForm.Data data = new GameTexAssetForm.Data(rawData);
        if (GameTexAssetForm.DataByName.ContainsKey(data.name))
        {
            var oldData = GameTexAssetForm.DataByName[data.name];
            GameTexAssetForm.RemoveData(oldData.id);
        }
        GameTexAssetForm.AddData(data);

        Z_EventHelper.Invoke(new AssetEvent()
        {
            importAssetName = data.name
        });
    }
    public void LoadTex(string texName, string path)
    {
        path = path + assetFolder + texName;
        if (SaveAndLoad.Exist(path) && !TexAssetForm.DataByName.ContainsKey(texName))
        {
            AddTex(AssetManager.instance.texCtrl.CreateDataByPath(path, texName));
        }
    }
    public void AddTex(TexAssetForm.Data rawData)
    {
        TexAssetForm.Data data = rawData;
        if (TexAssetForm.DataByName.ContainsKey(data.name))
        {
            var oldData = TexAssetForm.DataByName[data.name];
            TexAssetForm.RemoveData(oldData.id);
        }
        TexAssetForm.AddData(data);

        Z_EventHelper.Invoke(new AssetEvent()
        {
            importAssetName = data.name
        });
    }

    public void LoadStoryAudio(string audioName, string path)
    {
        path = path + assetFolder + audioName;
        if (!string.IsNullOrEmpty(audioName) && SaveAndLoad.Exist(path) && !StoryAudioAssetForm.DataByName.ContainsKey(audioName) && !GlobalNameHelper.IsInnerAssetName(audioName))
        {
            AddStoryAudio(AssetManager.instance.audioCtrl.CreateDataByPath(path, audioName));
        }
    }
    public void AddStoryAudio(AudioAssetForm.Data rawData)
    {
        StoryAudioAssetForm.Data data = new StoryAudioAssetForm.Data(rawData);
        if (StoryAudioAssetForm.DataByName.ContainsKey(data.name))
        {
            var oldData = StoryAudioAssetForm.DataByName[data.name];
            StoryAudioAssetForm.RemoveData(oldData.id);
        }
        StoryAudioAssetForm.AddData(data);

        Z_EventHelper.Invoke(new AssetEvent()
        {
            importAssetName = data.name
        });
    }
    public void LoadStoryVideo(string videoName, string path)
    {
        path = path + assetFolder + videoName;
        if (!string.IsNullOrEmpty(videoName) && SaveAndLoad.Exist(path) && !StoryVideoAssetForm.DataByName.ContainsKey(videoName) && !GlobalNameHelper.IsInnerAssetName(videoName))
        {
            AddStoryVideo(AssetManager.instance.videoCtrl.CreateDataByPath(path, videoName));
        }
    }
    public void AddStoryVideo(VideoAssetForm.Data rawData)
    {
        StoryVideoAssetForm.Data data = new StoryVideoAssetForm.Data(rawData);
        if (StoryVideoAssetForm.DataByName.ContainsKey(data.name))
        {
            var oldData = StoryVideoAssetForm.DataByName[data.name];
            StoryVideoAssetForm.RemoveData(oldData.id);
        }
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
            if (form.name.StartsWith(GlobalNameHelper.GetInternalPrefabName("")))
            {
                InstancePoolManager.instance.AddPool(form.GetGo());
            }
        }
        foreach (var form in MapObjectForm.DataById.Values)
        {
            var obj = _super.utilCtrl.CombineNewObjectByPrefabs(form.name, form.model, true);
            obj.transform.parent = InstancePoolManager.instance.defaultRoot;
            InstancePoolManager.instance.AddPool(obj);
        }

        //item生成mapItem
        MapItemForm.Clear();
        if (ItemProductForm.DatasByProtouid.ContainsKey(0))
        {
            foreach (var itemData in ItemProductForm.DatasByProtouid[0])
            {
                MapItemForm.AddData(new MapItemForm.Data(-1, itemData.name, itemData.iconTexName, itemData.model, itemData.label, itemData.uid));
            }
        }

        //character生成mapCharacter
        MapCharacterForm.Clear();
        if (CharacterProductForm.DatasByProtouid.ContainsKey(0))
        {
            foreach (var characterData in CharacterProductForm.DatasByProtouid[0])
            {
                MapCharacterForm.AddData(new MapCharacterForm.Data(-1, characterData.name, characterData.label, characterData.avatarTexName, characterData.uid));
            }
        }

        foreach (var form in ItemProductForm.DataByUid.Values)
        {
            var obj = _super.utilCtrl.CombineNewObjectByPrefabs(form.name, form.model, true, true);
            obj.transform.parent = InstancePoolManager.instance.defaultRoot;
            InstancePoolManager.instance.AddPool(obj);
        }

        var character = _super.utilCtrl.CombineNewCharacterByPrefabs(GlobalNameHelper.GetRuntimePrefabName("character"), new List<string>() { GlobalNameHelper.GetDefaultTexName(), GlobalNameHelper.GetDefaultTexName(), null }, true);
        character.transform.parent = InstancePoolManager.instance.defaultRoot;
        InstancePoolManager.instance.AddPool(character);

    }


}
