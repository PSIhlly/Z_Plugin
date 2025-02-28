using Form;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ui;
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
public class Main2SceneManager : Z_MonoManager<Main2SceneManager>
{

    public async void StartLoadSceneUgc(string fileName)
    {
        UiManager.instance.ShowUi<UiLoadingCtrl>();
        MapDataController data;
        


        if (SaveAndLoad.Exist(fileName))
        {
            data = await Task.Run(() =>
                {
                    return new MapDataController(SaveAndLoad.Load(fileName));
                });
        }
        else
        {
            //new
            
            data = await Task.Run(() =>
            {
                return new MapDataController();
            });
        }

        ModSceneManager.instance.Begin(data, fileName);

        Z_EventHelper.Invoke(new LoadingEvent()
        {
            state = LoadingState.Done
        });
        UiManager.instance.CloseUi<UiLoadingCtrl>();
        UiManager.instance.ShowUi<UiModSceneMainCtrl>();
    }
    public void UnloadSceneUgc()
    {
        UnloadScene();
    }
    public void UnloadScene()
    {
        //clean auto content
        var idLst = new List<int>(MapBaseForm.DataById.Keys);
        foreach(var id in idLst)
        {
            if(id<= MapBaseForm.autoIdCnt)
            {
                MapBaseForm.RemoveData(id);
            }
        }
    }
}
