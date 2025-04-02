using Form;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ui;
using Ui.Loading;
using Ui.ModSceneMain;
using UnityEngine;
using Z_DataSystem;
using Z_DataSystem.Form;
using Z_DesignStyle;
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
    public void StartLoadStoryPlay(string storyFolder,bool ignoreSave)
    {
        StartLoadStory(storyFolder);
        PlayManager.instance.BeginStory(storyFolder, ignoreSave);
    }
    public void StartLoadStory(string storyFolder)
    {
        if (SaveAndLoad.Exist(storyFolder + "/core"))
        {
            GameManager.instance.saveCtrl.LoadMaterial(storyFolder + "/core");
            GameManager.instance.saveCtrl.LoadObject(storyFolder + "/core");
            GameManager.instance.saveCtrl.LoadCharacter(storyFolder + "/core");
        }
    }
    public void UnloadStoryUgc()
    {
        ModManager.instance.EndStory();
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
        bool ok = await StartLoadScene(PlayManager.instance.GetStorySaveFolder() + "/" + fileName);
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
                var data = new MapData();
                return data;
            });
        }

        MapManager.instance.Begin(data);


        foreach (var texData in MapTextureForm.DataById.Values)
        {
            List<Texture2D> lst = new List<Texture2D>();
            for (int i = 0; i < texData.texsName.Count; i++)
            {
                lst.Add((Texture2D)TexAssetForm.DataByName[texData.texsName[i]].tex);
            }
            MapManager.instance.unitUtilCtrl.CreateTexAnimVariants(texData.name, lst.ToArray());
        }

        foreach (var maskData in MapMaskForm.DataById.Values)
        {
                var raws = new Texture2D[6];

            for(int i=0;i< maskData.texsName.Count;i++)
            {
                raws[i] = (Texture2D)TexAssetForm.DataByName[maskData.texsName[i]].tex;
            }
            MapManager.instance.unitUtilCtrl.CreateAlphaVariantsByBasic5(maskData.name, raws);
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
    public void UnloadScene()
    {

    }
    #endregion

}
