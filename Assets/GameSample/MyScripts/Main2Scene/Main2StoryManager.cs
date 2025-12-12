using Form;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TreeEditor;
using Ui;
using Ui.Loading;
using Ui.ModSceneMain;
using UnityEngine;
using Z_DataSystem;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Input;
using Z_Map;
using Z_Ui;
using Z_Ui.Form;
using Z_Ui.Loading;
using Z_UnitSystem;



public class Main2StoryManager : Z_MonoManager<Main2StoryManager>
{
    #region story
    public void StartLoadStoryUgc(int storyId)
    {
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
        StoryTexAssetForm.Clear();

        if (!SaveAndLoad.Exist(GetStoryCoreFolder(storyFolder)))
        {
            StoryForm.AddData(new StoryForm.Data(storyId, "new" + storyId, "empty", GlobalNameHelper.GetDefaultTexName()));

            SceneForm.Clear();
            var sceneData = new SceneForm.Data(1, "scene", GlobalNameHelper.GetDefaultTexName(), (0.5f, 0.5f));
            SceneForm.AddData(sceneData);

            CharacterParamForm.Clear();
            var hpParamData = new CharacterParamForm.Data(-1, "Hp", 0, 0f, 100f, 100f, 0, ParamShowType.AlwaysWithPanel);
            var speedParamData = new CharacterParamForm.Data(-1, "Speed", 0, 0f, 5f, 5f, 0, default);
            CharacterParamForm.AddData(hpParamData);
            CharacterParamForm.AddData(speedParamData);

            CharacterProductForm.Clear();
            ModManager.instance.assetCtrl.CreateCharacter("Player");
            var player = CharacterProductForm.DataByName["Player"];
            player.hpParamName = "Hp";
            player.speedParamName = "Speed";
            
            ProgressForm.Clear();
            ProgressForm.AddData(new ProgressForm.Data(1,0, sceneData.uid, new Vector3(500, 1000, 500), 1, new List<int>() { }, new List<int>() { 1 }, new List<int>() { 1 }, new Dictionary<string, string>(), new Dictionary<string, EventTriggerForm.Data>(), CameraMode.Overhead, ClipForm.defaultData.Copy(), new Dictionary<int, List<string>>(), false, 0));


            var data = new GameMapData();
            data.Init();
            
            GameManager.instance.saveCtrl.SaveCoreStory(storyId);

            GameManager.instance.saveCtrl.SaveSceneMap(GetStoryCoreFolder(storyFolder) + Main2StoryManager.GetSceneFileNameById(sceneData.uid), data);
            
        }

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
        ProgressForm.Clear();
        GameManager.instance.curStory = null;
    }
    #endregion

    #region scene
    public async void StartLoadSceneUgc(int sceneId)
    {
        bool ok = await StartLoadScene(ModManager.instance.GetStoryCoreFolder(), sceneId);
        ModManager.instance.BeginScene(sceneId);
    }
    public async void StartLoadScenePlay(int sceneId)
    {
        bool ok = await StartLoadScene(PlayManager.instance.GetStoryCacheFolder(), sceneId);
        PlayManager.instance.BeginScene(sceneId);
    }

    private async Task<bool> StartLoadScene(string storyCoreFolder, int id)
    {
        if (!SaveAndLoad.Exist(storyCoreFolder + "/" + GetSceneFileNameById(id)))
        {
            Debug.LogError(storyCoreFolder + "/" + GetSceneFileNameById(id) + " scene file not exist");
        }
        GameManager.instance.curScene = SceneForm.DataByUid[id];

        GameManager.instance.saveCtrl.ResetPrefabPool();

        LoadingManager.instance.AddLoadItem("scene");
        MapInfo data;

        data = await Task.Run(() =>
        {
            return GameManager.instance.saveCtrl.LoadSceneMap(storyCoreFolder + "/" + GetSceneFileNameById(id));
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
        data.mainData.viewSize = new Vector3Int((int)(InputManager.instance.screenWorldSize.x / 2) + 4, data.mainData.viewSize.y, (int)(InputManager.instance.screenWorldSize.y / 2) + 4);

        DynamicGlobalSettings.cameraMode = ProgressForm.DataByUid[1].cameraMode;
        MapManager.instance.Begin(data);




        foreach (var maskData in MapMaskForm.DataById.Values)
        {
            var raws = new Texture2D[6];

            for (int i = 0; i < maskData.texsName.Count; i++)
            {
                raws[i] = (Texture2D)TexAssetForm.DataByName[maskData.texsName[i]].GetTex();
            }
            GameManager.instance.mapCtrl.CreateAlphaVariantsByBasic6(maskData.name, raws);
        }

        foreach (var character in CharacterProductForm.DataByUid.Values)
        {
            GameManager.instance.characterCtrl.RegisterAnim(character);
        }
        return true;
    }
    public void UnloadSceneUgc()
    {
        UnloadScene();
        ModManager.instance.EndScene();
    }
    public void UnloadScenePlay()
    {
        UnloadScene();
        PlayManager.instance.EndScene();
    }
    public void UnloadScene()
    {
        GameManager.instance.mapCtrl.Reset();
        GameManager.instance.characterCtrl.Reset();
        GameManager.instance.curScene = null;
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
