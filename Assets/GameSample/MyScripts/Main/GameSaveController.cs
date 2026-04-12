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

        SaveMaterial(path);
        SaveObject(path);
        SaveCharacter(path);
        SaveSkill(path);
        SaveItem(path);
        SaveEffect(path);
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
        SaveAssets(path);
    }
    public void SaveOverview(int id)
    {
        var storyCoreFolder = Main2StoryManager.GetStoryCoreFolder(id);
        SaveAndLoad.Save(storyCoreFolder + "/" + storyFormFileName, StoryForm.GetJoByData(StoryForm.DataById[id]).ToString());

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
        SaveStoryTex("$i$$i$", storyCoreFolder);
        SaveAndLoad.Save(storyCoreFolder + "/" + imageAssetFormFileName, StoryTexAssetForm.GetJaByDatas().ToString());
        SaveAndLoad.Save(storyCoreFolder + "/" + audioAssetFormFileName, StoryAudioAssetForm.GetJaByDatas().ToString());
        SaveAndLoad.Save(storyCoreFolder + "/" + videoAssetFormFileName, StoryVideoAssetForm.GetJaByDatas().ToString());

        foreach (var data in StoryTexAssetForm.DataById.Values)
        {
            SaveStoryTex(data.name, storyCoreFolder);
        }
        foreach (var data in StoryAudioAssetForm.DataById.Values)
        {
            SaveStoryAudio(data.name, storyCoreFolder);
        }
        foreach (var data in StoryVideoAssetForm.DataById.Values)
        {
            SaveStoryVideo(data.name, storyCoreFolder);
        }
        SaveAndLoad.EachFile(storyCoreFolder + assetFolder, (name) =>
        {
            if (!StoryTexAssetForm.DataByName.ContainsKey(name) && !StoryAudioAssetForm.DataByName.ContainsKey(name) && !StoryVideoAssetForm.DataByName.ContainsKey(name))
            {
                SaveAndLoad.Delete(storyCoreFolder + assetFolder + name);
            }
        });
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
        LoadScene(folder);
        LoadMaterial(folder);
        LoadObject(folder);
        LoadCharacter(folder);
        LoadSkill(folder);

        LoadItem(folder);
        LoadEffect(folder);

        LoadEvent(folder);
        LoadProgress(folder);

        DeleteCache(id);
        CopyMapScene(folder, Main2StoryManager.GetStoryCacheFolder(id));
        LoadAsset(assetFolder);
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

        LoadEvent(null);
        LoadProgress(null);
        LoadUiItem(null);
        LoadAsset(null);
    }
    public void LoadSaveStory(int id)
    {
        ResetStory();
        var folder = Main2StoryManager.GetStorySaveFolder(id);

        var assetFolder = Main2StoryManager.GetStoryAssetFolder(id);
        LoadScene(folder);
        LoadMaterial(folder);
        LoadObject(folder);
        LoadCharacter(folder);
        LoadSkill(folder);

        LoadItem(folder);
        LoadEffect(folder);

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
    public void LoadMaterial(string folder)
    {
        var pathForm = folder + mapTextureFormFileName;
        if (SaveAndLoad.Exist(pathForm))
        {
            foreach (var form in MapTextureForm.GetDatasByJa(JArray.Parse(SaveAndLoad.Load<string>(pathForm))))
            {
                MapTextureForm.AddData(form);
            }
        }



        pathForm = folder + mapMaskFormFileName;
        if (SaveAndLoad.Exist(pathForm))
        {
            foreach (var form in MapMaskForm.GetDatasByJa(JArray.Parse(SaveAndLoad.Load<string>(pathForm))))
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

            foreach (var form in MapObjectForm.GetDatasByJa(JArray.Parse(SaveAndLoad.Load<string>(pathForm))))
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
            foreach (var form in CharacterProductForm.GetDatasByJa(JArray.Parse(SaveAndLoad.Load<string>(pathForm))))
            {
                CharacterProductForm.AddData(form);
            }
        }

        foreach (var data in CharacterProductForm.DataByUid.Values)
        {
            var icon = data.avatarTexName;

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
            foreach (var form in SkillProductForm.GetDatasByJa(JArray.Parse(SaveAndLoad.Load<string>(pathForm))))
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
            foreach (var form in ItemProductForm.GetDatasByJa(JArray.Parse(SaveAndLoad.Load<string>(pathForm))))
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
            foreach (var form in EffectForm.GetDatasByJa(JArray.Parse(SaveAndLoad.Load<string>(pathForm))))
            {
                EffectForm.AddData(form);
            }
        }
    }

    public void LoadAsset(string folder)
    {
        var path = "";
        path = folder + imageAssetFormFileName;
        StoryTexAssetForm.ClearAuto();
        if (SaveAndLoad.Exist(path))
        {
            foreach (var form in StoryTexAssetForm.GetDatasByJa(JArray.Parse(SaveAndLoad.Load<string>(path))))
            {
                StoryTexAssetForm.AddData(form);
                form.path = SaveAndLoad.GetRealPath(folder + assetFolder + form.name);
            }
        }
        path = folder + audioAssetFormFileName;
        if (SaveAndLoad.Exist(path))
        {
            foreach (var form in StoryAudioAssetForm.GetDatasByJa(JArray.Parse(SaveAndLoad.Load<string>(path))))
            {
                StoryAudioAssetForm.AddData(form);
                form.path = SaveAndLoad.GetRealPath(folder + assetFolder + form.name);
            }
        }
        path = folder + videoAssetFormFileName;
        if (SaveAndLoad.Exist(path))
        {
            foreach (var form in StoryVideoAssetForm.GetDatasByJa(JArray.Parse(SaveAndLoad.Load<string>(path))))
            {
                StoryVideoAssetForm.AddData(form);
                form.path = SaveAndLoad.GetRealPath(folder + assetFolder + form.name);
            }
        }
    }

    public void LoadEvent(string folder)
    {
        var pathForm = folder + eventFormFileName;

        EventProgramDataForm.ClearAuto();
        if (SaveAndLoad.Exist(pathForm))
        {
            foreach (var form in EventProgramDataForm.GetDatasByJa(JArray.Parse(SaveAndLoad.Load<string>(pathForm))))
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
