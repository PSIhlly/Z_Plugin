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
using Z_UnitSystem;

public enum LoadingState
{
    Loading,
    Done
}
public class LoadingEvent : Z_Event
{
    public LoadingState state;
}
public class Main2StoryManager : Z_MonoManager<Main2StoryManager>
{
    #region story
    public void StartLoadStoryUgc(int storyId)
    {
        StartLoadStory(storyId);
        string storyFolder = GetStoryFolderNameById(storyId);
        ModManager.instance.BeginStory(storyFolder);
    }
    public void StartLoadStoryPlay(int storyId, bool boxPlay)
    {
        StartLoadStory(storyId);
        string storyFolder = GetStoryFolderNameById(storyId);
        PlayManager.instance.BeginStory(storyFolder, boxPlay);
    }
    private void StartLoadStory(int storyId)
    {
        string storyFolder = GetStoryFolderNameById(storyId);
        StoryTexAssetForm.Clear();

        if (SaveAndLoad.Exist(ModManager.GetStoryCoreFolder(storyFolder)))
        {
            GameManager.instance.saveCtrl.LoadScene(ModManager.GetStoryCoreFolder(storyFolder));
            GameManager.instance.saveCtrl.LoadMaterial(ModManager.GetStoryCoreFolder(storyFolder));
            GameManager.instance.saveCtrl.LoadObject(ModManager.GetStoryCoreFolder(storyFolder));
            GameManager.instance.saveCtrl.LoadCharacter(ModManager.GetStoryCoreFolder(storyFolder));
            GameManager.instance.saveCtrl.LoadItem(ModManager.GetStoryCoreFolder(storyFolder));

            GameManager.instance.saveCtrl.LoadEvent(ModManager.GetStoryCoreFolder(storyFolder));
            GameManager.instance.saveCtrl.LoadConfig(ModManager.GetStoryCoreFolder(storyFolder));

        }
        else //初始化
        {
            StoryForm.AddData(new StoryForm.Data(storyId, "new" + storyId, "empty", GlobalNameHelper.GetDefaultTexName()));

            SceneForm.Clear();
            var sceneData = new SceneForm.Data(1, "scene", GlobalNameHelper.GetDefaultTexName(), (0.5f, 0.5f));
            SceneForm.AddData(sceneData);

            CharacterParamForm.Clear();
            var hpParamData = new CharacterParamForm.Data(-1, "Hp", 0, 0f, 100f, 100f, 0,default);
            var speedParamData = new CharacterParamForm.Data(-1, "Speed", 0, 0f, 5f, 5f, 0, default);
            CharacterParamForm.AddData(hpParamData);
            CharacterParamForm.AddData(speedParamData);


            var animDic = new Dictionary<string, CharacterAnimForm.Data>()
            {
                {"anim",ModManager.instance.assetCtrl.CreateCharacterAnim("anim") }
            };
            animDic["anim"].animClip.Add(ModManager.instance.assetCtrl.CreateCharacterAnimClip());
            CharacterProductForm.Clear();
            CharacterProductForm.AddData(new CharacterProductForm.Data(-1, "Player", "", GlobalNameHelper.GetDefaultCharacterTexName(), new Dictionary<string, CharacterParamForm.Data>() { { "Hp", hpParamData.Copy() }, { "Speed", speedParamData.Copy() } }, true, animDic, "anim", "anim", "Speed", "Hp",new Dictionary<string,EventTriggerForm.Data>(),new Dictionary<EquipPartType, int>(), "", GlobalNameHelper.GetDefaultCharacterTexName()));

            ConfigForm.Clear();
            ConfigForm.AddData(new ConfigForm.Data(1, sceneData.uid, new Vector3(500, 1000, 500), CharacterProductForm.DataByUid[1].name, new List<int>() { 1}, new List<int>() { 1}, new List<int>(), "", "", GlobalNameHelper.GetDefaultTexName()));

            var data = new GameMapData();
            data.Init();
            GameManager.instance.saveCtrl.SaveOverview(storyId);
            
            GameManager.instance.saveCtrl.SaveSceneMap(ModManager.GetStoryCoreFolder(storyFolder) + sceneData.uid, data);
            GameManager.instance.saveCtrl.SaveScene(ModManager.GetStoryCoreFolder(storyFolder));

            GameManager.instance.saveCtrl.SaveCharacter(ModManager.GetStoryCoreFolder(storyFolder));
            GameManager.instance.saveCtrl.SaveEvent(ModManager.GetStoryCoreFolder(storyFolder));
            GameManager.instance.saveCtrl.SaveConfig(ModManager.GetStoryCoreFolder(storyFolder));
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
        ConfigForm.Clear();
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
        bool ok = await StartLoadScene(
            PlayManager.instance.boxPlay ?
                PlayManager.instance.GetStoryCoreFolder()
                : PlayManager.instance.GetStorySaveFolder(), sceneId);

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

        UiManager.instance.ShowUi<UiLoadingCtrl>();
        MapData data;

        data = await Task.Run(() =>
        {
            return GameManager.instance.saveCtrl.LoadSceneMap(storyCoreFolder + "/" + GetSceneFileNameById(id));
        });

        /* {
             //new
             data = await Task.Run(() =>
             {
                 var data = new GameMapData();
                 data.Init();
                 return data;
             });
         }*/
        data.mainData.viewSize = new Vector3Int((int)(InputManager.instance.screenWorldSize.x / 2) + 2, data.mainData.viewSize.y, (int)(InputManager.instance.screenWorldSize.y / 2) + 2);

        MapManager.instance.Begin(data);




        foreach (var maskData in MapMaskForm.DataById.Values)
        {
            var raws = new Texture2D[6];

            for (int i = 0; i < maskData.texsName.Count; i++)
            {
                raws[i] = (Texture2D)TexAssetForm.DataByName[maskData.texsName[i]].tex;
            }
            GameManager.instance.mapCtrl.CreateAlphaVariantsByBasic6(maskData.name, raws);
        }

        foreach (var character in CharacterProductForm.DataByUid.Values)
        {
            CharacterAnimForm.Data idleAnim = null;
            CharacterAnimForm.Data moveAnim = null;
            foreach (var anim in character.animDic.Values)
            {
                if (anim.name == character.idleAnimName)
                {
                    idleAnim = anim;
                }
                if (anim.name == character.moveAnimName)
                {
                    moveAnim = anim;
                }
            }
            GameManager.instance.characterCtrl.CreateAnim(character, idleAnim, moveAnim);

        }

        Z_EventHelper.Invoke(new LoadingEvent()
        {
            state = LoadingState.Done
        });
        UiManager.instance.CloseUi<UiLoadingCtrl>();
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
    #endregion
}
