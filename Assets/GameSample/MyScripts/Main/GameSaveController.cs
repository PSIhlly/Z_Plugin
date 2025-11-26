using Form;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Ui.ModSceneMain.ModTool;
using UnityEngine;
using UnityEngine.UI;
using Z_ByteSerialize;
using Z_DataSystem;
using Z_DataSystem.Form;
using Z_Debug;
using Z_DesignStyle;
using Z_Map;
using Z_Map.Analysis;
using Z_Texture;
using Z_Ui.Form;
using Z_UnitSystem;

public class GameSaveController : Z_Controller<GameManager>
{
    public string mapTextureFormFileName => "mtf";
    public string mapMaskFormFileName => "mmf";
    public string mapObjectFormFileName => "mof";
    public string storyFormFileName => "sf";
    public string characterParamFormFileName => "cpaf";
    public string sceneFormFileName => "saf";
    public string characterProductFormFileName => "cprf";
    public string eventFormFileName => "ef";
    public string configFormFileName => "cf";
    public string itemParamFormFileName => "ipaf";
    public string effectFormFileName => "etf";
    public string skillFormFileName => "slf";
    public string itemProductFormFileName => "iprf";
    public string imageUiItemFormFileName => "iuif";


    public string assetFolder => "ast/";
    public GameSaveController(GameManager super) : base(super)
    {
    }
    #region save
    public void SaveSceneMap(string scenePath)
    {
        SaveAndLoad.Save(scenePath, JsonConvert.SerializeObject(MapManager.instance.data.GetJsonData()));
    }
    public void SaveSceneMap(string scenePath, MapInfo data)
    {
        SaveAndLoad.Save(scenePath, JsonConvert.SerializeObject(data.GetJsonData()));
    }

    public void SaveModStory(int id)
    {
        SaveOverview(id);
        string path = ModManager.GetStoryCoreFolder(Main2StoryManager.GetSceneFileNameById(id));

        SaveMaterial(path);
        SaveObject(path);
        SaveCharacter(path);
        SaveSkill(path);
        SaveItem(path);
        SaveEffect(path);
        SaveEvent(path);
        SaveConfig(path);
        SaveScene(path);

    }
    public void SaveOverview(int id)
    {
        var storyCoreFolder = ModManager.GetStoryCoreFolder(Main2StoryManager.GetStoryFolderNameById(id));
        SaveAndLoad.Save(storyCoreFolder + "/" + storyFormFileName, StoryForm.GetJoByData(StoryForm.DataById[id]).ToString());
        foreach (var data in StoryForm.DataById.Values)
        {
            var nm = data.icon;
            SaveTex(nm, storyCoreFolder);

        }
    }

    public void SaveMaterial(string storyCoreFolder)
    {

        SaveAndLoad.Save(storyCoreFolder + "/" + mapTextureFormFileName, MapTextureForm.GetJaByDatas().ToString());

        foreach (var data in MapTextureForm.DataById.Values)
        {
            for (int i = 0; i < data.texsName.Count; i++)
            {
                var nm = data.texsName[i];
                SaveStoryTex(nm, storyCoreFolder);
            }
        }

        SaveAndLoad.Save(storyCoreFolder + "/" + mapMaskFormFileName, MapMaskForm.GetJaByDatas().ToString());
        foreach (var data in MapMaskForm.DataById.Values)
        {
            for (int i = 0; i < Enum.GetValues(typeof(AlphaTexBasic6)).Length; i++)
            {
                var nm = data.texsName[i];
                SaveStoryTex(nm, storyCoreFolder);
            }
        }
    }
    public void SaveObject(string storyCoreFolder)
    {

        SaveAndLoad.Save(storyCoreFolder + "/" + mapObjectFormFileName, MapObjectForm.GetJaByDatas().ToString());
        foreach (var data in MapObjectForm.DataById.Values)
        {
            for (int i = 0; i < data.model.subUnitTexsName.Count; i++)
            {
                var nm = data.model.subUnitTexsName[i];
                SaveStoryTex(nm, storyCoreFolder);
            }
        }
    }

