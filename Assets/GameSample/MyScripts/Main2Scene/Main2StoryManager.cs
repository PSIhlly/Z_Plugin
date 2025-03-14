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
    public void StartLoadStoryUgc(string fileName)
    {
        StartLoadStory(fileName);
        ModManager.instance.BeginStory(fileName);
    }
    public void StartLoadStory(string fileName)
    {
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
        StartLoadScene(fileName);
        ModManager.instance.BeginScene(fileName);
    }

    public async void StartLoadScene(string fileName)
    {
        GameManager.instance.utilCtrl.ResetPrefabPool();
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
