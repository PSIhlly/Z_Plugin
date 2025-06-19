using Form;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
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
    public void StartLoadStoryUgc(string storyFolder)
    {
        StartLoadStory(storyFolder);
        ModManager.instance.BeginStory(storyFolder);
    }
    public void StartLoadStoryPlay(string storyFolder,bool boxPlay)
    {
        StartLoadStory(storyFolder);
        PlayManager.instance.BeginStory(storyFolder, boxPlay);
    }
    public void StartLoadStory(string storyFolder)
    {

        StoryTexAssetForm.Clear();

        if (SaveAndLoad.Exist(storyFolder + "/core"))
        {
            GameManager.instance.saveCtrl.LoadOverview(storyFolder + "/core");
            GameManager.instance.saveCtrl.LoadMaterial(storyFolder + "/core");
            GameManager.instance.saveCtrl.LoadObject(storyFolder + "/core");
            GameManager.instance.saveCtrl.LoadCharacter(storyFolder + "/core");
            GameManager.instance.saveCtrl.LoadItem(storyFolder + "/core");
            
            GameManager.instance.saveCtrl.LoadEvent(storyFolder + "/core"); 
            GameManager.instance.saveCtrl.LoadConfig(storyFolder + "/core");
        }else //初始化
        {
            StoryForm.Clear();
            StoryForm.AddData(new StoryForm.Data(1, "empty", "empty", "",""));
            CharacterParamForm.Clear();
            var hpParamData = new CharacterParamForm.Data(-1, "Hp", 0, 0f, 100f, 100f, 0);
            var speedParamData = new CharacterParamForm.Data(-1, "Speed", 0, 0f, 5f, 5f, 0);
            CharacterParamForm.AddData(hpParamData);
            CharacterParamForm.AddData(speedParamData);

            CharacterProductForm.Clear();
            CharacterProductForm.AddData(new CharacterProductForm.Data(-1,"Player","","",new Dictionary<string, CharacterParamForm.Data>() { {"Hp", hpParamData.Copy() }, { "Speed", speedParamData.Copy() } },true,new Dictionary<string, CharacterAnimForm.Data>(),"","","Speed","Hp","","",""));

            ConfigForm.Clear();
            ConfigForm.AddData(new ConfigForm.Data(1,1,new Vector3(500,1000,500), "Player",new List<int>()));
            
            var data = new GameMapData();
            data.Init();
            GameManager.instance.saveCtrl.SaveScene(storyFolder + "/core/" + "scene1",data);

            GameManager.instance.saveCtrl.SaveCharacter(storyFolder + "/core");
            GameManager.instance.saveCtrl.SaveEvent(storyFolder + "/core");
            GameManager.instance.saveCtrl.SaveConfig(storyFolder + "/core");
        }
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
    }
    #endregion

    #region scene
    public async void StartLoadSceneUgc(string fileName)
    {
        bool ok=await StartLoadScene(ModManager.instance.GetStoryCoreFolder()+"/"+ fileName);
        ModManager.instance.BeginScene(fileName);
    }
    public async void StartLoadScenePlay(string fileName)
    {
        bool ok = await StartLoadScene(
            PlayManager.instance.boxPlay?
                (PlayManager.instance.GetStoryCoreFolder() + "/" + fileName)
                : (PlayManager.instance.GetStorySaveFolder() + "/" + fileName));

        PlayManager.instance.BeginScene(fileName);
    }

    public async Task<bool> StartLoadScene(string fileName)
    {
        GameManager.instance.saveCtrl.ResetPrefabPool();

        UiManager.instance.ShowUi<UiLoadingCtrl>();
        MapData data;

        if (SaveAndLoad.Exist(fileName))
        {
            data = await Task.Run(() =>
            {
                return GameManager.instance.saveCtrl.LoadScene(fileName);
            });
        }
        else
        {
            //new
            data = await Task.Run(() =>
            {
                var data = new GameMapData();
                data.Init();
                return data;
            });
        }
        data.mainData.viewSize = new Vector3Int((int)(InputManager.instance.screenWorldSize.x/2)+2, data.mainData.viewSize.y, (int)(InputManager.instance.screenWorldSize.y/2)+2);

        MapManager.instance.Begin(data);


       

        foreach (var maskData in MapMaskForm.DataById.Values)
        {
                var raws = new Texture2D[6];

            for(int i=0;i< maskData.texsName.Count;i++)
            {
                raws[i] = (Texture2D)TexAssetForm.DataByName[maskData.texsName[i]].tex;
            }
            GameManager.instance.mapCtrl.CreateAlphaVariantsByBasic5(maskData.name, raws);
        }

        foreach (var character in CharacterProductForm.DataByUid.Values)
        {
            CharacterAnimForm.Data idleAnim = null;
            CharacterAnimForm.Data moveAnim = null;
            foreach (var anim in character.animDic.Values)
            {
                if(anim.name== character.idleAnimName)
                {
                    idleAnim = anim;
                }
                else if(anim.name == character.moveAnimName)
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
    }
    #endregion

}
