using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using Ui;
using UnityEngine;
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
    
    public void StartLoadSceneUgc(string fileName)
    {
        UiManager.instance.ShowUi<UiLoadingCtrl>();

        if(SaveAndLoad.Exist(fileName))
        {
            var data = SaveAndLoad.Load(fileName);
            ModSceneManager.instance.Begin(new MapDataController(data));
        }
        else
        {
            ModSceneManager.instance.Begin(new MapDataController());
        }

        Z_EventHelper.Invoke(new LoadingEvent()
        {
            state=  LoadingState.Done
        });
    }


}
