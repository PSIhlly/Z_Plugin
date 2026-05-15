using Form;
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
            StoryForm.AddData(new StoryForm.Data(storyId, "new" + storyId, "empty", GlobalNameHelper.GetDefaultTexName()));
            SceneForm.Clear();
            var sceneData = new SceneForm.Data(1, "scene", GlobalNameHelper.GetDefaultTexName(),Vector2.zero  ,false,false);
            SceneForm.AddData(sceneData);
            CharacterParamForm.Clear();
            CharacterProductForm.Clear();

            ItemParamForm.Clear();
            ItemProductForm.Clear();

            SkillParamForm.Clear();
            SkillProductForm.Clear();

            ModManager.instance.assetCtrl.CreateCharacter("Player");
            var player = CharacterProductForm.DataByNameProtouid[("Player", 0)];
            player.unique = true;
            ProgressForm.Clear();
            var progress = new ProgressForm.Data(1, 0, sceneData.uid, new Vector3(500, 1000, 500), player.uid, new List<int>() { }, new List<int>() { player.uid }, new List<int>() { player.uid }, new Dictionary<string, string>(), new Dictionary<string, EventTriggerForm.Data>(), CameraMode.Overhead, ClipForm.defaultData.Copy(), new Dictionary<int, List<string>>(), false, 0, defaultStyle,false,false,true,true,true,true,true,"",0);
            ProgressForm.AddData(progress);
            var data = new GameMapData();
            data.Init();
            EventProgramDataForm.Clear();
            StoryTexAssetForm.AddData(new StoryTexAssetForm.Data(AssetManager.instance.texCtrl.CreateDataByBytes(TextureHelper.GetTextureByte(TextureHelper.transparentTexture),GlobalNameHelper.GetExternDefaultTexName())));
            var cpr = new Compiler();
            switch (defaultStyle)
            {
                case EditorStyle.Avg:
                case EditorStyle.AvgAdvanced:

                    ModManager.instance.assetCtrl.CreateEvent("mainDialog", "enterGame", "dialog");
                    var dialogEvt = EventProgramDataForm.DataByName["mainDialog"];
                    dialogEvt.ApplyCode(@"ShowDialog(""$i$$i$"",""$i$$i$"",""player"",""hello"");ShowDialog(""$i$$i$"",""$i$$i$"",""player"",""you can edit it in event panel"");GameOver();");

                    progress.events["onBeginEvent"] = new EventTriggerForm.Data(-1, "onBeginEvent", new List<string>() { "mainDialog" }, default);
                    break;
                case EditorStyle.Rpg:
                case EditorStyle.RpgAdvanced:
                    var prmBox = CodeHelper.CreateBoxByNum(100);
                    var hpParamData = new CharacterParamForm.Data(-1, "Hp", 0, "", BoxDataForm.GetJoByData(prmBox).ToString(), BoxDataForm.GetJoByData(prmBox).ToString(), ParamShowType.AlwaysWithPanel);
                    prmBox.num = 5;
                    var speedParamData = new CharacterParamForm.Data(-1, "Speed", 0, "", BoxDataForm.GetJoByData(prmBox).ToString(), BoxDataForm.GetJoByData(prmBox).ToString(), default);
                    CharacterParamForm.AddData(hpParamData);
                    CharacterParamForm.AddData(speedParamData);
                    player.hpParamName = "Hp";
                    player.speedParamName = "Speed";
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
        StoryTexAssetForm.Clear();
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

        AudioManager.instance.BgmStreaming(GlobalSettings.BGM_FILE_NAME);
    }
    #endregion

    #region scene
    public async void StartLoadSceneUgc(int sceneId)
    {
        bool ok = await StartLoadScene(ModManager.instance.GetStoryCoreFolder(), sceneId);
        DynamicGlobalSettings.playing = false;
        ModManager.instance.BeginScene(sceneId);
    }
    public async void StartLoadScenePlay(int sceneId)
    {
        bool ok = await StartLoadScene(PlayManager.instance.GetStoryCacheFolder(), sceneId);
        DynamicGlobalSettings.playing = true;
        PlayManager.instance.BeginScene(sceneId);
    }

    private async Task<bool> StartLoadScene(string storyFolder, int id)
    {
        if (!SaveAndLoad.Exist(storyFolder + "/" + GetSceneFileNameById(id)))
        {
            Debug.LogError(storyFolder + "/" + GetSceneFileNameById(id) + " scene file not exist");
        }
        GameManager.instance.curScene = SceneForm.DataByUid[id];

        GameManager.instance.saveCtrl.ResetPrefabPool();

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
        data.mainData.viewSize = new Vector3Int((int)(InputManager.instance.screenWorldSize.x / 2) + 4, 1, (int)(InputManager.instance.screenWorldSize.y / 2) + 4);
         
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
    }
    public void ChangeScene(int sceneId)
    {
        UnloadScenePlay();

        StartLoadScenePlay(sceneId);
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