    public void SaveCharacter(string storyCoreFolder)
    {
        SaveAndLoad.Save(storyCoreFolder + "/" + characterParamFormFileName, CharacterParamForm.GetJaByDatas().ToString());

        SaveAndLoad.Save(storyCoreFolder + "/" + characterProductFormFileName, CharacterProductForm.GetJaByDatas().ToString());
        foreach (var data in CharacterProductForm.DataByUid.Values)
        {
            var icon = data.avatarTexName;
            SaveStoryTex(icon, storyCoreFolder);
            SaveStoryTex(data.tachie, storyCoreFolder);
            foreach (var anim in data.animDic.Values)
            {
                for (int i = 0; i < anim.animClip.Count; i++)
                {
                    foreach (BodyPartType part in Enum.GetValues(typeof(BodyPartType)))
                    {
                        if (anim.animClip[i].partTex.ContainsKey(part))
                        {
                            var nm = anim.animClip[i].partTex[part];
                            SaveStoryTex(nm, storyCoreFolder);
                        }
                    }
                }

            }
        }
    }
    public void SaveSkill(string storyCoreFolder)
    {
        SaveAndLoad.Save(storyCoreFolder + "/" + skillFormFileName, SkillForm.GetJaByDatas().ToString());
        foreach (var data in SkillForm.DataByUid.Values)
        {
            SaveStoryTex(data.icon, storyCoreFolder);
        }
    }
    public void SaveItem(string storyCoreFolder)
    {
        SaveAndLoad.Save(storyCoreFolder + "/" + itemParamFormFileName, ItemParamForm.GetJaByDatas().ToString());

        SaveAndLoad.Save(storyCoreFolder + "/" + itemProductFormFileName, ItemProductForm.GetJaByDatas().ToString());
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
    public void SaveEffect(string storyCoreFolder)
    {
        SaveAndLoad.Save(storyCoreFolder + "/" + effectFormFileName, EffectForm.GetJaByDatas().ToString());
        foreach (var data in EffectForm.DataByUid.Values)
        {
            foreach (var clip in data.clips)
            {
                SaveStoryTex(clip.tex, storyCoreFolder);
            }
        }
    }

    public void SaveEvent(string storyCoreFolder, EventProgramDataForm.Data data = null)
    {
        SaveAndLoad.Save(storyCoreFolder + "/" + eventFormFileName, EventProgramDataForm.GetJaByDatas().ToString());
        Action<EventProgramDataForm.Data> act =(data)=>
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

    public void SaveConfig(string storyCoreFolder)
    {
        SaveAndLoad.Save(storyCoreFolder + "/" + configFormFileName, ConfigForm.GetJaByDatas().ToString());
        foreach (var data in ConfigForm.DataByUid.Values)
        {
            var nm = data.miniMap;
            SaveStoryTex(nm, storyCoreFolder);
        }

    }
    public void SaveProgress(string progressPath)
    {
        SaveAndLoad.Save(progressPath, JsonConvert.SerializeObject(PlayManager.instance.data.GetJsonData()));
    }
    public void SaveUiItem(string storySaveFolder)
    {
        SaveAndLoad.Save(storySaveFolder + "/" + imageUiItemFormFileName, JsonConvert.SerializeObject(ImageUiItemForm.GetJaByDatas().ToString()));
    }
    public void SaveScene(string storyCoreFolder)
    {
        SaveAndLoad.Save(storyCoreFolder + "/" + sceneFormFileName, SceneForm.GetJaByDatas().ToString());
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
        if (data != null && data.bytes != null && !GlobalNameHelper.IsInnerAssetName(videoName))
        {
            var tex = VideoAssetForm.DataByName[videoName];
            path = path + assetFolder + videoName;
            if (!SaveAndLoad.Exist(path))
            {
                SaveAndLoad.Save(path, data.bytes);
            }
        }
    }
    private void SaveStoryAudio(string audioName, string path)
    {
        var data = AudioAssetForm.DataByName.GetDv(audioName, null);
        if (data != null && data.bytes != null && !GlobalNameHelper.IsInnerAssetName(audioName))
        {
            var tex = AudioAssetForm.DataByName[audioName];
            path = path + assetFolder + audioName;
            if (!SaveAndLoad.Exist(path))
            {
                SaveAndLoad.Save(path, data.bytes);
            }
        }
    }
    #endregion
    #endregion

    #region load
    
    public void LoadOverview()
    {
        StoryForm.Clear();
        string[] allDirectories = Directory.GetDirectories(SaveAndLoad.perPath);
        foreach (string dir in allDirectories)
        {
            var coreFolder = ModManager.GetStoryCoreFolder(dir);
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
    public void LoadMaterial(string storyCoreFolder)
    {
        var pathForm = storyCoreFolder + "/" + mapTextureFormFileName;
        MapMaskForm.ClearAuto();
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
                LoadStoryTex(nm, storyCoreFolder);

            }
        }

        pathForm = storyCoreFolder + "/" + mapMaskFormFileName;
        MapMaskForm.ClearAuto();
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
                LoadStoryTex(nm, storyCoreFolder);
            }
        }
    }
    public void LoadObject(string storyCoreFolder)
    {
        var pathForm = storyCoreFolder + "/" + mapObjectFormFileName;
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
                LoadStoryTex(nm, storyCoreFolder);

            }
        }
    }

    public void LoadCharacter(string storyCoreFolder)
    {
        var pathForm = storyCoreFolder + "/" + characterParamFormFileName;
        CharacterParamForm.ClearAuto();
        if (SaveAndLoad.Exist(pathForm))
        {
            foreach (var form in CharacterParamForm.GetDatasByJa(JArray.Parse(SaveAndLoad.Load<string>(pathForm))))
            {
                CharacterParamForm.AddData(form);
            }
        }

        pathForm = storyCoreFolder + "/" + characterProductFormFileName;
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
            LoadStoryTex(icon, storyCoreFolder);
            LoadStoryTex(data.tachie, storyCoreFolder);

            foreach (var anim in data.animDic.Values)
            {

                for (int i = 0; i < anim.animClip.Count; i++)
                {
                    foreach (BodyPartType part in Enum.GetValues(typeof(BodyPartType)))
                    {
                        if (anim.animClip[i].partTex.ContainsKey(part))
                        {
                            var nm = anim.animClip[i].partTex[part];
                            LoadStoryTex(nm, storyCoreFolder);
                        }

                    }
                }
            }
        }
    }
    public void LoadSkill(string storyCoreFolder)
    {
        var pathForm = storyCoreFolder + "/" + skillFormFileName;
        SkillForm.ClearAuto();
        if (SaveAndLoad.Exist(pathForm))
        {
            foreach (var form in SkillForm.GetDatasByJa(JArray.Parse(SaveAndLoad.Load<string>(pathForm))))
            {
                SkillForm.AddData(form);
            }
        }

        foreach (var data in SkillForm.DataByUid.Values)
        {
            LoadStoryTex(data.icon, storyCoreFolder);
        }
    }
    public void LoadItem(string storyCoreFolder)
    {
        var pathForm = storyCoreFolder + "/" + itemParamFormFileName;
        ItemParamForm.ClearAuto();
        if (SaveAndLoad.Exist(pathForm))
        {
            foreach (var form in ItemParamForm.GetDatasByJa(JArray.Parse(SaveAndLoad.Load<string>(pathForm))))
            {
                ItemParamForm.AddData(form);
            }
        }

        pathForm = storyCoreFolder + "/" + itemProductFormFileName;
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
            LoadStoryTex(icon, storyCoreFolder);

            foreach (var nm in data.model.subUnitTexsName)
            {
                LoadStoryTex(nm, storyCoreFolder);
            }
        }
    }

    public void LoadEffect(string storyCoreFolder)
    {
        var pathForm = storyCoreFolder + "/" + effectFormFileName;
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
                LoadStoryTex(clip.tex, storyCoreFolder);
            }
        }
    }

    public void LoadEvent(string storyCoreFolder)
    {
        var pathForm = storyCoreFolder + "/" + eventFormFileName;
        EventProgramDataForm.ClearAuto();
        if (SaveAndLoad.Exist(pathForm))
        {
            foreach (var form in EventProgramDataForm.GetDatasByJa(JArray.Parse(SaveAndLoad.Load<string>(pathForm))))
            {
                EventProgramDataForm.AddData(form);
                var imgs = form.code.Split(AssetDefines.IMAGE_MARK);//获取常量图片
                for (int i = 1; i < imgs.Length; i += 2)
                {
                    LoadStoryTex(AssetManager.instance.texCtrl.GetName(imgs[i]), storyCoreFolder);
                }
                var audios = form.code.Split(AssetDefines.AUDIO_MARK);//获取常量图片
                for (int i = 1; i < audios.Length; i += 2)
                {
                    LoadStoryAudio(AssetManager.instance.audioCtrl.GetName(audios[i]), storyCoreFolder);
                }
                var videos = form.code.Split(AssetDefines.VIDEO_MARK);//获取常量图片
                for (int i = 1; i < videos.Length; i += 2)
                {
                    LoadStoryVideo(AssetManager.instance.videoCtrl.GetName(videos[i]), storyCoreFolder);
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
    public void LoadConfig(string storyCoreFolder)
    {
        var pathForm = storyCoreFolder + "/" + configFormFileName;

        if (SaveAndLoad.Exist(pathForm))
        {
            foreach (var form in ConfigForm.GetDatasByJa(JArray.Parse(SaveAndLoad.Load<string>(pathForm))))
            {
                ConfigForm.AddData(form);
                var nm = form.miniMap;
                LoadStoryTex(nm, storyCoreFolder);
            }
        }
    }

    public PlayData LoadProgress(string progressPath)
    {
        if (SaveAndLoad.Exist(progressPath))
        {
            return new PlayData(SaveAndLoad.Load<string>(progressPath));
        }
        return new PlayData();
    }
    public void LoadUiItem(string storySaveFolder)
    {
        var pathForm = storySaveFolder + "/" + imageUiItemFormFileName;
        ImageUiItemForm.Clear();
        if (SaveAndLoad.Exist(pathForm))
        {
            foreach (var form in ImageUiItemForm.GetDatasByJa(JArray.Parse(SaveAndLoad.Load<string>(pathForm))))
            {
                ImageUiItemForm.AddData(form);
            }
        }
    }

    public void LoadScene(string storyCoreFolder)
    {
        var pathForm = storyCoreFolder + "/" + sceneFormFileName;
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
            LoadStoryTex(nm, storyCoreFolder);
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
            AddStoryVideo(AssetManager.instance.videoCtrl.CreateDataByPath(path,  videoName));
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

    #region del

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
        if (ItemProductForm.DatasByIsproto.ContainsKey(true))
        {
            foreach (var itemData in ItemProductForm.DatasByIsproto[true])
            {
                MapItemForm.AddData(new MapItemForm.Data(-1, itemData.name, itemData.iconTexName, itemData.model, itemData.label, itemData.uid));
            }
        }

        //character生成mapCharacter
        MapCharacterForm.Clear();
        if (CharacterProductForm.DatasByIsproto.ContainsKey(true))
        {
            foreach (var characterData in CharacterProductForm.DatasByIsproto[true])
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
